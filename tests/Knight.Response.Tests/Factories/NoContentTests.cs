using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Factories;

public class NoContentTests
{
    [Fact]
    public void NoContent_UnTyped_Defaults_Code_To_NoContent()
    {
        // Arrange
        // Act
        var result = Results.NoContent();

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.Code.ShouldNotBeNull();
        result.Code!.Value.ShouldBe(ResultCodes.NoContent.Value);
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void NoContent_Typed_Defaults_Code_To_NoContent()
    {
        // Arrange
        // Act
        var result = Results.NoContent<object>();

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.Code!.Value.ShouldBe(ResultCodes.NoContent.Value);
        result.Value.ShouldBeNull();
    }

    [Fact]
    public void NoContent_Defaults_To_Completed_And_Code_NoContent()
    {
        // Arrange / Act
        var result = Results.NoContent();

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.Code.ShouldBe(ResultCodes.NoContent);
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void NoContent_With_Message_And_Custom_Code()
    {
        // Arrange
        var message = "nothing to return";

        // Act
        var result = Results.NoContent(message: message, code: ResultCodes.Updated); // override code

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.Code.ShouldBe(ResultCodes.Updated);
        result.Messages[0].Content.ShouldBe(message);
        result.Messages[0].Type.ShouldBe(MessageType.Information);
    }

    [Fact]
    public void NoContent_Typed_Defaults_To_NoContent_Code()
    {
        // Arrange / Act
        var result = Results.NoContent<string>();

        // Assert
        result.Code.ShouldBe(ResultCodes.NoContent);
        result.Value.ShouldBeNull();
    }
}
