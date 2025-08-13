using Knight.Response.Core;
using Knight.Response.Factories;
using Knight.Response.Models;

namespace Knight.Response.Extensions;

/// <summary>
/// Advanced extension methods for validation, side effects, recovery,
/// and message composition on <see cref="Result"/> instances.
/// </summary>
public static class AdvancedResultExtensions
{
    /// <summary>
    /// Ensures that a successful result also satisfies a predicate.
    /// If the predicate is false, it returns a failed result with the provided message.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The original result to validate.</param>
    /// <param name="predicate">The condition that must hold for the value.</param>
    /// <param name="errorMessage">The message to include if the predicate fails.</param>
    /// <returns>
    /// The original <see cref="Result{T}"/> if successful and the predicate evaluates to <c>true</c>;
    /// otherwise, a failed <see cref="Result{T}"/> with the specified error message.
    /// </returns>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T?, bool> predicate, string errorMessage)
    {
        if (!result.IsSuccess)
        {
            return result;
        }

        return predicate(result.Value) ? result : Results.Failure<T>(errorMessage);
    }

    /// <summary>
    /// Executes an action for side effects when the result is successful, without altering the result.
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T?> action)
    {
        if (result.IsSuccess)
        {
            action(result.Value);
        }

        return result;
    }

    /// <summary>
    /// Converts a failed result into a successful one by providing a fallback value.
    /// If already successful, the original result is returned unchanged.
    /// </summary>
    public static Result<T> Recover<T>(this Result<T> result, Func<IReadOnlyList<Message>, T> recovery)
    {
        if (result.IsSuccess)
        {
            return result;
        }

        var fallback = recovery(result.Messages);
        return Results.Success(fallback);
    }

    /// <summary>
    /// Returns a new result with <paramref name="additional"/> messages appended to the current messages.
    /// </summary>
    public static Result WithMessages(this Result result, IEnumerable<Message> additional)
    {
        var combined = result.Messages.Concat(additional).ToList();
        return new(result.Status, combined);
    }

    /// <summary>
    /// Returns a new typed result with <paramref name="additional"/> messages appended to the current messages.
    /// </summary>
    public static Result<T> WithMessages<T>(this Result<T> result, IEnumerable<Message> additional)
    {
        var combined = result.Messages.Concat(additional).ToList();
        return new(result.Status, result.Value, combined);
    }

    /// <summary>
    /// Convenience overload to append a single message.
    /// </summary>
    public static Result WithMessage(this Result result, Message message)
        => result.WithMessages(new[] { message });

    /// <summary>
    /// Convenience overload to append a single message.
    /// </summary>
    public static Result<T> WithMessage<T>(this Result<T> result, Message message)
        => result.WithMessages(new[] { message });

    /// <summary>
    /// Convenience overload to append multiple messages via <c>params</c>.
    /// </summary>
    public static Result WithMessages(this Result result, params Message[] messages)
        => result.WithMessages((IEnumerable<Message>)messages);

    /// <summary>
    /// Convenience overload to append multiple messages via <c>params</c>.
    /// </summary>
    public static Result<T> WithMessages<T>(this Result<T> result, params Message[] messages)
        => result.WithMessages((IEnumerable<Message>)messages);
}
