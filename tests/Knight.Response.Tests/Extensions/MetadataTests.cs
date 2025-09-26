using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Extensions;

public class MetadataTests
{
    [Fact]
    public void WithDetail_Attaches_To_Last_Message_And_Preserves_Others()
    {
        // Arrange
        var result = Results.Failure(new[]
        {
            new Message(MessageType.Error, "first"),
            new Message(MessageType.Error, "second")
        });

        // Act
        var updated = result.WithDetail("key", 123);

        // Assert
        updated.Messages.Count.ShouldBe(2);
        updated.Messages[0].Metadata.Count.ShouldBe(0);
        updated.Messages[1].Metadata["key"].ShouldBe(123);
    }

    [Fact]
    public void WithDetail_NoMessages_Returns_Original()
    {
        // Arrange
        var result = Results.Success();

        // Act
        var updated = result.WithDetail("x", "y");

        // Assert
        updated.ShouldBeSameAs(result);
    }

    [Fact]
    public void WithDetail_Attaches_To_Last_Message_And_Is_CaseInsensitive()
    {
        // Arrange
        var original = Results.Failure([
            new Message(MessageType.Error, "E1"),
            new Message(MessageType.Error, "E2", new Dictionary<string, object?> { ["FIELD"] = "X" })
        ]);

        // Act
        var updated = original.WithDetail("field", "Y");

        // Assert
        original.Messages[1].Metadata["FIELD"].ShouldBe("X"); // unchanged
        updated.Messages[1].Metadata["field"].ShouldBe("Y");  // overwritten
    }

    [Fact]
    public void WithDetail_Attaches_Metadata_To_Last_Message()
    {
        // Arrange
        var result = Results.Failure("bad").WithDetail("Key", "Value");

        // Act
        var metadata = result.Messages.Last().Metadata;

        // Assert
        metadata["Key"].ShouldBe("Value");
    }

    [Fact]
    public void WithDetail_Replaces_Key_CaseInsensitive()
    {
        // Arrange
        var result = Results.Failure("bad")
            .WithDetail("KEY", "first")
            .WithDetail("key", "second");

        // Act
        var metadata = result.Messages.Last().Metadata;

        // Assert
        metadata.ContainsKey("key").ShouldBeTrue();
        metadata["key"].ShouldBe("second");
    }
}
