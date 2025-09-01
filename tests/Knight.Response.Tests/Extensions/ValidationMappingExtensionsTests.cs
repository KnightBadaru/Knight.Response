using System.ComponentModel.DataAnnotations;
using Knight.Response.Extensions;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Tests.Extensions
{
    public class ValidationMappingExtensionsTests
    {
        [Fact]
        public void Metadata_Path_Trims_Field_And_Uses_First_NonEmpty_Member()
        {
            // Arrange
            var errors = new[]
            {
                new ValidationResult("ok", ["  Name  ", "", "Other"])
            };
            string? capturedField = null;

            // Act
            var messages = ValidationMappingExtensions.ToMessagesWithMetadata(
                errors,
                (message, field) =>
                {
                    capturedField = field; // should be "Name" (trimmed, first non-empty)
                    return message;
                });

            // Assert
            messages.ShouldHaveSingleItem();
            capturedField.ShouldBe("Name");
            messages[0].Content.ShouldBe("ok"); // unprefixed
        }

        [Fact]
        public void Metadata_Path_Field_Is_Null_When_No_Members()
        {
            // Arrange
            var errors = new[] { new ValidationResult("x") };
            var capturedField = "set";

            // Act
            var messages = ValidationMappingExtensions.ToMessagesWithMetadata(
                errors,
                (message, field) =>
                {
                    capturedField = field;
                    return message;
                });

            // Assert
            messages.ShouldHaveSingleItem();
            capturedField.ShouldBeNull(); // kills mutants that default to empty string
            messages[0].Content.ShouldBe("x");
        }

        [Fact]
        public void Metadata_Path_Fallbacks_To_Default_When_Message_Is_Null_Empty_Or_Whitespace()
        {
            // Arrange
            var errors = new[]
            {
                new ValidationResult(null!, ["F"]),
                new ValidationResult(string.Empty, ["F"]),
                new ValidationResult("   ", ["F"]),
            };

            // Act
            var messages = ValidationMappingExtensions.ToMessagesWithMetadata(
                errors,
                (m, _) => m);

            // Assert
            messages.Count.ShouldBe(3);
            messages.All(m => m.Content == "Validation error.").ShouldBeTrue();
        }

        [Fact]
        public void Metadata_Path_Trims_Message_Content()
        {
            // Arrange
            var errors = new[] { new ValidationResult("  hello  ", ["Any"]) };

            // Act
            var messages = ValidationMappingExtensions.ToMessagesWithMetadata(
                errors,
                (m, _) => m);

            // Assert
            messages.ShouldHaveSingleItem();
            messages[0].Content.ShouldBe("hello"); // kills mutants removing .Trim()
        }

        [Fact]
        public void Metadata_Path_Applies_Enricher_Result()
        {
            // Arrange
            var errors = new[] { new ValidationResult("base", ["F"]) };

            // Act
            var messages = ValidationMappingExtensions.ToMessagesWithMetadata(
                errors,
                (m, field) =>
                {
                    field.ShouldBe("F");                        // ensure field passed through
                    return new Message(m.Type, m.Content + "!"); // mutate content to prove enrich applied
                });

            // Assert
            messages.ShouldHaveSingleItem();
            messages[0].Content.ShouldBe("base!");
        }
    }
}
