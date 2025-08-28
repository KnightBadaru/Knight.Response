# Knight.Response.AspNetCore

ASP.NET Core integration for [Knight.Response]. It converts `Result` / `Result<T>` to HTTP responses, supports RFC7807 ProblemDetails (including validation errors), and provides an exception middleware for consistent error payloads.

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
dotnet add package Knight.Response.AspNetCore
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
    // Defaults shown; override as needed
    options.IncludeFullResultPayload    = true;
    options.UseProblemDetails           = false;
    options.UseValidationProblemDetails = false;
    options.IncludeExceptionDetails     = false; // keep false in production

    // Inline override (not common; prefer the typed overload below)
    options.ValidationMapper = new MyValidationErrorMapper();
});

// Option A: register a custom validation mapper separately
builder.Services.AddScoped<IValidationErrorMapper, MyValidationErrorMapper>();

// Option B: use the typed overload
builder.Services.AddKnightResponse<MyValidationErrorMapper>(options =>
{
    options.UseValidationProblemDetails = true;
});
```

### 2) Add the exception middleware (optional)

```csharp
var app = builder.Build();

app.UseKnightResponseExceptionMiddleware(); // must be registered before endpoints

app.MapGet("/", () => "OK");
app.Run();
```

The middleware catches unhandled exceptions and returns a consistent payload (ProblemDetails or JSON), honoring your configured options.

---

## Using results in endpoints and controllers

You can return a `Result`/`Result<T>` as an ASP.NET Core `IResult` via helpers or extensions.

### Minimal APIs

```csharp
using Knight.Response.AspNetCore.Extensions;
using Knight.Response.Factories;

// Example domain service
public interface IUserService
{
    Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct);
}

public record UserDto(int Id, string Name);

// Minimal API endpoint
app.MapGet("/users/{id:int}", async (int id, HttpContext http, IUserService userService, CancellationToken ct) =>
{
    var result = await userService.GetByIdAsync(id, ct);

    // Converts Result<UserDto> → IResult (200 if success, 400/ProblemDetails otherwise)
    return result.ToIResult(http);
});
```

### MVC Controllers

```csharp
using Knight.Response.AspNetCore.Factories;
using Knight.Response.Core;
using Microsoft.AspNetCore.Mvc;

// Example DTOs
public record CreateUserRequest(string Name);
public record UserDto(int Id, string Name);

