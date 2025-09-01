using Microsoft.AspNetCore.Mvc;

namespace Knight.Response.Mvc.Infrastructure;

/// <summary>
/// MVC 2.x compatible <see cref="ValidationProblemDetails"/>:
/// adds an <see cref="Extensions"/> bag and a ctor that accepts a
/// dictionary of field errors.
/// </summary>
public sealed class CompatValidationProblemDetails : ValidationProblemDetails
{
    /// <summary>
    /// Initializes a new instance of <see cref="CompatValidationProblemDetails"/>.
    /// </summary>
    /// <remarks>
    /// This constructor mirrors the parameterless MVC 2.x shape and
    /// also exposes an <see cref="Extensions"/> dictionary for RFC7807
    /// custom members.
    /// </remarks>
    public CompatValidationProblemDetails() { }

    /// <summary>
    /// Initializes a new instance of <see cref="CompatValidationProblemDetails"/>
    /// with the provided field validation errors.
    /// </summary>
    /// <param name="errors">
    /// A dictionary of field names to one or more error messages.
    /// If a value is <c>null</c>, it is treated as an empty array.
    /// </param>
    /// <remarks>
    /// Entries are copied into <see cref="ValidationProblemDetails.Errors"/>.
    /// This overload is convenient when you already have a field-error map
    /// (e.g., from a custom validation layer) and want to materialize a
    /// problem response in one step.
    /// </remarks>
    public CompatValidationProblemDetails(IDictionary<string, string[]> errors)
    {
        if (errors != null)
        {
            foreach (var kvp in errors)
            {
                Errors[kvp.Key] = kvp.Value ?? [];
            }
        }
    }

    /// <summary>
    /// Gets a collection for non-standard members not defined by
    /// <see cref="ValidationProblemDetails"/>.
    /// </summary>
    /// <remarks>
    /// RFC7807 allows problem details to be extended with custom members.
    /// ASP.NET Core 2.x did not expose an <c>Extensions</c> bag on
    /// <see cref="ProblemDetails"/>, so this compat type adds one.
    /// <para>
    /// Example:
    /// <code>
    /// var vpd = new CompatValidationProblemDetails(errors)
    /// {
    ///     Title = "Validation failed",
    ///     Status = 400
    /// };
    /// vpd.Extensions["svcStatus"] = "Failed";
    /// vpd.Extensions["correlationId"] = Guid.NewGuid();
    /// </code>
    /// </para>
    /// Keys are compared using <see cref="StringComparer.Ordinal"/>.
    /// </remarks>
    public IDictionary<string, object> Extensions { get; }
        = new Dictionary<string, object>(StringComparer.Ordinal);
}
