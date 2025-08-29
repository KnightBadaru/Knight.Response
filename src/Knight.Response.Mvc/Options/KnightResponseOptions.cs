using Knight.Response.Abstractions.Http.Options;
using Knight.Response.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace Knight.Response.Mvc.Options;

/// <summary>
/// Options controlling how Knight.Response is rendered as HTTP results.
/// </summary>
public sealed class
    KnightResponseOptions : KnightResponseBaseOptions<HttpContext, CompatProblemDetails, CompatValidationProblemDetails>;
