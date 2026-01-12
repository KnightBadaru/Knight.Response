using Knight.Response.Extensions;
using Knight.Response.Factories;
using Shouldly;

namespace Knight.Response.Tests.Extensions;

public class ValueBetweenExclusiveTests
{
    [Theory]
    [InlineData(5, 1, 10, true)]
    [InlineData(1, 1, 10, false)]
    [InlineData(10, 1, 10, false)]
    public void ValueBetweenExclusive_Works(int value, int min, int max, bool expected)
    {
        Results.Success(value)
            .ValueBetweenExclusive(min, max)
            .ShouldBe(expected);
    }
}
