# Architecture

This document describes the architecture of Marketplace Engine as it exists in the
code today. Where earlier documentation described planned features (relational
database, JWT/OAuth, Redis) as if they existed, this document sticks to what is
actually implemented.

## Overview

Marketplace Engine is a single ASP.NET Core (net10.0) web application
(`src/MarketplaceEngine`) that implements a marketplace backend: listings, users,
categories, messaging, payments, reviews, moderation, full-text search,
recommendations and a seller dashboard. There is one companion test project
(`tests/marketplace-engine.Tests`, xUnit) and a benchmark project
(`MarketplaceEngine.Benchmarks`).

Key characteristics:

- **All state is in-memory.** There is no real database. `Data/MarketplaceDbContext`
  is a hand-rolled singleton holding `List<T>` collections for each entity, seeded
  with default data in its constructor. Repositories obtain it via
  `MarketplaceDbContext.GetInstance()` - not via DI.
- **Everything is a singleton.** Every repository and service is registered with
  `AddSingleton` in `Configuration/DependencyInjection.cs`. This is consistent
  (no captive-dependency problem: singletons only depend on singletons) and is a
  deliberate consequence of the in-memory storage model - the repositories *are*
  the data store, so they must be process-wide.
- **Two API surfaces.** Attribute-routed MVC controllers (`Controllers/`) provide
  the full CRUD API; a smaller set of minimal-API endpoints
  (`MapMarketplaceEndpoints`, plus search and recommendation endpoint groups in
  `Configuration/`) provides read-only routes. See "Routing" below.

## Project layout

```
src/MarketplaceEngine/
├── Program.cs                  # Composition root: DI, middleware pipeline, endpoint mapping
├── Configuration/              # DI registration + minimal-API endpoint mapping
│   ├── DependencyInjection.cs  #   AddMarketplaceServices, MapMarketplaceEndpoints
│   ├── FullTextSearchExtensions.cs
│   ├── RecommendationExtensions.cs
│   └── HealthCheckExtensions.cs
├── Controllers/                # Attribute-routed API controllers (api/v1/*, recommendations on api/v2)
├── Services/                   # Business logic (ListingService, SearchService, PaymentService, ...)
├── Repositories/               # IRepository<T> + entity repositories over MarketplaceDbContext
├── Data/                       # MarketplaceDbContext: singleton in-memory store with seed data
├── Domain/                     # Entities (Models/), value objects (Money, Location, Rating), enums
├── DTOs/                       # Request/response transfer objects
├── Middleware/                 # RequestLogging, RateLimiting, ErrorHandling
├── Infrastructure/
│   ├── Background/             # BackgroundJobQueue (ConcurrentQueue + worker task)
│   ├── Caching/                # CacheService (in-memory)
│   ├── Events/                 # EventBus (in-process pub/sub), domain events, handlers
│   ├── Integration/            # HttpClientService, DropshipProviderClient, WebhookService
│   ├── Security/               # TokenService (opaque API tokens), PermissionService, ApiKeyValidator
│   ├── Configuration/          # MarketplaceConfiguration POCO, JSON options
│   └── Formatters/             # Output formatters (FormatterFactory)
├── Exceptions/                 # MarketplaceException hierarchy
├── Recommendations/            # CollaborativeFilteringEngine, UserActivityTracker, options
├── Constants/ Utilities/       # Constants and helpers (Validation/Mapping utilities)
```

## Composition root (`Program.cs`)

Startup order:

1. `AddMarketplaceServices()` - repositories, domain services, cache, event bus +
   handlers, background queue, integration and security services.
2. `AddFullTextSearch()` and `AddRecommendationEngine(configuration)` - the
   recommendation engine binds `RecommendationOptions` from the
   `Marketplace:Recommendations` configuration section (defaults when absent).
3. Swagger (Development only, served at the site root), controllers, permissive
   CORS policy (`AllowAll`).
