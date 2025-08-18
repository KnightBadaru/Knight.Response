using Knight.Response.AspNetCore.Options;
using Knight.Response.Core;
using Knight.Response.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Knight.Response.AspNetCore.Factories;

/// <summary>
/// Central place that turns a <see cref="Result"/> into an <see cref="IResult"/> using
/// <see cref="ProblemDetails"/> or <see cref="ValidationProblemDetails"/>.
/// </summary>
/// <remarks>
/// Behavior is driven by <see cref="KnightResponseOptions"/>:
/// <list type="bullet">
///   <item>
///     <description>
///     <see cref="KnightResponseOptions.StatusCodeResolver"/> maps the domain <see cref="Status"/>
///     to an HTTP status code.
///     </description>
///   </item>
///   <item>
///     <description>
///     If <see cref="KnightResponseOptions.UseValidationProblemDetails"/> is <c>true</c>, the
///     factory tries to convert messages to a validation problem via
///     <see cref="KnightResponseOptions.ValidationMapper"/>; otherwise, it falls back to a
///     standard <see cref="ProblemDetails"/> payload.
///     </description>
///   </item>
///   <item>
///     <description>
///     <see cref="KnightResponseOptions.ValidationBuilder"/> and
///     <see cref="KnightResponseOptions.ProblemDetailsBuilder"/> allow last‑mile customization.
///     </description>
///   </item>
/// </list>
/// </remarks>
internal static class ProblemFactory
{
    /// <summary>
    /// Builds an <see cref="IResult"/> response (ProblemDetails or ValidationProblemDetails) from a
    /// <see cref="Result"/> according to the supplied <see cref="KnightResponseOptions"/>.
    /// </summary>
    /// <param name="http">The current HTTP context (used for request info and DI).</param>
    /// <param name="opts">Resolved Knight Response options.</param>
    /// <param name="result">The domain operation outcome to serialize.</param>
    /// <returns>
    /// An <see cref="IResult"/> created via <see cref="Results.Problem(ProblemDetails)"/> or
    /// <see cref="Results.ValidationProblem(IDictionary{string, string[]}, string?, string?, int?, string?, string?, IDictionary{string, object}?)"/>.
    /// </returns>
    public static IResult FromResult(HttpContext http, KnightResponseOptions opts, Result result)
    {
        var statusCode = opts.StatusCodeResolver(result.Status);

        // Try validation ProblemDetails if enabled and we have mappable errors
        if (opts.UseValidationProblemDetails)
        {
            var errors = opts.ValidationMapper.Map(result.Messages);
            if (errors.Count > 0)
            {
                var vpd = new ValidationProblemDetails(errors)
                {
                    Status = statusCode,
                    Title  = "One or more validation errors occurred.",
                    Type   = $"https://httpstatuses.io/{statusCode}"
                };

                // Include useful context for clients
                vpd.Extensions["svcStatus"] = result.Status.ToString();
                vpd.Extensions["messages"] = result.Messages
                    .Select(m => new { type = m.Type.ToString(), content = m.Content, m.Metadata })
                    .ToArray();

                // Allow app to tweak/extend
                opts.ValidationBuilder?.Invoke(http, result, vpd);

                // Default instance if not set by the builder
                vpd.Instance ??= http.Request.Path;

                return Results.ValidationProblem(
                    errors:     vpd.Errors,
                    detail:     vpd.Detail,
                    instance:   vpd.Instance,
                    statusCode: vpd.Status,
                    title:      vpd.Title,
                    type:       vpd.Type,
                    extensions: vpd.Extensions
                );
            }
        }

        // Fallback to standard ProblemDetails
        var title  = result.Messages.FirstOrDefault()?.Content ?? result.Status.ToString();
        var detail = result.Messages.Count > 1
            ? string.Join("; ", result.Messages.Select(m => m.Content))
            : null;

        var pd = new ProblemDetails
        {
            Status = statusCode,
            Title  = title,
            Detail = detail,
            Type   = $"https://httpstatuses.io/{statusCode}"
        };

        // Include structured messages for clients that want richer context
        pd.Extensions["messages"]  = result.Messages
            .Select(m => new { type = m.Type.ToString(), content = m.Content })
            .ToArray();
        pd.Extensions["svcStatus"] = result.Status.ToString();

        // Allow app-level customization
        opts.ProblemDetailsBuilder?.Invoke(http, result, pd);

        return Results.Problem(
            title:      pd.Title,
            statusCode: pd.Status,
            type:       pd.Type,
            detail:     pd.Detail,
            extensions: pd.Extensions
        );
    }
}
