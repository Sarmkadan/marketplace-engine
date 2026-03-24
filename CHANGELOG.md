// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Changelog

All notable changes to Marketplace Engine are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-09-30

### Added
- Comprehensive documentation suite (getting-started, architecture, api-reference, deployment, faq)
- Example programs demonstrating all major features in `examples/`
- Docker support with Dockerfile and docker-compose configuration
- GitHub Actions CI/CD workflow for automated testing and building
- CodeQL security scanning workflow
- Dependabot configuration for automated dependency updates
- NuGet packaging configuration with full metadata
- Makefile with common development targets (build, run, test, clean)
- Health check endpoint at `/api/v1/health`

### Changed
- Version bumped to 1.0.0 — all core functionality is stable
- Improved API response format with consistent `timestamp` and `requestId` fields
- Enhanced Swagger/OpenAPI documentation with detailed descriptions and examples

### Fixed
- Concurrent message handling edge case in MessagingService
- Pagination boundary calculation in listing search results
- Category listing count accuracy when listings change status

---

## [0.9.0] - 2025-08-14

### Added
- Webhook notification service (`WebhookService`) for external system integration
- External listing provider abstraction (`ExternalListingProvider`) for feed ingestion
- HTTP client service with retry policy and timeout configuration
- Background job queue (`BackgroundJobQueue`) for deferred work
- Output formatter for consistent serialization across all endpoints

### Changed
- Event bus now supports typed event handlers via `IEventHandler<T>`
- Improved error response structure with machine-readable `errorCode` field

### Fixed
- Memory leak in `CacheService` when cache entries expired under load
- Event handler registration order in DI container

---

## [0.8.0] - 2025-07-02

### Added
- Faceted search DTO (`FacetedSearchDto`) with aggregated filter counts
- Full-text search extensions (`FullTextSearchExtensions`) for in-memory relevance scoring
- `FullTextSearchService` with stemming and stop-word filtering
- Location-based search with distance calculation
- Trending listings algorithm based on view count and recency
- Bayesian rating system in `Rating` value object
- `TokenService` for generating and validating access tokens
- `PermissionService` for fine-grained role-based access checks

### Changed
- `SearchService` refactored to delegate FTS to `FullTextSearchService`
- `UserService.GetTopSellersAsync` now uses Bayesian average for fairer ranking
- API rate limiting middleware now returns `Retry-After` header

---

## [0.7.0] - 2025-06-10

### Added
- Rate limiting middleware with configurable requests-per-minute and burst size
- Request logging middleware with structured log output and correlation IDs
- Error handling middleware with global exception catch and semantic error codes
- Custom exception types: `ValidationException`, `ResourceNotFoundException`, `DuplicateResourceException`, `UnauthorizedException`
- JSON serialization options (`JsonOptions`) with camelCase and enum-as-string settings
- `MarketplaceConfiguration` strongly-typed settings class bound from `appsettings.json`

### Changed
- All controllers now return `ApiResponse<T>` wrapper instead of raw objects
- Validation moved to controller boundary; services trust validated inputs

### Fixed
- Listing update endpoint incorrectly accepting partial `null` fields
- Message ordering in conversation threads (was ascending, now descending by timestamp)

---

## [0.6.0] - 2025-05-19

### Added
- Event bus (`EventBus`) for in-process publish/subscribe between services
- Domain events (`ListingCreatedEvent`, `MessageSentEvent`, `UserRegisteredEvent`, etc.)
- Event handlers (`EventHandlers`) wired up in DI
- In-memory cache service (`CacheService`) with TTL and key-based invalidation
- `AppConstants` class centralizing magic strings and limit values

### Changed
- `ListingService` now publishes `ListingCreatedEvent` after successful creation
- `MessagingService` now publishes `MessageSentEvent` to allow notification hooks
- Dependency injection setup extracted to `DependencyInjection.cs` extension method

---

## [0.5.0] - 2025-04-28

### Added
- `ModerationService` for creating, reviewing, and resolving reports
- `ModerationController` exposing `/api/v1/moderation` endpoints
- `ModerationReport` domain model with priority and status tracking
- Report priority levels: Low, Medium, High, Critical
- Moderation workflow: Pending → In Review → Approved / Rejected
- Report assignment to specific moderators
- Complete audit trail of moderation actions

