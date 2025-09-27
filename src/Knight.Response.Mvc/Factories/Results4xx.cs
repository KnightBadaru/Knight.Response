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
    /// <summary>
    /// Returns 400 Bad Request populated from <paramref name="result"/>.
    /// </summary>
    public static IActionResult BadRequest(Result result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status400BadRequest, result);

    /// <summary>
    /// Returns 404 Not Found populated from <paramref name="result"/>
    /// .</summary>
    public static IActionResult NotFound(Result result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status404NotFound, result);

    /// <summary>
    /// Returns 409 Conflict populated from <paramref name="result"/>.
    /// </summary>
    public static IActionResult Conflict(Result result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status409Conflict, result);

    /// <summary>
    /// Returns 401 Unauthorized.
    /// </summary>
    public static IActionResult Unauthorized() => new UnauthorizedResult();

    /// <summary>Returns 403 Forbidden.</summary>
    public static IActionResult Forbidden() => new ForbidResult();

    // ----------------------- 422 / Unprocessable Entity -----------------------

    /// <summary>
    /// Returns 422 Unprocessable Entity populated from <paramref name="result"/>.
    /// </summary>
    public static IActionResult UnprocessableEntity(Result result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status422UnprocessableEntity, result);

    /// <summary>
    /// Returns 422 Unprocessable Entity populated from <paramref name="result"/>.
    /// </summary>
    public static IActionResult UnprocessableEntity<T>(Result<T> result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status422UnprocessableEntity, result);

    // ----------------------- 429 / Too Many Requests --------------------------

    /// <summary>
    /// Returns 429 Too Many Requests populated from <paramref name="result"/>.
    /// </summary>
    public static IActionResult TooManyRequests(Result result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status429TooManyRequests, result);

    /// <summary>
    /// Returns 429 Too Many Requests populated from <paramref name="result"/>.
    /// </summary>
    public static IActionResult TooManyRequests<T>(Result<T> result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status429TooManyRequests, result);

    // ----------------------- 503 / Service Unavailable ------------------------

    /// <summary>
    /// Returns 503 Service Unavailable populated from <paramref name="result"/>.
    /// </summary>
    public static IActionResult ServiceUnavailable(Result result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status503ServiceUnavailable, result);

    /// <summary>
    /// Returns 503 Service Unavailable populated from <paramref name="result"/>.
    /// </summary>
    public static IActionResult ServiceUnavailable<T>(Result<T> result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status503ServiceUnavailable, result);
}
