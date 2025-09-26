using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Factories;

public class UnAuthorizedTests
{
    [Fact]
    public void Unauthorized_Uses_Failed_With_Unauthorized_Code()
    {
        // Arrange & Act
        var result = Results.Unauthorized("auth?");

        // Assert
        result.IsFailure().ShouldBeTrue();
        result.Code.ShouldBe(ResultCodes.Unauthorized);
        result.Messages[0].Content.ShouldBe("auth?");
    }
}
