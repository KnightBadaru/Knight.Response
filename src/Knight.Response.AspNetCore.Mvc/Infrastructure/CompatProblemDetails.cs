using Microsoft.AspNetCore.Mvc;

namespace Knight.Response.AspNetCore.Mvc.Infrastructure;

// --------------------------------------------------------------------
// Compat types: add an Extensions bag and a convenience ctor for VPD
// --------------------------------------------------------------------

/// <summary>
/// MVC 2.x compatible ProblemDetails with an <see cref="Extensions"/> dictionary.
/// </summary>
internal sealed class CompatProblemDetails : ProblemDetails
{
    public IDictionary<string, object> Extensions { get; } = new Dictionary<string, object>(StringComparer.Ordinal);
}
