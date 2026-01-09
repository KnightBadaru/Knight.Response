using System.Collections;
using Knight.Response.Core;

namespace Knight.Response.Extensions;

/// <summary>
/// Status-aware helpers for <see cref="Result{T}"/> that combine <c>IsSuccess()</c>
/// with value inspection. Use these when "value checks only matter on success".
/// </summary>
public static class SuccessAndValueExtensions
{
    /// <summary>Returns <c>true</c> when the result is successful and its value is <c>null</c>.</summary>
    public static bool IsSuccessAndValueIsNull<T>(this Result<T> result) =>
        result.IsSuccess() && result.Value is null;

    /// <summary>
    /// Returns <c>true</c> when the result is successful and its value is <c>null</c> or "empty"
    /// (empty string / empty collection).
    /// </summary>
    public static bool IsSuccessAndValueIsNullOrEmpty<T>(this Result<T> result)
    {
        if (!result.IsSuccess())
        {
            return false;
        }

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
    /// Returns <c>true</c> when the result is successful and its collection value is <c>null</c> or empty.
    /// </summary>
    public static bool IsSuccessAndValueIsNullOrEmpty<T, TItem>(this Result<T> result)
        where T : IEnumerable<TItem>?
        => result.IsSuccess() && (result.Value is null || !result.Value.Any());

    /// <summary>
    /// Returns <c>true</c> when the result is successful and its string value is <c>null</c>, empty, or whitespace.
    /// </summary>
    public static bool IsSuccessAndValueIsNullOrWhiteSpace(this Result<string> result) =>
        result.IsSuccess() && string.IsNullOrWhiteSpace(result.Value);

    /// <summary>Returns <c>true</c> when the result is successful and its value is not <c>null</c>.</summary>
    public static bool IsSuccessAndValueIsNotNull<T>(this Result<T> result) =>
        result.IsSuccess() && result.Value is not null;

    /// <summary>
    /// Returns <c>true</c> when the result is successful and its collection value is non-null and non-empty.
    /// </summary>
    public static bool IsSuccessAndValueIsNotNullOrEmpty<T, TItem>(this Result<T> result)
        where T : IEnumerable<TItem>?
        => result.IsSuccess() && result.Value is not null && result.Value.Any();

    /// <summary>
    /// Returns <c>true</c> when the result is successful and its string value has non-whitespace content.
    /// </summary>
    public static bool IsSuccessAndValueIsNotNullOrWhiteSpace(this Result<string> result) =>
        result.IsSuccess() && !string.IsNullOrWhiteSpace(result.Value);

    /// <summary>Returns <c>true</c> when the result is successful and its boolean value is <c>true</c>.</summary>
    public static bool IsSuccessAndValueIsTrue(this Result<bool> result) =>
        result.IsSuccess() && result.Value;

    /// <summary>Returns <c>true</c> when the result is successful and its nullable boolean value is <c>true</c>.</summary>
    public static bool IsSuccessAndValueIsTrue(this Result<bool?> result) =>
        result.IsSuccess() && result.Value == true;

    /// <summary>Returns <c>true</c> when the result is successful and its boolean value is <c>false</c>.</summary>
    public static bool IsSuccessAndValueIsFalse(this Result<bool> result) =>
        result.IsSuccess() && !result.Value;

    /// <summary>Returns <c>true</c> when the result is successful and its nullable boolean value is <c>false</c>.</summary>
    public static bool IsSuccessAndValueIsFalse(this Result<bool?> result) =>
        result.IsSuccess() && result.Value == false;
}
