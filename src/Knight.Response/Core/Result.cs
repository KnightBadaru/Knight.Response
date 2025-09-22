using Knight.Response.Models;

namespace Knight.Response.Core;

/// <summary>
/// Represents the outcome of an operation, including its final status and any associated messages.
/// </summary>
/// <remarks>
/// This class encapsulates the result of an operation, providing a <see cref="Status"/> value
/// to indicate completion, failure, or other states, and optionally a list of <see cref="Message"/>
/// objects for additional details.
/// </remarks>
/// <param name="status">The <see cref="Models.Status"/> representing the operation's final outcome.</param>
/// <param name="code">Optional <see cref="Models.ResultCode"/> representing domain reason for the final outcome.</param>
/// <param name="messages">
/// An optional list of <see cref="Message"/> objects containing additional details or context about the result.
/// If omitted, an empty list is used.
/// </param>
public class Result(Status status, ResultCode? code = null, IReadOnlyList<Message>? messages = null)
{
    /// <summary>
    /// Gets the operation's final outcome e.g. Completed/Cancelled/Failed/Error
    /// </summary>
    public Status Status { get; } = status;

    /// <summary>Optional domain reason (transport-agnostic), e.g. "NotFound".</summary>
    public ResultCode? Code { get; } = code;

    /// <summary>
    /// Gets the collection of messages providing context or feedback for the result.
    /// </summary>
    /// <remarks>
    /// This property is never <c>null</c>. If no messages were provided, an empty list is returned.
    /// </remarks>
    public IReadOnlyList<Message> Messages { get; } = messages ?? [];

    /// <summary>
    /// Deconstructs the <see cref="Result"/> into its <see cref="Status"/> and <see cref="Messages"/>.
    /// </summary>
    /// <param name="status">The operation's final outcome.</param>
    /// <param name="messages">The collection of messages associated with the result.</param>
    public void Deconstruct(out Status status, out IReadOnlyList<Message> messages)
    {
        status = Status;
        messages = Messages;
    }
}
