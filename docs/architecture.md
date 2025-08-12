// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Architecture

This document describes the overall architecture and design patterns used in Marketplace Engine.

## Design Philosophy

Marketplace Engine follows these architectural principles:

1. **Clean Architecture** - Separation of concerns with clear boundaries
2. **Domain-Driven Design** - Business logic encapsulated in domain models
3. **SOLID Principles** - Single Responsibility, Open/Closed, Liskov, Interface Segregation, Dependency Inversion
4. **Testability** - Components designed to be testable and loosely coupled
5. **Scalability** - Prepared for horizontal scaling and database migration

## Layered Architecture

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         API Layer                               │
│  Controllers / Minimal APIs / OpenAPI Documentation             │
├─────────────────────────────────────────────────────────────────┤
│                      Service Layer                              │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Listing  │  Search  │  User   │  Message │ Moderation  │  │
│  │  Service  │  Service │ Service │ Service  │  Service    │  │
│  └──────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│                    Domain Layer                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │          Entities, Value Objects, Enums, Events          │  │
│  │  User  │  Listing  │  Message  │  Category  │  Report   │  │
│  │  Money │ Location  │  Rating   │  Exceptions            │  │
│  └──────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│                  Repository Layer                               │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  IRepository<T>  │  Specialized Repository Interfaces    │  │
│  │  In-Memory Implementation (Swappable)                    │  │
│  └──────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│                Infrastructure Layer                             │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Database  │  Cache  │  Events  │  Security  │  Logging │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## Layer Responsibilities

### API Layer

**Location:** `Controllers/` and `Program.cs`

Responsible for:
- HTTP request handling
- Input validation and binding
- API documentation (OpenAPI/Swagger)
- CORS and security headers
- Request/response marshalling

**Key Components:**
- `ListingsController` - Listing CRUD endpoints
- `UsersController` - User management endpoints
- `MessagesController` - Messaging endpoints
- `CategoriesController` - Category endpoints
- `ModerationController` - Moderation endpoints

### Service Layer

**Location:** `Services/`

Responsible for:
- Business logic encapsulation
- Transaction management
- Cross-cutting concerns
- Domain validation
- Service orchestration

**Core Services:**

| Service | Responsibility |
|---------|-----------------|
| `ListingService` | Create, update, search, and manage listings |
| `SearchService` | Advanced search, filtering, and discovery |
| `UserService` | User registration, profile, roles, ratings |
| `MessagingService` | Conversations, messaging, threading |
| `ModerationService` | Report creation, assignment, enforcement |
| `CategoryService` | Category hierarchy, management |

### Domain Layer

**Location:** `Domain/`

Responsible for:
- Business entities and rules
- Value objects
- Enumerations
- Domain events
- Exception types

**Core Entities:**

```
User
├── Email
├── Username
├── FullName
├── Role (Enum)
├── Rating (Value Object)
├── Location (Value Object)
└── Account Status

Listing
├── Title
├── Description
├── Price (Value Object - Money)
├── Category
├── Tags
├── Seller (User reference)
├── Status (Enum)
├── Location (Value Object)
└── Images

Message
├── Sender (User reference)
├── Recipient (User reference)
├── Content
├── Listing (Optional reference)
├── Status (Read/Unread)
└── Attachments

Category
├── Name
├── Description
├── ParentCategory (Optional)
├── IsActive
└── ListingCount

ModerationReport
├── Reporter (User reference)
├── Reason
├── Priority (Enum)
├── Status (Enum)
├── TargetListing (Optional)
└── AssignedModerator (Optional)
```

**Value Objects:**

```
Money
├── Amount (decimal)
└── Currency (string)

Location
├── City
├── State/Province
├── Country
└── Coordinates (Optional)

Rating
├── AverageRating (decimal)
├── TotalReviews (int)
├── PositiveCount (int)
└── NegativeCount (int)
```

### Repository Layer

**Location:** `Repositories/`

Responsible for:
- Data persistence abstraction
- Query building
- Transaction handling
- Data access patterns

**Key Repositories:**

```
IRepository<T>                    // Generic base interface
├── GetAsync(id)
├── GetAllAsync()
├── AddAsync(entity)
├── UpdateAsync(entity)
├── DeleteAsync(id)
└── SaveChangesAsync()

IListingRepository : IRepository<Listing>
├── GetActiveListingsAsync()
├── GetUserListingsAsync(userId)
├── SearchAsync(criteria)
└── GetTrendingListingsAsync()

IUserRepository : IRepository<User>
├── GetByEmailAsync(email)
├── GetByUsernameAsync(username)
├── GetTopSellersAsync(limit)
└── GetUserWithRatingsAsync(id)

IMessageRepository : IRepository<Message>
├── GetConversationAsync(userId1, userId2)
├── GetUserConversationsAsync(userId)
└── GetUnreadCountAsync(userId)
```

