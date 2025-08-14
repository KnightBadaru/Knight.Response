using Knight.Response.Factories;
using Knight.Response.Models;

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
        Assert.Equal(Status.Failed, status);
        Assert.Single(messages);
        Assert.Equal(messageContent, messages[0].Content);
        Assert.Equal(MessageType.Error, messages[0].Type);
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
        Assert.Equal(Status.Completed, status);
        Assert.Equal(42, value);
        Assert.Single(messages);
        Assert.Equal(messageContent, messages[0].Content);
        Assert.Equal(MessageType.Information, messages[0].Type);
    }
}
