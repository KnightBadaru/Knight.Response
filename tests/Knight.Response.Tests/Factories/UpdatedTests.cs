using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Factories;

public class UpdatedTests
{
    [Fact]
    public void Updated_Typed_Uses_Updated_Code_And_Preserves_Value()
    {
        // Arrange
        var value = new { Id = 7 };

        // Act
        var result = Results.Updated(value);

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.Code!.Value.ShouldBe(ResultCodes.Updated.Value);
        result.Value.ShouldBe(value);
    }

    [Fact]
    public void Updated_Defaults_Code_And_Allows_Custom_Message()
    {
        // Arrange / Act
        var result = Results.Updated(message: "done");

        // Assert
        result.Code.ShouldBe(ResultCodes.Updated);
        result.Messages[0].Content.ShouldBe("done");
    }

    [Fact]
    public void Updated_With_Custom_Code_Overrides_Default()
    {
        // Arrange
        var custom = new ResultCode("CustomCode");
        var result = Results.Updated("ok", code: custom);

        // Act & Assert
        result.Code.ShouldBe(custom);
    }
}
