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

## [2.0.0-preview04] - 2025-09-25

### Added

* New predicate helpers:

    * `IsUnsuccessfulOrNull<T>()` – true if the result is unsuccessful or its value is `null`.
    * `ValueIsNull<T>()` – explicit check for `Result<T>.Value == null`.
* New `ResultCodes.NoContent` well-known code for transport-agnostic 204 scenarios.
* Factory overloads:

    * `Results.NoContent()` and typed/untyped variants with optional messages and codes.
* Code helpers:

    * `WithCode(ResultCode?)` – assign or override domain codes.
    * `WithoutCode()` – clear any assigned code.
    * `HasCode(ResultCode)` and `HasCode(string)` – check equality by instance or string value (case-insensitive).

### Changed

* Extended `ResultExtensions` with richer match semantics:

    * `MatchValue` overloads for mapping to new results.
    * `Match` overloads for typed/untyped results, with/without messages.
* Documentation refinements across extensions for consistency.

### Fixed

* Improved unit test coverage for new extensions (`IsUnsuccessfulOrNull`, `Map`, `Match`).

---

## [2.0.0-preview03] - 2025-09-22

### Added

* Branching extensions

    * Match overloads (typed & untyped, with and without messages, returning value or void).
    * MatchValue overloads: convenience wrappers for producing new `Result/Result<T>` instances directly.

* Predicates

    * `IsUnsuccessful()` – shorthand for `Status != Completed`.
    * `ValueIsNull<T>()` – true when `Result<T>.Value` is `null`.

* Detail helpers

    * WithDetail(key, value) – attaches metadata to the last message, case-insensitive, immutable.

### Changed

* Removed redundant `OnFailure<T>()` extension (duplicated logic of `OnFailure`).
* Consolidated extension classes into a single `ResultExtensions` set for clarity.
* Improved XML documentation across extensions for NuGet API docs.

### Fixed

* Defensive handling of metadata copies in WithDetail.
* Validation metadata extraction (`TryGetValidationResults`) now covers both `ValidationResult` and `ValidationResults` keys consistently.

---

## [2.0.0-preview02] - 2025-09-22

### Added

* **IsUnsuccessful()** extension: shorthand for `!IsSuccess()`. Returns true for Failed, Error, or Cancelled results.

### Changed

* Removed redundant generic overload `TryGetValidationResults<T>()`. Consumers should use the non-generic `TryGetValidationResults()` for both `Result` and `Result<T>`.

### Notes

* Focused preview release to polish ergonomic extensions and simplify API surface.
* `IsValidationError()` deliberately not added; consumers can check via `TryGetValidationResults()` or custom codes.

---

## [2.0.0-preview01] - 2025-09-22

### Added

* **ResultCode support**: optional domain/system reason in addition to `Status`.
* **Built-in ResultCodes**: `ValidationFailed`, `NotFound`, `AlreadyExists`, `Created`, `Updated`, etc.
* **Extensions**:

    * `WithCode(ResultCode)` — attach/replace a result code.
    * `WithDetail(key, value)` — add structured metadata to the last message.
    * `TryGetValidationResults(out IReadOnlyList<ValidationResult>)` and `GetValidationResults()` — extract validation details from metadata.
* **Well-known extension predicates**: `IsSuccess()`, `IsFailure()`, `IsError()`, `IsCancelled()` replacing property-based `IsSuccess`.

### Changed

* `Result` and `Result<T>` constructors extended to accept optional `ResultCode`.
* `IsSuccess` property removed — now an extension method. Reduces API surface in serialized outputs.
* Extensions reorganized: advanced helpers (Ensure, Tap, Recover, WithMessages) merged into a single `ResultExtensions` module.

> **Notes:** This is a **preview release** of `Knight.Response` only. `.Abstractions.Http`, `.AspNetCore`, and `.Mvc` remain on stable 1.x line for now.
> Breaking change: `IsSuccess` is no longer a property; use `result.IsSuccess()` extension instead.

---

## [1.1.0] - 2025-09-01

### Added

* **ValidationResult support**: `Results.Validation(...)` and `Results.Validation<T>(...)` to convert `IEnumerable<ValidationResult>` into error results.
* **Enricher overloads**: `Results.Validation(..., Func<Message, string?, Message> enrich)` to attach metadata (e.g., `field`) without altering message text.
* **Mapping helpers**: internal `ValidationMappingExtensions` with:

    * `ToMessagesPrefixed(...)` — "Field: message" formatting.
    * `ToMessagesWithMetadata(...)` — raw text + field passed to enricher.
* Unit tests covering all branches; Stryker mutation score **100%** for `Knight.Response`.

### Changed

* Normalized validation messages: trim content; fallback to `"Validation error."` when null/empty/whitespace.
* Minor internal refactors for mutation-test robustness (e.g., centralized normalization).

### Fixed

* Ensured `Result` constructors normalize `null` messages to empty list (guarded by tests).

> **Notes:** No breaking changes. `Results.Validation(...)` maps to `Error` internally; existing APIs unchanged.

---

## [1.0.2] - 2025-08-28

### Changed

* Refactored unit tests to consistently use **Shouldly** assertions (instead of `Assert.*`).
* Improved **README** documentation to clarify the default value behavior of `Result<T>.Value`:

    * Value types (`int`, `bool`, etc.) return their CLR default (`0`, `false`, etc.) unless explicitly set.
    * Reference types and nullable value types return `null` when not set.
* General documentation refinements for consistency.

---

## [1.0.1] - 2025-08-15

### Changed

* Updated README badges for NuGet, build, and mutation coverage using Stryker Dashboard.
* Configured Stryker Dashboard integration and added GitHub Actions workflow for mutation testing.
* Minor refinements to CI/CD workflows.

---

## [1.0.0] - 2025-08-15

### Added

* Initial stable release of Knight.Response
* Immutable `Result` / `Result<T>`
* Factory methods: `Success`, `Failure`, `Error`, `Cancel`, `NotFound`, `FromCondition`, `Aggregate`, `Error(Exception)`
* Functional extensions: `OnSuccess`, `OnFailure`, `Map`, `Bind`, `Ensure`, `Tap`, `Recover`, `WithMessages` / `WithMessage`
* Deconstructors for pattern matching
* README, LICENSE (MIT), CI for NuGet publish

---

[1.0.0]: https://github.com/KnightBadaru/Knight.Response/releases/tag/response-v1.0.0
[1.0.1]: https://github.com/KnightBadaru/Knight.Response/releases/tag/response-v1.0.1
[1.0.2]: https://github.com/KnightBadaru/Knight.Response/releases/tag/response-v1.0.2
[1.1.0]: https://github.com/KnightBadaru/Knight.Response/releases/tag/response-v1.1.0
[2.0.0-preview01]: https://github.com/KnightBadaru/Knight.Response/releases/tag/response-v2.0.0-preview01
[2.0.0-preview02]: https://github.com/KnightBadaru/Knight.Response/releases/tag/response-v2.0.0-preview02
[2.0.0-preview03]: https://github.com/KnightBadaru/Knight.Response/releases/tag/response-v2.0.0-preview03
[2.0.0-preview04]: https://github.com/KnightBadaru/Knight.Response/releases/tag/response-v2.0.0-preview04