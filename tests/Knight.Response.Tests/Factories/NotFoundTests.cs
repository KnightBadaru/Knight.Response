using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Factories;

public class NotFoundTests
{
    [Fact]
    public void NotFound_UnTyped_Defaults_Code_To_NotFound()
    {
        // Arrange
        // Act
        var result = Results.NotFound();

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.Code.ShouldNotBeNull();
        result.Code!.Value.ShouldBe(ResultCodes.NotFound.Value);
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].Type.ShouldBe(MessageType.Warning);
    }

    [Fact]
    public void NotFound_Typed_Defaults_Code_To_NotFound()
    {
        // Arrange
        // Act
        var result = Results.NotFound<string>();

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.Code.ShouldNotBeNull();
        result.Code!.Value.ShouldBe(ResultCodes.NotFound.Value);
        result.Value.ShouldBeNull();
    }

    [Fact]
    public void NotFound_Default_Should_Be_Completed_With_Warning()
    {
        // Act
        var result = Results.NotFound();

        // Assert
        result.IsSuccess().ShouldBeTrue();
        result.Status.ShouldBe(Status.Completed);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == "Resource not found." && m.Type == MessageType.Warning);
    }

    [Fact]
    public void NotFound_Generic_With_Override_Status()
    {
        // Act
        const string messageContent = "not user";
        var result = Results.NotFound<string>(messageContent, status: Status.Failed);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Failed);
        result.Value.ShouldBeNull();
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == messageContent && m.Type == MessageType.Warning);
    }

    [Fact]
    public void NotFound_Defaults_To_Warning_And_ResultCodes_NotFound()
    {
        // Arrange / Act
        var result = Results.NotFound();

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.Code.ShouldBe(ResultCodes.NotFound);
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].Type.ShouldBe(MessageType.Warning);
    }

    [Fact]
    public void NotFound_Allows_Custom_Message_And_Status()
    {
        // Arrange / Act
        var result = Results.NotFound(message: "missing", status: Status.Failed);

        // Assert
        result.Status.ShouldBe(Status.Failed);
        result.Code.ShouldBe(ResultCodes.NotFound);
        result.Messages[0].Content.ShouldBe("missing");
    }

    [Fact]
    public void NotFound_Typed_Uses_Default_Code_And_Message()
    {
        // Arrange / Act
        var result = Results.NotFound<int>();

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.Code.ShouldBe(ResultCodes.NotFound);
        result.Messages.Count.ShouldBe(1);
    }

    [Fact]
    public void NotFound_Override_Code_With_WithoutCode()
    {
        // Arrange / Act
        var result = Results.NotFound().WithoutCode();

        // Assert
        result.Code.ShouldBeNull();
    }
}
