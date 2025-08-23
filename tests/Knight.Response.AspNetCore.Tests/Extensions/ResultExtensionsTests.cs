using Knight.Response.AspNetCore.Extensions;
using Knight.Response.AspNetCore.Options;
using Knight.Response.AspNetCore.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Shouldly;

namespace Knight.Response.AspNetCore.Tests.Extensions;

public class ResultExtensionsTests
{
    [Fact]
    public async Task ToIResult_Success_Respects_IncludeFullResultPayload()
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = true };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results.Success(new { id = 1 }).ToIResult(http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status200OK);
        body.ShouldContain("\"status\":\"Completed\"");
        body.ShouldContain("\"id\":1");
    }

    [Fact]
    public async Task ToIResult_Failure_Uses_ProblemDetails_When_Enabled()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results.Failure("oops").ToIResult(http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);
        body.ShouldContain("\"type\":\"https://httpstatuses.io/400\"");
        body.ShouldContain("oops");
    }

    // -------------------- Created (201) --------------------

    [Fact]
    public async Task ToCreatedResult_Success_FullPayload_201_With_Location_And_Result()
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = true };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results.Success().ToCreatedResult(http, "/api/items/1");
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location.ToString().ShouldBe("/api/items/1");
        body.ShouldContain("\"status\":\"Completed\"");
    }

    [Fact]
    public async Task ToCreatedResult_Success_ValueOnly_For_Generic_When_FullPayloadFalse()
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results.Success(new { id = 7, name = "knight" })
            .ToCreatedResult(http, "/api/users/7");
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location.ToString().ShouldBe("/api/users/7");
        body.ShouldContain("\"id\":7");
        body.ShouldNotContain("\"status\""); // value-only
    }

    [Fact]
    public async Task ToCreatedResult_Success_NonGeneric_FullPayloadFalse_201_With_EmptyBody()
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results.Success()
            .ToCreatedResult(http, "/api/ping/1");
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location.ToString().ShouldBe("/api/ping/1");
        body.ShouldBe(string.Empty); // Created(location, null)
    }

    [Fact]
    public async Task ToCreatedResult_Failure_Uses_ClientError_And_ProblemDetails_When_Enabled()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results.Failure("bad")
            .ToCreatedResult(http, "/api/items");
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);
        body.ShouldContain("\"type\":\"https://httpstatuses.io/400\"");
        body.ShouldContain("bad");
    }

    [Fact]
    public async Task ToCreatedResult_Routes_To_201_And_Sets_Location_NonGeneric()
    {
        // Arrange
        const string location = "/api/items/5";
        var (http, _) = TestHost.CreateHttpContext(new KnightResponseOptions { IncludeFullResultPayload = true });

        // Act
        var ires = Knight.Response.Factories.Results.Success().ToCreatedResult(http, location);
        var (status, body, headers) = await TestHost.ExecuteAsync(ires, http);

        // Assert
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location.ToString().ShouldBe(location);
        body.ShouldContain("\"status\":\"Completed\"");
    }

    [Fact]
    public async Task ToCreatedResult_Generic_ValueOnly_When_FullPayloadFalse()
    {
        // Arrange
        const string location = "/api/things/11";
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var ires = Knight.Response.Factories.Results
            .Success(new { id = 11 })
            .ToCreatedResult(http, location);

        var (status, body, headers) = await TestHost.ExecuteAsync(ires, http);

        // Assert
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location.ToString().ShouldBe(location);
        body.ShouldContain("\"id\":11");
        body.ShouldNotContain("\"status\"");
    }

    // -------------------- Accepted (202) --------------------

    [Fact]
    public async Task ToAcceptedResult_Success_202_With_Location_And_Result_When_FullPayloadTrue()
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = true };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results.Success(new { ticket = "abc" })
            .ToAcceptedResult(http, "/status/abc");
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status202Accepted);
        headers.Location.ToString().ShouldBe("/status/abc");
        body.ShouldContain("\"ticket\":\"abc\"");
        body.ShouldContain("\"status\":\"Completed\"");
    }

    [Fact]
    public async Task ToAcceptedResult_Generic_ValueOnly_When_FullPayloadFalse()
    {
        // Arrange
        const string location = "/status/42";
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var ires = Knight.Response.Factories.Results
            .Success(new { id = 42 })
            .ToAcceptedResult(http, location);

        var (status, body, headers) = await TestHost.ExecuteAsync(ires, http);

        // Assert
        status.ShouldBe(StatusCodes.Status202Accepted);
        headers.Location.ToString().ShouldBe(location);
        body.ShouldContain("\"id\":42");
        body.ShouldNotContain("\"status\"");
    }

    [Fact]
    public async Task ToAcceptedResult_NonGeneric_FullPayloadFalse_202_With_EmptyBody()
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results.Success()
            .ToAcceptedResult(http, "/status/1");
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status202Accepted);
        headers.Location.ToString().ShouldBe("/status/1");
        body.ShouldBe(string.Empty);
    }

    // -------------------- No Content (204) --------------------

    [Fact]
    public async Task ToNoContentResult_Success_204_NoBody()
    {
        // Arrange
        var (http, _) = TestHost.CreateHttpContext();

        // Act
        var result = Knight.Response.Factories.Results.Success()
            .ToNoContentResult(http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status204NoContent);
        body.ShouldBeEmpty();
    }

    // -------------------- ToHttpResult (switching) --------------------

    [Theory]
    [InlineData(StatusCodes.Status200OK)]
    [InlineData(StatusCodes.Status201Created)]
    [InlineData(StatusCodes.Status202Accepted)]
    [InlineData(StatusCodes.Status204NoContent)]
    public async Task ToHttpResult_Result_Success_Maps_Status_And_Behavior(int statusCode)
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = true };
        var (http, _) = TestHost.CreateHttpContext(opts);
        var loc = statusCode is StatusCodes.Status201Created or StatusCodes.Status202Accepted ? "/r/1" : null;

        // Act
        var result = Knight.Response.Factories.Results.Success().ToHttpResult(statusCode, http, loc);
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(statusCode);

        if (statusCode == StatusCodes.Status204NoContent)
        {
            body.ShouldBe(string.Empty);
        }
        else if (statusCode is StatusCodes.Status201Created or StatusCodes.Status202Accepted)
        {
            headers.Location.ToString().ShouldBe("/r/1");
            body.ShouldContain("\"status\":\"Completed\"");
        }
        else
        {
            body.ShouldContain("\"status\":\"Completed\"");
        }
    }

    [Theory]
    [InlineData(StatusCodes.Status200OK)]
    [InlineData(StatusCodes.Status201Created)]
    [InlineData(StatusCodes.Status202Accepted)]
    public async Task ToHttpResult_Generic_Success_Maps_Status_And_Body_By_Options(int code)
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var (http, _) = TestHost.CreateHttpContext(opts);
        var model = new { id = 9 };
        var loc = code is StatusCodes.Status201Created or StatusCodes.Status202Accepted ? "/g/9" : null;

        // Act
        var result = Knight.Response.Factories.Results.Success(model).ToHttpResult(code, http, loc);
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(code);
        body.ShouldContain("\"id\":9");
        body.ShouldNotContain("\"status\"");

        if (code is StatusCodes.Status201Created or StatusCodes.Status202Accepted)
        {
            headers.Location.ToString().ShouldBe("/g/9");
        }
    }

    [Fact]
    public async Task ToHttpResult_201_Routes_To_Created_And_Sets_Location()
    {
        // Arrange
        const string location = "/api/items/5";
        var (http, _) = TestHost.CreateHttpContext(new KnightResponseOptions { IncludeFullResultPayload = false });

        // Act
        var ires = Knight.Response.Factories.Results
            .Success(new { id = 5 })
            .ToHttpResult(StatusCodes.Status201Created, http, location);

        var (status, body, headers) = await TestHost.ExecuteAsync(ires, http);

        // Assert
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location!.ToString().ShouldBe(location);
        body.ShouldContain("\"id\":5");
        body.ShouldNotContain("\"status\"");
    }

    [Fact]
    public async Task ToHttpResult_202_Routes_To_Accepted_And_Sets_Location()
    {
        // Arrange
        const string location = "/status/9";
        var (http, _) = TestHost.CreateHttpContext(new KnightResponseOptions { IncludeFullResultPayload = true });

        // Act
        var ires = Knight.Response.Factories.Results
            .Success("ok")
            .ToHttpResult(StatusCodes.Status202Accepted, http, location);

        var (status, body, headers) = await TestHost.ExecuteAsync(ires, http);

        // Assert
        status.ShouldBe(StatusCodes.Status202Accepted);
        headers.Location!.ToString().ShouldBe(location);
        body.ShouldContain("\"status\":\"Completed\"");
    }

    [Fact]
    public async Task ToHttpResult_204_Routes_To_NoContent()
    {
        // Arrange
        var (http, _) = TestHost.CreateHttpContext(new KnightResponseOptions());

        // Act
        var ires = Knight.Response.Factories.Results
            .Success()
            .ToHttpResult(StatusCodes.Status204NoContent, http);

        var (status, body, _) = await TestHost.ExecuteAsync(ires, http);

        // Assert
        status.ShouldBe(StatusCodes.Status204NoContent);
        body.ShouldBeEmpty();
    }
}
