using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates a failed <see cref="Result"/> with messages.
    /// </summary>
    public static Result Failure(IReadOnlyList<Message> messages) => new(Status.Failed, messages: messages);

    /// <summary>
    /// Creates a failed <see cref="Result"/> with messages and an optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result Failure(IReadOnlyList<Message> messages, ResultCode? code) => new(Status.Failed, code: code, messages: messages);

    /// <summary>
    /// Creates a failed <see cref="Result"/> with a single message.
    /// </summary>
    /// <param name="reason">The failure reason.</param>
    /// <param name="type">Message type for the reason (defaults to <see cref="MessageType.Error"/>).</param>
    /// <param name="metadata">Optional metadata.</param>
    public static Result Failure(
        string reason,
        MessageType type = MessageType.Error,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Failed, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>
    /// Creates a failed <see cref="Result"/> with a single message and optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result Failure(
        string reason,
        ResultCode? code,
        MessageType type = MessageType.Error,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Failed, code: code, messages: new List<Message> { new(type, reason, metadata) });


    // -------- Typed --------

    /// <summary>
    /// Creates a failed <see cref="Result{T}"/> with messages.
    /// </summary>
    public static Result<T> Failure<T>(IReadOnlyList<Message> messages) => new(Status.Failed, value: default, messages: messages);

    /// <summary>
    /// Creates a failed <see cref="Result{T}"/> with messages and optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result<T> Failure<T>(IReadOnlyList<Message> messages, ResultCode? code)
        => new(Status.Failed, code: code, messages: messages);

    /// <summary>
    /// Creates a failed <see cref="Result{T}"/> with a single message.
    /// </summary>
    public static Result<T> Failure<T>(
        string reason,
        MessageType type = MessageType.Error,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Failed, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>
    /// Creates a failed <see cref="Result{T}"/> with a single message and optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result<T> Failure<T>(
        string reason,
        ResultCode? code,
        MessageType type = MessageType.Error,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Failed, code: code, messages: new List<Message> { new(type, reason, metadata) });
}
