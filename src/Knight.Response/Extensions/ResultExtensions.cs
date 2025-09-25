using System.ComponentModel.DataAnnotations;
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
    /// <summary>
    /// Collects validation results from a set of messages, supporting both
    /// "ValidationResult" (single) and "ValidationResults" (multiple).
    /// </summary>
    private static IReadOnlyList<ValidationResult> CollectValidationResults(IReadOnlyList<Message> messages)
    {
        var found = new List<ValidationResult>();

        foreach (var msg in messages)
        {
            if (msg.Metadata.Count == 0)
            {
                continue;
            }

            // case-insensitive lookup on IReadOnlyDictionary
            if (TryGetInsensitive(msg.Metadata, "ValidationResult", out var single) && single is ValidationResult vr)
            {
                found.Add(vr);
            }

            if (TryGetInsensitive(msg.Metadata, "ValidationResults", out var many))
            {
                if (many is IEnumerable<ValidationResult> typed)
                {
                    found.AddRange(typed);
                }
                else if (many is IEnumerable<object> objs)
                {
                    found.AddRange(objs.OfType<ValidationResult>());
                }
            }
        }

        return found.Count == 0 ? Array.Empty<ValidationResult>() : found;
    }

    /// <summary>
    /// Case-insensitive key lookup for <see cref="IReadOnlyDictionary{TKey, TValue}"/>.
    /// </summary>
    private static bool TryGetInsensitive(IReadOnlyDictionary<string, object?> dict, string key, out object? value)
    {
        foreach (var kvp in dict)
        {
            if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                value = kvp.Value;
                return true;
            }
        }

        value = null;
        return false;
    }

    // ---------------------------------------------------------------------
    // Predicates
    // ---------------------------------------------------------------------

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
    /// Determines whether the typed result's value is <c>null</c>.
    /// This does not consider <see cref="Status"/>; it only checks the value reference.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The typed result to inspect.</param>
    /// <returns><c>true</c> when <paramref name="result"/>.Value is <c>null</c>; otherwise <c>false</c>.</returns>
    public static bool ValueIsNull<T>(this Result<T> result) => result.Value is null;

    /// <summary>
    /// Returns <c>true</c> when <paramref name="result"/> has a <see cref="ResultCode"/>
    /// equal to <paramref name="code"/> (case-insensitive).
    /// </summary>
    /// <param name="result">The result to inspect.</param>
    /// <param name="code">The code string to match.</param>
    public static bool HasCode(this Result result, string code) =>
        result.Code is { } c && string.Equals(c.Value, code, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns <c>true</c> when <paramref name="result"/> has a <see cref="ResultCode"/>
    /// equal to <paramref name="code"/> (case-insensitive).
    /// </summary>
    /// <param name="result">The result to inspect.</param>
    /// <param name="code">The code to match.</param>
    /// <returns>
    /// <c>true</c> if <see cref="Result.Code"/> is non-null and equals <paramref name="code"/> by value;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Compares <see cref="ResultCode.Value"/> using <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </remarks>
    public static bool HasCode(this Result result, ResultCode code) =>
        result.Code is { } c &&
        string.Equals(c.Value, code.Value, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns <c>true</c> when the typed <paramref name="result"/> has a <see cref="ResultCode"/>
    /// equal to <paramref name="code"/> (case-insensitive).
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The typed result to inspect.</param>
    /// <param name="code">The code string to match.</param>
    /// <returns>
    /// <c>true</c> if <see cref="Result.Code"/> is non-null and equals <paramref name="code"/> by value;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Compares <see cref="ResultCode.Value"/> using <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </remarks>
    public static bool HasCode<T>(this Result<T> result, string code) =>
        result.Code is { } c &&
        string.Equals(c.Value, code, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns <c>true</c> when the typed <paramref name="result"/> has a <see cref="ResultCode"/>
    /// equal to <paramref name="code"/> (case-insensitive).
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The typed result to inspect.</param>
    /// <param name="code">The code to match.</param>
    /// <returns>
    /// <c>true</c> if <see cref="Result.Code"/> is non-null and equals <paramref name="code"/> by value;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Compares <see cref="ResultCode.Value"/> using <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </remarks>
    public static bool HasCode<T>(this Result<T> result, ResultCode code) =>
        result.Code is { } c &&
        string.Equals(c.Value, code.Value, StringComparison.OrdinalIgnoreCase);

    // ---------------------------------------------------------------------
    // Match
    // ---------------------------------------------------------------------

    // (Result<T>) — WITH messages (returns R)

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

    // ---------------------------------------------------------------------
    // Observation hooks
    // ---------------------------------------------------------------------

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

    // ---------------------------------------------------------------------
    // Functional composition
    // ---------------------------------------------------------------------

    /// <summary>
    /// Transforms the value of a successful result using <paramref name="mapper"/>.
    /// On failure, propagates messages as a failed result of <typeparamref name="TU"/>.
    /// </summary>
    public static Result<TU> Map<T, TU>(this Result<T> result, Func<T?, TU> mapper)
    {
        if (!result.IsSuccess())
        {
            return Results.Failure<TU>(result.Messages);
        }

        return Results.Success(mapper(result.Value));
    }

    /// <summary>
    /// Chains another result-producing operation when the current result is successful.
    /// On failure, propagates the existing messages.
    /// </summary>
    public static Result<TU> Bind<T, TU>(this Result<T> result, Func<T?, Result<TU>> binder)
        => result.IsSuccess() ? binder(result.Value) : Results.Failure<TU>(result.Messages);

    // ---------------------------------------------------------------------
    // Validation / side-effects / recovery (Advanced)
    // ---------------------------------------------------------------------

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

    // ---------------------------------------------------------------------
    // Code helpers
    // ---------------------------------------------------------------------

    /// <summary>
    /// Returns a new <see cref="Result"/> with <see cref="Result.Code"/> set
    /// to a new <see cref="ResultCode"/> created from <paramref name="code"/>.
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <param name="code">The raw string code to wrap into a <see cref="ResultCode"/>.</param>
    /// <returns>A new <see cref="Result"/> instance with the updated code.</returns>
    public static Result WithCode(this Result result, string code)
        => new(result.Status, new ResultCode(code), result.Messages);

    /// <summary>
    /// Returns a new <see cref="Result"/> with <see cref="Result.Code"/> set to <paramref name="code"/>.
    /// </summary>
    public static Result WithCode(this Result result, ResultCode code)
        => new(result.Status, code, result.Messages);

    /// <summary>
    /// Returns a new <see cref="Result{T}"/> with <see cref="Result.Code"/> set
    /// to a new <see cref="ResultCode"/> created from <paramref name="code"/>.
    /// </summary>
    /// <typeparam name="T">The value type of the result.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <param name="code">The raw string code to wrap into a <see cref="ResultCode"/>.</param>
    /// <returns>A new <see cref="Result{T}"/> instance with the updated code.</returns>
    public static Result<T> WithCode<T>(this Result<T> result, string code)
        => new(result.Status, result.Value, new ResultCode(code), result.Messages);

    /// <summary>
    /// Returns a new <see cref="Result{T}"/> with <see cref="Result.Code"/> set to <paramref name="code"/>.
    /// </summary>
    public static Result<T> WithCode<T>(this Result<T> result, ResultCode code)
        => new(result.Status, result.Value, code, result.Messages);

    /// <summary>
    /// Returns a new <see cref="Result"/> with <see cref="Result.Code"/> set to <paramref name="code"/>
    /// when <paramref name="condition"/> is <c>true</c>; otherwise returns the original instance.
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <param name="condition">When <c>true</c>, the code is applied.</param>
    /// <param name="code">The domain code to assign.</param>
    /// <returns>The updated or original <see cref="Result"/>.</returns>
    public static Result WithCodeIf(this Result result, bool condition, ResultCode code)
        => condition ? new(result.Status, code, result.Messages) : result;

    /// <summary>
    /// Returns a new <see cref="Result"/> with <see cref="Result.Code"/> set to a new
    /// <see cref="ResultCode"/> created from <paramref name="code"/> when
    /// <paramref name="condition"/> is <c>true</c>; otherwise returns the original instance.
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <param name="condition">When <c>true</c>, the code is applied.</param>
    /// <param name="code">The raw string code to wrap and assign.</param>
    /// <returns>The updated or original <see cref="Result"/>.</returns>
    public static Result WithCodeIf(this Result result, bool condition, string code)
        => condition ? new(result.Status, new ResultCode(code), result.Messages) : result;

    /// <summary>
    /// Returns a new <see cref="Result"/> with <see cref="Result.Code"/> set to <paramref name="code"/>
    /// when <paramref name="predicate"/> evaluates to <c>true</c>; otherwise returns the original instance.
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <param name="predicate">A predicate evaluated against <paramref name="result"/>.</param>
    /// <param name="code">The domain code to assign.</param>
    /// <returns>The updated or original <see cref="Result"/>.</returns>
    public static Result WithCodeIf(this Result result, Func<Result, bool> predicate, ResultCode code)
        => predicate(result) ? new(result.Status, code, result.Messages) : result;

    /// <summary>
    /// Returns a new <see cref="Result"/> with <see cref="Result.Code"/> set to a new
    /// <see cref="ResultCode"/> created from <paramref name="code"/> when
    /// <paramref name="predicate"/> evaluates to <c>true</c>; otherwise returns the original instance.
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <param name="predicate">A predicate evaluated against <paramref name="result"/>.</param>
    /// <param name="code">The raw string code to wrap and assign.</param>
    /// <returns>The updated or original <see cref="Result"/>.</returns>
    public static Result WithCodeIf(this Result result, Func<Result, bool> predicate, string code)
        => predicate(result) ? new(result.Status, new ResultCode(code), result.Messages) : result;

    /// <summary>
    /// Returns a new <see cref="Result{T}"/> with <see cref="Result.Code"/> set to <paramref name="code"/>
    /// when <paramref name="condition"/> is <c>true</c>; otherwise returns the original instance.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <param name="condition">When <c>true</c>, the code is applied.</param>
    /// <param name="code">The domain code to assign.</param>
    /// <returns>The updated or original <see cref="Result{T}"/>.</returns>
    public static Result<T> WithCodeIf<T>(this Result<T> result, bool condition, ResultCode code)
        => condition ? new(result.Status, result.Value, code, result.Messages) : result;

    /// <summary>
    /// Returns a new <see cref="Result{T}"/> with <see cref="Result.Code"/> set to a new
    /// <see cref="ResultCode"/> created from <paramref name="code"/> when
    /// <paramref name="condition"/> is <c>true</c>; otherwise returns the original instance.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <param name="condition">When <c>true</c>, the code is applied.</param>
    /// <param name="code">The raw string code to wrap and assign.</param>
    /// <returns>The updated or original <see cref="Result{T}"/>.</returns>
    public static Result<T> WithCodeIf<T>(this Result<T> result, bool condition, string code)
        => condition ? new(result.Status, result.Value, new ResultCode(code), result.Messages) : result;

    /// <summary>
    /// Returns a new <see cref="Result{T}"/> with <see cref="Result.Code"/> set to <paramref name="code"/>
    /// when <paramref name="predicate"/> evaluates to <c>true</c>; otherwise returns the original instance.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <param name="predicate">A predicate evaluated against <paramref name="result"/>.</param>
    /// <param name="code">The domain code to assign.</param>
    /// <returns>The updated or original <see cref="Result{T}"/>.</returns>
    public static Result<T> WithCodeIf<T>(this Result<T> result, Func<Result<T>, bool> predicate, ResultCode code)
        => predicate(result) ? new(result.Status, result.Value, code, result.Messages) : result;

    /// <summary>
    /// Returns a new <see cref="Result{T}"/> with <see cref="Result.Code"/> set to a new
    /// <see cref="ResultCode"/> created from <paramref name="code"/> when
    /// <paramref name="predicate"/> evaluates to <c>true</c>; otherwise returns the original instance.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <param name="predicate">A predicate evaluated against <paramref name="result"/>.</param>
    /// <param name="code">The raw string code to wrap and assign.</param>
    /// <returns>The updated or original <see cref="Result{T}"/>.</returns>
    public static Result<T> WithCodeIf<T>(this Result<T> result, Func<Result<T>, bool> predicate, string code)
        => predicate(result) ? new(result.Status, result.Value, new ResultCode(code), result.Messages) : result;

    /// <summary>
    /// Returns a new <see cref="Result"/> with <see cref="Result.Code"/> set to <c>null</c>.
    /// Useful when you want to explicitly clear any existing code.
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <returns>A new <see cref="Result"/> with <see cref="Result.Code"/> unset.</returns>
    public static Result WithoutCode(this Result result)
        => new(result.Status, code: null, result.Messages);

    /// <summary>
    /// Returns a new <see cref="Result{T}"/> with <see cref="Result.Code"/> set to <c>null</c>.
    /// Useful when you want to explicitly clear any existing code.
    /// </summary>
    /// <typeparam name="T">The value type of the result.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <returns>A new <see cref="Result{T}"/> with <see cref="Result.Code"/> unset.</returns>
    public static Result<T> WithoutCode<T>(this Result<T> result)
        => new(result.Status, result.Value, code: null, result.Messages);

    /// <summary>
    /// Returns a new <see cref="Result"/> with <see cref="Result.Code"/> cleared (set to <c>null</c>)
    /// when <paramref name="condition"/> is <c>true</c>; otherwise returns the original instance.
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <param name="condition">When <c>true</c>, the code is cleared.</param>
    /// <returns>The updated or original <see cref="Result"/>.</returns>
    public static Result WithoutCodeIf(this Result result, bool condition)
        => condition ? new(result.Status, null, result.Messages) : result;

    /// <summary>
    /// Returns a new <see cref="Result"/> with <see cref="Result.Code"/> cleared (set to <c>null</c>)
    /// when <paramref name="predicate"/> evaluates to <c>true</c>; otherwise returns the original instance.
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <param name="predicate">A predicate evaluated against <paramref name="result"/>.</param>
    /// <returns>The updated or original <see cref="Result"/>.</returns>
    public static Result WithoutCodeIf(this Result result, Func<Result, bool> predicate)
        => predicate(result) ? new(result.Status, null, result.Messages) : result;

    /// <summary>
    /// Returns a new <see cref="Result{T}"/> with <see cref="Result.Code"/> cleared (set to <c>null</c>)
    /// when <paramref name="condition"/> is <c>true</c>; otherwise returns the original instance.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <param name="condition">When <c>true</c>, the code is cleared.</param>
    /// <returns>The updated or original <see cref="Result{T}"/>.</returns>
    public static Result<T> WithoutCodeIf<T>(this Result<T> result, bool condition)
        => condition ? new(result.Status, result.Value, null, result.Messages) : result;

    /// <summary>
    /// Returns a new <see cref="Result{T}"/> with <see cref="Result.Code"/> cleared (set to <c>null</c>)
    /// when <paramref name="predicate"/> evaluates to <c>true</c>; otherwise returns the original instance.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The source typed result.</param>
    /// <param name="predicate">A predicate evaluated against <paramref name="result"/>.</param>
    /// <returns>The updated or original <see cref="Result{T}"/>.</returns>
    public static Result<T> WithoutCodeIf<T>(this Result<T> result, Func<Result<T>, bool> predicate)
        => predicate(result) ? new(result.Status, result.Value, null, result.Messages) : result;

    // ---------------------------------------------------------------------
    // Message helper
    // ---------------------------------------------------------------------

    /// <summary>
    /// Returns a new <see cref="Result"/> with <paramref name="message"/> appended.
    /// </summary>
    public static Result WithMessage(this Result result, Message message)
    {
        var list = new List<Message>(result.Messages) { message };
        return new(result.Status, result.Code, list);
    }

    /// <summary>
    /// Returns a new <see cref="Result{T}"/> with <paramref name="message"/> appended.
    /// </summary>
    public static Result<T> WithMessage<T>(this Result<T> result, Message message)
    {
        var list = new List<Message>(result.Messages) { message };
        return new(result.Status, result.Value, result.Code, list);
    }

    /// <summary>
    /// Returns a new <see cref="Result"/> with <paramref name="additional"/> appended.
    /// </summary>
    public static Result WithMessages(this Result result, IEnumerable<Message> additional)
    {
        var combined = result.Messages.Concat(additional).ToList();
        return new(result.Status, result.Code, combined);
    }

    /// <summary>
    /// Returns a new <see cref="Result{T}"/> with <paramref name="additional"/> appended.
    /// </summary>
    public static Result<T> WithMessages<T>(this Result<T> result, IEnumerable<Message> additional)
    {
        var combined = result.Messages.Concat(additional).ToList();
        return new(result.Status, result.Value, result.Code, combined);
    }

    /// <summary>
    /// Params overload to append multiple messages.
    /// </summary>
    public static Result WithMessages(this Result result, params Message[] messages)
        => result.WithMessages((IEnumerable<Message>)messages);

    /// <summary>
    /// Params overload to append multiple messages (typed).
    /// </summary>
    public static Result<T> WithMessages<T>(this Result<T> result, params Message[] messages)
        => result.WithMessages((IEnumerable<Message>)messages);

    // ---------------------------------------------------------------------
    // Metadata helpers
    // ---------------------------------------------------------------------

    /// <summary>
    /// Returns a new instance with a metadata key/value attached to the <b>last</b> message.
    /// If there are no messages, the original instance is returned unchanged.
    /// </summary>
    public static Result WithDetail(this Result result, string key, object? value)
    {
        if (result.Messages.Count == 0)
        {
            return result;
        }

        var list = new List<Message>(result.Messages);
        var lastIndex = list.Count - 1;
        var last = list[lastIndex];

        var meta = last.Metadata.Count > 0
            ? last.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        meta[key] = value;
        list[lastIndex] = new Message(last.Type, last.Content, meta);
        return new(result.Status, result.Code, list);
    }

    /// <summary>
    /// Returns a new instance with a metadata key/value attached to the <b>last</b> message.
    /// If there are no messages, the original instance is returned unchanged.
    /// </summary>
    public static Result<T> WithDetail<T>(this Result<T> result, string key, object? value)
    {
        if (result.Messages.Count == 0)
        {
            return result;
        }

        var list = new List<Message>(result.Messages);
        var lastIndex = list.Count - 1;
        var last = list[lastIndex];

        var meta = last.Metadata.Count > 0
            ? last.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        meta[key] = value;
        list[lastIndex] = new Message(last.Type, last.Content, meta);
        return new(result.Status, result.Value, result.Code, list);
    }

    // ---------------------------------------------------------------------
    // Value helpers
    // ---------------------------------------------------------------------

    /// <summary>
    /// Gets <see cref="Result{T}.Value"/> when successful; otherwise returns <paramref name="fallback"/>.
    /// </summary>
    public static T? GetValueOrDefault<T>(this Result<T> result, T? fallback = default)
        => result.IsSuccess() ? result.Value : fallback;

    /// <summary>
    /// Tries to read <see cref="Result{T}.Value"/> when successful.
    /// </summary>
    public static bool TryGetValue<T>(this Result<T> result, out T? value)
    {
        if (result.IsSuccess())
        {
            value = result.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Gets <see cref="Result{T}.Value"/> or throws an exception created by <paramref name="exceptionFactory"/>.
    /// </summary>
    public static T GetValueOrThrow<T>(this Result<T> result, Func<Result<T>, Exception> exceptionFactory)
        => result.IsSuccess() ? result.Value! : throw exceptionFactory(result);

    // ---------------------------------------------------------------------
    // Validation extraction
    // ---------------------------------------------------------------------

    /// <summary>
    /// Attempts to extract all <see cref="ValidationResult"/> instances attached to
    /// any message metadata on the <paramref name="result"/>. Supports both the
    /// per-message key "<c>ValidationResult</c>" and the aggregate key
    /// "<c>ValidationResults</c>".
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <param name="validationResults">
    /// When found, receives a non-empty, read-only list of <see cref="ValidationResult"/>.
    /// When none are found, receives an empty list.
    /// </param>
    /// <returns><c>true</c> if at least one validation result was found; otherwise <c>false</c>.</returns>
    public static bool TryGetValidationResults(this Result result, out IReadOnlyList<ValidationResult> validationResults)
    {
        var list = CollectValidationResults(result.Messages);
        validationResults = list;
        return list.Count > 0;
    }

    /// <summary>
    /// Returns all <see cref="ValidationResult"/> instances attached to message metadata on the result.
    /// If none are present, returns an empty list.
    /// </summary>
    public static IReadOnlyList<ValidationResult> GetValidationResults(this Result result)
        => CollectValidationResults(result.Messages);

    /// <summary>
    /// Returns all <see cref="ValidationResult"/> instances attached to message metadata on the result.
    /// If none are present, returns an empty list.
    /// </summary>
    public static IReadOnlyList<ValidationResult> GetValidationResults<T>(this Result<T> result)
        => CollectValidationResults(result.Messages);
}
