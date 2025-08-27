using Knight.Response.Models;

namespace Knight.Response.AspNetCore.Mvc.Mappers;

/// <summary>Maps domain messages to a field error dictionary used by ValidationProblemDetails.</summary>
public interface IValidationErrorMapper
{
    /// <summary>
    /// Return a map of field => list of error messages. Return empty if nothing to map.
    /// </summary>
    IDictionary<string, string[]> Map(IReadOnlyList<Message> messages);
}
