using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Factories;

public class ForbiddenTests
{
    [Fact]
    public void Forbidden_Uses_Failed_With_Forbidden_Code()
    {
        // Arrange & Act
        var result = Results.Forbidden("nope");

        // Assert
        result.IsFailure().ShouldBeTrue();
        result.Code.ShouldBe(ResultCodes.Forbidden);
        result.Messages[0].Content.ShouldBe("nope");
    }
}
