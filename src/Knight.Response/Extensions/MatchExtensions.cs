using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Extensions;

/// <summary>
/// Composable, immutable helpers for <see cref="Result"/> and <see cref="Result&lt;T&gt;"/>:
/// predicates, observation hooks, functional composition, validation, code &amp; message helpers,
/// and ergonomic value accessors. All methods are pure (they return new instances).
/// </summary>
public static class MatchExtensions
{
    /// <summary>
    /// Branches on (a) unsuccessful, (b) success with <c>null</c> value, or (c) success with value,
    /// and returns a value of <typeparamref name="TR"/>. The unsuccessful branch receives the result messages.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <typeparam name="TR">The return type.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <param name="onUnsuccessful">Invoked when <paramref name="result"/> is not successful. Receives the messages.</param>
    /// <param name="onNoValue">Invoked when successful but <c>Value</c> is <c>null</c>.</param>
    /// <param name="onValue">Invoked when successful and <c>Value</c> is non-<c>null</c>.</param>
    /// <returns>The value produced by the executed branch.</returns>
    public static TR Match<T, TR>(
        this Result<T> result,
        Func<IReadOnlyList<Message>, TR> onUnsuccessful,
        Func<TR> onNoValue,
        Func<T, TR> onValue)
    {
        if (result.IsUnsuccessful())
        {
            return onUnsuccessful(result.Messages);
        }

        if (result.Value is null)
        {
            return onNoValue();
        }

        return onValue(result.Value);
    }


    // (Result<T>) — WITH messages (actions)

    /// <summary>
    /// Action-based match for typed results. Branches on unsuccessful / no value / value.
    /// The unsuccessful branch receives the result messages.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <param name="onUnsuccessful">Invoked when the result is not successful. Receives the messages.</param>
    /// <param name="onNoValue">Invoked when successful but <c>Value</c> is <c>null</c>.</param>
    /// <param name="onValue">Invoked when successful and <c>Value</c> is non-<c>null</c>.</param>
    public static void Match<T>(
        this Result<T> result,
        Action<IReadOnlyList<Message>> onUnsuccessful,
        Action onNoValue,
        Action<T> onValue)
    {
        if (result.IsUnsuccessful())
        {
            onUnsuccessful(result.Messages);
            return;
        }

        if (result.Value is null)
        {
            onNoValue();
            return;
        }

        onValue(result.Value);
    }

    // (Result<T>) — NO messages (returns R)

    /// <summary>
    /// Match without handing messages to the caller:
    /// (a) unsuccessful, (b) success with <c>null</c> value, (c) success with value.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <typeparam name="TR">The return type.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <param name="onUnsuccessful">Invoked when the result is not successful.</param>
    /// <param name="onNoValue">Invoked when successful but <c>Value</c> is <c>null</c>.</param>
    /// <param name="onValue">Invoked when successful and <c>Value</c> is non-<c>null</c>.</param>
    /// <returns>The value produced by the executed branch.</returns>
    public static TR Match<T, TR>(
        this Result<T> result,
        Func<TR> onUnsuccessful,
        Func<TR> onNoValue,
        Func<T, TR> onValue)
    {
        if (result.IsUnsuccessful())
        {
            return onUnsuccessful();
        }

        if (result.Value is null)
        {
            return onNoValue();
        }

        return onValue(result.Value);
    }

    // (Result<T>) — NO messages (actions)

    /// <summary>
    /// Action-based match for typed results without messages.
    /// Branches on unsuccessful / no value / value.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <param name="onUnsuccessful">Invoked when the result is not successful.</param>
    /// <param name="onNoValue">Invoked when successful but <c>Value</c> is <c>null</c>.</param>
    /// <param name="onValue">Invoked when successful and <c>Value</c> is non-<c>null</c>.</param>
    public static void Match<T>(
        this Result<T> result,
        Action onUnsuccessful,
        Action onNoValue,
        Action<T> onValue)
    {
        if (result.IsUnsuccessful())
        {
            onUnsuccessful();
            return;
        }

        if (result.Value is null)
        {
            onNoValue();
            return;
        }

        onValue(result.Value);
    }

    // (Result) — WITH messages (returns R)

    /// <summary>
    /// Match for untyped <see cref="Result"/>: unsuccessful vs success.
    /// The unsuccessful branch receives the result messages.
    /// </summary>
    /// <typeparam name="TR">The return type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="onUnsuccessful">Invoked when the result is not successful. Receives the messages.</param>
    /// <param name="onSuccess">Invoked when the result is successful.</param>
    /// <returns>The value produced by the executed branch.</returns>
    public static TR Match<TR>(
        this Result result,
        Func<IReadOnlyList<Message>, TR> onUnsuccessful,
        Func<TR> onSuccess)
    {
        return result.IsUnsuccessful()
            ? onUnsuccessful(result.Messages)
            : onSuccess();
    }