4. Middleware pipeline (order matters):
   `ErrorHandlingMiddleware` → `RequestLoggingMiddleware` → `RateLimitingMiddleware`
   → HTTPS redirection → CORS → routing. The error handler is outermost so that
   exceptions thrown anywhere in the pipeline (including other middleware) are
   converted to the standard JSON error envelope.
5. Endpoint mapping: `MapControllers()`, `MapMarketplaceEndpoints()`,
   `MapRecommendationEndpoints()`, `MapFullTextSearchEndpoints()`.
6. Domain event handlers registered in DI are subscribed to the `EventBus`.
7. `BackgroundJobQueue.Start()` before `app.Run()`, with `StopAsync()` in a
   `finally` block for graceful shutdown.

## Routing

Two route families coexist:

- **Controllers** (`api/v1/...`): listings, users, categories, messages, payments,
  reviews, moderation, seller dashboard, health. Recommendations live on
  `api/v2/recommendations`.
- **Minimal APIs**: `MapMarketplaceEndpoints` maps a small read-only subset
  (health, listings, users, categories); `MapFullTextSearchEndpoints` maps
  `GET /api/v1/search/full-text` and `/api/v1/search/suggestions`;
  `MapRecommendationEndpoints` maps the recommendation routes.

Because both families target `api/v1`, minimal-API routes must not duplicate a
path already served by a controller - ASP.NET Core endpoint routing throws
`AmbiguousMatchException` (HTTP 500) at request time when two endpoints match a
request equally. The overlapping minimal-API routes were removed for this reason;
only paths without a controller equivalent (e.g. `/api/v1/users/sellers/top`,
the search group) remain in the minimal-API surface.

## Data flow

Typical request (create listing):

```
HTTP POST /api/v1/listings
  → RateLimiting / logging / error-handling middleware
  → ListingsController
  → ListingService (validation, business rules)
  → IListingRepository (ListingRepository)
  → MarketplaceDbContext.GetInstance().Listings   (List<Listing>, in-memory)
  → EventBus.PublishAsync(ListingCreatedEvent)    (in-process handlers)
  → DTO mapped and returned
```

There is no persistence: restarting the process resets all data to the seed set.

## Key components

### Storage: `MarketplaceDbContext`

A plain class (not EF Core) exposing `List<T>` properties for Users, Categories,
Listings, Messages, ModerationReports, Payments and Reviews. Access is through a
double-checked-lock singleton (`GetInstance()`). Repositories synchronize their
own operations; `tests/ListingRepositoryConcurrencyTests.cs` exercises concurrent
access.

Trade-off: this makes the whole system trivially runnable (no infra dependencies)
and fast, at the cost of durability and horizontal scalability. The repository
interfaces (`IRepository<T>` and per-entity interfaces) are the intended seam for
introducing a real database later - services never touch the context directly.

### Events: `EventBus`

A simple in-process pub/sub keyed by event type (`Dictionary<Type, List<Delegate>>`).
Services publish domain events (`ListingCreatedEvent`, `MessageSentEvent`,
`UserActivityRecordedEvent`, ...); handlers implement `IEventHandler<TEvent>` and
are registered in DI, then subscribed to the bus at startup in `Program.cs`.
`PublishAsync` awaits all handlers (`Task.WhenAll`) and rethrows on failure - so a
failing handler fails the publishing operation. Not a durable queue; replace with
a message broker for at-least-once semantics.

### Background jobs: `BackgroundJobQueue`

A `ConcurrentQueue<IBackgroundJob>` drained by a single long-running task started
from `Program.cs`. It is a plain singleton, not an `IHostedService`, so start/stop
is wired manually in `Program.cs`. Jobs are best-effort and lost on restart.

### Rate limiting: `RateLimitingMiddleware`

Fixed-window limiter, 100 requests/minute per client IP, backed by a static
`ConcurrentDictionary`. The client IP is taken from `X-Forwarded-For` when present
(first hop), falling back to the connection address. **Limitation:** because
`X-Forwarded-For` is client-controlled unless a trusted reverse proxy strips it,
the limit is trivially bypassable when the app is exposed directly. `/api/v1/health`
is exempt. An in-constructor fire-and-forget task evicts stale buckets every 5 min.

