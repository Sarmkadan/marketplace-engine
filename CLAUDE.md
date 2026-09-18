# CLAUDE.md

Marketplace Engine: an ASP.NET Core (net10.0) marketplace backend (listings, users, categories, search, messaging, payments, reviews, moderation, recommendations) with an entirely in-memory data store - no database, no external infra.

## Build

```bash
dotnet restore
dotnet build                          # Debug
dotnet build -c Release               # Release (what CI runs)
dotnet run --project src/MarketplaceEngine   # http://localhost:5000, Swagger at / (Development only)
make build | make run | make watch    # Makefile wrappers for the above
docker build -t marketplace-engine:latest . ; docker-compose up -d
```

Requires .NET SDK 10.0.100+ (`global.json`, rollForward latestMinor).

## Test

```bash
dotnet test --verbosity normal                                   # all tests (xUnit)
dotnet test --filter "FullyQualifiedName~ListingServiceTests"    # single class
dotnet test -c Release --no-build --logger "trx;LogFileName=test-results.trx"   # CI form
make test
```

Test project: `tests/marketplace-engine.Tests` (xUnit 2.9, FluentAssertions 7, Moq 4.20). Tests that touch the shared `MarketplaceDbContext` singleton must be tagged `[Collection("Database")]` (see `DatabaseCollection.cs`) so they do not run in parallel.

Benchmarks: `dotnet run -c Release --project MarketplaceEngine.Benchmarks` (BenchmarkDotNet).

## Lint / Format

```bash
dotnet format          # or: make format
```

Style is defined in `.editorconfig` (naming rules at severity "suggestion"). No analyzers beyond defaults; `TreatWarningsAsErrors=false` in `Directory.Build.props`. Nullable and ImplicitUsings are enabled solution-wide.

## Key directories and entry points

- `MarketplaceEngine.sln` - solution: main project, tests, benchmarks.
- `src/MarketplaceEngine/Program.cs` - composition root: DI, middleware pipeline, endpoint mapping, `BackgroundJobQueue.Start()`.
- `src/MarketplaceEngine/Configuration/` - `DependencyInjection.cs` (`AddMarketplaceServices`, `MapMarketplaceEndpoints`), plus `FullTextSearchExtensions`, `RecommendationExtensions`, `HealthCheckExtensions`.
- `Controllers/` - attribute-routed controllers under `api/v1/*` (recommendations on `api/v2`). Minimal-API routes must not duplicate a controller path (causes `AmbiguousMatchException`).
- `Services/` - business logic; `Repositories/` - `IRepository<T>` + per-entity repositories; services never touch the context directly.
- `Data/MarketplaceDbContext.cs` - hand-rolled singleton of `List<T>` collections with seed data, obtained via `GetInstance()` (not DI). Restarting the process resets all data.
- `Domain/` - `Models/` (entities), `ValueObjects/` (Money, Location, Rating), `Enums/`, `Services/`.
- `DTOs/`, `Exceptions/` (`MarketplaceException` hierarchy), `Middleware/` (ErrorHandling -> RequestLogging -> RateLimiting, in that order), `Infrastructure/` (Background, Caching, Events, Integration, Security, Configuration, Formatters), `Recommendations/`, `Constants/`, `Utilities/`.
- `docs/ARCHITECTURE.md` - authoritative description of what is actually implemented; `docs/*.md` - per-type docs. `examples/` - usage samples and `curl-examples.sh`.
- `.github/workflows/` - CI (`ci.yml`, `build.yml`): restore, build Release, test.

## Conventions

- Every service/repository is registered `AddSingleton` (consistent with the in-memory store). Keep it that way; do not introduce scoped dependencies into singletons.
- Every `.cs` file starts with the author header block (`// Author: Vladyslav Zaiets | https://sarmkadan.com`) - keep existing headers, add it to new files.
- File-scoped namespaces (`namespace MarketplaceEngine.Services;`); `#nullable enable` at top of files; `sealed` classes for services.
- XML doc comments (`/// <summary>`) on all public types and members (`GenerateDocumentationFile` is on).
- Naming: PascalCase types/members, `I`-prefixed interfaces, `_camelCase` private fields, `var` when the type is obvious. Constructor dependencies null-checked with `?? throw new ArgumentNullException(nameof(x))`.
- Async methods end in `Async`. Domain events are `*Event` records published through `EventBus`; handlers implement `IEventHandler<TEvent>`.
- Test classes: `<Type>Tests.cs`, extra suites split as `<Type>TestsValidation.cs` / `<Type>TestsExtensions.cs`; field `_sut` for the system under test; Moq for repositories.
- Errors: throw from the `MarketplaceException` hierarchy; `ErrorHandlingMiddleware` converts them to the JSON error envelope.
- Commits: conventional-commit style (`docs:`, `chore:`, `feat:`, `fix:`). No `Co-Authored-By` trailers.
- Do not commit `.aider*`, `bin/`, `obj/`, `TestResults/` (gitignored).
