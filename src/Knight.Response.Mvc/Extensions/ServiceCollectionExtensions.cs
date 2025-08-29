using Knight.Response.Abstractions.Http.Mappers;
using Knight.Response.Mvc.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Knight.Response.Mvc.Extensions;

/// <summary>DI extensions for Knight.Response MVC (2.x) integration.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Knight.Response MVC integration, options, and the default validation mapper.
    /// </summary>
    public static IServiceCollection AddKnightResponseMvc(
        this IServiceCollection services,
        Action<KnightResponseOptions>? configure = null)
    {
        // Options (uses Defaults if not configured)
        var builder = services.AddOptions<KnightResponseOptions>();
        if (configure is not null)
        {
            builder.Configure(configure);
        }

        // Default mapper from Abstractions (can be overridden by user)
        services.AddScoped<IValidationErrorMapper, DefaultValidationErrorMapper>();

        return services;
    }

    /// <summary>
    /// Registers Knight.Response MVC integration with a custom validation error mapper type.
    /// </summary>
    public static IServiceCollection AddKnightResponseMvc<TMapper>(
        this IServiceCollection services,
        Action<KnightResponseOptions>? configure = null)
        where TMapper : class, IValidationErrorMapper
    {
        services.AddScoped<IValidationErrorMapper, TMapper>();
        return services.AddKnightResponseMvc(configure);
    }
}
