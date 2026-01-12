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

    [Fact]
    public void Unauthorized_Uses_Custom_Code_When_Provided()
    {
        var custom = ResultCodes.Forbidden;

        var result = Results.Unauthorized("nope", custom);

        result.Code.ShouldNotBeNull();
        result.Code!.Value.ShouldBe(custom.Value);
    }

    [Fact]
    public void Unauthorized_With_Messages_Sets_Default_Code()
    {
        var messages = new[] { new Message(MessageType.Error, "boom") };

        var result = Results.Unauthorized(messages);

        result.Code!.Value.ShouldBe(ResultCodes.Unauthorized.Value);
        result.Messages.ShouldHaveSingleItem();
    }
}
