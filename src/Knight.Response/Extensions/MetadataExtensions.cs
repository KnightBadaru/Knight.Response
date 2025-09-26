using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Extensions;

/// <summary>
/// Composable, immutable helpers for <see cref="Result"/> and <see cref="Result&lt;T&gt;"/>:
/// predicates, observation hooks, functional composition, validation, code &amp; message helpers,
/// and ergonomic value accessors. All methods are pure (they return new instances).
/// </summary>
public static class MetadataExtensions
{
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
}
