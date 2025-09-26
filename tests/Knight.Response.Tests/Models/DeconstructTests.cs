using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Models;

public class DeconstructTests
{
    [Fact]
    public void Deconstruct_Untyped_Provides_Status_Code_Messages()
    {
        // Arrange
        var r = Results.Failure("nope").WithCode(ResultCodes.PreconditionFailed);

        // Act
        var (status, code, messages) = r;

        // Assert
        status.ShouldBe(Status.Failed);
        code.ShouldBe(ResultCodes.PreconditionFailed);
        messages.Count.ShouldBe(1);
    }

    [Fact]
    public void Deconstruct_Typed_Provides_Status_Code_Messages_Value()
    {
        // Arrange
        var r = Results.Success(10).WithCode(ResultCodes.Updated);

        // Act
        var (status, code, messages, value) = r;

        // Assert
        status.ShouldBe(Status.Completed);
        code.ShouldBe(ResultCodes.Updated);
        messages.ShouldBeEmpty();
        value.ShouldBe(10);
    }
}
