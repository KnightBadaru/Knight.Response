using Knight.Response.Abstractions.Http.Resolution;
using Knight.Response.Core;
using Knight.Response.Extensions;
using Knight.Response.Mvc.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Knight.Response.Mvc.Factories;

/// <summary>
/// Provides static helpers to convert <see cref="Result"/> and <see cref="Result{T}"/>
/// into <see cref="IActionResult"/> for ASP.NET Core MVC applications.
/// </summary>
/// <remarks>
/// These helpers honor <see cref="KnightResponseOptions"/> when a <see cref="HttpContext"/> is available
/// (payload shape, ProblemDetails behavior, etc.). Defaults are used when no context is provided.
/// </remarks>
public static partial class ApiResults
{
    // ----------------------- ProblemDetails -----------------------------

    /// <summary>
    /// Forces an RFC7807 response from <paramref name="result"/> using the configured options
    /// (ValidationProblemDetails if mappable and enabled, otherwise ProblemDetails), ignoring
    /// the default Ok/Created/Accepted/NoContent shaping.
    /// </summary>
    public static IActionResult Problem(Result result, HttpContext http, int? statusCode = null)
    {
        var opts = Resolve(http);
        // Let the resolver pick a code (CodeToHttp -> Status fallback) when statusCode is not supplied
        statusCode ??= ResultHttpResolver.ResolveHttpCode(result, opts);
        return ProblemFactory.FromResult(http, opts, result, statusCode);
    }

    // ======================= Core builders ====================

    /// <summary>
    /// Internal builder for non-generic <see cref="Result"/>.
    /// Chooses the appropriate <see cref="IActionResult"/> based on success/failure and configured options.
    /// </summary>
    private static IActionResult BuildSuccessOrFailure(HttpContext? http, int statusCode, Result result, string? location = null)
    {
        var opts = Resolve(http);
        var resolved = ResultHttpResolver.ResolveHttpCode(result, opts);

        if (result.IsUnsuccessful())
        {
            if (!IsExplicitFailureCode(statusCode))
            {
                statusCode = resolved; // ensure proper 4xx/5xx
            }

            if (opts.UseProblemDetails && http is not null)
            {
                return ProblemFactory.FromResult(http, opts, result, statusCode);
            }

            return new ObjectResult(opts.IncludeFullResultPayload ? result : result.Messages)
            {
                StatusCode = statusCode
            };
        }

        // success path
        if (IsDefaultSuccessCode(statusCode))
        {
            statusCode = resolved; // e.g., 204 when Code == NoContent, else 200
        }

        if (statusCode == StatusCodes.Status204NoContent)
        {
            return new ObjectResult(null) { StatusCode = StatusCodes.Status204NoContent };
        }

        if (http is not null && !string.IsNullOrEmpty(location))
        {
            http.Response.Headers["Location"] = location;
        }

        return new ObjectResult(opts.IncludeFullResultPayload ? result : null)
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Internal builder for generic <see cref="Result{T}"/>.
    /// Chooses the appropriate <see cref="IActionResult"/> based on success/failure and configured options.
    /// </summary>
    private static IActionResult BuildSuccessOrFailure<T>(HttpContext? http, int statusCode, Result<T> result, string? location = null)
    {
        var opts = Resolve(http);
        var resolved = ResultHttpResolver.ResolveHttpCode(result, opts);

        if (result.IsUnsuccessful())
        {
            if (!IsExplicitFailureCode(statusCode))
            {
                statusCode = resolved; // ensure proper 4xx/5xx
            }

            if (opts.UseProblemDetails && http is not null)
            {
                return ProblemFactory.FromResult(http, opts, result, statusCode);
            }

            return new ObjectResult(opts.IncludeFullResultPayload ? result : result.Messages)
            {
                StatusCode = statusCode
            };
        }

        // success path
        if (IsDefaultSuccessCode(statusCode))
        {
            statusCode = resolved; // e.g., 204 when Code == NoContent, else 200
        }

        if (statusCode == StatusCodes.Status204NoContent)
        {
            return new ObjectResult(null) { StatusCode = StatusCodes.Status204NoContent };
        }

        if (http is not null && !string.IsNullOrEmpty(location))
        {
            http.Response.Headers["Location"] = location;
        }

        return new ObjectResult(opts.IncludeFullResultPayload ? result : result.Value)
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Resolves the configured <see cref="KnightResponseOptions"/> from DI,
    /// falling back to <see cref="KnightResponseOptions.Defaults"/>.
    /// </summary>
    private static KnightResponseOptions Resolve(HttpContext? http)
    {
        if (http is null)
        {
            return new KnightResponseOptions();
        }

        var opt = http.RequestServices.GetService<IOptions<KnightResponseOptions>>()?.Value;
        return opt ?? new KnightResponseOptions();
    }

    private static bool IsExplicitFailureCode(int statusCode)
    {
        // Any explicit 4xx or 5xx code counts as “explicit failure”
        return statusCode >= StatusCodes.Status400BadRequest;
    }

    private static bool IsDefaultSuccessCode(int statusCode)
    {
        // Treat 0 (unset) and 200 OK as “default success”
        return statusCode == 0 || statusCode == StatusCodes.Status200OK;
    }
}
