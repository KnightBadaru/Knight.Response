# Knight Response Monorepo

This repository contains the Knight.Response family of .NET libraries, focused on providing lightweight, immutable result handling for service layers in C#. These libraries are designed to improve code clarity, maintainability, and robustness by standardising how operations return and handle outcomes.

---

## Projects

### 1. Knight.Response

A lightweight, immutable `Result`/`Result<T>` library with factories, functional extensions, and pattern matching for C# service layers.

* Immutable results for safer, predictable state handling
* Functional extensions (`Map`, `Bind`, `OnSuccess`, `OnFailure`, etc.)
* Built-in message handling (`Info`, `Warning`, `Error`)
* Fully unit tested with **100% mutation score**
* .NET Standard 2.0 compatible

**Docs:** [Knight.Response README](src/Knight.Response/README.md)

---

## Contributing

We welcome contributions! Please read the following before getting started:

* [CODE\_OF\_CONDUCT.md](CODE_OF_CONDUCT.md)
* [CONTRIBUTING.md](CONTRIBUTING.md)

---

## License

All projects in this repository are licensed under the [MIT License](LICENSE).
