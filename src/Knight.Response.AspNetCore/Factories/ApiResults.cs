using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Knight.Response.Core;
using Knight.Response.AspNetCore.Options;

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
public static class ApiResults
{
    // 200 ---------------------------------------------------------------------

    /// <summary>
    /// Produces a <c>200 OK</c> response for a non-generic <see cref="Result"/>.
    /// </summary>
    /// <remarks>
    /// On success: either the full <see cref="Result"/> payload or an empty body is returned based on
    /// <see cref="KnightResponseOptions.IncludeFullResultPayload"/>.<br/>
    /// On failure: a client error is emitted (ProblemDetails if enabled).
    /// </remarks>
    /// <param name="result">The operation outcome.</param>
    /// <param name="http">
    /// Optional <see cref="HttpContext"/> to resolve <see cref="KnightResponseOptions"/> from DI.
    /// Defaults to <see cref="KnightResponseOptions.Defaults"/> when omitted.
    /// </param>
    /// <returns>An <see cref="IResult"/> representing HTTP 200 or a failure response.</returns>
    public static IResult Ok(Result result, HttpContext? http = null) =>
        Build(http, StatusCodes.Status200OK, result);

    /// <summary>
    /// Produces a <c>200 OK</c> response for a <see cref="Result{T}"/>.
    /// </summary>
    /// <remarks>
    /// On success: responds with either the full <see cref="Result{T}"/> or just <c>Value</c> depending on
    /// <see cref="KnightResponseOptions.IncludeFullResultPayload"/>.<br/>
    /// On failure: a client error is emitted (ProblemDetails if enabled).
    /// </remarks>
    /// <param name="result">The typed operation outcome.</param>
    /// <param name="http">Optional <see cref="HttpContext"/> used to resolve options.</param>
    /// <returns>An <see cref="IResult"/> representing HTTP 200 or a failure response.</returns>
    public static IResult Ok<T>(Result<T> result, HttpContext? http = null) =>
        Build(http, StatusCodes.Status200OK, result);

    // 201 ---------------------------------------------------------------------

    /// <summary>
    /// Produces a <c>201 Created</c> response for a non-generic <see cref="Result"/>.
    /// </summary>
    /// <remarks>
    /// On success: sends either the full <see cref="Result"/> payload or an empty body (controlled by
    /// <see cref="KnightResponseOptions.IncludeFullResultPayload"/>).<br/>
    /// On failure: emits a client error (ProblemDetails if enabled).
    /// </remarks>
    /// <param name="result">The operation outcome.</param>
    /// <param name="http">Optional <see cref="HttpContext"/> used to resolve options.</param>
    /// <param name="location">
    /// Optional <c>Location</c> header value (typically the newly created resource URI).
    /// </param>
    /// <returns>An <see cref="IResult"/> representing HTTP 201 or a failure response.</returns>
    public static IResult Created(Result result, HttpContext? http = null, string? location = null) =>
        Build(http, StatusCodes.Status201Created, result, location);

    /// <summary>
    /// Produces a <c>201 Created</c> response for a <see cref="Result{T}"/>.
    /// </summary>
    /// <remarks>
    /// On success: sends either the full <see cref="Result{T}"/> or just <c>Value</c>
    /// (controlled by <see cref="KnightResponseOptions.IncludeFullResultPayload"/>).<br/>
    /// On failure: emits a client error (ProblemDetails if enabled).
    /// </remarks>
    /// <param name="result">The typed operation outcome.</param>
    /// <param name="http">Optional <see cref="HttpContext"/> used to resolve options.</param>
    /// <param name="location">Optional <c>Location</c> header value.</param>
    /// <returns>An <see cref="IResult"/> representing HTTP 201 or a failure response.</returns>
    public static IResult Created<T>(Result<T> result, HttpContext? http = null, string? location = null) =>
        Build(http, StatusCodes.Status201Created, result, location);

    // 204 ---------------------------------------------------------------------

    /// <summary>
    /// Produces a <c>204 No Content</c> when <paramref name="result"/> is successful; otherwise a client error.
    /// </summary>
    /// <param name="result">The operation outcome.</param>
    /// <param name="http">Optional <see cref="HttpContext"/> used to resolve options.</param>
    /// <returns><see cref="Results.NoContent"/> or a failure response.</returns>
    public static IResult NoContent(Result result, HttpContext? http = null) =>
        // result.IsSuccess ? Results.NoContent() : BuildFailure(http, result);
        Build(http, StatusCodes.Status204NoContent, result);

    // 202 / Accepted -----------------------------------------------------------

    /// <summary>
    /// Produces a <c>202 Accepted</c> response for a non-generic <see cref="Result"/>.
    /// </summary>
    /// <remarks>
    /// On success: either the full <see cref="Result"/> payload or an empty body is returned based on
    /// <see cref="KnightResponseOptions.IncludeFullResultPayload"/>.<br/>
    /// On failure: emits a client error (ProblemDetails if enabled).
    /// </remarks>
    /// <param name="result">The operation outcome.</param>
    /// <param name="http">Optional <see cref="HttpContext"/> used to resolve options.</param>
    /// <param name="location">Optional <c>Location</c> header value (e.g., status resource URI).</param>
    /// <returns>An <see cref="IResult"/> representing HTTP 202 or a failure response.</returns>
    public static IResult Accepted(Result result, HttpContext? http = null, string? location = null) =>
        Build(http, StatusCodes.Status202Accepted, result, location);

