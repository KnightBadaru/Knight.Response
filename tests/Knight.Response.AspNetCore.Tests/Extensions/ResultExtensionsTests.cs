using Knight.Response.AspNetCore.Extensions;
using Knight.Response.AspNetCore.Options;
using Knight.Response.AspNetCore.Tests.Infrastructure;
using Knight.Response.Core;
using Knight.Response.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Knight.Response.AspNetCore.Tests.Extensions;

public class ResultExtensionsTests
{
    [Fact]
    public async Task ToIResult_Success_Respects_IncludeFullResultPayload()
    {
        // Arrange
        var dto = new Dictionary<string, int>
        {
            ["id"] = 1
        };
        var opts = new KnightResponseOptions { IncludeFullResultPayload = true };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results
            .Success(dto)
            .ToIResult(http);

        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status200OK);

        var response = TestHelpers.Deserialize<Result<Dictionary<string, int>>>(body);

        response.ShouldNotBeNull();
        response.Value!.ShouldBe(dto);
        response.Status.ShouldBe(Status.Completed);
    }

    [Fact]
    public async Task ToIResult_Failure_Uses_ProblemDetails_When_Enabled()
    {
        // Arrange
        const string error = "oops!";
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results
            .Failure(error)
            .ToIResult(http);

        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);

        var pd = TestHelpers.Deserialize<ProblemDetails>(body);
        pd.ShouldNotBeNull();
        pd.Title.ShouldBe(error);
        pd.Type.ShouldBe(TestHelpers.HttpStatusUrl());
        pd.Status.ShouldBe(StatusCodes.Status400BadRequest);

        TestHelpers.SvcStatusShouldBeFailed(pd);
        var messages = TestHelpers.Ext<List<Message>>(pd, "messages");
        messages.ShouldContain(m => m.Type == MessageType.Error && m.Content == error);
    }

    // -------------------- Created (201) --------------------

    [Fact]
    public async Task ToCreatedResult_Success_FullPayload_201_With_Location_And_Result()
    {
        // Arrange
        const string location = "/api/items/1";
        var opts = new KnightResponseOptions { IncludeFullResultPayload = true };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results
            .Success()
            .ToCreatedResult(http, location);

        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        var response = TestHelpers.Deserialize<Result>(body);
        response.ShouldNotBeNull();
        response.Status.ShouldBe(Status.Completed);
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location.ToString().ShouldBe(location);
    }

    [Fact]
    public async Task ToCreatedResult_Success_ValueOnly_For_Generic_When_FullPayloadFalse()
    {
        // Arrange
        const string location = "/api/items/1";
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results
            .Success(new { id = 7, name = "knight" })
            .ToCreatedResult(http, location);

        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status201Created);
        var response = TestHelpers.Deserialize<Result>(body);
        response.ShouldNotBeNull();
        response.Status.ShouldBe(Status.Completed);
        headers.Location.ToString().ShouldBe(location);
        body.ShouldContain("\"id\":7");
        body.ShouldNotContain("\"status\""); // value-only
    }

    [Fact]
    public async Task ToCreatedResult_Success_NonGeneric_FullPayloadFalse_201_With_EmptyBody()
    {
        // Arrange
        const string location = "/api/ping/1";
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results
            .Success() // No content
            .ToCreatedResult(http, location);

        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location.ToString().ShouldBe(location);
        body.ShouldBe(string.Empty); // Created(location, null)
    }

    [Fact]
    public async Task ToCreatedResult_Failure_Uses_ClientError_And_ProblemDetails_When_Enabled()
    {
        // Arrange
        const string error = "This is bad!";
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results
            .Failure(error)
            .ToCreatedResult(http, "/api/items");

        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);

        var pd = TestHelpers.Deserialize<ProblemDetails>(body);
        pd.ShouldNotBeNull();
        pd.Title.ShouldBe(error);
        pd.Type.ShouldBe(TestHelpers.HttpStatusUrl());
        pd.Status.ShouldBe(StatusCodes.Status400BadRequest);

        TestHelpers.SvcStatusShouldBeFailed(pd);
        var messages = TestHelpers.Ext<List<Message>>(pd, "messages");
        messages.ShouldContain(m => m.Type == MessageType.Error && m.Content == error);
    }

    [Fact]
    public async Task ToCreatedResult_Routes_To_201_And_Sets_Location_NonGeneric()
    {
        // Arrange
        const string location = "/api/items/5";
        var (http, _) = TestHost.CreateHttpContext(new KnightResponseOptions { IncludeFullResultPayload = true });

        // Act
        var result = Knight.Response.Factories.Results
            .Success()
            .ToCreatedResult(http, location);

        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        var response = TestHelpers.Deserialize<Result>(body);
        response.ShouldNotBeNull();
        response.Status.ShouldBe(Status.Completed);
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location.ToString().ShouldBe(location);
    }

    // -------------------- Accepted (202) --------------------

    [Fact]
    public async Task ToAcceptedResult_Success_202_With_Location_And_Result_When_FullPayloadTrue()
    {
        // Arrange
        const string location = "/status/abc";
        var opts = new KnightResponseOptions { IncludeFullResultPayload = true };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results.Success(new { ticket = "abc" })
            .ToAcceptedResult(http, location);
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        var response = TestHelpers.Deserialize<Result>(body);
        response.ShouldNotBeNull();
        response.Status.ShouldBe(Status.Completed);
        status.ShouldBe(StatusCodes.Status202Accepted);
        headers.Location.ToString().ShouldBe(location);
        body.ShouldContain("\"ticket\":\"abc\"");
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
        var response = TestHelpers.Deserialize<Result>(body);
        response.ShouldNotBeNull();
        response.Status.ShouldBe(Status.Completed);
        headers.Location.ToString().ShouldBe(location);
        body.ShouldContain("\"id\":42");
        body.ShouldNotContain("\"status\""); // value-only
    }

    [Fact]
    public async Task ToAcceptedResult_NonGeneric_FullPayloadFalse_202_With_EmptyBody()
    {
        // Arrange
        const string location = "/status/1";
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = Knight.Response.Factories.Results.Success()
            .ToAcceptedResult(http, location);
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status202Accepted);
        headers.Location.ToString().ShouldBe(location);
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
        const string location = "/r/1";
        var opts = new KnightResponseOptions { IncludeFullResultPayload = true };
        var (http, _) = TestHost.CreateHttpContext(opts);
        var loc = statusCode is StatusCodes.Status201Created or StatusCodes.Status202Accepted ? location : null;

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
            headers.Location.ToString().ShouldBe(location);
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
        const string location = "/g/9";
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var (http, _) = TestHost.CreateHttpContext(opts);
        var model = new { id = 9 };
        var loc = code is StatusCodes.Status201Created or StatusCodes.Status202Accepted ? location : null;

        // Act
        var result = Knight.Response.Factories.Results.Success(model).ToHttpResult(code, http, loc);
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        var response = TestHelpers.Deserialize<Result>(body);
        response.ShouldNotBeNull();
        response.Status.ShouldBe(Status.Completed);

        status.ShouldBe(code);
        body.ShouldContain("\"id\":9");
        body.ShouldNotContain("\"status\"");

        if (code is StatusCodes.Status201Created or StatusCodes.Status202Accepted)
        {
            headers.Location.ToString().ShouldBe(location);
        }
    }

    [Fact]
    public async Task ToHttpResult_201_Routes_To_Created_And_Sets_Location()
    {
        // Arrange
        const string location = "/api/items/5";
        var (http, _) = TestHost.CreateHttpContext(new KnightResponseOptions { IncludeFullResultPayload = false });

        // Act
        var result = Knight.Response.Factories.Results
            .Success(new { id = 5 })
            .ToHttpResult(StatusCodes.Status201Created, http, location);

        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        var response = TestHelpers.Deserialize<Result>(body);
        response.ShouldNotBeNull();
        response.Status.ShouldBe(Status.Completed);

        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location.ToString().ShouldBe(location);
        body.ShouldContain("\"id\":5");
        body.ShouldNotContain("\"status\"");
    }

    [Fact]
    public async Task ToHttpResult_202_Routes_To_Accepted_And_Sets_Location()
    {
        // Arrange
        const string value = "ok";
        const string location = "/status/9";
        var (http, _) = TestHost.CreateHttpContext(new KnightResponseOptions { IncludeFullResultPayload = true });

        // Act
        var result = Knight.Response.Factories.Results
            .Success(value)
            .ToHttpResult(StatusCodes.Status202Accepted, http, location);

        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        var response = TestHelpers.Deserialize<Result<string>>(body);
        response.ShouldNotBeNull();
        response.Status.ShouldBe(Status.Completed);
        response.Value.ShouldBe(value);

        status.ShouldBe(StatusCodes.Status202Accepted);
        headers.Location.ToString().ShouldBe(location);
    }

    [Fact]
    public async Task ToHttpResult_204_Routes_To_NoContent()
    {
        // Arrange
        var (http, _) = TestHost.CreateHttpContext(new KnightResponseOptions());

        // Act
        var result = Knight.Response.Factories.Results
            .Success()
            .ToHttpResult(StatusCodes.Status204NoContent, http);

        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status204NoContent);
        body.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToHttpResult_Generic_204_NoContent_Returns_EmptyBody()
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = true };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var response = Knight.Response.Factories.Results.Success(new { id = 1 });
        var result = response.ToHttpResult(StatusCodes.Status204NoContent, http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status204NoContent);
        body.ShouldBeEmpty();
    }
}
