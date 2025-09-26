using Knight.Response.Core;
using Knight.Response.Factories;
using Knight.Response.Models;

namespace Knight.Response.Extensions;

/// <summary>
/// Composable, immutable helpers for <see cref="Result"/> and <see cref="Result&lt;T&gt;"/>:
/// predicates, observation hooks, functional composition, validation, code &amp; message helpers,
/// and ergonomic value accessors. All methods are pure (they return new instances).
/// </summary>
public static class ResultExtensions
{
    /// <summary>Returns <c>true</c> when <paramref name="result"/> completed successfully.</summary>
    public static bool IsSuccess(this Result result) => result.Status == Status.Completed;

    /// <summary>Returns <c>true</c> when <paramref name="result"/> represents a business failure.</summary>
    public static bool IsFailure(this Result result) => result.Status == Status.Failed;

    /// <summary>Returns <c>true</c> when <paramref name="result"/> represents an unexpected error.</summary>
    public static bool IsError(this Result result) => result.Status == Status.Error;

    /// <summary>Returns <c>true</c> when <paramref name="result"/> was cancelled.</summary>
    public static bool IsCancelled(this Result result) => result.Status == Status.Cancelled;

    /// <summary>
    /// Determines whether the result is not successful
    /// (i.e. <see cref="Status.Failed"/>, <see cref="Status.Error"/>, or <see cref="Status.Cancelled"/>).
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns><c>true</c> if the result is not successful; otherwise <c>false</c>.</returns>
    public static bool IsUnsuccessful(this Result result) => !result.IsSuccess();

    /// <summary>
    /// Returns <c>true</c> if the result is either unsuccessful
    /// (i.e., <see cref="Result.Status"/> is not <see cref="Status.Completed"/>)
    /// or if it completed successfully but its <see cref="Result{T}.Value"/> is <c>null</c>.
    /// </summary>
    /// <typeparam name="T">The type of the value carried by the result.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <returns>
    /// <c>true</c> if the result is unsuccessful or the value is <c>null</c>;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This helper is useful when your logic requires a non-null value
    /// for successful results, allowing a single check instead of
    /// combining <see cref="ResultExtensions.IsUnsuccessful"/> and a null test.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = GetUser();
    /// if (result.IsUnsuccessfulOrNull())
    /// {
    ///     return Results.Failure("Unable to retrieve user");
    /// }
    ///
    /// // safe to use result.Value
    /// Console.WriteLine(result.Value!.Name);
    /// </code>
    /// </example>
    public static bool IsUnsuccessfulOrNull<T>(this Result<T> result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        return result.IsUnsuccessful() || result.ValueIsNull();
    }

    /// <summary>
    /// Executes <paramref name="action"/> if <paramref name="result"/> is successful.
    /// Returns the original <paramref name="result"/> for chaining.
    /// </summary>
    public static Result OnSuccess(this Result result, Action action)
    {
        if (result.IsSuccess())
        {
            action();
        }

        return result;
    }

    /// <summary>
    /// Executes <paramref name="action"/> if <paramref name="result"/> is successful.
    /// Returns the original <paramref name="result"/> for chaining.
    /// </summary>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T?> action)
    {
        if (result.IsSuccess())
        {
            action(result.Value);
        }

        return result;
    }

    /// <summary>
    /// Executes <paramref name="action"/> if <paramref name="result"/> is not successful.
    /// Returns the original <paramref name="result"/> for chaining.
    /// </summary>
    public static Result OnFailure(this Result result, Action<IReadOnlyList<Message>> action)
    {
        if (!result.IsSuccess())
        {
            action(result.Messages);
        }

        return result;
    }

    /// <summary>
    /// Transforms the value of a successful result using <paramref name="mapper"/>.
    /// On failure, propagates messages as a failed result of <typeparamref name="TU"/>.
    /// </summary>
    public static Result<TU> Map<T, TU>(this Result<T> result, Func<T?, TU> mapper)
    {
        if (!result.IsSuccess())
        {
            return Results.Failure<TU>(result.Messages, result.Code);
        }

        return Results.Success(mapper(result.Value),  result.Code);
    }

    /// <summary>
    /// Chains another result-producing operation when the current result is successful.
    /// On failure, propagates the existing messages.
    /// </summary>
    public static Result<TU> Bind<T, TU>(this Result<T> result, Func<T?, Result<TU>> binder)
        => result.IsSuccess() ? binder(result.Value) : Results.Failure<TU>(result.Messages,  result.Code);

    /// <summary>
    /// Ensures that a successful result also satisfies <paramref name="predicate"/>.
    /// If not, returns <see cref="Results.Failure{T}(string, MessageType, System.Collections.Generic.IReadOnlyDictionary{string, object})"/>.
    /// </summary>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T?, bool> predicate, string errorMessage)
    {
        if (!result.IsSuccess())
        {
            return result;
        }

        return predicate(result.Value) ? result : Results.Failure<T>(errorMessage);
    }

    /// <summary>
    /// Executes <paramref name="action"/> for side-effects when the result is successful; returns the original result.
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T?> action)
    {
        if (result.IsSuccess())
        {
            action(result.Value);
        }

        return result;
    }

    /// <summary>
    /// Converts a failed result into a successful one by providing a fallback value via <paramref name="recovery"/>.
    /// If already successful, the original result is returned unchanged.
    /// </summary>
    public static Result<T> Recover<T>(this Result<T> result, Func<IReadOnlyList<Message>, T> recovery)
    {
        if (result.IsSuccess())
        {
            return result;
        }

        var fallback = recovery(result.Messages);
        return Results.Success(fallback);
    }
}
