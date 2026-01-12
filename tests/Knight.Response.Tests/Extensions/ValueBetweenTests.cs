using Knight.Response.Extensions;
using Knight.Response.Factories;
using Shouldly;

namespace Knight.Response.Tests.Extensions;

public class ValueBetweenTests
{
    [Theory]
    [InlineData(5, 1, 10, true)]
    [InlineData(1, 1, 10, true)]
    [InlineData(10, 1, 10, true)]
    [InlineData(0, 1, 10, false)]
    [InlineData(11, 1, 10, false)]
    public void ValueBetween_Inclusive_Works(int value, int min, int max, bool expected)
    {
        Results.Success(value)
            .ValueBetween(min, max)
            .ShouldBe(expected);
    }
}
