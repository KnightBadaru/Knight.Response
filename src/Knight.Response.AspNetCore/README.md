# Knight.Response.AspNetCore

ASP.NET Core integration for [Knight.Response]. It converts `Result` / `Result<T>` to HTTP responses, supports RFC7807 ProblemDetails (including validation errors), and provides exception middleware for consistent error payloads.

[![NuGet Version](https://img.shields.io/nuget/v/Knight.Response.AspNetCore.svg)](https://www.nuget.org/packages/Knight.Response.AspNetCore)
[![ci](https://github.com/KnightBadaru/Knight.Response/actions/workflows/ci.yml/badge.svg)](https://github.com/KnightBadaru/Knight.Response/actions/workflows/ci.yml)
[![Mutation score](https://img.shields.io/endpoint?url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FKnightBadaru%2FKnight.Response%2Fmain%3Fmodule%3DKnight.Response.AspNetCore&label=mutation%20score)](https://dashboard.stryker-mutator.io/reports/github.com/KnightBadaru/Knight.Response/main?module=Knight.Response.AspNetCore)

> NuGet packages you’ll typically use:
>
> * `Knight.Response` – core result types and extensions
> * `Knight.Response.AspNetCore` – this HTTP integration layer

---

## Install

```bash
dotnet add package Knight.Response.AspNetCore --version 2.0.0-preview01
```

---

## Quick start

### 1) Register in DI

```csharp
// Program.cs
using Knight.Response.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Knight.Response HTTP integration (optional inline configuration)
builder.Services.AddKnightResponse(options =>
{
    options.IncludeFullResultPayload    = true;
    options.UseProblemDetails           = false;
    options.UseValidationProblemDetails = false;
    options.IncludeExceptionDetails     = false;
});

// Option A: register a custom validation mapper separately
builder.Services.AddScoped<IValidationErrorMapper, MyValidationErrorMapper>();

// Option B: typed overload
builder.Services.AddKnightResponse<MyValidationErrorMapper>(options =>
{
    options.UseValidationProblemDetails = true;
});
```

### 2) Exception middleware (optional)

```csharp
var app = builder.Build();

app.UseKnightResponseExceptionMiddleware();

app.MapGet("/", () => "OK");
app.Run();
```

---

## Returning results in endpoints/controllers

### Minimal APIs

```csharp
using Knight.Response.AspNetCore.Extensions;
using Knight.Response.Factories;

app.MapGet("/users/{id:int}", async (int id, HttpContext http, IUserService users, CancellationToken ct) =>
{
    var result = await users.GetByIdAsync(id, ct);
    return result.ToIResult(http); // maps to 200/ProblemDetails
});
```

### MVC Controllers

```csharp
using Knight.Response.AspNetCore.Factories;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;
    public UsersController(IUserService users) => _users = users;

    [HttpPost]
    public async Task<IResult> Create([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        var result = await _users.CreateAsync(req, ct);
        return ApiResults.Created(result, HttpContext, location: result.IsSuccess ? $"/api/users/{result.Value.Id}" : null);
    }

    [HttpGet("{id:int}")]
    public async Task<IResult> Get(int id, CancellationToken ct)
    {
        var result = await _users.GetByIdAsync(id, ct);
        return ApiResults.Ok(result, HttpContext);
    }
}
```

---

## Behavior

* **Success (2xx):**

    * `IncludeFullResultPayload = true` → returns full `Result`.
    * `IncludeFullResultPayload = false` → returns `Value` (for `Result<T>`) or empty body.
* **Failure (non-2xx):**

    * `UseProblemDetails = true` → RFC7807 ProblemDetails (ValidationProblemDetails if enabled + errors).
    * `UseProblemDetails = false` → simple JSON messages.
* **Status codes:** determined by `CodeToHttp` (for domain codes) or fallback `StatusCodeResolver`.

---

## Default status mapping

| Result Code / Status  | Default HTTP code |
| --------------------- | ----------------- |
| `Created`             | 201               |
| `Updated`             | 200               |
| `Deleted`             | 200               |
| `NoContent`           | 204               |
| `ValidationFailed`    | 400               |
| `Unauthorized`        | 401               |
| `Forbidden`           | 403               |
| `NotFound`            | 404               |
| `AlreadyExists`       | 409               |
| `PreconditionFailed`  | 412               |
| `ConcurrencyConflict` | 409               |
| `NotSupported`        | 405               |
| `ServiceUnavailable`  | 503               |
| `UnexpectedError`     | 500               |
| `Status.Failed`       | 400               |
| `Status.Cancelled`    | 409               |
| `Status.Error`        | 500               |
| `Status.Completed`    | 200               |

---

## Factories / helpers

* `ApiResults.Ok`, `.Created`, `.Accepted`, `.NoContent`
* `ApiResults.BadRequest`, `.NotFound`, `.Conflict`, `.Unauthorized`, `.Forbidden`
* `ApiResults.UnprocessableEntity`, `.TooManyRequests`, `.ServiceUnavailable`
* `ApiResults.Problem` → force ProblemDetails regardless of success/failure

Extensions: `Result.ToIResult(HttpContext?)`, `Result<T>.ToIResult(HttpContext?)`

---

## Options

```csharp
public sealed class KnightResponseOptions
    : KnightResponseBaseOptions<HttpContext, ProblemDetails, ValidationProblemDetails>
{
    // IncludeFullResultPayload
    // UseProblemDetails
    // UseValidationProblemDetails
    // IncludeExceptionDetails
    // CodeToHttp, StatusCodeResolver
    // ValidationMapper (default)
    // ProblemDetailsBuilder, ValidationBuilder
}
```

---

## Middleware

* Logs exceptions with `ILogger`
* Converts to ProblemDetails/JSON
* Honors `IncludeExceptionDetails`
* Uses `ResultHttpResolver` to determine HTTP status

```csharp
app.UseKnightResponseExceptionMiddleware();
```

---

## License

MIT License – see [LICENSE](../../LICENSE)

## Contributing

Contributions welcome via [CONTRIBUTING.md](../../CONTRIBUTING.md).
