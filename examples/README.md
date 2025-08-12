// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Marketplace Engine - Examples

Complete, runnable examples demonstrating all features of Marketplace Engine.

## C# Examples

### 1. BasicListingExample.cs

**Demonstrates:** Listing CRUD operations and queries

```bash
cd ..
dotnet run --project examples/BasicListingExample.cs
```

**Features:**
- Create listings
- Retrieve listing details
- List all listings
- Get user's listings
- Update listing price
- Mark listings as inactive/featured

### 2. SearchExample.cs

**Demonstrates:** Advanced search and filtering

**Features:**
- Full-text search
- Filter by price range, category, location
- Sort and paginate results
- Get trending listings
- Get featured listings

### 3. MessagingExample.cs

**Demonstrates:** User-to-user messaging system

**Features:**
- Send messages
- Retrieve conversations
- Get message history
- Mark messages as read
- Message threading

### 4. ModerationExample.cs

**Demonstrates:** Content moderation workflow

**Features:**
- Create reports (Low, Medium, High, Critical)
- Retrieve pending reports
- Assign to moderators
- Update report status
- Moderation statistics

### 5. CategoryManagementExample.cs

**Demonstrates:** Hierarchical category organization

**Features:**
- Create main categories and subcategories
- Get category hierarchy
- List categories with listings
- Get listings by category
- Update and deactivate categories

### 6. UserManagementExample.cs

**Demonstrates:** User registration and management

**Features:**
- Register new users
- Retrieve user profiles
- Update user roles
- View user ratings
- Get top sellers
- Manage user status (activate/deactivate)

## cURL Examples

Shell script with HTTP API examples using cURL.

### Usage

```bash
# Make executable
chmod +x curl-examples.sh

# Run examples
./curl-examples.sh

# Or use with jq for pretty JSON output
./curl-examples.sh | jq .
```

### Included Examples

1. Create a listing
2. Get all listings
3. Search listings with filters
4. Get listing details
5. Update listing
6. Register user
7. Get user profile
8. Get top sellers
9. Send message
10. Get conversations
11. Get all categories
12. Get category listings
13. Create category (admin)
14. Create moderation report
15. Get moderation reports
16. Health check

## Docker Compose Examples

### Extended Configuration

**File:** `docker-compose.example.yml`

Includes:
- Marketplace API
- Redis Cache
- PostgreSQL Database
- Nginx Reverse Proxy
- Adminer (DB Management)
- Redis Commander (Cache Management)

**Usage:**

```bash
# Copy and rename
cp docker-compose.example.yml ../docker-compose.full.yml

# Update configuration as needed
nano ../docker-compose.full.yml

# Start services
cd ..
docker-compose -f docker-compose.full.yml up -d

# Access:
# - API: http://localhost:5000
# - Adminer: http://localhost:8080
# - Redis Commander: http://localhost:8081
```

## Running Examples

### Prerequisites

Ensure the API is running:

```bash
cd ..
dotnet run --project src/MarketplaceEngine
```

API will be available at `http://localhost:5000`

### Running C# Examples

Each example is a standalone program:

```bash
# From project root
dotnet run --project examples/BasicListingExample.cs

dotnet run --project examples/SearchExample.cs

dotnet run --project examples/MessagingExample.cs

# etc.
```

### Testing with cURL

```bash
# All examples
chmod +x examples/curl-examples.sh
./examples/curl-examples.sh

# Single endpoint
curl http://localhost:5000/api/v1/health

# With pretty JSON
curl http://localhost:5000/api/v1/listings | jq .
```

## Expected Output

All examples output success/error messages and data results.

Example output from BasicListingExample:

```
=== Marketplace Engine - Basic Listing Example ===

1. Creating a new listing...
✓ Created listing ID: 1
  Title: iPhone 14 Pro - 256GB Space Black
  Price: $999.99 USD

2. Retrieving listing details...
✓ Retrieved listing:
  ID: 1
  Title: iPhone 14 Pro - 256GB Space Black
  ...
```

## Troubleshooting

### "API not responding"

Ensure API is running:

```bash
cd ..
dotnet run --project src/MarketplaceEngine
```

### "Unable to find target framework"

Ensure .NET 10 SDK is installed:

```bash
dotnet --version
# Should be 10.0.x or higher
```

### cURL: "Connection refused"

Check API URL and port:

```bash
curl http://localhost:5000/api/v1/health
```

## Extending Examples

To add your own examples:

1. Create a new `.cs` file in `examples/`
2. Follow the existing pattern (using DependencyInjection)
3. Add author header
4. Document features in README

Example template:

```csharp
// =============================================================================
// Author: [Your Name] | [Your Website]
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Services;
using MarketplaceEngine.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarketplaceEngine.Examples;

/// <summary>
/// Demonstrates [feature].
/// </summary>
public class YourExample
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddMarketplaceServices();
        var provider = services.BuildServiceProvider();

        var service = provider.GetRequiredService<YourService>();

        try
        {
            // Your code here
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }
}
```

## Next Steps

1. Review [Getting Started](../docs/getting-started.md)
2. Check [API Reference](../docs/api-reference.md)
3. Read [Architecture](../docs/architecture.md)
4. See [Deployment Guide](../docs/deployment.md)

---

**Happy coding! 🚀**
