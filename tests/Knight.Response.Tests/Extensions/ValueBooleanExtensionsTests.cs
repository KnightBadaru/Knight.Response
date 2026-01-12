using Knight.Response.Extensions;
using Knight.Response.Factories;
using Shouldly;

namespace Knight.Response.Tests.Extensions;

public class ValueBooleanExtensionsTests
{
    [Theory]
    [InlineData(true,  true)]
    [InlineData(false, false)]
    public void ValueIsTrue_ReturnsExpected(bool value, bool expected)
    {
        var result = Results.Success(value);

        result.ValueIsTrue().ShouldBe(expected);
    }

    [Theory]
    [InlineData(true,  false)]
    [InlineData(false, true)]
    public void ValueIsFalse_ReturnsExpected(bool value, bool expected)
    {
        var result = Results.Success(value);

        result.ValueIsFalse().ShouldBe(expected);
    }

    [Fact]
    public void NullableBool_True_IsTrue()
    {
        Results.Success<bool?>(true).ValueIsTrue().ShouldBeTrue();
    }

    [Fact]
    public void NullableBool_Null_IsFalse()
    {
        Results.Success<bool?>().ValueIsTrue().ShouldBeFalse();
    }
}
