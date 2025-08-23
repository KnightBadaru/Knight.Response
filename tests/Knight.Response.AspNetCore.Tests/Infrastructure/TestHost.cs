using System.Text.Json;
using Knight.Response.AspNetCore.Mappers;
using Knight.Response.AspNetCore.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Knight.Response.AspNetCore.Tests.Infrastructure;

internal static class TestHost
{
    // Minimal public mapper used as the default for tests when one isn't supplied.
    private sealed class PassthroughTestMapper : IValidationErrorMapper
    {
        public IDictionary<string, string[]> Map(
            IReadOnlyList<Models.Message> messages)
            => new Dictionary<string, string[]>();
    }

    public static (DefaultHttpContext http, IServiceProvider services) CreateHttpContext(
        KnightResponseOptions? options = null,
        IValidationErrorMapper? validationMapper = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // Register options
        var opts = options ?? new KnightResponseOptions();
        opts.ValidationMapper = validationMapper ?? new PassthroughTestMapper();
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(opts));

        // Mapper - not using internal DefaultValidationErrorMapper from tests
        services.AddScoped<IValidationErrorMapper>(_ => validationMapper ?? new PassthroughTestMapper());

        var provider = services.BuildServiceProvider();

        var http = new DefaultHttpContext
        {
            RequestServices = provider
        };
        http.Response.Body = new MemoryStream();

        return (http, provider);
    }

    public static async Task<(int status, string body, IHeaderDictionary headers)> ExecuteAsync(
        this IResult result, HttpContext http)
    {
        http.Response.Body = new MemoryStream();
        await result.ExecuteAsync(http);
        http.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(http.Response.Body);
        var body = await reader.ReadToEndAsync();
        return (http.Response.StatusCode, body, http.Response.Headers);
    }

    public static T? Deserialize<T>(string json) =>
        string.IsNullOrWhiteSpace(json)
            ? default
            : JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
}
