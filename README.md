# Marketplace Engine

An ASP.NET Core (net10.0) marketplace backend with an entirely in-memory data store - no database, no external infrastructure.

## Getting Started

### Prerequisites
- .NET SDK 10.0.100+ (check `global.json` for exact version)
- Git (for cloning the repository)

### Installation
1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd marketplace-engine
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

### Running the Application
```bash
dotnet run --project src/MarketplaceEngine
```
The API will be available at `http://localhost:5000` with Swagger UI at `/` (in Development environment).

### Using Makefile
Alternatively, use the provided Makefile wrappers:
```bash
make build   # Equivalent to dotnet build
make run     # Equivalent to dotnet run
make watch   # Runs with file watching for development
make test    # Runs all tests
make format  # Formats code according to .editorconfig
```

## How the Pieces Fit

The marketplace engine follows a clean architecture with these core services working together:

```
Product Catalog → Pricing Engine → Inventory Tracker → Payment Gateway → Order Processor
```

1. **ProductCatalog** - Browse and search active listings
2. **PricingEngine** - Calculate prices with fees, discounts, and bulk pricing
3. **InventoryTracker** - Manage stock levels and reservations
4. **PaymentGateway** - Handle payment authorization, capture, and refunds
5. **OrderProcessor** - Orchestrate the complete order lifecycle

## End-to-End Example: Placing an Order

Here's a complete, copy-pasteable example showing how to set up dependency injection and place an order:

```csharp
using MarketplaceEngine.Configuration;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;

// 1. Set up dependency injection (normally done in Program.cs)
var services = new ServiceCollection();
services.AddMarketplaceServices(); // Registers all marketplace services

// Build the service provider
var serviceProvider = services.BuildServiceProvider();

// 2. Get required services from DI container
var productCatalog = serviceProvider.GetRequiredService<ProductCatalog>();
var pricingEngine = serviceProvider.GetRequiredService<PricingEngine>();
var inventoryTracker = serviceProvider.GetRequiredService<InventoryTracker>();
var paymentGateway = serviceProvider.GetRequiredService<PaymentGateway>();
var orderProcessor = serviceProvider.GetRequiredService<OrderProcessor>();

// 3. Example: Place an order for a listing
async Task PlaceSampleOrder()
{
    // For demonstration, we'll use hardcoded IDs
    // In a real app, these would come from user authentication/context
    var listingId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    var buyerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var paymentMethod = "credit_card";

    try
    {
        // Step 1: Get product details
        var listing = await productCatalog.GetProductDetailAsync(listingId);
        if (listing == null)
        {
            Console.WriteLine("Listing not found");
            return;
        }

        Console.WriteLine($"Found listing: {listing.Title} - {listing.Price}");

        // Step 2: Calculate pricing (including platform fee)
        var pricingBreakdown = await pricingEngine.CalculateTotalAsync(listingId);
        Console.WriteLine($"Price breakdown:");
        Console.WriteLine($"  Base price: {pricingBreakdown.BuyerTotal}");
        Console.WriteLine($"  Platform fee: {pricingBreakdown.PlatformFee}");
        Console.WriteLine($"  Seller payout: {pricingBreakdown.SellerPayout}");
        Console.WriteLine($"  Buyer pays: {pricingBreakdown.BuyerTotal + pricingBreakdown.PlatformFee}");

        // Step 3: Check inventory
        var availableStock = inventoryTracker.GetAvailableStock(listingId);
        Console.WriteLine($"Available stock: {availableStock}");

        if (availableStock <= 0)
        {
            Console.WriteLine("Item is out of stock");
            return;
        }

        // Step 4: Place the order (creates a payment in Pending status)
        var payment = await orderProcessor.PlaceOrderAsync(listingId, buyerId, paymentMethod);
        Console.WriteLine($"Order placed! Payment ID: {payment.Id}");

        // Step 5: Authorize payment
        var authorizedPayment = await paymentGateway.AuthorizeAsync(payment.Id);
        Console.WriteLine($"Payment authorized. Transaction ID: {authorizedPayment.ExternalTransactionId}");

        // Step 6: Reserve inventory
        var reservationSuccess = await inventoryTracker.ReserveStockAsync(listingId, 1);
        if (!reservationSuccess)
        {
            Console.WriteLine("Failed to reserve inventory");
            // In a real app, you'd fail the payment here
            return;
        }
        Console.WriteLine("Inventory reserved");

        // Step 7: Capture payment (completes the charge)
        var capturedPayment = await paymentGateway.CaptureAsync(authorizedPayment.Id);
        Console.WriteLine($"Payment captured");

        // Step 8: Commit inventory reservation
        await inventoryTracker.CommitReservationAsync(listingId, 1);
        Console.WriteLine("Inventory commitment completed");

        // Step 9: Confirm order (marks listing as delisted)
        var confirmedPayment = await orderProcessor.ConfirmOrderAsync(capturedPayment.Id);
        Console.WriteLine($"Order confirmed! Payment status: {confirmedPayment.Status}");

        // Final summary
        Console.WriteLine("\n=== ORDER SUMMARY ===");
        Console.WriteLine($"Listing: {listing.Title}");
        Console.WriteLine($"Buyer ID: {buyerId}");
        Console.WriteLine($"Seller ID: {listing.SellerId}");
        Console.WriteLine($"Amount paid: {pricingBreakdown.BuyerTotal + pricingBreakdown.PlatformFee}");
        Console.WriteLine($"Seller receives: {pricingBreakdown.SellerPayout}");
        Console.WriteLine($"Platform fee: {pricingBreakdown.PlatformFee}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error placing order: {ex.Message}");
        // In a production system, you'd have proper error handling and cleanup
    }
}

// Run the example
await PlaceSampleOrder();
```

