# Changelog

All notable changes to **Knight.Response.Mvc** will be documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added

* Nothing yet.

### Changed

* Nothing yet.

### Fixed

* Nothing yet.

---

## [2.0.0-preview05] - 2025-01-13

### Added


### Changed

* Aligns with **Knight.Response.Abstractions.Http 2.0.0-preview04** for consistent `ResultHttpResolver` usage and defaults.

---

## [2.0.0-preview04] - 2025-09-27

### Added

* **ProblemDetails enrichment:** `ProblemFactory` now includes `Result.Code` in the `extensions` bag when available, giving clients richer error context alongside `svcStatus` and `messages`.

### Changed

* Aligns with **Knight.Response.Abstractions.Http 2.0.0-preview03** for consistent `ResultHttpResolver` usage and defaults.

---

## [2.0.0-preview03] - 2025-09-25

### Changed

* **Centralized HTTP resolution:** `ApiResults` now delegates all status code resolution to `ResultHttpResolver` from **Knight.Response.Abstractions.Http**.
* **Cleaner implementation:** Duplicate inline logic for mapping `Result.Status` and `Result.Code` has been removed in favor of `ResultHttpResolver.ResolveHttpCode(...)`.
* **Consistent handling:** Both generic and non-generic `BuildSuccessOrFailure` methods now share the same resolution pipeline.

---

## [2.0.0-preview02] - 2025-09-24

### Changed

* **Dependency update:** downgraded `Microsoft.AspNetCore.Mvc` from **2.3.0** → **2.2.0** for broader compatibility in .NET Framework (System.Web MVC / Web API 2) applications.

---

## [2.0.0-preview01] - 2025-09-24

### Changed

* **Dependency bump:** now references `Knight.Response.Abstractions.Http 2.0.0-preview01`.
* **Service registration:** `AddKnightResponse` no longer performs post-configuration of the mapper. Instead, it uses `TryAddScoped<IValidationErrorMapper, DefaultValidationErrorMapper>()` so that:

    * If a mapper has already been registered, it will not be overridden.
    * If a consumer calls the `AddKnightResponse<TMapper>` overload, that mapper is registered and respected.
* **Mapper resolution:** `ProblemFactory` now resolves the mapper at runtime:

    1. From the current request’s DI scope (`HttpContext.RequestServices`), if available.
    2. From the `KnightResponseOptions.ValidationMapper` override, if explicitly set.
    3. From a new instance of `DefaultValidationErrorMapper` as a last fallback.

### BREAKING CHANGES

* Default behavior is aligned with **Knight.Response.Abstractions.Http 2.0.0-preview01**:

    * All options (`UseProblemDetails`, `UseValidationProblemDetails`, `IncludeFullResultPayload`, `IncludeExceptionDetails`) now default to **`false`**.
    * Mapper is no longer pre-bound on options. It is resolved per-request with the fallback strategy above.

---

## [1.0.0] - 2025-09-14

### Changed

* Promoted to stable release.
* No API changes since `0.1.0`; only documentation refinements and readiness validation.

---

## [0.1.0] - 2025-09-14

### Added

* Initial preview release of `Knight.Response.Mvc`.
* `ResultExtensions` for converting `Result` / `Result<T>` into `IActionResult`.
* `ServiceCollectionExtensions` for service registration.
* `ApiResults` factory for consistent HTTP responses.
* `ProblemFactory` for building RFC 7807-style error objects.
* `CompatProblemDetails` and `CompatValidationProblemDetails` for MVC compatibility.
* `KnightResponseOptions` for customization.

---

[0.1.0]: https://github.com/KnightBadaru/Knight.Response/releases/tag/mvc-v0.1.0
[1.0.0]: https://github.com/KnightBadaru/Knight.Response/releases/tag/mvc-v1.0.0
[2.0.0-preview01]: https://github.com/KnightBadaru/Knight.Response/releases/tag/mvc-v2.0.0-preview01
[2.0.0-preview02]: https://github.com/KnightBadaru/Knight.Response/releases/tag/mvc-v2.0.0-preview02
[2.0.0-preview03]: https://github.com/KnightBadaru/Knight.Response/releases/tag/mvc-v2.0.0-preview03
[2.0.0-preview04]: https://github.com/KnightBadaru/Knight.Response/releases/tag/mvc-v2.0.0-preview04
[2.0.0-preview05]: https://github.com/KnightBadaru/Knight.Response/releases/tag/mvc-v2.0.0-preview05
