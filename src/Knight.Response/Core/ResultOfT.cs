using Knight.Response.Models;

namespace Knight.Response.Core;

/// <summary>
/// Represents the outcome of an operation that returns a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the value returned by the operation.</typeparam>
/// <remarks>
/// This generic version of <see cref="Result"/> encapsulates the result of an operation,
/// providing a <see cref="Status"/> to indicate completion, failure, or other states,
/// an optional <typeparamref name="T"/> value, and any associated <see cref="Message"/> objects.
/// </remarks>
/// <param name="status">The <see cref="Models.Status"/> representing the operation's final outcome.</param>
/// <param name="value">The optional value produced by the operation.</param>
/// <param name="messages">
/// An optional list of <see cref="Message"/> objects containing additional details or context about the result.
/// If omitted, an empty list is used.
/// </param>
public class Result<T>(Status status, T? value = default, IReadOnlyList<Message>? messages = null)
    : Result(status, messages)
{
    /// <summary>
    /// Gets the value produced by the operation, if any.
    /// </summary>
    /// <remarks>
    /// If the operation did not produce a value or the value is not applicable for the current <see cref="Status"/>,
    /// this property may be <c>default</c> (<c>null</c> for reference types).
    /// </remarks>
    public T? Value { get; } = value;

    /// <summary>
    /// Deconstructs the <see cref="Result{T}"/> into its <see cref="Status"/>, <see cref="Result.Messages"/>, and <see cref="Value"/>.
    /// </summary>
    /// <param name="status">The operation's final outcome.</param>
    /// <param name="messages">The collection of messages associated with the result.</param>
    /// <param name="value">The value produced by the operation, if any.</param>
    public void Deconstruct(out Status status, out IReadOnlyList<Message> messages, out T? value)
    {
        status = Status;
        messages = Messages;
        value = Value;
    }
}
