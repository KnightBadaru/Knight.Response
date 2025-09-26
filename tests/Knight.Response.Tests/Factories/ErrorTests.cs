using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;
using static Knight.Response.Tests.Infrastructure.MessageBuilder;

namespace Knight.Response.Tests.Factories;

public class ErrorTests
{
    [Fact]
    public void Error_WithMessages_Should_Create_Error_Result()
    {
        // Arrange
        const string content = "oops!";
        var messages = new List<Message> { Error(content) };

        // Act
        var result = Results.Error(messages);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Error);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == content && m.Type == MessageType.Error);
    }

    [Fact]
    public void Error_WithReason_Should_Default_To_Error_MessageType()
    {
        // Act
        const string reason = "reason";
        var result = Results.Error(reason);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Error);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == reason && m.Type == MessageType.Error);
    }

    [Fact]
    public void Error_Generic_WithMessages_Should_Create_Typed_Error()
    {
        // Arrange
        var messages = new List<Message> { Error() };

        // Act
        var result = Results.Error<string>(messages);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Error);
        result.Value.ShouldBeNull();
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == messages.First().Content && m.Type == messages.First().Type);
    }

    [Fact]
    public void Error_Generic_WithReason_Should_Create_Typed_Error()
    {
        // Act
        const string reason = "fail!";
        var result = Results.Error<int>(reason);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Error);
        result.Value.ShouldBe(0);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == reason && m.Type == MessageType.Error);
    }

    [Fact]
    public void Error_From_Exception_Should_Use_Exception_Message()
    {
        // Arrange
        const string message = "ex-message";
        var exception = new InvalidOperationException(message);

        // Act
        var result = Results.Error(exception);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Error);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == message && m.Type == MessageType.Error);
    }

    [Fact]
    public void Error_Generic_From_Exception_Should_Use_Exception_Message()
    {
        // Arrange
        const string message = "yikes!";
        var exception = new InvalidOperationException(message);

        // Act
        var result = Results.Error<string>(exception);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Error);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == message && m.Type == MessageType.Error);
    }

    [Fact]
    public void Error_FromException_UsesMessage()
    {
        // Arrange
        var ex = new InvalidOperationException("oops");

        // Act
        var result = Results.Error(ex);

        // Assert
        result.Status.ShouldBe(Status.Error);
        result.Messages.Single().Content.ShouldBe("oops");
    }
}
