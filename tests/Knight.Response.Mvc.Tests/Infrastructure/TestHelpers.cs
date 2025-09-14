using System.Text.Json;
using Knight.Response.Core;
using Knight.Response.Factories;
using Knight.Response.Models;

namespace Knight.Response.Mvc.Tests.Infrastructure;

public static class TestHelpers
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static string HttpStatusUrl(int statusCode = 400) => $"https://httpstatuses.io/{statusCode}";
    public static string SvcStatus => "svcStatus";
    public static string Failed => "Failed";

    public static T? Deserialize<T>(string json) =>
        string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, JsonOpts);

    public static Result Failure(params string[] messages)
    {
        var list = new List<Message>();
        foreach (var m in messages)
        {
            list.Add(new Message(MessageType.Error, m));
        }

        return Results.Error(list);
    }
}
