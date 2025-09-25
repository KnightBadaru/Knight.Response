# Knight.Response

**Knight.Response** is a lightweight, immutable, fluent result library for C# services, APIs, and applications. It provides a clean and consistent way to handle outcomes, success/failure states, codes, messages, and functional chaining — making your code simpler, safer, and more expressive.

[![NuGet Version](https://img.shields.io/nuget/v/Knight.Response.svg)](https://www.nuget.org/packages/Knight.Response)
[![ci](https://github.com/KnightBadaru/Knight.Response/actions/workflows/ci.yml/badge.svg)](https://github.com/KnightBadaru/Knight.Response/actions/workflows/ci.yml)
[![Mutation score](https://img.shields.io/endpoint?url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FKnightBadaru%2FKnight.Response%2Fmain%3Fmodule%3DKnight.Response&label=mutation%20score)](https://dashboard.stryker-mutator.io/reports/github.com/KnightBadaru/Knight.Response/main?module=Knight.Response)

---

## Features

* Immutable `Result` and `Result<T>` types
* Statuses: `Completed`, `Cancelled`, `Failed`, `Error`
* Optional `ResultCode` for domain/system-defined reasons
* Built-in `ResultCodes` (ValidationFailed, NotFound, AlreadyExists, NoContent, Created, Updated, Deleted, etc.)
* Rich `Message` with `MessageType` + optional structured `Metadata`
* Factory methods: `Success`, `Failure`, `Error`, `Cancel`, `NotFound`, `NoContent`, `FromCondition`, `Aggregate`, `Error(Exception)`
* Functional extensions: `OnSuccess`, `OnFailure`, `Map`, `Bind`
* Advanced extensions: `Ensure`, `Tap`, `Recover`, `WithCode`, `WithoutCode`, `WithMessage`, `WithMessages`, `WithDetail`
* Branching: `Match`, `MatchValue` for expressive success/failure/null handling
* Predicates: `IsSuccess`, `IsFailure`, `IsError`, `IsCancelled`, `IsUnsuccessful`, `IsUnsuccessfulOrNull`, `ValueIsNull`
* Validation helpers: `TryGetValidationResults`, `GetValidationResults`
* Pattern matching via deconstruction
* Zero runtime dependencies

---

## Installation

```bash
dotnet add package Knight.Response --version 2.0.0-preview04
```

---

## Quick Start

```csharp
using Knight.Response;
using Knight.Response.Models;

var userResult = Results.Success(new User("Knight"), code: ResultCodes.Created)
    .Ensure(u => u!.IsActive, "User is not active")
    .Tap(u => _audit.LogLogin(u!));

if (userResult.IsSuccess())
{
    Console.WriteLine(userResult.Value!.Name);
}
else if (userResult.HasCode(ResultCodes.ValidationFailed))
{
    Console.WriteLine("Validation failed");
}
```

---

## Core Concepts

```csharp
public class Result
{
    public Status Status { get; }
    public ResultCode? Code { get; }       // optional reason, e.g. "NotFound"
    public IReadOnlyList<Message> Messages { get; }
}

public class Result<T> : Result
{
    public T? Value { get; }
}
```

### Status

* `Completed`
* `Cancelled`
* `Failed`
* `Error`

### ResultCode

Optional, transport-agnostic reason for the result.
Use built-in `ResultCodes` or define your own.

### MessageType

* `Information`
* `Warning`
* `Error`

### Metadata

`Message` supports optional `Metadata` as a read-only dictionary for structured context, e.g.:

```csharp
var msg = new Message(
    MessageType.Warning,
    "Rate limit exceeded",
    new Dictionary<string, object?> { ["retryAfter"] = 30 }
);
```

---

## Factory Methods

| Method                                                         | Description                                       |
| -------------------------------------------------------------- | ------------------------------------------------- |
| `Results.Success()`                                            | Create a success result                           |
| `Results.Success<T>(T value)`                                  | Success result with data                          |
| `Results.Success<T>(T value, IReadOnlyList<Message> messages)` | Success with value and messages                   |
| `Results.Failure(string reason, ResultCode? code = null)`      | Failure result with message + optional code       |
| `Results.Failure(IReadOnlyList<Message> messages)`             | Failure from messages                             |
| `Results.Error(string reason, ResultCode? code = null)`        | Error result                                      |
| `Results.Error(Exception ex)`                                  | Error result from exception                       |
| `Results.Cancel(string reason)`                                | Cancelled result (defaults to `Warning`)          |
| `Results.Cancel(IReadOnlyList<Message> messages)`              | Cancelled from messages                           |
| `Results.NotFound()`                                           | "Not found" (defaults to `Completed` + `Warning`) |
| `Results.NoContent()`                                          | "No content" (defaults to `Completed`)            |
| `Results.FromCondition(bool condition, string failMessage)`    | Success if true, Failure if false                 |
| `Results.Aggregate(IEnumerable<Result> results)`               | Combine multiple results                          |

---

## Core Extensions

```csharp
result.OnSuccess(() => Console.WriteLine("All good"))
      .OnFailure(msgs => Console.WriteLine($"Failed: {string.Join(", ", msgs.Select(m => m.Content))}"));

var mapped = Results.Success(2).Map(x => x * 5); // Success(10)

var bound = Results.Success("abc").Bind(v => Results.Success(v.ToUpper()));
```

* **IsSuccess / IsFailure / IsError / IsCancelled / IsUnsuccessful / IsUnsuccessfulOrNull**
* **OnSuccess** – Runs an action if result is success
* **OnFailure** – Runs an action if result is not success
* **Map** – Transforms the value if success
* **Bind** – Chains another `Result` operation if success

---

## Advanced Extensions

* **Ensure** – Ensures a predicate holds for success; otherwise returns failure
* **Tap** – Executes an action for side effects, preserving the result
* **Recover** – Transforms a failure into a success with fallback value
* **WithCode / WithoutCode** – Adds, replaces, or clears a `ResultCode`
* **WithMessage** – Adds a single message to a result
* **WithMessages** – Adds multiple messages to a result
* **WithDetail** – Attaches structured metadata to the last message
* **ValueIsNull** – Checks if `Result<T>.Value` is null

```csharp
var ensured = Results.Success(5).Ensure(x => x > 0, "Must be positive");
var tapped = ensured.Tap(v => Console.WriteLine($"Value: {v}"));
var recovered = Results.Failure<int>("bad", code: ResultCodes.ValidationFailed).Recover(_ => 42);
var withMsg = recovered.WithMessage(new Message(MessageType.Information, "Recovered with default"));

if (recovered.ValueIsNull())
    Console.WriteLine("No value present.");
```

---

## Branching with `Match`

```csharp
var result = Results.Failure<int>("Not valid", code: ResultCodes.ValidationFailed);

result.Match(
    onUnsuccessful: msgs => Console.WriteLine($"Failed: {string.Join(", ", msgs.Select(m => m.Content))}"),
    onSuccess: () => Console.WriteLine("All good!")
);
```

### Producing results with `MatchValue`

```csharp
var processed = result.MatchValue(
    onUnsuccessful: msgs => Results.Failure<int>("Could not process further"),
    onNoValue: () => Results.NoContent<int>(),
    onValue: v => Results.Success(v * 10)
);
```

These let you **return directly** from a method, eliminating repeated `if/else` checks.

---

## Validation Support

Validation errors can be converted into results and later retrieved:

```csharp
using System.ComponentModel.DataAnnotations;

var errors = new List<ValidationResult>
{
    new("Name is required", new[] { "Name" }),
    new("Amount must be greater than 0", new[] { "Amount" })
};

var result = Results.Validation(errors);

if (result.TryGetValidationResults(out var vr))
{
    foreach (var v in vr)
        Console.WriteLine($"Validation: {v.ErrorMessage}");
}
```

---

## Pattern Matching

You can deconstruct results directly:

```csharp
var (status, code, messages) = Results.Failure("fail!", code: ResultCodes.ValidationFailed);

var (status, code, messages, value) = Results.Success(42, code: ResultCodes.Created);
```

Or use C# pattern matching:

```csharp
switch (result)
{
    case { Status: Status.Completed }:
        Console.WriteLine("Success!");
        break;
    case { Status: Status.Failed, Messages: var msgs }:
        Console.WriteLine($"Failed: {msgs[0].Content}");
        break;
}
```

---

## Important: `Result<T>.Value` and default values

When `T` is a **non-nullable value type**, failed results will contain the *default* value of that type (`0` for `int`, `false` for `bool`, etc.). This is a limitation of .NET generics.

```csharp
var r1 = Results.Failure<int>("bad");
Console.WriteLine(r1.Value); // 0

var r2 = Results.Failure<int?>("bad");
Console.WriteLine(r2.Value); // null
```

### Guidance

* Use **nullable value types** (e.g. `int?`, `bool?`) if you want `null` to represent "no value".
* Use **non-nullable value types** if a default like `0` or `false` makes sense in your domain.
* Reference types (e.g. `string`, `User`) will correctly return `null` when not `Completed`.

---

## License

This project is licensed under the [MIT License](../../LICENSE).

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](../../CONTRIBUTING.md).