using Knight.Response.Core;
using Knight.Response.Extensions;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates a PreconditionFailed failure result (defaults to <see cref="ResultCodes.PreconditionFailed"/>).
    /// </summary>
    public static Result PreconditionFailed(string reason = "Precondition failed.", ResultCode? code = null) =>
        Failure(reason).WithCode(code ?? ResultCodes.PreconditionFailed);

    /// <summary>
    /// Creates a PreconditionFailed failure result with messages (defaults to <see cref="ResultCodes.PreconditionFailed"/>).
    /// </summary>
    public static Result PreconditionFailed(IReadOnlyList<Message> messages, ResultCode? code = null) =>
        Failure(messages).WithCode(code ?? ResultCodes.PreconditionFailed);

    /// <summary>
    /// Creates a PreconditionFailed failure result with reason (defaults to <see cref="ResultCodes.PreconditionFailed"/>).
    /// </summary>
    public static Result<T> PreconditionFailed<T>(string reason = "Precondition failed.", ResultCode? code = null) =>
        Failure<T>(reason).WithCode(code ?? ResultCodes.PreconditionFailed);

    /// <summary>
    /// Creates a PreconditionFailed failure result with messages (defaults to <see cref="ResultCodes.PreconditionFailed"/>).
    /// </summary>
    public static Result<T> PreconditionFailed<T>(IReadOnlyList<Message> messages, ResultCode? code = null) =>
        Failure<T>(messages).WithCode(code ?? ResultCodes.PreconditionFailed);
}