    // (Result) — WITH messages (actions)

    /// <summary>
    /// Action-based match for untyped <see cref="Result"/>: unsuccessful vs success.
    /// The unsuccessful branch receives the result messages.
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <param name="onUnsuccessful">Invoked when the result is not successful. Receives the messages.</param>
    /// <param name="onSuccess">Invoked when the result is successful.</param>
    public static void Match(
        this Result result,
        Action<IReadOnlyList<Message>> onUnsuccessful,
        Action onSuccess)
    {
        if (result.IsUnsuccessful())
        {
            onUnsuccessful(result.Messages);
        }
        else
        {
            onSuccess();
        }
    }

    // (Result) — NO messages (returns R)

    /// <summary>
    /// Match for untyped <see cref="Result"/> without messages: unsuccessful vs success.
    /// </summary>
    /// <typeparam name="TR">The return type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="onUnsuccessful">Invoked when the result is not successful.</param>
    /// <param name="onSuccess">Invoked when the result is successful.</param>
    /// <returns>The value produced by the executed branch.</returns>
    public static TR Match<TR>(
        this Result result,
        Func<TR> onUnsuccessful,
        Func<TR> onSuccess)
    {
        return result.IsUnsuccessful() ? onUnsuccessful() : onSuccess();
    }

    // (Result) — NO messages (actions)

    /// <summary>
    /// Action-based match for untyped <see cref="Result"/> without messages: unsuccessful vs success.
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <param name="onUnsuccessful">Invoked when the result is not successful.</param>
    /// <param name="onSuccess">Invoked when the result is successful.</param>
    public static void Match(
        this Result result,
        Action onUnsuccessful,
        Action onSuccess)
    {
        if (result.IsUnsuccessful())
        {
            onUnsuccessful();
        }
        else
        {
            onSuccess();
        }
    }

    // Result<T> -> Result<U> -------------------------

    /// <summary>
    /// Branches into a new <see cref="Result{U}"/> based on the current <see cref="Result{T}"/> state.
    /// Unsuccessful → <paramref name="onUnsuccessful"/>; successful with null value → <paramref name="onNoValue"/>;
    /// successful with non-null value → <paramref name="onValue"/>.
    /// This is a convenience wrapper over <see cref="Match{T,R}(Result{T}, Func{IReadOnlyList{Message}, R}, Func{R}, Func{T, R})"/>
    /// where <c>R</c> is <see cref="Result{U}"/>.
    /// </summary>
    /// <typeparam name="T">The input result's value type.</typeparam>
    /// <typeparam name="TU">The output result's value type.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <param name="onUnsuccessful">Invoked when the result is not successful. Receives the messages.</param>
    /// <param name="onNoValue">Invoked when successful but <c>Value</c> is <c>null</c>.</param>
    /// <param name="onValue">Invoked when successful and <c>Value</c> is non-<c>null</c>.</param>
    /// <returns>A new <see cref="Result{U}"/> produced by the executed branch.</returns>
    public static Result<TU> MatchValue<T, TU>(
        this Result<T> result,
        Func<IReadOnlyList<Message>, Result<TU>> onUnsuccessful,
        Func<Result<TU>> onNoValue,
        Func<T, Result<TU>> onValue)
        => result.Match(onUnsuccessful, onNoValue, onValue);

    /// <summary>
    /// Same as <see cref="MatchValue{T, U}(Result{T}, Func{IReadOnlyList{Message}, Result{U}}, Func{Result{U}}, Func{T, Result{U}})"/>
    /// but does not pass messages to the unsuccessful branch.
    /// </summary>
    /// <typeparam name="T">The input result's value type.</typeparam>
    /// <typeparam name="TU">The output result's value type.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <param name="onUnsuccessful">Invoked when the result is not successful.</param>
    /// <param name="onNoValue">Invoked when successful but <c>Value</c> is <c>null</c>.</param>
    /// <param name="onValue">Invoked when successful and <c>Value</c> is non-<c>null</c>.</param>
    /// <returns>A new <see cref="Result{U}"/> produced by the executed branch.</returns>
    public static Result<TU> MatchValue<T, TU>(
        this Result<T> result,
        Func<Result<TU>> onUnsuccessful,
        Func<Result<TU>> onNoValue,
        Func<T, Result<TU>> onValue)
        => result.Match(onUnsuccessful, onNoValue, onValue);

    // Result<T> -> Result (untyped) -------------------------

