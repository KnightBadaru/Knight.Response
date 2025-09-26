using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Extensions;

/// <summary>
/// Composable, immutable helpers for <see cref="Result"/> and <see cref="Result&lt;T&gt;"/>:
/// predicates, observation hooks, functional composition, validation, code &amp; message helpers,
/// and ergonomic value accessors. All methods are pure (they return new instances).
/// </summary>
public static class MessageExtensions
{
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
}
