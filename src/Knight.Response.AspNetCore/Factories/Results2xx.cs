using Knight.Response.AspNetCore.Options;
using Knight.Response.Core;
using Microsoft.AspNetCore.Http;

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

    /// <summary>
    /// Produces a <c>201 Created</c> response with a <c>Location</c> header for a non-generic <see cref="Result"/>.
    /// </summary>
    /// <remarks>
    /// On success: sends either the full <see cref="Result"/> payload or an empty body (controlled by
    /// <see cref="KnightResponseOptions.IncludeFullResultPayload"/>).<br/>
    /// On failure: emits a client error (ProblemDetails if enabled).
    /// </remarks>
    /// <param name="result">The operation outcome.</param>
    /// <param name="location">The value for the <c>Location</c> header (typically the new resource URI).</param>
    /// <param name="http">Optional <see cref="HttpContext"/> used to resolve options.</param>
    /// <returns>An <see cref="IResult"/> representing HTTP 201 or a failure response.</returns>
    public static IResult CreatedAt(Result result, string location, HttpContext? http = null) =>
        Build(http, StatusCodes.Status201Created, result, location);

    /// <summary>
    /// Produces a <c>201 Created</c> response with a <c>Location</c> header for a <see cref="Result{T}"/>.
    /// </summary>
    /// <remarks>
    /// On success: sends either the full <see cref="Result{T}"/> or just <c>Value</c>
    /// (controlled by <see cref="KnightResponseOptions.IncludeFullResultPayload"/>).<br/>
    /// On failure: emits a client error (ProblemDetails if enabled).
    /// </remarks>
    /// <typeparam name="T">The value type carried by the result.</typeparam>
    /// <param name="result">The typed operation outcome.</param>
    /// <param name="location">The value for the <c>Location</c> header (typically the new resource URI).</param>
    /// <param name="http">Optional <see cref="HttpContext"/> used to resolve options.</param>
    /// <returns>An <see cref="IResult"/> representing HTTP 201 or a failure response.</returns>
    public static IResult CreatedAt<T>(Result<T> result, string location, HttpContext? http = null) =>
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

    /// <summary>
    /// Produces a <c>202 Accepted</c> response with a <c>Location</c> header for a non-generic <see cref="Result"/>.
    /// </summary>
    /// <remarks>
    /// On success: either the full <see cref="Result"/> payload or an empty body is returned based on
    /// <see cref="KnightResponseOptions.IncludeFullResultPayload"/>.<br/>
    /// On failure: emits a client error (ProblemDetails if enabled).
    /// </remarks>
    /// <param name="result">The operation outcome.</param>
    /// <param name="location">The value for the <c>Location</c> header (e.g., a status resource URI).</param>
    /// <param name="http">Optional <see cref="HttpContext"/> used to resolve options.</param>
    /// <returns>An <see cref="IResult"/> representing HTTP 202 or a failure response.</returns>
    public static IResult AcceptedAt(Result result, string location, HttpContext? http = null) =>
        Build(http, StatusCodes.Status202Accepted, result, location);

    /// <summary>
    /// Produces a <c>202 Accepted</c> response with a <c>Location</c> header for a <see cref="Result{T}"/>.
    /// </summary>
    /// <remarks>
    /// On success: either the full <see cref="Result{T}"/> or just <c>Value</c> is returned based on
    /// <see cref="KnightResponseOptions.IncludeFullResultPayload"/>.<br/>
    /// On failure: emits a client error (ProblemDetails if enabled).
    /// </remarks>
    /// <typeparam name="T">The value type carried by the result.</typeparam>
    /// <param name="result">The typed operation outcome.</param>
    /// <param name="location">The value for the <c>Location</c> header (e.g., a status resource URI).</param>
    /// <param name="http">Optional <see cref="HttpContext"/> used to resolve options.</param>
    /// <returns>An <see cref="IResult"/> representing HTTP 202 or a failure response.</returns>
    public static IResult AcceptedAt<T>(Result<T> result, string location, HttpContext? http = null) =>
        Build(http, StatusCodes.Status202Accepted, result, location);
}
