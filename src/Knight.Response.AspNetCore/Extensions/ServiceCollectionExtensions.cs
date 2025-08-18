using Knight.Response.AspNetCore.Mappers;
using Knight.Response.AspNetCore.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Knight.Response.AspNetCore.Extensions;

/// <summary>
/// DI extensions for Knight.Response ASP.NET Core integration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Knight.Response for ASP.NET Core and allows configuration via <see cref="KnightResponseOptions"/>.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="configure">
    /// Optional configuration callback for <see cref="KnightResponseOptions"/>.
    /// Use this to enable RFC7807 ProblemDetails (<see cref="KnightResponseOptions.UseProblemDetails"/>),
    /// ValidationProblemDetails (<see cref="KnightResponseOptions.UseValidationProblemDetails"/>),
    /// full result payloads on success (<see cref="KnightResponseOptions.IncludeFullResultPayload"/>),
    /// or exception detail inclusion (<see cref="KnightResponseOptions.IncludeExceptionDetails"/>).
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddKnightResponse(
        this IServiceCollection services,
        Action<KnightResponseOptions>? configure = null)
    {
        // Register defaults
        services.AddScoped<IValidationErrorMapper, DefaultValidationErrorMapper>();

        // Configure options
        var builder = services.AddOptions<KnightResponseOptions>();
        if (configure is not null)
        {
            builder.Configure(configure);
        }

        // If the consumer did not set a mapper (null) or left the default in place,
        // prefer the DI-registered mapper.
        builder.PostConfigure<IValidationErrorMapper>((opts, diMapper) =>
        {
            if (opts.ValidationMapper is null ||
                opts.ValidationMapper.GetType() == typeof(DefaultValidationErrorMapper))
            {
                opts.ValidationMapper = diMapper;
            }
        });

        return services;
    }

    /// <summary>
    /// Registers Knight.Response for ASP.NET Core with a custom validation error mapper type
    /// and allows configuration via <see cref="KnightResponseOptions"/>.
    /// </summary>
    /// <typeparam name="TMapper">
    /// The custom validation error mapper to register. Must implement <see cref="IValidationErrorMapper"/>.
    /// </typeparam>
    /// <param name="services">The application service collection.</param>
    /// <param name="configure">
    /// Optional configuration callback for <see cref="KnightResponseOptions"/>.
    /// Use this to enable RFC7807 ProblemDetails (<see cref="KnightResponseOptions.UseProblemDetails"/>),
    /// ValidationProblemDetails (<see cref="KnightResponseOptions.UseValidationProblemDetails"/>),
    /// full result payloads on success (<see cref="KnightResponseOptions.IncludeFullResultPayload"/>),
    /// or exception detail inclusion (<see cref="KnightResponseOptions.IncludeExceptionDetails"/>).
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddKnightResponse<TMapper>(
        this IServiceCollection services,
        Action<KnightResponseOptions>? configure = null)
        where TMapper : class, IValidationErrorMapper
    {
        services.AddScoped<IValidationErrorMapper, TMapper>();
        return services.AddKnightResponse(configure);
    }
}
