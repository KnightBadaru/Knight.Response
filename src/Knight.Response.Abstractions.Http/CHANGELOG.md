# Changelog

All notable changes to **Knight.Response.Abstractions.Http** will be documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added
- Nothing yet.

### Changed
- Nothing yet.

### Fixed
- Nothing yet.

---

## [1.0.0] - 2025-02-27

### Added

* **KnightResponseBaseOptions\<TProblem, TValidationProblem>**: Base options type to configure HTTP-specific behavior (ProblemDetails, ValidationProblemDetails, payload shaping).
* **IValidationErrorMapper**: Abstraction for mapping `Message` collections to validation errors.
* **DefaultValidationErrorMapper**: Default implementation of `IValidationErrorMapper`.
* Support for sharing configuration and mappers between **Knight.Response.AspNetCore** and **Knight.Response.AspNetCore.Mvc** packages.

---

[1.0.0]: https://github.com/KnightBadaru/Knight.Response/releases/tag/abstractions-http-v1.0.0
