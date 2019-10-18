# Marketplace Engine

A lightweight, production-grade marketplace backend engine built with .NET 10. Provides comprehensive functionality for managing listings, searching products, organizing categories, enabling user messaging, and content moderation.

## Features

### 🏪 Listings Management
- Create, update, and manage product/service listings
- Support for images, tags, and detailed descriptions
- Featured listings capability
- Price management with multi-currency support
- Listing status tracking (Active, Inactive, Under Review, Flagged, Delisted, Archived)

### 🔍 Search & Discovery
- Full-text search across listings
- Tag-based filtering
- Category browsing with hierarchical organization
- Location-based search with distance calculation
- Trending and recent listings
- Advanced filtering by price range, category, and tags

### 👥 User Management
- User registration and email verification
- Profile management with location support
- Role-based access control (User, Administrator, Moderator, Support, Premium Seller)
- User ratings and reviews
- Account activation/deactivation
- Last activity tracking

### 💬 Messaging System
- Private user-to-user messaging
- Conversation threading with replies
- Message flagging for moderation
- Listing-specific conversations
- Attachment support
- Read/unread status tracking

### 🛡️ Moderation & Safety
- Content moderation reporting
- Multi-priority report system
- Moderation workflow (Pending, In Review, Approved, Rejected)
- User suspension and banning
- Content removal capabilities
- Report assignment to moderators

### 📦 Data Organization
- Hierarchical category system
- Sub-categories support
- Listing count tracking per category
- Active/inactive category management

## Architecture

### Domain-Driven Design
- Strong domain models with business logic encapsulation
- Value objects for Money, Location, and Rating
- Entity validation and state management

### Service Layer
- `ListingService` - Listing lifecycle and operations
- `SearchService` - Advanced search and discovery
- `UserService` - User account management
- `ModerationService` - Content moderation
- `CategoryService` - Category hierarchy management
- `MessagingService` - User messaging system

### Repository Pattern
- Generic `IRepository<T>` interface for basic CRUD
- Specialized repository interfaces with domain-specific queries
- In-memory data storage (ready for database migration)

### Dependency Injection
- Complete DI setup via Microsoft.Extensions.DependencyInjection
- Service auto-wiring via `AddMarketplaceServices()`
- Configuration-driven initialization

## Technology Stack

- **.NET 10** - Latest LTS framework
- **C# 14** - Modern language features
- **ASP.NET Core** - Web API framework
- **OpenAPI/Swagger** - API documentation

## Getting Started

### Prerequisites
- .NET 10 SDK or later
- Visual Studio 2022, VS Code, or any .NET-compatible IDE

### Installation

```bash
# Clone the repository
git clone https://github.com/vladyslav-zaiets/marketplace-engine.git
cd marketplace-engine

# Restore packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project src/MarketplaceEngine
```

The API will be available at `http://localhost:5000` (HTTP) or `https://localhost:5001` (HTTPS).

### API Documentation

Once the application is running, access the Swagger UI:
- Development: `http://localhost:5000`
- Production: Visit the root endpoint

## Project Structure

```
marketplace-engine/
├── src/MarketplaceEngine/
│   ├── Program.cs                 # Application entry point
│   ├── Domain/
│   │   ├── Models/               # Entity models (User, Listing, Category, etc.)
│   │   ├── ValueObjects/         # Money, Location, Rating
│   │   └── Enums/                # ListingStatus, UserRole, ModerationStatus
│   ├── Services/                 # Business logic layer
│   ├── Repositories/             # Data access layer
│   ├── Data/                      # Database context and connection management
│   ├── Exceptions/               # Custom exception types
│   ├── Configuration/            # DI setup and configuration
│   └── Constants/                # Application-wide constants
├── LICENSE                        # MIT License
├── .gitignore                    # Git ignore rules
└── README.md                     # This file
```

## Key Classes

### Domain Models
- **User** - Marketplace user with profile, ratings, and verification
- **Listing** - Product/service listing with images, tags, and pricing
- **Category** - Hierarchical category organization
- **Message** - User-to-user messaging with threading
- **ModerationReport** - Content moderation tracking

### Value Objects
- **Money** - Immutable monetary amount with currency
- **Location** - Geographic location with distance calculations
- **Rating** - User/listing ratings with Bayesian averaging

### Services
Each service provides high-level business operations with validation and error handling.

## API Endpoints

### Health Check
- `GET /api/v1/health` - Service health status

### Listings
- `GET /api/v1/listings` - Get paginated listings
- `GET /api/v1/listings/{id}` - Get listing details
- `GET /api/v1/listings/search?q=...` - Search listings

### Users
- `GET /api/v1/users/{id}` - Get user profile
- `GET /api/v1/users/sellers/top` - Get top sellers

### Categories
- `GET /api/v1/categories` - List categories
- `GET /api/v1/categories/{id}/listings` - Get category listings

## Configuration

### appsettings.json
Main configuration file with marketplace settings, database configuration, and feature flags.

### appsettings.Development.json
Development-specific overrides with debug logging and relaxed constraints.

## Error Handling

The application uses custom exception hierarchy:
- `MarketplaceException` - Base exception
- `ResourceNotFoundException` - When resource not found
- `ValidationException` - When input validation fails
- `UnauthorizedException` - When user lacks permission
- `DuplicateResourceException` - When resource already exists

## Validation

All domain models include:
- Property validation in constructors and setters
- Business rule validation
- Serialization-safe design

## Development Guidelines

### Adding New Features

1. Define domain models in `Domain/Models/`
2. Create repository interface in `Repositories/`
3. Implement repository in `Repositories/`
4. Create service in `Services/`
5. Register in `Configuration/DependencyInjection.cs`
6. Add API endpoints in `Program.cs`

### Code Standards

- Use nullable reference types (enabled in .csproj)
- Implement input validation at boundaries
- Use async/await for I/O operations
- Follow C# naming conventions
- Include XML comments on public types

## Phase 1 - What's Included

✅ Complete domain model with 5+ entity classes  
✅ Service layer with full business logic  
✅ Repository pattern with CRUD + custom queries  
✅ Dependency injection setup  
✅ Custom exceptions and error types  
✅ Constants and enums  
✅ In-memory data storage  
✅ API endpoints with OpenAPI documentation  
✅ Configuration management  

## Future Phases

- Phase 2: Database integration (SQL Server/PostgreSQL)
- Phase 3: Authentication & Authorization (OAuth 2.0)
- Phase 4: Real-time features (WebSockets)
- Phase 5: Advanced search (Elasticsearch)
- Phase 6: Payment integration (Stripe)
- Phase 7: Notification system (Email/SMS)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**Vladyslav Zaiets**  
CTO & Software Architect  
https://sarmkadan.com

## Contributing

Contributions are welcome! Please follow the existing code style and conventions.

## Support

For issues, questions, or suggestions, please use the GitHub Issues page.

---

**Built with ❤️ using .NET 10 and modern C# practices**
