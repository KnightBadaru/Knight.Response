using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Factories;

public class ServiceTests
{
    [Fact]
    public void ServiceUnavailable_Uses_Error_With_ServiceUnavailable_Code()
    {
        // Arrange & Act
        var result = Results.ServiceUnavailable("downstream");

        // Assert
        result.IsError().ShouldBeTrue();
        result.Code.ShouldBe(ResultCodes.ServiceUnavailable);
        result.Messages[0].Content.ShouldBe("downstream");
    }
}
