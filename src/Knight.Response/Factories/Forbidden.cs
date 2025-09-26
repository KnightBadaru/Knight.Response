using Knight.Response.Core;
using Knight.Response.Extensions;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates a Forbidden failure result (defaults to <see cref="ResultCodes.Forbidden"/>).
    /// </summary>
    public static Result Forbidden(string reason = "Forbidden.", ResultCode? code = null) =>
        Failure(reason).WithCode(code ?? ResultCodes.Forbidden);

    /// <summary>
    /// Creates a Forbidden failure result with messages (defaults to <see cref="ResultCodes.Forbidden"/>).
    /// </summary>
    public static Result Forbidden(IReadOnlyList<Message> messages, ResultCode? code = null) =>
        Failure(messages).WithCode(code ?? ResultCodes.Forbidden);

    /// <summary>
    /// Creates a Forbidden failure result with messages (defaults to <see cref="ResultCodes.Forbidden"/>).
    /// </summary>
    public static Result<T> Forbidden<T>(string reason = "Forbidden", ResultCode? code = null) =>
        Failure<T>(reason).WithCode(code ?? ResultCodes.Forbidden);

    /// <summary>
    /// Creates a Forbidden failure result with messages (defaults to <see cref="ResultCodes.Forbidden"/>).
    /// </summary>
    public static Result<T> Forbidden<T>(IReadOnlyList<Message> messages, ResultCode? code = null) =>
        Failure<T>(messages).WithCode(code ?? ResultCodes.Forbidden);
}
