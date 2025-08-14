using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using static Knight.Response.Tests.Helpers.MessageBuilder;

namespace Knight.Response.Tests.Extensions;

public class AdvancedResultExtensionsTests
{
    // -------- Ensure --------

    [Fact]
    public void Ensure_When_Success_And_Predicate_True_Should_Return_Original_Success()
    {
        // Arrange
        var original = Results.Success(10);

        // Act
        var ensured = original.Ensure(v => v == 10, "must be 10");

        // Assert
        Assert.True(ensured.IsSuccess);
        Assert.Equal(original.Value, ensured.Value);
        Assert.Empty(ensured.Messages);
    }

    [Fact]
    public void Ensure_When_Success_And_Predicate_False_Should_Return_Failure_With_Message()
    {
        // Arrange
        const string errorMessage = "not equal";

        // Act
        var ensured = Results.Success(3).Ensure(v => v == 4, errorMessage);

        // Assert
        Assert.False(ensured.IsSuccess);
        Assert.Equal(Status.Failed, ensured.Status);
        Assert.Equal(errorMessage, ensured.Messages.Single().Content);
    }

    [Fact]
    public void Ensure_When_Already_Failed_Should_Return_Original_Failure()
    {
        // Arrange
        const string reason = "failed";
        var original = Results.Failure<int>(reason);

        // Act
        var ensured = original.Ensure(_ => false, "ignored");

        // Assert
        Assert.False(ensured.IsSuccess);
        Assert.Equal(Status.Failed, ensured.Status);
        Assert.Equal(reason, ensured.Messages.Single().Content);
    }

    // -------- Tap --------

    [Fact]
    public void Tap_When_Success_Should_Invoke_Action_And_Preserve_Result()
    {
        // Arrange
        int tapped = 0;
        var original = Results.Success(7);

        // Act
        var tappedResult = original.Tap(_ => tapped++);

        // Assert
        Assert.Equal(1, tapped);
        Assert.Equal(original.Value, tappedResult.Value);
        Assert.True(tappedResult.IsSuccess);
        Assert.Empty(tappedResult.Messages);
    }

    [Fact]
    public void Tap_When_Failure_Should_Not_Invoke_Action()
    {
        // Arrange
        int tapped = 0;

        // Act
        var result = Results.Failure<int>("fail").Tap(_ => tapped++);

        // Assert
        Assert.Equal(0, tapped);
        Assert.False(result.IsSuccess);
    }

    // -------- Recover --------

    [Fact]
    public void Recover_When_Failure_Should_Return_Success_With_Fallback()
    {
        // Arrange
        const string recoveredValue = "fallback";

        // Act
        var recovered = Results.Failure<string>("missing").Recover(_ => recoveredValue);

        // Assert
        Assert.True(recovered.IsSuccess);
        Assert.Equal(recoveredValue, recovered.Value);
        // recovery returns success with no messages by design
        Assert.Empty(recovered.Messages);
    }

    [Fact]
    public void Recover_When_Success_Should_Return_Original()
    {
        // Arrange
        const string value = "value";
        var original = Results.Success(value);

        // Act
        var recovered = original.Recover(_ => "fallback");

        // Assert
        Assert.True(recovered.IsSuccess);
        Assert.Equal(value, recovered.Value);
    }

    // -------- WithMessages / WithMessage --------

    [Fact]
    public void WithMessage_On_NonGeneric_Should_Append_Single_Message()
    {
        // Act
        var result = Results.Failure("a").WithMessage(Info("b"));

        // Assert
        Assert.Equal(new[] { "a", "b" }, result.Messages.Select(m => m.Content).ToArray());
    }

    [Fact]
    public void WithMessage_On_Generic_Should_Append_Single_Message_And_Preserve_Value()
    {
        // Arrange
        const string messageContent = "warning";

        // Act
        var result = Results.Success(1).WithMessage(Warn(messageContent));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        Assert.Single(result.Messages);
        Assert.Equal(messageContent, result.Messages[0].Content);
    }

    [Fact]
    public void WithMessages_Params_On_NonGeneric_Should_Append_In_Order()
    {
        // Act
        var result = Results.Failure("content").WithMessages(Info("information"), Warn("warning"), Error("err"));

        // Assert
        Assert.Equal(new[] { "content", "information", "warning", "err" }, result.Messages.Select(m => m.Content));
    }

    [Fact]
    public void WithMessages_Enumerable_On_Generic_Should_Append_All()
    {
        // Arrange
        const string value = "val";

        // Act
        var result = Results.Success(value).WithMessages(new[] { Warn("a"), Warn("b") });

        // Assert
        Assert.Equal(new[] { "a", "b" }, result.Messages.Select(m => m.Content));
        Assert.Equal(value, result.Value);
        Assert.True(result.IsSuccess);
    }
}
