using System.ComponentModel.DataAnnotations;
using Knight.Response.Core;
using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;
using static Knight.Response.Tests.Infrastructure.MessageBuilder;

namespace Knight.Response.Tests.Extensions;

public class ResultExtensionsTests
{
    [Fact]
    public void OnSuccess_Should_Invoke_Action_When_Success()
    {
        // Arrange
        var called = false;

        // Act
        var result = Results.Success().OnSuccess(() => called = true);

        // Assert
        called.ShouldBeTrue();
        result.Status.ShouldBe(Status.Completed);
    }

    [Fact]
    public void OnSuccess_Generic_Should_Pass_Value()
    {
        // Arrange
        string? received = null;
        const string value = "knight";

        // Act
        var result = Results.Success(value).OnSuccess(v => received = v);

        // Assert
        received.ShouldBe(value);
        result.IsSuccess().ShouldBeTrue();
    }

    [Fact]
    public void OnFailure_Should_Invoke_Action_When_Failed()
    {
        // Arrange
        var called = false;

        // Act
        var result = Results.Failure("shrugs").OnFailure(_ => called = true);

        // Assert
        called.ShouldBeTrue();
        result.Status.ShouldBe(Status.Failed);
    }

    [Fact]
    public void OnFailure_Generic_Should_Pass_Messages()
    {
        // Arrange
        var count = 0;

        // Act
        var result = Results.Error("err").OnFailure(messages => count = messages.Count);

        // Assert
        count.ShouldBe(1);
        result.Status.ShouldBe(Status.Error);
    }

    [Fact]
    public void Map_On_Success_Should_Transform_Value()
    {
        // Act
        var mapped = Results.Success(2).Map(x => x * 5);

        // Assert
        mapped.Value.ShouldBe(10);
        mapped.IsSuccess().ShouldBeTrue();
    }

    [Fact]
    public void Map_On_Failure_Should_Propagate_Failure()
    {
        // Arrange
        const string value = "fail";

        // Act
        var mapped = Results.Failure<int>("fail")
            .Map(x => x + 1);

        // Assert
        mapped.IsSuccess().ShouldBeFalse();
        mapped.Status.ShouldBe(Status.Failed);
        mapped.Messages.ShouldHaveSingleItem();
        mapped.Messages.ShouldContain(m => m.Type == MessageType.Error && m.Content == value);
    }

    [Fact]
    public void Map_On_Success_String_Should_Handle_Null_With_Coalesce()
    {
        // Arrange
        const string fallback = "fallback";

        // Act
        var mapped = Results.Success<string?>().Map(s => s ?? fallback);

        // Assert
        mapped.IsSuccess().ShouldBeTrue();
        mapped.Value.ShouldBe(fallback);
    }

    [Fact]
    public void Bind_On_Success_Should_Return_Binder_Result()
    {
        // Act
        var bound = Results.Success("u").Bind(s => Results.Success(s + "v"));

        // Assert
        bound.IsSuccess().ShouldBeTrue();
        bound.Value.ShouldBe("uv");
    }

    [Fact]
    public void Bind_On_Failure_Should_Propagate_Messages()
    {
        // Arrange
        const string reason = "failed";

        // Act
        var bound = Results.Failure<string>(reason).Bind(_ => Results.Success("x"));

        // Assert
        bound.IsSuccess().ShouldBeFalse();
        bound.Messages.ShouldHaveSingleItem();
        bound.Messages.ShouldContain(m => m.Type == MessageType.Error && m.Content == reason);
    }

    [Fact]
    public void OnFailure_When_Not_Successful_Should_Invoke_Action()
    {
        // Arrange
        var invoked = false;
        IReadOnlyList<Message>? captured = null;
        const string messageContent = "something broke, oops";
        var failure = Results.Failure<int>(new List<Message>
        {
            new(MessageType.Error, messageContent)
        });

        // Act
        var returned = failure.OnFailure(messages =>
        {
            invoked = true;
            captured = messages;
        });

        // Assert
        invoked.ShouldBeTrue();
        returned.ShouldBe(failure);
        captured.ShouldNotBeNull();
        captured.ShouldHaveSingleItem();
        captured.ShouldContain(m => m.Type == MessageType.Error && m.Content == messageContent);
    }

    [Fact]
    public void OnFailure_When_Successful_Should_Not_Invoke_Action()
    {
        // Arrange
        var invoked = false;
        var success = Results.Success(42);

        // Act
        var returned = success.OnFailure(_ => invoked = true);

        // Assert
        invoked.ShouldBeFalse();
        returned.ShouldBe(success);

    }

