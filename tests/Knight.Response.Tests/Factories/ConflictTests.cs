using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Factories;

public class ConflictTests
{
    [Fact]
    public void Conflict_Uses_Failed_With_ConcurrencyConflict_Code()
    {
        // Arrange & Act
        var result = Results.ConcurrencyConflict("conflict!");

        // Assert
        result.IsFailure().ShouldBeTrue();
        result.Code.ShouldBe(ResultCodes.ConcurrencyConflict);
        result.Messages[0].Content.ShouldBe("conflict!");
    }
}
