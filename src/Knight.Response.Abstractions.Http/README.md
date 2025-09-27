# Knight.Response.Abstractions.Http

Cross‑framework abstractions for HTTP result handling in the **Knight.Response** family.

This package collects the **shared contracts, options, and resolvers** that both

* `Knight.Response.AspNetCore` (for modern ASP.NET Core / minimal APIs), and
* `Knight.Response.AspNetCore.Mvc` (for legacy ASP.NET Core MVC 2.x / .NET Framework scenarios)

use to keep behavior consistent.

[![NuGet Version](https://img.shields.io/nuget/v/Knight.Response.Abstractions.Http.svg)](https://www.nuget.org/packages/Knight.Response.Abstractions.Http)
[![ci](https://github.com/KnightBadaru/Knight.Response/actions/workflows/ci.yml/badge.svg)](https://github.com/KnightBadaru/Knight.Response/actions/workflows/ci.yml)
[![Mutation score](https://img.shields.io/endpoint?url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FKnightBadaru%2FKnight.Response%2Fmain%3Fmodule%3DKnight.Response.Abstractions.Http&label=mutation%20score)](https://dashboard.stryker-mutator.io/reports/github.com/KnightBadaru/Knight.Response/main?module=Knight.Response.Abstractions.Http)

> **Target frameworks:** .NET Standard 2.0 (works with .NET Framework 4.7.1+ and .NET Core/.NET 6+).

---

## What’s inside

* **`KnightResponseBaseOptions<TH, TProblem, TValidation>`**
  Base options class that centralizes:

    * `UseProblemDetails` (default: `false`)
    * `UseValidationProblemDetails` (default: `false`)
    * `IncludeFullResultPayload` (default: `false`, was `true` before 2.0.0-preview01)
    * `IncludeExceptionDetails` (default: `false`)
    * `CodeToHttp` delegate (optional, domain codes → HTTP codes)
    * `StatusCodeResolver` delegate (defaults to 400/409/500/200, nullable from 2.0.0-preview03)
    * Optional builders/hooks for shaping problem responses

  The generic parameters let each integration plug in the framework’s
  own **HttpContext type (`TH`)**, `ProblemDetails` type (`TProblem`),
  and `ValidationProblemDetails` type (`TValidation`) without duplication.

* **HTTP mapping defaults and resolvers**

    * **`KnightResponseHttpDefaults`** — default mapping from `Status` to HTTP codes.
    * **`ResultHttpResolver`** — unified logic for resolving an HTTP code from a `Result`,
      honoring `CodeToHttp` first and falling back to `StatusCodeResolver`.

* **Validation error mapping**

    * **`IValidationErrorMapper`** — abstraction for converting domain `Message`s
      into a `Dictionary<string,string[]>` suitable for validation problems.
    * **`DefaultValidationErrorMapper`** — conservative default implementation.
    * Consumers can register their own mapper in DI. If none is provided, integrations
      will fall back to the default.

---

## Install

```bash
dotnet add package Knight.Response.Abstractions.Http
```

You typically **won’t reference this directly** in application code; it’s a
transitive dependency of the ASP.NET/MVC integration packages. It’s useful if you’re
building your own integration and want to align with Knight.Response conventions.

---

## Quick examples

### Custom validation mapper

```csharp
using Knight.Response.Abstractions.Http.Mappers;

public sealed class MyValidationMapper : IValidationErrorMapper
{
    public IDictionary<string, string[]> Map(IReadOnlyList<Message> messages)
        => new Dictionary<string, string[]> { ["name"] = new[] { "is required" } };
}
```

Register this type in the DI container of your chosen integration package
(e.g., `Knight.Response.AspNetCore` or `Knight.Response.Mvc`).

### Custom HTTP code mapping

```csharp
using Knight.Response.Models;
using Knight.Response.Abstractions.Http.Options;

services.AddKnightResponse(opts =>
{
    opts.CodeToHttp = code => code?.Value switch
    {
        var v when v == ResultCodes.NotFound.Value       => 404,
        var v when v == ResultCodes.AlreadyExists.Value  => 409,
        var v when v == ResultCodes.NoContent.Value      => 204,
        var v when v == ResultCodes.NotSupported.Value   => 405,
        _ => null
    };
});
```

---

## Related packages

* `Knight.Response` — immutable `Result` / `Result<T>` core library.
* `Knight.Response.AspNetCore` — modern ASP.NET Core + minimal APIs integration.
* `Knight.Response.AspNetCore.Mvc` — ASP.NET Core MVC 2.x / .NET Framework–friendly integration.

---

## License

This project is licensed under the [MIT License](../../LICENSE).

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](../../CONTRIBUTING.md).
