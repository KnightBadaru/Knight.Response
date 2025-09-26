using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates an error <see cref="Result"/> with messages.
    /// </summary>
    public static Result Error(IReadOnlyList<Message> messages) => new(Status.Error, messages: messages);

    /// <summary>
    /// Creates an error <see cref="Result"/> with messages and optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result Error(
        IReadOnlyList<Message> messages,
        ResultCode? code)
        => new(Status.Error, code: code, messages: messages);

    /// <summary>
    /// reates an error <see cref="Result"/> with a single message.
    /// </summary>
    public static Result Error(
        string reason,
        MessageType type = MessageType.Error,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Error, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>
    /// Creates an error <see cref="Result"/> with a single message and optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result Error(
        string reason,
        ResultCode? code,
        MessageType type = MessageType.Error,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Error, code: code, messages: new List<Message> { new(type, reason, metadata) });


    // -------- Typed --------

    /// <summary>
    /// Creates an error <see cref="Result{T}"/> with messages.
    /// </summary>
    public static Result<T> Error<T>(IReadOnlyList<Message> messages) => new(Status.Error, value: default, messages: messages);

    /// <summary>
    /// Creates an error <see cref="Result{T}"/> with messages and optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result<T> Error<T>(IReadOnlyList<Message> messages, ResultCode? code)
        => new(Status.Error, code: code, messages: messages);

    /// <summary>
    /// Creates an error <see cref="Result{T}"/> with a single message.
    /// </summary>
    public static Result<T> Error<T>(
        string reason,
        MessageType type = MessageType.Error,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Error, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>
    /// Creates an error <see cref="Result{T}"/> with a single message and optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result<T> Error<T>(
        string reason,
        ResultCode? code,
        MessageType type = MessageType.Error,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Error, code: code, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>
    /// Wraps an <see cref="Exception"/> as an error <see cref="Result"/>.
    /// </summary>
    public static Result Error(Exception ex) => Error(ex.Message);

    /// <summary>
    /// Wraps an <see cref="Exception"/> as an error <see cref="Result{T}"/>.
    /// </summary>
    public static Result<T> Error<T>(Exception ex) => Error<T>(ex.Message);
}
