using Knight.Response.Models;

namespace Knight.Response.Tests.Models;

public class MessageTests
{
    [Fact]
    public void Metadata_Should_Default_To_Empty_When_Not_Provided()
    {
        // Act
        var message = new Message(MessageType.Information, "hello");

        // Assert
        Assert.NotNull(message.Metadata);
        Assert.Empty(message.Metadata);
    }

    [Fact]
    public void Metadata_Should_Expose_Provided_Entries()
    {
        // Arrange
        const string key = "requestId";
        const string value = "abc-123";
        const string key2 = "attempt";
        const int value2 = 2;
        var metadata = new Dictionary<string, object?> { [key] = value, [key2] = value2 };

        // Act
        var message = new Message(MessageType.Warning, "note", metadata);

        // Assert
        Assert.Equal(2, message.Metadata.Count);
        Assert.Equal(value, message.Metadata[key]);
        Assert.Equal(value2, (int?)message.Metadata[key2]);
    }

    [Fact]
    public void Metadata_Does_Not_Change_If_Source_Dictionary_Is_Modified()
    {
        // Arrange
        var metadata = new Dictionary<string, object?> { ["a"] = 1 };

        // Act
        var message = new Message(MessageType.Information, "message", metadata);
        metadata["b"] = 2; // try to mutate after construction

        // Assert
        Assert.Single(message.Metadata); // still only "a"
        Assert.True(message.Metadata.ContainsKey("a"));
        Assert.False(message.Metadata.ContainsKey("b"));
    }
}
