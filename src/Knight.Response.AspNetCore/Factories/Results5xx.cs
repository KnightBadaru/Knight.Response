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
    /// Produces a <c>503 Service Unavailable</c> response populated from <paramref name="result"/>.
    /// Uses ProblemDetails if <see cref="KnightResponseOptions.UseProblemDetails"/> is enabled.
    /// </summary>
    public static IResult ServiceUnavailable(Result result, HttpContext? http = null) =>
        BuildFailure(http, result, StatusCodes.Status503ServiceUnavailable);

    /// <summary>
    /// Produces a <c>503 Service Unavailable</c> response populated from <paramref name="result"/>.
    /// Uses ProblemDetails if <see cref="KnightResponseOptions.UseProblemDetails"/> is enabled.
    /// </summary>
    public static IResult ServiceUnavailable<T>(Result<T> result, HttpContext? http = null) =>
        BuildFailure(http, result, StatusCodes.Status503ServiceUnavailable);
}
