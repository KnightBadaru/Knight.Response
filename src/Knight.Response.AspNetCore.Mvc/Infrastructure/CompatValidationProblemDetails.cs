using Microsoft.AspNetCore.Mvc;

namespace Knight.Response.AspNetCore.Mvc.Infrastructure;

/// <summary>
/// MVC 2.x compatible ValidationProblemDetails:
/// adds an <see cref="Extensions"/> bag and a ctor that accepts a dictionary of errors.
/// </summary>
internal sealed class CompatValidationProblemDetails : ValidationProblemDetails
{
    public CompatValidationProblemDetails() { }

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

    public IDictionary<string, object> Extensions { get; } = new Dictionary<string, object>(StringComparer.Ordinal);
}
