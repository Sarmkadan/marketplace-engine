# Contributing to Marketplace Engine

Thanks for helping out! This guide covers the basics for working on the `MarketplaceEngine` solution.

## Prerequisites

- .NET SDK 10.0.100 or newer. `global.json` pins `10.0.100` with `rollForward: latestMinor`; the projects target `net10.0`.
- Git.
- Docker (optional, only for container testing).

## Build

```bash
dotnet restore
dotnet build                  # Debug
dotnet build -c Release       # Release, the configuration CI uses
```

Or use `make build`. Build warnings are errors (`TreatWarningsAsErrors` in `Directory.Build.props`), so a clean build is required.

## Run tests

```bash
dotnet test --verbosity normal
dotnet test --filter "FullyQualifiedName~ListingServiceTests"   # single class
```

Tests live in `tests/marketplace-engine.Tests` (xUnit). Tests that touch the shared `MarketplaceDbContext` must be tagged `[Collection("Database")]`. Or use `make test`.

## Coding conventions

- Follow the rules in `.editorconfig`. Run `dotnet format` (or `make format`) before committing.
- Nullable reference types and implicit usings are enabled; add `#nullable enable` to new files.
- Use file-scoped namespaces (`namespace MarketplaceEngine.Services;`).
- Methods that return `Task` end in `Async`.
- Keep the existing `// Author:` header in every `.cs` file and add it to new ones.
- Add `/// <summary>` XML docs to public types and members.
- Do not introduce warnings. Fix them rather than suppressing them.

## Branching and pull requests

1. Branch from `main`: `git switch -c feat/<short-name>`.
2. Keep each PR to one feature or fix.
3. Use conventional commit prefixes: `feat:`, `fix:`, `docs:`, `chore:`, `build:`, and so on.
4. Make sure the local build and tests pass before you push.
5. Open a PR against `main`. The `CI` workflow (`.github/workflows/ci.yml`) must be green.
6. Get one reviewer's approval before merging.

Fill in the PR template in `.github/PULL_REQUEST_TEMPLATE.md`.

## Reporting bugs

Open an issue on GitHub using one of the templates:

- [Bug report](.github/ISSUE_TEMPLATE/bug_report.md)
- [Feature request](.github/ISSUE_TEMPLATE/feature_request.md)

For bugs, include the steps to reproduce, the expected and actual behaviour, and your .NET SDK version and OS.

## License

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE).