### Error handling: `ErrorHandlingMiddleware`

Maps the `MarketplaceException` hierarchy (`ValidationException`,
`ResourceNotFoundException`, `UnauthorizedException`, ...) to HTTP status codes and
a consistent JSON envelope (`code`, `message`, ...). Controllers therefore mostly
avoid try/catch for domain errors.

### Security: `TokenService`, `PermissionService`, `ApiKeyValidator`

`TokenService` issues opaque random API tokens (32 bytes, 30-day expiry) with an
in-memory revocation list - it is not JWT and there is no authentication middleware
enforcing tokens on requests; authorization checks are done by services via
`PermissionService`. Secrets for the token service, webhook signing and the
dropship provider are read from configuration (`Marketplace:Security:TokenSecret`,
`Marketplace:Webhooks:Secret`, `Marketplace:Dropship:*`) with development-only
placeholder fallbacks; set real values via environment variables or user-secrets
in any non-toy deployment.

### Integration: `DropshipProviderClient`, `WebhookService`

`IListingProvider`/`DropshipProviderClient` wraps an external listing feed over
`HttpClientService` (typed `HttpClient` via `AddHttpClient`).
`ExternalListingSyncService` pulls provider listings into the local store.
`WebhookService` dispatches signed webhook events to registered handlers.

### Search and recommendations

- `SearchService` and `FullTextSearchService` filter/score in memory over the
  listing repository (relevance scoring, facets, suggestions). Complexity is O(n)
  in listing count - acceptable for the in-memory scale this project targets.
- `Recommendations/` implements collaborative filtering
  (`CollaborativeFilteringEngine`) fed by `UserActivityTracker` signals;
  `RecommendationService.TrackUserActivityAsync` records a signal and publishes
  `UserActivityRecordedEvent`. Options come from `Marketplace:Recommendations`.

## Design decisions and trade-offs

| Decision | Rationale | Trade-off |
|---|---|---|
| In-memory singleton store instead of a DB | Zero-dependency demo/reference project; instant startup; simple tests | No durability, single-process only |
| Singleton lifetime for everything | Repositories hold the data, so they must be process-wide; services are stateless over them | A future DbContext-based repository would need scoped lifetimes and a DI rework |
| Controllers + minimal APIs side by side | Controllers carry the full API; minimal APIs demonstrate the lightweight style for simple read paths | Route ownership must be watched to avoid ambiguous matches |
| Custom `EventBus` instead of MediatR/broker | No external dependency; enough for in-process side effects | Synchronous fan-out, no durability/retries |
| Exception-based error contract | Centralizes HTTP mapping in one middleware | Exceptions used for expected control flow (e.g. not-found) |

## Extension points

- **Persistence:** implement `IListingRepository`/`IUserRepository`/... against a
  real database and change the registrations in
  `Configuration/DependencyInjection.cs`; nothing above the repository layer
  depends on `MarketplaceDbContext`.
- **Events:** implement `IEventHandler<TEvent>`, register it in DI and subscribe it
  at startup (see `RegisterEventHandlers` / the startup subscription block in
  `Program.cs`).
- **External providers:** implement `IListingProvider` for other listing feeds.
- **Background work:** implement `IBackgroundJob` and enqueue it on
  `BackgroundJobQueue`.
- **Output formats:** add a formatter and extend `FormatterFactory`.

## Known limitations

- No real authentication middleware; tokens exist but are not enforced per-request.
- All data is lost on restart; seed data is re-created by `MarketplaceDbContext`.
- Rate limiting trusts `X-Forwarded-For` and is per-process (not distributed).
- CORS policy is `AllowAll` - fine for a demo, not for production.
- `EventBus` handler failures propagate to the publisher.
- Search is linear scan; no indexes.

---

Note: `docs/architecture.md` (lowercase) is an older document that mixes the
implemented Phase 1 with planned Phase 2/3 features (SQL schema, JWT, Redis,
Elasticsearch). Treat this file as the source of truth for current behavior.
