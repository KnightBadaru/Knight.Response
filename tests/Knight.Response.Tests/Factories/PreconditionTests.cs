using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Factories;

public class PreconditionTests
{
    [Fact]
    public void PreconditionFailed_Uses_Failed_With_PreconditionFailed_Code()
    {
        // Arrange & Act
        var result = Results.PreconditionFailed("pre");

        // Assert
        result.IsFailure().ShouldBeTrue();
        result.Code.ShouldBe(ResultCodes.PreconditionFailed);
        result.Messages[0].Content.ShouldBe("pre");
    }
}