    /// <summary>
    /// Branches into a new untyped <see cref="Result"/> based on the current <see cref="Result{T}"/> state.
    /// Unsuccessful → <paramref name="onUnsuccessful"/>; successful with null value → <paramref name="onNoValue"/>;
    /// successful with non-null value → <paramref name="onValue"/>.
    /// </summary>
    /// <typeparam name="T">The input result's value type.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <param name="onUnsuccessful">Invoked when the result is not successful. Receives the messages.</param>
    /// <param name="onNoValue">Invoked when successful but <c>Value</c> is <c>null</c>.</param>
    /// <param name="onValue">Invoked when successful and <c>Value</c> is non-<c>null</c>.</param>
    /// <returns>A new untyped <see cref="Result"/> produced by the executed branch.</returns>
    public static Result MatchValue<T>(
        this Result<T> result,
        Func<IReadOnlyList<Message>, Result> onUnsuccessful,
        Func<Result> onNoValue,
        Func<T, Result> onValue)
        => result.Match(onUnsuccessful, onNoValue, onValue);

    /// <summary>
    /// Same as <see cref="MatchValue{T}(Result{T}, Func{IReadOnlyList{Message}, Result}, Func{Result}, Func{T, Result})"/>
    /// but does not pass messages to the unsuccessful branch.
    /// </summary>
    /// <typeparam name="T">The input result's value type.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <param name="onUnsuccessful">Invoked when the result is not successful.</param>
    /// <param name="onNoValue">Invoked when successful but <c>Value</c> is <c>null</c>.</param>
    /// <param name="onValue">Invoked when successful and <c>Value</c> is non-<c>null</c>.</param>
    /// <returns>A new untyped <see cref="Result"/> produced by the executed branch.</returns>
    public static Result MatchValue<T>(
        this Result<T> result,
        Func<Result> onUnsuccessful,
        Func<Result> onNoValue,
        Func<T, Result> onValue)
        => result.Match(onUnsuccessful, onNoValue, onValue);

    // Result (untyped) -> Result<U> / Result -------------------------

    /// <summary>
    /// Branches into a new <see cref="Result{U}"/> based on the current untyped <see cref="Result"/>:
    /// unsuccessful → <paramref name="onUnsuccessful"/>; success → <paramref name="onSuccess"/>.
    /// </summary>
    /// <typeparam name="TU">The output result's value type.</typeparam>
    /// <param name="result">The source untyped result.</param>
    /// <param name="onUnsuccessful">Invoked when the result is not successful. Receives the messages.</param>
    /// <param name="onSuccess">Invoked when the result is successful.</param>
    /// <returns>A new <see cref="Result{U}"/> produced by the executed branch.</returns>
    public static Result<TU> MatchValue<TU>(
        this Result result,
        Func<IReadOnlyList<Message>, Result<TU>> onUnsuccessful,
        Func<Result<TU>> onSuccess)
        => result.Match(onUnsuccessful, onSuccess);

    /// <summary>
    /// Branches into a new untyped <see cref="Result"/> based on the current untyped <see cref="Result"/>:
    /// unsuccessful → <paramref name="onUnsuccessful"/>; success → <paramref name="onSuccess"/>.
    /// </summary>
    /// <param name="result">The source untyped result.</param>
    /// <param name="onUnsuccessful">Invoked when the result is not successful. Receives the messages.</param>
    /// <param name="onSuccess">Invoked when the result is successful.</param>
    /// <returns>A new untyped <see cref="Result"/> produced by the executed branch.</returns>
    public static Result MatchValue(
        this Result result,
        Func<IReadOnlyList<Message>, Result> onUnsuccessful,
        Func<Result> onSuccess)
        => result.Match(onUnsuccessful, onSuccess);

    /// <summary>
    /// Same as <see cref="MatchValue{U}(Result, Func{IReadOnlyList{Message}, Result{U}}, Func{Result{U}})"/>
    /// but does not pass messages to the unsuccessful branch.
    /// </summary>
    /// <typeparam name="TU">The output result's value type.</typeparam>
    /// <param name="result">The source untyped result.</param>
    /// <param name="onUnsuccessful">Invoked when the result is not successful.</param>
    /// <param name="onSuccess">Invoked when the result is successful.</param>
    /// <returns>A new <see cref="Result{U}"/> produced by the executed branch.</returns>
    public static Result<TU> MatchValue<TU>(
        this Result result,
        Func<Result<TU>> onUnsuccessful,
        Func<Result<TU>> onSuccess)
        => result.Match(onUnsuccessful, onSuccess);

    /// <summary>
    /// Same as <see cref="MatchValue(Result, Func{IReadOnlyList{Message}, Result}, Func{Result})"/>
    /// but does not pass messages to the unsuccessful branch.
    /// </summary>
    /// <param name="result">The source untyped result.</param>
    /// <param name="onUnsuccessful">Invoked when the result is not successful.</param>
    /// <param name="onSuccess">Invoked when the result is successful.</param>
    /// <returns>A new untyped <see cref="Result"/> produced by the executed branch.</returns>
    public static Result MatchValue(
        this Result result,
        Func<Result> onUnsuccessful,
        Func<Result> onSuccess)
        => result.Match(onUnsuccessful, onSuccess);
}
