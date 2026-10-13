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

## Pricing and discounts

Here are some copy-pasteable examples showing how to apply discounts and taxes using the PricingEngine.

```csharp
// Example: Applying a percentage discount using the ApplyDiscount method
// First, get the listing price (in a real app, you'd get this from a service or repository)
Money basePrice = new Money(100.00m, "USD"); // Example: $100.00
// Apply a 10% discount
var (discountedPrice, savings, platformFee, sellerPayout) = pricingEngine.ApplyDiscount(basePrice, 0.10m);
Console.WriteLine($"Original price: {basePrice}");
Console.WriteLine($"Discount (10%): {savings}");
Console.WriteLine($"Price after discount: {discountedPrice}");
Console.WriteLine($"Platform fee (5%): {platformFee}");
Console.WriteLine($"Seller payout: {sellerPayout}");

// Example: Applying a fixed-amount discount
// Note: The PricingEngine doesn't have a direct method for fixed-amount discounts,
// but we can calculate it manually or extend the engine.
Money basePrice2 = new Money(100.00m, "USD"); // Example: $100.00
decimal fixedDiscountAmount = 5.00m; // $5 discount
if (fixedDiscountAmount > 0 && fixedDiscountAmount < basePrice2.Amount)
{
    decimal discountPercent = (float)fixedDiscountAmount / (float)basePrice2.Amount;
    var (discountedPrice2, savings2, platformFee2, sellerPayout2) = pricingEngine.ApplyDiscount(basePrice2, discountPercent);
    Console.WriteLine($"Original price: {basePrice2}");
    Console.WriteLine($"Fixed discount: {fixedDiscountAmount}");
    Console.WriteLine($"Price after discount: {discountedPrice2}");
    Console.WriteLine($"Platform fee (5%): {platformFee2}");
    Console.WriteLine($"Seller payout: {sellerPayout2}");
}

// Example: Quantity-tier discounts using the CalculateBulkPrice method
Money unitPrice = new Money(25.00m, "USD"); // Example: $25.00 per unit
int quantity = 10;
// Get bulk price for 10 items (qualifies for 10% discount)
Money totalPrice = pricingEngine.CalculateBulkPrice(unitPrice, quantity);
Console.WriteLine($"Unit price: {unitPrice}");
Console.WriteLine($"Quantity: {quantity}");
Console.WriteLine($"Total price (with bulk discount): {totalPrice}");

// Calculate the breakdown for the total price
var (buyerTotal, platformFee3, sellerPayout3) = pricingEngine.CalculateBreakdown(totalPrice);
Console.WriteLine($"Buyer pays: {buyerTotal}");
Console.WriteLine($"Platform fee (5%): {platformFee3}");
Console.WriteLine($"Seller payout: {sellerPayout3}");

// Example: Including tax
// Note: The PricingEngine doesn't have built-in tax calculation,
// but we can demonstrate how to add tax to the final amount.
Money basePrice3 = new Money(100.00m, "USD"); // Example: $100.00
var (buyerTotal2, platformFee4, sellerPayout4) = pricingEngine.CalculateBreakdown(basePrice3);

// Add tax (example: 8% sales tax)
decimal taxRate = 0.08m;
decimal taxAmount = Math.Round((buyerTotal2.Amount + platformFee4.Amount) * taxRate, 2, MidpointRounding.AwayFromZero);
Money tax = new Money(taxAmount, buyerTotal2.CurrencyCode);
Money totalWithTax = new Money(buyerTotal2.Amount + platformFee4.Amount + taxAmount, buyerTotal2.CurrencyCode);

Console.WriteLine($"Base price: {buyerTotal2}");
Console.WriteLine($"Platform fee (5%): {platformFee4}");
Console.WriteLine($"Subtotal: {new Money(buyerTotal2.Amount + platformFee4.Amount, buyerTotal2.CurrencyCode)}");
Console.WriteLine($"Tax (8%): {tax}");
Console.WriteLine($"Total: {totalWithTax}");
```

