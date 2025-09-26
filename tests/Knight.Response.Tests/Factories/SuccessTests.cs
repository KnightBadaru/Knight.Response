using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;
using static Knight.Response.Tests.Infrastructure.MessageBuilder;

namespace Knight.Response.Tests.Factories;

public class SuccessTests
{
    [Fact]
    public void Success_Should_Create_Completed_Result()
    {
        // Act
        var result = Results.Success();

        // Assert
        result.IsSuccess().ShouldBeTrue();
        result.Status.ShouldBe(Status.Completed);
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void Success_Generic_NoValue_Should_Be_Completed_And_Null()
    {
        // Act
        var result = Results.Success<string>();

        // Assert
        result.IsSuccess().ShouldBeTrue();
        result.Status.ShouldBe(Status.Completed);
        result.Value.ShouldBeNull();
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void Success_WithValue_Should_Contain_Value()
    {
        // Act
        const string value = "value";
        var result = Results.Success(value);

        // Assert
        result.IsSuccess().ShouldBeTrue();
        result.Status.ShouldBe(Status.Completed);
        result.Value.ShouldBe(value);
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void Success_WithMessages_Should_Include_Messages()
    {
        // Arrange
        const string text = "ok";
        var messages = new List<Message> { Info(text) };

        // Act
        var result = Results.Success(messages: messages);

        // Assert
        result.IsSuccess().ShouldBeTrue();
        result.Status.ShouldBe(Status.Completed);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == text && m.Type == MessageType.Information);
    }

    [Fact]
    public void Success_Generic_WithMessages_Should_Include_Messages()
    {
        // Arrange
        const string text = "word";
        var messages = new List<Message> { Info(text) };

        // Act
        var result = Results.Success<string>(messages);

        // Assert
        result.IsSuccess().ShouldBeTrue();
        result.Status.ShouldBe(Status.Completed);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == text && m.Type == MessageType.Information);
    }

    [Fact]
    public void Success_WithValue_And_Messages_Should_Populate_Both()
    {
        // Arrange
        const string value = "value";
        const string message = "word";
        var messages = new List<Message> { Info(message) };

        // Act
        var result = Results.Success(value, messages);

        // Assert
        result.IsSuccess().ShouldBeTrue();
        result.Status.ShouldBe(Status.Completed);
        result.Messages.ShouldHaveSingleItem();
        result.Value.ShouldBe(value);
        result.Messages.ShouldContain(m => m.Content == message && m.Type == MessageType.Information);
    }

    [Fact]
    public void Success_T_SetsCompleted_AndValue_AndOptionalCode()
    {
        // Arrange & Act
        var result = Results.Success(42, code: new ResultCode("Created"));

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.Code?.Value.ShouldBe("Created");
        result.Value.ShouldBe(42);
        result.Messages.ShouldBeEmpty();
    }
}
