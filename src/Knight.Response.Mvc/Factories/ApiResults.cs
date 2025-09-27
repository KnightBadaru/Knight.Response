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
    /// Returns 201 Created with a <c>Location</c> header. On success, returns full Result or empty body
    /// based on options; on failure emits a client error (ProblemDetails/messages).
    /// </summary>
    public static IActionResult CreatedAt(Result result, string location, HttpContext? http = null) =>
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
    /// Returns 202 Accepted with a <c>Location</c> header. On success, returns full Result or empty body
    /// based on options; on failure emits a client error (ProblemDetails/messages).
    /// </summary>
    public static IActionResult AcceptedAt(Result result, string location, HttpContext? http = null) =>
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

    // ----------------------- Explicit failure shorthands -----

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
