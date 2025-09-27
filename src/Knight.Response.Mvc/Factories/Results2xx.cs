using Knight.Response.Core;
using Knight.Response.Mvc.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    // ----------------------- 200 / OK -----------------------

    /// <summary>
    /// Returns 200 OK. On success, returns either the full <see cref="Result"/> payload
    /// or an empty 200 depending on <see cref="KnightResponseOptions.IncludeFullResultPayload"/>.
    /// On failure, returns a client error (<see cref="ProblemDetails"/> or messages).
    /// </summary>
    public static IActionResult Ok(Result result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status200OK, result);

    /// <summary>
    /// Returns 200 OK. On success, returns either the full <see cref="Result{T}"/> payload
    /// or just the <c>Value</c>, depending on <see cref="KnightResponseOptions.IncludeFullResultPayload"/>.
    /// On failure, returns a client error (<see cref="ProblemDetails"/> or messages).
    /// </summary>
    public static IActionResult Ok<T>(Result<T> result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status200OK, result);

    // ----------------------- 201 / Created -----------------------

    /// <summary>
    /// Returns 201 Created. On success, returns the full <see cref="Result"/> payload
    /// or an empty 201 depending on <see cref="KnightResponseOptions.IncludeFullResultPayload"/>.
    /// On failure, emits a client error (ProblemDetails / ValidationProblemDetails when enabled).
    /// </summary>
    /// <param name="result">The domain <see cref="Result"/> to convert.</param>
    /// <param name="http">
    /// Optional <see cref="HttpContext"/> used to resolve <see cref="KnightResponseOptions"/> from DI.
    /// When <c>null</c>, defaults from <see cref="KnightResponseOptions.Defaults"/> are used.
    /// </param>
    /// <param name="location">Optional <c>Location</c> header to include in the response.</param>
    public static IActionResult Created(Result result, HttpContext? http = null, string? location = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status201Created, result, location);

    /// <summary>
    /// Returns 201 Created. On success, returns either the full <see cref="Result{T}"/> payload
    /// or just the <c>Value</c>, depending on <see cref="KnightResponseOptions.IncludeFullResultPayload"/>.
    /// On failure, emits a client error (ProblemDetails / ValidationProblemDetails when enabled).
    /// </summary>
    /// <typeparam name="T">The value type carried by the result.</typeparam>
    /// <param name="result">The domain <see cref="Result{T}"/> to convert.</param>
    /// <param name="http">
    /// Optional <see cref="HttpContext"/> used to resolve <see cref="KnightResponseOptions"/> from DI.
    /// When <c>null</c>, defaults from <see cref="KnightResponseOptions.Defaults"/> are used.
    /// </param>
    /// <param name="location">Optional <c>Location</c> header to include in the response.</param>
    public static IActionResult Created<T>(Result<T> result, HttpContext? http = null, string? location = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status201Created, result, location);

    /// <summary>
    /// Returns 201 Created with a <c>Location</c> header. On success, returns full Result or empty body
    /// based on options; on failure emits a client error (ProblemDetails/messages).
    /// </summary>
    public static IActionResult CreatedAt(Result result, string location, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status201Created, result, location);

    /// <summary>
    /// Returns 201 Created with a <c>Location</c> header. On success, returns Value (or full Result if configured);
    /// on failure emits a client error (ProblemDetails/messages).
    /// </summary>
    public static IActionResult CreatedAt<T>(Result<T> result, string location, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status201Created, result, location);

    // ----------------------- 202 / Accepted -----------------------

    /// <summary>
    /// Returns 202 Accepted. On success, returns the full <see cref="Result"/> payload
    /// or an empty 202 depending on <see cref="KnightResponseOptions.IncludeFullResultPayload"/>.
    /// On failure, emits a client error (ProblemDetails / ValidationProblemDetails when enabled).
    /// </summary>
    /// <param name="result">The domain <see cref="Result"/> to convert.</param>
    /// <param name="http">
    /// Optional <see cref="HttpContext"/> used to resolve <see cref="KnightResponseOptions"/> from DI.
    /// When <c>null</c>, defaults from <see cref="KnightResponseOptions.Defaults"/> are used.
    /// </param>
    /// <param name="location">Optional <c>Location</c> header (e.g., status resource URL).</param>
    public static IActionResult Accepted(Result result, HttpContext? http = null, string? location = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status202Accepted, result, location);

    /// <summary>
    /// Returns 202 Accepted. On success, returns either the full <see cref="Result{T}"/> payload
    /// or just the <c>Value</c>, depending on <see cref="KnightResponseOptions.IncludeFullResultPayload"/>.
    /// On failure, emits a client error (ProblemDetails / ValidationProblemDetails when enabled).
    /// </summary>
    /// <typeparam name="T">The value type carried by the result.</typeparam>
    /// <param name="result">The domain <see cref="Result{T}"/> to convert.</param>
    /// <param name="http">
    /// Optional <see cref="HttpContext"/> used to resolve <see cref="KnightResponseOptions"/> from DI.
    /// When <c>null</c>, defaults from <see cref="KnightResponseOptions.Defaults"/> are used.
    /// </param>
    /// <param name="location">Optional <c>Location</c> header (e.g., status resource URL).</param>
    public static IActionResult Accepted<T>(Result<T> result, HttpContext? http = null, string? location = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status202Accepted, result, location);

    /// <summary>
    /// Returns 202 Accepted with a <c>Location</c> header. On success, returns full Result or empty body
    /// based on options; on failure emits a client error (ProblemDetails/messages).
    /// </summary>
    public static IActionResult AcceptedAt(Result result, string location, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status202Accepted, result, location);

    /// <summary>
    /// Returns 202 Accepted with a <c>Location</c> header. On success, returns Value (or full Result if configured);
    /// on failure emits a client error (ProblemDetails/messages).
    /// </summary>
    public static IActionResult AcceptedAt<T>(Result<T> result, string location, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status202Accepted, result, location);

    // ----------------------- 204 / No Content ----------------

    /// <summary>
    /// Returns 204 No Content when the <paramref name="result"/> is successful.
    /// On failure, emits a client error (<see cref="ProblemDetails"/> or messages).
    /// </summary>
    public static IActionResult NoContent(Result result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status204NoContent, result);

    /// <summary>
    /// Returns 204 No Content when the <paramref name="result"/> is successful.
    /// On failure, emits a client error (<see cref="ProblemDetails"/> or messages).
    /// </summary>
    public static IActionResult NoContent<T>(Result<T> result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status204NoContent, result);
}
