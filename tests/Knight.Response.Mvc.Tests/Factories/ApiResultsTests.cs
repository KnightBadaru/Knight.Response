using Knight.Response.Core;
using Knight.Response.Factories;
using Knight.Response.Mvc.Factories;
using Knight.Response.Mvc.Options;
using Knight.Response.Mvc.Infrastructure;
using Knight.Response.Mvc.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Knight.Response.Mvc.Tests.Factories
{
    public class ApiResultsTests
    {
        // -------------------- OK (200) --------------------

        [Fact]
        public void Ok_Success_Defaults_Returns200_WithValueOnly()
        {
            // Arrange
            var result = Results.Success();
            var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
            var http = TestHost.CreateHttpContext(opts);

            // Act
            var actionResult = ApiResults.Ok(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
            objectResult.Value.ShouldBeNull();
        }

        [Fact]
        public void Ok_Success_FullPayload_Returns200_WithFullResult()
        {
            // Arrange
            var dto = new Widget(1, "w1");
            var http = TestHost.CreateHttpContext();
            var result = Results.Success(dto);

            // Act
            var actionResult = ApiResults.Ok(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
            var responseValue = objectResult.Value.ShouldBeOfType<Result<Widget>>();
            responseValue.Value.ShouldBe(dto);
        }

        [Fact]
        public void Ok_Generic_Success_Defaults_Returns200_WithValueOnly()
        {
            // Arrange
            var dto = new Widget(1, "w1");
            var result = Results.Success(dto);
            var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
            var http = TestHost.CreateHttpContext(opts);

            // Act
            var actionResult = ApiResults.Ok(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
            var responseValue = objectResult.Value.ShouldBeOfType<Widget>();
            responseValue.ShouldBe(dto);
        }

        [Fact]
        public void Ok_Generic_Success_FullPayload_Returns200_WithFullResult()
        {
            // Arrange
            var dto = new Widget(1, "w1");
            var http = TestHost.CreateHttpContext();
            var result = Results.Success(dto);

            // Act
            var actionResult = ApiResults.Ok(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
            var responseValue = objectResult.Value.ShouldBeOfType<Result<Widget>>();
            responseValue.Value.ShouldBe(dto);
        }

        [Fact]
        public void Ok_Failure_WithProblemDetails_ReturnsClientErrorProblem()
        {
            // Arrange
            var opts = new KnightResponseOptions
            {
                UseProblemDetails = true
            };
            var http = TestHost.CreateHttpContext(opts);
            var result = Results.Failure<Widget>("bad-things-happened");

            // Act
            var actionResult = ApiResults.Ok(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldNotBeNull();
            objectResult.StatusCode?.ShouldBeGreaterThanOrEqualTo(StatusCodes.Status400BadRequest);
            objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        }

        [Fact]
        public void Ok_Failure_WithHttpNull_FallsBack_ToMessages_NotProblemDetails()
        {
            // Arrange
            var result = Results.Failure("bad");
            var opts = new KnightResponseOptions();
            var statusCode = opts.StatusCodeResolver(result.Status);

            // Act
            var actionResult = ApiResults.Ok(result, http: null);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldNotBeNull();
            objectResult.StatusCode?.ShouldBe(statusCode);
        }

        [Fact]
        public void OkT_Failure_WithHttpNull_FallsBack_ToMessages_NotProblemDetails()
        {
            // Arrange
            var result = Results.Failure<Widget>("bad");
            var opts = new KnightResponseOptions();
            var statusCode = opts.StatusCodeResolver(result.Status);

            // Act
            var actionResult = ApiResults.Ok(result, http: null);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldNotBeNull();
            objectResult.StatusCode?.ShouldBe(statusCode);
        }

        // -------------------- Created (201) --------------------

        [Fact]
        public void Created_Success_Sets201_AndOptionalLocationHeader()
        {
            // Arrange
            var dto = new Widget(7, "w7");
            var http = TestHost.CreateHttpContext();
            var result   = Results.Success(dto);
            var location = "/api/widgets/7";

            // Act
            var actionResult = ApiResults.Created(result, http, location);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
            http.Response.Headers["Location"].ToString().ShouldBe(location);
        }

        [Fact]
        public void Created_Failure_EmitsProblemDetails()
        {
            // Arrange
            var opts = new KnightResponseOptions
            {
                UseProblemDetails = true
            };
            var http = TestHost.CreateHttpContext(opts);
            var result = Results.Failure<Widget>("cannot-create");

            // Act
            var actionResult = ApiResults.Created(result, http, "/api/widgets/9");

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldNotBeNull();
            objectResult.StatusCode?.ShouldBeGreaterThanOrEqualTo(StatusCodes.Status400BadRequest);
            objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        }

        [Fact]
        public void Created_NonGeneric_Success_Sets201_AndLocation()
        {
            // Arrange
            var result = Results.Success();
            var http   = TestHost.CreateHttpContext();
            var location    = "/ops/42";

            // Act
            var actionResult = ApiResults.Created(result, http, location);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
            http.Response.Headers["Location"].ToString().ShouldBe(location);
        }

        [Fact]
        public void Created_NonGeneric_Failure_EmitsProblemDetails_WhenEnabled()
        {
            // Arrange
            var opts = new KnightResponseOptions
            {
                UseProblemDetails = true
            };
            var http = TestHost.CreateHttpContext(opts);
            var result = Results.Failure("cannot-create");

            // Act
            var actionResult = ApiResults.Created(result, http, "/ignored");

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldNotBeNull();
            objectResult.StatusCode?.ShouldBeGreaterThanOrEqualTo(StatusCodes.Status400BadRequest);
            objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        }

        // -------------------- Accepted (202) --------------------

        [Fact]
        public void Accepted_NonGeneric_Sets202_AndOptionalLocationHeader()
        {
            // Arrange
            var http = TestHost.CreateHttpContext();
            var result   = Results.Success();
            var location = "/operations/123";

            // Act
            var actionResult = ApiResults.Accepted(result, http, location);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status202Accepted);
            http.Response.Headers["Location"].ToString().ShouldBe(location);
            objectResult.Value.ShouldBe(result); // non-generic – payload is the Result by design
        }

        [Fact]
        public void Accepted_Failure_UsesProblemDetailsWhenEnabled()
        {
            // Arrange
            var opts = new KnightResponseOptions
            {
                UseProblemDetails = true
            };
            var http = TestHost.CreateHttpContext(opts);
            var result = Results.Failure("queued-but-invalid");

            // Act
            var actionResult = ApiResults.Accepted(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldNotBeNull();
            var status = opts.StatusCodeResolver(result.Status);
            objectResult.StatusCode?.ShouldBeGreaterThanOrEqualTo(status);
            objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        }

        [Fact]
        public void AcceptedT_Success_Defaults_ToValuePayload_AndSets202_AndLocation()
        {
            // Arrange
            var dto = new Widget(1, "w1");
            var result = Results.Success(dto);
            var http  = TestHost.CreateHttpContext();
            var location = "/w1/1";

            // Act
            var actionResult = ApiResults.Accepted(result, http, location);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status202Accepted);
            var responseValue = objectResult.Value.ShouldBeOfType<Result<Widget>>();
            responseValue.Value.ShouldBe(dto);
        }

        [Fact]
        public void AcceptedT_Success_Respects_FullyPayload_Configuration()
        {
            // Arrange
            var dto = new Widget(9, "w9");
            var result = Results.Success(dto);
            var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
            var http   =TestHost.CreateHttpContext(opts);

            // Act
            var actionResult = ApiResults.Accepted(result, http, "/ops/9");

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status202Accepted);
            var responseValue = objectResult.Value.ShouldBeOfType<Widget>();
            responseValue.ShouldBe(dto);
        }

        // -------------------- NoContent (204) --------------------

        [Fact]
        public void NoContent_Success_Returns204_NoBody()
        {
            // Arrange
            var http = TestHost.CreateHttpContext();
            var result = Results.Success();

            // Act
            var actionResult = ApiResults.NoContent(result, http);

            // Assert
            var statusCodeResult = actionResult.ShouldBeOfType<ObjectResult>();
            statusCodeResult.StatusCode.ShouldBe(StatusCodes.Status204NoContent);
            statusCodeResult.Value.ShouldBeNull();
        }

        [Fact]
        public void NoContentT_Success_Returns204_NoBody()
        {
            // Arrange
            var http = TestHost.CreateHttpContext();
            var result = Results.Success<Widget>();

            // Act
            var actionResult = ApiResults.NoContent(result, http);

            // Assert
            var statusCodeResult = actionResult.ShouldBeOfType<ObjectResult>();
            statusCodeResult.StatusCode.ShouldBe(StatusCodes.Status204NoContent);
            statusCodeResult.Value.ShouldBeNull();
        }

        [Fact]
        public void NoContent_Failure_ReturnsProblem()
        {
            // Arrange
            var opts = new KnightResponseOptions
            {
                UseProblemDetails = true
            };
            var http = TestHost.CreateHttpContext(opts);
            var result = Results.Failure("cannot-complete");

            // Act
            var actionResult = ApiResults.NoContent(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldNotBeNull();
            var status = opts.StatusCodeResolver(result.Status);
            objectResult.StatusCode?.ShouldBeGreaterThanOrEqualTo(status);
            objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        }

        // -------------------- Mapping --------------------

        [Fact]
        public void BadRequest_Always400_WithProblem()
        {
            // Arrange
            var opts = new KnightResponseOptions
            {
                UseProblemDetails = true
            };
            var http = TestHost.CreateHttpContext(opts);
            var result = Results.Failure("invalid-input");

            // Act
            var actionResult = ApiResults.BadRequest(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
            objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        }

        [Fact]
        public void NotFound_Always404_WithProblem()
        {
            // Arrange
            var opts = new KnightResponseOptions
            {
                UseProblemDetails = true
            };
            var http = TestHost.CreateHttpContext(opts);
            var result = Results.Failure("missing");

            // Act
            var actionResult = ApiResults.NotFound(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
            objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        }

        [Fact]
        public void Conflict_Always409_WithProblem()
        {
            // Arrange
            var opts = new KnightResponseOptions
            {
                UseProblemDetails = true
            };
            var http = TestHost.CreateHttpContext(opts);
            var result = Results.Failure("conflict");

            // Act
            var actionResult = ApiResults.Conflict(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
            objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        }

        [Fact]
        public void Unauthorized_Always401_WithProblem()
        {
            // Arrange
            // Act
            var action = ApiResults.Unauthorized();

            // Assert
            var objectResult = action.ShouldBeOfType<UnauthorizedResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public void Forbidden_Always403_WithProblem()
        {
            // Arrange
            // Act
            var actionResult = ApiResults.Forbidden();

            // Assert
            actionResult.ShouldBeOfType<ForbidResult>();
        }

        // =====================================================
        // Validation problem path
        // =====================================================

        [Fact]
        public void BadRequest_WithValidationProblemDetails_UsesValidationShape()
        {
            // Arrange
            var opts = new KnightResponseOptions
            {
                UseProblemDetails = true,
                UseValidationProblemDetails = true
            };
            var http = TestHost.CreateHttpContext<FakeValidationMapper>(opts);
            var result = Results.Failure("Name: Name is required.");

            // Act
            var actionResult = ApiResults.BadRequest(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
            objectResult.Value.ShouldBeOfType<CompatValidationProblemDetails>();
        }

        // -------------------- Integration --------------------

        [Fact]
        public async Task Result_Should_Serialize_Through_MVC()
        {
            // Arrange
            var http = TestHost.CreateHttpContext();
            var result = Results.Success();

            // Act
            var actionResult = ApiResults.Ok(result, http);
            var testResult = await TestHost.ExecuteAsync(actionResult, http);

            // Assert
            var response = TestHelpers.Deserialize<Result>(testResult.Body)!;
            response.IsSuccess.ShouldBeTrue();
            testResult.Body.ShouldNotContain("\"Value\"");
        }

        [Fact]
        public async Task ResultT_Should_Serialize_Through_MVC()
        {
            // Arrange
            var dto = new Widget(2, "Second");
            var http = TestHost.CreateHttpContext();
            var result = Results.Success(dto);

            // Act
            var actionResult = ApiResults.Created(result, http);
            var testResult = await TestHost.ExecuteAsync(actionResult, http);

            // Assert
            var response = TestHelpers.Deserialize<Result<Widget>>(testResult.Body)!;
            response.IsSuccess.ShouldBeTrue();
            response.Value.ShouldBe(dto);
        }
    }
}
