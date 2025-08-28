# Knight.Response.Abstractions.Http

Cross‑framework abstractions for HTTP result handling in the **Knight.Response** family.

This package collects the **shared contracts and options** that both

* `Knight.Response.AspNetCore` (for modern ASP.NET Core / minimal APIs), and
* `Knight.Response.AspNetCore.Mvc` (for legacy ASP.NET Core MVC 2.x / .NET Framework scenarios)

use to keep behavior consistent.

[![NuGet Version](https://img.shields.io/nuget/v/Knight.Response.Abstraction.Http.svg)](https://www.nuget.org/packages/Knight.Response.Abstractions.Http)
[![ci](https://github.com/KnightBadaru/Knight.Response/actions/workflows/ci.yml/badge.svg)](https://github.com/KnightBadaru/Knight.Response/actions/workflows/ci.yml)

> **Target frameworks:** .NET Standard 2.0 (works with .NET Framework 4.7.1+ and .NET Core/.NET 6+).

---

## What’s inside

* **`KnightResponseBaseOptions<TProblem, TValidation>`**
  Base options class that centralizes:

    * `UseProblemDetails`
    * `UseValidationProblemDetails`
    * `IncludeFullResultPayload`
    * `IncludeExceptionDetails`
    * `StatusCodeResolver` delegates
    * Optional builders/hooks for shaping problem responses

  The generic parameters let each integration plug in the framework’s
  own `ProblemDetails` / `ValidationProblemDetails` types without duplication.

* **Validation error mapping**

    * **`IValidationErrorMapper`** — abstraction for converting domain `Message`s
      into a `Dictionary<string,string[]>` suitable for validation problems.
    * **`DefaultValidationErrorMapper`** — pragmatic mapper that:

        * reads an explicit `field` from message metadata when available, and
        * falls back to parsing messages in the form `"field: message"`.

---

## Install

```bash
dotnet add package Knight.Response.Abstractions.Http
```

You typically **won’t reference this directly** in application code; it’s a
transitive dependency of the ASP.NET integration packages. It’s useful if you’re
building your own integration and want to align with Knight.Response conventions.

---

## Quick example (custom mapper)

```csharp
using Knight.Response.Abstractions.Http.Mappers;

public sealed class MyValidationMapper : IValidationErrorMapper
{
    public IDictionary<string, string[]> Map(IReadOnlyList<Message> messages)
        => new Dictionary<string, string[]> { ["name"] = new[] { "is required" } };
}
```

Register this type in the DI container of your chosen integration package
(e.g., `Knight.Response.AspNetCore` or `Knight.Response.AspNetCore.Mvc`).

---

## Related packages

* `Knight.Response` — immutable `Result` / `Result<T>` core library.
* `Knight.Response.AspNetCore` — modern ASP.NET Core + minimal APIs integration.
* `Knight.Response.AspNetCore.Mvc` — ASP.NET Core MVC 2.x / .NET Framework–friendly integration.

---

## License

Licensed under the MIT License. See `LICENSE` in the repository root.
