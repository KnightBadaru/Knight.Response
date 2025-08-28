using Knight.Response.Abstractions.Http.Mappers;
using Knight.Response.AspNetCore.Factories;
using Knight.Response.AspNetCore.Options;
using Knight.Response.AspNetCore.Tests.Infrastructure;
using Knight.Response.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        const string content = "name is required";
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            UseValidationProblemDetails = true,
            StatusCodeResolver = _ => StatusCodes.Status400BadRequest
        };

        var mapper = new FakeValidationMapper(yieldErrors: true);
        var (http, _) = TestHost.CreateHttpContext(opts, mapper);

        var response = Knight.Response.Factories.Results.Failure(content);

        // Act
        var result = ApiResults.BadRequest(response, http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);

        var vpd = TestHelpers.Deserialize<ValidationProblemDetails>(body);
        vpd.ShouldNotBeNull();
        vpd.Title.ShouldBe("One or more validation errors occurred.");
        vpd.Type.ShouldBe(TestHelpers.HttpStatusUrl());
        vpd.Status.ShouldBe(StatusCodes.Status400BadRequest);

        // key -> name, as set in FakeValidationMapper
        vpd.Errors.ShouldContain(e => e.Key == "name" &&  e.Value.Contains(content));

        TestHelpers.SvcStatusShouldBeFailed(vpd);
        var messages = TestHelpers.Ext<List<Message>>(vpd, "messages");
        messages.ShouldNotBeEmpty();
        messages.ShouldContain(m => m.Content == content);
    }

    [Fact]
    public async Task FromResult_When_UseProblemDetails_With_No_FieldErrors_Returns_PD()
    {
        // Arrange
        const string content1 = "first";
        const string content2 = "second";
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            UseValidationProblemDetails = true,   // enabled but mapper returns no field errors
            StatusCodeResolver = _ => StatusCodes.Status400BadRequest
        };

        var mapper = new FakeValidationMapper(yieldErrors: false);
        var (http, _) = TestHost.CreateHttpContext(opts, mapper);

        var response = Knight.Response.Factories.Results.Failure([
            new(MessageType.Error, content1),
            new(MessageType.Error, content2)]);

        // Act
        var result = ApiResults.BadRequest(response, http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);

        var pd = TestHelpers.Deserialize<ProblemDetails>(body);
        pd.ShouldNotBeNull();
        pd.Title.ShouldBe(content1);
        pd.Detail.ShouldBe($"{content1}; {content2}");
        pd.Type.ShouldBe(TestHelpers.HttpStatusUrl());
        pd.Status.ShouldBe(StatusCodes.Status400BadRequest);

        TestHelpers.SvcStatusShouldBeFailed(pd);
        var messages = TestHelpers.Ext<List<Message>>(pd, "messages");
        messages.Count.ShouldBe(2);
        messages.ShouldContain(m => m.Type == MessageType.Error && m.Content == content1);
        messages.ShouldContain(m => m.Type == MessageType.Error && m.Content == content2);
        body.ShouldNotContain("\"errors\""); // not VPD
    }

    [Fact]
    public async Task FromResult_WhenMapperYieldsNoErrors_FallsBackTo_ProblemDetails()
    {
        // Arrange
        const string content = "bad input!";
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            UseValidationProblemDetails = true
        };

        var mapper = Substitute.For<IValidationErrorMapper>();
        mapper.Map(Arg.Any<IReadOnlyList<Message>>()).Returns(new Dictionary<string, string[]>());

        var (http, _) = TestHost.CreateHttpContext(opts, mapper);
        var response = Knight.Response.Factories.Results.Failure(content);

        // Act
        var result = ApiResults.BadRequest(response, http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);

        var pd = TestHelpers.Deserialize<ProblemDetails>(body);
        pd.ShouldNotBeNull();
        pd.Title.ShouldBe(content);
        pd.Type.ShouldBe(TestHelpers.HttpStatusUrl());
        pd.Status.ShouldBe(StatusCodes.Status400BadRequest);

        TestHelpers.SvcStatusShouldBeFailed(pd);
        var messages = TestHelpers.Ext<List<Message>>(pd, "messages");
        messages.ShouldContain(m => m.Type == MessageType.Error && m.Content == content);
        body.ShouldNotContain("\"errors\""); // not VPD
    }

    [Fact]
    public async Task ProblemDetailsBuilder_CanMutatePayload()
    {
        // Arrange
        const string error = "huh?";
        const string title = "Custom Title";
        const string ext = "Tenant";
        const string extValue= "Knight";

        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            ProblemDetailsBuilder = (_, _, pd) =>
            {
                pd.Title = title;
                pd.Extensions[ext] = extValue;
            }
        };
        var (http, _) = TestHost.CreateHttpContext(opts);
        var response = Knight.Response.Factories.Results.Failure(error);

        // Act
        var result = ApiResults.BadRequest(response, http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);

        var pd = TestHelpers.Deserialize<ProblemDetails>(body);
        pd.ShouldNotBeNull();
        pd.Title.ShouldBe(title);
        pd.Type.ShouldBe(TestHelpers.HttpStatusUrl());
        pd.Status.ShouldBe(StatusCodes.Status400BadRequest);

        TestHelpers.SvcStatusShouldBeFailed(pd);
        TestHelpers.ExtString(pd, ext).ShouldBe(extValue);
    }

    [Fact]
    public async Task ValidationBuilder_CanMutate_VPD()
    {
        // Arrange
        const string validationTitle = "Invalid Data";
        const string error = "Very Bad Input!";
        const string ext = "Hint";
        const string extValue= "Fix Field";
        var errors = new Dictionary<string, string[]>
        {
            ["name"] = ["required"]
        };
        var mapper = Substitute.For<IValidationErrorMapper>();
        mapper.Map(Arg.Any<IReadOnlyList<Message>>()).Returns(errors);

        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            UseValidationProblemDetails = true,
            ValidationBuilder = (_, _, vpd) =>
            {
                vpd.Title = validationTitle;
                vpd.Extensions[ext] = extValue;
            }
        };

        var (http, _) = TestHost.CreateHttpContext(opts, mapper);
        var response = Knight.Response.Factories.Results.Failure(error);

        // Act
        var result = ApiResults.BadRequest(response, http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);

        var vpd = TestHelpers.Deserialize<ValidationProblemDetails>(body);
        vpd.ShouldNotBeNull();
        vpd.Title.ShouldBe(validationTitle);
        vpd.Type.ShouldBe(TestHelpers.HttpStatusUrl());
        vpd.Status.ShouldBe(StatusCodes.Status400BadRequest);
        TestHelpers.SvcStatusShouldBeFailed(vpd);
        TestHelpers.ExtString(vpd, ext).ShouldBe(extValue);
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

    // -------------------- Mutant War! --------------------

    [Fact]
    public async Task ProblemDetailsBuilder_Is_Invoked_And_Can_Customize_Extensions()
    {
        // Arrange
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            UseValidationProblemDetails = false,
            StatusCodeResolver = _ => StatusCodes.Status400BadRequest,
            ProblemDetailsBuilder = (_, _, pd) =>
            {
                pd.Extensions["trace"] = "abc-123";
            }
        };
        var (http, _) = TestHost.CreateHttpContext(opts);
        var response = Knight.Response.Factories.Results.Failure("oops");

        // Act
        var result = ApiResults.BadRequest(response, http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);
        var pd = TestHelpers.Deserialize<ProblemDetails>(body)!;

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);
        TestHelpers.ExtString(pd, "svcStatus").ShouldBe("Failed");
        TestHelpers.ExtString(pd, "trace").ShouldBe("abc-123");
    }

    [Fact]
    public async Task ValidationBuilder_Is_Invoked_And_Can_Customize_Extensions()
    {
        // Arrange
        var mapper = new FakeValidationMapper(yieldErrors: true);
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            UseValidationProblemDetails = true,
            StatusCodeResolver = _ => StatusCodes.Status400BadRequest,
            ValidationBuilder = (_, _, vpd) =>
            {
                vpd.Extensions["hint"] = "check-forms";
            }
        };
        var (http, _) = TestHost.CreateHttpContext(opts, mapper);
        var response = Knight.Response.Factories.Results.Failure("email: required");

        // Act
        var result = ApiResults.BadRequest(response, http);
        var (status, body, _) = await TestHost.ExecuteAsync(result, http);

        // Assert
        status.ShouldBe(StatusCodes.Status400BadRequest);
        var vpd = TestHelpers.Deserialize<ValidationProblemDetails>(body)!;
        vpd.Errors.ShouldContainKey("name");
        TestHelpers.ExtString(vpd, "hint").ShouldBe("check-forms");
    }
}
