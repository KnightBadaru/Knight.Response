using System.Reflection;
using Knight.Response.Core;
using Knight.Response.Factories;
using Knight.Response.Models;
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
        private const string Location = "location";
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
            var response = objectResult.Value.ShouldBeOfType<Result<Widget>>();
            response.ShouldNotBeNull();
            response.ShouldBe(result);
            response.Value.ShouldBe(dto);
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
            var response = objectResult.Value.ShouldBeOfType<Widget>();
            response.ShouldBe(dto);
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
            var response = objectResult.Value.ShouldBeOfType<Result<Widget>>();
            response.ShouldNotBeNull();
            response.ShouldBe(result);
            response.Value.ShouldBe(dto);
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
            http.Response.Headers[Location].ToString().ShouldBe(location);
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
            http.Response.Headers[Location].ToString().ShouldBe(location);
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
            http.Response.Headers[Location].ToString().ShouldBe(location);
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

        // -------------------- Mutants Killers --------------------

        [Fact]
        public void Ok_Failure_UseProblemDetailsTrue_WithNullHttp_Uses_Default_Opts()
        {
            // Arrange
            var result = Results.Error(new List<Message> { new(MessageType.Error, "boom") });

            // Act
            var actionResult = ApiResults.Ok(result, http: null);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            var response = objectResult.Value.ShouldBeOfType<Result>();
            response.ShouldBe(result);
        }

        [Fact]
        public void Ok_Failure_UseProblemDetailsFalse_WithHttpNotNull_FallsBackToMessages()
        {
            // Arrange
            var result = Results.Error(new List<Message> { new(MessageType.Error, "boom") });
            var opts = new KnightResponseOptions { UseProblemDetails = false, IncludeFullResultPayload = false };
            var http = TestHost.CreateHttpContext(opts);

            // Act
            var actionResult = ApiResults.Ok(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            var response = objectResult.Value.ShouldBeOfType<List<Message>>();
            response.ShouldBe(result.Messages);
        }

        [Fact]
        public void Ok_Failure_IncludeFullResultPayload_True_ReturnsFullResultBody()
        {
            // Arrange
            var result = Results.Error(new List<Message> { new(MessageType.Error, "x") });
            var opts = new KnightResponseOptions { UseProblemDetails = false, IncludeFullResultPayload = true };
            var http   = TestHost.CreateHttpContext(opts);

            // Act
            var actionResult = ApiResults.Ok(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.Value.ShouldBeSameAs(result);
        }

        [Fact]
        public void Ok_Failure_IncludeFullResultPayload_False_ReturnsMessagesBody()
        {
            // Arrange
            var result = Results.Error(new List<Message> { new(MessageType.Error, "x") });
            var opts = new KnightResponseOptions { UseProblemDetails = false, IncludeFullResultPayload = false };
            var http   = TestHost.CreateHttpContext(opts);

            // Act
            var actionResult = ApiResults.Ok(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.Value.ShouldBeAssignableTo<IEnumerable<Message>>();
            var response = objectResult.Value.ShouldBeOfType<List<Message>>();
            response.ShouldBe(result.Messages);
        }

        [Fact]
        public void Created_NonGeneric_Success_WithNullLocation_WritesEmptyLocationHeader()
        {
            // Arrange
            var http   = TestHost.CreateHttpContext();
            var result = Results.Success();

            // Act
            var actionResult = ApiResults.Created(result, http, location: null);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
            http.Response.Headers.ContainsKey(Location).ShouldBeFalse();
        }

        // -------------------- Mutants <T> Killers --------------------

        [Fact]
        public void OkT_Failure_UseProblemDetailsTrue_WithNullHttp_Uses_Default_Opts()
        {
            // Arrange
            var result = Results.Error<Widget>(new List<Message> { new(MessageType.Error, "boom") });

            // Act
            var actionResult = ApiResults.Ok(result, http: null);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            var response = objectResult.Value.ShouldBeOfType<Result<Widget>>();
            response.ShouldBe(result);
        }

        [Fact]
        public void OkT_Failure_UseProblemDetailsFalse_IncludeFullResultPayloadFalse_FallsBackToMessages_NotProblemDetails()
        {
            // Arrange
            var result = Results.Error<Widget>(new List<Message> { new(MessageType.Error, "boom") });
            var opts = new KnightResponseOptions { UseProblemDetails = false, IncludeFullResultPayload = false };
            var http = TestHost.CreateHttpContext(opts);

            // Act
            var actionResult = ApiResults.Ok(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            var response = objectResult.Value.ShouldBeOfType<List<Message>>();
            response.ShouldBe(result.Messages);
        }

        [Fact]
        public void OkT_Failure_UseProblemDetailsFalse_WithHttpNotNull_FallsBackToMessages()
        {
            // Arrange
            var result = Results.Error<Widget>(new List<Message> { new(MessageType.Error, "bad") });
            var opts = new KnightResponseOptions { UseProblemDetails = false, IncludeFullResultPayload = false };
            var http   = TestHost.CreateHttpContext(opts);

            // Act
            var actionResult = ApiResults.Ok(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            var response = objectResult.Value.ShouldBeOfType<List<Message>>();
            response.ShouldBe(result.Messages);
        }

        [Fact]
        public void OkT_Failure_IncludeFullResultPayload_True_ReturnsFullResultBody()
        {
            // Arrange
            var result = Results.Error<Widget>(new List<Message> { new(MessageType.Error, "x") });
            var opts = new KnightResponseOptions { IncludeFullResultPayload = true };
            var http   = TestHost.CreateHttpContext(opts);

            // Act
            var actionResult = ApiResults.Ok(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.Value.ShouldBeSameAs(result);
        }

        [Fact]
        public void OkT_Failure_IncludeFullResultPayload_False_ReturnsMessagesBody()
        {
            // Arrange
            var result = Results.Error<Widget>(new List<Message> { new(MessageType.Error, "x") });
            var opts = new KnightResponseOptions { IncludeFullResultPayload = false };
            var http   = TestHost.CreateHttpContext(opts);

            // Act
            var actionResult = ApiResults.Ok(result, http);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            var response = objectResult.Value.ShouldBeOfType<List<Message>>();
            response.ShouldBe(result.Messages);
        }

        [Fact]
        public void CreatedT_NonGeneric_Success_WithNullLocation_WritesEmptyLocationHeader()
        {
            // Arrange
            var dto = new Widget(2, "2");
            var http   = TestHost.CreateHttpContext();
            var result = Results.Success(dto);

            // Act
            var actionResult = ApiResults.Created(result, http, location: null);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
            http.Response.Headers.ContainsKey(Location).ShouldBeFalse();
        }

        [Fact]
        public void GenericBuilder_Failure_StatusCodeExactly400_DoesNotInvokeResolver()
        {
            var opts = new KnightResponseOptions
            {
                UseProblemDetails = false,
                IncludeFullResultPayload = false,
                StatusCodeResolver = _ => StatusCodes.Status418ImATeapot
            };
            var http = TestHost.CreateHttpContext(opts);

            var failure = Results.Error<Widget>([
                new Message(MessageType.Error, "boom")
            ]);

            const int boundary = StatusCodes.Status400BadRequest;

            // Reflect the private generic builder:
            var mi = typeof(ApiResults)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .First(m => m.Name == "BuildSuccessOrFailure" &&
                            m.IsGenericMethodDefinition &&
                            m.GetParameters().Length == 4);

            var generic = mi.MakeGenericMethod(typeof(Widget));

            // Act
            var actionResult = (IActionResult)generic.Invoke(null, [http, boundary, failure, null])!;

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(boundary);
        }

        [Fact]
        public void GenericBuilder_Failure_StatusCodeBelow400_InvokesResolver()
        {
            var opts = new KnightResponseOptions
            {
                UseProblemDetails = false,
                StatusCodeResolver = _ => StatusCodes.Status422UnprocessableEntity
            };

            var http = TestHost.CreateHttpContext(opts);
            var failure = Results.Error<Widget>([new Message(MessageType.Error, "x")]);

            var methodInfo = typeof(ApiResults)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .First(m => m.Name == "BuildSuccessOrFailure" &&
                            m.IsGenericMethodDefinition &&
                            m.GetParameters().Length == 4);

            var generic = methodInfo.MakeGenericMethod(typeof(Widget));

            // Act
            var actionResult = (IActionResult)generic.Invoke(null, [http, StatusCodes.Status200OK, failure, null])!;

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
