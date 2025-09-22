using System.ComponentModel.DataAnnotations;
using Knight.Response.Core;
using Knight.Response.Factories;
using Knight.Response.Models;

namespace Knight.Response.Extensions;

/// <summary>
/// Composable, immutable helpers for <see cref="Result"/> and <see cref="Result{T}"/>:
/// predicates, observation hooks, functional composition, validation, code & message helpers,
/// and ergonomic value accessors. All methods are pure (they return new instances).
/// </summary>
public static class ResultExtensions
{
    // ---- helpers ----

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

    // ---- helpers ----
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
    /// Returns <c>true</c> when <paramref name="result"/> has a <see cref="ResultCode"/>
    /// equal to <paramref name="code"/> (case-insensitive).
    /// </summary>
    /// <param name="result">The result to inspect.</param>
    /// <param name="code">The code string to match.</param>
    public static bool HasCode(this Result result, string code) =>
        result.Code is { } c && string.Equals(c.Value, code, StringComparison.OrdinalIgnoreCase);

    // ---------------------------------------------------------------------
    // Observation hooks
    // ---------------------------------------------------------------------

    /// <summary>
    /// Executes <paramref name="action"/> if <paramref name="result"/> is successful.
    /// Returns the original <paramref name="result"/> for chaining.
    /// </summary>
    public static Result OnSuccess(this Result result, Action action)
    {
        if (result.IsSuccess()) action();
        return result;
    }

    /// <summary>
    /// Executes <paramref name="action"/> if <paramref name="result"/> is successful.
    /// Returns the original <paramref name="result"/> for chaining.
    /// </summary>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T?> action)
    {
        if (result.IsSuccess()) action(result.Value);
        return result;
    }

    /// <summary>
    /// Executes <paramref name="action"/> if <paramref name="result"/> is not successful.
    /// Returns the original <paramref name="result"/> for chaining.
    /// </summary>
    public static Result OnFailure(this Result result, Action<IReadOnlyList<Message>> action)
    {
        if (!result.IsSuccess()) action(result.Messages);
        return result;
    }

    /// <summary>
    /// Executes <paramref name="action"/> if <paramref name="result"/> is not successful.
    /// Returns the original <paramref name="result"/> for chaining.
    /// </summary>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<IReadOnlyList<Message>> action)
    {
        if (!result.IsSuccess()) action(result.Messages);
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
        if (!result.IsSuccess()) return Results.Failure<TU>(result.Messages);
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
        if (!result.IsSuccess()) return result;
        return predicate(result.Value) ? result : Results.Failure<T>(errorMessage);
    }

    /// <summary>
    /// Executes <paramref name="action"/> for side-effects when the result is successful; returns the original result.
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T?> action)
    {
        if (result.IsSuccess()) action(result.Value);
        return result;
    }

    /// <summary>
    /// Converts a failed result into a successful one by providing a fallback value via <paramref name="recovery"/>.
    /// If already successful, the original result is returned unchanged.
    /// </summary>
    public static Result<T> Recover<T>(this Result<T> result, Func<IReadOnlyList<Message>, T> recovery)
    {
        if (result.IsSuccess()) return result;
        var fallback = recovery(result.Messages);
        return Results.Success(fallback);
    }

    // ---------------------------------------------------------------------
    // Code & Message helpers (immutable)
    // ---------------------------------------------------------------------

    /// <summary>Returns a new <see cref="Result"/> with <see cref="Result.Code"/> set to <paramref name="code"/>.</summary>
    public static Result WithCode(this Result result, ResultCode code)
        => new(result.Status, code, result.Messages);

    /// <summary>Returns a new <see cref="Result{T}"/> with <see cref="Result.Code"/> set to <paramref name="code"/>.</summary>
    public static Result<T> WithCode<T>(this Result<T> result, ResultCode code)
        => new(result.Status, result.Value, code, result.Messages);

    /// <summary>Returns a new <see cref="Result"/> with <paramref name="message"/> appended.</summary>
    public static Result WithMessage(this Result result, Message message)
    {
        var list = new List<Message>(result.Messages) { message };
        return new(result.Status, result.Code, list);
    }

    /// <summary>Returns a new <see cref="Result{T}"/> with <paramref name="message"/> appended.</summary>
    public static Result<T> WithMessage<T>(this Result<T> result, Message message)
    {
        var list = new List<Message>(result.Messages) { message };
        return new(result.Status, result.Value, result.Code, list);
    }

    /// <summary>Returns a new <see cref="Result"/> with <paramref name="additional"/> appended.</summary>
    public static Result WithMessages(this Result result, IEnumerable<Message> additional)
    {
        var combined = result.Messages.Concat(additional).ToList();
        return new(result.Status, result.Code, combined);
    }

    /// <summary>Returns a new <see cref="Result{T}"/> with <paramref name="additional"/> appended.</summary>
    public static Result<T> WithMessages<T>(this Result<T> result, IEnumerable<Message> additional)
    {
        var combined = result.Messages.Concat(additional).ToList();
        return new(result.Status, result.Value, result.Code, combined);
    }

    /// <summary>Params overload to append multiple messages.</summary>
    public static Result WithMessages(this Result result, params Message[] messages)
        => result.WithMessages((IEnumerable<Message>)messages);

    /// <summary>Params overload to append multiple messages (typed).</summary>
    public static Result<T> WithMessages<T>(this Result<T> result, params Message[] messages)
        => result.WithMessages((IEnumerable<Message>)messages);

    /// <summary>
    /// Returns a new instance with a metadata key/value attached to the <b>last</b> message.
    /// If there are no messages, the original instance is returned unchanged.
    /// </summary>
    public static Result WithDetail(this Result result, string key, object? value)
    {
        if (result.Messages.Count == 0) return result;

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
        if (result.Messages.Count == 0) return result;

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

    /// <summary>Gets <see cref="Result{T}.Value"/> when successful; otherwise returns <paramref name="fallback"/>.</summary>
    public static T? GetValueOrDefault<T>(this Result<T> result, T? fallback = default)
        => result.IsSuccess() ? result.Value : fallback;

    /// <summary>Tries to read <see cref="Result{T}.Value"/> when successful.</summary>
    public static bool TryGetValue<T>(this Result<T> result, out T? value)
    {
        if (result.IsSuccess()) { value = result.Value; return true; }
        value = default;
        return false;
    }

    /// <summary>Gets <see cref="Result{T}.Value"/> or throws an exception created by <paramref name="exceptionFactory"/>.</summary>
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
    /// Attempts to extract all <see cref="ValidationResult"/> instances attached to
    /// any message metadata on the <paramref name="result"/>. Supports both the
    /// per-message key "<c>ValidationResult</c>" and the aggregate key
    /// "<c>ValidationResults</c>".
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="validationResults">
    /// When found, receives a non-empty, read-only list of <see cref="ValidationResult"/>.
    /// When none are found, receives an empty list.
    /// </param>
    /// <returns><c>true</c> if at least one validation result was found; otherwise <c>false</c>.</returns>
    public static bool TryGetValidationResults<T>(this Result<T> result, out IReadOnlyList<ValidationResult> validationResults)
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
