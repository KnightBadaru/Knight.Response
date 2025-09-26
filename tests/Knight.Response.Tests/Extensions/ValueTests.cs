using Knight.Response.Extensions;
using Knight.Response.Factories;
using Shouldly;

namespace Knight.Response.Tests.Extensions;

public class ValueTests
{
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

    [Fact]
    public void ValueIsNullOrEmpty_True_When_Success_But_Empty_Collection()
    {
        // Arrange
        var result = Results.Success(Array.Empty<int>());

        // Act
        var b = result.ValueIsNullOrEmpty<int[], int>();

        // Assert
        b.ShouldBeTrue();
    }

    [Fact]
    public void ValueIsNullOrEmpty_False_When_Success_And_Has_Items()
    {
        // Arrange
        var result = Results.Success(new[] { 1 });

        // Act
        var b = result.ValueIsNullOrEmpty<int[], int>();

        // Assert
        b.ShouldBeFalse();
    }

    [Fact]
    public void ValueIsNullOrWhiteSpace_True_When_Success_But_Whitespace()
    {
        // Arrange
        var result = Results.Success("   ");

        // Act
        var b = result.ValueIsNullOrWhiteSpace();

        // Assert
        b.ShouldBeTrue();
    }

    [Fact]
    public void ValueIsNullOrWhiteSpace_False_When_Success_And_Text()
    {
        // Arrange
        var result = Results.Success("ok");

        // Act
        var b = result.ValueIsNullOrWhiteSpace();

        // Assert
        b.ShouldBeFalse();
    }

    [Fact]
    public void GetValueOrDefault_Returns_Fallback_On_Failure()
    {
        // Arrange
        var result = Results.Failure<int>("nope");

        // Act
        var v = result.GetValueOrDefault(99);

        // Assert
        v.ShouldBe(99);
    }

    [Fact]
    public void TryGetValue_Returns_True_And_Value_On_Success()
    {
        // Arrange
        var result = Results.Success(7);

        // Act
        var ok = result.TryGetValue(out var v);

        // Assert
        ok.ShouldBeTrue();
        v.ShouldBe(7);
    }

    [Fact]
    public void TryGetValue_Returns_False_On_Failure()
    {
        // Arrange
        var result = Results.Failure<int>("bad");

        // Act
        var ok = result.TryGetValue(out var v);

        // Assert
        ok.ShouldBeFalse();
        v.ShouldBe(default);
    }

    [Fact]
    public void GetValueOrThrow_Throws_On_Failure()
    {
        // Arrange
        var result = Results.Failure<int>("boom");

        // Act
        Action act = () => result.GetValueOrThrow(_ => new InvalidOperationException("x"));

        // Assert
        act.ShouldThrow<InvalidOperationException>().Message.ShouldBe("x");
    }

    [Fact]
    public void GetValueOrDefault_TryGetValue_GetValueOrThrow_Should_Work_As_Documented()
    {
        // Arrange
        var ok = Results.Success(7);
        var bad = Results.Failure<int>("bad");

        // Act
        var v1 = ok.GetValueOrDefault(42);
        var v2 = bad.GetValueOrDefault(42);

        var tryOk = ok.TryGetValue(out var out1);
        var tryBad = bad.TryGetValue(out var out2);

        Func<object?> throwAct = () => bad.GetValueOrThrow(_ => new InvalidOperationException("nope"));

        // Assert
        v1.ShouldBe(7);
        v2.ShouldBe(42);

        tryOk.ShouldBeTrue();
        out1.ShouldBe(7);

        tryBad.ShouldBeFalse();
        out2.ShouldBe(0); // default(int)

        throwAct.ShouldThrow<InvalidOperationException>().Message.ShouldBe("nope");
    }

    [Fact]
    public void ValueIsNullOrEmpty_Returns_True_For_Empty_List()
    {
        // Arrange
        var result = Results.Success(new List<int>());

        // Act
        var isEmpty = result.ValueIsNullOrEmpty<List<int>, int>();

        // Assert
        isEmpty.ShouldBeTrue();
    }

    [Fact]
    public void ValueIsNullOrWhiteSpace_Returns_True_For_Whitespace_String()
    {
        // Arrange
        var result = Results.Success("   ");

        // Act
        var isEmpty = result.ValueIsNullOrWhiteSpace();

        // Assert
        isEmpty.ShouldBeTrue();
    }

    [Fact]
    public void ValueIsNullOrWhiteSpace_Returns_False_For_NonEmpty_String()
    {
        // Arrange
        var result = Results.Success("Knight");

        // Act
        var isEmpty = result.ValueIsNullOrWhiteSpace();

        // Assert
        isEmpty.ShouldBeFalse();
    }
}
