using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates a cancelled <see cref="Result"/> with messages.
    /// </summary>
    public static Result Cancel() => new(Status.Cancelled);

    /// <summary>
    /// Creates a cancelled <see cref="Result"/> with an optional domain <see cref="ResultCode"/>.
    /// </summary>
    /// <remarks>
    /// Useful when an operation is aborted, and you want to carry only a domain reason
    /// (messages can still be added later with extension helpers).
    /// </remarks>
    public static Result Cancel(ResultCode? code)
        => new(Status.Cancelled, code: code);

    /// <summary>
    /// Creates a cancelled <see cref="Result"/> with messages.
    /// </summary>
    public static Result Cancel(IReadOnlyList<Message> messages) => new(Status.Cancelled, messages: messages);

    /// <summary>
    /// Creates a cancelled <see cref="Result"/> with messages and optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result Cancel(
        IReadOnlyList<Message> messages,
        ResultCode? code)
        => new(Status.Cancelled, code: code, messages: messages);

    /// <summary>
    /// Creates a cancelled <see cref="Result"/> with a single message (defaults to warning).
    /// </summary>
    public static Result Cancel(
        string reason,
        MessageType type = MessageType.Warning,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Cancelled, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>
    /// Creates a cancelled <see cref="Result"/> with a single message and optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result Cancel(
        string reason,
        ResultCode? code,
        MessageType type = MessageType.Warning,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Cancelled, code: code, messages: new List<Message> { new(type, reason, metadata) });


    // -------- Typed --------

    /// <summary>
    /// Creates a cancelled <see cref="Result{T}"/>.
    /// </summary>
    public static Result<T> Cancel<T>() => new(Status.Cancelled);

    /// <summary>
    /// Creates a cancelled <see cref="Result{T}"/> with messages.
    /// </summary>
    public static Result<T> Cancel<T>(IReadOnlyList<Message> messages) => new(Status.Cancelled, messages: messages);

    /// <summary>
    /// Creates a cancelled <see cref="Result{T}"/> with messages and optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result<T> Cancel<T>(
        IReadOnlyList<Message> messages,
        ResultCode? code)
        => new(Status.Cancelled, code: code, messages: messages);

    /// <summary>
    /// Creates a cancelled <see cref="Result{T}"/> with a single message (defaults to warning).
    /// </summary>
    public static Result<T> Cancel<T>(
        string reason,
        MessageType type = MessageType.Warning,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Cancelled, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>
    /// Creates a cancelled <see cref="Result{T}"/> with a single message and optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result<T> Cancel<T>(
        string reason, ResultCode? code,
        MessageType type = MessageType.Warning,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Cancelled, code: code, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>
    /// Creates a cancelled <see cref="Result{T}"/> that carries the current entity state.
    /// </summary>
    /// <typeparam name="T">The entity/value type.</typeparam>
    /// <param name="value">A snapshot of the current state to return.</param>
    public static Result<T> Cancel<T>(T value)
        => new(Status.Cancelled, value);

    /// <summary>
    /// Creates a cancelled <see cref="Result{T}"/> that carries the current entity state and an optional domain <see cref="ResultCode"/>.
    /// </summary>
    /// <typeparam name="T">The entity/value type.</typeparam>
    /// <param name="value">A snapshot of the current state to return.</param>
    /// <param name="code">Optional domain code (e.g., <c>ResultCodes.UserCancelled</c>).</param>
    public static Result<T> Cancel<T>(T value, ResultCode? code)
        => new(Status.Cancelled, value, code: code);

    /// <summary>
    /// Creates a cancelled <see cref="Result{T}"/> with current state and messages.
    /// </summary>
    /// <typeparam name="T">The entity/value type.</typeparam>
    /// <param name="value">A snapshot of the current state to return.</param>
    /// <param name="messages">Additional messages explaining the cancellation.</param>
    public static Result<T> Cancel<T>(T value, IReadOnlyList<Message> messages)
        => new(Status.Cancelled, value, messages: messages);

    /// <summary>
    /// Creates a cancelled <see cref="Result{T}"/> with current state, messages, and an optional domain <see cref="ResultCode"/>.
    /// </summary>
    /// <typeparam name="T">The entity/value type.</typeparam>
    /// <param name="value">A snapshot of the current state to return.</param>
    /// <param name="messages">Additional messages explaining the cancellation.</param>
    /// <param name="code">Optional domain code (e.g., <c>ResultCodes.UserCancelled</c>).</param>
    public static Result<T> Cancel<T>(T value, IReadOnlyList<Message> messages, ResultCode? code)
        => new(Status.Cancelled, value, code: code, messages: messages);

    /// <summary>
    /// Creates a cancelled <see cref="Result{T}"/> with current state and a single message (defaults to <see cref="MessageType.Warning"/>).
    /// </summary>
    /// <typeparam name="T">The entity/value type.</typeparam>
    /// <param name="value">A snapshot of the current state to return.</param>
    /// <param name="reason">Human-readable reason for the cancellation.</param>
    /// <param name="type">Message type (defaults to <see cref="MessageType.Warning"/>).</param>
    /// <param name="metadata">Optional metadata for the message.</param>
    public static Result<T> Cancel<T>(
        T value,
        string reason,
        MessageType type = MessageType.Warning,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Cancelled, value, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>
    /// Creates a cancelled <see cref="Result{T}"/> with current state, a single message, and an optional domain <see cref="ResultCode"/>.
    /// </summary>
    /// <typeparam name="T">The entity/value type.</typeparam>
    /// <param name="value">A snapshot of the current state to return.</param>
    /// <param name="reason">Human-readable reason for the cancellation.</param>
    /// <param name="code">Optional domain code (e.g., <c>ResultCodes.UserCancelled</c>).</param>
    /// <param name="type">Message type (defaults to <see cref="MessageType.Warning"/>).</param>
    /// <param name="metadata">Optional metadata for the message.</param>
    public static Result<T> Cancel<T>(
        T value,
        string reason,
        ResultCode? code,
        MessageType type = MessageType.Warning,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Cancelled, value, code: code, messages: new List<Message> { new(type, reason, metadata) });
}
