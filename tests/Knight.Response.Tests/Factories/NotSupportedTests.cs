using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Factories;

public class NotSupportedTests
{
    [Fact]
    public void NotSupported_Uses_Failed_With_NotSupported_Code()
    {
        // Arrange & Act
        var result = Results.NotSupported("nope");

        // Assert
        result.IsFailure().ShouldBeTrue();
        result.Code.ShouldBe(ResultCodes.NotSupported);
        result.Messages[0].Content.ShouldBe("nope");
    }
}
