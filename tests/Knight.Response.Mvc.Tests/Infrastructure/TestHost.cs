using Knight.Response.Abstractions.Http.Mappers;
using Knight.Response.Mvc.Extensions;
using Knight.Response.Mvc.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Knight.Response.Mvc.Tests.Infrastructure;

internal static class TestHost
{
    private sealed class PassthroughTestMapper : IValidationErrorMapper
    {
        public IDictionary<string, string[]> Map(IReadOnlyList<Models.Message> messages)
            => new Dictionary<string, string[]>();
    }

    public static DefaultHttpContext CreateHttpContext(
        KnightResponseOptions? options = null,
        IValidationErrorMapper? validationMapper = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // Minimal set so ObjectResult can write via formatters (System.Text.Json)
        services.AddMvcCore()
            .AddDataAnnotations()
            .AddFormatterMappings()
            .AddJsonOptions(_ => { });

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

        services.AddScoped<IValidationErrorMapper>(_ => validationMapper ?? new PassthroughTestMapper());

        var provider = services.BuildServiceProvider();

        var http = new DefaultHttpContext
        {
            RequestServices = provider
        };
        http.Response.Body = new MemoryStream();

        return http;
    }


    public static async Task<(int status, string body, IHeaderDictionary headers)>
        ExecuteAsync(IActionResult result, HttpContext http)
    {
        // Ensure we can serialize ObjectResult via MVC formatters
        // EnsureMvcCoreServices(http);

        http.Response.Body = new MemoryStream();

        var ctx = new ActionContext(
            http,
            new RouteData(),
            new ActionDescriptor()
        );

        await result.ExecuteResultAsync(ctx);

        http.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(http.Response.Body);
        var body = await reader.ReadToEndAsync();

        return (http.Response.StatusCode, body, http.Response.Headers);
    }
}
