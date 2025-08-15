# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog and this project adheres to Semantic Versioning.

## [1.0.1] - 2025-08-15

### Changed
- Updated README badges for NuGet, build, and mutation coverage using Stryker Dashboard. 
- Configured Stryker Dashboard integration and added GitHub Actions workflow for mutation testing.
- Minor refinements to CI/CD workflows.

## [1.0.0] - 2025-08-15

### Added
- Initial stable release of Knight.Response
- Immutable Result/Result
- Factory methods: Success, Failure, Error, Cancel, NotFound, FromCondition, Aggregate, Error(Exception)
- Functional extensions: OnSuccess, OnFailure, Map, Bind, Ensure, Tap, Recover, WithMessages/WithMessage
- Deconstructors for pattern matching
- README, LICENSE (MIT), CI for NuGet publish