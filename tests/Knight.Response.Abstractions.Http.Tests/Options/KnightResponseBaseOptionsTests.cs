using Knight.Response.Abstractions.Http.Mappers;
using Knight.Response.Abstractions.Http.Options;
using Knight.Response.Models;
using Microsoft.AspNetCore.Http;                  // From Microsoft.AspNetCore.Http.Abstractions (TEST PROJECT ONLY)
using NSubstitute;
using Shouldly;

namespace Knight.Response.Abstractions.Http.Tests.Options;

public class KnightResponseBaseOptionsTests
{
    // ---- Simple POCOs used to close TP / TV -------------------------------

    private sealed class TestProblemDetails
    {
        public string? Title { get; set; }
        public IDictionary<string, object?> Extensions { get; } = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class TestValidationProblemDetails
    {
        public string? Title { get; set; }
        public IDictionary<string, object?> Extensions { get; } = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    }

    // Close the generics for easier testing.
    private sealed class TestOptions : KnightResponseBaseOptions<HttpContext, TestProblemDetails, TestValidationProblemDetails>;

    // ---- Mapper double -----------------------------------------------------

    private sealed class CustomMapper : IValidationErrorMapper
    {
        public IDictionary<string, string[]> Map(IReadOnlyList<Message> messages)
        {
            var dict = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            dict["custom"] = [$"count:{messages.Count}"];
            return dict;
        }
    }

    // -----------------------------------------------------------------------
    //  Basics / Defaults
    // -----------------------------------------------------------------------

    [Fact]
    public void Defaults_Are_Off_And_Default_Mapper_Is_Null()
    {
        // Arrange
        var opts = new TestOptions();

        // Act / Assert
        opts.IncludeFullResultPayload.ShouldBeFalse();
        opts.UseProblemDetails.ShouldBeFalse();
        opts.UseValidationProblemDetails.ShouldBeFalse();
        opts.IncludeExceptionDetails.ShouldBeFalse();

        opts.ValidationMapper.ShouldBeNull();
    }

    [Fact]
    public void Can_Set_All_Boolean_Flags_And_Custom_Mapper()
    {
        // Arrange
        var opts = new TestOptions
        {
            IncludeFullResultPayload      = true,
            UseProblemDetails             = true,
            UseValidationProblemDetails   = true,
            IncludeExceptionDetails       = true,
            ValidationMapper              = new CustomMapper()
        };

        // Act / Assert
        opts.IncludeFullResultPayload.ShouldBeTrue();
        opts.UseProblemDetails.ShouldBeTrue();
        opts.UseValidationProblemDetails.ShouldBeTrue();
        opts.IncludeExceptionDetails.ShouldBeTrue();
        opts.ValidationMapper.ShouldBeOfType<CustomMapper>();
    }

    // -----------------------------------------------------------------------
    //  Builders: ProblemDetails / ValidationProblemDetails
    // -----------------------------------------------------------------------

    [Fact]
    public void ProblemDetailsBuilder_Can_Mutate_Instance_And_Sees_Exact_Objects()
    {
        // Arrange
        var http = Substitute.For<HttpContext>();
        var pd   = new TestProblemDetails();
        var opts = new TestOptions();

        var calls = 0;

        opts.ProblemDetailsBuilder = (ctx, result, problem) =>
        {
            calls++;
            result.ShouldNotBeNull();
            ctx.ShouldBeSameAs(http);         // exact instance
            problem.ShouldBeSameAs(pd);       // exact instance

            // mutation uses the supplied result
            problem.Title = $"R:{result.Status}";
            problem.Extensions["svcStatus"] = result.Status.ToString();
        };

        var failed  = Factories.Results.Failure("boom");
        var cancel  = Factories.Results.Cancel("nah");

        // Act
        opts.ProblemDetailsBuilder!(http, failed, pd);
        pd.Title.ShouldBe("R:Failed");
        pd.Extensions["svcStatus"].ShouldBe("Failed");

        // Mutate same PD again with a different status to ensure delegate re-runs and uses the argument
        opts.ProblemDetailsBuilder!(http, cancel, pd);

        // Assert
        calls.ShouldBe(2);
        pd.Title.ShouldBe("R:Cancelled");
        pd.Extensions["svcStatus"].ShouldBe("Cancelled");
    }

    [Fact]
    public void ValidationBuilder_Can_Mutate_Instance_And_Sees_Exact_Objects()
    {
        // Arrange
        var http = Substitute.For<HttpContext>();
        var vpd  = new TestValidationProblemDetails();
        var opts = new TestOptions();

        var calls = 0;

        opts.ValidationBuilder = (ctx, result, problem) =>
        {
            calls++;
            result.ShouldNotBeNull();
            ctx.ShouldBeSameAs(http);
            problem.ShouldBeSameAs(vpd);

            problem.Title = $"V:{result.Status}";
            problem.Extensions["svcStatus"] = result.Status.ToString();
        };

        var err  = Factories.Results.Error("oops");
        var done = Factories.Results.Success();

        // Act
        opts.ValidationBuilder!(http, err, vpd);
        vpd.Title.ShouldBe("V:Error");
        vpd.Extensions["svcStatus"].ShouldBe("Error");

        opts.ValidationBuilder!(http, done, vpd);

        // Assert
        calls.ShouldBe(2);
        vpd.Title.ShouldBe("V:Completed");
        vpd.Extensions["svcStatus"].ShouldBe("Completed");
    }

