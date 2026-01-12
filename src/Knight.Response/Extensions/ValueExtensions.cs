using System.Collections;
using Knight.Response.Core;

namespace Knight.Response.Extensions;

/// <summary>
/// Value-centric helpers for <see cref="Result{T}"/>.
/// These methods are side-effect free and focus on inspecting / comparing values.
/// </summary>
public static class ValueExtensions
{
    /// <summary>
    /// Determines whether the typed result's value is <c>null</c>.
    /// This does not consider <see cref="Result.Status"/>; it only checks the value reference.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The typed result to inspect.</param>
    /// <returns><c>true</c> when <paramref name="result"/>.Value is <c>null</c>; otherwise <c>false</c>.</returns>
    public static bool ValueIsNull<T>(this Result<T> result) => result.Value is null;

    /// <summary>
    /// Determines whether the result's value is <c>null</c> or "empty" (empty string / empty collection).
    /// This does not consider <see cref="Result.Status"/>; it only inspects the value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <returns>
    /// <c>true</c> if the value is <c>null</c>, an empty <see cref="string"/>,
    /// an empty <see cref="ICollection"/> (non-generic), or an empty <see cref="IEnumerable"/>; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Useful when your domain treats "empty" as equivalent to "no value".
    /// </remarks>
    public static bool ValueIsNullOrEmpty<T>(this Result<T> result)
    {
        var value = result.Value;

        if (value is null)
        {
            return true;
        }

        if (value is string s)
        {
            return string.IsNullOrEmpty(s);
        }

        if (value is ICollection coll)
        {
            return coll.Count == 0;
        }

        // Non-generic enumerable (covers most IEnumerable<T> values too, via IEnumerable)
        if (value is IEnumerable seq)
        {
            var e = seq.GetEnumerator();
            try
            {
                return !e.MoveNext();
            }
            finally
            {
                (e as IDisposable)?.Dispose();
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the result's collection value is <c>null</c> or contains no items.
    /// This does not consider <see cref="Result.Status"/>; it only inspects the value.
    /// </summary>
    /// <typeparam name="T">The collection type (must implement <see cref="IEnumerable{TItem}"/>).</typeparam>
    /// <typeparam name="TItem">The element type of the collection.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <returns>
    /// <c>true</c> if the value is <c>null</c> or empty; otherwise <c>false</c>.
    /// </returns>
    public static bool ValueIsNullOrEmpty<T, TItem>(this Result<T> result)
        where T : IEnumerable<TItem>?
        => result.Value is null || !result.Value.Any();

    /// <summary>
    /// Determines whether the result's string value is <c>null</c>, empty, or whitespace.
    /// This does not consider <see cref="Result.Status"/>; it only inspects the value.
    /// </summary>
    /// <param name="result">The string result to inspect.</param>
    /// <returns><c>true</c> if the string value is <c>null</c>, empty, or whitespace; otherwise <c>false</c>.</returns>
    public static bool ValueIsNullOrWhiteSpace(this Result<string> result)
        => string.IsNullOrWhiteSpace(result.Value);

    /// <summary>
    /// Determines whether the typed result's value is <b>not</b> <c>null</c>.
    /// This does not consider <see cref="Result.Status"/>; it only checks the value reference.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <param name="result">The typed result to inspect.</param>
    /// <returns><c>true</c> when <paramref name="result"/>.Value is non-<c>null</c>; otherwise <c>false</c>.</returns>
    public static bool ValueIsNotNull<T>(this Result<T> result) => result.Value is not null;

    /// <summary>
    /// Determines whether the result's collection value is non-<c>null</c> and contains at least one item.
    /// This does not consider <see cref="Result.Status"/>; it only inspects the value.
    /// </summary>
    /// <typeparam name="T">The collection type (must implement <see cref="IEnumerable{TItem}"/>).</typeparam>
    /// <typeparam name="TItem">The element type of the collection.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <returns><c>true</c> if the collection is non-<c>null</c> and non-empty; otherwise <c>false</c>.</returns>
    public static bool ValueIsNotNullOrEmpty<T, TItem>(this Result<T> result)
        where T : IEnumerable<TItem>?
        => result.Value is not null && result.Value.Any();

    /// <summary>
    /// Determines whether the result's string value contains non-whitespace content.
    /// This does not consider <see cref="Result.Status"/>; it only inspects the value.
    /// </summary>
    /// <param name="result">The string result to inspect.</param>
    /// <returns><c>true</c> if the string value has non-whitespace content; otherwise <c>false</c>.</returns>
    public static bool ValueIsNotNullOrWhiteSpace(this Result<string> result)
        => !string.IsNullOrWhiteSpace(result.Value);

    /// <summary>
    /// Determines whether the result's boolean value is <c>true</c>.
    /// This does not consider <see cref="Result.Status"/>; it only inspects the value.
    /// </summary>
    public static bool ValueIsTrue(this Result<bool> result) => result.Value;

    /// <summary>
    /// Determines whether the result's nullable boolean value is <c>true</c>.
    /// This does not consider <see cref="Result.Status"/>; it only inspects the value.
    /// </summary>
    public static bool ValueIsTrue(this Result<bool?> result) => result.Value == true;

    /// <summary>
    /// Determines whether the result's boolean value is <c>false</c>.
    /// This does not consider <see cref="Result.Status"/>; it only inspects the value.
    /// </summary>
    public static bool ValueIsFalse(this Result<bool> result) => !result.Value;

    /// <summary>
    /// Determines whether the result's nullable boolean value is <c>false</c>.
    /// This does not consider <see cref="Result.Status"/>; it only inspects the value.
    /// </summary>
    public static bool ValueIsFalse(this Result<bool?> result) => result.Value == false;

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
    /// Returns whether the left result's value equals the right result's value.
    /// Uses <see cref="EqualityComparer{T}.Default"/> and handles nulls.
    /// </summary>
    public static bool ValueEquals<T>(this Result<T> left, Result<T> right) =>
        EqualityComparer<T>.Default.Equals(left.Value, right.Value);

    /// <summary>
    /// Returns whether the left result's value does not equal the right result's value.
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
    public static bool ValueGreaterThan<T>(this Result<T> result, T other) where T : IComparable<T> =>
        result.Value is T v && Comparer<T>.Default.Compare(v, other) > 0;

    /// <summary>
    /// Returns whether the result's value is greater than or equal to <paramref name="other"/>.
    /// Returns <c>false</c> if the value is <c>null</c>.
    /// </summary>
    public static bool ValueGreaterThanOrEqual<T>(this Result<T> result, T other) where T : IComparable<T> =>
        result.Value is T v && Comparer<T>.Default.Compare(v, other) >= 0;

    /// <summary>
    /// Returns whether the result's value is less than <paramref name="other"/>.
    /// Returns <c>false</c> if the value is <c>null</c>.
    /// </summary>
    public static bool ValueLessThan<T>(this Result<T> result, T other) where T : IComparable<T> =>
        result.Value is T v && Comparer<T>.Default.Compare(v, other) < 0;

    /// <summary>
    /// Returns whether the result's value is less than or equal to <paramref name="other"/>.
    /// Returns <c>false</c> if the value is <c>null</c>.
    /// </summary>
    public static bool ValueLessThanOrEqual<T>(this Result<T> result, T other) where T : IComparable<T> =>
        result.Value is T v && Comparer<T>.Default.Compare(v, other) <= 0;

    /// <summary>
    /// Returns whether the left result's value is greater than the right result's value.
    /// Returns <c>false</c> if either value is <c>null</c>.
    /// </summary>
    public static bool ValueGreaterThan<T>(this Result<T> left, Result<T> right) where T : IComparable<T> =>
        left.Value is T lv && right.Value is T rv && Comparer<T>.Default.Compare(lv, rv) > 0;

    /// <summary>
    /// Returns whether the left result's value is less than the right result's value.
    /// Returns <c>false</c> if either value is <c>null</c>.
    /// </summary>
    public static bool ValueLessThan<T>(this Result<T> left, Result<T> right) where T : IComparable<T> =>
        left.Value is T lv && right.Value is T rv && Comparer<T>.Default.Compare(lv, rv) < 0;

    /// <summary>
    /// Determines whether the result's value lies within the inclusive range
    /// [<paramref name="minInclusive"/>, <paramref name="maxInclusive"/>].
    /// Returns <c>false</c> if the value is <c>null</c>.
    /// </summary>
    public static bool ValueBetween<T>(this Result<T> result, T minInclusive, T maxInclusive)
        where T : IComparable<T>
    {
        var v = result.Value;
        return v is not null
               && v.CompareTo(minInclusive) >= 0
               && v.CompareTo(maxInclusive) <= 0;
    }

    /// <summary>
    /// Determines whether the result's value lies within the exclusive range
    /// (<paramref name="minExclusive"/>, <paramref name="maxExclusive"/>).
    /// Returns <c>false</c> if the value is <c>null</c>.
    /// </summary>
    public static bool ValueBetweenExclusive<T>(this Result<T> result, T minExclusive, T maxExclusive)
        where T : IComparable<T>
    {
        var v = result.Value;
        return v is not null
               && v.CompareTo(minExclusive) > 0
               && v.CompareTo(maxExclusive) < 0;
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