## Handling payment failures

When processing payments, it's important to handle failures gracefully. The PaymentGateway and OrderProcessor
throw MarketplaceException for various failure scenarios that you can catch and handle.

Here's an example showing how to catch and handle payment failures:

```csharp
try
{
    // Place the order (creates a payment in Pending status)
    var payment = await orderProcessor.PlaceOrderAsync(listingId, buyerId, paymentMethod);
    Console.WriteLine($"Order placed! Payment ID: {payment.Id}");

    // Authorize payment
    var authorizedPayment = await paymentGateway.AuthorizeAsync(payment.Id);
    Console.WriteLine($"Payment authorized. Transaction ID: {authorizedPayment.ExternalTransactionId}");

    // Reserve inventory
    var reservationSuccess = await inventoryTracker.ReserveStockAsync(listingId, 1);
    if (!reservationSuccess)
    {
        // Failed to reserve inventory, we should cancel the order
        await orderProcessor.CancelOrderAsync(payment.Id, "Failed to reserve inventory");
        Console.WriteLine("Failed to reserve inventory, order cancelled.");
        return;
    }
    Console.WriteLine("Inventory reserved");

    // Capture payment (completes the charge)
    var capturedPayment = await paymentGateway.CaptureAsync(authorizedPayment.Id);
    Console.WriteLine($"Payment captured");

    // Commit inventory reservation
    await inventoryTracker.CommitReservationAsync(listingId, 1);
    Console.WriteLine("Inventory commitment completed");

    // Confirm order (marks listing as delisted)
    var confirmedPayment = await orderProcessor.ConfirmOrderAsync(capturedPayment.Id);
    Console.WriteLine($"Order confirmed! Payment status: {confirmedPayment.Status}");
}
catch (MarketplaceException ex) when (ex.Message == "Authorization declined by provider (simulated).")
{
    // Handle simulated payment decline (from DefaultPaymentProvider)
    Console.WriteLine($"Payment authorization failed: {ex.Message}");
    // In a real app, you might ask the user to try a different payment method
}
catch (MarketplaceException ex) when (ex.Message.Contains("not found"))
{
    // Handle not found errors (payment or listing)
    Console.WriteLine($"Resource not found: {ex.Message}");
}
catch (MarketplaceException ex) when (ex.Message.Contains("cannot be authorized") || 
                                  ex.Message.Contains("must be authorized before capture") ||
                                  ex.Message.Contains("Only completed payments can be refunded") ||
                                  ex.Message.Contains("Completed orders must be refunded, not cancelled") ||
                                  ex.Message.Contains("Order is already cancelled"))
{
    // Handle invalid payment state errors
    Console.WriteLine($"Invalid payment state: {ex.Message}");
}
catch (MarketplaceException ex)
{
    // Handle other marketplace exceptions (validation, business rule violations, etc.)
    Console.WriteLine($"Payment processing failed: {ex.Message}");
    if (ex.ValidationErrors != null)
    {
        foreach (var validationError in ex.ValidationErrors)
        {
            Console.WriteLine($"  Validation error for {validationError.Key}: {string.Join(", ", validationError.Value)}");
        }
    }
}
catch (Exception ex)
{
    // Handle unexpected errors
    Console.WriteLine($"Unexpected error processing payment: {ex.Message}");
    // Log the full exception for debugging
    _logger.LogError(ex, "Unexpected error processing payment");
}

// Alternative approach: Check payment status after operations
// Note: The current implementation throws exceptions on failure, so this is less common
// but useful for checking status after successful operations
var payment = await orderProcessor.PlaceOrderAsync(listingId, buyerId, paymentMethod);
if (payment.Status == PaymentStatus.Pending)
{
    Console.WriteLine("Payment is pending authorization");
    // Proceed with authorization
}
else if (payment.Status == PaymentStatus.Failed)
{
    Console.WriteLine("Payment creation failed");
    // Check payment.FailureReason if available
}
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