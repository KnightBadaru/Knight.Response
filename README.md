# Knight Response Monorepo

This repository contains the **Knight.Response** family of .NET libraries, focused on providing lightweight, immutable result handling for service layers in C#. These libraries are designed to improve code clarity, maintainability, and robustness by standardising how operations return and handle outcomes.

---

## Projects

### 1. Knight.Response

A lightweight, immutable `Result`/`Result<T>` library with factories, functional extensions, and pattern matching for C# service layers.

* Immutable results for safer, predictable state handling
* Functional extensions (`Map`, `Bind`, `OnSuccess`, `OnFailure`, etc.)
* Built-in message handling (`Info`, `Warning`, `Error`)
* Fully unit tested with **100% mutation score**
* .NET Standard 2.0 compatible

**Docs:** [Knight.Response README](src/Knight.Response/README.md)

---

### 2. Knight.Response.AspNetCore

ASP.NET Core integration for `Result`/`Result<T>`. Provides consistent translation to HTTP responses (`IResult`), RFC7807 `ProblemDetails`, validation error mapping, and exception middleware.

* Extension methods for Minimal APIs and Controllers
* `ApiResults` helpers (`Ok`, `Created`, `Accepted`, etc.)
* RFC7807 `ProblemDetails` and `ValidationProblemDetails` support
* Exception middleware for consistent error payloads
* Configurable via `KnightResponseOptions`

**Docs:** [Knight.Response.AspNetCore README](src/Knight.Response.AspNetCore/README.md)

---

## Usage Examples

### Minimal API

```csharp
using Knight.Response.Factories;
using Knight.Response.AspNetCore.Extensions;

app.MapGet("/users/{id:int}", async (int id, HttpContext http, IUserService userService) =>
{
    var result = await userService.GetUserAsync(id);
    return result.ToIResult(http); // Converts Result<T> → IResult automatically
});
```

### MVC Controller

```csharp
using Knight.Response.Factories;
using Knight.Response.AspNetCore.Factories;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    [HttpPost]
    public async Task<IResult> Create([FromBody] CreateUserRequest request)
    {
        var result = await _userService.CreateUserAsync(request);
        return ApiResults.Created(result, HttpContext, location: $"/api/users/{result.Value.Id}");
    }

    [HttpGet("{id:int}")]
    public async Task<IResult> Get(int id)
    {
        var result = await _userService.GetUserAsync(id);
        return ApiResults.Ok(result, HttpContext);
    }
}
```

---

## Contributing

We welcome contributions! Please read the following before getting started:

* [CODE\_OF\_CONDUCT.md](CODE_OF_CONDUCT.md)
* [CONTRIBUTING.md](CONTRIBUTING.md)

---

## License

All projects in this repository are licensed under the [MIT License](LICENSE).
