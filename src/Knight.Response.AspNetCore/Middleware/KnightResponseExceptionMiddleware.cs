using Knight.Response.AspNetCore.Options;
using Knight.Response.Core;
using Knight.Response.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Knight.Response.AspNetCore.Middleware;

/// <summary>
/// Middleware that catches unhandled exceptions, logs them, and converts them into a
/// <see cref="Result"/> error response. If <see cref="KnightResponseOptions.UseProblemDetails"/>
/// is enabled, ProblemDetails are emitted; otherwise a simple JSON payload is returned.
/// </summary>
/// <remarks>
/// This middleware is intentionally conservative: it never exposes stack traces by default
/// (set <see cref="KnightResponseOptions.IncludeExceptionDetails"/> in development-only if desired).
/// </remarks>
public sealed class KnightResponseExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<KnightResponseExceptionMiddleware> _logger;
    private readonly KnightResponseOptions _options;

    /// <summary>
    /// Creates the middleware.
    /// </summary>
    public KnightResponseExceptionMiddleware(
        RequestDelegate next,
        ILogger<KnightResponseExceptionMiddleware> logger,
        IOptions<KnightResponseOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value ?? KnightResponseOptions.Defaults;
    }

    /// <summary>
    /// Invokes the middleware for the current request.
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Path}", context.Request.Path);

            var message =
                _options.IncludeExceptionDetails
                    ? $"Unhandled exception: {ex.Message}"
                    : "An unexpected error occurred.";

            var error = Knight.Response.Factories.Results.Error(message);

            if (_options.IncludeExceptionDetails)
            {
                // Safely add structured diagnostics safely when enabled
                var meta = new Dictionary<string, object?>
                {
                    ["exceptionType"] = ex.GetType().FullName,
                    ["stackTrace"]    = ex.StackTrace,
                    ["source"]        = ex.Source,
                    ["path"]          = context.Request.Path.Value,
                    ["method"]        = context.Request.Method,
                    ["traceId"]       = context.TraceIdentifier
                };

                error = error.WithMessage(new Models.Message(
                    Models.MessageType.Error,
                    "Exception details attached.",
                    meta
                ));
            }

            var failure = Factories.ProblemFactory.FromResult(context, _options, error);

            // Make sure we start a fresh response and let the IResult write headers/body
            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
            }

            await failure.ExecuteAsync(context);
        }
    }
}
