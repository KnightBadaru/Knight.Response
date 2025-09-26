using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Factories;

public class DeletedTests
{
    [Fact]
    public void Deleted_Allows_Optional_Message_And_Defaults_Code()
    {
        // Arrange
        var msg = new Message(MessageType.Information, "Removed");

        // Act
        var result = Results.Deleted(messages: [msg]);

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.Code!.Value.ShouldBe(ResultCodes.Deleted.Value);
        result.Messages.ShouldHaveSingleItem().Content.ShouldBe("Removed");
    }

    [Fact]
    public void Deleted_Typed_Defaults_Code_And_No_Messages()
    {
        // Arrange / Act
        var result = Results.Deleted<string>();

        // Assert
        result.Code.ShouldBe(ResultCodes.Deleted);
        result.Messages.ShouldBeEmpty();
    }
}
