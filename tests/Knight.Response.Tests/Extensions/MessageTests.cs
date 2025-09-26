using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;
using static Knight.Response.Tests.Infrastructure.MessageBuilder;

namespace Knight.Response.Tests.Extensions;

public class MessageTests
{
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
        var result = Results.Failure("content")
            .WithMessages(Info("information"), Warn("warning"), Error("err"));

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

    [Fact]
    public void WithMessages_Params_Appends_To_Result()
    {
        // Arrange
        var r = Results.Success();
        var m1 = new Message(MessageType.Information, "a");
        var m2 = new Message(MessageType.Warning, "b");

        // Act
        var updated = r.WithMessages(m1, m2);

        // Assert
        updated.Messages.Count.ShouldBe(2);
        updated.Messages[0].Content.ShouldBe("a");
        updated.Messages[1].Content.ShouldBe("b");
    }

    [Fact]
    public void WithMessages_Params_Typed_Appends_To_Result()
    {
        // Arrange
        var r = Results.Success(1);
        var m = new Message(MessageType.Information, "x");

        // Act
        var updated = r.WithMessages(m);

        // Assert
        updated.Value.ShouldBe(1);
        updated.Messages.Count.ShouldBe(1);
    }

    [Fact]
    public void WithMessage_And_WithDetail_Should_Append_And_Enrich_Last_Message()
    {
        // Arrange
        var result = Results.Success()
            .WithMessage(new Message(MessageType.Information, "hello"));

        // Act
        var enriched = result.WithDetail("k", 123);

        // Assert
        enriched.Messages.Count.ShouldBe(1);
        enriched.Messages[0].Content.ShouldBe("hello");
        enriched.Messages[0].Metadata["k"].ShouldBe(123);
    }
}
