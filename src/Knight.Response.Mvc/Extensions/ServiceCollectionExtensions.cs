using Knight.Response.Abstractions.Http.Mappers;
using Knight.Response.Mvc.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Knight.Response.Mvc.Extensions;

/// <summary>DI extensions for Knight.Response MVC (2.x) integration.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Knight.Response MVC integration, options, and the default validation mapper.
    /// </summary>
    public static IServiceCollection AddKnightResponse(
        this IServiceCollection services,
        Action<KnightResponseOptions>? configure = null)
    {
        // Register defaults
        // Only add the default if nothing else has registered IValidationErrorMapper
        services.TryAddScoped<IValidationErrorMapper, DefaultValidationErrorMapper>();

        // Configure options
        var builder = services.AddOptions<KnightResponseOptions>();
        if (configure is not null)
        {
            builder.Configure(configure);
        }

        return services;
    }

    /// <summary>
    /// Registers Knight.Response MVC integration with a custom validation error mapper type.
    /// </summary>
    public static IServiceCollection AddKnightResponse<TMapper>(
        this IServiceCollection services,
        Action<KnightResponseOptions>? configure = null)
        where TMapper : class, IValidationErrorMapper
    {
        services.AddScoped<IValidationErrorMapper, TMapper>();
        return services.AddKnightResponse(configure);
    }
}
