using Knight.Response.Abstractions.Http.Resolution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Knight.Response.Core;
using Knight.Response.AspNetCore.Options;
using Knight.Response.Extensions;

namespace Knight.Response.AspNetCore.Factories;

/// <summary>
/// Helpers to translate <see cref="Result"/> / <see cref="Result{T}"/> into ASP.NET Core <see cref="IResult"/>.
/// Honors <see cref="KnightResponseOptions"/> (resolved from DI via <see cref="HttpContext"/> when provided).
/// </summary>
/// <remarks>
/// Pass an <see cref="HttpContext"/> (recommended in Minimal APIs/Handlers) to resolve options and emit
/// ProblemDetails on failures when configured. If omitted, <see cref="KnightResponseOptions.Defaults"/> are used:
/// include full result payloads on success; simple JSON on errors (no ProblemDetails).
/// </remarks>
public static partial class ApiResults
{
    /// <summary>
    /// Forces an RFC7807 response from <paramref name="result"/> using configured options
    /// (ValidationProblemDetails if mappable/enabled, else ProblemDetails), ignoring
    /// the default Ok/Created/Accepted/NoContent shaping.
    /// </summary>
    /// <param name="result">The domain result.</param>
    /// <param name="http">The current <see cref="HttpContext"/> (required).</param>
    /// <param name="statusCode">
    /// Optional explicit HTTP status code. If <c>null</c>, the code is resolved via
    /// <see cref="ResultHttpResolver.ResolveHttpCode{TH,TP,TV}(Result, KnightResponseBaseOptions{TH,TP,TV})"/>.
    /// </param>
    /// <returns>An <see cref="IResult"/> ProblemDetails (or ValidationProblemDetails) response.</returns>
    public static IResult Problem(Result result, HttpContext http, int? statusCode = null)
    {
        var opts = ResolveOptions(http);
        statusCode ??= ResultHttpResolver.ResolveHttpCode(result, opts);
        return ProblemFactory.FromResult(http, opts, result, statusCode);
    }

    private static IResult Build(HttpContext? http, int statusCode, Result result, string? location = null) =>
        result.IsSuccess()
            ? BuildSuccess(http, statusCode, result, location)
            : BuildFailure(http, result, statusCode);

    private static IResult Build<T>(HttpContext? http, int statusCode, Result<T> result, string? location = null) =>
        result.IsSuccess()
            ? BuildSuccess(http, statusCode, result, location)
            : BuildFailure(http, result, statusCode);

    private static IResult BuildSuccess(HttpContext? http, int statusCode, Result result, string? location)
    {
        var opts = ResolveOptions(http);
        var resolved = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Let resolver upgrade default 200/0 (e.g., to 204 for NoContent)
        if (IsDefaultSuccessCode(statusCode))
        {
            statusCode = resolved;
        }

        if (statusCode == StatusCodes.Status204NoContent)
        {
            return Results.NoContent();
        }

        if (statusCode == StatusCodes.Status201Created)
        {
            return opts.IncludeFullResultPayload
                ? Results.Created(location, result)
                : Results.Created(location, null);
        }

        if (statusCode == StatusCodes.Status202Accepted)
        {
            return opts.IncludeFullResultPayload
                ? Results.Accepted(location, result)
                : Results.Accepted(location);
        }

        return opts.IncludeFullResultPayload
            ? Results.Json(result, statusCode: statusCode)
            : Results.StatusCode(statusCode);
    }

    private static IResult BuildSuccess<T>(HttpContext? http, int statusCode, Result<T> result, string? location)
    {
        var opts = ResolveOptions(http);
        var resolved = ResultHttpResolver.ResolveHttpCode(result, opts);

        if (IsDefaultSuccessCode(statusCode))
        {
            statusCode = resolved;
        }

        if (statusCode == StatusCodes.Status204NoContent)
        {
            return Results.NoContent();
        }

        if (statusCode == StatusCodes.Status201Created)
        {
            return opts.IncludeFullResultPayload
                ? Results.Created(location, result)
                : Results.Created(location, result.Value);
        }

        if (statusCode == StatusCodes.Status202Accepted)
        {
            return opts.IncludeFullResultPayload
                ? Results.Accepted(location, result)
                : Results.Accepted(location, result.Value);
        }

        return opts.IncludeFullResultPayload
            ? Results.Json(result, statusCode: statusCode)
            : Results.Json(result.Value, statusCode: statusCode);
    }

    private static IResult BuildFailure(HttpContext? http, Result result, int? statusCode = null)
    {
        var opts = ResolveOptions(http);
        var resolved = ResultHttpResolver.ResolveHttpCode(result, opts);

        // If caller didn’t force a 4xx/5xx, use the resolved failure code
        var final = statusCode.HasValue
            ? IsExplicitFailureCode(statusCode.Value) ? statusCode.Value : resolved
            : resolved;

        if (opts.UseProblemDetails && http is not null)
        {
            return ProblemFactory.FromResult(http, opts, result, final);
        }

        return Results.Json(opts.IncludeFullResultPayload
            ? result
            : result.Messages, statusCode: final);
    }

    private static KnightResponseOptions ResolveOptions(HttpContext? http)
    {
        if (http is null)
        {
            return new KnightResponseOptions();
        }

        var opt = http.RequestServices.GetService<IOptions<KnightResponseOptions>>()?.Value;
        return opt ?? new KnightResponseOptions();
    }

    private static bool IsExplicitFailureCode(int code) => code >= StatusCodes.Status400BadRequest;
    private static bool IsDefaultSuccessCode(int code) => code == 0 || code == StatusCodes.Status200OK;
}
