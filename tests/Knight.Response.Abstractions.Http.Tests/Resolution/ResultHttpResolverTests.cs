using Knight.Response.Abstractions.Http.Options;
using Knight.Response.Abstractions.Http.Resolution;
using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Abstractions.Http.Tests.Resolution;

public class ResultHttpResolverTests
{
    // ---------------------------
    // Untyped Result
    // ---------------------------

    [Fact]
    public void ResolveHttpCode_Untyped_Uses_CodeToHttp_When_Provided()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            CodeToHttp = c => c?.Value == ResultCodes.NotFound.Value ? 404 : null,
            // Status fallback won't be hit
        };
        var result = Results.Failure("nope").WithCode(ResultCodes.NotFound);

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(404);
    }

    [Fact]
    public void ResolveHttpCode_Untyped_Falls_Back_To_Status_When_CodeToHttp_Returns_Null()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            CodeToHttp = _ => null, // explicit null forces fallback
            StatusCodeResolver = KnightResponseHttpDefaults.StatusToHttp
        };
        var result = Results.Failure("nope"); // Status.Failed -> 400

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(400);
    }

    [Fact]
    public void ResolveHttpCode_Untyped_Falls_Back_To_Status_When_CodeToHttp_Is_Null()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            CodeToHttp = null,
            StatusCodeResolver = KnightResponseHttpDefaults.StatusToHttp
        };
        var result = Results.Error("boom"); // Status.Error -> 500

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(500);
    }

    [Fact]
    public void ResolveHttpCode_Untyped_With_Null_Code_Uses_Status_Fallback()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            CodeToHttp = c => c is null ? null : 418,
            StatusCodeResolver = KnightResponseHttpDefaults.StatusToHttp
        };
        var result = Results.Success(); // Completed -> 200 (Code is null)

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(200);
    }

    // ---------------------------
    // Typed Result<T>
    // ---------------------------

    [Fact]
    public void ResolveHttpCode_Typed_Uses_CodeToHttp_When_Provided()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            CodeToHttp = c => c?.Value == ResultCodes.AlreadyExists.Value ? 409 : null
        };
        var result = Results.Failure<int>("exists").WithCode(ResultCodes.AlreadyExists);

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(409);
    }

    [Fact]
    public void ResolveHttpCode_Typed_Fallbacks_To_Status_When_CodeToHttp_Is_Null()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            CodeToHttp = null,
            StatusCodeResolver = KnightResponseHttpDefaults.StatusToHttp
        };
        var result = Results.Cancel<int>("cancelled"); // Cancelled -> 409

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(409);
    }

    [Fact]
    public void ResolveHttpCode_Typed_With_Null_Code_Uses_Status_Fallback()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            // Invoked with null; returns null → fallback to Status mapping
            CodeToHttp = _ => null,
            StatusCodeResolver = KnightResponseHttpDefaults.StatusToHttp
        };
        var result = Results.Success(value: 123, code: null); // Completed -> 200

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(200);
    }

    [Fact]
    public void ResolveHttpCode_Typed_With_Null_Code_Uses_CodeToHttp_When_It_Returns_Value()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            // Always returns 418, even for null codes
            CodeToHttp = _ => 418,
            StatusCodeResolver = KnightResponseHttpDefaults.StatusToHttp
        };
        var result = Results.Success(value: 1, code: null);

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(418);
    }

    [Fact]
    public void ResolveHttpCode_Typed_With_No_CodeToHttp_Uses_Status_Mapping()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            CodeToHttp = null,
            StatusCodeResolver = KnightResponseHttpDefaults.StatusToHttp
        };
        var result = Results.Error<int>("boom"); // Status.Error -> 500

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(500);
    }

    // ---------------------------
    // Defaults mapping coverage
    // ---------------------------

    [Theory]
    [InlineData(Status.Completed, 200)]
    [InlineData(Status.Failed,    400)]
    [InlineData(Status.Cancelled, 409)]
    [InlineData(Status.Error,     500)]
    public void KnightResponseHttpDefaults_StatusToHttp_Covers_All_Status(Status status, int expected)
    {
        // Arrange
        // Act
        var mapped = KnightResponseHttpDefaults.StatusToHttp(status);

        // Assert
        mapped.ShouldBe(expected);
    }

    [Fact]
    public void CodeToHttp_Takes_Precedence_When_Returns_NonNull()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            CodeToHttp = c => c?.Value == ResultCodes.NotFound.Value ? 404 : null,
            StatusCodeResolver = s => s == Status.Error ? 500 : 200
        };
        var result = Results.NotFound().WithCode(ResultCodes.NotFound);

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(404);
    }

    [Fact]
    public void Falls_Back_To_Status_When_CodeToHttp_Null_Or_Returns_Null()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            CodeToHttp = _ => null,
            StatusCodeResolver = KnightResponseHttpDefaults.StatusToHttp
        };
        var result = Results.Error("boom");

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(500);
    }

    [Fact]
    public void ResolveHttpCode_With_Code_Uses_CodeToHttp_Defaults()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            CodeToHttp = KnightResponseHttpDefaults.CodeToHttp,
            StatusCodeResolver = null // ensure we hit code mapping first
        };

        var result = Results.NotFound(message: "missing"); // defaults Code => ResultCodes.NotFound

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(404);
    }

    [Fact]
    public void ResolveHttpCode_NoCode_And_NoResolvers_FallsBackTo_Default_Status_Mapping()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            CodeToHttp = null,
            StatusCodeResolver = null // force ultimate fallback
        };

        var result = Results.Success(); // Status.Completed

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(200); // KnightResponseHttpDefaults.StatusToHttp(Completed)
    }

    [Fact]
    public void ResolveHttpCode_AllResolversNull_For_Error_Status_Uses_Default_500()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            CodeToHttp = null,
            StatusCodeResolver = null
        };

        var result = Results.Error("boom"); // Status.Error

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(500);
    }

    [Fact]
    public void ResolveHttpCode_Typed_With_Code_Uses_CodeToHttp_Defaults()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            CodeToHttp = KnightResponseHttpDefaults.CodeToHttp,
            StatusCodeResolver = null
        };

        var result = Results.Success(value: 123, code: ResultCodes.Created); // maps to 201 via defaults

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(201);
    }

    [Fact]
    public void ResolveHttpCode_CodeToHttp_ReturnsNull_Uses_Custom_StatusCodeResolver()
    {
        // Arrange
        var opts = new KnightResponseBaseOptions<object, object, object>
        {
            CodeToHttp = _ => null,
            StatusCodeResolver = _ => 499 // custom fallback
        };

        var result = Results.Failure("nope"); // Status.Failed

        // Act
        var code = ResultHttpResolver.ResolveHttpCode(result, opts);

        // Assert
        code.ShouldBe(499);
    }
}
