using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Extensions;

public class CodeTests
{
    [Fact]
    public void WithCode_Sets_Code_Without_Altering_Status_Or_Messages()
    {
        // Arrange
        var result = Results.Success().WithMessage(new Message(MessageType.Information, "ok"));

        // Act
        var with = result.WithCode(ResultCodes.Created);

        // Assert
        with.Status.ShouldBe(Status.Completed);
        with.Code!.Value.ShouldBe(ResultCodes.Created.Value);
        with.Messages.Count.ShouldBe(1);
    }

    [Fact]
    public void WithoutCode_Removes_Code()
    {
        // Arrange
        var result = Results.Success(ResultCodes.Updated);

        // Act
        var without = result.WithoutCode();

        // Assert
        without.Code.ShouldBeNull();
        without.Status.ShouldBe(Status.Completed);
    }

    [Fact]
    public void HasCode_By_String_Is_Case_Insensitive()
    {
        // Arrange
        var result = Results.Success(ResultCodes.NoContent);

        // Act
        var hasLower = result.HasCode("nocontent");

        // Assert
        hasLower.ShouldBeTrue();
    }

    [Fact]
    public void HasCode_String_Matches_Ignoring_Case()
    {
        // Arrange
        var result = Results.Success(code: ResultCodes.Created);

        // Act
        var ok = result.HasCode("created");

        // Assert
        ok.ShouldBeTrue();
    }

    [Fact]
    public void HasCode_By_ResultCode_Compares_By_Value()
    {
        // Arrange
        var result = Results.Success(ResultCodes.NoContent);

        // Act
        var has = result.HasCode(ResultCodes.NoContent);

        // Assert
        has.ShouldBeTrue();
    }

    [Fact]
    public void HasCode_String_Returns_False_When_Code_Is_Null()
    {
        // Arrange
        var result = Results.Success(); // no code

        // Act
        var ok = result.HasCode("Anything");

        // Assert
        ok.ShouldBeFalse();
    }

    [Fact]
    public void HasCode_ResultCode_Reference_Equality_Succeeds()
    {
        // Arrange
        var code = ResultCodes.Updated;
        var result = Results.Success(code: code);

        // Act
        var ok = result.HasCode(code);

        // Assert
        ok.ShouldBeTrue();
    }

    [Fact]
    public void WithCode_And_WithoutCode_Roundtrip()
    {
        // Arrange
        var result = Results.Success().WithCode(ResultCodes.NoContent);

        // Act
        var cleared = result.WithoutCode();

        // Assert
        result.Code.ShouldNotBeNull();
        cleared.Code.ShouldBeNull();
        cleared.Status.ShouldBe(Status.Completed);
    }

    [Fact]
    public void WithCode_Typed_Retains_Value_And_Messages()
    {
        // Arrange
        var original = Results.Success(5, messages: new[] { new Message(MessageType.Information, "ok") });

        // Act
        var updated = original.WithCode(ResultCodes.Created);

        // Assert
        updated.Status.ShouldBe(Status.Completed);
        updated.Value.ShouldBe(5);
        updated.Code.ShouldBe(ResultCodes.Created);
        updated.Messages.Count.ShouldBe(1);
    }

    [Fact]
    public void WithCode_And_HasCode_Should_Work_For_String_And_ResultCode()
    {
        // Arrange
        var r = Results.Success().WithCode(ResultCodes.NotFound);

        // Act
        var hasByString = r.HasCode("NotFound");
        var hasByObj = r.HasCode(ResultCodes.NotFound);

        // Assert
        hasByString.ShouldBeTrue();
        hasByObj.ShouldBeTrue();
    }

    [Fact]
    public void WithCodeIf_And_WithoutCodeIf_Should_Set_And_Clear_Conditionally()
    {
        // Arrange
        var result = Results.Success();

        // Act
        var set = result.WithCodeIf(true, ResultCodes.Updated);
        var keep = set.WithoutCodeIf(false);
        var cleared = keep.WithoutCodeIf(true);

        // Assert
        set.Code!.Value.ShouldBe("Updated");
        keep.Code!.Value.ShouldBe("Updated");
        cleared.Code.ShouldBeNull();
    }

    [Fact]
    public void WithoutCode_Should_Clear_Code_Unconditionally()
    {
        // Arrange
        var result = Results.Success().WithCode(ResultCodes.Created);

        // Act
        var cleared = result.WithoutCode();

        // Assert
        cleared.Code.ShouldBeNull();
    }

    [Fact]
    public void HasCode_Typed_String_Works()
    {
        // Arrange
        var result = Results.Success(123).WithCode(ResultCodes.Created);

        // Act
        var ok = result.HasCode("CREATED");

        // Assert
        ok.ShouldBeTrue();
    }

    [Fact]
    public void HasCode_Typed_ResultCode_Works()
    {
        // Arrange
        var result = Results.Success("v").WithCode(ResultCodes.NoContent);

        // Act
        var ok = result.HasCode(ResultCodes.NoContent);

        // Assert
        ok.ShouldBeTrue();
    }

    [Fact]
    public void WithoutCode_Clears_Existing_Code()
    {
        // Arrange
        var result = Results.NotFound().WithCode(ResultCodes.NotFound);

        // Act
        var cleared = result.WithoutCode();

        // Assert
        cleared.Code.ShouldBeNull();
    }

    [Fact]
    public void HasCode_Is_CaseInsensitive()
    {
        // Arrange
        var result = Results.NotFound().WithCode(new ResultCode("NotFound"));

        // Act & Assert
        result.HasCode("notfound").ShouldBeTrue();
    }
}
