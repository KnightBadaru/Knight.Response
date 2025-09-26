using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates a result indicating the operation completed successfully
    /// but there is no content to return. Defaults <see cref="Result.Code"/> to <see cref="ResultCodes.NoContent"/>.
    /// </summary>
    public static Result NoContent() =>
        Success(code: ResultCodes.NoContent);

    /// <summary>
    /// Creates a result indicating the operation completed successfully
    /// but there is no content to return, with an optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result NoContent(ResultCode? code) =>
        Success(code: code ?? ResultCodes.NoContent);

    /// <summary>
    /// Creates a result indicating the operation completed successfully
    /// but there is no content to return, with messages. Defaults code to <see cref="ResultCodes.NoContent"/>.
    /// </summary>
    public static Result NoContent(IReadOnlyList<Message> messages) =>
        Success(messages: messages, code: ResultCodes.NoContent);

    /// <summary>
    /// Creates a result indicating the operation completed successfully
    /// but there is no content to return, with messages and an optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result NoContent(IReadOnlyList<Message> messages, ResultCode? code) =>
        Success(messages: messages, code: code ?? ResultCodes.NoContent);

    /// <summary>
    /// Creates a result indicating the operation completed successfully
    /// but there is no content to return, with a single message.
    /// </summary>
    public static Result NoContent(
        string message,
        MessageType type = MessageType.Information,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => Success(code: ResultCodes.NoContent,
                messages: new List<Message> { new(type, message, metadata) });

    /// <summary>
    /// Creates a result indicating the operation completed successfully
    /// but there is no content to return, with a single message and an optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result NoContent(
        string message,
        ResultCode? code,
        MessageType type = MessageType.Information,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => Success(code: code ?? ResultCodes.NoContent,
                messages: new List<Message> { new(type, message, metadata) });


    // -------- Typed --------

    /// <summary>
    /// Creates a typed result indicating the operation completed successfully
    /// but there is no content to return. Defaults code to <see cref="ResultCodes.NoContent"/>.
    /// </summary>
    public static Result<T> NoContent<T>() => Success<T>(code: ResultCodes.NoContent);

    /// <summary>
    /// Creates a typed result indicating the operation completed successfully
    /// but there is no content to return, with an optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result<T> NoContent<T>(ResultCode? code) =>
        Success<T>(code: code ?? ResultCodes.NoContent);

    /// <summary>
    /// Creates a typed result indicating the operation completed successfully
    /// but there is no content to return, with messages. Defaults code to <see cref="ResultCodes.NoContent"/>.
    /// </summary>
    public static Result<T> NoContent<T>(IReadOnlyList<Message> messages) =>
        Success<T>(messages: messages, code: ResultCodes.NoContent);

    /// <summary>
    /// Creates a typed result indicating the operation completed successfully
    /// but there is no content to return, with messages and an optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result<T> NoContent<T>(IReadOnlyList<Message> messages, ResultCode? code) =>
        Success<T>(messages: messages, code: code ?? ResultCodes.NoContent);

    /// <summary>
    /// Creates a typed result indicating the operation completed successfully
    /// but there is no content to return, with a single message.
    /// </summary>
    public static Result<T> NoContent<T>(
        string message,
        MessageType type = MessageType.Information,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => Success<T>(code: ResultCodes.NoContent,
                   messages: new List<Message> { new(type, message, metadata) });

    /// <summary>
    /// Creates a typed result indicating the operation completed successfully
    /// but there is no content to return, with a single message and an optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result<T> NoContent<T>(
        string message,
        ResultCode? code,
        MessageType type = MessageType.Information,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => Success<T>(code: code ?? ResultCodes.NoContent,
                   messages: new List<Message> { new(type, message, metadata) });
}
