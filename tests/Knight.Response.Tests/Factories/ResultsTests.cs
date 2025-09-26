using Knight.Response.Core;
using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Factories;

public class ResultsTests
{
    [Fact]
    public void FromCondition_True_Should_Return_Success()
    {
        // Act
        var result = Results.FromCondition(true, "should not see");

        // Assert
        result.IsSuccess().ShouldBeTrue();
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
        result.IsSuccess().ShouldBeFalse();
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
        result.IsSuccess().ShouldBeTrue();
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
        result.IsSuccess().ShouldBeFalse();
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
        aggregate.IsSuccess().ShouldBeTrue();
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
        aggregate.IsSuccess().ShouldBeFalse();
        aggregate.Status.ShouldBe(Status.Failed);
        aggregate.Messages.Select(m => m.Content).ShouldBe([fail, err]);
    }

    [Fact]
    public void Constructor_NullMessages_NormalizesToEmpty()
    {
        // Arrange
        IReadOnlyList<Message>? messages = null;

        // Act
        var result = new Result(Status.Completed, messages: messages);

        // Assert
        result.IsSuccess().ShouldBeTrue();
        result.Messages.ShouldNotBeNull();
        result.Messages.Count.ShouldBe(0);
    }

    [Fact]
    public void Constructor_UsesProvidedMessages()
    {
        // Arrange
        var messages = new List<Message> { new(MessageType.Information, "hi") };

        // Act
        var result = new Result(Status.Completed, messages: messages);

        // Assert
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].Content.ShouldBe("hi");
    }

    [Fact]
    public void Update_And_Delete_Are_Success_With_Correct_Codes()
    {
        // Arrange & Act
        var u = Results.Updated(message: "ok");
        var d = Results.Deleted();

        // Assert
        u.IsSuccess().ShouldBeTrue();
        u.Code.ShouldBe(ResultCodes.Updated);
        u.Messages[0].Content.ShouldBe("ok");

        d.IsSuccess().ShouldBeTrue();
        d.Code.ShouldBe(ResultCodes.Deleted);
        d.Messages.ShouldBeEmpty();
    }
}
