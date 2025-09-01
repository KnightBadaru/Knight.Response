using Knight.Response.Core;
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
public static class ApiResults
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

    // ----------------------- 204 / No Content ----------------

    /// <summary>
    /// Returns 204 No Content when the <paramref name="result"/> is successful.
    /// On failure, emits a client error (<see cref="ProblemDetails"/> or messages).
    /// </summary>
    public static IActionResult NoContent(Result result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status204NoContent, result);

    // ----------------------- Explicit failure shorthands -----

    /// <summary>Returns 400 Bad Request populated from <paramref name="result"/>.</summary>
    public static IActionResult BadRequest(Result result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status400BadRequest, result);

    /// <summary>Returns 404 Not Found populated from <paramref name="result"/>.</summary>
    public static IActionResult NotFound(Result result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status404NotFound, result);

    /// <summary>Returns 409 Conflict populated from <paramref name="result"/>.</summary>
    public static IActionResult Conflict(Result result, HttpContext? http = null) =>
        BuildSuccessOrFailure(http, StatusCodes.Status409Conflict, result);

    /// <summary>Returns 401 Unauthorized.</summary>
    public static IActionResult Unauthorized() => new UnauthorizedResult();

    /// <summary>Returns 403 Forbidden.</summary>
    public static IActionResult Forbidden() => new ForbidResult();

    // ======================= Core builders ====================

    /// <summary>
    /// Internal builder for non-generic <see cref="Result"/>.
    /// Chooses the appropriate <see cref="IActionResult"/> based on success/failure and configured options.
    /// </summary>
    private static IActionResult BuildSuccessOrFailure(HttpContext? http, int statusCode, Result result, string? location = null)
    {
        var opts = Resolve(http);
        if (!result.IsSuccess)
        {
            if (statusCode < StatusCodes.Status400BadRequest)
            {
                statusCode = opts.StatusCodeResolver(result.Status);
            }

            if (opts.UseProblemDetails && http is not null)
            {
                // Centralised ProblemDetails formatting
                return ProblemFactory.FromResult(http, opts, result, statusCode);
            }

            // Simple fallback: return messages with the requested status.
            return new ObjectResult(opts.IncludeFullResultPayload ? result : result.Messages) { StatusCode = statusCode };
        }

        // NoContent
        if (statusCode == StatusCodes.Status204NoContent)
        {
            return new ObjectResult(null) { StatusCode = StatusCodes.Status204NoContent };
        }

        if (http is not null)
        {
            http.Response.Headers["Location"] = location ?? string.Empty;
        }

        return new ObjectResult(opts.IncludeFullResultPayload ? result : null) { StatusCode = statusCode };
    }

    /// <summary>
    /// Internal builder for generic <see cref="Result{T}"/>.
    /// Chooses the appropriate <see cref="IActionResult"/> based on success/failure and configured options.
    /// </summary>
    private static IActionResult BuildSuccessOrFailure<T>(HttpContext? http, int statusCode, Result<T> result, string? location = null)
    {
        var opts = Resolve(http);
        if (!result.IsSuccess)
        {
            if (statusCode < StatusCodes.Status400BadRequest)
            {
                statusCode = opts.StatusCodeResolver(result.Status);
            }

            if (opts.UseProblemDetails && http is not null)
            {
                // Centralised ProblemDetails formatting
                return ProblemFactory.FromResult(http, opts, result, statusCode);
            }

            // Simple fallback: return messages with the requested status.
            return new ObjectResult(opts.IncludeFullResultPayload ? result : result.Messages) { StatusCode = statusCode };
        }

        // NoContent
        if (statusCode == StatusCodes.Status204NoContent)
        {
            return new ObjectResult(null) { StatusCode = StatusCodes.Status204NoContent };
        }

        if (http is not null)
        {
            http.Response.Headers["Location"] = location ?? string.Empty;
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
}
