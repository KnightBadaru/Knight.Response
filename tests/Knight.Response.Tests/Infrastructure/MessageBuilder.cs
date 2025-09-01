using Knight.Response.Models;

namespace Knight.Response.Tests.Infrastructure;

internal static class MessageBuilder
{
    public static Message Info(string text = "info") => new(MessageType.Information, text);
    public static Message Warn(string text = "warn") => new(MessageType.Warning, text);
    public static Message Error(string text = "error") => new(MessageType.Error, text);
}
