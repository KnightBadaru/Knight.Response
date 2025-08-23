using Knight.Response.AspNetCore.Factories;
using Knight.Response.AspNetCore.Mappers;
using Knight.Response.AspNetCore.Options;
using Knight.Response.AspNetCore.Tests.Infrastructure;
using Knight.Response.Models;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;

namespace Knight.Response.AspNetCore.Tests.Factories;

public class ProblemFactoryTests
{
    private sealed class FakeValidationMapper : IValidationErrorMapper
    {
        private readonly bool _yieldErrors;
        public FakeValidationMapper(bool yieldErrors) => _yieldErrors = yieldErrors;

        public IDictionary<string, string[]> Map(IReadOnlyList<Message> messages)
        {
            if (!_yieldErrors)
            {
                return new Dictionary<string, string[]>();
            }

            return new Dictionary<string, string[]>
            {
                ["name"] = messages.Select(m => m.Content).ToArray()
            };
        }
    }

    [Fact]
    public async Task FromResult_When_UseValidationProblemDetails_And_Mapper_YieldsErrors_Returns_VPD()
    {
        // Arrange
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            UseValidationProblemDetails = true,
            StatusCodeResolver = _ => StatusCodes.Status400BadRequest
        };

        var mapper = new FakeValidationMapper(yieldErrors: true);
        var (http, _) = TestHost.CreateHttpContext(opts, mapper);

        var response = Knight.Response.Factories.Results.Failure("name is required");

        // Act
        var result = ApiResults.BadRequest(response, http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);
        body.ShouldContain("\"errors\"");
        body.ShouldContain("\"name\"");
        body.ShouldContain("\"type\":\"https://httpstatuses.io/400\"");
        body.ShouldContain("\"svcStatus\":\"Failed\"");
    }

    [Fact]
    public async Task FromResult_When_UseProblemDetails_With_No_FieldErrors_Returns_PD()
    {
        // Arrange
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            UseValidationProblemDetails = true,   // enabled but mapper returns no field errors
            StatusCodeResolver = _ => StatusCodes.Status400BadRequest
        };

        var mapper = new FakeValidationMapper(yieldErrors: false);
        var (http, _) = TestHost.CreateHttpContext(opts, mapper);

        var response = Knight.Response.Factories.Results.Failure([
            new(MessageType.Error, "first"),
            new(MessageType.Error, "second")]);

        // Act
        var result = ApiResults.BadRequest(response, http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);
        body.ShouldContain("\"type\":\"https://httpstatuses.io/400\"");
        body.ShouldContain("\"svcStatus\":\"Failed\"");
        body.ShouldContain("\"messages\"");
    }

    [Fact]
    public async Task FromResult_WhenMapperYieldsNoErrors_FallsBackTo_ProblemDetails()
    {
        // Arrange
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            UseValidationProblemDetails = true
        };

        var mapper = Substitute.For<IValidationErrorMapper>();
        mapper.Map(Arg.Any<IReadOnlyList<Message>>()).Returns(new Dictionary<string, string[]>());

        var (http, _) = TestHost.CreateHttpContext(opts, mapper);
        var response = Knight.Response.Factories.Results.Failure("bad input");

        // Act
        var result = ApiResults.BadRequest(response, http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);
        body.ShouldContain("\"type\":\"https://httpstatuses.io/400\"");
        body.ShouldContain("\"svcStatus\"");
        body.ShouldContain("\"messages\"");
        body.ShouldNotContain("\"errors\""); // not VPD
    }

    [Fact]
    public async Task ProblemDetailsBuilder_CanMutatePayload()
    {
        // Arrange
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            ProblemDetailsBuilder = (ctx, result, pd) =>
            {
                pd.Title = "custom title";
                pd.Extensions["tenant"] = "knight";
            }
        };
        var (http, _) = TestHost.CreateHttpContext(opts);
        var response = Knight.Response.Factories.Results.Failure("oops");

        // Act
        // var result = Knight.Response.AspNetCore.Factories.ProblemFactory.FromResult(http, opts, res);
        var result = ApiResults.BadRequest(response, http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);
        body.ShouldContain("\"custom title\"");
        body.ShouldContain("\"tenant\":\"knight\"");
    }

    [Fact]
    public async Task ValidationBuilder_CanMutate_VPD()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            ["name"] = new[] { "required" }
        };
        var mapper = Substitute.For<IValidationErrorMapper>();
        mapper.Map(Arg.Any<IReadOnlyList<Message>>()).Returns(errors);

        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            UseValidationProblemDetails = true,
            ValidationBuilder = (ctx, result, vpd) =>
            {
                vpd.Title = "invalid data";
                vpd.Extensions["hint"] = "fix fields";
            }
        };

        var (http, _) = TestHost.CreateHttpContext(opts, mapper);
        var response = Knight.Response.Factories.Results.Failure("bad");

        // Act
        var result = ApiResults.BadRequest(response, http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);
        body.ShouldContain("\"invalid data\"");
        body.ShouldContain("\"errors\"");
        body.ShouldContain("\"name\"");
        body.ShouldContain("\"hint\":\"fix fields\"");
    }

    [Fact]
    public async Task StatusCodeResolver_Is_Honored_For_PD()
    {
        // Arrange
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            StatusCodeResolver = _ => 422
        };
        var (http, _) = TestHost.CreateHttpContext(opts);
        var response = Knight.Response.Factories.Results.Failure("oops");

        // Act
        var result = ApiResults.Ok(response, http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(422);
        body.ShouldContain("\"type\":\"https://httpstatuses.io/422\"");
    }
}