// Example domain service
public interface IUserService
{
    Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken ct);
    Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct);
}

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var result = await _userService.CreateAsync(request, ct);

        // Maps Result<UserDto> → 201 Created or error
        return ApiResults.Created(result, HttpContext, location: result.IsSuccess 
            ? $"/api/users/{result.Value.Id}" 
            : null);
    }

    [HttpGet("{id:int}")]
    public async Task<IResult> Get(int id, CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(id, ct);

        // Maps Result<UserDto> → 200 OK if found, otherwise 400/ProblemDetails
        return ApiResults.Ok(result, HttpContext);
    }
}
```

---

## What gets returned

Behavior depends on `KnightResponseOptions`:

* **Success (2xx):**

    * `IncludeFullResultPayload = true`
      Returns the full `Result`/`Result<T>` as JSON (`201 Created` uses the body and optional `Location` header).
    * `IncludeFullResultPayload = false`
      Returns `Value` (for `Result<T>`) or an empty 2xx with the chosen status.

* **Failure (non-2xx):**

    * `UseProblemDetails = true`
      Returns RFC7807 `ProblemDetails`. If `UseValidationProblemDetails = true` and the validation mapper produces field errors, returns `ValidationProblemDetails`.
    * `UseProblemDetails = false`
      Returns a simple JSON array of messages with the chosen status.

* **Status codes:**
  Determined by `StatusCodeResolver` (defaults shown below).

---

## Default status code mapping

| Knight.Response Status | Default HTTP Status Code              |
| ---------------------- | ------------------------------------- |
| `Success`              | 200 (or requested 2xx, e.g. 201, 204) |
| `Failed`               | 400                                   |
| `Cancelled`            | 409                                   |
| `Error`                | 500                                   |

You can override this mapping via `KnightResponseOptions.StatusCodeResolver`.

---

## API surface

### Result → IResult conversion

Use either the **extensions** or the **factory**:

* Extensions (Minimal API friendly)

    * `Result.ToIResult(HttpContext? http = null)`
    * `Result<T>.ToIResult(HttpContext? http = null)`

* Factory methods (works everywhere)

    * `ApiResults.Ok(Result|Result<T>, HttpContext? http = null)`
    * `ApiResults.Created(Result|Result<T>, HttpContext? http = null, string? location = null)`
    * `ApiResults.Accepted(Result|Result<T>, HttpContext? http = null, string? location = null)`
    * `ApiResults.NoContent(Result, HttpContext? http = null)`
    * `ApiResults.BadRequest(Result, HttpContext? http = null)`
    * `ApiResults.NotFound(Result, HttpContext? http = null)`
    * `ApiResults.Conflict(Result, HttpContext? http = null)`
    * `ApiResults.Unauthorized()`, `ApiResults.Forbidden()`

All methods consult DI for `KnightResponseOptions` if `HttpContext` is provided; otherwise defaults are used.

---

## Options

Options type comes from

```csharp
public sealed class KnightResponseOptions
    : KnightResponseBaseOptions<ProblemDetails, ValidationProblemDetails>
{
    // Same core properties:
    // - IncludeFullResultPayload
    // - UseProblemDetails
    // - UseValidationProblemDetails
    // - IncludeExceptionDetails
    // - StatusCodeResolver
    // - ValidationMapper (default: DefaultValidationErrorMapper from Abstractions)
    // - ProblemDetailsBuilder, ValidationBuilder
}
```

Register and customize:

```csharp
builder.Services.AddKnightResponse(options =>
{
    options.IncludeFullResultPayload    = true;
    options.UseProblemDetails           = true;
    options.UseValidationProblemDetails = true;

    options.StatusCodeResolver = status => status switch
    {
        Status.Failed    => StatusCodes.Status400BadRequest,
        Status.Cancelled => StatusCodes.Status409Conflict,
        Status.Error     => StatusCodes.Status500InternalServerError,
        _                => StatusCodes.Status400BadRequest
    };

    // Optional customization of ProblemDetails payload
    options.ProblemDetailsBuilder = (http, result, pd) =>
    {
        pd.Extensions["svcStatus"] = result.Status.ToString();
    };
});
```

---

## Validation mapping

By default, validation messages are projected to a simple `Dictionary<string, string[]>` using `DefaultValidationErrorMapper`. Plug in your own:

```csharp
builder.Services.AddKnightResponse();
builder.Services.AddScoped<IValidationErrorMapper, MyCustomMapper>();
```

If the mapper returns one or more field errors and `UseValidationProblemDetails` is enabled, responses use `ValidationProblemDetails`. Otherwise they fall back to standard `ProblemDetails`.

---

## Exception middleware

`UseKnightResponseExceptionMiddleware()` wraps the pipeline and converts unhandled exceptions to a consistent error response:

* Logs exceptions via `ILogger<KnightResponseExceptionMiddleware>`
* Honors `UseProblemDetails` and `IncludeExceptionDetails`
* Uses `StatusCodeResolver` to select the HTTP status (default 500)

Register early in the pipeline (after logging, before endpoints):

```csharp
app.UseKnightResponseExceptionMiddleware();
```

---

## Notes

* Works with both Minimal APIs and MVC controllers. `IResult` is first-class in .NET 8 and can be returned from controllers as well.
* If you prefer `IActionResult`, you can wrap `IResult` via `Results.Extensions.ToActionResult()` in .NET 8+, but this library focuses on `IResult` for modern apps.
* For consistent payloads across your app, prefer returning `Result`/`Result<T>` and translating via these helpers everywhere.

---

## License

This project is licensed under the [MIT License](../../LICENSE).

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](../../CONTRIBUTING.md).
