using Knight.Response.Core;
using Knight.Response.Extensions;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates an Unauthorized failure result (defaults to <see cref="ResultCodes.Unauthorized"/>).
    /// </summary>
    public static Result Unauthorized(string reason = "Unauthorized.", ResultCode? code = null) =>
        Failure(reason).WithCode(code ?? ResultCodes.Unauthorized);

    /// <summary>
    /// Creates an Unauthorized failure result with messages (defaults to <see cref="ResultCodes.Unauthorized"/>).
    /// </summary>
    public static Result Unauthorized(IReadOnlyList<Message> messages, ResultCode? code = null) =>
        Failure(messages).WithCode(code ?? ResultCodes.Unauthorized);

    /// <summary>
    /// Creates an Unauthorized failure result with reason (defaults to <see cref="ResultCodes.Unauthorized"/>).
    /// </summary>
    public static Result<T> Unauthorized<T>(string reason = "Unauthorized.", ResultCode? code = null) =>
        Failure<T>(reason).WithCode(code ?? ResultCodes.Unauthorized);

    /// <summary>
    /// Creates an Unauthorized failure result with messages (defaults to <see cref="ResultCodes.Unauthorized"/>).
    /// </summary>
    public static Result<T> Unauthorized<T>(IReadOnlyList<Message> messages, ResultCode? code = null) =>
        Failure<T>(messages).WithCode(code ?? ResultCodes.Unauthorized);
}
