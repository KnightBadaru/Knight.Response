using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;
using static Knight.Response.Tests.Infrastructure.MessageBuilder;

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
        ensured.IsSuccess.ShouldBeTrue();
        ensured.Value.ShouldBe(original.Value);
        ensured.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void Ensure_When_Success_And_Predicate_False_Should_Return_Failure_With_Message()
    {
        // Arrange
        const string errorMessage = "not equal";

        // Act
        var ensured = Results.Success(3).Ensure(v => v == 4, errorMessage);

        // Assert
        ensured.IsSuccess.ShouldBeFalse();
        ensured.Status.ShouldBe(Status.Failed);
        ensured.Messages.Single().Content.ShouldBe(errorMessage);
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
        ensured.IsSuccess.ShouldBeFalse();
        ensured.Status.ShouldBe(Status.Failed);
        ensured.Messages.Single().Content.ShouldBe(reason);
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
        tapped.ShouldBe(1);
        tappedResult.Value.ShouldBe(original.Value);
        tappedResult.IsSuccess.ShouldBeTrue();
        tappedResult.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void Tap_When_Failure_Should_Not_Invoke_Action()
    {
        // Arrange
        int tapped = 0;

        // Act
        var result = Results.Failure<int>("fail").Tap(_ => tapped++);

        // Assert
        tapped.ShouldBe(0);
        result.IsSuccess.ShouldBeFalse();
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
        recovered.IsSuccess.ShouldBeTrue();
        recovered.Value.ShouldBe(recoveredValue);
        recovered.Messages.ShouldBeEmpty(); // recovery returns success with no messages by design
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
        recovered.IsSuccess.ShouldBeTrue();
        recovered.Value.ShouldBe(value);
    }

    // -------- WithMessages / WithMessage --------

    [Fact]
    public void WithMessage_On_NonGeneric_Should_Append_Single_Message()
    {
        // Act
        var result = Results.Failure("a").WithMessage(Info("b"));

        // Assert
        result.Messages.Select(m => m.Content).ShouldBe(["a", "b"]);
    }

    [Fact]
    public void WithMessage_On_Generic_Should_Append_Single_Message_And_Preserve_Value()
    {
        // Arrange
        const string messageContent = "warning";

        // Act
        var result = Results.Success(1).WithMessage(Warn(messageContent));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(1);
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].Content.ShouldBe(messageContent);
    }

    [Fact]
    public void WithMessages_Params_On_NonGeneric_Should_Append_In_Order()
    {
        // Act
        var result = Results.Failure("content").WithMessages(Info("information"), Warn("warning"), Error("err"));

        // Assert
        result.Messages.Select(m => m.Content)
              .ShouldBe(["content", "information", "warning", "err"]);
    }

    [Fact]
    public void WithMessages_Enumerable_On_Generic_Should_Append_All()
    {
        // Arrange
        const string value = "val";

        // Act
        var result = Results.Success(value).WithMessages(Warn("a"), Warn("b"));

        // Assert
        result.Messages.Select(m => m.Content).ShouldBe(["a", "b"]);
        result.Value.ShouldBe(value);
        result.IsSuccess.ShouldBeTrue();
    }
}