    [Fact]
    public void Builders_Are_Optional_And_Null_Safe_For_Caller()
    {
        // Arrange
        var opts   = new TestOptions(); // no builders set
        var http   = Substitute.For<HttpContext>();
        var pd     = new TestProblemDetails();
        var vpd    = new TestValidationProblemDetails();
        var result = Factories.Results.Success();

        // Act / Assert
        // Caller pattern uses null-conditional when invoking; simulate that here:
        Should.NotThrow(() => opts.ProblemDetailsBuilder?.Invoke(http, result, pd));
        Should.NotThrow(() => opts.ValidationBuilder?.Invoke(http, result, vpd));

        // No mutation occurred
        pd.Title.ShouldBeNull();
        vpd.Title.ShouldBeNull();
        pd.Extensions.ShouldBeEmpty();
        vpd.Extensions.ShouldBeEmpty();

        // Also ensure "unsetting" (assigning null) is legal
        opts.ProblemDetailsBuilder = null;
        opts.ValidationBuilder = null;

        opts.ProblemDetailsBuilder.ShouldBeNull();
        opts.ValidationBuilder.ShouldBeNull();
    }

    // -----------------------------------------------------------------------
    //  StatusCodeResolver
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(Status.Failed,    400)]
    [InlineData(Status.Cancelled, 409)]
    [InlineData(Status.Error,     500)]
    [InlineData(Status.Completed, 200)] // default branch
    public void Default_Resolver_Maps_Status_To_Expected_Code(Status status, int expected)
    {
        // Arrange
        var opts = new TestOptions();

        // Act
        var code = opts.StatusCodeResolver(status);

        // Assert
        code.ShouldBe(expected);
    }

    [Fact]
    public void Custom_Resolver_Is_Invoked_And_Can_Map_Arbitrarily()
    {
        // Arrange
        var opts = new TestOptions();

        var calls = 0;
        opts.StatusCodeResolver = s =>
        {
            calls++;
            // Give each status a distinctive non-standard mapping to ensure the delegate is used
            return s switch
            {
                Status.Failed    => 418, // I'm a teapot ☕️
                Status.Cancelled => 299, // custom
                Status.Error     => 599, // custom (non-standard)
                _                => 288  // custom default
            };
        };

        // Act
        var failed    = opts.StatusCodeResolver(Status.Failed);
        var cancelled = opts.StatusCodeResolver(Status.Cancelled);
        var error     = opts.StatusCodeResolver(Status.Error);
        var completed = opts.StatusCodeResolver(Status.Completed);

        // Assert
        calls.ShouldBe(4);
        failed.ShouldBe(418);
        cancelled.ShouldBe(299);
        error.ShouldBe(599);
        completed.ShouldBe(288);
    }

    [Fact]
    public void Resolver_Can_Be_Reassigned_At_Runtime_And_New_Delegate_Is_Used()
    {
        // Arrange
        var opts = new TestOptions();

        // First resolver
        var calls1 = 0;
        opts.StatusCodeResolver = _ => { calls1++; return 207; }; // Multi-Status, arbitrary

        // Act (first phase)
        var phase1 = opts.StatusCodeResolver(Status.Error);

        // Reassign resolver
        var calls2 = 0;
        opts.StatusCodeResolver = _ => { calls2++; return 233; }; // arbitrary again

        // Act (second phase)
        var phase2 = opts.StatusCodeResolver(Status.Error);

        // Assert
        phase1.ShouldBe(207);
        phase2.ShouldBe(233);

        calls1.ShouldBe(1);
        calls2.ShouldBe(1);
    }

    // -----------------------------------------------------------------------
    //  Defaults
    // -----------------------------------------------------------------------

    [Fact]
    public void Defaults_Returns_Fresh_Instance_With_Expected_Defaults()
    {
        // Act
        var def1 = TestOptions.Defaults;
        var def2 = TestOptions.Defaults;

        // Assert – not null and different instances (fresh each time)
        def1.ShouldNotBeNull();
        def2.ShouldNotBeNull();
        ReferenceEquals(def1, def2).ShouldBeFalse();

        // Default feature flags
        def1.IncludeFullResultPayload.ShouldBeFalse();
        def1.IncludeExceptionDetails.ShouldBeFalse();
        def1.UseProblemDetails.ShouldBeFalse();
        def1.UseValidationProblemDetails.ShouldBeFalse();

        // Builders are null by default
        def1.ProblemDetailsBuilder.ShouldBeNull();
        def1.ValidationBuilder.ShouldBeNull();

        // Validation mapper defaulted and usable
        def1.ValidationMapper.ShouldBeNull();

        // Status resolver present and maps as per defaults
        def1.StatusCodeResolver.ShouldNotBeNull();
        def1.StatusCodeResolver(Status.Failed).ShouldBe(400);
        def1.StatusCodeResolver(Status.Cancelled).ShouldBe(409);
        def1.StatusCodeResolver(Status.Error).ShouldBe(500);
        def1.StatusCodeResolver(Status.Completed).ShouldBe(200);
    }


    // -----------------------------------------------------------------------
    //  Mapper behaviour sanity
    // -----------------------------------------------------------------------

    [Fact]
    public void CustomMapper_Is_Used_And_Produces_Expected_Shape()
    {
        // Arrange
        var messages = new[]
        {
            new Message(MessageType.Error, "first"),
            new Message(MessageType.Warning, "second", new Dictionary<string, object?>{{"field","name"}}),
            new Message(MessageType.Information, "third")
        };

        var mapper = new CustomMapper();

        // Act
        var dict = mapper.Map(messages);

        // Assert
        dict.ShouldContainKey("custom");
        dict["custom"].ShouldBe(["count:3"]);
        dict.Keys.Count.ShouldBe(1); // ensure we didn't accidentally emit other keys
    }
}
