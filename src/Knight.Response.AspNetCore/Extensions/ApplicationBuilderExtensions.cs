using Knight.Response.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Knight.Response.AspNetCore.Extensions;

/// <summary>
/// Extension methods for <see cref="IApplicationBuilder"/> to integrate Knight.Response ASP.NET Core middleware.
/// </summary>
/// <remarks>
/// These extensions allow you to quickly plug the Knight.Response exception handling pipeline into
/// your ASP.NET Core application. The middleware will catch unhandled exceptions and translate them into a
/// consistent <see cref="Knight.Response.Core.Result"/>-based HTTP response, using either JSON or RFC7807 ProblemDetails
/// depending on your <see cref="Options.KnightResponseOptions"/> configuration.
/// </remarks>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Registers the <see cref="KnightResponseExceptionMiddleware"/> in the application pipeline.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to register the middleware on.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance for chaining.</returns>
    /// <remarks>
    /// When enabled, this middleware:
    /// <list type="bullet">
    /// <item>
    /// <description>Catches unhandled exceptions thrown by downstream middleware and endpoints.</description>
    /// </item>
    /// <item>
    /// <description>Logs the exception using <see cref="Microsoft.Extensions.Logging.ILogger"/>.</description>
    /// </item>
    /// <item>
    /// <description>Builds a <see cref="Knight.Response.Core.Result"/> representing an error.</description>
    /// </item>
    /// <item>
    /// <description>Emits the result as JSON or RFC7807 ProblemDetails, depending on <see cref="Options.KnightResponseOptions.UseProblemDetails"/>.</description>
    /// </item>
    /// <item>
    /// <description>Optionally includes exception details if <see cref="Options.KnightResponseOptions.IncludeExceptionDetails"/> is enabled (development scenarios only).</description>
    /// </item>
    /// </list>
    /// This method should typically be called early in the pipeline, after logging and before other error-handling components.
    /// </remarks>
    public static IApplicationBuilder UseKnightResponseExceptionMiddleware(this IApplicationBuilder app)
        => app.UseMiddleware<KnightResponseExceptionMiddleware>();
}
