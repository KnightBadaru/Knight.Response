using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;

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
        Assert.True(called);
        Assert.Equal(Status.Completed, result.Status);
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
        Assert.Equal(value, received);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void OnFailure_Should_Invoke_Action_When_Failed()
    {
        // Arrange
        var called = false;

        // Act
        var r = Results.Failure("shrugs").OnFailure(_ => called = true);

        // Assert
        Assert.True(called);
        Assert.Equal(Status.Failed, r.Status);
    }

    [Fact]
    public void OnFailure_Generic_Should_Pass_Messages()
    {
        // Arrange
        var count = 0;

        // Act
        var result = Results.Error("err").OnFailure(messages => count = messages.Count);

        // Assert
        Assert.Equal(1, count);
        Assert.Equal(Status.Error, result.Status);
    }

    [Fact]
    public void Map_On_Success_Should_Transform_Value()
    {
        // Act
        var mapped = Results.Success(2).Map(x => x * 5);

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal(10, mapped.Value);
    }

    [Fact]
    public void Map_On_Failure_Should_Propagate_Failure()
    {
        // Act
        var mapped = Results.Failure<int>("fail").Map(x => x + 1);

        // Assert
        Assert.False(mapped.IsSuccess);
        Assert.Equal(Status.Failed, mapped.Status);
        Assert.Single(mapped.Messages);
    }

    [Fact]
    public void Map_On_Success_String_Should_Handle_Null_With_Coalesce()
    {
        // Arrange
        const string fallback = "fallback";

        // Act
        var mapped = Results.Success<string?>().Map(s => s ?? fallback);

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal(fallback, mapped.Value);
    }

    [Fact]
    public void Bind_On_Success_Should_Return_Binder_Result()
    {
        // Act
        var bound = Results.Success("u").Bind(s => Results.Success(s + "v"));

        // Assert
        Assert.True(bound.IsSuccess);
        Assert.Equal("uv", bound.Value);
    }

    [Fact]
    public void Bind_On_Failure_Should_Propagate_Messages()
    {
        // Arrange
        const string reason = "failed";

        // Act
        var bound = Results.Failure<string>(reason).Bind(_ => Results.Success("x"));

        // Assert
        Assert.False(bound.IsSuccess);
        Assert.Equal(reason, bound.Messages.Single().Content);
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
        Assert.True(invoked);
        Assert.Same(failure, returned);
        Assert.NotNull(captured);
        Assert.Single(captured!);
        Assert.Equal(messageContent, captured![0].Content);
        Assert.Equal(MessageType.Error, captured![0].Type);
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
        Assert.False(invoked);
        Assert.Same(success, returned);
    }
}
