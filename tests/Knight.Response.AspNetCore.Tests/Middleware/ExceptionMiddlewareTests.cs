using Knight.Response.AspNetCore.Extensions;
using Knight.Response.AspNetCore.Middleware;
using Knight.Response.AspNetCore.Options;
using Knight.Response.AspNetCore.Tests.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace Knight.Response.AspNetCore.Tests.Middleware;

public class ExceptionMiddlewareTests
{
   private static KnightResponseExceptionMiddleware Build(RequestDelegate next, KnightResponseOptions? opts = null)
    {
        var logger = Substitute.For<ILogger<KnightResponseExceptionMiddleware>>();
        var options = Microsoft.Extensions.Options.Options.Create(opts ?? new KnightResponseOptions
        {
            UseProblemDetails = true,
            IncludeExceptionDetails = false,
            StatusCodeResolver = _ => StatusCodes.Status500InternalServerError
        });

        return new KnightResponseExceptionMiddleware(next, logger, options);
    }

    [Fact]
    public async Task Middleware_When_Exception_And_ProblemDetails_Enabled_Returns_500_PD_Without_Exception_Detail()
    {
        // Arrange
        var middleware = Build(_ => throw new InvalidOperationException("boom"));
        var (http, _) = TestHost.CreateHttpContext();

        // Act
        await middleware.Invoke(http);

        // Assert
        http.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);

        http.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(http.Response.Body).ReadToEndAsync();

