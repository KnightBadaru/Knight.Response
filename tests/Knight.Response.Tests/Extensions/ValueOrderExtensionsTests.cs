using Knight.Response.Extensions;
using Knight.Response.Factories;
using Shouldly;

namespace Knight.Response.Tests.Extensions;

public class ValueOrderExtensionsTests
{
    [Fact]
    public void Ordering_ReturnsFalse_WhenValueIsNull()
    {
        Results.Success<int>()
            .ValueGreaterThan(5)
            .ShouldBeFalse();
    }
}
