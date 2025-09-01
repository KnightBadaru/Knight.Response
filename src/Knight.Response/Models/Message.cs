using System.Collections.ObjectModel;

namespace Knight.Response.Models;

/// <summary>
/// Represents a message associated with a result, including its type, content, and optional metadata.
/// </summary>
/// <remarks>
/// A <see cref="Message"/> is typically used to provide additional context or feedback
/// about an operation, such as informational notices, warnings, or errors.
/// Metadata can be included to store structured data related to the message.
/// The <see cref="Metadata"/> dictionary is defensively copied and wrapped in a read-only view.
/// </remarks>
/// <param name="type">The <see cref="MessageType"/> indicating the message's category or severity.</param>
/// <param name="content">The human-readable content or description of the message.</param>
/// <param name="metadata">
/// Optional key-value pairs providing structured data or contextual details
/// about the message.
/// </param>
public class Message(MessageType type, string content, IReadOnlyDictionary<string, object?>? metadata = null)
{
    /// <summary>
    /// Gets the category or severity of the message.
    /// </summary>
    public MessageType Type { get; } = type;

    /// <summary>
    /// Gets the human-readable content or description of the message.
    /// </summary>
    public string Content { get; } = content;

    /// <summary>
    /// Gets additional structured data associated with the message.
    /// </summary>
    /// <remarks>
    /// This dictionary may be empty if no metadata was provided.
    /// </remarks>
    public IReadOnlyDictionary<string, object?> Metadata { get; } =
        metadata is null
            ? new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>())
            : new ReadOnlyDictionary<string, object?>(metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase));
}
