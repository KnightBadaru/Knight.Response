using Knight.Response.Core;
using Knight.Response.Factories;
using Knight.Response.Models;

namespace Knight.Response.Extensions;

/// <summary>
/// Core extension methods for composing and observing <see cref="Result"/> pipelines.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Executes <paramref name="action"/> if <paramref name="result"/> is successful.
    /// Returns the original <paramref name="result"/> for chaining.
    /// </summary>
    public static Result OnSuccess(this Result result, Action action)
    {
        if (result.IsSuccess)
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
        if (result.IsSuccess)
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
        if (!result.IsSuccess)
        {
            action(result.Messages);
        }

        return result;
    }

    /// <summary>
    /// Executes <paramref name="action"/> if <paramref name="result"/> is not successful.
    /// Returns the original <paramref name="result"/> for chaining.
    /// </summary>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<IReadOnlyList<Message>> action)
    {
        if (!result.IsSuccess)
        {
            action(result.Messages);
        }

        return result;
    }

    /// <summary>
    /// Transforms the value of a successful <see cref="Result{T}"/> into another value.
    /// On failure, propagates the existing messages as a failed <see cref="Result{TU}"/>.
    /// </summary>
    public static Result<TU> Map<T, TU>(this Result<T> result, Func<T?, TU> mapper)
    {
        if (!result.IsSuccess)
        {
            return Results.Failure<TU>(result.Messages);
        }

        var newValue = mapper(result.Value);
        return Results.Success(newValue);

    }

    /// <summary>
    /// Chains another result-producing operation when the current result is successful.
    /// On failure, propagates the existing messages.
    /// </summary>
    public static Result<TU> Bind<T, TU>(this Result<T> result, Func<T?, Result<TU>> binder)
        => result.IsSuccess ? binder(result.Value) : Results.Failure<TU>(result.Messages);
}
