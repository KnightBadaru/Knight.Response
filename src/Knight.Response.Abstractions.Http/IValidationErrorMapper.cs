using Knight.Response.Models;

namespace Knight.Response.Abstractions.Http;

/// <summary>
/// Maps <see cref="Message"/>s from a <c>Result</c> into a field-error dictionary
/// consumable by ValidationProblemDetails.
/// </summary>
public interface IValidationErrorMapper
{
    /// <summary>
    /// Return a map of field => list of error messages. Return empty if nothing to map.
    /// </summary>
    IDictionary<string, string[]> Map(IReadOnlyList<Message> messages);
}
