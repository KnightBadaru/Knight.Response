using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;
using static Knight.Response.Tests.Infrastructure.MessageBuilder;

namespace Knight.Response.Tests.Factories;

public class FailureTests
{
    [Fact]
    public void Failure_WithMessages_Should_Create_Failed_Result()
    {
        // Arrange
        var messages = new List<Message> { Error("fail-1"), Error("fail-2") };

        // Act
        var result = Results.Failure(messages);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Equal(Status.Failed, result.Status);
        Assert.Equal(2, result.Messages.Count);


        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Failed);
        result.Messages.ShouldNotBeEmpty();
        result.Messages.Count.ShouldBe(messages.Count);
        foreach (var message in messages)
        {
            // result.Messages.ShouldContain(message);
            result.Messages.ShouldContain(m =>
                m.Type == message.Type && m.Content == message.Content && m.Metadata == message.Metadata);
        }
    }

    [Fact]
    public void Failure_WithReason_Should_Create_Single_Error_Message()
    {
        // Act
        const string reason = "reason";
        var result = Results.Failure(reason);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Failed);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == reason && m.Type == MessageType.Error);
    }

    [Fact]
    public void Failure_Generic_WithMessages_Should_Preserve_Type()
    {
        // Arrange
        var messages = new List<Message> { Error() };

        // Act
        var result = Results.Failure<int?>(messages);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Failed);
        result.Messages.ShouldHaveSingleItem();
        result.Value.ShouldBeNull();
        result.Messages.ShouldContain(m => m.Content == messages.First().Content && m.Type == messages.First().Type);
    }

    [Fact]
    public void Failure_Generic_WithReason_Should_Create_Typed_Failed_Result()
    {
        // Act
        const string reason = "reason";
        var result = Results.Failure<string>(reason);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Status.ShouldBe(Status.Failed);
        result.Messages.ShouldHaveSingleItem();
        result.Value.ShouldBeNull();
        result.Messages.ShouldContain(m => m.Content == reason && m.Type == MessageType.Error);
    }

    [Fact]
    public void Failure_FromString_SetsFailed_AndErrorMessage()
    {
        // Arrange & Act
        var result = Results.Failure("boom");

        // Assert
        result.Status.ShouldBe(Status.Failed);
        result.Messages.Single().Content.ShouldBe("boom");
    }
}
