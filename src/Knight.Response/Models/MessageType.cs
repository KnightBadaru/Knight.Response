using System.Text.Json.Serialization;
using Knight.Response.Core;

namespace Knight.Response.Models;

/// <summary>
/// Represents the category or severity level of a message.
/// </summary>
/// <remarks>
/// This enum is typically used to classify messages included in a <see cref="Result"/>
/// or <see cref="Result{T}"/> to provide context to the caller.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageType
{
    /// <summary>
    /// Indicates an informational message, such as a status update or confirmation.
    /// </summary>
    Information,

    /// <summary>
    /// Indicates a warning message, which signals a potential issue or important notice,
    /// but does not prevent the operation from completing.
    /// </summary>
    Warning,

    /// <summary>
    /// Indicates an error message, which represents a failure or critical issue
    /// encountered during the operation.
    /// </summary>
    Error
}
