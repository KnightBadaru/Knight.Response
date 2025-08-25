# Knight.Response

**Knight.Response** is a lightweight, immutable, fluent result library for C# services, APIs, and applications.
It provides a clean and consistent way to handle outcomes, success/failure states, messages, and functional chaining — making your code simpler, safer, and more expressive.

[![NuGet Version](https://img.shields.io/nuget/v/Knight.Response.svg)](https://www.nuget.org/packages/Knight.Response)
[![ci](https://github.com/KnightBadaru/Knight.Response/actions/workflows/ci.yml/badge.svg)](https://github.com/KnightBadaru/Knight.Response/actions/workflows/ci.yml)
[![Mutation score](https://img.shields.io/endpoint?url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FKnightBadaru%2FKnight.Response%2Fmain%3Fmodule%3DKnight.Response&label=mutation%20score)](https://dashboard.stryker-mutator.io/reports/github.com/KnightBadaru/Knight.Response/main?module=Knight.Response)


---

## Features

* Immutable `Result` and `Result<T>` types
* Statuses: `Completed`, `Cancelled`, `Failed`, `Error`
* Rich `Message` with `MessageType` + optional structured `Metadata`
* Factory methods: `Success`, `Failure`, `Error`, `Cancel`, `NotFound`, `FromCondition`, `Aggregate`, `Error(Exception)`
* Functional extensions: `OnSuccess`, `OnFailure`, `Map`, `Bind`
* Advanced extensions: `Ensure`, `Tap`, `Recover`, `WithMessage`, `WithMessages`
* Pattern matching via deconstruction
* Zero runtime dependencies

---

## Installation

```bash
dotnet add package Knight.Response
```

---

## Quick Start

```csharp
using Knight.Response;

var userResult = Results.Success(new User("Knight"))
    .Ensure(u => u!.IsActive, "User is not active")
    .Tap(u => _audit.LogLogin(u!));

if (userResult.IsSuccess)
{
    Console.WriteLine(userResult.Value!.Name);
}
```

---

## Core Concepts

```csharp
public class Result
{
    public Status Status { get; }
    public IReadOnlyList<Message> Messages { get; }
    public bool IsSuccess { get; } // Status == Completed
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

| Method                                                               | Description                                       |
| -------------------------------------------------------------------- | ------------------------------------------------- |
| `Results.Success()`                                                  | Create a success result                           |
| `Results.Success<T>(T value)`                                        | Success result with data                          |
| `Results.Success<T>(T value, IReadOnlyList<Message> messages)`       | Success with value and messages                   |
| `Results.Failure(string reason)`                                     | Failure result with message                       |
| `Results.Failure(IReadOnlyList<Message> messages)`                   | Failure from messages                             |
| `Results.Error(string reason)`                                       | Error result                                      |
| `Results.Error(Exception ex)`                                        | Error result from exception                       |
| `Results.Cancel(string reason)`                                      | Cancelled result (defaults to `Warning`)          |
| `Results.Cancel(IReadOnlyList<Message> messages)`                    | Cancelled from messages                           |
| `Results.NotFound()`                                                 | "Not found" (defaults to `Completed` + `Warning`) |
| `Results.NotFound(string message, Status status = Status.Completed)` | "Not found" with override status                  |
| `Results.FromCondition(bool condition, string failMessage)`          | Success if true, Failure if false                 |
| `Results.Aggregate(IEnumerable<Result> results)`                     | Combine multiple results                          |

---

## Core Extensions

```csharp
result.OnSuccess(() => Console.WriteLine("All good"))
      .OnFailure(msgs => Console.WriteLine($"Failed: {string.Join(", ", msgs.Select(m => m.Content))}"));

var mapped = Results.Success(2).Map(x => x * 5); // Success(10)

var bound = Results.Success("abc").Bind(v => Results.Success(v.ToUpper()));
```

* **OnSuccess** – Runs an action if result is success
* **OnFailure** – Runs an action if result is failure/error/cancelled
* **Map** – Transforms the value if success
* **Bind** – Chains another `Result` operation if success

---

## Advanced Extensions

* **Ensure** – Ensures a predicate holds for success; otherwise returns failure
* **Tap** – Executes an action for side effects, preserving the result
* **Recover** – Transforms a failure into a success with fallback value
* **WithMessage** – Adds a single message to a result
* **WithMessages** – Adds multiple messages to a result

```csharp
var ensured = Results.Success(5).Ensure(x => x > 0, "Must be positive");
var tapped = ensured.Tap(v => Console.WriteLine($"Value: {v}"));
var recovered = Results.Failure<int>("bad").Recover(_ => 42);
var withMsg = recovered.WithMessage(new Message(MessageType.Information, "Recovered with default"));
```

---

## Pattern Matching

You can deconstruct results directly:

```csharp
var (status, messages) = Results.Failure("fail!");

var (status, messages, value) = Results.Success(42);
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

## License

This project is licensed under the [MIT License](../../LICENSE).

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](../../CONTRIBUTING.md).
