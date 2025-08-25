using Knight.Response.AspNetCore.Extensions;
using Knight.Response.AspNetCore.Mappers;
using Knight.Response.AspNetCore.Options;
using Knight.Response.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Knight.Response.AspNetCore.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    private sealed class CustomMapper : IValidationErrorMapper
    {
        public IDictionary<string, string[]> Map(IReadOnlyList<Message> m) => new Dictionary<string, string[]>();
    }

    private sealed class OptionsMapper : IValidationErrorMapper
    {
        public IDictionary<string, string[]> Map(IReadOnlyList<Message> m) => new Dictionary<string, string[]>();
    }

    [Fact]
    public void AddKnightResponse_Registers_Defaults_And_Applies_Configure()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddKnightResponse(o => o.IncludeFullResultPayload = true);

        // Act
        var sp = services.BuildServiceProvider();
        var mapper = sp.GetRequiredService<IValidationErrorMapper>();
        var opts = sp.GetRequiredService<IOptions<KnightResponseOptions>>().Value;

        // Assert
        mapper.ShouldBeOfType<DefaultValidationErrorMapper>();
        opts.IncludeFullResultPayload.ShouldBeTrue();
    }

    [Fact]
    public void AddKnightResponse_CustomMapper_Wins()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddKnightResponse<CustomMapper>();

        // Act
        var sp = services.BuildServiceProvider();
        var mapper = sp.GetRequiredService<IValidationErrorMapper>();

        // Assert
        mapper.ShouldBeOfType<CustomMapper>();
    }

    [Fact]
    public void AddKnightResponse_OptionsMapper_Wins_Over_DI()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IValidationErrorMapper, CustomMapper>();

        services.AddKnightResponse(o => o.ValidationMapper = new OptionsMapper());

        // Act
        var sp = services.BuildServiceProvider();
        var mapper = sp.GetRequiredService<IValidationErrorMapper>();
        var opts   = sp.GetRequiredService<IOptions<KnightResponseOptions>>().Value;

        // Assert
        mapper.ShouldBeOfType<CustomMapper>();              // DI service itself
        opts.ValidationMapper.ShouldBeOfType<OptionsMapper>(); // options used at runtime by pipeline
    }

    [Fact]
    public void AddKnightResponse_DI_Wins_When_Options_Keep_Default()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IValidationErrorMapper, CustomMapper>();

        services.AddKnightResponse(); // options.ValidationMapper stays DefaultValidationErrorMapper

        // Act
        var sp   = services.BuildServiceProvider();
        var opts = sp.GetRequiredService<IOptions<KnightResponseOptions>>().Value;

        // Assert
        opts.ValidationMapper.ShouldBeOfType<CustomMapper>();
    }

    [Fact]
    public void AddKnightResponse_ConfigDelegate_Sets_Options()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddKnightResponse(o => o.UseProblemDetails = true);

        // Act
        var sp = services.BuildServiceProvider();
        var opts = sp.GetRequiredService<IOptions<KnightResponseOptions>>().Value;

        // Assert
        opts.UseProblemDetails.ShouldBeTrue();
    }

    [Fact]
    public void ProblemDetailsBuilder_From_Options_Is_Applied()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddKnightResponse(o =>
        {
            o.UseProblemDetails = true;
            o.ProblemDetailsBuilder = (ctx, res, pd) => pd.Extensions["source"] = "test";
        });

        // Act
        var sp = services.BuildServiceProvider();
        var opts = sp.GetRequiredService<IOptions<KnightResponseOptions>>().Value;

        // Assert
        opts.UseProblemDetails.ShouldBeTrue();
        opts.ProblemDetailsBuilder.ShouldNotBeNull();
    }

    [Fact]
    public void ValidationBuilder_From_Options_Is_Applied()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddKnightResponse(o =>
        {
            o.UseValidationProblemDetails = true;
            o.ValidationBuilder = (ctx, res, vpd) => vpd.Extensions["flag"] = "on";
        });

        // Act
        var sp = services.BuildServiceProvider();
        var opts = sp.GetRequiredService<IOptions<KnightResponseOptions>>().Value;

        // Assert
        opts.UseValidationProblemDetails.ShouldBeTrue();
        opts.ValidationBuilder.ShouldNotBeNull();
    }
}
