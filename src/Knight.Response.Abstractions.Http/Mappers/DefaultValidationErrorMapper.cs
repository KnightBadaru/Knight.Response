using System.Text.RegularExpressions;
using Knight.Response.Models;

namespace Knight.Response.Abstractions.Http.Mappers;

/// <summary>
/// Default, conservative mapper:
/// 1) respects <c>metadata["field"]</c> if present;
/// 2) otherwise parses messages of the form "Field: message";
/// 3) ignores messages that cannot be mapped to a field.
/// </summary>
public sealed class DefaultValidationErrorMapper : IValidationErrorMapper
{
    private static readonly Regex ColonPattern = new(@"^\s*(?<field>[A-Za-z0-9_.\[\]]+)\s*:\s*(?<msg>.+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Return a map of field => list of error messages. Return empty if nothing to map.
    /// </summary>
    public IDictionary<string, string[]> Map(IReadOnlyList<Message> messages)
    {
        var dictionary = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var message in messages)
        {
            // Prefer explicit metadata
            if (message.Metadata.TryGetValue("field", out var fieldObj) && fieldObj is string fieldValue && !string.IsNullOrWhiteSpace(fieldValue))
            {
                Add(dictionary, fieldValue, message.Content);
                continue;
            }

            // Fallback: "field: message"
            var match = ColonPattern.Match(message.Content);
            if (match.Success)
            {
                var field = match.Groups["field"].Value.Trim();
                var msg   = match.Groups["msg"].Value.Trim();
                if (!string.IsNullOrWhiteSpace(field))
                {
                    Add(dictionary, field, msg);
                }
            }
        }

        return dictionary.ToDictionary(pair => pair.Key, valuePair => valuePair.Value.ToArray(), StringComparer.OrdinalIgnoreCase);

        static void Add(Dictionary<string, List<string>> dictionary, string field, string message)
        {
            if (!dictionary.TryGetValue(field, out var list))
            {
                dictionary[field] = list = new List<string>(1);
            }

            list.Add(message);
        }
    }
}
