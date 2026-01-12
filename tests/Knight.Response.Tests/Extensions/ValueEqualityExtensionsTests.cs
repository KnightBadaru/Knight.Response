using Knight.Response.Extensions;
using Knight.Response.Factories;
using Shouldly;

namespace Knight.Response.Tests.Extensions;

public class ValueEqualityExtensionsTests
{
    [Fact]
    public void ValueEquals_ReturnsTrue_WhenEqual()
    {
        Results.Success(5).ValueEquals(5).ShouldBeTrue();
    }

    [Fact]
    public void ValueEquals_ReturnsFalse_WhenDifferent()
    {
        Results.Success(5).ValueEquals(6).ShouldBeFalse();
    }

    [Fact]
    public void ValueNotEquals_IsInverse()
    {
        Results.Success(5).ValueNotEquals(6).ShouldBeTrue();
        Results.Success(5).ValueNotEquals(5).ShouldBeFalse();
    }

    [Fact]
    public void ResultVsResult_Equality_Works()
    {
        var left  = Results.Success(10);
        var right = Results.Success(10);
        var diff  = Results.Success(11);

        left.ValueEquals(right).ShouldBeTrue();
        left.ValueEquals(diff).ShouldBeFalse();
    }
}
