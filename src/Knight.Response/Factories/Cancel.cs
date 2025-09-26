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
}
