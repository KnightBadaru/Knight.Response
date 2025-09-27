# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added

* Nothing yet.

### Changed

* Nothing yet.

### Fixed

* Nothing yet.

---

## [2.0.0-preview01] - 2025-09-27

### Added

* Introduced `ResultHttpResolver` integration for consistent status code resolution using `CodeToHttp` and `StatusCodeResolver`.
* ProblemDetails/ValidationProblemDetails now include `svcCode` when a `ResultCode` is present.
* New `ApiResults.UnprocessableEntity` (422), `TooManyRequests` (429), and `ServiceUnavailable` (503) helpers.
* Added `CreatedAt` and `AcceptedAt` overloads with `Location` header support.
* Introduced global `ToActionResult` extension for automatic conversion of `Result` / `Result<T>` based on configuration.

### Changed

* Exception middleware now uses `ResultHttpResolver` instead of directly invoking `StatusCodeResolver`.
* Default `NoContent` and `NotFound` factories now assign `ResultCodes.NoContent` and `ResultCodes.NotFound` respectively.

### Fixed

* Ensured consistent mapping of `ResultCodes.NotSupported` → HTTP 405 (Method Not Allowed).

---

## [1.0.2] - 2025-09-01

### Changed

* Updated dependency: now requires **Knight.Response.Abstractions.Http 1.0.2**.

    * Ensures compatibility with Knight.Response 1.1.0 and its new validation APIs.
* No functional changes in `Knight.Response.AspNetCore` itself.

---

## [1.0.1] - 2025-08-28

### Changed

* Refactored internal implementation to use **Knight.Response.Abstractions.Http** for shared options and validation mapping.

    * Removed local `KnightResponseOptions`, `IValidationErrorMapper`, and `DefaultValidationErrorMapper` duplicates.
    * Now depends on the abstractions library for consistency across ASP.NET Core and MVC integrations.
* No functional changes to public API surface.

---

## [1.0.0] - 2025-08-25

### Added

* Initial release of **Knight.Response.AspNetCore**
* Conversion helpers via `ApiResults`:

    * `Ok`, `Created`, `Accepted`, `NoContent`, `BadRequest`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`
* Extension methods for minimal APIs and controllers:

    * `Result.ToIResult(...)`
    * `Result<T>.ToIResult(...)`
* RFC7807 **ProblemDetails** and **ValidationProblemDetails** support

    * Enabled via `UseProblemDetails` and `UseValidationProblemDetails`
* Exception middleware:

    * `UseKnightResponseExceptionMiddleware` for consistent error payloads
* DI integration (`AddKnightResponse`) with the following options:

    * `IncludeFullResultPayload`
    * `UseProblemDetails`
    * `UseValidationProblemDetails`
    * `IncludeExceptionDetails`
    * `StatusCodeResolver`
    * `ValidationMapper`
    * `ProblemDetailsBuilder`, `ValidationBuilder`
* DI overload:

    * `AddKnightResponse<TMapper>` for custom validation error mappers
* Default validation error mapper:

    * `DefaultValidationErrorMapper`

---

[1.0.0]: https://github.com/KnightBadaru/Knight.Response/releases/tag/aspnetcore-v1.0.0
[1.0.1]: https://github.com/KnightBadaru/Knight.Response/releases/tag/aspnetcore-v1.0.1
[1.0.2]: https://github.com/KnightBadaru/Knight.Response/releases/tag/aspnetcore-v1.0.2
[2.0.0-preview01]: https://github.com/KnightBadaru/Knight.Response/releases/tag/aspnetcore-v2.0.0-preview01
