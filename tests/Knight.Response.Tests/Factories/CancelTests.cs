using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;
using static Knight.Response.Tests.Infrastructure.MessageBuilder;

namespace Knight.Response.Tests.Factories;

public class CancelTests
{
    [Fact]
    public void Cancel_WithMessages_Should_Set_Cancelled_Status()
    {
        // Arrange
        const string message = "user cancelled";
        var messages = new List<Message> { Warn(message) };

        // Act
        var result = Results.Cancel(messages);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Cancelled);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == message && m.Type == MessageType.Warning);
    }

    [Fact]
    public void Cancel_WithReason_Should_Default_To_Warning()
    {
        // Arrange
        const string reason = "stop";

        // Act
        var result = Results.Cancel(reason);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Cancelled);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == reason && m.Type == MessageType.Warning);
    }

    [Fact]
    public void Cancel_Generic_Should_Create_Typed_Cancel_Result()
    {
        // Arrange
        const string reason = "halt";

        // Act
        var result = Results.Cancel<string>(reason);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Cancelled);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == reason && m.Type == MessageType.Warning);
    }

    [Fact]
    public void Cancel_Generic_WithMessages_Should_Set_Cancelled_And_Default_Value()
    {
        // Arrange
        const string messageContent = "user cancelled";
        var messages = new List<Message> { new(MessageType.Warning, messageContent) };

        // Act
        var result = Results.Cancel<string>(messages);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Cancelled);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == messageContent && m.Type == MessageType.Warning);
    }

    [Fact]
    public void Cancel_Generic_WithMessages_ValueType_Should_Have_Default()
    {
        // Arrange
        const string messageContent = "stop";

        // Act
        var result = Results.Cancel<int>(new List<Message> { new(MessageType.Warning, messageContent) });

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Cancelled);
        result.Value.ShouldBe(default);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == messageContent && m.Type == MessageType.Warning);
    }

    [Fact]
    public void Cancel_DefaultsToWarningMessage()
    {
        // Arrange & Act
        var result = Results.Cancel("cancelled");

        // Assert
        result.Status.ShouldBe(Status.Cancelled);
        result.Messages.Single().Type.ShouldBe(MessageType.Warning);
    }
}
