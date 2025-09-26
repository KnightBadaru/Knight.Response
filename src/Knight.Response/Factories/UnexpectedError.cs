using Knight.Response.Core;
using Knight.Response.Extensions;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates an UnexpectedError error result (defaults to <see cref="ResultCodes.UnexpectedError"/>).
    /// </summary>
    public static Result UnexpectedError(string reason = "Unexpected error.", ResultCode? code = null) =>
        Error(reason).WithCode(code ?? ResultCodes.UnexpectedError);

    /// <summary>
    /// Creates an UnexpectedError error result with messages (defaults to <see cref="ResultCodes.UnexpectedError"/>).
    /// </summary>
    public static Result UnexpectedError(IReadOnlyList<Message> messages, ResultCode? code = null) =>
        Error(messages).WithCode(code ?? ResultCodes.UnexpectedError);

    /// <summary>
    /// Creates an UnexpectedError error result with reason (defaults to <see cref="ResultCodes.UnexpectedError"/>).
    /// </summary>
    public static Result<T> UnexpectedError<T>(string reason = "Unexpected error.", ResultCode? code = null) =>
        Error<T>(reason).WithCode(code ?? ResultCodes.UnexpectedError);

    /// <summary>
    /// Creates an UnexpectedError error result with messages (defaults to <see cref="ResultCodes.UnexpectedError"/>).
    /// </summary>
    public static Result<T> UnexpectedError<T>(IReadOnlyList<Message> messages, ResultCode? code = null) =>
        Error<T>(messages).WithCode(code ?? ResultCodes.UnexpectedError);
}
