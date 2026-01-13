using System.ComponentModel.DataAnnotations;
using Knight.Response.Abstractions.Http.Mappers;
using Knight.Response.Factories;
using Knight.Response.Models;
using Knight.Response.Mvc.Factories;
using Knight.Response.Mvc.Infrastructure;
using Knight.Response.Mvc.Options;
using Knight.Response.Mvc.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Knight.Response.Mvc.Tests.Factories;

public class ProblemFactoryTests
{
    [Fact]
    public void ProblemDetails_WhenUseProblemDetailsTrue_ProducesCompatProblem_WithStatus()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var http = TestHost.CreateHttpContext(opts);
        var result = TestHelpers.Failure("Something went wrong");
        var status = StatusCodes.Status400BadRequest;

        // Act
        var actionResult = ProblemFactory.FromResult(http, opts, result, status);

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(status);
        objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
    }

    //This is for next version when using Knight.Response v2
    [Fact]
    public void ValidationProblemDetails_WhenEnabled_AndValidationPresent_UsesCompatValidationProblemDetails()
    {
        // Arrange
        const string errorKey1 = "Name";
        const string errorKey2 = "Amount";
        const string errorMessage1 = "Name is required.";
        const string errorMessage2 = "Amount: Must be greater than 0";

        var opts = new KnightResponseOptions { UseProblemDetails = true, UseValidationProblemDetails = true };
        var http = TestHost.CreateHttpContext<DefaultValidationErrorMapper>(opts);

        var result = Results.ValidationFailure([
            new Message(
                MessageType.Error, errorMessage1, metadata: new Dictionary<string, object?> { ["field"] = errorKey1 }),
            new Message(
                MessageType.Error, errorMessage2, metadata: new Dictionary<string, object?> { ["field"] = errorKey2 })
        ]);

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
        var input = TestHelpers.Failure("Oops");
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
        var result = TestHelpers.Failure(errorMessage1, errorMessage2);
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

    [Fact]
    public void ProblemDetails_Sets_Type_And_Extensions_With_Expected_Keys_And_Values()
    {
        // Arrange
        var http = TestHost.CreateHttpContext(new KnightResponseOptions { UseProblemDetails = true });
        var opts = http.RequestServices.GetRequiredService<IOptions<KnightResponseOptions>>().Value;

        // Two messages to exercise detail + extensions population
        var error1 = "First error";
        var error2 = "Second error";
        var result = Results.Error(
            new List<Message>
            {
                new(MessageType.Error, error1),
                new(MessageType.Error, error2)
            });

        const int status = StatusCodes.Status400BadRequest;

        // Act
        var actionResult = ProblemFactory.FromResult(http, opts, result, status);

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(status);
        var pd = objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        pd.Type.ShouldBe(TestHelpers.HttpStatusUrl()); // statusCode is default to 400 as test
        pd.Extensions.ShouldContainKey(TestHelpers.SvcStatus);
        pd.Extensions.ShouldContainKey("messages");
        pd.Extensions[TestHelpers.SvcStatus].ShouldBe(result.Status.ToString());
        var messages = pd.Extensions["messages"].ShouldBeOfType<object[]>();
        messages.Length.ShouldBe(result.Messages.Count);
    }

    [Fact]
    public void Ctor_NullEntryValues_Become_EmptyArrays_Not_Null()
    {
        // Arrange
        const string key1 = "name";
        const string key2 = "age";
        const string value2 = "invalid";
        var errors = new Dictionary<string, string[]?>
        {
            [key1] = null,
            [key2]  = [value2]
        };

        // Act
        var vpd = new CompatValidationProblemDetails(errors!);

        // Assert
        vpd.Errors.ShouldContainKey(key1);
        vpd.Errors[key1].ShouldNotBeNull();
        vpd.Errors[key1].ShouldBeEmpty();

        vpd.Errors.ShouldContainKey(key2);
        vpd.Errors[key2].ShouldBe([value2]);
    }

    [Fact]
    public void ProblemFactory_Uses_ScopedMapper_From_RequestServices()
    {
        var opts = new  KnightResponseOptions
        {
            UseProblemDetails = true,
            UseValidationProblemDetails = true
        };
        var http = TestHost.CreateHttpContext<HyphenMapper>(opts);

        var result = Results.Error([new Message(MessageType.Error, "Field: bad")]);

        // Act
        var action = ProblemFactory.FromResult(http, opts, result, StatusCodes.Status400BadRequest);

        // Assert
        var obj = action.ShouldBeOfType<ObjectResult>();
        var vpd = obj.Value.ShouldBeOfType<CompatValidationProblemDetails>();
        vpd.Errors.Keys.ShouldContain("_"); // came from the custom mapper
    }


    // -------------------- Mutants Killers --------------------
    // ============================================================================
    // // Mutation Testing Summary for ProblemFactory
    // // ============================================================================
    // // These tests are intentionally very explicit to guard against subtle mutants
    // // that Stryker can generate. They ensure we keep a 100% mutation score.
    // //
    // // Covered mutants:
    // //
    // // ValidationProblemDetails Title literal:
    // //    - Test asserts exact string: "One or more validation errors occurred."
    // //
    // // Type string interpolation:
    // //    - Test asserts URL format: $"https://httpstatuses.io/{code}"
    // //
    // // Extensions dictionary keys:
    // //    - Tests assert presence of "svcStatus" and "messages"
    // //
    // // Builder invocation (ValidationBuilder / ProblemDetailsBuilder):
    // //    - Tests confirm delegates are invoked and can mutate PD/VPD
    // //
    // // Message projection via ToShallow:
    // //    - Test reflects into anonymous objects to confirm `type`, `content`,
    // //      and `metadata` are included
    // //
    // // Title/Detail fallbacks:
    // //    - Zero messages → Title falls back to result.Status
    // //    - One message  → Title = message.Content, Detail = null
    // //    - >1 messages  → Title = first.Content, Detail = joined string
    // //
    // // Without these, Stryker survivors creep in by flipping
    // //   - && to ||
    // //   - > 1 to >= 1
    // //   - dropping ?? right-hand side
    // //   - removing builder invocation statements
    // //
    // // KEEP THESE TESTS as a safety net to maintain a mutation score of 100%.
    // //
    // // ============================================================================

    // This is for next version when using Knight.Response v2
    // [Fact]
    // public void ValidationProblemDetails_Title_Is_Set_AsExpected()
    // {
    //     // Arrange
    //     var opts = new KnightResponseOptions { UseProblemDetails = true, UseValidationProblemDetails = true };
    //     var http = TestHost.CreateHttpContext(opts);
    //     var result = Results.Error(new List<Message>
    //     {
    //         new(MessageType.Error, "Name: missing")
    //     });
    //
    //     // Act
    //     var actionResult = ProblemFactory.FromResult(http, opts, result, StatusCodes.Status400BadRequest);
    //
    //     // Assert
    //     var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
    //     var vpd = objectResult.Value.ShouldBeOfType<CompatValidationProblemDetails>();
    //     vpd.Title.ShouldBe("One or more validation errors occurred.");
    // }
    //
    // [Fact]
    // public void ValidationProblemDetails_Type_ContainsStatusCode()
    // {
    //     // Arrange
    //     const int code = StatusCodes.Status422UnprocessableEntity;
    //     var opts = new KnightResponseOptions { UseProblemDetails = true, UseValidationProblemDetails = true };
    //     var http = TestHost.CreateHttpContext(opts);
    //     var result = Results.Error(new List<Message> { new(MessageType.Error, "X: fail") });
    //
    //     // Act
    //     var actionResult = ProblemFactory.FromResult(http, opts, result, code);
    //
    //     // Assert
    //     var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
    //     var vpd = objectResult.Value.ShouldBeOfType<CompatValidationProblemDetails>();
    //     vpd.Type.ShouldBe(TestHelpers.HttpStatusUrl(code));
    // }
    //
    // [Fact]
    // public void ValidationProblemDetails_Includes_Extensions_SvcStatus_And_Messages()
    // {
    //     // Arrange
    //     var opts =  new KnightResponseOptions { UseProblemDetails = true, UseValidationProblemDetails = true };
    //     var http = TestHost.CreateHttpContext(opts);
    //     var result = Results.Error(new List<Message>
    //     {
    //         new(MessageType.Error, "Field: bad"),
    //         new(MessageType.Error, "Field: worse")
    //     });
    //
    //     // Act
    //     var actionResult = ProblemFactory.FromResult(http, opts, result, StatusCodes.Status400BadRequest);
    //
    //     // Assert
    //     var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
    //     var vpd = objectResult.Value.ShouldBeOfType<CompatValidationProblemDetails>();
    //     vpd.Extensions.ContainsKey(TestHelpers.SvcStatus).ShouldBeTrue();
    //     vpd.Extensions.ContainsKey("messages").ShouldBeTrue();
    // }

    [Fact]
    public void ProblemDetails_Title_UsesFirstMessage_WhenSingleMessage()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var http = TestHost.CreateHttpContext(opts);
        var result = Results.Error(new List<Message> { new(MessageType.Error, "oops") });

        // Act
        var actionResult = ProblemFactory.FromResult(http, opts, result, StatusCodes.Status500InternalServerError);

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        var pd = objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        pd.Title.ShouldBe("oops");
    }

    [Fact]
    public void ProblemDetails_Detail_JoinsMessages_WhenMultipleMessages()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var http = TestHost.CreateHttpContext(opts);
        var result = Results.Error(new List<Message>
        {
            new(MessageType.Error, "first"),
            new(MessageType.Error, "second")
        });

        // Act
        var actionResult = ProblemFactory.FromResult(http, opts, result, StatusCodes.Status500InternalServerError);

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        var pd = objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        pd.Detail.ShouldContain("first");
        pd.Detail.ShouldContain("second");
    }

    [Fact]
    public void ProblemDetails_Type_ContainsStatusCode()
    {
        // Arrange
        const int code = StatusCodes.Status403Forbidden;
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var http = TestHost.CreateHttpContext(opts);
        var result = Results.Error(new List<Message> { new(MessageType.Error, "denied") });

        // Act
        var actionResult = ProblemFactory.FromResult(http, opts, result, code);

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        var pd = objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        pd.Type.ShouldBe(TestHelpers.HttpStatusUrl(code));
    }

    [Fact]
    public void ProblemDetails_Includes_Extensions_SvcStatus_And_Messages()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var http = TestHost.CreateHttpContext(opts);
        var result = Results.Error(new List<Message> { new(MessageType.Error, "broken") });

        // Act
        var action = ProblemFactory.FromResult(http, opts, result, StatusCodes.Status500InternalServerError);

        // Assert
        var obj = action.ShouldBeOfType<ObjectResult>();
        var pd = obj.Value.ShouldBeOfType<CompatProblemDetails>();
        pd.Extensions.ContainsKey(TestHelpers.SvcStatus).ShouldBeTrue();
        pd.Extensions.ContainsKey("messages").ShouldBeTrue();
    }

    // This is for next version when using Knight.Response v2
    // [Fact]
    // public void ValidationProblemDetails_Invokes_Custom_ValidationBuilder()
    // {
    //     // Arrange
    //     var called = false;
    //     var opts = new KnightResponseOptions
    //     {
    //         UseProblemDetails = true,
    //         UseValidationProblemDetails = true,
    //         ValidationBuilder = (_, _, vpd) =>
    //         {
    //             called = true;
    //             vpd.Title = "custom-vpd";
    //             vpd.Extensions["custom"] = true;
    //         }
    //     };
    //     var http = TestHost.CreateHttpContext(opts);
    //     var result = Results.Error([
    //         new Message(MessageType.Error, "Name: missing")
    //     ]);
    //
    //     // Act
    //     var actionResult = ProblemFactory.FromResult(http, opts, result, StatusCodes.Status400BadRequest);
    //
    //     // Assert
    //     var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
    //     var vpd = objectResult.Value.ShouldBeOfType<CompatValidationProblemDetails>();
    //     called.ShouldBeTrue();
    //     vpd.Title.ShouldBe("custom-vpd");
    //     vpd.Extensions.ContainsKey("custom").ShouldBeTrue();
    // }

    [Fact]
    public void ProblemDetails_Invokes_Custom_ProblemDetailsBuilder()
    {
        // Arrange
        var called = false;
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            ProblemDetailsBuilder = (_, _, pd) =>
            {
                called = true;
                pd.Title = "custom-pd";
                pd.Extensions["x"] = 1;
            }
        };
        var http = TestHost.CreateHttpContext(opts);
        var result = Results.Error([new Message(MessageType.Error, "boom")]);

        // Act
        var actionResult = ProblemFactory.FromResult(http, opts, result, StatusCodes.Status500InternalServerError);

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        var pd = objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        called.ShouldBeTrue();
        pd.Title.ShouldBe("custom-pd");
        pd.Extensions.ContainsKey("x").ShouldBeTrue();
    }

    [Fact]
    public void ProblemDetails_NoMessages_TitleFallsBackToStatus_DetailIsNull()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var http = TestHost.CreateHttpContext(opts);
        var result = Results.Error([]);

        // Act
        var actionResult = ProblemFactory.FromResult(http, opts, result, StatusCodes.Status403Forbidden);

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        var pd = objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        pd.Title.ShouldBe(result.Status.ToString());
        pd.Detail.ShouldBeNull();
    }

    [Fact]
    public void ProblemDetails_MultipleMessages_DetailIsJoinedString()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var http = TestHost.CreateHttpContext(opts);
        var result = Results.Error([
            new Message(MessageType.Error, "first"),
            new Message(MessageType.Error, "second")
        ]);

        // Act
        var actionResult = ProblemFactory.FromResult(http, opts, result, StatusCodes.Status500InternalServerError);

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        var pd = objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        pd.Detail.ShouldNotBeNull();
        pd.Detail!.ShouldContain("first");
        pd.Detail.ShouldContain("second");
    }

    [Fact]
    public void ProblemDetails_MessagesExtension_ContainsShallowShape()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var http = TestHost.CreateHttpContext(opts);
        var result = Results.Error([
            new Message(MessageType.Error, "content-1", new Dictionary<string, object?>{{"k","v"}})
        ]);

        // Act
        var actionResult = ProblemFactory.FromResult(http, opts, result, StatusCodes.Status400BadRequest);

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        var pd = objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        var arr = pd.Extensions["messages"].ShouldBeOfType<object[]>();
        arr.Length.ShouldBe(1);

        // reflect anonymous ToShallow object to ensure properties exist
        var shallow = arr[0];
        var t = shallow.GetType();
        t.GetProperty("type").ShouldNotBeNull();
        t.GetProperty("content").ShouldNotBeNull();
        t.GetProperty("metadata").ShouldNotBeNull();

        // and sample values
        t.GetProperty("type")!.GetValue(shallow)!.ToString().ShouldBe(nameof(MessageType.Error));
        t.GetProperty("content")!.GetValue(shallow)!.ToString().ShouldBe("content-1");
    }

    [Fact]
    public void ProblemDetails_SingleMessage_DetailIsNull()
    {
        // Arrange
        var opts = new KnightResponseOptions { UseProblemDetails = true };
        var http = TestHost.CreateHttpContext(opts);
        var result = Results.Error([
            new Message(MessageType.Error, "only-one")
        ]);

        // Act
        var actionResult = ProblemFactory.FromResult(http, opts, result, StatusCodes.Status500InternalServerError);

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        var pd  = objectResult.Value.ShouldBeOfType<CompatProblemDetails>();
        pd.Title.ShouldBe("only-one");
        pd.Detail.ShouldBeNull();
    }
}
