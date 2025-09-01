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

## [1.0.2] - 2025-09-01

### Changed
- Updated dependency: now requires **Knight.Response 1.1.0**.
  - Ensures compatibility with the new `Results.Validation(...)` APIs.
- No functional changes in `Knight.Response.Abstractions.Http` itself.

---

## [1.0.1] - 2025-02-28

### Fixed
- Corrected documentation to show the proper generic signature of  
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