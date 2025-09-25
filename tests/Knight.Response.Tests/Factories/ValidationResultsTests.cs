using System.ComponentModel.DataAnnotations;
using Knight.Response.Extensions;
using Knight.Response.Factories;
using Knight.Response.Models;
using Knight.Response.Tests.Infrastructure;
using Shouldly;

namespace Knight.Response.Tests.Factories;

public class ValidationResultsTests
{
    const string ValidationResult = "ValidationResult";
    // -------------------------------
    // Non-generic Results.Validation
    // -------------------------------

    [Fact]
    public void Validation_NullCollection_ReturnsSuccess_NoMessages()
    {
        // Arrange
        IEnumerable<ValidationResult>? errors = null;

        // Act
        var result = Results.Validation(errors);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess().ShouldBeTrue();
        result.Messages.ShouldNotBeNull();
        result.Messages.Count.ShouldBe(0);
    }

    [Fact]
    public void Validation_EmptyCollection_ReturnsSuccess_NoMessages()
    {
        // Arrange
        var errors = Array.Empty<ValidationResult>();

        // Act
        var result = Results.Validation(errors);

        // Assert
        result.IsSuccess().ShouldBeTrue();
        result.Messages.Count.ShouldBe(0);
    }

    [Fact]
    public void Validation_WithFieldNames_MapsToErrorMessages_WithFieldPrefix()
    {
        // Arrange
        var errors = new List<ValidationResult>
        {
            new("Name is required.", ["Name"]),
            new("Amount must be greater than 0.", ["Amount"]),
        };

        // Act
        var result = Results.Validation(errors);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages.Count.ShouldBe(2);

        var texts = result.Messages.Select(m => m.Content).ToList();
        texts[0].ShouldBe("Name: Name is required.");
        texts[1].ShouldBe("Amount: Amount must be greater than 0.");
        result.Messages.All(m => m.Type == MessageType.Error).ShouldBeTrue();
    }

