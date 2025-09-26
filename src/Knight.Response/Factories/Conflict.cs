using Knight.Response.Core;
using Knight.Response.Extensions;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates a ConcurrencyConflict failure result (defaults to <see cref="ResultCodes.ConcurrencyConflict"/>).
    /// </summary>
    public static Result ConcurrencyConflict(string message = "Concurrency conflict.", ResultCode? code = null) =>
        Failure(message).WithCode(code ?? ResultCodes.ConcurrencyConflict);

    /// <summary>
    /// Creates a ConcurrencyConflict failure result with messages (defaults to <see cref="ResultCodes.ConcurrencyConflict"/>).
    /// </summary>
    public static Result ConcurrencyConflict(IReadOnlyList<Message> messages, ResultCode? code = null) =>
        Failure(messages).WithCode(code ?? ResultCodes.ConcurrencyConflict);

    /// <summary>
    /// Creates a ConcurrencyConflict failure result with messages (defaults to <see cref="ResultCodes.ConcurrencyConflict"/>).
    /// </summary>
    public static Result<T> ConcurrencyConflict<T>(IReadOnlyList<Message> messages, ResultCode? code = null) =>
        Failure<T>(messages).WithCode(code ?? ResultCodes.ConcurrencyConflict);
}
