using Knight.Response.AspNetCore.Mappers;
using Knight.Response.Core;
using Knight.Response.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Knight.Response.AspNetCore.Options;

/// <summary>
/// Options controlling how Knight.Response is rendered as HTTP results.
/// </summary>
public sealed class KnightResponseOptions
{
    /// <summary>
    /// Provides a reusable set of default options.
    /// </summary>
    public static KnightResponseOptions Defaults { get; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to include the full <see cref="Result"/> or <see cref="Result{T}"/>
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
    /// <para>
    /// When <c>true</c>, exception messages and stack traces will be included in <see cref="Result"/> payloads
    /// and/or <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/>. This should only be enabled in development environments.
    /// </para>
    /// Default is <c>false</c>.
    /// </summary>
    public bool IncludeExceptionDetails { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to use RFC7807 <c>application/problem+json</c> for failures.
    /// <para>
    /// When <c>true</c>, failures will be serialized as <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/>
    /// instead of raw message collections.
    /// </para>
    /// Default is <c>false</c>.
    /// </summary>
    public bool UseProblemDetails { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether validation-style failures should be serialized as
    /// <see cref="Microsoft.AspNetCore.Mvc.ValidationProblemDetails"/>.
    /// <para>
    /// This only applies when <see cref="UseProblemDetails"/> is also <c>true</c>.
    /// </para>
    /// Default is <c>false</c>.
    /// </summary>
    public bool UseValidationProblemDetails { get; set; } = false;

    /// <summary>
    /// Function that maps a <see cref="Status"/> to an HTTP status code for ProblemDetails responses.
    /// Defaults: Failed=400, Cancelled=409, Error=500. Completed isn't used here.
    /// </summary>
    public Func<Status, int> StatusCodeResolver { get; set; } = status => status switch
    {
        Status.Failed    => 400,
        Status.Cancelled => 409,
        Status.Error     => 500,
        _                => 400
    };

    /// <summary>
    /// Gives you full control to customise the ProblemDetails before it is sent.
    /// This is only called for non-success results when <see cref="UseProblemDetails"/> is true.
    /// </summary>
    public Action<HttpContext, Result, ProblemDetails>? ProblemDetailsBuilder { get; set; }

    /// <summary>
    /// Gets or sets the validation error mapper used to convert Knight.Response messages into model state dictionaries.
    /// </summary>
    public IValidationErrorMapper ValidationMapper { get; set; } = new DefaultValidationErrorMapper();

    /// <summary>
    /// Optional hook to tweak the created ValidationProblemDetails.
    /// </summary>
    public Action<HttpContext, Result, ValidationProblemDetails>? ValidationBuilder { get; set; }
}