### Changed
- `ListingService` now checks moderation status before serving flagged listings
- `UserService` exposes `SuspendUserAsync` and `BanUserAsync` for moderation use cases

---

## [0.4.0] - 2025-04-07

### Added
- `MessagingService` for sending and retrieving messages
- `MessagesController` exposing `/api/v1/messages` endpoints
- `Message` domain model with conversation threading
- Conversation listing with unread count and last-message preview
- Listing-specific message threads linking buyers to sellers
- `IMessageRepository` and `MessageRepository` implementations

### Changed
- `UserDto` extended with `UnreadMessageCount` field
- Pagination utility applied to conversation listing queries

---

## [0.3.0] - 2025-03-20

### Added
- `CategoryService` for managing hierarchical categories
- `CategoriesController` exposing `/api/v1/categories` endpoints
- `Category` domain model with parent-child relationships and listing counts
- `SearchService` with full-text search, tag filtering, and advanced filter support
- `SearchFilters` record supporting price range, location, status, and category
- Paginated search results with `TotalCount`, `TotalPages`, and `CurrentPage`
- `PaginationUtility` for consistent page/offset calculations across services

### Changed
- `ListingService.GetListingsAsync` uses `SearchService` for filter support
- Repository interfaces extended with `FindAsync(predicate)` for flexible queries

---

## [0.2.0] - 2025-03-03

### Added
- `UserService` for registration, profile management, and role assignment
- `UsersController` exposing `/api/v1/users` endpoints
- `User` domain model with roles, ratings, avatar, bio, and account status
- Role-based access control with five roles: User, Administrator, Moderator, Support, PremiumSeller
- `ListingService` with full listing lifecycle: create, update, delete, archive, activate
- `ListingsController` exposing `/api/v1/listings` endpoints
- `Listing` domain model with tags, images, location, featured flag, and status tracking
- Bulk archive and bulk activate operations on listing collections
- `Money` value object with currency validation
- `Location` value object with city, region, country, and coordinates
- `IListingRepository`, `ListingRepository`, `IUserRepository`, `UserRepository`

### Changed
- `IRepository<T>` generic interface extended with `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`

---

## [0.1.0] - 2025-02-15

### Added
- Initial project structure: solution file, main project, and test project
- `MarketplaceDbContext` skeleton with Entity Framework Core placeholder
- Generic `IRepository<T>` interface
- Domain model stubs: `User`, `Listing`, `Message`, `Category`, `ModerationReport`
- Value object stubs: `Money`, `Location`, `Rating`
- Enum types: `ListingStatus`, `ModerationStatus`, `UserRole`
- `Program.cs` with minimal ASP.NET Core host and Swagger wired up
- `ApiResponse<T>` DTO wrapper for uniform response structure
- `.editorconfig` with C# coding style rules
- `.gitignore` for .NET projects

---

## Roadmap (Planned Phases)

### Phase 2 (Q1 2026) - Database & Authentication
- [ ] SQL Server / PostgreSQL integration via Entity Framework Core
- [ ] Database migrations
- [ ] JWT authentication and refresh tokens
- [ ] OAuth 2.0 / social login support
- [ ] Full unit and integration test suite

### Phase 3 (Q2 2026) - Search & Cache
- [ ] Elasticsearch integration for production-grade full-text search
- [ ] Redis distributed cache
- [ ] Cache invalidation strategies
- [ ] Search analytics and query suggestions

### Phase 4 (Q3 2026) - Real-Time Features
- [ ] WebSocket support for live notifications
- [ ] Real-time messaging (SignalR)
- [ ] Presence system and activity feeds

### Phase 5 (Q4 2026) - Payment Integration
- [ ] Stripe payment processing
- [ ] Invoice generation and payout management
- [ ] Transaction history and dispute handling

### Phase 6 (2027) - Notifications & Analytics
- [ ] Email and SMS notification system
- [ ] Push notification support
- [ ] Seller analytics dashboard
- [ ] Fraud detection rules engine

---

## Contributors

- **Vladyslav Zaiets** - CTO & Software Architect (https://sarmkadan.com)

---

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

**For latest updates, visit the GitHub repository: https://github.com/Sarmkadan/marketplace-engine**