### Infrastructure Layer

**Location:** `Infrastructure/`

Responsible for:
- External system integration
- Caching
- Event publishing
- Security/authentication
- Logging and monitoring

**Components:**

| Component | Purpose |
|-----------|---------|
| `CacheService` | In-memory caching for performance |
| `EventBus` | Publish/subscribe event system |
| `TokenService` | JWT token generation (Phase 2) |
| `PermissionService` | RBAC and authorization |
| `ExternalListingProvider` | Third-party integration |
| `WebhookService` | Webhook notifications |
| `HttpClientService` | HTTP communication |

## Dependency Injection

The application uses Microsoft's built-in DI container:

**Setup Location:** `Configuration/DependencyInjection.cs`

```csharp
// Services registration pattern
services
    .AddScoped<IListingRepository, ListingRepository>()
    .AddScoped<ListingService>()
    .AddSingleton<CacheService>()
    .AddEventHandlers();
```

**Key Principles:**
- Services are resolved through `IServiceProvider`
- Scoped lifetime for domain services
- Singleton for stateless utilities
- Transient for stateful components

## Data Flow

### Create Listing Flow

```
HTTP POST /api/v1/listings
    ↓
ListingsController.Create()
    ↓
ListingService.CreateListingAsync()
    ├─ Validate input
    ├─ Create Listing entity
    ├─ Calculate derived fields
    ├─ IListingRepository.AddAsync()
    │   └─ In-memory storage (Phase 1)
    │       → Database (Phase 2)
    ├─ Publish DomainEvent
    ├─ EventHandlers respond to event
    └─ Return ListingDto
        ↓
Controller maps to response
    ↓
HTTP 201 Created response
```

### Search Flow

```
HTTP GET /api/v1/listings/search?q=...
    ↓
SearchService.SearchAsync()
    ├─ Parse search query
    ├─ Build filter criteria
    ├─ Query repository
    │   └─ Apply filters in-memory (Phase 1)
    │       → Database index (Phase 2)
    │       → Elasticsearch (Phase 3)
    ├─ Sort results
    ├─ Apply pagination
    ├─ Enrich with cache data
    └─ Return paginated results
        ↓
Controller maps to response
    ↓
HTTP 200 OK response
```

### Message Send Flow

```
HTTP POST /api/v1/messages
    ↓
MessagesController.Send()
    ↓
MessagingService.SendMessageAsync()
    ├─ Validate sender/recipient
    ├─ Create Message entity
    ├─ IMessageRepository.AddAsync()
    ├─ Update conversation metadata
    ├─ Publish MessageSentEvent
    ├─ EventHandlers:
    │   ├─ SendNotification handler
    │   ├─ UpdateUserActivity handler
    │   └─ LogEvent handler
    └─ Return MessageDto
        ↓
HTTP 201 Created response
```

## Design Patterns Used

### 1. Repository Pattern

Abstracts data access, allowing swappable implementations:

```csharp
// Interface-based
public interface IListingRepository : IRepository<Listing>
{
    Task<IEnumerable<Listing>> SearchAsync(SearchCriteria criteria);
}

// In-memory (Phase 1)
public class ListingRepository : IListingRepository { }

// Database (Phase 2)
public class DatabaseListingRepository : IListingRepository { }
```

### 2. Dependency Injection

Loose coupling through constructor injection:

```csharp
public class ListingService
{
    private readonly IListingRepository _repository;
    private readonly IUserRepository _userRepository;

    public ListingService(
        IListingRepository repository,
        IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }
}
```

### 3. Value Objects

Immutable objects representing domain concepts:

```csharp
public sealed record Money(decimal Amount, string Currency)
{
    public Money(decimal amount, string currency)
        : this(amount, currency)
    {
        if (amount < 0) throw new ArgumentException("Amount must be positive");
        if (string.IsNullOrEmpty(currency)) throw new ArgumentException("Currency required");
    }
}
```

### 4. Service Locator (Minimal)

Configuration-based registration:

```csharp
builder.Services.AddMarketplaceServices();
```

### 5. Event-Driven Architecture

Decoupled service communication:

