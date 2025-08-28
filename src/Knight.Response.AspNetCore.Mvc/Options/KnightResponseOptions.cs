using Knight.Response.AspNetCore.Mvc.Mappers;
using Knight.Response.Core;
using Knight.Response.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Knight.Response.AspNetCore.Mvc.Options;

/// <summary>
/// Controls how Result/Result&lt;T&gt; are mapped to <see cref="IActionResult"/>.
/// Defaults are safe: full payload off, ProblemDetails on with validation mapping.
/// </summary>
public sealed class KnightResponseOptions
{
    /// <summary>Include the full Result/Result&lt;T&gt; object as the response body on success.</summary>
    public bool IncludeFullResultPayload { get; set; } = false;

    /// <summary>Use RFC7807 <see cref="ProblemDetails"/> for failures.</summary>
    public bool UseProblemDetails { get; set; } = true;

    /// <summary>Project messages to field errors when possible (yields <see cref="ValidationProblemDetails"/>).</summary>
    public bool UseValidationProblemDetails { get; set; } = true;

    /// <summary>
    /// Map a Knight.Response domain <see cref="Status"/> to an HTTP status code for failures.
    /// </summary>
    public Func<Status, int> StatusCodeResolver { get; set; } = status =>
        status == Status.Error ? StatusCodes.Status500InternalServerError
            : StatusCodes.Status400BadRequest;

    /// <summary>Mapper that turns domain messages into validation errors.</summary>
    public IValidationErrorMapper ValidationMapper { get; set; } = new DefaultValidationErrorMapper();

    /// <summary>Optional hook to customize the generated <see cref="ProblemDetails"/>.</summary>
    public Action<HttpContext, Result, ProblemDetails>? ProblemDetailsBuilder { get; set; }

    /// <summary>Optional hook to customize the generated <see cref="ValidationProblemDetails"/>.</summary>
    public Action<HttpContext, Result, ValidationProblemDetails>? ValidationBuilder { get; set; }

    /// <summary>Singleton defaults you can copy and tweak in your app startup.</summary>
    public static KnightResponseOptions Defaults { get; } = new();
}
