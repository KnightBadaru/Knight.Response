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
    /// Determines whether the typed result's value is <b>not</b> <c>null</c>.
    /// This does not consider <see cref="Result.Status"/>; it only checks the value reference.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The typed result to inspect.</param>
    /// <returns><c>true</c> when <paramref name="result"/>.Value is non-<c>null</c>; otherwise <c>false</c>.</returns>
    public static bool ValueIsNotNull<T>(this Result<T> result) => result.Value is not null;

    /// <summary>
    /// Determines whether the result is successful and its collection value is <b>not</b> <c>null</c>
    /// and contains at least one element.
    /// </summary>
    /// <typeparam name="T">The collection type (e.g., <c>IEnumerable&lt;TItem&gt;</c>).</typeparam>
    /// <typeparam name="TItem">The element type of the collection.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <returns>
    /// <c>true</c> if the result is <see cref="Status.Completed"/> and <paramref name="result"/>.Value
    /// is a non-<c>null</c> collection with at least one element; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This is the logical opposite of <c>ValueIsNullOrEmpty</c>, which treats an unsuccessful result as "null/empty".
    /// </remarks>
    public static bool ValueIsNotNullOrEmpty<T, TItem>(this Result<T> result)
        where T : IEnumerable<TItem>?
        => result.IsSuccess() && result.Value is not null && result.Value.Any();

    /// <summary>
    /// Determines whether the result is successful and its string value is <b>not</b> <c>null</c>,
    /// empty, or whitespace.
    /// </summary>
    /// <param name="result">The string result to inspect.</param>
    /// <returns>
    /// <c>true</c> if the result is <see cref="Status.Completed"/> and its string value has
    /// non-whitespace content; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This is the logical opposite of <c>ValueIsNullOrWhiteSpace</c>, which returns <c>true</c>
    /// when the result is unsuccessful or the string is null/whitespace.
    /// </remarks>
    public static bool ValueIsNotNullOrWhiteSpace(this Result<string> result)
        => result.IsSuccess() && !string.IsNullOrWhiteSpace(result.Value);

    /// <summary>
    /// Determines whether the result is unsuccessful, or if successful,
    /// whether its boolean value is <c>true</c>.
    /// </summary>
    /// <param name="result">The boolean result to inspect.</param>
    /// <returns>
    /// <c>true</c> if the result is unsuccessful, or if its value is <c>true</c>;
    /// otherwise <c>false</c>.
    /// </returns>
    public static bool ValueIsTrue(this Result<bool> result)
        => result.IsUnsuccessful() || result.Value;

    /// <summary>
    /// Determines whether the result is unsuccessful, or if successful,
    /// whether its nullable boolean value is <c>true</c>.
    /// </summary>
    /// <param name="result">The nullable boolean result to inspect.</param>
    /// <returns>
    /// <c>true</c> if the result is unsuccessful, or if its value is <c>true</c>;
    /// otherwise <c>false</c>.
    /// </returns>
    public static bool ValueIsTrue(this Result<bool?> result)
        => result.IsUnsuccessful() || result.Value == true;

    /// <summary>
    /// Determines whether the result is unsuccessful, or if successful,
    /// whether its boolean value is <c>false</c>.
    /// </summary>
    /// <param name="result">The boolean result to inspect.</param>
    /// <returns>
    /// <c>true</c> if the result is unsuccessful, or if its value is <c>false</c>;
    /// otherwise <c>false</c>.
    /// </returns>
    public static bool ValueIsFalse(this Result<bool> result)
        => result.IsUnsuccessful() || !result.Value;

    /// <summary>
    /// Determines whether the result is unsuccessful, or if successful,
    /// whether its nullable boolean value is <c>false</c> or <c>null</c>.
    /// </summary>
    /// <param name="result">The nullable boolean result to inspect.</param>
    /// <returns>
    /// <c>true</c> if the result is unsuccessful, or if its value is <c>false</c> or <c>null</c>;
    /// otherwise <c>false</c>.
    /// </returns>
    public static bool ValueIsFalse(this Result<bool?> result)
        => result.IsUnsuccessful() || result.Value != true;

    // -------------------- Equality --------------------

    /// <summary>
    /// Returns whether the result's value equals <paramref name="other"/>.
    /// Uses <see cref="EqualityComparer{T}.Default"/> and handles nulls.
    /// </summary>
    public static bool ValueEquals<T>(this Result<T> result, T other) =>
        EqualityComparer<T>.Default.Equals(result.Value, other);

    /// <summary>
    /// Returns whether the result's value does not equal <paramref name="other"/>.
    /// Uses <see cref="EqualityComparer{T}.Default"/> and handles nulls.
    /// </summary>
    public static bool ValueNotEquals<T>(this Result<T> result, T other) =>
        !EqualityComparer<T>.Default.Equals(result.Value, other);

    /// <summary>
    /// Returns whether the result's value equals the other result's value.
    /// Uses <see cref="EqualityComparer{T}.Default"/> and handles nulls.
    /// </summary>
    public static bool ValueEquals<T>(this Result<T> left, Result<T> right) =>
        EqualityComparer<T>.Default.Equals(left.Value, right.Value);

    /// <summary>
    /// Returns whether the result's value does not equal the other result's value.
    /// Uses <see cref="EqualityComparer{T}.Default"/> and handles nulls.
    /// </summary>
    public static bool ValueNotEquals<T>(this Result<T> left, Result<T> right) =>
        !EqualityComparer<T>.Default.Equals(left.Value, right.Value);

    // -------------------- Ordering --------------------
    // Note: If Value is null, these return false (no ordering to evaluate).

    /// <summary>
    /// Returns whether the result's value is greater than <paramref name="other"/>.
    /// Returns <c>false</c> if the value is <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Does not check if result is successful or not.
    /// </remarks>
    public static bool ValueGreaterThan<T>(this Result<T> result, T other) where T : IComparable<T> =>
        result.Value is T v && Comparer<T>.Default.Compare(v, other) > 0;

    /// <summary>
    /// Returns whether the result's value is greater than or equal to <paramref name="other"/>.
    /// Returns <c>false</c> if the value is <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Does not check if result is successful or not.
    /// </remarks>
    public static bool ValueGreaterThanOrEqual<T>(this Result<T> result, T other) where T : IComparable<T> =>
        result.Value is T v && Comparer<T>.Default.Compare(v, other) >= 0;

    /// <summary>
    /// Returns whether the result's value is less than <paramref name="other"/>.
    /// Returns <c>false</c> if the value is <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Does not check if result is successful or not.
    /// </remarks>
    public static bool ValueLessThan<T>(this Result<T> result, T other) where T : IComparable<T> =>
        result.Value is T v && Comparer<T>.Default.Compare(v, other) < 0;

    /// <summary>
    /// Returns whether the result's value is less than or equal to <paramref name="other"/>.
    /// Returns <c>false</c> if the value is <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Does not check if result is successful or not.
    /// </remarks>
    public static bool ValueLessThanOrEqual<T>(this Result<T> result, T other) where T : IComparable<T> =>
        result.Value is T v && Comparer<T>.Default.Compare(v, other) <= 0;

    // -------- Result vs Result ordering (convenience) --------

    /// <summary>
    /// Returns whether the left result's value is greater than the right result's value.
    /// Returns <c>false</c> if either value is <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Does not check if result is successful or not.
    /// </remarks>
    public static bool ValueGreaterThan<T>(this Result<T> left, Result<T> right) where T : IComparable<T> =>
        left.Value is T lv && right.Value is T rv && Comparer<T>.Default.Compare(lv, rv) > 0;

    /// <summary>
    /// Returns whether the left result's value is less than the right result's value.
    /// Returns <c>false</c> if either value is <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Does not check if result is successful or not.
    /// </remarks>
    public static bool ValueLessThan<T>(this Result<T> left, Result<T> right) where T : IComparable<T> =>
        left.Value is T lv && right.Value is T rv && Comparer<T>.Default.Compare(lv, rv) < 0;

    /// <summary>
    /// Determines whether the result is successful and its value lies within
    /// the inclusive range [<paramref name="minInclusive"/>, <paramref name="maxInclusive"/>].
    /// </summary>
    /// <remarks>
    /// Does not check if result is successful.
    /// </remarks>
    /// <typeparam name="T">A value type that supports ordering.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="minInclusive">Lower bound (inclusive).</param>
    /// <param name="maxInclusive">Upper bound (inclusive).</param>
    /// <returns><c>true</c> if the value lies within the range; otherwise <c>false</c>.</returns>
    public static bool ValueBetween<T>(this Result<T> result, T minInclusive, T maxInclusive)
        where T : IComparable<T>
    {
        var v = result.Value;
        if (v == null)
        {
            return false;
        }

        return v.CompareTo(minInclusive) >= 0 && v.CompareTo(maxInclusive) <= 0;
    }

    /// <summary>
    /// Determines whether the result is successful and its value lies within
    /// the exclusive range (<paramref name="minExclusive"/>, <paramref name="maxExclusive"/>).
    /// </summary>
    /// <remarks>
    /// Does not check if result is successful.
    /// </remarks>
    /// <typeparam name="T">A value type that supports ordering.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="minExclusive">Lower bound (exclusive).</param>
    /// <param name="maxExclusive">Upper bound (exclusive).</param>
    /// <returns><c>true</c> if the value lies strictly between the bounds; otherwise <c>false</c>.</returns>
    public static bool ValueBetweenExclusive<T>(this Result<T> result, T minExclusive, T maxExclusive)
        where T : IComparable<T>
    {
        var v = result.Value;
        if (v == null)
        {
            return false;
        }

        return v.CompareTo(minExclusive) > 0 && v.CompareTo(maxExclusive) < 0;
    }

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