    [Fact]
    public void Validation_NoFieldName_MapsToErrorMessage_WithoutPrefix()
    {
        // Arrange
        var errors = new List<ValidationResult>
        {
            new("General validation failed.") // no MemberNames
        };

        // Act
        var result = Results.Validation(errors);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].Content.ShouldBe("General validation failed.");
        result.Messages[0].Type.ShouldBe(MessageType.Error);
    }

    [Fact]
    public void Validation_MixedFieldAndNonField_ProducesCorrectContents()
    {
        // Arrange
        var errors = new List<ValidationResult>
        {
            new("Name is required.", ["Name"]),
            new("General validation failed.") // no member
        };

        // Act
        var result = Results.Validation(errors);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages.Count.ShouldBe(2);

        var ordered = result.Messages.Select(m => m.Content).ToList();
        ordered[0].ShouldBe("Name: Name is required.");
        ordered[1].ShouldBe("General validation failed.");
    }

    [Fact]
    public void Validation_MultipleMemberNames_PicksFirstMemberForPrefix()
    {
        // Arrange
        var errors = new List<ValidationResult>
        {
            new("Value invalid.", ["Primary", "Secondary"])
        };

        // Act
        var result = Results.Validation(errors);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].Content.ShouldBe("Primary: Value invalid.");
    }

    [Fact]
    public void Validation_NullOrEmptyErrorMessage_FallsBackToDefaultContent()
    {
        // Arrange
        var errors = new List<ValidationResult>
        {
            new(null!, ["FieldX"]),
            new(string.Empty, ["FieldY"]),
            new("   ", ["FieldZ"]),
        };

        // Act
        var result = Results.Validation(errors);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages.Count.ShouldBe(3);
        result.Messages.All(m => m.Content.Contains("Validation error.")).ShouldBeTrue();
    }

    [Fact]
    public void Validation_WithMetadataEnricher_DelegateReceivesExpectedField()
    {
        // Arrange
        var errors = new List<ValidationResult>
        {
            new("Name is required.", ["Name"]),
            new("General failure.") // no field
        };

        string? firstFieldSeen = null;
        string? secondFieldSeen = null;

        // Act
        var result = Results.Validation(
            errors,
            (msg, field) =>
            {
                // capture what we were called with
                if (firstFieldSeen == null)
                {
                    firstFieldSeen = field;
                }
                else
                {
                    secondFieldSeen = field;
                }

                // Testing the callback contract here
                return msg;
            });

        // Assert
        result.IsSuccess().ShouldBeFalse();
        firstFieldSeen.ShouldBe("Name");
        secondFieldSeen.ShouldBeNull();
    }

    [Fact]
    public void Validation_WithMetadataEnricher_ContentIsRawErrorMessage_NotPrefixed()
    {
        // Arrange
        var errors = new List<ValidationResult>
        {
            new("Name is required.", ["Name"]),
        };

        // Act
        var result = Results.Validation(
            errors,
            (msg, _) => msg // no enrichment; verify base text is raw, not prefixed
        );

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].Content.ShouldBe("Name is required."); // no "Name: " prefix
    }

    // --------------------------------
    // Generic Results.Validation<T>
    // --------------------------------

    [Fact]
    public void ValidationT_NullCollection_ReturnsSuccess_DefaultValue()
    {
        // Arrange
        IEnumerable<ValidationResult>? errors = null;

        // Act
        var result = Results.Validation<Widget>(errors);

        // Assert
        result.IsSuccess().ShouldBeTrue();
        result.Messages.Count.ShouldBe(0);
        result.Value.ShouldBe(default); // default(Widget)
    }

    [Fact]
    public void ValidationT_EmptyCollection_ReturnsSuccess_DefaultValue()
    {
        // Arrange
        var errors = Enumerable.Empty<ValidationResult>();

        // Act
        var result = Results.Validation<Widget>(errors);

        // Assert
        result.IsSuccess().ShouldBeTrue();
        result.Messages.Count.ShouldBe(0);
        result.Value.ShouldBe(default);
    }

    [Fact]
    public void ValidationT_WithErrors_ReturnsError_WithMappedMessages()
    {
        // Arrange
        var errors = new List<ValidationResult>
        {
            new("Id must be > 0.", ["Id"]),
            new("Name is required.", ["Name"])
        };

        // Act
        var result = Results.Validation<Widget>(errors);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages.Count.ShouldBe(2);
        result.Messages[0].Content.ShouldBe("Id: Id must be > 0.");
        result.Messages[1].Content.ShouldBe("Name: Name is required.");
    }

    [Fact]
    public void ValidationT_WithMetadataEnricher_CallsDelegateAndKeepsRawContent()
    {
        // Arrange
        var errors = new List<ValidationResult>
        {
            new("Invalid.", ["FieldA"])
        };

        string? capturedField = null;

        // Act
        var result = Results.Validation<Widget>(
            errors,
            (msg, field) =>
            {
                capturedField = field;
                return msg;
            });

        // Assert
        result.IsSuccess().ShouldBeFalse();
        capturedField.ShouldBe("FieldA");
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].Content.ShouldBe("Invalid."); // raw, not prefixed
    }

    [Fact]
    public void ValidationT_NullOrEmptyErrorMessage_FallsBackToDefaultContent()
    {
        // Arrange
        var errors = new List<ValidationResult>
        {
            new(null!, ["X"]),
            new(string.Empty, ["Y"]),
            new("   ", ["Z"]),
        };

        // Act
        var result = Results.Validation<Widget>(errors);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages.Count.ShouldBe(3);
        result.Messages.All(m => m.Content.Contains("Validation error.")).ShouldBeTrue();
    }

    [Fact]
    public void Validation_NullErrors_ReturnsSuccess_EmptyMessages()
    {
        // Arrange
        IEnumerable<ValidationResult>? errors = null;

        // Act
        var result = Results.Validation(errors);

        // Assert
        result.IsSuccess().ShouldBeTrue();
        result.Messages.Count.ShouldBe(0);
    }

    [Fact]
    public void Validation_NonEmptyErrors_ReturnsError()
    {
        // Arrange
        var errors = new[] { new ValidationResult("Boom", ["Field"]) };

        // Act
        var result = Results.Validation(errors);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages.Count.ShouldBe(1);
    }

    [Fact]
    public void Validation_Prefixed_TrimsSpacesAroundFieldAndMessage()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationResult("  must not be blank  ", ["  Name  "])
        };

        // Act
        var result = Results.Validation(errors);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages[0].Content.ShouldBe("Name: must not be blank");
    }

    [Fact]
    public void Validation_Prefixed_Trims_Field_And_Message_And_Picks_First_Member()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationResult("  must not be blank  ", ["  Name  ", "Other"])
        };

        // Act
        var result = Results.Validation(errors);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages.ShouldHaveSingleItem();
        result.Messages[0].Content.ShouldBe("Name: must not be blank");
    }

    [Fact]
    public void Validation_Metadata_IncludesRawValidationResult_WhenFieldPresent()
    {
        // Arrange
        var vr = new ValidationResult("must have name", ["Name"]);
        var errors = new List<ValidationResult> { vr };

        // Act
        var result = Results.Validation(errors);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages.Count.ShouldBe(1);
        var message = result.Messages[0];

        // key must be EXACT string: "ValidationResult"
        message.Metadata.ContainsKey(ValidationResult).ShouldBeTrue();

        // value must be the SAME instance we passed in (reference equality)
        message.Metadata[ValidationResult].ShouldBeSameAs(vr);
    }

    [Fact]
    public void Validation_Metadata_IncludesRawValidationResult_WhenNoField()
    {
        // Arrange
        var vr = new ValidationResult("general rule failed");
        var errors = new List<ValidationResult> { vr };

        // Act
        var result = Results.Validation(errors);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages.Count.ShouldBe(1);
        var meta = result.Messages[0].Metadata;

        meta.ShouldNotBeNull();
        meta.ContainsKey(ValidationResult).ShouldBeTrue();
        meta[ValidationResult].ShouldBeSameAs(vr);
    }

    [Fact]
    public void Validation_Metadata_Present_ForMultipleErrors_AllMessagesCarryTheirOwnInstance()
    {
        // Arrange
        var vr1 = new ValidationResult("Name required", ["Name"]);
        var vr2 = new ValidationResult("Amount > 0", ["Amount"]);
        var errors = new List<ValidationResult> { vr1, vr2 };

        // Act
        var result = Results.Validation(errors);

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages.Count.ShouldBe(2);

        result.Messages[0].Metadata[ValidationResult].ShouldBeSameAs(vr1);
        result.Messages[1].Metadata[ValidationResult].ShouldBeSameAs(vr2);
    }

    [Fact]
    public void Validation_Metadata_Key_IsCaseSensitive_ExactMatchOnly()
    {
        // Arrange
        var vr = new ValidationResult("oops", ["X"]);
        var errors = new List<ValidationResult> { vr };

        // Act
        var result = Results.Validation(errors);

        // Assert
        var meta = result.Messages[0].Metadata;
        meta.ContainsKey(ValidationResult).ShouldBeTrue();
        meta.ContainsKey(ValidationResult).ShouldBeTrue();
        meta.ContainsKey("validationresult").ShouldBeTrue(); // case in-sensitive
    }

    // ---------- Non-generic ----------

    [Fact]
    public void Validation_WithEnricher_NullErrors_ReturnsSuccess_EnricherNotCalled()
    {
        // Arrange
        IEnumerable<ValidationResult>? errors = null;
        var calls = 0;

        // Act
        var result = Results.Validation(errors, (m, _) => { calls++; return m; });

        // Assert
        result.IsSuccess().ShouldBeTrue();                // Success branch hit
        calls.ShouldBe(0);                      // Enricher must NOT be called
    }

    [Fact]
    public void Validation_WithEnricher_EmptyErrors_ReturnsSuccess_EnricherNotCalled()
    {
        // Arrange
        var calls = 0;

        // Act
        var result = Results.Validation(new List<ValidationResult>(), (m, _) =>
        {
            calls++;
            return m;
        });

        // Assert
        result.IsSuccess().ShouldBeTrue();                // list.Count == 0
        calls.ShouldBe(0);                      // proves no fallthrough to Error path
    }

    [Fact]
    public void Validation_WithEnricher_SingleError_ReturnsError_EnricherCalledOnce()
    {
        // Arrange
        var errors = new List<ValidationResult>
        {
            new("oops", ["Field"])
        };
        var calls = 0;

        // Act
        var result = Results.Validation(errors, (m, _) => { calls++; return m; });

        // Assert
        result.IsSuccess().ShouldBeFalse();                       // Error branch hit
        result.Messages.Count.ShouldBe(1);
        calls.ShouldBe(1);                             // proves Error path executed
    }

    [Fact]
    public void Validation_WithEnricher_TwoErrors_EnricherCalledTwice()
    {
        // Arrange
        var errors = new List<ValidationResult>
        {
            new("a", ["A"]),
            new("b", ["B"])
        };
        var calls = 0;

        // Act
        var result = Results.Validation(errors, (m, _) => { calls++; return m; });

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages.Count.ShouldBe(2);
        calls.ShouldBe(2);                             // kills mutants that skip/enforce wrong branch
    }

    // ---------- Generic ----------

    [Fact]
    public void ValidationT_WithEnricher_NullErrors_ReturnsSuccess_DefaultValue_EnricherNotCalled()
    {
        // Arrange
        IEnumerable<ValidationResult>? errors = null;
        var calls = 0;

        // Act
        var result = Results.Validation<Widget>(errors, (m, _) => { calls++; return m; });

        // Assert
        result.IsSuccess().ShouldBeTrue();                // Success branch
        result.Value.ShouldBe(default);         // default(T)
        calls.ShouldBe(0);
    }

    [Fact]
    public void ValidationT_WithEnricher_EmptyErrors_ReturnsSuccess_DefaultValue_EnricherNotCalled()
    {
        // Arrange
        var calls = 0;

        // Act
        var result = Results.Validation<Widget>(new List<ValidationResult>(), (m, _) =>
        {
            calls++;
            return m;
        });

        // Assert
        result.IsSuccess().ShouldBeTrue();
        result.Value.ShouldBe(default);
        calls.ShouldBe(0);
    }

    [Fact]
    public void ValidationT_WithEnricher_SingleError_ReturnsError_EnricherCalledOnce()
    {
        // Arrange
        var errors = new List<ValidationResult>
        {
            new("bad", ["X"])
        };
        var calls = 0;

        // Act
        var result = Results.Validation<Widget>(errors, (m, _) => { calls++; return m; });

        // Assert
        result.IsSuccess().ShouldBeFalse();
        result.Messages.Count.ShouldBe(1);
        calls.ShouldBe(1);
    }
}