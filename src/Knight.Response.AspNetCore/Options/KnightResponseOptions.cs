using Knight.Response.Abstractions.Http.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Knight.Response.AspNetCore.Options;

/// <summary>
/// Options controlling how Knight.Response is rendered as HTTP results.
/// </summary>
public sealed class
    KnightResponseOptions : KnightResponseBaseOptions<HttpContext, ProblemDetails, ValidationProblemDetails>;
