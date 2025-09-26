using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Factories;

public class UnexpectedErrorTests
{
    [Fact]
    public void UnexpectedError_Uses_Error_With_UnexpectedError_Code()
    {
        // Arrange & Act
        var result = Results.UnexpectedError("boom");

        // Assert
        result.IsError().ShouldBeTrue();
        result.Code.ShouldBe(ResultCodes.UnexpectedError);
        result.Messages[0].Content.ShouldBe("boom");
    }
}
