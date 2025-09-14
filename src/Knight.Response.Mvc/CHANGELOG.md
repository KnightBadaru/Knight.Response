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

## [0.1.0] - 2025-09-14
### Added
- Initial release of `Knight.Response.Mvc`
- `ResultExtensions` for converting `Result` / `Result<T>` into `IActionResult`
- `ServiceCollectionExtensions` for service registration
- `ApiResults` factory for consistent HTTP responses
- `ProblemFactory` for building RFC 7807-style error objects
- `CompatProblemDetails` and `CompatValidationProblemDetails` for MVC compatibility
- `KnightResponseOptions` for customization

---

[0.1.0]: https://github.com/KnightBadaru/Knight.Response/releases/tag/mvc-v0.1.0