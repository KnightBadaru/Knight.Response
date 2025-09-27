using Knight.Response.Abstractions.Http.Mappers;
using Knight.Response.Abstractions.Http.Resolution;
using Knight.Response.Core;
using Knight.Response.Models;
using Knight.Response.Mvc.Infrastructure;
using Knight.Response.Mvc.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Knight.Response.Mvc.Factories;

/// <summary>
/// Central place that turns a <see cref="Result"/> into an <see cref="IActionResult"/>
/// using RFC7807 payloads where possible (ProblemDetails / ValidationProblemDetails).
/// Compatible with ASP.NET Core MVC 2.x (no native Extensions bag).
/// </summary>
internal static class ProblemFactory
{
    private static object ToShallow(Message m) =>
        new { type = m.Type.ToString(), content = m.Content, metadata = m.Metadata };

    // Resolution order (supports Scoped mappers safely):
    // 1) per-request DI (preferred)
    // 2) options override (if a user explicitly set one)
    // 3) fresh default instance as a final fallback
    private static IValidationErrorMapper ResolveMapper(HttpContext? http, KnightResponseOptions opts)
    {
        var mapperFromRequest = http?.RequestServices?.GetService<IValidationErrorMapper>();
        return mapperFromRequest ?? (opts.ValidationMapper ?? new DefaultValidationErrorMapper());
    }

    /// <summary>
    /// Build an <see cref="IActionResult"/> from a <see cref="Result"/> using the supplied options.
    /// </summary>
    /// <param name="http">Current HTTP context.</param>
    /// <param name="opts">Resolved <see cref="KnightResponseOptions"/>.</param>
    /// <param name="result">The domain result.</param>
    /// <param name="statusCode">
    /// Optional explicit HTTP status code; if null, <see cref="KnightResponseOptions.StatusCodeResolver"/> is used.
    /// </param>
    public static IActionResult FromResult(HttpContext http, KnightResponseOptions opts, Result result, int? statusCode = null)
    {
        var resolved = statusCode ?? ResultHttpResolver.ResolveHttpCode(result, opts);
        var code     = resolved;

        // Try ValidationProblemDetails if enabled and we have mappable errors
        if (opts.UseValidationProblemDetails)
        {
            var mapper = ResolveMapper(http, opts);
            var errors = mapper.Map(result.Messages);

            if (errors.Count > 0)
            {
                var vpd = new CompatValidationProblemDetails(errors)
                {
                    Status   = code,
                    Title    = "One or more validation errors occurred.",
                    Type     = $"https://httpstatuses.io/{code}",
                    Instance = http.Request?.Path
                };

                // include result metadata
                vpd.Extensions["svcStatus"] = result.Status.ToString();
                vpd.Extensions["messages"]  = result.Messages.Select(ToShallow).ToArray();

                if (result.Code is not null)
                {
                    vpd.Extensions["svcCode"] = result.Code.Value;
                }

                // last-mile customization
                opts.ValidationBuilder?.Invoke(http, result, vpd);

                return new ObjectResult(vpd) { StatusCode = vpd.Status };
            }
        }

        // Fallback to standard ProblemDetails
        var title  = result.Messages.FirstOrDefault()?.Content ?? result.Status.ToString();
        var detail = result.Messages.Count > 1
            ? string.Join("; ", result.Messages.Select(m => m.Content))
            : null;

        var pd = new CompatProblemDetails
        {
            Status   = code,
            Title    = title,
            Detail   = detail,
            Type     = $"https://httpstatuses.io/{code}",
            Instance = http.Request?.Path.Value
        };

        pd.Extensions["svcStatus"] = result.Status.ToString();
        pd.Extensions["messages"]  = result.Messages.Select(ToShallow).ToArray();

        if (result.Code is not null)
        {
            pd.Extensions["svcCode"] = result.Code.Value;
        }

        opts.ProblemDetailsBuilder?.Invoke(http, result, pd);

        return new ObjectResult(pd) { StatusCode = pd.Status };
    }
}