        body.ShouldContain("\"type\":\"https://httpstatuses.io/500\"");
        body.ShouldNotContain("boom");
    }

    [Fact]
    public async Task Middleware_When_Exception_And_IncludeExceptionDetails_True_Contains_Message()
    {
        // Arrange
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            IncludeExceptionDetails = true,
            StatusCodeResolver = _ => StatusCodes.Status500InternalServerError
        };

        var middleware = Build(_ => throw new ArgumentNullException("x", "bad"), opts);
        var (http, _) = TestHost.CreateHttpContext(opts);

        // Act
        await middleware.Invoke(http);

        // Assert
        http.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);

        http.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(http.Response.Body).ReadToEndAsync();

        // logger.Received().Log(LogLevel.Error,Arg.Any<Exception?>(), Arg.Any<string>(), Arg.Any<object?[]>());

        body.ShouldContain("Unhandled exception:");
        body.ShouldContain("bad");

        body.ShouldContain("Unhandled exception");
        body.ShouldContain("exceptionType");
        body.ShouldContain("stackTrace");
        body.ShouldContain("source");
        body.ShouldContain("path");
        body.ShouldContain("method");
        body.ShouldContain("traceId");
        body.ShouldContain("Exception details attached.");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExceptionMiddleware_Respects_IncludeExceptionDetails(bool includeDetails)
    {
        // Arrange
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            IncludeExceptionDetails = includeDetails
        };

        var (http, services) = TestHost.CreateHttpContext(opts);
        var app = new ApplicationBuilder(services)
            .UseKnightResponseExceptionMiddleware()
            .Use(_ => _ => throw new InvalidOperationException("kaput"))
            .Build();

        // Act
        await app(http);
        http.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        http.Response.Body.Position = 0;
        var body = await new StreamReader(http.Response.Body).ReadToEndAsync();

        // Assert
        if (includeDetails)
        {
            body.ShouldContain("kaput");
        }
        else
        {
            body.ShouldContain("An unexpected error occurred.");
            body.ShouldNotContain("kaput");
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExceptionMiddleware_WithProblemDetailsDisabled_Returns_SimpleMessages(bool includeFullPayload)
    {
        // Arrange
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = false,
            IncludeExceptionDetails = false,
            IncludeFullResultPayload = includeFullPayload,
        };

        var (http, services) = TestHost.CreateHttpContext(opts);
        var app = new ApplicationBuilder(services)
            .UseKnightResponseExceptionMiddleware()
            .Use(_ => _ => throw new Exception("boom"))
            .Build();

        // Act
        await app(http);
        http.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        http.Response.Body.Position = 0;
        var body = await new StreamReader(http.Response.Body).ReadToEndAsync();

        // Assert (simple array of messages)
        body.ShouldContain("\"type\"");
        body.ShouldContain("\"content\"");
        body.ShouldContain("\"metadata\"");
        body.ShouldNotContain("\"type\":\"https://httpstatuses.io/");

        if (includeFullPayload)
        {
            body.ShouldContain("isSuccess");
        }
        else
        {
            body.ShouldNotContain("isSuccess");
        }
    }

    [Fact]
    public async Task ExceptionMiddleware_Uses_StatusCodeResolver_For_Error_Status()
    {
        // Arrange
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            IncludeExceptionDetails = false,
            StatusCodeResolver = _ => 503
        };

        var (http, services) = TestHost.CreateHttpContext(opts);
        var app = new ApplicationBuilder(services)
            .UseKnightResponseExceptionMiddleware()
            .Use(_ => _ => throw new Exception("downstream"))
            .Build();

        // Act
        await app(http);

        // Assert
        http.Response.StatusCode.ShouldBe(StatusCodes.Status503ServiceUnavailable);
    }

    [Fact]
    public async Task ExceptionMiddleware_PassesThrough_When_NoException()
    {
        // Arrange
        var opts = new KnightResponseOptions();
        var (http, services) = TestHost.CreateHttpContext(opts);
        var app = new ApplicationBuilder(services)
            .UseKnightResponseExceptionMiddleware()
            .Use(_ => ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status204NoContent;
                return Task.CompletedTask;
            })
            .Build();

        // Act
        await app(http);

        // Assert
        http.Response.StatusCode.ShouldBe(StatusCodes.Status204NoContent);
    }

    // -------------------- Mutant War! --------------------

    [Fact]
    public async Task Middleware_IncludeExceptionDetails_False_Emits_Generic_Message_No_Diagnostics()
    {
        // Arrange
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            IncludeExceptionDetails = false, // important branch
            StatusCodeResolver = _ => StatusCodes.Status500InternalServerError
        };

        var (http, services) = TestHost.CreateHttpContext(opts);

        // Build a bare pipeline: [our middleware] -> [terminal throws]
        RequestDelegate app = new ApplicationBuilder(services)
            .UseKnightResponseExceptionMiddleware()
            .Use(_ => _ => throw new InvalidOperationException("secret"))
            .Build();

        // Act
        await app(http);
        http.Response.Body.Position = 0;
        var body = await new StreamReader(http.Response.Body).ReadToEndAsync();

        // Assert
        http.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        body.ShouldContain("\"type\":\"https://httpstatuses.io/500\"");
        body.ShouldContain("An unexpected error occurred."); // generic
        body.ShouldNotContain("stackTrace");
        body.ShouldNotContain("exceptionType");
        body.ShouldNotContain("secret"); // message not leaked
    }

    [Fact]
    public async Task Middleware_IncludeExceptionDetails_True_Emits_Diagnostics()
    {
        // Arrange
        var opts = new KnightResponseOptions
        {
            UseProblemDetails = true,
            IncludeExceptionDetails = true, // enable diagnostics
            StatusCodeResolver = _ => StatusCodes.Status500InternalServerError
        };

        var (http, services) = TestHost.CreateHttpContext(opts);

        // Build a bare pipeline: [our middleware] -> [terminal throws]
        RequestDelegate app = new ApplicationBuilder(services)
            .UseKnightResponseExceptionMiddleware()
            .Use(_ => _ => throw new InvalidOperationException("leaked"))
            .Build();

        // Act
        await app(http);
        http.Response.Body.Position = 0;
        var body = await new StreamReader(http.Response.Body).ReadToEndAsync();

        // Assert
        http.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        body.ShouldContain("\"type\":\"https://httpstatuses.io/500\"");
        body.ShouldContain("Unhandled exception: leaked"); // includes ex.Message
        body.ShouldContain("stackTrace");
        body.ShouldContain("exceptionType");
        body.ShouldContain("traceId");
    }
}
