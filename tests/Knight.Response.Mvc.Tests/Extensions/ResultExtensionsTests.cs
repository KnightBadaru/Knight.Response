using Knight.Response.Core;
using Knight.Response.Factories;
using Knight.Response.Models;
using Knight.Response.Mvc.Extensions;
using Knight.Response.Mvc.Infrastructure;
using Knight.Response.Mvc.Options;
using Knight.Response.Mvc.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Knight.Response.Mvc.Tests.Extensions;

public class ResultExtensionsTests
{
    // =====================================================
    // ToOkActionResult (non-generic)
    // =====================================================

    private const string Location = "Location";

    [Fact]
    public void ToOkActionResult_NonGeneric_Success_200_NoBody()
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var result = Results.Success();
        var http   = TestHost.CreateHttpContext(opts);

        // Act
        var action = result.ToOkActionResult(http);

        // Assert
        var objectResult = action.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        objectResult.Value.ShouldBeNull();
    }

    // =====================================================
    // ToOkActionResult (generic)
    // =====================================================

    [Fact]
    public void ToOkActionResult_Generic_Success_200_ValueOnly()
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
        var payload = new Widget(1, "w1");
        var result  = Results.Success(payload);
        var http    = TestHost.CreateHttpContext(opts);

        // Act
        var action = result.ToOkActionResult(http);

        // Assert
        var objectResult = action.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        var response = objectResult.Value.ShouldBeOfType<Widget>();
        response.ShouldBe(payload);
    }

    [Fact]
    public void ToOkActionResult_Generic_Success_200_FullPayload_WhenConfigured()
    {
        // Arrange
        var opts = new KnightResponseOptions { IncludeFullResultPayload = true };
        var payload = new Widget(2, "w2");
        var result  = Results.Success(payload);
        var http    = TestHost.CreateHttpContext(opts);

        // Act
        var action = result.ToOkActionResult(http);

        // Assert
        var objectResult = action.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        var response = objectResult.Value.ShouldBeOfType<Result<Widget>>();
        response.Value.ShouldBe(payload); // full Result<T>
    }

    [Fact]
    public void ToOkActionResult_Generic_Failure_EmitsProblemDetails_WhenEnabled()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        const string content = "oops";
        var result = Results.Error<Widget>(new List<Message> { new(MessageType.Error, content) });
        var http   = TestHost.CreateHttpContext(opts);

        // Act
        var action = result.ToOkActionResult(http);

        // Assert
        var objectResult = action.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldNotBeNull();
        var status = opts.StatusCodeResolver(result.Status);
        objectResult.StatusCode?.ShouldBeGreaterThanOrEqualTo(status);
        var problem = objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        problem.Title.ShouldBe(content);
    }

    [Fact]
    public void ToOkActionResult_Generic_Failure_WithHttpNull_NoProblemDetails()
    {
        // Arrange
        var result = Results.Error<Widget>(new List<Message> { new(MessageType.Error, "no http") });

        // Act
        var action = result.ToOkActionResult(http: null);

        // Assert
        var status = new KnightResponseOptions().StatusCodeResolver(result.Status);
        var objectResult = action.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(status);
        objectResult.Value.ShouldNotBeNull();
    }

    // =====================================================
    // ToCreatedActionResult (generic)
    // =====================================================

    [Fact]
    public void ToCreatedActionResult_Success_201_SetsLocation()
    {
        // Arrange
        var result   = Results.Success();
        var http     = TestHost.CreateHttpContext();
        var location = "/api/widgets/10";

        // Act
        var action = result.ToCreatedActionResult(http, location);

        // Assert
        var objectResult = action.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
        http.Response.Headers[Location].ToString().ShouldBe(location);
    }

    [Fact]
    public void ToCreatedActionResult_Failure_EmitsProblemDetails_WhenEnabled()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        const string content = "cannot create";
        var result = Results.Error(new List<Message> { new(MessageType.Error, content) });
        var http   = TestHost.CreateHttpContext(opts);

        // Act
        var action = result.ToCreatedActionResult(http, "/ignored");

        // Assert
        var objectResult = action.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldNotBeNull();
        var status = opts.StatusCodeResolver(result.Status);
        objectResult.StatusCode?.ShouldBeGreaterThanOrEqualTo(status);
        var problem = objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        problem.Title.ShouldBe(content);
    }

    [Fact]
    public void ToCreatedActionResult_Generic_Success_201_SetsLocation()
    {
        // Arrange
        var result   = Results.Success(new Widget(10, "w10"));
        var http     = TestHost.CreateHttpContext();
        var location = "/api/widgets/10";

        // Act
        var action = result.ToCreatedActionResult(http, location);

        // Assert
        var objectResult = action.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
        http.Response.Headers[Location].ToString().ShouldBe(location);
    }

    [Fact]
    public void ToCreatedActionResult_Generic_Failure_EmitsProblemDetails_WhenEnabled()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        const string content = "cannot create";
        var result = Results.Error<Widget>(new List<Message> { new(MessageType.Error, content) });
        var http   = TestHost.CreateHttpContext(opts);

        // Act
        var action = result.ToCreatedActionResult(http, "/ignored");

        // Assert
        var objectResult = action.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldNotBeNull();
        var status = opts.StatusCodeResolver(result.Status);
        objectResult.StatusCode?.ShouldBeGreaterThanOrEqualTo(status);
        var problem = objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        problem.Title.ShouldBe(content);
    }

    // =====================================================
    // ToAcceptedActionResult (generic + non-generic)
    // =====================================================

    [Fact]
    public void ToAcceptedActionResult_Generic_Success_202_SetsLocation()
    {
        // Arrange
        var result   = Results.Success(new Widget(99, "w99"));
        var http     = TestHost.CreateHttpContext();
        var location = "/operations/99";

        // Act
        var action = result.ToAcceptedActionResult(http, location);

        // Assert
        var objectResult = action.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status202Accepted);
        http.Response.Headers[Location].ToString().ShouldBe(location);
    }

    [Fact]
    public void ToAcceptedActionResult_NonGeneric_Success_202_SetsLocation()
    {
        // Arrange
        var result   = Results.Success();
        var http     = TestHost.CreateHttpContext();
        var location = "/operations/42";

        // Act
        var action = result.ToAcceptedActionResult(http, location);

        // Assert
        var objectResult = action.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status202Accepted);
        http.Response.Headers[Location].ToString().ShouldBe(location);
    }

    // =====================================================
    // ToNoContentActionResult (non-generic)
    // =====================================================

    [Fact]
    public void ToNoContentActionResult_NonGeneric_Success_204_NoBody()
    {
        // Arrange
        var result = Results.Success();
        var http   = TestHost.CreateHttpContext();

        // Act
        var action = result.ToNoContentActionResult(http);

        // Assert
        var objectResult = action.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status204NoContent);
        objectResult.Value.ShouldBeNull();
    }

    // =====================================================
    // Specific error helpers (non-generic)
    // =====================================================

    [Fact]
    public void ToBadRequestActionResult_Failure_400()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var result = TestHelpers.Failure("invalid");
        var http   = TestHost.CreateHttpContext(opts);

        // Act
        var action = result.ToBadRequestActionResult(http);

        // Assert
        var objectResult = action.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void ToNotFoundActionResult_Failure_404()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var result = TestHelpers.Failure("missing");
        var http   = TestHost.CreateHttpContext(opts);

        // Act
        var action = result.ToNotFoundActionResult(http);

        // Assert
        var obj = action.ShouldBeOfType<ObjectResult>();
        obj.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public void ToConflictActionResult_Failure_409()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var result = TestHelpers.Failure("conflict");
        var http   = TestHost.CreateHttpContext(opts);

        // Act
        var action = result.ToConflictActionResult(http);

        // Assert
        var obj = action.ShouldBeOfType<ObjectResult>();
        obj.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
    }
}