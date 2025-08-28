using System.Reflection;
using System.Text.RegularExpressions;
using Knight.Response.Abstractions.Http.Mappers;
using Knight.Response.Models;
using Shouldly;

namespace Knight.Response.Abstractions.Http.Tests.Mappers;

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
        const string message1 = "name is required";
        const string message2 = "not a valid address";
        var mapper = new DefaultValidationErrorMapper();
        var messages = new[]
        {
            Msg($"name: {message1}"),
            Msg($"email: {message2}")
        };

        // Act
        var dict = mapper.Map(messages);

        // Assert
        dict.Keys.Count.ShouldBe(2);
        dict["name"].ShouldBe([message1]);
        dict["email"].ShouldBe([message2]);
    }

    [Fact]
    public void Map_Aggregates_Multiple_Errors_For_Same_Field()
    {
        // Arrange
        const string field = "name";
        const string message1 = "is required";
        const string message2 = "must be at least 2 chars";
        const string message3 = "must be unique";
        var mapper = new DefaultValidationErrorMapper();
        var messages = new[]
        {
            Msg($"{field}: {message1}"),
            Msg($"{field}: {message2}"),
            Msg($"{field}: {message3}")
        };

        // Act
        var dict = mapper.Map(messages);

        // Assert
        dict.Keys.ShouldBe([field]);
        dict[field].ShouldBe([message1, message2, message3]);
    }

    [Fact]
    public void Map_FieldNames_Are_CaseInsensitive()
    {
        // Arrange
        const string field = "email";
        const string message1 = "is required";
        const string message2 = "invalid";
        var mapper = new DefaultValidationErrorMapper();
        var messages = new[]
        {
            Msg($"{field.ToUpper()}: {message1}"),
            Msg($"{field}: {message2}")
        };

        // Act
        var dict = mapper.Map(messages);

        // Assert
        dict.Keys.Count.ShouldBe(1);
        dict.ContainsKey(field).ShouldBeTrue();
        dict[field].ShouldBe([message1, message2]);
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
        var dict = mapper.Map([Msg(content)]);

        // Assert
        dict.Keys.ShouldBe([field], Case.Insensitive);
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
        var dict = mapper.Map([Msg(content)]);

        // Assert
        dict.Count.ShouldBe(0);
    }

    [Fact]
    public void Metadata_Field_Takes_Priority_Over_Colon_Parse()
    {
        // Arrange
        var mapper = new DefaultValidationErrorMapper();
        var meta = new Dictionary<string, object?> { ["field"] = "profile.age" };
        var messages = new[] { new Message(MessageType.Error, "age: invalid", meta) };

        // Act
        var dict = mapper.Map(messages);

        // Assert
        dict.Keys.ShouldBe(["profile.age"], Case.Insensitive);
        dict["profile.age"].ShouldBe(["age: invalid"]); // content kept; meta set the field
    }

    [Fact]
    public void ColonPattern_Is_Compiled_And_CultureInvariant()
    {
        // Arrange
        var field = typeof(DefaultValidationErrorMapper)
            .GetField("ColonPattern", BindingFlags.NonPublic | BindingFlags.Static);
        field.ShouldNotBeNull();

        var regex = field.GetValue(null) as Regex;
        regex.ShouldNotBeNull();

        // Act
        var opts = regex.Options;

        // Assert — kill the bitwise mutant (Compiled & CultureInvariant -> None)
        opts.HasFlag(RegexOptions.Compiled).ShouldBeTrue();
        opts.HasFlag(RegexOptions.CultureInvariant).ShouldBeTrue();

        // (Sanity check the pattern still works)
        var match = regex.Match("User.Email: required");
        match.Success.ShouldBeTrue();
        match.Groups["field"].Value.ShouldBe("User.Email");
        match.Groups["msg"].Value.ShouldBe("required");
    }
}
