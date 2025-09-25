# Knight.Response.Mvc

`Knight.Response.Mvc` provides integration between **Knight.Response** and **ASP.NET MVC / Web API 2 (System.Web on .NET Framework 4.7.1+)**.
It ensures consistent API response handling with standardized result-to-HTTP mapping, including `ProblemDetails` compatibility, validation error shaping, and factory methods.

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

    * `KnightResponseOptions` → Configurable options for customizing error shape, problem details, status mapping, etc.
    * `ResultHttpResolver` → Centralises `Result.Status` and `ResultCode` → HTTP status resolution

---

## Installation

```powershell
dotnet add package Knight.Response.Mvc --version 2.0.0-preview03
```

This package depends on:

* `Knight.Response` (core results)
* `Knight.Response.Abstractions.Http` (shared HTTP abstractions: options, resolver, validation mapper)
* `Microsoft.AspNetCore.Mvc (2.2.0)` — referenced for `ProblemDetails`, `ValidationProblemDetails`, and schema alignment.

> Note: This does **not** require ASP.NET Core runtime. Types are used for schema compatibility only.

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

---

## Success Payload Shape

Controlled by `KnightResponseOptions.IncludeFullResultPayload`:

* **false** (default since 2.0.0-preview01):

    * Ok → returns `Value` (of `Result<T>`) only
    * Created / Accepted → `Value` only
* **true**: return the full `Result` object on success (useful when clients want codes/messages on success).

### Failure Mapping

* If `UseValidationProblemDetails` is **true** and the mapper produces field errors → response is **ValidationProblemDetails**.
* Otherwise → response is **ProblemDetails**.
* Status code resolution order:

    1. `CodeToHttp` (domain `ResultCode` → HTTP status)
    2. `StatusCodeResolver` (default mapping: Failed=400, Cancelled=409, Error=500, Completed not used)

---

## Options

Options type comes from:

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

### Mapper Resolution

At runtime, the validation error mapper is resolved in this order:

1. From the current request’s DI scope (`HttpContext.RequestServices`).
2. From the `ValidationMapper` override on options.
3. If neither is set, `DefaultValidationErrorMapper` is used.

---

## Related Packages

* [Knight.Response](../Knight.Response) — Core response model
* [Knight.Response.Abstractions.Http](../Knight.Response.Abstractions.Http) — Shared HTTP abstractions (options, status resolver, validation mapper)
* [Knight.Response.AspNetCore](../Knight.Response.AspNetCore) — ASP.NET Core integration

### Which package do I use?

* Use **Knight.Response.Mvc** → For **System.Web MVC / Web API 2** apps targeting .NET Framework.
* Use **Knight.Response.AspNetCore** → For **ASP.NET Core** apps targeting .NET 6/7/8+.

---

## License

This project is licensed under the [MIT License](../../LICENSE).

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](../../CONTRIBUTING.md).
