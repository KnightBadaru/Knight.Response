# Changelog
All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog and this project adheres to Semantic Versioning.

## [1.0.0] - 2025-08-15
### Added
- Initial stable release of Knight.Response
- Immutable Result/Result<T>
- Factory methods: Success, Failure, Error, Cancel, NotFound, FromCondition, Aggregate, Error(Exception)
- Functional extensions: OnSuccess, OnFailure, Map, Bind, Ensure, Tap, Recover, WithMessages/WithMessage
- Deconstructors for pattern matching
- README, LICENSE (MIT), CI for NuGet publish