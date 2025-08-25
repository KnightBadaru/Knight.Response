using Knight.Response.AspNetCore.Mappers;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.AspNetCore.Tests.Mappers;

public class DefaultValidationErrorMapperTests
{
    private static Message Msg(string content, string? fieldMeta = null)
    {
        var meta = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(fieldMeta))
        {
            meta["field"] = fieldMeta;
        }

        return new Message(MessageType.Error, content, meta);
    }

    [Fact]
    public void Map_MetadataField_TakesPrecedence_OverColonPattern()
    {
        // Arrange
        const string content = "ignoredField: should-not-be-used";
        const string field = "email";
        var mapper = new DefaultValidationErrorMapper();
        var messages = new[]
        {
            // Has both metadata and "field: msg" – metadata must win
            Msg(content, fieldMeta: field)
        };

        // Act
        var dict = mapper.Map(messages);

        // Assert
        dict.Keys.ShouldBe([field]);
        dict[field].ShouldBe([content]);
    }

    [Fact]
    public void Map_ColonPattern_Parses_Field_And_Message()
    {
        // Arrange
        var mapper = new DefaultValidationErrorMapper();
        var messages = new[]
        {
            Msg("name: name is required"),
            Msg("email: not a valid address")
        };

        // Act
        var dict = mapper.Map(messages);

        // Assert
        dict.Keys.Count.ShouldBe(2);
        dict["name"].ShouldBe(["name is required"]);
        dict["email"].ShouldBe(["not a valid address"]);
    }

    [Fact]
    public void Map_Aggregates_Multiple_Errors_For_Same_Field()
    {
        // Arrange
        const string field = "name";
        var mapper = new DefaultValidationErrorMapper();
        var messages = new[]
        {
            Msg($"{field}: is required"),
            Msg($"{field}: must be at least 2 chars"),
            Msg($"{field}: must be unique")
        };

        // Act
        var dict = mapper.Map(messages);

        // Assert
        dict.Keys.ShouldBe([field]);
        dict[field].ShouldBe(["is required", "must be at least 2 chars", "must be unique"]);
    }

    [Fact]
    public void Map_FieldNames_Are_CaseInsensitive()
    {
        // Arrange
        const string field = "email";
        var mapper = new DefaultValidationErrorMapper();
        var messages = new[]
        {
            Msg($"{field.ToUpper()}: is required"),
            Msg($"{field}: invalid")
        };

        // Act
        var dict = mapper.Map(messages);

        // Assert
        dict.Keys.Count.ShouldBe(1);
        dict.ContainsKey(field).ShouldBeTrue();
        dict[field].ShouldBe(["is required", "invalid"]);
    }

    [Fact]
    public void Map_Ignores_Messages_Without_Metadata_Or_ColonPattern()
    {
        // Arrange
        var mapper = new DefaultValidationErrorMapper();
        var messages = new[]
        {
            new Message(MessageType.Error, "no shape here", new Dictionary<string, object?>()),
            new Message(MessageType.Information,  "informational", new Dictionary<string, object?>())
        };

        // Act
        var dict = mapper.Map(messages);

        // Assert
        dict.Count.ShouldBe(0);
    }

    [Fact]
    public void Map_Supports_Dotted_And_Bracketed_Field_Paths()
    {
        // Arrange
        var mapper = new DefaultValidationErrorMapper();
        var messages = new[]
        {
            Msg("user.name: required"),
            Msg("addresses[0].line1: required"),
            Msg("addresses[1].postcode: invalid")
        };

        // Act
        var dict = mapper.Map(messages);

        // Assert
        dict.Keys.ShouldBe(["user.name", "addresses[0].line1", "addresses[1].postcode"]);
        dict["user.name"].ShouldBe(["required"]);
        dict["addresses[0].line1"].ShouldBe(["required"]);
        dict["addresses[1].postcode"].ShouldBe(["invalid"]);
    }

    [Fact]
    public void Map_Respects_Metadata_Field_Value_Whitespace()
    {
        // Arrange
        const string field = "  user.name  ";
        const string content = "ignored colon pattern";
        var mapper = new DefaultValidationErrorMapper();
        var meta = new Dictionary<string, object?> { ["field"] = field };
        var messages = new[]
        {
            new Message(MessageType.Error, content, meta)
        };

        // Act
        var dict = mapper.Map(messages);

        // Assert
        // Mapper uses the value as-is (no trim on metadata).
        dict.Keys.ShouldBe([field]);
        dict[field].ShouldBe([content.Trim()]);
    }

    [Fact]
    public void Map_ColonPattern_Trims_Field_And_Message_Sides()
    {
        // Arrange
        const string field = "name";
        const string content = "too short  ";
        var mapper = new DefaultValidationErrorMapper();
        var messages = new[]
        {
            Msg($"  {field}  :   {content}")
        };

        // Act
        var dict = mapper.Map(messages);

        // Assert
        dict.Keys.ShouldBe([field]);
        dict[field].ShouldBe([content.Trim()]); // message keeps trailing spaces after the colon, by design
    }

    [Fact]
    public void Map_Empty_Input_Returns_Empty_Dictionary()
    {
        // Arrange
        var mapper = new DefaultValidationErrorMapper();

        // Act
        var dict = mapper.Map(new List<Message>());

        // Assert
        dict.Count.ShouldBe(0);
    }

    [Fact]
    public void Map_Mixed_Metadata_And_Colon_Entries_Aggregate_Correctly()
    {
        // Arrange
        var mapper = new DefaultValidationErrorMapper();
        var messages = new[]
        {
            Msg("first: required"),
            Msg("second: invalid"),
            Msg("value is missing", fieldMeta: "first")
        };

        // Act
        var dict = mapper.Map(messages);

        // Assert
        dict.Keys.Count.ShouldBe(2);
        dict["first"].ShouldBeEquivalentTo(new[] { "required", "value is missing" });
        dict["second"].ShouldBe(["invalid"]);
    }

    [Theory]
    [InlineData("  User.Email  :  required  ")]
    [InlineData("\tUser.Email:\trequired")]
    public void Map_ColonPattern_With_Whitespace_And_Casing_Still_Parses(string content)
    {
        var mapper = new DefaultValidationErrorMapper();
        var dict = mapper.Map([Msg(content)]);

        dict.Keys.ShouldBe(["User.Email"], Case.Insensitive);
        dict["User.Email"].ShouldBe(["required"]);
    }

    // -------------------- Mutant War! --------------------

    [Theory]
    [InlineData("  User.Email  :  required  ", "User.Email", "required")]
    [InlineData("\tUser.Name:\tinvalid  ", "User.Name", "invalid")]
    public void ColonPattern_Parses_And_Trims_Field_And_Message(string content, string field, string msg)
    {
        // Arrange
        var mapper = new DefaultValidationErrorMapper();

        // Act
        var dict = mapper.Map(new[] { Msg(content) });

        // Assert
        dict.Keys.ShouldBe(new[] { field }, Case.Insensitive);
        dict[field][0].ShouldBe(msg);
    }

    [Theory]
    [InlineData("No Colon Here")]
    [InlineData(": just colon at start")]
    [InlineData("field:")]                 // empty message
    [InlineData("   :  missing field")]    // empty field
    public void Invalid_Colon_Strings_Should_Not_Produce_Entries(string content)
    {
        // Arrange
        var mapper = new DefaultValidationErrorMapper();

        // Act
        var dict = mapper.Map(new[] { Msg(content) });

        // Assert
        dict.Count.ShouldBe(0);
    }

    [Fact]
    public void Metadata_Field_Takes_Priority_Over_Colon_Parse()
    {
        // Arrange
        var mapper = new DefaultValidationErrorMapper();
        var meta = new Dictionary<string, object?> { ["field"] = "profile.age" };
        var msgs = new[] { new Knight.Response.Models.Message(Knight.Response.Models.MessageType.Error, "age: invalid", meta) };

        // Act
        var dict = mapper.Map(msgs);

        // Assert
        dict.Keys.ShouldBe(new[] { "profile.age" }, Case.Insensitive);
        dict["profile.age"].ShouldBe(new[] { "age: invalid" }); // content kept; meta set the field
    }
}
