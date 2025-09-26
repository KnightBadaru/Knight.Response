using System.ComponentModel.DataAnnotations;
using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Extensions;

/// <summary>
/// Composable, immutable helpers for <see cref="Result"/> and <see cref="Result&lt;T&gt;"/>:
/// predicates, observation hooks, functional composition, validation, code &amp; message helpers,
/// and ergonomic value accessors. All methods are pure (they return new instances).
/// </summary>
public static class ValidationExtensions
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
