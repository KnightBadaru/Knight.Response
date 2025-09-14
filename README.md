# Knight Response Monorepo

This repository contains the **Knight.Response** family of .NET libraries for clear, consistent, and testable result handling in service layers and web APIs.

---

## Projects

### 1) Knight.Response

A lightweight, immutable `Result` / `Result<T>` library with factories and functional-style helpers.

* Immutable results with message collection (Info/Warning/Error)
* Functional extensions: `Map`, `Bind`, `OnSuccess`, `OnFailure`, `Ensure`, `Tap`, `Recover`, `WithMessage(s)`
* Deconstruction support and rich factory methods (e.g., `Success`, `Failure`, `Error`, `Cancel`, `Aggregate`)
* Targets **.NET Standard 2.0**

**Docs:** [src/Knight.Response/README.md](src/Knight.Response/README.md)

---

### 2) Knight.Response.Abstractions.Http

Shared HTTP-facing primitives used by both `AspNetCore` and `Mvc` integrations.

* **`KnightResponseBaseOptions<THttp, TProblem, TValidationProblem>`** – base options type with:

    * `IncludeFullResultPayload`
    * `UseProblemDetails`
    * `UseValidationProblemDetails`
    * `IncludeExceptionDetails`
    * Pluggable `StatusCodeResolver`
    * Optional builders for shaping problem payloads
* **`IValidationErrorMapper`** – abstraction to project domain messages into `Dictionary<string,string[]>`
* **`DefaultValidationErrorMapper`** – pragmatic mapper supporting explicit `field` metadata and "field: message" parsing
* Targets **.NET Standard 2.0** for broad compatibility

**Docs:** [src/Knight.Response.Abstractions.Http/README.md](src/Knight.Response.Abstractions.Http/README.md)

---

### 3) Knight.Response.AspNetCore

ASP.NET Core integration that translates `Result` / `Result<T>` to `IResult`, with ProblemDetails, optional ValidationProblemDetails, and exception middleware.

* `ApiResults` helpers (`Ok`, `Created`, `Accepted`, `NoContent`, `BadRequest`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`)
* `Result.ToIResult(...)` extensions for Minimal APIs and controllers
* Centralised **ProblemDetails** / **ValidationProblemDetails** emission
* Exception middleware: `UseKnightResponseExceptionMiddleware()`
* Configured via `KnightResponseOptions` (built on the abstractions above)
* Targets **.NET 8**

**Docs:** [src/Knight.Response.AspNetCore/README.md](src/Knight.Response.AspNetCore/README.md)

---

### 4) Knight.Response.Mvc

Classic MVC / Web API 2 (`IActionResult` / `IHttpActionResult`) integration for legacy .NET Framework apps.
Provides the same clear and consistent `Result` → response translation as the ASP.NET Core package, but for the **System.Web** stack.

* `ApiResults` helpers (`Ok`, `Created`, `NoContent`, `BadRequest`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`)
* `Result.ToApiResponse(...)` extensions for MVC / Web API 2 controllers
* Centralised **ProblemDetails** / **ValidationProblemDetails** emission
* Configured via `KnightResponseOptions` (built on the shared abstractions)
* Targets **.NET Framework 4.7.1+**

**Docs:** [src/Knight.Response.Mvc/README.md](src/Knight.Response.Mvc/README.md)

### Which package do I use?

* Use **Knight.Response.Mvc** → For **System.Web MVC / Web API 2** apps targeting .NET Framework.
* Use **Knight.Response.AspNetCore** → For **ASP.NET Core** apps targeting .NET 6/7/8+.

> Note: `Knight.Response.Mvc` may reference `Microsoft.AspNetCore.Mvc` types (like `ProblemDetails`) purely for **compatibility**. This does **not** make your app an ASP.NET Core app; it remains a System.Web application.


---

## Quick glimpse

### Minimal API example

```csharp
using Knight.Response.Factories;
using Knight.Response.AspNetCore.Extensions;

app.MapGet("/users/{id:int}", async (int id, HttpContext http, IUserService svc) =>
{
    var result = await svc.GetUserAsync(id);          // Result<User>
    return result.ToIResult(http);                    // IResult (200/4xx/5xx mapped)
});
```

### MVC controller example (AspNetCore)

```csharp
using Knight.Response.Factories;
using Knight.Response.AspNetCore.Factories;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _svc;
    public UsersController(IUserService svc) => _svc = svc;

    [HttpPost]
    public async Task<IResult> Create([FromBody] CreateUser req)
    {
        var result = await _svc.CreateAsync(req);     // Result<User>
        return ApiResults.Created(result, HttpContext, $"/api/users/{result.Value.Id}");
    }
}
```

---

## Contributing

Please read our guidelines before submitting changes:

* [CODE\_OF\_CONDUCT.md](CODE_OF_CONDUCT.md)
* [CONTRIBUTING.md](CONTRIBUTING.md)

---

## License

MIT – see [LICENSE](LICENSE).