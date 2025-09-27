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

    // 422 / Unprocessable Entity -----------------------------------------------

    /// <summary>
    /// Produces a <c>422 Unprocessable Entity</c> response populated from <paramref name="result"/>.
    /// Uses ProblemDetails if <see cref="KnightResponseOptions.UseProblemDetails"/> is enabled.
    /// </summary>
    public static IResult UnprocessableEntity(Result result, HttpContext? http = null) =>
        BuildFailure(http, result, StatusCodes.Status422UnprocessableEntity);

    /// <summary>
    /// Produces a <c>422 Unprocessable Entity</c> response populated from <paramref name="result"/>.
    /// Uses ProblemDetails if <see cref="KnightResponseOptions.UseProblemDetails"/> is enabled.
    /// </summary>
    public static IResult UnprocessableEntity<T>(Result<T> result, HttpContext? http = null) =>
        BuildFailure(http, result, StatusCodes.Status422UnprocessableEntity);

    // 429 / Too Many Requests ---------------------------------------------------

    /// <summary>
    /// Produces a <c>429 Too Many Requests</c> response populated from <paramref name="result"/>.
    /// Uses ProblemDetails if <see cref="KnightResponseOptions.UseProblemDetails"/> is enabled.
    /// </summary>
    public static IResult TooManyRequests(Result result, HttpContext? http = null) =>
        BuildFailure(http, result, StatusCodes.Status429TooManyRequests);

    /// <summary>
    /// Produces a <c>429 Too Many Requests</c> response populated from <paramref name="result"/>.
    /// Uses ProblemDetails if <see cref="KnightResponseOptions.UseProblemDetails"/> is enabled.
    /// </summary>
    public static IResult TooManyRequests<T>(Result<T> result, HttpContext? http = null) =>
        BuildFailure(http, result, StatusCodes.Status429TooManyRequests);

}
