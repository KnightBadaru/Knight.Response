using System.Collections;
using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Extensions;

/// <summary>
/// Composable, immutable helpers for <see cref="Result"/> and <see cref="Result&lt;T&gt;"/>:
/// predicates, observation hooks, functional composition, validation, code &amp; message helpers,
/// and ergonomic value accessors. All methods are pure (they return new instances).
/// </summary>
public static class ValueExtensions
{
    /// <summary>
    /// Determines whether the typed result's value is <c>null</c>.
    /// This does not consider <see cref="Status"/>; it only checks the value reference.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The typed result to inspect.</param>
    /// <returns><c>true</c> when <paramref name="result"/>.Value is <c>null</c>; otherwise <c>false</c>.</returns>
    public static bool ValueIsNull<T>(this Result<T> result) => result.Value is null;

    /// <summary>
    /// Returns <c>true</c> if the result's <see cref="Result{T}.Value"/> is <c>null</c>
    /// or is an empty string / empty collection.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="result"/>.Value is:
    /// <list type="bullet">
    ///   <item><description><c>null</c></description></item>
    ///   <item><description>an empty <see cref="string"/></description></item>
    ///   <item><description>an empty <see cref="ICollection"/> (non-generic)</description></item>
    ///   <item><description>an empty <see cref="IEnumerable{T}"/> (generic)</description></item>
    /// </list>
    /// Otherwise, returns <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This is especially useful when your domain treats "empty" as equivalent to "no value".
    /// </remarks>
    public static bool ValueIsNullOrEmpty<T>(this Result<T> result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        var value = result.Value;

        if (value is null)
        {
            return true;
        }

        // string
        if (value is string s)
        {
            return string.IsNullOrEmpty(s);
        }

        // non-generic collection
        if (value is ICollection coll)
        {
            return coll.Count == 0;
        }

        // generic collection
        if (value is IEnumerable<object> gen)
        {
            return !gen.Any();
        }

        return false;
    }

    /// <summary>
    /// Determines whether the result is unsuccessful, or if successful, whether
    /// its value is <c>null</c> or an empty sequence.
    /// </summary>
    /// <typeparam name="T">The collection type (must implement <see cref="IEnumerable{TItem}"/>).</typeparam>
    /// <typeparam name="TItem">The element type of the collection.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <returns>
    /// <c>true</c> if the result is unsuccessful, or if its value is <c>null</c> or contains no items;
    /// otherwise <c>false</c>.
    /// </returns>
    public static bool ValueIsNullOrEmpty<T, TItem>(this Result<T> result)
        where T : IEnumerable<TItem>?
        => result.IsUnsuccessful() || result.Value is null || !result.Value.Any();

    /// <summary>
    /// Determines whether the result is unsuccessful, or if successful,
    /// whether its string value is <c>null</c>, empty, or consists only of whitespace.
    /// </summary>
    /// <param name="result">The string result to inspect.</param>
    /// <returns>
    /// <c>true</c> if the result is unsuccessful, or if its value is <c>null</c>, empty,
    /// or contains only whitespace characters; otherwise <c>false</c>.
    /// </returns>
    public static bool ValueIsNullOrWhiteSpace(this Result<string> result)
        => result.IsUnsuccessful() || string.IsNullOrWhiteSpace(result.Value);

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
}
