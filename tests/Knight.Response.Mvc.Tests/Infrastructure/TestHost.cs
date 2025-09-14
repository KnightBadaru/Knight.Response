using System.Buffers;
using Knight.Response.Abstractions.Http.Mappers;
using Knight.Response.Mvc.Extensions;
using Knight.Response.Mvc.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;

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
        services.AddMvcCore();
        services.Configure<MvcOptions>(o =>
        {
            o.OutputFormatters.Add(new JsonOutputFormatter(new JsonSerializerSettings(), ArrayPool<char>.Shared));
        });
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IActionResultExecutor<ObjectResult>, ObjectResultExecutor>());

        services.AddScoped<IValidationErrorMapper>(_ => validationMapper ?? new PassthroughTestMapper());

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

        var provider = services.BuildServiceProvider();

        var http = new DefaultHttpContext
        {
            RequestServices = provider
        };
        http.Response.Body = new MemoryStream();

        return http;
    }
    public static DefaultHttpContext CreateHttpContext<TMapper>(
        KnightResponseOptions? options = null) where TMapper : class, IValidationErrorMapper
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore();
        services.Configure<MvcOptions>(o =>
        {
            o.OutputFormatters.Add(new JsonOutputFormatter(new JsonSerializerSettings(), ArrayPool<char>.Shared));
        });
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IActionResultExecutor<ObjectResult>, ObjectResultExecutor>());

        // Register using the real extension so DI matches production
        services.AddKnightResponse<TMapper>(o =>
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

        var provider = services.BuildServiceProvider();

        var http = new DefaultHttpContext
        {
            RequestServices = provider
        };
        http.Response.Body = new MemoryStream();

        return http;
    }


    public static async Task<TestResult> ExecuteAsync(IActionResult result, HttpContext http)
    {
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

        return new TestResult(http.Response.StatusCode, body, http.Response.Headers);
    }
}
