using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Factories;

public class CreatedTests
{
    [Fact]
    public void Created_Uses_Created_Code_And_Completed_Status()
    {
        // Arrange
        // Act
        var result = Results.Created();

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.Code!.Value.ShouldBe(ResultCodes.Created.Value);
    }

    [Fact]
    public void Created_UnTyped_Defaults_Code_And_Message()
    {
        // Arrange / Act
        var result = Results.Created();

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.Code.ShouldBe(ResultCodes.Created);
        result.Messages.Count.ShouldBe(0);
    }

    [Fact]
    public void Created_Typed_With_Value_And_Custom_Message()
    {
        // Arrange / Act
        var result = Results.Created(42, message: "made", type: MessageType.Warning);

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.Value.ShouldBe(42);
        result.Code.ShouldBe(ResultCodes.Created);
        result.Messages[0].Content.ShouldBe("made");
        result.Messages[0].Type.ShouldBe(MessageType.Warning);
    }

    [Fact]
    public void Created_Adds_Single_Message()
    {
        var result = Results.Created(1, "created");

        result.Messages.ShouldHaveSingleItem();
    }

    [Fact]
    public void Created_Defaults_To_Created_Code()
    {
        var result = Results.Created(123, "ok");

        result.Code.ShouldNotBeNull();
        result.Code!.Value.ShouldBe(ResultCodes.Created.Value);
    }

    [Fact]
    public void Created_Uses_Custom_Code_When_Provided()
    {
        var custom = new ResultCode("MyCode");

        var result = Results.Created(123, "ok", code: custom);

        result.Code!.Value.ShouldBe(custom.Value);
    }
}
