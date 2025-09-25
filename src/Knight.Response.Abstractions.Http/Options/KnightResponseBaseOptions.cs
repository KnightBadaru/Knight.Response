using Knight.Response.Abstractions.Http.Mappers;
using Knight.Response.Abstractions.Http.Resolution;
using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Abstractions.Http.Options;

/// <summary>
/// Framework-agnostic HTTP shaping options for Knight.Response.
/// Concrete HTTP packages should derive a non-generic options class that
/// closes <typeparamref name="TP"/> and <typeparamref name="TV"/> to their
/// framework-native ProblemDetails/ValidationProblemDetails types.
/// </summary>
/// <typeparam name="TH">HttpContext for the host framework.</typeparam>
/// <typeparam name="TP">ProblemDetails type for the host framework.</typeparam>
/// <typeparam name="TV">ValidationProblemDetails type for the host framework.</typeparam>
public class KnightResponseBaseOptions<TH, TP, TV>
{
    /// <summary>
    /// Provides a reusable set of defaults (copy-on-read) that host
    /// frameworks may use when options are not registered.
    /// </summary>
    public static KnightResponseBaseOptions<TH, TP, TV> Defaults => new();

    /// <summary>
    /// Gets or sets a value indicating whether to include the full <see cref="Result{T}"/> or <see cref="Result{T}"/>
    /// payload in successful responses.
    /// <para>
    /// When <c>false</c>, only the <c>Value</c> (for <see cref="Result{T}"/>) or an empty body is returned on success.
    /// </para>
    /// Default is <c>false</c>.
    /// </summary>
    public bool IncludeFullResultPayload { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include exception details in responses produced
    /// by <c>KnightResponseExceptionMiddleware</c>.
    /// Default is <c>false</c>.
    /// </summary>
    public bool IncludeExceptionDetails { get; set; }

    // /// <summary>
    // /// When <c>true</c>, failures are represented as RFC7807 ProblemDetails.
    // /// </summary>

    /// <summary>
    /// Gets or sets a value indicating whether to use RFC7807 <c>application/problem+json</c> for failures.
    /// Default is <c>false</c>.
    /// </summary>
    public bool UseProblemDetails { get; set; }

    // /// <summary>
    // /// When <c>true</c> and a mapper yields field errors, failures are
    // /// represented as ValidationProblemDetails; otherwise fall back to
    // /// standard ProblemDetails.
    // /// </summary>

    /// <summary>
    /// Gets or sets a value indicating whether validation-style failures should be serialized as
    /// ValidationProblemDetails.
    /// <para>
    /// This only applies when <see cref="UseProblemDetails"/> is also <c>true</c>.
    /// </para>
    /// Default is <c>false</c>.
    /// </summary>
    public bool UseValidationProblemDetails { get; set; }

    /// <summary>
    /// Optional delegate for mapping a domain-level <see cref="ResultCode"/> to an HTTP status code.
    /// </summary>
    /// <remarks>
    /// This allows consumers to override or extend the built-in status mapping logic.
    /// <list type="bullet">
    ///   <item><description>
    ///   If provided, this delegate will be invoked first for any <see cref="Result.Code"/>.
    ///   </description></item>
    ///   <item><description>
    ///   If it returns a non-<c>null</c> value, that HTTP code will be used directly.
    ///   </description></item>
    ///   <item><description>
    ///   If it returns <c>null</c> or is <c>null</c> itself, the pipeline falls back to
    ///   <see cref="StatusCodeResolver"/> which maps based on <see cref="Status"/>.
    ///   </description></item>
    /// </list>
    /// Example usage:
    /// <code>
    /// options.CodeToHttp = code => code?.Value switch
    /// {
    ///     "NotFound"      => StatusCodes.Status404NotFound,
    ///     "AlreadyExists" => StatusCodes.Status409Conflict,
    ///     _ => null
    /// };
    /// </code>
    /// </remarks>
    public Func<ResultCode?, int?>? CodeToHttp { get; set; }

    /// <summary>
    /// Function that maps a <see cref="Status"/> to an HTTP status code for ProblemDetails responses.
    /// Defaults: Failed=400, Cancelled=409, Error=500, Completed=200.
    /// <para>
    /// If <c>null</c>, <see cref="KnightResponseHttpDefaults.StatusToHttp"/> will be used as fallback.
    /// </para>
    /// </summary>
    public Func<Status, int>? StatusCodeResolver { get; set; }

    /// <summary>
    /// Optional last‑mile customization hook for standard ProblemDetails.
    /// </summary>
    public Action<TH, Result, TP>? ProblemDetailsBuilder { get; set; }

    /// <summary>
    /// Optional last‑mile customization hook for ValidationProblemDetails.
    /// </summary>
    public Action<TH, Result, TV>? ValidationBuilder { get; set; }


    /// <summary>
    /// Optional override. If not set, the mapper is resolved from the current request's DI container:
    /// <c>http.RequestServices.GetService&lt;IValidationErrorMapper&gt;()</c>.
    /// Use this to force a specific instance only if you know you do not need per-request scope.
    /// </summary>
    public IValidationErrorMapper? ValidationMapper { get; set; }
}
