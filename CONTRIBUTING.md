# Contributing to Marketplace Engine

Thank you for your interest in contributing!

## Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Docker (optional, for container testing)

## Building Locally

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Or use the Makefile
make build
```

## Running Tests

```bash
# Run all tests
dotnet test --verbosity normal

# Run with detailed output and TRX report
dotnet test --verbosity normal --logger "trx;LogFileName=test-results.trx"

# Or via Makefile
make test
```

## Pull Request Guidelines

1. Fork the repository and create a feature branch from `main`
2. Keep changes focused — one feature or fix per PR
3. Ensure all tests pass before submitting
4. Add or update tests for any changed behaviour
5. Update documentation if you change public APIs or configuration
6. Write a clear PR description explaining what changed and why

## Code Style

- Follow the `.editorconfig` settings already in the repository
- Use `var` where the type is apparent
- File-scoped namespaces (`namespace Foo;`)
- XML documentation on all public types and members
- Private fields prefixed with `_` in camelCase
- Keep all existing file headers — do not remove them

## Reporting Issues

Use GitHub Issues to report bugs or request features. For bugs, include:
- Steps to reproduce
- Expected vs actual behaviour
- .NET version and OS

## License

By contributing you agree that your contributions are licensed under the [MIT License](LICENSE).
