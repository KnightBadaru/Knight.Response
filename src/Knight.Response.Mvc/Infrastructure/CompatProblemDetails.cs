using Microsoft.AspNetCore.Mvc;

namespace Knight.Response.Mvc.Infrastructure;

// --------------------------------------------------------------------
// Compat types: add an Extensions bag and a convenience ctor for VPD
// --------------------------------------------------------------------

/// <summary>
/// MVC 2.x compatible ProblemDetails with an <see cref="Extensions"/> dictionary.
/// </summary>
public sealed class CompatProblemDetails : ProblemDetails
{
    /// <summary>
    /// Gets a collection for extension members not part of the
    /// standard <see cref="ProblemDetails"/> contract.
    /// </summary>
    /// <remarks>
    /// RFC 7807 allows problem details to be extended with additional
    /// members. In ASP.NET Core 2.x, <see cref="ProblemDetails"/> did not
    /// include an <c>Extensions</c> dictionary, so this compat type adds one.
    /// <para>
    /// Typical usage:
    /// <code>
    /// var pd = new CompatProblemDetails
    /// {
    ///     Title = "Validation failed",
    ///     Status = 400
    /// };
    /// pd.Extensions["svcStatus"] = "Failed";
    /// pd.Extensions["correlationId"] = Guid.NewGuid();
    /// </code>
    /// </para>
    /// </remarks>
    public IDictionary<string, object> Extensions { get; } = new Dictionary<string, object>(StringComparer.Ordinal);
}
