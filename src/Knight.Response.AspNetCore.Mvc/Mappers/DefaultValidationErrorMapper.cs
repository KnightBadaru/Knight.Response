using System.Text.RegularExpressions;
using Knight.Response.Models;

namespace Knight.Response.AspNetCore.Mvc.Mappers;

/// <summary>
/// Default mapper supports either metadata["field"] or "field: message" content.
/// Trims the message; preserves the original field casing.
/// </summary>
internal sealed class DefaultValidationErrorMapper : IValidationErrorMapper
{
    private static readonly Regex Colon = new(@"^\s*(?<field>[A-Za-z0-9_.\[\]]+)\s*:\s*(?<msg>.+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public IDictionary<string, string[]> Map(IReadOnlyList<Message> messages)
    {
        var dict = new Dictionary<string, List<string>>();
        foreach (var m in messages)
        {
            if (m.Metadata.TryGetValue("field", out var fObj) && fObj is string field && !string.IsNullOrWhiteSpace(field))
            {
                Add(field, m.Content.Trim());
                continue;
            }

            var match = Colon.Match(m.Content);
            if (match.Success)
            {
                Add(match.Groups["field"].Value, match.Groups["msg"].Value.Trim());
            }
        }
        return dict.ToDictionary(k => k.Key, v => v.Value.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray());

        void Add(string field, string? message)
        {
            if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (!dict.TryGetValue(field, out var list))
            {
                dict[field] = list = new List<string>();
            }

            list.Add(message!);
        }
    }
}
