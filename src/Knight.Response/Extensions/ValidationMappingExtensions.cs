using System.ComponentModel.DataAnnotations;
using Knight.Response.Models;

namespace Knight.Response.Extensions;

/// <summary>
/// Utilities for converting <see cref="ValidationResult"/> into Knight.Response <see cref="Message"/>s.
/// </summary>
internal static class ValidationMappingExtensions
{
    /// <summary>
    /// Converts validation results to error messages using "Field: Validation error" when a error message is not present.
    /// </summary>
    public static IReadOnlyList<Message> ToMessagesPrefixed(IEnumerable<ValidationResult> errors)
    {
        var list = new List<Message>();

        foreach (var e in errors)
        {
            // Pick first member name if present
            var field = e.MemberNames?.FirstOrDefault();

            // Normalize message text
            var baseText = string.IsNullOrWhiteSpace(e.ErrorMessage)
                ? "Validation error."
                : e.ErrorMessage!.Trim();

            var content = string.IsNullOrWhiteSpace(e.ErrorMessage)
                ? $"{field?.Trim()}: {baseText.Trim()}"
                : e.ErrorMessage.Trim();

            var metadata = new Dictionary<string, object?>{["ValidationResult"] = e};
            list.Add(new Message(MessageType.Error, content, metadata));
        }

        return list;
    }

    /// <summary>
    /// Converts validation results to error messages and lets callers attach field metadata
    /// using the provided enricher delegate. The base message text is the raw error message
    /// (no field prefix); the field (if any) is passed to <paramref name="enrich"/>.
    /// </summary>
    public static IReadOnlyList<Message> ToMessagesWithMetadata(
        IEnumerable<ValidationResult> errors,
        Func<Message, string?, Message> enrich)
    {
        var list = new List<Message>();

        foreach (var e in errors)
        {
            var field = e.MemberNames?
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
                ?.Trim();

            // Raw (unprefixed) content for the enrich path
            var content = NormalizeErrorMessage(e.ErrorMessage);

            var msg = new Message(MessageType.Error, content);
            msg = enrich(msg, field);

            list.Add(msg);
        }

        return list;
    }

    private static string NormalizeErrorMessage(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            text = "Validation error.";
        }

        return text!.Trim();
    }
}