    // -------- Ensure --------

    [Fact]
    public void Ensure_When_Success_And_Predicate_True_Should_Return_Original_Success()
    {
        // Arrange
        var original = Results.Success(10);

        // Act
        var ensured = original.Ensure(v => v == 10, "must be 10");

        // Assert
        ensured.IsSuccess().ShouldBeTrue();
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
        ensured.IsSuccess().ShouldBeFalse();
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
        ensured.IsSuccess().ShouldBeFalse();
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
        tappedResult.IsSuccess().ShouldBeTrue();
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
        result.IsSuccess().ShouldBeFalse();
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
        recovered.IsSuccess().ShouldBeTrue();
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
        recovered.IsSuccess().ShouldBeTrue();
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
        result.IsSuccess().ShouldBeTrue();
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
        result.IsSuccess().ShouldBeTrue();
    }

    // v2

    [Fact]
    public void IsUnsuccessful_False_On_Completed()
    {
        // Arrange
        var result = Results.Success();

        // Act
        var actual = result.IsUnsuccessful();

        // Assert
        actual.ShouldBeFalse();
    }

    [Theory]
    [InlineData(Status.Failed)]
    [InlineData(Status.Error)]
    [InlineData(Status.Cancelled)]
    public void IsUnsuccessful_True_On_NonCompleted(Status status)
    {
        // Arrange
        var result = new Result(status);

        // Act
        var actual = result.IsUnsuccessful();

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public void ValueIsNull_True_When_Value_Is_Null()
    {
        // Arrange
        var result = Results.Success<string?>();

        // Act
        var actual = result.ValueIsNull();

        // Assert
        actual.ShouldBeTrue();
    }

    //

    [Fact]
    public void Match_Unsuccessful_PassesMessages()
    {
        // Arrange
        var result = Results.Failure<string>("boom");

        // Act
        var value = result.Match(
            msgs =>
            {
                msgs[0].Content.ShouldBe("boom");
                return "X";
            },
            () => "no value",
            v => v);

        // Assert
        value.ShouldBe("X");
    }

    [Fact]
    public void Match_NoValue_Branch()
    {
        // Arrange
        var result = Results.Success<string?>();

        // Act
        var value = result.Match(
            onUnsuccessful: _ => "bad",
            onNoValue: () => "empty",
            onValue: v => v ?? "value");

        // Assert
        value.ShouldBe("empty");
    }

    [Fact]
    public void Match_WithValue_Branch()
    {
        // Arrange
        var result = Results.Success("ok");

        // Act
        var value = result.Match(
            _ => "bad",
            () => "empty",
            v => v.ToUpper());

        // Assert
        value.ShouldBe("OK");
    }

    [Fact]
    public void MatchValue_Returns_Result_From_Branches()
    {
        // Arrange
        var result = Results.Success("a");

        // Act
        var output = result.MatchValue(
            onUnsuccessful: msgs => Results.Failure<int>(msgs),
            onNoValue: () => Results.Cancel<int>("empty"),
            onValue: v => Results.Success(v.Length));

        // Assert
        output.Status.ShouldBe(Status.Completed);
        output.Value.ShouldBe(1);
    }

    [Fact]
    public void WithDetail_Attaches_To_Last_Message_And_Is_CaseInsensitive()
    {
        // Arrange
        var original = Results.Failure([
            new Message(MessageType.Error, "E1"),
            new Message(MessageType.Error, "E2", new Dictionary<string, object?> { ["FIELD"] = "X" })
        ]);

        // Act
        var updated = original.WithDetail("field", "Y");

        // Assert
        original.Messages[1].Metadata["FIELD"].ShouldBe("X"); // unchanged
        updated.Messages[1].Metadata["field"].ShouldBe("Y");  // overwritten
    }

    [Fact]
    public void TryGetValidationResults_Finds_Single_And_Many()
    {
        // Arrange
        var vr1 = new ValidationResult("A", ["F1"]);
        var vr2 = new ValidationResult("B");

        var messages = new List<Message>
        {
            new(MessageType.Error, "e1", new Dictionary<string, object?> { ["ValidationResult"] = vr1 }),
            new(MessageType.Error, "e2", new Dictionary<string, object?> { ["ValidationResults"] = new [] { vr2 } })
        };

        var result = new Result(Status.Error, messages: messages);

        // Act
        var success = result.TryGetValidationResults(out var extracted);

        // Assert
        success.ShouldBeTrue();
        extracted.Select(e => e.ErrorMessage).ShouldBe(["A", "B"]);
    }
}
