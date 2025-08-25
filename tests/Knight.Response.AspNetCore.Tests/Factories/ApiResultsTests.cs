using System.Text.Json;
using Knight.Response.AspNetCore.Factories;
using Knight.Response.AspNetCore.Options;
using Knight.Response.AspNetCore.Tests.Infrastructure;
using Knight.Response.Core;
using Knight.Response.Models;
using Microsoft.AspNetCore.Http;
using Shouldly;

namespace Knight.Response.AspNetCore.Tests.Factories;

public class ApiResultsTests
{
    public static IEnumerable<object[]> TrueFalseData =>
        new List<object[]>
        {
            new object[]{ true },
            new object[]{ false }
        };

    public static IEnumerable<object[]> FailureData =>
        new List<object[]>
        {
            new object[]{ true, true, 400 },
            new object[]{ true, false, 400 },
            new object[]{ false, true, 400 },
            new object[]{ false, false, 400 },
            new object[]{ true, true, 500 },
            new object[]{ true, false, 500 },
            new object[]{ false, true, 500 },
            new object[]{ false, false, 500 }
        };

    private void AssertResponse(
        string content,
        bool includeFullPayload = false
        )
    {
        if (includeFullPayload)
        {
            content.ShouldContain("\"status\"");
            content.ShouldContain("\"messages\"");
            content.ShouldContain("\"isSuccess\"");
        }
        else
        {
            content.ShouldNotContain("\"status\"");
            content.ShouldNotContain("\"messages\"");
            content.ShouldNotContain("\"isSuccess\"");
        }
    }

    private void AssertResponse<T>(
        string content,
        T? value,
        bool includeFullPayload = false
        )
    {
        if (includeFullPayload)
        {
            var result = TestHelpers.Deserialize<Result<T>>(content);

            result.ShouldNotBeNull();
            result.Value.ShouldBe(value);
            result.IsSuccess.ShouldBeTrue();
            result.Status.ShouldBe(Status.Completed);
        }
        else
        {
            AssertResponse(content, includeFullPayload);

            if (value != null)
            {
                content.ShouldBe(JsonSerializer.Serialize(value));
            }
        }
    }

    private void AssertFailureResponse(
        string content,
        int statusCode = StatusCodes.Status400BadRequest,
        bool includeFullPayload = false,
        bool useProblemDetails = false
        )
    {
        if (useProblemDetails)
        {
            content.ShouldContain($"\"type\":\"https://httpstatuses.io/{statusCode}\"");
            content.ShouldContain("\"svcStatus\"");
            content.ShouldContain("\"messages\"");
        }
        else if (includeFullPayload)
        {
            content.ShouldContain("\"status\"");
            content.ShouldContain("\"messages\"");
            content.ShouldContain("\"isSuccess\"");
        }
        else
        {
            content.ShouldContain("\"type\"");
            content.ShouldContain("\"content\"");
            content.ShouldContain("\"metadata\"");

            content.ShouldNotContain("\"status\"");
            content.ShouldNotContain("\"messages\"");
            content.ShouldNotContain("\"isSuccess\"");
        }
    }

