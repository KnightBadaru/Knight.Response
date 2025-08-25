using Knight.Response.Models;
using Shouldly;

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
        message.Metadata.Count.ShouldBe(2);
        message.Metadata[key].ShouldBe(value);
        message.Metadata[key2].ShouldBe(value2);
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
        message.Metadata.ShouldHaveSingleItem(); // still only "a"
        message.Metadata.ShouldContain(m => m.Key == "a");
        message.Metadata.ShouldNotContain(m => m.Key == "b");
    }
}
