using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Extensions;

public class MatchTests
{
    [Fact]
    public void Match_Typed_Returns_OnUnsuccessful_With_Messages()
    {
        // Arrange
        var result = Results.Failure<int>("nope");

        // Act
        var branch = result.Match(
            onUnsuccessful: msgs => $"fail:{msgs.Count}",
            onNoValue: () => "novalue",
            onValue: _ => "value");

        // Assert
        branch.ShouldBe("fail:1");
    }

    [Fact]
    public void Match_Typed_Returns_OnNoValue_When_Success_And_Null()
    {
        // Arrange
        var result = Results.Success<string>();

        // Act
        var branch = result.Match(
            onUnsuccessful: _ => "fail",
            onNoValue: () => "novalue",
            onValue: _ => "value");

        // Assert
        branch.ShouldBe("novalue");
    }

    [Fact]
    public void Match_Typed_Returns_OnValue_When_Success_With_Value()
    {
        // Arrange
        var result = Results.Success("abc");

        // Act
        var branch = result.Match(
            onUnsuccessful: _ => "fail",
            onNoValue: () => "novalue",
            onValue: s => s.ToUpperInvariant());

        // Assert
        branch.ShouldBe("ABC");
    }

    [Fact]
    public void MatchValue_Typed_Produces_Result_From_Branches()
    {
        // Arrange
        var result = Results.Success("abc");

        // Act
        var mapped = result.MatchValue(
            onUnsuccessful: _ => Results.Failure<int>("bad"),
            onNoValue: () => Results.NoContent<int>(),
            onValue: s => Results.Success(s.Length));

        // Assert
        mapped.IsSuccess().ShouldBeTrue();
        mapped.Value.ShouldBe(3);
    }

    [Fact]
    public void Match_UnTyped_Returns_Success_Or_Failure_Branch()
    {
        // Arrange
        var ok = Results.Success();
        var bad = Results.Failure("x");

        // Act
        var a = ok.Match(onUnsuccessful: _ => "fail", onSuccess: () => "ok");
        var b = bad.Match(onUnsuccessful: _ => "fail", onSuccess: () => "ok");

        // Assert
        a.ShouldBe("ok");
        b.ShouldBe("fail");
    }

    [Fact]
    public void Match_Typed_Returns_OnNoValue_When_Success_And_Value_Null()
    {
        // Arrange
        var result = Results.Success<string?>(value: null);

        // Act
        var branch = result.Match(
            onUnsuccessful: () => "unsuccessful",
            onNoValue: () => "no-value",
            onValue: _ => "value");

        // Assert
        branch.ShouldBe("no-value");
    }

    [Fact]
    public void Match_Typed_WithMessages_Returns_Unsuccessful_Branch()
    {
        // Arrange
        var result = Results.Failure<string>("bad");

        // Act
        var msg = result.Match(
            onUnsuccessful: m => string.Join("|", m.Select(x => x.Content)),
            onNoValue: () => "no",
            onValue: _ => "ok");

        // Assert
        msg.ShouldBe("bad");
    }

    [Fact]
    public void Match_Untyped_Returns_Success_Branch()
    {
        // Arrange
        var result = Results.Success();

        // Act
        var v = result.Match(onUnsuccessful: () => 0, onSuccess: () => 1);

        // Assert
        v.ShouldBe(1);
    }

    [Fact]
    public void MatchValue_Typed_To_Result_Produces_New_Result()
    {
        // Arrange
        var result = Results.Success(5);

        // Act
        var mapped = result.MatchValue(
            onUnsuccessful: _ => Results.Failure<int>("nope"),
            onNoValue: () => Results.NoContent<int>(),
            onValue: x => Results.Success(x * 2));

        // Assert
        mapped.IsSuccess().ShouldBeTrue();
        mapped.Value.ShouldBe(10);
    }

    [Fact]
    public void Match_Actions_Invokes_Unsuccessful_Branch_When_Not_Success()
    {
        // Arrange
        var result = Results.Failure("no");
        var flag = 0;

        // Act
        result.Match(onUnsuccessful: _ => flag = 1, onSuccess: () => flag = 2);

        // Assert
        flag.ShouldBe(1);
    }

    [Fact]
    public void Match_Actions_Invokes_Success_Branch_When_Success()
    {
        // Arrange
        var result = Results.Success();
        var flag = 0;

        // Act
        result.Match(onUnsuccessful: () => flag = -1, onSuccess: () => flag = 3);

        // Assert
        flag.ShouldBe(3);
    }

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
    public void Match_Typed_WithMessages_Should_Route_Unsuccessful()
    {
        // Arrange
        var result = Results.Failure<int>("oops");

        // Act
        var branch = result.Match(
            onUnsuccessful: messages => $"fail:{messages[0].Content}",
            onNoValue: () => "none",
            onValue: _ => "value"
        );

        // Assert
        branch.ShouldBe("fail:oops");
    }

    [Fact]
    public void Match_Typed_WithMessages_Should_Route_NoValue()
    {
        // Arrange
        var result = Results.Success<string?>();

        // Act
        var branch = result.Match(
            onUnsuccessful: _ => "fail",
            onNoValue: () => "none",
            onValue: _ => "value"
        );

        // Assert
        branch.ShouldBe("none");
    }

    [Fact]
    public void Match_Typed_WithMessages_Should_Route_Value()
    {
        // Arrange
        var result = Results.Success("abc");

        // Act
        var branch = result.Match(
            onUnsuccessful: _ => "fail",
            onNoValue: () => "none",
            onValue: v => v.ToUpperInvariant()
        );

        // Assert
        branch.ShouldBe("ABC");
    }

    [Fact]
    public void Match_Typed_NoMessages_Should_Route_All_Paths()
    {
        // Arrange
        var fail = Results.Failure<int>("bad");
        var none = Results.Success<string?>();
        var ok = Results.Success("x");

        // Act
        var f = fail.Match(onUnsuccessful: () => "F", onNoValue: () => "N", onValue: _ => "V");
        var n = none.Match(onUnsuccessful: () => "F", onNoValue: () => "N", onValue: _ => "V");
        var v = ok.Match(onUnsuccessful: () => "F", onNoValue: () => "N", onValue: _ => "V");

        // Assert
        f.ShouldBe("F");
        n.ShouldBe("N");
        v.ShouldBe("V");
    }

    [Fact]
    public void MatchValue_Typed_To_Typed_Should_Produce_Failure_On_Unsuccessful()
    {
        // Arrange
        var result = Results.Failure<int>("nope");

        // Act
        var outResult = result.MatchValue(
            onUnsuccessful: msgs => Results.Failure<string>(msgs),
            onNoValue: () => Results.Success("empty"),
            onValue: v => Results.Success(v.ToString())
        );

        // Assert
        outResult.IsSuccess().ShouldBeFalse();
        outResult.Messages[0].Content.ShouldBe("nope");
    }

    [Fact]
    public void MatchValue_Typed_To_Untyped_Should_Choose_NoValue()
    {
        // Arrange
        var result = Results.Success<string?>();

        // Act
        var outResult = result.MatchValue(
            onUnsuccessful: _ => Results.Error("x"),
            onNoValue: () => Results.Success(),
            onValue: _ => Results.Failure("unexpected")
        );

        // Assert
        outResult.IsSuccess().ShouldBeTrue();
    }
}
