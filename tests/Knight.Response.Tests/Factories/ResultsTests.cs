using Knight.Response.Core;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;
using static Knight.Response.Tests.Infrastructure.MessageBuilder;

namespace Knight.Response.Tests.Factories;

public class ResultsTests
{
    // ---------- Success ----------

    [Fact]
    public void Success_Should_Create_Completed_Result()
    {
        // Act
        var result = Results.Success();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Status.ShouldBe(Status.Completed);
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void Success_Generic_NoValue_Should_Be_Completed_And_Null()
    {
        // Act
        var result = Results.Success<string>();

        // Assert
        result.IsSuccess.ShouldBeTrue();
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
        result.IsSuccess.ShouldBeTrue();
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
        result.IsSuccess.ShouldBeTrue();
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
        result.IsSuccess.ShouldBeTrue();
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
        result.IsSuccess.ShouldBeTrue();
        result.Status.ShouldBe(Status.Completed);
        result.Messages.ShouldHaveSingleItem();
        result.Value.ShouldBe(value);
        result.Messages.ShouldContain(m => m.Content == message && m.Type == MessageType.Information);
    }

    // ---------- Failure ----------

    [Fact]
    public void Failure_WithMessages_Should_Create_Failed_Result()
    {
        // Arrange
        var messages = new List<Message> { Error("fail-1"), Error("fail-2") };

        // Act
        var result = Results.Failure(messages);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Status.Failed, result.Status);
        Assert.Equal(2, result.Messages.Count);


        result.IsSuccess.ShouldBeFalse();
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
        result.IsSuccess.ShouldBeFalse();
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
        result.IsSuccess.ShouldBeFalse();
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
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(Status.Failed);
        result.Messages.ShouldHaveSingleItem();
        result.Value.ShouldBeNull();
        result.Messages.ShouldContain(m => m.Content == reason && m.Type == MessageType.Error);
    }

    // ---------- Error ----------

    [Fact]
    public void Error_WithMessages_Should_Create_Error_Result()
    {
        // Arrange
        const string content = "oops!";
        var messages = new List<Message> { Error(content) };

        // Act
        var result = Results.Error(messages);

        // Assert
        result.IsSuccess.ShouldBeFalse();
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
        result.IsSuccess.ShouldBeFalse();
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
        result.IsSuccess.ShouldBeFalse();
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
        result.IsSuccess.ShouldBeFalse();
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
        result.IsSuccess.ShouldBeFalse();
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
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(Status.Error);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == message && m.Type == MessageType.Error);
    }

    // ---------- Cancel ----------

    [Fact]
    public void Cancel_WithMessages_Should_Set_Cancelled_Status()
    {
        // Arrange
        const string message = "user cancelled";
        var messages = new List<Message> { Warn(message) };

        // Act
        var result = Results.Cancel(messages);

        // Assert
        result.IsSuccess.ShouldBeFalse();
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
        result.IsSuccess.ShouldBeFalse();
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
        result.IsSuccess.ShouldBeFalse();
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
        result.IsSuccess.ShouldBeFalse();
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
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(Status.Cancelled);
        result.Value.ShouldBe(default);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == messageContent && m.Type == MessageType.Warning);
    }

    // ---------- NotFound ----------

    [Fact]
    public void NotFound_Default_Should_Be_Completed_With_Warning()
    {
        // Act
        var result = Results.NotFound();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Status.ShouldBe(Status.Completed);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == "Resource not found." && m.Type == MessageType.Warning);
    }

    [Fact]
    public void NotFound_Generic_With_Override_Status()
    {
        // Act
        const string messageContent = "not user";
        var result = Results.NotFound<string>(messageContent, Status.Failed);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(Status.Failed);
        result.Value.ShouldBeNull();
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == messageContent && m.Type == MessageType.Warning);
    }

    // ---------- FromCondition / Aggregate ----------

    [Fact]
    public void FromCondition_True_Should_Return_Success()
    {
        // Act
        var result = Results.FromCondition(true, "should not see");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Status.ShouldBe(Status.Completed);
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void FromCondition_False_Should_Return_Failure_With_Message()
    {
        // Act
        const string messageContent = "err";
        var result = Results.FromCondition(false, messageContent);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(Status.Failed);
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == messageContent && m.Type == MessageType.Error);
    }

    [Fact]
    public void FromCondition_Generic_Should_Return_Value_On_True()
    {
        // Act
        const int value = 7;
        var result = Results.FromCondition(true, value, "err");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Status.ShouldBe(Status.Completed);
        result.Value.ShouldBe(value);
        result.Messages.ShouldBeEmpty();
        // result.Messages.ShouldContain(m => m.Content == content && m.Type == MessageType.Error);
    }

    [Fact]
    public void FromCondition_Generic_False_Should_Return_Failure_With_Message()
    {
        // Act
        const string messageContent = "no way!";
        var result = Results.FromCondition<string>(false, "value-should-not-appear", messageContent);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(Status.Failed);
        result.Value.ShouldBeNull();
        result.Messages.ShouldHaveSingleItem();
        result.Messages.ShouldContain(m => m.Content == messageContent && m.Type == MessageType.Error);
    }

    [Fact]
    public void Aggregate_All_Success_Should_Return_Success()
    {
        // Arrange
        var results = new[] { Results.Success(), Results.Success() };

        // Act
        var aggregate = Results.Aggregate(results);

        // Assert
        aggregate.IsSuccess.ShouldBeTrue();
        aggregate.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void Aggregate_Any_Failure_Should_Collect_Messages()
    {
        // Arrange
        const string fail = "fail";
        const string err = "error";
        var results = new[]
        {
            Results.Success(),
            Results.Failure(fail),
            Results.Error(err)
        };

        // Act
        var aggregate = Results.Aggregate(results);

        // Assert
        aggregate.IsSuccess.ShouldBeFalse();
        aggregate.Status.ShouldBe(Status.Failed);
        aggregate.Messages.Select(m => m.Content).ShouldBe([fail, err]);
    }

    // ---------- Constructor ----------

    [Fact]
    public void Constructor_NullMessages_NormalizesToEmpty()
    {
        // Arrange
        IReadOnlyList<Message>? messages = null;

        // Act
        var result = new Result(Status.Completed, messages);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Messages.ShouldNotBeNull();
        result.Messages.Count.ShouldBe(0);
    }

    [Fact]
    public void Constructor_UsesProvidedMessages()
    {
        // Arrange
        var messages = new List<Message> { new(MessageType.Information, "hi") };

        // Act
        var result = new Result(Status.Completed, messages);

        // Assert
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].Content.ShouldBe("hi");
    }
}
