# Knight.Response.Mvc

`Knight.Response.Mvc` integrates **Knight.Response** with **ASP.NET MVC / Web API 2 (System.Web on .NET Framework 4.7.1+)**, providing consistent API responses with standardized result-to-HTTP mapping. It includes `ProblemDetails` compatibility, validation error shaping, and factory helpers.

This package is the **MVC counterpart** to `Knight.Response.AspNetCore`, designed for legacy projects that cannot move to ASP.NET Core but still want modern structured response handling.

[![NuGet Version](https://img.shields.io/nuget/v/Knight.Response.Mvc.svg)](https://www.nuget.org/packages/Knight.Response.Mvc)
[![ci](https://github.com/KnightBadaru/Knight.Response/actions/workflows/ci.yml/badge.svg)](https://github.com/KnightBadaru/Knight.Response/actions/workflows/ci.yml)
[![Mutation score](https://img.shields.io/endpoint?url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FKnightBadaru%2FKnight.Response%2Fmain%3Fmodule%3DKnight.Response.Mvc&label=mutation%20score)](https://dashboard.stryker-mutator.io/reports/github.com/KnightBadaru/Knight.Response/main?module=Knight.Response.Mvc)

---

## Features

* **Extension Methods**

    * `ResultExtensions` → Convert `Result` / `Result<T>` into `IActionResult`
    * `ServiceCollectionExtensions` → Register Knight.Response services for MVC

* **Factories**

    * `ApiResults` → Helpers to return success, failure, unauthorized, forbidden, etc.
    * `ProblemFactory` → Builds RFC 7807-compatible `ProblemDetails` and `ValidationProblemDetails`

* **Infrastructure**

    * `CompatProblemDetails` → `ProblemDetails` implementation compatible with MVC / Web API 2
    * `CompatValidationProblemDetails` → Modern validation error details for MVC

* **Options**

    * `KnightResponseOptions` → Configurable response shaping (problem details, validation, exception details)
    * `ResultHttpResolver` → Central mapping from `Result.Status` / `ResultCode` → HTTP status codes

---

## Installation

```powershell
dotnet add package Knight.Response.Mvc --version 2.0.0-preview03
```

Dependencies:

* `Knight.Response` (core results)
* `Knight.Response.Abstractions.Http` (shared HTTP abstractions)
* `Microsoft.AspNetCore.Mvc (2.2.0)` — referenced for schema compatibility (`ProblemDetails`, `ValidationProblemDetails`), runtime not required.

---

## Usage

### 1. Configure Services

Register in `Startup.cs`:

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
        return result.ToOkActionResult();
    }

    [HttpPost]
    public IHttpActionResult Create(CreateAccountRequest request)
    {
        var result = _accountService.Create(request);
        return result.ToCreatedActionResult();
    }
}
```

---

## Response Shape

### Success Payloads

Controlled by `KnightResponseOptions.IncludeFullResultPayload`:

* **false** (default):

    * `Ok` → returns `Value` (for `Result<T>`) only
    * `Created` / `Accepted` → `Value` only
* **true** → returns the full `Result` object (with codes/messages)

### Failure Mapping

* If `UseValidationProblemDetails` is **true** and field errors are mapped → returns **ValidationProblemDetails**
* Otherwise → returns **ProblemDetails**

**Status code resolution order:**

1. `CodeToHttp` (domain `ResultCode` → HTTP)
2. `StatusCodeResolver` (default: Failed=400, Cancelled=409, Error=500)

---

## Options

```csharp
public sealed class KnightResponseOptions
    : KnightResponseBaseOptions<HttpContext, ProblemDetails, ValidationProblemDetails>
{
    // Core properties:
    // - IncludeFullResultPayload (default: false)
    // - UseProblemDetails (default: false)
    // - UseValidationProblemDetails (default: false)
    // - IncludeExceptionDetails (default: false)
    // - CodeToHttp (optional)
    // - StatusCodeResolver (default mapping provided)
    // - ValidationMapper (optional override)
    // - ProblemDetailsBuilder, ValidationBuilder (hooks)
}
```

### Validation Mapper Resolution

1. From `HttpContext.RequestServices`
2. From `ValidationMapper` override in options
3. Default: `DefaultValidationErrorMapper`

---

## Related Packages

* [Knight.Response](../Knight.Response) — Core response model
* [Knight.Response.Abstractions.Http](../Knight.Response.Abstractions.Http) — Shared HTTP abstractions
* [Knight.Response.AspNetCore](../Knight.Response.AspNetCore) — ASP.NET Core integration

### Which package should I use?

* **Knight.Response.Mvc** → For System.Web MVC / Web API 2 (.NET Framework)
* **Knight.Response.AspNetCore** → For ASP.NET Core (6/7/8+)

---

## License

MIT License — see [LICENSE](../../LICENSE).

## Contributing

Contributions welcome! See [CONTRIBUTING.md](../../CONTRIBUTING.md).
