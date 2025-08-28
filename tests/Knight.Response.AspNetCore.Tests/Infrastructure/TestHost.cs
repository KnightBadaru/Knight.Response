using System.Text.Json;
using Knight.Response.Abstractions.Http.Mappers;
using Knight.Response.AspNetCore.Extensions;
using Knight.Response.AspNetCore.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
// Add this to use AddKnightResponse

namespace Knight.Response.AspNetCore.Tests.Infrastructure;

internal static class TestHost
{
    // Minimal passthrough mapper for tests when none is provided.
    private sealed class PassthroughTestMapper : IValidationErrorMapper
    {
        public IDictionary<string, string[]> Map(IReadOnlyList<Models.Message> messages)
            => new Dictionary<string, string[]>();
    }

    // --- Overload 1: preferred; concrete options ---
    public static (DefaultHttpContext http, IServiceProvider services) CreateHttpContext(
        KnightResponseOptions? options = null,
        IValidationErrorMapper? validationMapper = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // Register using the real extension so DI matches production
        services.AddKnightResponse(o =>
        {
            if (options != null)
            {
                o.UseProblemDetails             = options.UseProblemDetails;
                o.UseValidationProblemDetails   = options.UseValidationProblemDetails;
                o.IncludeFullResultPayload      = options.IncludeFullResultPayload;
                o.IncludeExceptionDetails       = options.IncludeExceptionDetails;
                o.StatusCodeResolver            = options.StatusCodeResolver;
                o.ProblemDetailsBuilder         = options.ProblemDetailsBuilder;
                o.ValidationBuilder             = options.ValidationBuilder;
            }
        });

        // If tests want a specific mapper, override the one the extension would pick up
        services.AddScoped<IValidationErrorMapper>(_ => validationMapper ?? new PassthroughTestMapper());

        var provider = services.BuildServiceProvider();

        var http = new DefaultHttpContext
        {
            RequestServices = provider
        };
        http.Response.Body = new MemoryStream();

        return (http, provider);
    }

    // --- Overload 2: accept the BASE options and adapt to concrete (optional)
    public static (DefaultHttpContext http, IServiceProvider services) CreateHttpContext(
        Knight.Response.Abstractions.Http.Options.KnightResponseBaseOptions<HttpContext, ProblemDetails, ValidationProblemDetails> baseOptions,
        IValidationErrorMapper? validationMapper = null)
    {
        // Map base -> concrete (only what you need for tests)
        var concrete = new KnightResponseOptions
        {
            UseProblemDetails           = baseOptions.UseProblemDetails,
            UseValidationProblemDetails = baseOptions.UseValidationProblemDetails,
            IncludeFullResultPayload    = baseOptions.IncludeFullResultPayload,
            IncludeExceptionDetails     = baseOptions.IncludeExceptionDetails,
            StatusCodeResolver          = baseOptions.StatusCodeResolver,
            ProblemDetailsBuilder       = baseOptions.ProblemDetailsBuilder,
            ValidationBuilder           = baseOptions.ValidationBuilder
        };

        return CreateHttpContext(concrete, validationMapper);
    }

    // unchanged
    public static async Task<(int status, string body, IHeaderDictionary headers)> ExecuteAsync(IResult result, HttpContext http)
    {
        http.Response.Body = new MemoryStream();
        await result.ExecuteAsync(http);
        http.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(http.Response.Body);
        var body = await reader.ReadToEndAsync();
        return (http.Response.StatusCode, body, http.Response.Headers);
    }

    public static T? Deserialize<T>(string json) =>
        string.IsNullOrWhiteSpace(json)
            ? default
            : JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
}
