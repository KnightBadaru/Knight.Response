using Knight.Response.Core;
using Knight.Response.Extensions;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates a NotSupported failure result (defaults to <see cref="ResultCodes.NotSupported"/>).
    /// </summary>
    public static Result NotSupported(string reason = "Operation not supported.", ResultCode? code = null) =>
        Failure(reason).WithCode(code ?? ResultCodes.NotSupported);

    /// <summary>
    /// Creates a NotSupported failure result with messages (defaults to <see cref="ResultCodes.NotSupported"/>).
    /// </summary>
    public static Result NotSupported(IReadOnlyList<Message> messages, ResultCode? code = null) =>
        Failure(messages).WithCode(code ?? ResultCodes.NotSupported);

    /// <summary>
    /// Creates a NotSupported failure result with reason (defaults to <see cref="ResultCodes.NotSupported"/>).
    /// </summary>
    public static Result<T> NotSupported<T>(string reason = "Operation not supported.", ResultCode? code = null) =>
        Failure<T>(reason).WithCode(code ?? ResultCodes.NotSupported);

    /// <summary>
    /// Creates a NotSupported failure result with messages (defaults to <see cref="ResultCodes.NotSupported"/>).
    /// </summary>
    public static Result<T> NotSupported<T>(IReadOnlyList<Message> messages, ResultCode? code = null) =>
        Failure<T>(messages).WithCode(code ?? ResultCodes.NotSupported);
}
