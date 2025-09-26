using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates a "deleted" result. Defaults to <see cref="ResultCodes.Deleted"/>.
    /// </summary>
    public static Result Deleted() =>
        new(Status.Completed, code: ResultCodes.Deleted);

    /// <summary>
    /// Creates a "deleted" result with custom messages. Defaults code to <see cref="ResultCodes.Deleted"/>.
    /// </summary>
    public static Result Deleted(IReadOnlyList<Message> messages, ResultCode? code = null) =>
        new(Status.Completed, code: code ?? ResultCodes.Deleted, messages: messages);

    /// <summary>
    /// Creates a "deleted" result with a single message. Defaults code to <see cref="ResultCodes.Deleted"/>.
    /// </summary>
    public static Result Deleted(
        string message,
        MessageType type = MessageType.Information,
        IReadOnlyDictionary<string, object?>? metadata = null,
        ResultCode? code = null) =>
        new(Status.Completed, code: code ?? ResultCodes.Deleted,
            messages: new List<Message> { new(type, message, metadata) });


    // -------- Typed --------

    /// <summary>
    /// Creates a typed "deleted" result. Defaults code to <see cref="ResultCodes.Deleted"/>.
    /// </summary>
    public static Result<T> Deleted<T>(ResultCode? code = null) =>
        new(Status.Completed, code: code ?? ResultCodes.Deleted);

    /// <summary>
    /// Creates a typed "deleted" result with a value. Defaults code to <see cref="ResultCodes.Deleted"/>.
    /// </summary>
    public static Result<T> Deleted<T>(T value, ResultCode? code = null) =>
        new(Status.Completed, value, code: code ?? ResultCodes.Deleted);

    /// <summary>
    /// Creates a typed "deleted" result with value + messages. Defaults code to <see cref="ResultCodes.Deleted"/>.
    /// </summary>
    public static Result<T> Deleted<T>(T value, IReadOnlyList<Message> messages, ResultCode? code = null) =>
        new(Status.Completed, value, code: code ?? ResultCodes.Deleted, messages: messages);

    /// <summary>
    /// Creates a typed "deleted" result with value + single message. Defaults code to <see cref="ResultCodes.Deleted"/>.
    /// </summary>
    public static Result<T> Deleted<T>(
        T value,
        string message,
        MessageType type = MessageType.Information,
        IReadOnlyDictionary<string, object?>? metadata = null,
        ResultCode? code = null) =>
        new(Status.Completed, value, code: code ?? ResultCodes.Deleted,
            messages: new List<Message> { new(type, message, metadata) });
}
