namespace Knight.Response.AspNetCore.Mappers;

/// <summary>
/// Maps Result messages to a ValidationProblemDetails `errors` dictionary.
/// </summary>
public interface IValidationErrorMapper
{
    /// <summary>
    /// Return a map of field => list of error messages. Return empty if nothing to map.
    /// </summary>
    IDictionary<string, string[]> Map(IReadOnlyList<Knight.Response.Models.Message> messages);
}
