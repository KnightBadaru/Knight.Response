# Knight.Response.Mvc

`Knight.Response.Mvc` provides integration between **Knight.Response** and **ASP.NET MVC / Web API 2 (System.Web on .NET Framework 4.7.1+)**.
It brings consistent API response handling, including `ProblemDetails` compatibility and factory methods for building standardized responses.

This package is the **MVC counterpart** to `Knight.Response.AspNetCore`, designed for projects that cannot use ASP.NET Core but still want the same structured response model.

[![NuGet Version](https://img.shields.io/nuget/v/Knight.Response.Mvc.svg)](https://www.nuget.org/packages/Knight.Response.Mvc)
[![ci](https://github.com/KnightBadaru/Knight.Response/actions/workflows/ci.yml/badge.svg)](https://github.com/KnightBadaru/Knight.Response/actions/workflows/ci.yml)
[![Mutation score](https://img.shields.io/endpoint?url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FKnightBadaru%2FKnight.Response%2Fmain%3Fmodule%3DKnight.Response.Mvc&label=mutation%20score)](https://dashboard.stryker-mutator.io/reports/github.com/KnightBadaru/Knight.Response/main?module=Knight.Response.Mvc)

---

## Features

* **Extension Methods**
    * `ResultExtensions` → Converts `Result` / `Result<T>` into `IActionResult`
    * `ServiceCollectionExtensions` → Registers Knight.Response services for MVC
* **Factories**
    * `ApiResults` → Strongly-typed helpers to return success, failure, unauthorized, forbidden, etc.
    * `ProblemFactory` → Builds RFC 7807-compatible `ProblemDetails` for error responses
* **Infrastructure**
    * `CompatProblemDetails` → Compatible `ProblemDetails` implementation for MVC / Web API 2
    * `CompatValidationProblemDetails` → Validation error details aligned with modern ASP.NET Core behavior
* **Options**
    * `KnightResponseOptions` → Configurable options for customizing error shape, problem details, etc.

---

## Installation

```powershell
dotnet add package Knight.Response.Mvc
```

This package depends on:
* `Knight.Response` (core results)
* `Knight.Response.Abstractions.Http` (shared options + mapper)
* `Microsoft.AspNetCore.Mvc` (2.x)

---

## Usage

### 1. Configure Services

Register Knight.Response in `Startup.cs`:

```csharp
public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        var services = new ServiceCollection();

        services.AddKnightResponse(); // from ServiceCollectionExtensions
    }
}
```

---

### 2. Return Results in Controllers

```csharp
public class AccountsController : ApiController
{
    [HttpGet]
    public IHttpActionResult GetAccount(string id)
    {
        var result = _accountService.GetById(id);

        // Convert Result<T> → IActionResult
        return result.ToOkActionResult();
    }

    [HttpPost]
    public IHttpActionResult Create(CreateAccountRequest request)
    {
        var result = _accountService.Create(request);

        // Returns standardized 201 Created or BadRequest/ProblemDetails as per configuration
        return result.ToCreatedActionResult();
    }
}
```

## Success payload shape
Controlled by `KnightResponseOptions.IncludeFullResultPayload`:
* **true** (default): return the full `Result` object on success (useful for clients that want messages even on success).
* **false**:
  * Ok → returns `Value` (of `Result<T>`) only
  * Created / Accepted → `Value` only

### Failure mapping
* If `UseValidationProblemDetails` is **true** and the mapper produces fields errors, the response is a **ValidationProblemDetails**.
* Otherwise, a standard **ProblemDetails** is returned.
* Status code is resolved from `Result.Status` using `StatusCodeResolver` (defaults: `Failed`= 400, `Cancelled`= 409, `Error`= 500, `Completed` not used)

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
});
```

---

## Related Packages

* [Knight.Response](../Knight.Response) — Core response model
* [Knight.Response.AspNetCore](../Knight.Response.AspNetCore) — ASP.NET Core integration

### Which package do I use?

* Use **Knight.Response.Mvc** → For **System.Web MVC / Web API 2** apps targeting .NET Framework.
* Use **Knight.Response.AspNetCore** → For **ASP.NET Core** apps targeting .NET 6/7/8+.

> Note: `Knight.Response.Mvc` may reference `Microsoft.AspNetCore.Mvc` types (like `ProblemDetails`) purely for **compatibility**. This does **not** require ASP.NET Core runtime.

---

## License

This project is licensed under the [MIT License](../../LICENSE).

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](../../CONTRIBUTING.md).
