using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates an "updated" result. Defaults to <see cref="ResultCodes.Updated"/>.
    /// </summary>
    public static Result Updated() =>
        new(Status.Completed, code: ResultCodes.Updated);

    /// <summary>
    /// Creates an "updated" result with custom messages. Defaults code to <see cref="ResultCodes.Updated"/>.
    /// </summary>
    public static Result Updated(
        IReadOnlyList<Message> messages,
        ResultCode? code = null) =>
        new(Status.Completed, code: code ?? ResultCodes.Updated, messages: messages);

    /// <summary>
    /// Creates an "updated" result with a single message. Defaults code to <see cref="ResultCodes.Updated"/>.
    /// </summary>
    public static Result Updated(
        string message,
        MessageType type = MessageType.Information,
        IReadOnlyDictionary<string, object?>? metadata = null,
        ResultCode? code = null)
        => new(Status.Completed, code: code ?? ResultCodes.Updated,
            messages: new List<Message> { new(type, message, metadata) });


    // -------- Typed --------

    /// <summary>
    /// Creates a typed "updated" result. Defaults code to <see cref="ResultCodes.Updated"/>.
    /// </summary>
    public static Result<T> Updated<T>(ResultCode? code = null)
        => new(Status.Completed, code: code ?? ResultCodes.Updated);

    /// <summary>
    /// Creates a typed "updated" result with a value. Defaults code to <see cref="ResultCodes.Updated"/>.
    /// </summary>
    public static Result<T> Updated<T>(T value, ResultCode? code = null)
        => new(Status.Completed, value, code: code ?? ResultCodes.Updated);

    /// <summary>
    /// Creates a typed "updated" result with value + messages. Defaults code to <see cref="ResultCodes.Updated"/>.
    /// </summary>
    public static Result<T> Updated<T>(
        T value,
        IReadOnlyList<Message> messages,
        ResultCode? code = null)
        => new(Status.Completed, value, code: code ?? ResultCodes.Updated, messages: messages);

    /// <summary>
    /// Creates a typed "updated" result with value + single message. Defaults code to <see cref="ResultCodes.Updated"/>.
    /// </summary>
    public static Result<T> Updated<T>(
        T value,
        string message,
        MessageType type = MessageType.Information,
        IReadOnlyDictionary<string, object?>? metadata = null,
        ResultCode? code = null) =>
        new(Status.Completed, value, code: code ?? ResultCodes.Updated,
            messages: new List<Message> { new(type, message, metadata) });
}
