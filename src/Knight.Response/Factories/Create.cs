using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates a "created" result. Defaults to <see cref="Status.Completed"/> with <see cref="ResultCodes.Created"/>.
    /// </summary>
    public static Result Created() =>
        new(Status.Completed, code: ResultCodes.Created);

    /// <summary>
    /// Creates a "created" result with custom messages (defaults code to <see cref="ResultCodes.Created"/>).
    /// </summary>
    public static Result Created(
        IReadOnlyList<Message> messages,
        ResultCode? code = null)
        => new(Status.Completed, code: code ?? ResultCodes.Created, messages: messages);

    /// <summary>
    /// Creates a "created" result with a single message (defaults code to <see cref="ResultCodes.Created"/>).
    /// </summary>
    public static Result Created(
        string message,
        MessageType type = MessageType.Information,
        IReadOnlyDictionary<string, object?>? metadata = null,
        ResultCode? code = null)
        => new(Status.Completed, code: code ?? ResultCodes.Created,
            messages: new List<Message> { new(type, message, metadata) });


    // -------- Typed --------

    /// <summary>
    /// Creates a typed "created" result. Defaults code to <see cref="ResultCodes.Created"/>.
    /// </summary>
    public static Result<T> Created<T>(ResultCode? code = null) =>
        new(Status.Completed, code: code ?? ResultCodes.Created);

    /// <summary>
    /// Creates a typed "created" result with a value. Defaults code to <see cref="ResultCodes.Created"/>.
    /// </summary>
    public static Result<T> Created<T>(T value, ResultCode? code = null) =>
        new(Status.Completed, value, code: code ?? ResultCodes.Created);

    /// <summary>
    /// Creates a typed "created" result with value + messages. Defaults code to <see cref="ResultCodes.Created"/>.
    /// </summary>
    public static Result<T> Created<T>(T value, IReadOnlyList<Message> messages, ResultCode? code = null) =>
        new(Status.Completed, value, code: code ?? ResultCodes.Created, messages: messages);

    /// <summary>
    /// Creates a typed "created" result with value + single message. Defaults code to <see cref="ResultCodes.Created"/>.
    /// </summary>
    public static Result<T> Created<T>(
        T value,
        string message,
        MessageType type = MessageType.Information,
        IReadOnlyDictionary<string, object?>? metadata = null,
        ResultCode? code = null) =>
        new(Status.Completed, value, code: code ?? ResultCodes.Created,
            messages: new List<Message> { new(type, message, metadata) });
}
