using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates a "not found" result. Defaults to <see cref="Status.Completed"/> with
    /// a <see cref="MessageType.Warning"/> and <see cref="ResultCodes.NotFound"/>.
    /// </summary>
    public static Result NotFound(
        string message = "Resource not found.",
        ResultCode? code = null,
        Status status = Status.Completed)
        => new(status,
            code: code ?? ResultCodes.NotFound,
            messages: new List<Message> { new(MessageType.Warning, message) });

    /// <summary>
    /// "Not found" with optional domain <see cref="ResultCode"/> (defaults to <see cref="ResultCodes.NotFound"/>).
    /// </summary>
    public static Result NotFound(ResultCode? code) =>
        new(Status.Completed, code: code ?? ResultCodes.NotFound,
            messages: new List<Message> { new(MessageType.Warning, "Resource not found.") });

    /// <summary>
    /// "Not found" using provided messages (defaults code to <see cref="ResultCodes.NotFound"/>).
    /// </summary>
    public static Result NotFound(
        IReadOnlyList<Message> messages,
        ResultCode? code = null,
        Status status = Status.Completed)
        => new(status, code: code ?? ResultCodes.NotFound, messages: messages);

    /// <summary>
    /// "Not found" single-message overload with type/metadata (defaults code to <see cref="ResultCodes.NotFound"/>).
    /// </summary>
    public static Result NotFound(
        string message,
        MessageType type,
        IReadOnlyDictionary<string, object?>? metadata,
        ResultCode? code = null,
        Status status = Status.Completed)
        => new(status, code: code ?? ResultCodes.NotFound,
            messages: new List<Message> { new(type, message, metadata) });


    /// <summary>
    /// Creates a "not found" result with an optional domain <see cref="ResultCode"/> (defaults to <see cref="ResultCodes.NotFound"/>).
    /// </summary>
    public static Result NotFound(
        ResultCode? code,
        string message,
        Status status = Status.Completed)
        => new(status,
            code: code ?? ResultCodes.NotFound,
            messages: new List<Message> { new(MessageType.Warning, message) });

    // -------- Typed --------

    /// <summary>
    /// Creates a typed "not found" result. Defaults to <see cref="Status.Completed"/> with
    /// a <see cref="MessageType.Warning"/> and <see cref="ResultCodes.NotFound"/>.
    /// </summary>
    public static Result<T> NotFound<T>(
        string message = "Resource not found.",
        ResultCode? code = null,
        Status status = Status.Completed)
        => new(status,
            code: code ?? ResultCodes.NotFound,
            messages: new List<Message> { new(MessageType.Warning, message) });

    /// <summary>
    /// Typed "not found" with optional code (defaults to <see cref="ResultCodes.NotFound"/>).
    /// </summary>
    public static Result<T> NotFound<T>(ResultCode? code)
        => new(Status.Completed, code: code ?? ResultCodes.NotFound,
            messages: new List<Message> { new(MessageType.Warning, "Resource not found.") });

    /// <summary>
    /// Typed "not found" using provided messages (defaults code to <see cref="ResultCodes.NotFound"/>).
    /// </summary>
    public static Result<T> NotFound<T>(
        IReadOnlyList<Message> messages,
        ResultCode? code = null,
        Status status = Status.Completed)
        => new(status, code: code ?? ResultCodes.NotFound, messages: messages);

    /// <summary>
    /// Typed "not found" single-message overload with type/metadata (defaults code to <see cref="ResultCodes.NotFound"/>).
    /// </summary>
    public static Result<T> NotFound<T>(
        string message,
        MessageType type,
        IReadOnlyDictionary<string, object?>? metadata,
        ResultCode? code = null,
        Status status = Status.Completed)
        => new(status, code: code ?? ResultCodes.NotFound,
            messages: new List<Message> { new(type, message, metadata) });

    /// <summary>
    /// Creates a typed "not found" result with an optional domain <see cref="ResultCode"/> (defaults to <see cref="ResultCodes.NotFound"/>).
    /// </summary>
    public static Result<T> NotFound<T>(
        ResultCode? code,
        string message,
        Status status = Status.Completed)
        => new(status,
            code: code ?? ResultCodes.NotFound,
            messages: new List<Message> { new(MessageType.Warning, message) });
}