```csharp
public class ListingCreatedEvent : DomainEvent
{
    public int ListingId { get; set; }
    public int SellerId { get; set; }
}

// Handler
public class ListingCreatedEventHandler : IEventHandler<ListingCreatedEvent>
{
    public async Task HandleAsync(ListingCreatedEvent @event)
    {
        // Process event asynchronously
    }
}
```

## Database Design (Phase 2)

Prepared for relational database migration:

```sql
-- Users table
CREATE TABLE Users (
    Id INT PRIMARY KEY,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    Username NVARCHAR(100) UNIQUE NOT NULL,
    FullName NVARCHAR(255),
    PasswordHash NVARCHAR(MAX),
    Role INT,
    CreatedAt DATETIME2
);

-- Listings table
CREATE TABLE Listings (
    Id INT PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    SellerId INT FOREIGN KEY REFERENCES Users(Id),
    Price DECIMAL(18,2),
    Currency NVARCHAR(3),
    Status INT,
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2
);

-- Messages table
CREATE TABLE Messages (
    Id INT PRIMARY KEY,
    SenderId INT FOREIGN KEY REFERENCES Users(Id),
    RecipientId INT FOREIGN KEY REFERENCES Users(Id),
    Content NVARCHAR(MAX),
    ListingId INT FOREIGN KEY REFERENCES Listings(Id),
    IsRead BIT,
    CreatedAt DATETIME2
);

-- Categories table
CREATE TABLE Categories (
    Id INT PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    ParentCategoryId INT FOREIGN KEY REFERENCES Categories(Id),
    IsActive BIT,
    CreatedAt DATETIME2
);

-- ModerationReports table
CREATE TABLE ModerationReports (
    Id INT PRIMARY KEY,
    ReporterId INT FOREIGN KEY REFERENCES Users(Id),
    Reason NVARCHAR(MAX),
    Priority INT,
    Status INT,
    ListingId INT FOREIGN KEY REFERENCES Listings(Id),
    AssignedModeratorId INT FOREIGN KEY REFERENCES Users(Id),
    CreatedAt DATETIME2
);
```

## Caching Strategy

**Phase 1 (Current):**
- In-memory cache for frequent queries
- User role cache
- Category list cache
- Search result cache (short-lived)

**Phase 2+:**
- Distributed Redis cache
- Cache invalidation events
- Cache-aside pattern
- Distributed session storage

## Security Architecture

**Phase 1:**
- Header-based user identification (X-User-Id)
- Role-based endpoint access
- CORS policy validation

**Phase 2:**
- JWT token authentication
- OAuth 2.0 integration
- Rate limiting per user
- API key management

## Error Handling

**Custom Exception Hierarchy:**

```
Exception
└── MarketplaceException (Base)
    ├── ValidationException
    ├── ResourceNotFoundException
    ├── UnauthorizedException
    ├── DuplicateResourceException
    └── OperationFailedException
```

**Error Response Format:**

```json
{
  "success": false,
  "message": "User not found",
  "errorCode": "USER_NOT_FOUND",
  "details": ["UserId: 999 does not exist"],
  "timestamp": "2026-05-04T10:30:00Z"
}
```

## Scalability Considerations

### Horizontal Scaling
- Stateless service design
- Session stored externally (Phase 2)
- Load balancer ready
- No server affinity required

### Vertical Scaling
- Async/await throughout
- Connection pooling ready
- Memory-efficient algorithms
- Efficient data structures

### Database Scaling (Phase 2)
- Prepared for read replicas
- Query optimization with indexes
- Sharding-friendly schema
- Event sourcing ready

## Code Organization

```
src/MarketplaceEngine/
├── Program.cs                      # Entry point
├── MarketplaceEngine.csproj       # Project file
├── Domain/                         # Business logic
│   ├── Models/
│   ├── ValueObjects/
│   └── Enums/
├── Services/                       # Business services
├── Repositories/                   # Data access
├── Controllers/                    # API endpoints
├── Configuration/                  # DI setup
├── Infrastructure/                 # External integration
├── Middleware/                     # HTTP pipeline
├── Exceptions/                     # Error types
├── DTOs/                          # Transfer objects
├── Constants/                      # Constants
└── Utilities/                      # Helpers
```

## Performance Characteristics

| Operation | Complexity | Current (Phase 1) |
|-----------|-----------|------------------|
| Get Listing | O(1) | <1ms |
| Search (100 results) | O(n) | ~10ms |
| Get User | O(1) | <1ms |
| Send Message | O(1) | <5ms |
| Create Listing | O(1) | <5ms |

**Phase 2+ will add database overhead (~5-20ms) but support unlimited scale.**

---

**Next:** See [API Reference](api-reference.md) for endpoint documentation.
