using Knight.Response.AspNetCore.Factories;
using Knight.Response.AspNetCore.Options;
using Knight.Response.Core;
using Microsoft.AspNetCore.Http;

namespace Knight.Response.AspNetCore.Extensions;

/// <summary>
/// Extensions to translate <see cref="Result"/> / <see cref="Result{T}"/> to ASP.NET Core <see cref="IResult"/>.
/// These honor <see cref="KnightResponseOptions"/> if a <see cref="HttpContext"/> is provided
/// (payload shape, ProblemDetails behaviour, etc.).
/// </summary>
public static class ResultHttpExtensions
{
    // -------- OK (200) -------------------------------------------------------

    /// <summary>
    /// Convert a <see cref="Result"/> into <see cref="IResult"/> (200 on success).
    /// Honors <see cref="KnightResponseOptions"/> via <paramref name="httpContext"/> if supplied.
    /// </summary>
    public static IResult ToIResult(this Result result, HttpContext? httpContext = null)
        => ApiResults.Ok(result, httpContext);

    /// <summary>
    /// Convert a <see cref="Result{T}"/> into <see cref="IResult"/> (200 on success).
    /// Honors <see cref="KnightResponseOptions"/> via <paramref name="httpContext"/> if supplied.
    /// </summary>
    public static IResult ToIResult<T>(this Result<T> result, HttpContext? httpContext = null)
        => ApiResults.Ok(result, httpContext);

    // -------- Created (201) --------------------------------------------------

    /// <summary>
    /// Convert a <see cref="Result"/> into a 201 Created <see cref="IResult"/> on success.
    /// Optionally sets the <paramref name="location"/> header. Falls back to a client error on failure.
    /// </summary>
    public static IResult ToCreatedResult(
        this Result result,
        HttpContext? httpContext = null,
        string? location = null) =>
        ApiResults.Created(result, httpContext, location);

    /// <summary>
    /// Convert a <see cref="Result{T}"/> into a 201 Created <see cref="IResult"/> on success.
    /// Optionally sets the <paramref name="location"/> header. Falls back to a client error on failure.
    /// </summary>
    public static IResult ToCreatedResult<T>(
        this Result<T> result,
        HttpContext? httpContext = null,
        string? location = null) =>
        ApiResults.Created(result, httpContext, location);

    // -------- Accepted (202) -------------------------------------------------

    /// <summary>
    /// Convert a <see cref="Result"/> into a 202 Accepted <see cref="IResult"/> on success.
    /// Optionally sets the <paramref name="location"/> header. Falls back to a client error on failure.
    /// </summary>
    public static IResult ToAcceptedResult(
        this Result result,
        HttpContext? httpContext = null,
        string? location = null) =>
        ApiResults.Accepted(result, httpContext, location);

    /// <summary>
    /// Convert a <see cref="Result{T}"/> into a 202 Accepted <see cref="IResult"/> on success.
    /// Optionally sets the <paramref name="location"/> header. Falls back to a client error on failure.
    /// </summary>
    public static IResult ToAcceptedResult<T>(
        this Result<T> result,
        HttpContext? httpContext = null,
        string? location = null) =>
        ApiResults.Accepted(result, httpContext, location);

    // -------- No Content (204) ----------------------------------------------

    /// <summary>
    /// Convert a <see cref="Result"/> into a 204 No Content <see cref="IResult"/> on success;
    /// falls back to a client error on failure.
    /// </summary>
    public static IResult ToNoContentResult(
        this Result result,
        HttpContext? httpContext = null) =>
        ApiResults.NoContent(result, httpContext);

    // -------- General (pick success status) ---------------------------------

    /// <summary>
    /// Convert a <see cref="Result"/> into an <see cref="IResult"/> using the specified success status code.
    /// On failure, a client error (and optionally ProblemDetails) is returned based on <see cref="KnightResponseOptions"/>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="successStatusCode">The status to use for successful results (e.g. 200, 201, 202, 204).</param>
    /// <param name="httpContext">Optional <see cref="HttpContext"/> to resolve options.</param>
    /// <param name="location">
    /// Optional Location header (used for 201/202 scenarios). Ignored for other status codes.
    /// </param>
    public static IResult ToHttpResult(
        this Result result,
        int successStatusCode,
        HttpContext? httpContext = null,
        string? location = null)
        => successStatusCode switch
        {
            StatusCodes.Status201Created => ApiResults.Created(result, httpContext, location),
            StatusCodes.Status202Accepted => ApiResults.Accepted(result, httpContext, location),
            StatusCodes.Status204NoContent => ApiResults.NoContent(result, httpContext),
            _ => ApiResults.Ok(result, httpContext)
        };

    /// <summary>
    /// Convert a <see cref="Result{T}"/> into an <see cref="IResult"/> using the specified success status code.
    /// On failure, a client error (and optionally ProblemDetails) is returned based on <see cref="KnightResponseOptions"/>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="successStatusCode">The status to use for successful results (e.g. 200, 201, 202).</param>
    /// <param name="httpContext">Optional <see cref="HttpContext"/> to resolve options.</param>
    /// <param name="location">
    /// Optional Location header (used for 201/202 scenarios). Ignored for other status codes.
    /// </param>
    public static IResult ToHttpResult<T>(
        this Result<T> result,
        int successStatusCode,
        HttpContext? httpContext = null,
        string? location = null)
        => successStatusCode switch
        {
            StatusCodes.Status201Created => ApiResults.Created(result, httpContext, location),
            StatusCodes.Status202Accepted => ApiResults.Accepted(result, httpContext, location),
            StatusCodes.Status204NoContent => ApiResults.NoContent(result, httpContext),
            _ => ApiResults.Ok(result, httpContext)
        };
}