## Project Structure

```
MarketplaceEngine.sln          - Solution file
src/MarketplaceEngine/         - Main project source
├── Program.cs                 - Application entry point with DI setup
├── Configuration/             - Dependency injection configuration
├── Controllers/               - API controllers (attribute-routed)
├── Services/                  - Business logic services
├── Repositories/              - Data access interfaces and implementations
├── Domain/                    - Domain models, value objects, enums
├── DTOs/                      - Data transfer objects
├── Exceptions/                - Custom exception hierarchy
├── Middleware/                - Custom middleware (error handling, logging, rate limiting)
├── Infrastructure/            - Cross-cutting concerns (caching, events, background jobs, etc.)
├── Recommendations/           - Recommendation engine
├── Constants/                 - Application constants
├── Utilities/                 - Helper classes
└── Data/                      - In-memory database context (MarketplaceDbContext)
tests/marketplace-engine.Tests/ - Unit tests (xUnit)
```

## Build Targets

- `dotnet build` - Debug build
- `dotnet build -c Release` - Release build (used by CI)
- `dotnet run --project src/MarketplaceEngine` - Run the application
- `dotnet test` - Run all tests
- `dotnet format` - Apply code formatting

## API Documentation

When running in Development mode, Swagger UI is available at the root path (`/`) showing all available endpoints.

## Testing

Run the test suite with:
```bash
dotnet test --verbosity normal
```

To run a specific test class:
```bash
dotnet test --filter "FullyQualifiedName~ListingServiceTests"
```

CI-style test run:
```bash
dotnet test -c Release --no-build --logger "trx;LogFileName=test-results.trx"
```

## Benchmarks

Run performance benchmarks:
```bash
dotnet run -c Release --project MarketplaceEngine.Benchmarks
```

## Conventions Followed

- All services and repositories are registered as singletons (consistent with in-memory store)
- File-scoped namespaces with `#nullable enable` at the top of files
- `sealed` classes for services where appropriate
- XML doc comments on all public types and members
- PascalCase for types/members, `I`-prefix for interfaces, `_camelCase` for private fields
- Async methods end in `Async`
- Constructor dependencies are null-checked with `?? throw new ArgumentNullException(nameof(x))`
- Domain events are `*Event` records published through `EventBus`
- Test classes follow `<Type>Tests.cs` naming convention
- Errors are thrown from the `MarketplaceException` hierarchy
- Conventional commits (`docs:`, `chore:`, `feat:`, `fix:`)