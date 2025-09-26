using Knight.Response.Core;
using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

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

    [Fact]
    public void IsUnsuccessfulOrNull_Returns_True_When_Unsuccessful()
    {
        // Arrange
        var r = Results.Failure<string>("bad");

        // Act
        var b = r.IsUnsuccessfulOrNull();

        // Assert
        b.ShouldBeTrue();
    }

    [Fact]
    public void IsUnsuccessfulOrNull_Returns_True_When_Success_But_Value_Null()
    {
        // Arrange
        var r = Results.Success<string>();

        // Act
        var b = r.IsUnsuccessfulOrNull();

        // Assert
        b.ShouldBeTrue();
    }

    [Fact]
    public void IsUnsuccessfulOrNull_Returns_False_When_Success_With_Value()
    {
        // Arrange
        var r = Results.Success("x");

        // Act
        var b = r.IsUnsuccessfulOrNull();

        // Assert
        b.ShouldBeFalse();
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

    [Fact]
    public void Map_On_Failure_Propagates_As_Failure_Of_New_Type()
    {
        // Arrange
        var fail = Results.Failure<int>("bad");

        // Act
        var mapped = fail.Map(x => x * 10);

        // Assert
        mapped.IsFailure().ShouldBeTrue();
        mapped.Messages[0].Content.ShouldBe("bad");
    }

    [Fact]
    public void Bind_On_Failure_Propagates_Failure()
    {
        // Arrange
        var fail = Results.Failure<string>("no");

        // Act
        var bound = fail.Bind(_ => Results.Success(123));

        // Assert
        bound.IsFailure().ShouldBeTrue();
        bound.Messages[0].Content.ShouldBe("no");
    }

    [Fact]
    public void Ensure_False_Turns_Into_Failure()
    {
        // Arrange
        var ok = Results.Success(5);

        // Act
        var ensured = ok.Ensure(v => v > 10, "too small");

        // Assert
        ensured.IsFailure().ShouldBeTrue();
        ensured.Messages[0].Content.ShouldBe("too small");
    }

    [Fact]
    public void Tap_Runs_Only_On_Success()
    {
        // Arrange
        var counter = 0;
        var ok = Results.Success(1);
        var bad = Results.Failure<int>("oops");

        // Act
        ok.Tap(_ => counter++);
        bad.Tap(_ => counter++);

        // Assert
        counter.ShouldBe(1);
    }

    [Fact]
    public void Recover_Turns_Failure_Into_Success_With_Fallback()
    {
        // Arrange
        var bad = Results.Failure<int>("x");

        // Act
        var recovered = bad.Recover(_ => 42);

        // Assert
        recovered.IsSuccess().ShouldBeTrue();
        recovered.Value.ShouldBe(42);
    }

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
    public void IsUnsuccessfulOrNull_When_Unsuccessful_Should_Return_True()
    {
        // Arrange
        var result = Results.Failure<int>("bad");

        // Act
        var flag = result.IsUnsuccessfulOrNull();

        // Assert
        flag.ShouldBeTrue();
    }

    [Fact]
    public void IsUnsuccessfulOrNull_When_Success_And_Value_Is_Null_Should_Return_True()
    {
        // Arrange
        var result = Results.Success<string?>();

        // Act
        var flag = result.IsUnsuccessfulOrNull();

        // Assert
        flag.ShouldBeTrue();
    }

    [Fact]
    public void IsUnsuccessfulOrNull_When_Success_And_Value_NotNull_Should_Return_False()
    {
        // Arrange
        var result = Results.Success("ok");

        // Act
        var flag = result.IsUnsuccessfulOrNull();

        // Assert
        flag.ShouldBeFalse();
    }

    [Fact]
    public void ValueIsNull_Should_Reflect_Nullness_Only()
    {
        // Arrange
        var a = Results.Success<string?>();
        var b = Results.Success("value");

        // Act
        var aNull = a.ValueIsNull();
        var bNull = b.ValueIsNull();

        // Assert
        aNull.ShouldBeTrue();
        bNull.ShouldBeFalse();
    }

    [Fact]
    public void IsUnsuccessfulOrNull_Should_Throw_On_Null_Extension_Target()
    {
        // Arrange
        Result<string>? result = null;

        // Act
        Func<object?> act = () => result!.IsUnsuccessfulOrNull();

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void OnSuccess_Should_Invoke_Action_Only_On_Success()
    {
        // Arrange
        var invoked = 0;
        var ok = Results.Success();
        var bad = Results.Failure("bad");

        // Act
        ok.OnSuccess(() => invoked++);
        bad.OnSuccess(() => invoked++);

        // Assert
        invoked.ShouldBe(1);
    }

    [Fact]
    public void OnFailure_Should_Invoke_Action_Only_On_Failure()
    {
        // Arrange
        var invoked = 0;
        var ok = Results.Success();
        var bad = Results.Failure("bad");

        // Act
        ok.OnFailure(_ => invoked++);
        bad.OnFailure(_ => invoked++);

        // Assert
        invoked.ShouldBe(1);
    }

    [Fact]
    public void Map_Should_Map_On_Success_And_Propagate_Failure()
    {
        // Arrange
        var ok = Results.Success(2);
        var bad = Results.Failure<int>("nope");

        // Act
        var mappedOk = ok.Map(x => x * 5);
        var mappedBad = bad.Map(x => x * 5);

        // Assert
        mappedOk.IsSuccess().ShouldBeTrue();
        mappedOk.Value.ShouldBe(10);
        mappedBad.IsSuccess().ShouldBeFalse();
        mappedBad.Messages[0].Content.ShouldBe("nope");
    }

    [Fact]
    public void Bind_Should_Chain_On_Success_And_Propagate_Failure()
    {
        // Arrange
        var ok = Results.Success("ab");
        var bad = Results.Failure<string>("broken");

        // Act
        var boundOk = ok.Bind(v => Results.Success((v ?? "").ToUpperInvariant()));
        var boundBad = bad.Bind(v => Results.Success((v ?? "").ToUpperInvariant()));

        // Assert
        boundOk.IsSuccess().ShouldBeTrue();
        boundOk.Value.ShouldBe("AB");
        boundBad.IsSuccess().ShouldBeFalse();
        boundBad.Messages[0].Content.ShouldBe("broken");
    }
}
