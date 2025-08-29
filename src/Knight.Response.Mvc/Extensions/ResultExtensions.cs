using Knight.Response.Core;
using Knight.Response.Mvc.Factories;
using Knight.Response.Mvc.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Knight.Response.Mvc.Extensions;

/// <summary>
/// Controller-friendly extension methods that convert <see cref="Result"/> / <see cref="Result{T}"/>
/// to ASP.NET Core <see cref="IActionResult"/>.
/// <para>
/// These honor <see cref="KnightResponseOptions"/> when a <see cref="HttpContext"/> is supplied
/// (e.g., <see cref="KnightResponseOptions.IncludeFullResultPayload"/>).
/// If <see cref="HttpContext"/> is <c>null</c>, defaults from <see cref="KnightResponseOptions.Defaults"/> are used.
/// </para>
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a non-generic <see cref="Result"/> to a 200 OK <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="result">The domain result to convert.</param>
    /// <param name="http">
    /// Optional <see cref="HttpContext"/> to resolve <see cref="KnightResponseOptions"/> from DI.
    /// </param>
    /// <returns>An OK <see cref="IActionResult"/> (or a client error if <paramref name="result"/> failed).</returns>
    public static IActionResult ToOkActionResult(this Result result, HttpContext? http = null)
        => ApiResults.Ok(result, http);

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to a 200 OK <see cref="IActionResult"/>.
    /// </summary>
    /// <typeparam name="T">The value type carried by the result.</typeparam>
    /// <param name="result">The domain result to convert.</param>
    /// <param name="http">
    /// Optional <see cref="HttpContext"/> to resolve <see cref="KnightResponseOptions"/> from DI.
    /// </param>
    /// <returns>An OK <see cref="IActionResult"/> (or a client error if <paramref name="result"/> failed).</returns>
    public static IActionResult ToOkActionResult<T>(this Result<T> result, HttpContext? http = null)
        => ApiResults.Ok(result, http);

    /// <summary>
    /// Converts a non-generic <see cref="Result"/> to a 201 Created <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="result">The domain result to convert.</param>
    /// <param name="http">
    /// Optional <see cref="HttpContext"/> to resolve <see cref="KnightResponseOptions"/> from DI.
    /// </param>
    /// <param name="location">Optional <c>Location</c> header to include in the response.</param>
    /// <returns>A Created result (or a client error if <paramref name="result"/> failed).</returns>
    public static IActionResult ToCreatedActionResult(this Result result, HttpContext? http = null, string? location = null)
        => ApiResults.Created(result, http, location);

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to a 201 Created <see cref="IActionResult"/>.
    /// </summary>
    /// <typeparam name="T">The value type carried by the result.</typeparam>
    /// <param name="result">The domain result to convert.</param>
    /// <param name="http">
    /// Optional <see cref="HttpContext"/> to resolve <see cref="KnightResponseOptions"/> from DI.
    /// </param>
    /// <param name="location">Optional <c>Location</c> header to include in the response.</param>
    /// <returns>A Created result (or a client error if <paramref name="result"/> failed).</returns>
    public static IActionResult ToCreatedActionResult<T>(this Result<T> result, HttpContext? http = null, string? location = null)
        => ApiResults.Created(result, http, location);

    /// <summary>
    /// Converts a non-generic <see cref="Result"/> to a 202 Accepted <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="result">The domain result to convert.</param>
    /// <param name="http">
    /// Optional <see cref="HttpContext"/> to resolve <see cref="KnightResponseOptions"/> from DI.
    /// </param>
    /// <param name="location">Optional <c>Location</c> header (e.g., a status resource URL).</param>
    /// <returns>An Accepted result (or a client error if <paramref name="result"/> failed).</returns>
    public static IActionResult ToAcceptedActionResult(this Result result, HttpContext? http = null, string? location = null)
        => ApiResults.Accepted(result, http, location);

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to a 202 Accepted <see cref="IActionResult"/>.
    /// </summary>
    /// <typeparam name="T">The value type carried by the result.</typeparam>
    /// <param name="result">The domain result to convert.</param>
    /// <param name="http">
    /// Optional <see cref="HttpContext"/> to resolve <see cref="KnightResponseOptions"/> from DI.
    /// </param>
    /// <param name="location">Optional <c>Location</c> header (e.g., a status resource URL).</param>
    /// <returns>An Accepted result (or a client error if <paramref name="result"/> failed).</returns>
    public static IActionResult ToAcceptedActionResult<T>(this Result<T> result, HttpContext? http = null, string? location = null)
        => ApiResults.Accepted(result, http, location);

    /// <summary>
    /// Converts a non-generic <see cref="Result"/> to a 204 No Content <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="result">The domain result to convert.</param>
    /// <param name="http">
    /// Optional <see cref="HttpContext"/> to resolve <see cref="KnightResponseOptions"/> from DI.
    /// </param>
    /// <returns>A No Content result (or a client error if <paramref name="result"/> failed).</returns>
    public static IActionResult ToNoContentActionResult(this Result result, HttpContext? http = null)
        => ApiResults.NoContent(result, http);

    /// <summary>
    /// Converts a non-generic <see cref="Result"/> to a 400 Bad Request <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="result">The domain result to convert.</param>
    /// <param name="http">
    /// Optional <see cref="HttpContext"/> to resolve <see cref="KnightResponseOptions"/> from DI.
    /// </param>
    /// <returns>A Bad Request result using ProblemDetails/ValidationProblemDetails when enabled.</returns>
    public static IActionResult ToBadRequestActionResult(this Result result, HttpContext? http = null)
        => ApiResults.BadRequest(result, http);

    /// <summary>
    /// Converts a non-generic <see cref="Result"/> to a 404 Not Found <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="result">The domain result to convert.</param>
    /// <param name="http">
    /// Optional <see cref="HttpContext"/> to resolve <see cref="KnightResponseOptions"/> from DI.
    /// </param>
    /// <returns>A Not Found result using ProblemDetails/ValidationProblemDetails when enabled.</returns>
    public static IActionResult ToNotFoundActionResult(this Result result, HttpContext? http = null)
        => ApiResults.NotFound(result, http);

    /// <summary>
    /// Converts a non-generic <see cref="Result"/> to a 409 Conflict <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="result">The domain result to convert.</param>
    /// <param name="http">
    /// Optional <see cref="HttpContext"/> to resolve <see cref="KnightResponseOptions"/> from DI.
    /// </param>
    /// <returns>A Conflict result using ProblemDetails/ValidationProblemDetails when enabled.</returns>
    public static IActionResult ToConflictActionResult(this Result result, HttpContext? http = null)
        => ApiResults.Conflict(result, http);
}
