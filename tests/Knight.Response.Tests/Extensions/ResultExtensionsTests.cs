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
        result.IsSuccess.ShouldBeTrue();
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
        mapped.IsSuccess.ShouldBeTrue();
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
        mapped.IsSuccess.ShouldBeFalse();
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
        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe(fallback);
    }

    [Fact]
    public void Bind_On_Success_Should_Return_Binder_Result()
    {
        // Act
        var bound = Results.Success("u").Bind(s => Results.Success(s + "v"));

        // Assert
        bound.IsSuccess.ShouldBeTrue();
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
        bound.IsSuccess.ShouldBeFalse();
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
}
