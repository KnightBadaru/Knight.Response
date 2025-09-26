using Knight.Response.Core;
using Knight.Response.Extensions;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates a ServiceUnavailable error result (defaults to <see cref="ResultCodes.ServiceUnavailable"/>).
    /// </summary>
    public static Result ServiceUnavailable(string reason = "Service unavailable.", ResultCode? code = null) =>
        Error(reason).WithCode(code ?? ResultCodes.ServiceUnavailable);

    /// <summary>
    /// Creates a ServiceUnavailable error result with messages (defaults to <see cref="ResultCodes.ServiceUnavailable"/>).
    /// </summary>
    public static Result ServiceUnavailable(IReadOnlyList<Message> messages, ResultCode? code = null) =>
        Error(messages).WithCode(code ?? ResultCodes.ServiceUnavailable);

    /// <summary>
    /// Creates a ServiceUnavailable error result with reason (defaults to <see cref="ResultCodes.ServiceUnavailable"/>).
    /// </summary>
    public static Result<T> ServiceUnavailable<T>(string reason = "Service unavailable.", ResultCode? code = null) =>
        Error<T>(reason).WithCode(code ?? ResultCodes.ServiceUnavailable);

    /// <summary>
    /// Creates a ServiceUnavailable error result with messages (defaults to <see cref="ResultCodes.ServiceUnavailable"/>).
    /// </summary>
    public static Result<T> ServiceUnavailable<T>(IReadOnlyList<Message> messages, ResultCode? code = null) =>
        Error<T>(messages).WithCode(code ?? ResultCodes.ServiceUnavailable);
}
