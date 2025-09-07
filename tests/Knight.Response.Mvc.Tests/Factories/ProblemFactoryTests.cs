using Knight.Response.Core;
using Knight.Response.Factories;
using Knight.Response.Models;
using Knight.Response.Mvc.Factories;
using Knight.Response.Mvc.Infrastructure;
using Knight.Response.Mvc.Options;
using Knight.Response.Mvc.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Knight.Response.Mvc.Tests.Factories
{
    public class ProblemFactoryTests
    {
        private static Result Failure(params string[] messages)
        {
            var msg = messages.Select(m => new Message(MessageType.Error, m)).ToList();
            return Results.Error(msg);
        }

        [Fact]
        public void ProblemDetails_WhenUseProblemDetailsTrue_ProducesCompatProblem_WithStatus()
        {
            // Arrange
            var opts = new KnightResponseOptions { UseProblemDetails = true };
            var http = TestHost.CreateHttpContext(opts);
            var result = Failure("Something went wrong");
            var status = StatusCodes.Status400BadRequest;

            // Act
            var actionResult = ProblemFactory.FromResult(http, opts, result, status);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(status);
            objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        }

        [Fact]
        public void ValidationProblemDetails_WhenEnabled_AndValidationPresent_UsesCompatValidationProblemDetails()
        {
            // Arrange
            const string errorKey1 = "Name";
            const string errorKey2 = "Amount";
            const string errorMessage1 = "Name is required.";
            const string errorMessage2 = "Amount: Must be greater than 0";

            var opts = new KnightResponseOptions { UseProblemDetails = true, UseValidationProblemDetails = true };
            var http = TestHost.CreateHttpContext(opts);
            var result = Failure($"{errorKey1}: {errorMessage1}", $"{errorKey2}: {errorMessage2}");
            const int status = StatusCodes.Status400BadRequest;

            // Act
            var actionResult = ProblemFactory.FromResult(http, opts, result, status);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(status);
            var response = objectResult.Value.ShouldBeOfType<CompatValidationProblemDetails>();
            response.Errors.Keys.ShouldContain(errorKey1);
            response.Errors.Keys.ShouldContain(errorKey2);
        }

        [Fact]
        public void ValidationEnabled_ButNoValidationErrors_FallsBackToCompatProblemDetails()
        {
            // Arrange
            var opts = new KnightResponseOptions { UseProblemDetails = true,  UseValidationProblemDetails = true };
            var http = TestHost.CreateHttpContext(opts);
            var result = Results.Failure("General error without field");
            const int status = StatusCodes.Status400BadRequest;

            // Act
            var actionResult = ProblemFactory.FromResult(http, opts, result, status);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(status);
            objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        }

        [Fact]
        public void StatusCode_Propagates_ToObjectResult()
        {
            // Arrange
            var opts = new KnightResponseOptions { UseProblemDetails = true };
            var http = TestHost.CreateHttpContext(opts);
            var input = Failure("Oops");
            const int status = StatusCodes.Status409Conflict;

            // Act
            var actionResult = ProblemFactory.FromResult(http, opts, input, status);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(status);
            objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        }

        [Fact]
        public void FromResult_ReturnsObjectResult_WithExpectedValueType()
        {
            // Arrange
            const string errorMessage1 = "Something broke";
            const string errorMessage2 = "And broke badly";

            var opts = new KnightResponseOptions { UseProblemDetails = true };
            var http = TestHost.CreateHttpContext(opts);
            var result = Failure(errorMessage1, errorMessage2);
            const int status = StatusCodes.Status500InternalServerError;

            // Act
            var actionResult = ProblemFactory.FromResult(http, opts, result, status);

            // Assert
            var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(status);
            var pd = objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
            pd.Detail.ShouldNotBeNullOrEmpty(); // Because there are multiple error messages.
            pd.Detail.ShouldContain(errorMessage1);
            pd.Detail.ShouldContain(errorMessage2);
            pd.Detail.ShouldContain(";");
        }
    }
}