    [Theory]
    [MemberData(nameof(TrueFalseData))]
    public async Task Ok_Success_Returns_200_And_Respects_FullPayload_Settings(bool includeFullPayload)
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = includeFullPayload };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = ApiResults.Ok(Knight.Response.Factories.Results.Success(), http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status200OK);
        AssertResponse(body, includeFullPayload);
    }

    [Theory]
    [MemberData(nameof(TrueFalseData))]
    public async Task Ok_Generic_Success_Returns_200_And_Respects_FullPayload_Settings(bool includeFullPayload)
    {
        // Arrange
        var value = 42;
        var opts = new KnightResponseOptions { IncludeFullResultPayload = includeFullPayload };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = ApiResults.Ok(Knight.Response.Factories.Results.Success(value), http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status200OK);
        AssertResponse(body, value, includeFullPayload);
    }

    [Theory]
    [MemberData(nameof(FailureData))]
    public async Task Ok_Failure_Respects_ProblemDetails_Settings(
        bool useProblemDetails,
        bool includeFullPayload,
        int statusCode)
    {
        // Arrange
        const string reason = "failed";
        var opts = new KnightResponseOptions
        {
            IncludeFullResultPayload = includeFullPayload,
            UseProblemDetails = useProblemDetails,
            StatusCodeResolver = _ => statusCode
        };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = ApiResults.Ok(Knight.Response.Factories.Results.Failure(reason), http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(statusCode);
        body.ShouldContain(reason);
        AssertFailureResponse(body, statusCode, includeFullPayload, useProblemDetails);
    }

    [Theory]
    [MemberData(nameof(FailureData))]
    public async Task Ok_Generic_Failure_Respects_ProblemDetails_Settings(
        bool useProblemDetails, bool includeFullPayload, int statusCode)
    {
        // Arrange
        var opts = new KnightResponseOptions {
            IncludeFullResultPayload = includeFullPayload,
            UseProblemDetails = useProblemDetails,
            StatusCodeResolver = _ => statusCode
        };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var response = Knight.Response.Factories.Results.Failure<string>("bad");
        var result = ApiResults.Ok(response, http);
        var (actualStatus, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        actualStatus.ShouldBe(statusCode);
        AssertFailureResponse(body, statusCode, includeFullPayload, useProblemDetails);
    }


    [Theory]
    [MemberData(nameof(TrueFalseData))]
    public async Task Created_Success_Sets_Location_Header_And_Body_Respects_FullPayload(bool includeFullPayload)
    {
        // Arrange
        const string location = "/api/users/7";
        var dto = new { id = 7, name = "user" };
        var opts = new KnightResponseOptions { IncludeFullResultPayload = includeFullPayload };
        var (http, _) = TestHost.CreateHttpContext(opts);
        var response = Knight.Response.Factories.Results.Success(dto);

        // Act
        var result = ApiResults.Created(response, http, location);
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location.ToString().ShouldBe(location);
        AssertResponse(body, dto, includeFullPayload);
    }

    [Fact]
    public async Task Created_Success_Empty_Location()
    {
        // Arrange
        var opts = KnightResponseOptions.Defaults;
        var (http, _) = TestHost.CreateHttpContext(opts);
        var response = Knight.Response.Factories.Results.Success();

        // Act
        var result = ApiResults.Created(response, http);
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location.ShouldBeEmpty();
        AssertResponse(body, opts.IncludeFullResultPayload);
    }

    [Fact]
    public async Task Created_Generic_Success_Empty_Location()
    {
        // Arrange
        var dto = new { id = 7, name = "user" };
        var opts = KnightResponseOptions.Defaults;
        var (http, _) = TestHost.CreateHttpContext(opts);
        var response = Knight.Response.Factories.Results.Success(dto);

        // Act
        var result = ApiResults.Created(response, http);
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location.ShouldBeEmpty();
        AssertResponse(body, dto, opts.IncludeFullResultPayload);
    }

    [Theory]
    [MemberData(nameof(TrueFalseData))]
    public async Task Created_NonGeneric_Success_Sets_Location_HeaderAnd_Body_Respects_FullPayload(bool includeFullPayload)
    {
        // Arrange
        const string location = "/api/tasks/123";
        var opts = new KnightResponseOptions { IncludeFullResultPayload = includeFullPayload };
        var (http, _) = TestHost.CreateHttpContext(opts);
        var res = Knight.Response.Factories.Results.Success();

        // Act
        var result = ApiResults.Created(res, http, location);
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location.ToString().ShouldBe(location);
        AssertResponse(body, includeFullPayload);
    }

    [Theory]
    [MemberData(nameof(FailureData))]
    public async Task Created_Failure_Respects_ProblemDetails_Settings(
        bool useProblemDetails,
        bool includeFullPayload,
        int statusCode)
    {
        // Arrange
        const string reason = "failed";
        var opts = new KnightResponseOptions
        {
            IncludeFullResultPayload = includeFullPayload,
            UseProblemDetails = useProblemDetails,
            StatusCodeResolver = _ => statusCode
        };
        var (http, _) = TestHost.CreateHttpContext(opts);
        var res = Knight.Response.Factories.Results.Failure(reason);

        // Act
        var result = ApiResults.Created(res, http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(statusCode);
        body.ShouldContain(reason);
        AssertFailureResponse(body, statusCode, includeFullPayload, useProblemDetails);
    }

    [Theory]
    [MemberData(nameof(FailureData))]
    public async Task Created_Generic_Failure_Respects_ProblemDetails_Settings(
        bool useProblemDetails, bool includeFullPayload, int statusCode)
    {
        // Arrange
        var opts = new KnightResponseOptions {
            IncludeFullResultPayload = includeFullPayload,
            UseProblemDetails = useProblemDetails,
            StatusCodeResolver = _ => statusCode
        };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var response = Knight.Response.Factories.Results.Failure<object>("nope");
        var result = ApiResults.Created(response, http, "/any");
        var (actualStatus, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        actualStatus.ShouldBe(statusCode);
        AssertFailureResponse(body, statusCode, includeFullPayload, useProblemDetails);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Created_WithNullOrEmptyLocation_DoesNotSetLocation(string? location)
    {
        // Arrange
        var (http, _) = TestHost.CreateHttpContext(new KnightResponseOptions { IncludeFullResultPayload = false });

        // Act
        var result = ApiResults.Created(Knight.Response.Factories.Results.Success(new { id = 1 }), http, location);
        var (status, _, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location.ToString().ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(TrueFalseData))]
    public async Task Accepted_Success_Sets_202_And_Body_Respects_FullPayload(bool includeFullPayload)
    {
        // Arrange
        const string location = "/status/1";
        var opts = new KnightResponseOptions { IncludeFullResultPayload = includeFullPayload };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = ApiResults.Accepted(Knight.Response.Factories.Results.Success(), http, location);
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status202Accepted);
        headers.Location.ToString().ShouldBe(location);
        AssertResponse(body, includeFullPayload);
    }

    [Theory]
    [MemberData(nameof(TrueFalseData))]
    public async Task Accepted_Generic_Success_Sets_202_And_Body_Respects_FullPayload(bool includeFullPayload)
    {
        // Arrange
        const string location = "/status/1";
        var opts = new KnightResponseOptions { IncludeFullResultPayload = includeFullPayload };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = ApiResults.Accepted(Knight.Response.Factories.Results.Success<string>(), http, location);
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status202Accepted);
        headers.Location.ToString().ShouldBe(location);
        AssertResponse(body, includeFullPayload);
    }

    [Fact]
    public async Task Accepted_Success_Empty_Location()
    {
        // Arrange
        var opts = KnightResponseOptions.Defaults;
        var (http, _) = TestHost.CreateHttpContext(opts);
        var response = Knight.Response.Factories.Results.Success();

        // Act
        var result = ApiResults.Accepted(response, http);
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status202Accepted);
        headers.Location.ShouldBeEmpty();
        AssertResponse(body, opts.IncludeFullResultPayload);
    }

    [Fact]
    public async Task Accepted_Generic_Success_Empty_Location()
    {
        // Arrange
        var dto = new { id = 7, name = "user" };
        var opts = KnightResponseOptions.Defaults;
        var (http, _) = TestHost.CreateHttpContext(opts);
        var response = Knight.Response.Factories.Results.Success(dto);

        // Act
        var result = ApiResults.Accepted(response, http);
        var (status, body, headers) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status202Accepted);
        headers.Location.ShouldBeEmpty();
        AssertResponse(body, dto, opts.IncludeFullResultPayload);
    }

    [Theory]
    [MemberData(nameof(FailureData))]
    public async Task Accepted_Generic_Failure_Respects_ProblemDetails_Settings(
        bool useProblemDetails, bool includeFullPayload, int statusCode)
    {
        // Arrange
        var opts = new KnightResponseOptions {
            IncludeFullResultPayload = includeFullPayload,
            UseProblemDetails = useProblemDetails,
            StatusCodeResolver = _ => statusCode
        };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var response = Knight.Response.Factories.Results.Failure<string>("bad");
        var result = ApiResults.Accepted(response, http, "/status/1");
        var (actualStatus, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        actualStatus.ShouldBe(statusCode);
        AssertFailureResponse(body, statusCode, includeFullPayload, useProblemDetails);
    }

    [Theory]
    [MemberData(nameof(FailureData))]
    public async Task Accept_Failure_Respects_ProblemDetails_Settings(
        bool useProblemDetails,
        bool includeFullPayload,
        int statusCode)
    {
        // Arrange
        const string reason = "not acceptable!";
        var opts = new KnightResponseOptions
        {
            IncludeFullResultPayload = includeFullPayload,
            UseProblemDetails = useProblemDetails,
            StatusCodeResolver = _ => statusCode
        };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = ApiResults.Accepted(Knight.Response.Factories.Results.Failure(reason), http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(statusCode);
        body.ShouldContain(reason);
        AssertFailureResponse(body, statusCode, includeFullPayload, useProblemDetails);
    }

    [Theory]
    [MemberData(nameof(TrueFalseData))]
    public async Task NoContent_Success_Returns_204_And_Body_Respects_FullPayload(bool includeFullPayload)
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = includeFullPayload };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = ApiResults.NoContent(Knight.Response.Factories.Results.Success(), http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status204NoContent);
        body.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(TrueFalseData))]
    public async Task NoContent_Generic_Success_Returns_204_And_Body_Respects_FullPayload(bool includeFullPayload)
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = includeFullPayload };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = ApiResults.NoContent(Knight.Response.Factories.Results.Success("ok"), http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status204NoContent);
        body.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(FailureData))]
    public async Task NoContent_Failure_Respects_ProblemDetails_Settings(
        bool useProblemDetails,
        bool includeFullPayload,
        int statusCode)
    {
        // Arrange
        const string reason = "oops";
        var opts = new KnightResponseOptions
        {
            IncludeFullResultPayload = includeFullPayload,
            UseProblemDetails = useProblemDetails,
            StatusCodeResolver = _ => statusCode
        };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = ApiResults.NoContent(Knight.Response.Factories.Results.Failure(reason), http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(statusCode);
        body.ShouldContain(reason);
        AssertFailureResponse(body, statusCode, includeFullPayload, useProblemDetails);
    }

    [Fact]
    public async Task NoContent_Success_Returns_204_And_EmptyBody()
    {
        // Arrange
        var (http, _) = TestHost.CreateHttpContext(new KnightResponseOptions());

        // Act
        var result = ApiResults.NoContent(Knight.Response.Factories.Results.Success(), http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status204NoContent);
        body.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(FailureData))]
    public async Task NotFound_Maps_Result_To_404_And_Respects_ProblemDetails_Settings(
        bool useProblemDetails,
        bool includeFullPayload,
        int statusCode)
    {
        // Arrange
        const string reason = "not found!";
        var opts = new KnightResponseOptions
        {
            IncludeFullResultPayload = includeFullPayload,
            UseProblemDetails = useProblemDetails,
            StatusCodeResolver = _ => statusCode
        };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = ApiResults.NotFound(Knight.Response.Factories.Results.Failure(reason), http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status404NotFound);
        body.ShouldContain(reason);
        AssertFailureResponse(body, StatusCodes.Status404NotFound, includeFullPayload, useProblemDetails);
    }

    [Theory]
    [MemberData(nameof(FailureData))]
    public async Task Conflict_Maps_Result_To_409_And_Respects_ProblemDetails_Settings(
        bool useProblemDetails,
        bool includeFullPayload,
        int statusCode)
    {
        // Arrange
        const string reason = "collision!";
        var opts = new KnightResponseOptions
        {
            IncludeFullResultPayload = includeFullPayload,
            UseProblemDetails = useProblemDetails,
            StatusCodeResolver = _ => statusCode
        };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var result = ApiResults.Conflict(Knight.Response.Factories.Results.Failure(reason), http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status409Conflict);
        body.ShouldContain(reason);
        AssertFailureResponse(body, StatusCodes.Status409Conflict, includeFullPayload, useProblemDetails);
    }

    [Fact]
    public async Task Unauthorized_Return_Standard_Codes()
    {
        // Arrange
        var (http, _) = TestHost.CreateHttpContext();

        // Act
        var unAuthResult = await TestHost.ExecuteAsync(ApiResults.Unauthorized(), http);

        // Assert
        unAuthResult.status.ShouldBe(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Forbidden_Return_Standard_Codes()
    {
        // Arrange
        var (http, _) = TestHost.CreateHttpContext();

        // Act
        var forbiddenResult = await TestHost.ExecuteAsync(ApiResults.Forbidden(), http);

        // Assert
        forbiddenResult.status.ShouldBe(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task BuildFailure_DoesNotOverride_Explicit_400()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = false };
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        var response = Knight.Response.Factories.Results.Failure("bad");
        var result = ApiResults.BadRequest(response, http); // passes 400 explicitly
        var (status, _, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);
    }

    // -------------------- Mutant War! --------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Created_With_NullOrEmpty_Location_Does_Not_Set_Header(string? location)
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var (http, _) = TestHost.CreateHttpContext(opts);
        var res = Knight.Response.Factories.Results.Success(new { id = 1 });

        // Act
        var ires = ApiResults.Created(res, http, location);
        var (status, _, headers) = await TestHost.ExecuteAsync(ires, http);

        // Assert
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location.ShouldBeEmpty();
    }

    [Fact]
    public async Task Created_With_NonEmpty_Location_Sets_Header()
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var (http, _) = TestHost.CreateHttpContext(opts);
        var res = Knight.Response.Factories.Results.Success(new { id = 2 });

        // Act
        var ires = ApiResults.Created(res, http, "/api/items/2");
        var (status, _, headers) = await TestHost.ExecuteAsync(ires, http);

        // Assert
        status.ShouldBe(StatusCodes.Status201Created);
        headers.Location!.ToString().ShouldBe("/api/items/2");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Accepted_With_NullOrEmpty_Location_Does_Not_Set_Header(string? location)
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var (http, _) = TestHost.CreateHttpContext(opts);
        var res = Knight.Response.Factories.Results.Success();

        // Act
        var ires = ApiResults.Accepted(res, http, location);
        var (status, _, headers) = await TestHost.ExecuteAsync(ires, http);

        // Assert
        status.ShouldBe(StatusCodes.Status202Accepted);
        headers.Location.ShouldBeEmpty();
    }

    [Fact]
    public async Task Accepted_With_NonEmpty_Location_Sets_Header()
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var (http, _) = TestHost.CreateHttpContext(opts);
        var res = Knight.Response.Factories.Results.Success<string>();

        // Act
        var ires = ApiResults.Accepted(res, http, "/status/42");
        var (status, _, headers) = await TestHost.ExecuteAsync(ires, http);

        // Assert
        status.ShouldBe(StatusCodes.Status202Accepted);
        headers.Location!.ToString().ShouldBe("/status/42");
    }
}
