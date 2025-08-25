using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Knight.Response.AspNetCore.Tests.Infrastructure;

public static class TestHelpers
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static T? Deserialize<T>(string json) =>
        string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, JsonOpts);

    /// <summary>Get an extension as a string (handles JsonElement or direct string).</summary>
    public static string ExtString(ProblemDetails pd, string key)
    {
        pd.Extensions.ShouldContainKey(key);
        var val = pd.Extensions[key];
        if (val is string s)
        {
            return s;
        }

        var je = (JsonElement)val!;
        return je.ValueKind == JsonValueKind.String ? je.GetString()! : je.ToString();
    }

    /// <summary>Generic helper to deserialize an extension value to a type.</summary>
    public static T Ext<T>(ProblemDetails pd, string key)
    {
        pd.Extensions.ShouldContainKey(key);
        var val = pd.Extensions[key];

        // Cast to type T if set
        if (val is T typed)
        {
            return typed;
        }

        // Otherwise, assumes it is JsonElement coming from (de)serialization
        var je = (JsonElement)val!;
        return je.Deserialize<T>(JsonOpts)!;
    }

    /// <summary>Extract errors from VPD.</summary>
    public static IReadOnlyDictionary<string, string[]> ExtErrors(ValidationProblemDetails vpd)
        => Ext<Dictionary<string, string[]>>(vpd, "errors");

    public static void SvcStatusShouldBeFailed(ProblemDetails pd, string status = "Failed")
    {
        ExtString(pd, "svcStatus").ShouldBe("Failed");
    }

    public static string HttpStatusUrl(int statusCode = 400) => $"https://httpstatuses.io/{statusCode}";
    public static string SvcStatus => "svcStatus";
    public static string Failed => "Failed";
}
