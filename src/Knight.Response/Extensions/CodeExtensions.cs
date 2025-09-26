using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Extensions;

/// <summary>
/// Composable, immutable helpers for <see cref="Result"/> and <see cref="Result&lt;T&gt;"/>:
/// predicates, observation hooks, functional composition, validation, code &amp; message helpers,
/// and ergonomic value accessors. All methods are pure (they return new instances).
/// </summary>
public static class CodeExtensions
{
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
}
