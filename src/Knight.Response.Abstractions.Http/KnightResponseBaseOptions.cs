using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Abstractions.Http
{
    /// <summary>
    /// Framework-agnostic HTTP shaping options for Knight.Response.
    /// Concrete HTTP packages should derive a non-generic options class that
    /// closes <typeparamref name="TP"/> and <typeparamref name="TV"/> to their
    /// framework-native ProblemDetails/ValidationProblemDetails types.
    /// </summary>
    /// <typeparam name="TP">ProblemDetails type for the host framework.</typeparam>
    /// <typeparam name="TV">ValidationProblemDetails type for the host framework.</typeparam>
    public class KnightResponseBaseOptions<TP, TV>
    {
        /// <summary>
        /// Provides a reusable set of defaults (copy-on-read) that host
        /// frameworks may use when options are not registered.
        /// </summary>
        public static KnightResponseBaseOptions<TP, TV> Defaults => new();

        /// <summary>
        /// Gets or sets a value indicating whether to include the full <see cref="Result{T}"/> or <see cref="Result{T}"/>
        /// payload in successful responses.
        /// <para>
        /// When <c>false</c>, only the <c>Value</c> (for <see cref="Result{T}"/>) or an empty body is returned on success.
        /// </para>
        /// Default is <c>true</c>.
        /// </summary>
        public bool IncludeFullResultPayload { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to include exception details in responses produced
        /// by <c>KnightResponseExceptionMiddleware</c>.
        /// Default is <c>false</c>.
        /// </summary>
        public bool IncludeExceptionDetails { get; set; } = false;

        // /// <summary>
        // /// When <c>true</c>, failures are represented as RFC7807 ProblemDetails.
        // /// </summary>

        /// <summary>
        /// Gets or sets a value indicating whether to use RFC7807 <c>application/problem+json</c> for failures.
        /// Default is <c>false</c>.
        /// </summary>
        public bool UseProblemDetails { get; set; } = false;

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
        public bool UseValidationProblemDetails { get; set; } = false;

        // /// <summary>
        // /// Maps a domain <see cref="Status"/> to an HTTP status code.
        // /// </summary>

        /// <summary>
        /// Function that maps a <see cref="Status"/> to an HTTP status code for ProblemDetails responses.
        /// Defaults: Failed=400, Cancelled=409, Error=500. Completed isn't used here.
        /// </summary>
        public Func<Status, int> StatusCodeResolver { get; set; } = DefaultStatusResolver;

        /// <summary>
        /// Optional last‑mile customization hook for standard ProblemDetails.
        /// </summary>
        public Action<object /*HttpContext*/, object /*Result*/, TP>? ProblemDetailsBuilder { get; set; }

        /// <summary>
        /// Optional last‑mile customization hook for ValidationProblemDetails.
        /// </summary>
        public Action<object /*HttpContext*/, object /*Result*/, TV>? ValidationBuilder { get; set; }

        /// <summary>
        /// Gets or sets the validation error mapper used to convert Knight.Response messages into model state dictionaries.
        /// </summary>
        public IValidationErrorMapper ValidationMapper { get; set; } = new DefaultValidationErrorMapper();

        private static int DefaultStatusResolver(Status s) =>
            s == Status.Error      ? 500 :
            s == Status.Cancelled  ? 409 :
            s == Status.Failed     ? 400 :
                                     200;
    }
}
