# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)  
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added
- Nothing yet.

### Changed
- Nothing yet.

### Fixed
- Nothing yet.

---

## [1.1.0] - 2025-09-01

### Added
- **ValidationResult support**: `Results.Validation(...)` and `Results.Validation<T>(...)` to convert `IEnumerable<ValidationResult>` into error results.
- **Enricher overloads**: `Results.Validation(..., Func<Message, string?, Message> enrich)` to attach metadata (e.g., `field`) without altering message text.
- **Mapping helpers**: internal `ValidationMappingExtensions` with:
    - `ToMessagesPrefixed(...)` — `"Field: message"` formatting.
    - `ToMessagesWithMetadata(...)` — raw text + field passed to enricher.
- Unit tests covering all branches; Stryker mutation score **100%** for `Knight.Response`.

### Changed
- Normalized validation messages: trim content; fallback to `"Validation error."` when null/empty/whitespace.
- Minor internal refactors for mutation-test robustness (e.g., centralized normalization).

### Fixed
- Ensured `Result` constructors normalize `null` messages to empty list (guarded by tests).

> **Notes:** No breaking changes. `Results.Validation(...)` maps to `Error` internally; existing APIs unchanged.  

---
## [1.0.2] - 2025-08-28

### Changed
- Refactored unit tests to consistently use **Shouldly** assertions (instead of `Assert.*`).
- Improved **README** documentation to clarify the default value behavior of `Result<T>.Value`:
    - Value types (`int`, `bool`, etc.) return their CLR default (`0`, `false`, etc.) unless explicitly set.
    - Reference types and nullable value types return `null` when not set.
- General documentation refinements for consistency.

---

## [1.0.1] - 2025-08-15

### Changed
- Updated README badges for NuGet, build, and mutation coverage using Stryker Dashboard.
- Configured Stryker Dashboard integration and added GitHub Actions workflow for mutation testing.
- Minor refinements to CI/CD workflows.

---

## [1.0.0] - 2025-08-15

### Added
- Initial stable release of Knight.Response
- Immutable `Result` / `Result<T>`
- Factory methods: `Success`, `Failure`, `Error`, `Cancel`, `NotFound`, `FromCondition`, `Aggregate`, `Error(Exception)`
- Functional extensions: `OnSuccess`, `OnFailure`, `Map`, `Bind`, `Ensure`, `Tap`, `Recover`, `WithMessages` / `WithMessage`
- Deconstructors for pattern matching
- README, LICENSE (MIT), CI for NuGet publish

---

[1.0.0]: https://github.com/KnightBadaru/Knight.Response/releases/tag/response-v1.0.0
[1.0.1]: https://github.com/KnightBadaru/Knight.Response/releases/tag/response-v1.0.1
[1.0.2]: https://github.com/KnightBadaru/Knight.Response/releases/tag/response-v1.0.2
[1.1.0]: https://github.com/KnightBadaru/Knight.Response/releases/tag/response-v1.1.0