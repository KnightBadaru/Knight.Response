using Knight.Response.Factories;
using Knight.Response.Models;
using static Knight.Response.Tests.Helpers.MessageBuilder;

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
        Assert.True(result.IsSuccess);
        Assert.Equal(Status.Completed, result.Status);
        Assert.Empty(result.Messages);
    }

    [Fact]
    public void Success_Generic_NoValue_Should_Be_Completed_And_Null()
    {
        // Act
        var result = Results.Success<string>();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Status.Completed, result.Status);
        Assert.Null(result.Value);
        Assert.Empty(result.Messages);
    }

    [Fact]
    public void Success_WithValue_Should_Contain_Value()
    {
        // Act
        const string value = "value";
        var result = Results.Success(value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
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
        Assert.True(result.IsSuccess);
        Assert.Single(result.Messages);
        Assert.Equal(text, result.Messages[0].Content);
    }

    [Fact]
    public void Success_Generic_WithMessages_Should_Include_Messages()
    {
        // Arrange
        var messages = new List<Message> { Info("ok") };

        // Act
        var result = Results.Success<string>(messages);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Messages);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Success_WithValue_And_Messages_Should_Populate_Both()
    {
        // Arrange
        const string value = "value";
        var messages = new List<Message> { Info("note") };

        // Act
        var result = Results.Success(value, messages);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
        Assert.Single(result.Messages);
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
    }

    [Fact]
    public void Failure_WithReason_Should_Create_Single_Error_Message()
    {
        // Act
        const string reason = "reason";
        var result = Results.Failure(reason);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Status.Failed, result.Status);
        Assert.Single(result.Messages);
        Assert.Equal(reason, result.Messages[0].Content);
        Assert.Equal(MessageType.Error, result.Messages[0].Type);
    }

    [Fact]
    public void Failure_Generic_WithMessages_Should_Preserve_Type()
    {
        // Arrange
        var messages = new List<Message> { Error() };

        // Act
        var result = Results.Failure<int>(messages);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Status.Failed, result.Status);
        Assert.Equal(default, result.Value);
    }

    [Fact]
    public void Failure_Generic_WithReason_Should_Create_Typed_Failed_Result()
    {
        // Act
        var result = Results.Failure<string>("reason");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Status.Failed, result.Status);
        Assert.Null(result.Value);
        Assert.Single(result.Messages);
    }

    // ---------- Error ----------

    [Fact]
    public void Error_WithMessages_Should_Create_Error_Result()
    {
        // Arrange
        var messages = new List<Message> { Error("oops") };

        // Act
        var result = Results.Error(messages);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Status.Error, result.Status);
        Assert.Single(result.Messages);
    }

    [Fact]
    public void Error_WithReason_Should_Default_To_Error_MessageType()
    {
        // Act
        const string reason = "reason";
        var r = Results.Error(reason);

        // Assert
        Assert.Equal(Status.Error, r.Status);
        Assert.Equal(MessageType.Error, r.Messages[0].Type);
        Assert.Equal(reason, r.Messages[0].Content);
    }

    [Fact]
    public void Error_Generic_WithMessages_Should_Create_Typed_Error()
    {
        // Arrange
        var messages = new List<Message> { Error() };

        // Act
        var result = Results.Error<string>(messages);

        // Assert
        Assert.Equal(Status.Error, result.Status);
        Assert.Null(result.Value);
        Assert.Single(result.Messages);
    }

    [Fact]
    public void Error_Generic_WithReason_Should_Create_Typed_Error()
    {
        // Act
        var result = Results.Error<int>("fail!");

        // Assert
        Assert.Equal(Status.Error, result.Status);
        Assert.Equal(default, result.Value);
        Assert.Single(result.Messages);
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
        Assert.Equal(Status.Error, result.Status);
        Assert.Contains(result.Messages, m => m.Content == message);
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
        Assert.Equal(Status.Error, result.Status);
        Assert.Contains(result.Messages, m => m.Content == message);
    }

    // ---------- Cancel ----------

    [Fact]
    public void Cancel_WithMessages_Should_Set_Cancelled_Status()
    {
        // Arrange
        var messages = new List<Message> { Warn("user cancelled") };

        // Act
        var result = Results.Cancel(messages);

        // Assert
        Assert.Equal(Status.Cancelled, result.Status);
        Assert.False(result.IsSuccess);
        Assert.Single(result.Messages);
    }

    [Fact]
    public void Cancel_WithReason_Should_Default_To_Warning()
    {
        // Act
        const string reason = "stop";
        var result = Results.Cancel(reason);

        // Assert
        Assert.Equal(Status.Cancelled, result.Status);
        Assert.Equal(MessageType.Warning, result.Messages[0].Type);
        Assert.Equal(reason, result.Messages[0].Content);
    }

    [Fact]
    public void Cancel_Generic_Should_Create_Typed_Cancel_Result()
    {
        // Act
        var result = Results.Cancel<string>("halt");

        // Assert
        Assert.Equal(Status.Cancelled, result.Status);
        Assert.Null(result.Value);
        Assert.Single(result.Messages);
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
        Assert.Equal(Status.Cancelled, result.Status);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Single(result.Messages);
        Assert.Equal(messageContent, result.Messages[0].Content);
        Assert.Equal(MessageType.Warning, result.Messages[0].Type);
    }

    [Fact]
    public void Cancel_Generic_WithMessages_ValueType_Should_Have_Default()
    {
        // Act
        var result = Results.Cancel<int>(new List<Message> { new(MessageType.Warning, "stop") });

        // Assert
        Assert.Equal(Status.Cancelled, result.Status);
        Assert.False(result.IsSuccess);
        Assert.Equal(default, result.Value);
        Assert.Single(result.Messages);
    }

    // ---------- NotFound ----------

    [Fact]
    public void NotFound_Default_Should_Be_Completed_With_Warning()
    {
        // Act
        var result = Results.NotFound();

        // Assert
        Assert.Equal(Status.Completed, result.Status);
        Assert.Single(result.Messages);
        Assert.Equal(MessageType.Warning, result.Messages[0].Type);
    }

    [Fact]
    public void NotFound_Generic_With_Override_Status()
    {
        // Act
        const string messageContent = "not user";
        var result = Results.NotFound<string>(messageContent, Status.Failed);

        // Assert
        Assert.Equal(Status.Failed, result.Status);
        Assert.Single(result.Messages);
        Assert.Equal(messageContent, result.Messages[0].Content);
        Assert.Equal(MessageType.Warning, result.Messages[0].Type);
        Assert.Null(result.Value);
    }

    // ---------- FromCondition / Aggregate ----------

    [Fact]
    public void FromCondition_True_Should_Return_Success()
    {
        // Act
        var result = Results.FromCondition(true, "should not see");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Messages);
    }

    [Fact]
    public void FromCondition_False_Should_Return_Failure_With_Message()
    {
        // Act
        const string messageContent = "err";
        var result = Results.FromCondition(false, messageContent);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Status.Failed, result.Status);
        Assert.Single(result.Messages);
        Assert.Equal(messageContent, result.Messages[0].Content);
    }

    [Fact]
    public void FromCondition_Generic_Should_Return_Value_On_True()
    {
        // Act
        var result = Results.FromCondition(true, 7, "err");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value);
    }

    [Fact]
    public void FromCondition_Generic_False_Should_Return_Failure_With_Message()
    {
        // Act
        const string messageContent = "no way!";
        var result = Results.FromCondition<string>(false, "value-should-not-appear", messageContent);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Status.Failed, result.Status);
        Assert.Single(result.Messages);
        Assert.Equal(messageContent, result.Messages[0].Content);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Aggregate_All_Success_Should_Return_Success()
    {
        // Arrange
        var results = new[] { Results.Success(), Results.Success() };

        // Act
        var aggregate = Results.Aggregate(results);

        // Assert
        Assert.True(aggregate.IsSuccess);
        Assert.Empty(aggregate.Messages);
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
        Assert.False(aggregate.IsSuccess);
        Assert.Equal(Status.Failed, aggregate.Status);
        Assert.Equal(new[] { fail, err }, aggregate.Messages.Select(m => m.Content));
    }
}
