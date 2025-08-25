using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Core;

public class ResultDeconstructTests
{
    [Fact]
    public void Deconstruct_Result_Should_Expose_Status_And_Messages()
    {
        // Arrange
        const string messageContent = "boom";
        var result = Results.Failure(new List<Message> { new(MessageType.Error, messageContent) });

        // Act
        var (status, messages) = result;

        // Assert
        status.ShouldBe(Status.Failed);
        messages.ShouldHaveSingleItem();
        messages.ShouldContain(m => m.Type == MessageType.Error && m.Content == messageContent);
    }

    [Fact]
    public void Deconstruct_ResultT_Should_Expose_Status_Messages_And_Value()
    {
        // Arrange
        const string messageContent = "boom";
        var messageList = new List<Message> { new(MessageType.Information, messageContent) };
        var result = Results.Success(42, messageList);

        // Act
        var (status, messages, value) = result;

        // Assert
        status.ShouldBe(Status.Completed);
        value.ShouldBe(42);
        messages.ShouldHaveSingleItem();
        messages.ShouldContain(m => m.Type == MessageType.Information && m.Content == messageContent);
    }
}
