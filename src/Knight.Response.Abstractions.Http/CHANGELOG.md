# Changelog

All notable changes to **Knight.Response.Abstractions.Http** will be documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## \[Unreleased]

### Added

* Nothing yet.

### Changed

* Nothing yet.

### Fixed

* Nothing yet.

---

## [2.0.0-preview03] - 2025-09-27

### Added
* **ProblemDetails enrichment**: `Result.Code` (when present) is now included in the extensions bag of both ProblemDetails and ValidationProblemDetails. 
  * Key: "svcCode" → the string value of the ResultCode.

### Changed
* **Dependency bump**: aligned to `Knight.Response 2.0.0-preview05`. 
* Internal cleanup and resolver improvements for consistency across MVC and Minimal APIs.

---

## [2.0.0-preview02] - 2025-09-25

### Added

* **ResultHttpResolver**: central resolver for mapping `Result.Status`/`Result.Code` to HTTP status codes.
* **CodeToHttp**: new optional delegate on `KnightResponseBaseOptions` for mapping `ResultCode` values to HTTP codes.
* **KnightResponseHttpDefaults**: provides built-in defaults for `Status` → HTTP mapping (`Error=500`, `Cancelled=409`, `Failed=400`, `Completed=200`).

### Changed

* **Dependency bump:** aligned to `Knight.Response 2.0.0-preview04`.
* `ResolveHttpCode` methods now check `CodeToHttp` first (if set and non-null), then fall back to `StatusCodeResolver`, and finally to `KnightResponseHttpDefaults.StatusToHttp` if none provided.
* `StatusCodeResolver` on `KnightResponseBaseOptions` is now nullable. Consumers can omit it and rely entirely on defaults.

### BREAKING CHANGES

* `StatusCodeResolver` is no longer guaranteed non-null. Integrations must be updated to call through `ResultHttpResolver` instead of invoking it directly.

---

## [2.0.0-preview01] - 2025-09-24

### Changed

* **Dependency bump:** aligned to `Knight.Response 2.0.0-preview03`.
* **Defaults are opt-in:** `KnightResponseBaseOptions` properties now default to **`false`** (*previously* some were enabled by default). This makes behavior explicit and safer by default:

    * `UseProblemDetails = false`
    * `UseValidationProblemDetails = false`
    * `IncludeFullResultPayload = false` (*was `true` before*)
    * `IncludeExceptionDetails = false`
* **Mapper resolution moved to runtime:** the validation error mapper is now resolved **per request** in the consuming integrations’ problem factories. This avoids resolving scoped services from the root container during options configuration.

### Removed

* **Mapper no longer pre-bound on options:** `KnightResponseBaseOptions` no longer eagerly assigns a default mapper during options configuration. Instead, mappers are resolved at runtime: if DI provides an `IValidationErrorMapper`, that is used; otherwise, if `ValidationMapper` is set on options, that is used; otherwise, a new `DefaultValidationErrorMapper` is created.

### BREAKING CHANGES

* **Payload shaping default changed:** `IncludeFullResultPayload` is now **`false`**. If you relied on the full `Result`/`Result<T>` body being returned on success by default, explicitly set it to `true` in your integration options.
* **Mapper no longer pre-bound on options:** if you previously depended on a default mapper coming from options configuration, ensure your host registers an `IValidationErrorMapper` and that integrations resolve it per-request.

> Notes:
>
> * This package remains **.NET Standard 2.0** and is intended to be a thin abstraction shared by platform integrations. See README for details.

---

## [1.0.2] - 2025-09-01

### Changed

* Updated dependency: now requires **Knight.Response 1.1.0**.

    * Ensures compatibility with the new `Results.Validation(...)` APIs.
* No functional changes in `Knight.Response.Abstractions.Http` itself.

---

## [1.0.1] - 2025-02-28

### Fixed

* Corrected documentation to show the proper generic signature of
  **`KnightResponseBaseOptions<TH, TProblem, TValidation>`**
  (previously shown incorrectly as `<TProblem, TValidation>`).

---

## [1.0.0] - 2025-02-28

### Added

* **KnightResponseBaseOptions<TH, TProblem, TValidation>**: Base options type to configure HTTP-specific behavior (HttpContext, ProblemDetails, ValidationProblemDetails, payload shaping).
* **IValidationErrorMapper**: Abstraction for mapping `Message` collections to validation errors.
* **DefaultValidationErrorMapper**: Default implementation of `IValidationErrorMapper`.
* Support for sharing configuration and mappers between **Knight.Response.AspNetCore** and **Knight.Response.AspNetCore.Mvc** packages.

---

[1.0.0]: https://github.com/KnightBadaru/Knight.Response/releases/tag/abstractions-http-v1.0.0
[1.0.1]: https://github.com/KnightBadaru/Knight.Response/releases/tag/abstractions-http-v1.0.1
[1.0.2]: https://github.com/KnightBadaru/Knight.Response/releases/tag/abstractions-http-v1.0.2
[2.0.0-preview01]: https://github.com/KnightBadaru/Knight.Response/releases/tag/abstractions-http-v2.0.0-preview01
[2.0.0-preview02]: https://github.com/KnightBadaru/Knight.Response/releases/tag/abstractions-http-v2.0.0-preview02
[2.0.0-preview03]: https://github.com/KnightBadaru/Knight.Response/releases/tag/abstractions-http-v2.0.0-preview03