    /// <summary>
    /// Produces a <c>202 Accepted</c> response for a <see cref="Result{T}"/>.
    /// </summary>
    /// <remarks>
    /// On success: either the full <see cref="Result{T}"/> or just <c>Value</c> is returned based on
    /// <see cref="KnightResponseOptions.IncludeFullResultPayload"/>.<br/>
    /// On failure: emits a client error (ProblemDetails if enabled).
    /// </remarks>
    /// <param name="result">The typed operation outcome.</param>
    /// <param name="http">Optional <see cref="HttpContext"/> used to resolve options.</param>
    /// <param name="location">Optional <c>Location</c> header value (e.g., status resource URI).</param>
    /// <returns>An <see cref="IResult"/> representing HTTP 202 or a failure response.</returns>
    public static IResult Accepted<T>(Result<T> result, HttpContext? http = null, string? location = null) =>
        Build(http, StatusCodes.Status202Accepted, result, location);

    // Direct error helpers -----------------------------------------------------

    /// <summary>
    /// Produces a <c>400 Bad Request</c> response populated from <paramref name="result"/>.
    /// Uses ProblemDetails if <see cref="KnightResponseOptions.UseProblemDetails"/> is enabled.
    /// </summary>
    public static IResult BadRequest(Result result, HttpContext? http = null) =>
        BuildFailure(http, result, StatusCodes.Status400BadRequest);

    /// <summary>
    /// Produces a <c>404 Not Found</c> response populated from <paramref name="result"/>.
    /// Uses ProblemDetails if <see cref="KnightResponseOptions.UseProblemDetails"/> is enabled.
    /// </summary>
    public static IResult NotFound(Result result, HttpContext? http = null) =>
        BuildFailure(http, result, StatusCodes.Status404NotFound);

    /// <summary>
    /// Produces a <c>409 Conflict</c> response populated from <paramref name="result"/>.
    /// Uses ProblemDetails if <see cref="KnightResponseOptions.UseProblemDetails"/> is enabled.
    /// </summary>
    public static IResult Conflict(Result result, HttpContext? http = null) =>
        BuildFailure(http, result, StatusCodes.Status409Conflict);

    /// <summary>Produces a bare <c>401 Unauthorized</c> response.</summary>
    public static IResult Unauthorized() => Results.Unauthorized();

    /// <summary>Produces a bare <c>403 Forbidden</c> response.</summary>
    public static IResult Forbidden() => Results.StatusCode(StatusCodes.Status403Forbidden);

    // Internals ================================================================

    private static IResult Build(HttpContext? http, int successStatus, Result result, string? location = null) =>
        result.IsSuccess
            ? BuildSuccess(http, successStatus, result, location)
            : BuildFailure(http, result, successStatus);

    private static IResult Build<T>(HttpContext? http, int successStatus, Result<T> result, string? location = null) =>
        result.IsSuccess
            ? BuildSuccess(http, successStatus, result, location)
            : BuildFailure(http, result, successStatus);

    private static IResult BuildSuccess(HttpContext? http, int status, Result result, string? location)
    {
        var opts = ResolveOptions(http);

        if (status == StatusCodes.Status204NoContent)
        {
            return Results.NoContent();
        }

        if (status == StatusCodes.Status201Created)
        {
            return opts.IncludeFullResultPayload
                ? Results.Created(location, result)
                : Results.Created(location, null);
        }

        if (status == StatusCodes.Status202Accepted)
        {
            return opts.IncludeFullResultPayload
                ? Results.Accepted(location, result)
                : Results.Accepted(location);
        }

        return opts.IncludeFullResultPayload
            ? Results.Json(result, statusCode: status)
            : Results.StatusCode(status);
    }

    private static IResult BuildSuccess<T>(HttpContext? http, int status, Result<T> result, string? location)
    {
        var opts = ResolveOptions(http);

        if (status == StatusCodes.Status204NoContent)
        {
            return Results.NoContent();
        }

        if (status == StatusCodes.Status201Created)
        {
            return opts.IncludeFullResultPayload
                ? Results.Created(location, result)
                : Results.Created(location, result.Value);
        }

        if (status == StatusCodes.Status202Accepted)
        {
            return opts.IncludeFullResultPayload
                ? Results.Accepted(location, result)
                : Results.Accepted(location, result.Value);
        }

        return opts.IncludeFullResultPayload
            ? Results.Json(result, statusCode: status)
            : Results.Json(result.Value, statusCode: status);
    }

    private static IResult BuildFailure(HttpContext? http, Result result, int? statusCode = null)
    {
        var opts = ResolveOptions(http);
        if (statusCode.HasValue && statusCode.Value < StatusCodes.Status400BadRequest)
        {
            statusCode = opts.StatusCodeResolver(result.Status);
        }

        statusCode ??= opts.StatusCodeResolver(result.Status);

        if (opts.UseProblemDetails && http is not null)
        {
            // Centralised ProblemDetails formatting
            return ProblemFactory.FromResult(http, opts, result, statusCode);
        }

        // Simple fallback: return messages with the requested status.
        return Results.Json(opts.IncludeFullResultPayload ? result : result.Messages, statusCode: statusCode);
    }

    private static KnightResponseOptions ResolveOptions(HttpContext? http)
    {
        if (http is null)
        {
            return KnightResponseOptions.Defaults;
        }

        var opt = http.RequestServices.GetService<IOptions<KnightResponseOptions>>()?.Value;
        return opt ?? KnightResponseOptions.Defaults;
    }
}
