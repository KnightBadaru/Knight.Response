using System.ComponentModel.DataAnnotations;
using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Extensions;

public class ValidationTests
{
    [Fact]
    public void TryGetValidationResults_Finds_Single_And_Many()
    {
        // Arrange
        var vr1 = new ValidationResult("Name required", new[] { "Name" });
        var vr2 = new ValidationResult("Amount > 0", new[] { "Amount" });

        var msg1 = new Message(MessageType.Error, "Name: Name required",
            new Dictionary<string, object?> { ["ValidationResult"] = vr1 });

        var msg2 = new Message(MessageType.Error, "Amount: Amount > 0",
            new Dictionary<string, object?> { ["ValidationResults"] = new[] { vr2 } });

        var result = Results.Error(new[] { msg1, msg2 });

        // Act
        var ok = result.TryGetValidationResults(out var list);

        // Assert
        ok.ShouldBeTrue();
        list.Count.ShouldBe(2);
        list.ShouldContain(vr1);
        list.ShouldContain(vr2);
    }

    [Fact]
    public void TryGetValidationResults_Returns_Empty_When_None()
    {
        // Arrange
        var result = Results.Error("boom");

        // Act
        var ok = result.TryGetValidationResults(out var list);

        // Assert
        ok.ShouldBeFalse();
        list.ShouldBeEmpty();
    }

    [Fact]
    public void Validation_Returns_Success_When_Empty()
    {
        // Arrange
        var errors = Array.Empty<ValidationResult>();

        // Act
        var result = Results.ValidationFailure(errors);

        // Assert
        result.IsSuccess().ShouldBeTrue();
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void Validation_Maps_To_Error_Result_With_Prefixed_Messages()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationResult("Name is required", ["Name"]),
            new ValidationResult("Oops")
        };

        // Act
        var result = Results.ValidationFailure(errors);

        // Assert
        result.IsFailure().ShouldBeTrue();
        result.Messages.Count.ShouldBe(2);
        result.Messages[0].Content.ShouldStartWith("Name:");
    }

    [Fact]
    public void ValidationFailed_Defaults_Code_And_Allows_Override()
    {
        // Arrange / Act
        var a = Results.ValidationFailure("bad input");
        var b = Results.ValidationFailure("bad input", code: ResultCodes.PreconditionFailed);

        // Assert
        a.Code.ShouldBe(ResultCodes.ValidationFailed);
        b.Code.ShouldBe(ResultCodes.PreconditionFailed);
        a.IsFailure().ShouldBeTrue();
        b.IsFailure().ShouldBeTrue();
    }

    [Fact]
    public void ValidationFailed_Typed_With_Messages_Defaults_Code()
    {
        // Arrange
        var messages = new[] { new Message(MessageType.Error, "e1") };

        // Act
        var result = Results.ValidationFailure<int>(messages);

        // Assert
        result.Code.ShouldBe(ResultCodes.ValidationFailed);
        result.IsFailure().ShouldBeTrue();
    }

    [Fact]
    public void TryGetValidationResults_Finds_Single_And_Many_CaseInsensitive()
    {
        // Arrange
        var vr1 = new ValidationResult("E1", new[] { "Name" });
        var vr2 = new ValidationResult("E2");
        var vr3 = new ValidationResult("E3");

        var result = Results.Failure(new[]
        {
            new Message(MessageType.Error, "m1",
                new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["validationresult"] = vr1
                }),
            new Message(MessageType.Error, "m2",
                new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["ValidationResults"] = new[] { vr2, vr3 }
                })
        });

        // Act
        var ok = result.TryGetValidationResults(out var found);

        // Assert
        ok.ShouldBeTrue();
        found.Count.ShouldBe(3);
        found.ShouldContain(vr1);
        found.ShouldContain(vr2);
        found.ShouldContain(vr3);
    }

    [Fact]
    public void GetValidationResults_Returns_Empty_When_None()
    {
        // Arrange
        var result = Results.Failure("x");

        // Act
        var list = result.GetValidationResults();

        // Assert
        list.ShouldBeEmpty();
    }

    [Fact]
    public void TryGetValidationResults_Should_Collect_Single_And_Many()
    {
        // Arrange
        var single = new ValidationResult("Name required", new[] { "Name" });
        var many = new[]
        {
            new ValidationResult("Amount > 0", new[] { "Amount" }),
            new ValidationResult("Date required", new[] { "Date" })
        };

        var result = Results.Error(new List<Message>
        {
            new(MessageType.Error, "e1", new Dictionary<string, object?> { ["ValidationResult"] = single }),
            new(MessageType.Error, "e2", new Dictionary<string, object?> { ["ValidationResults"] = many })
        });

        // Act
        var ok = result.TryGetValidationResults(out var list);

        // Assert
        ok.ShouldBeTrue();
        list.Count.ShouldBe(3);
        list.Any(v => v.ErrorMessage == "Name required").ShouldBeTrue();
        list.Any(v => v.ErrorMessage == "Amount > 0").ShouldBeTrue();
        list.Any(v => v.ErrorMessage == "Date required").ShouldBeTrue();
    }
}
