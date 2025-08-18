using System.Text.RegularExpressions;
using Knight.Response.Models;

namespace Knight.Response.AspNetCore.Mappers;

internal sealed class DefaultValidationErrorMapper : IValidationErrorMapper
{
    private static readonly Regex ColonPattern = new(@"^\s*(?<field>[A-Za-z0-9_.\[\]]+)\s*:\s*(?<msg>.+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

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
                var field = match.Groups["field"].Value;
                var msg   = match.Groups["msg"].Value;
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
