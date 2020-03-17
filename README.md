# Marketplace Engine

...

## ListingCreatedEvent

The `ListingCreatedEvent` is raised when a new listing is created, triggering actions like notifications, indexing, and recommendations. It contains metadata about the listing, including its unique identifier, seller information, title, and category.

### Usage Example

```csharp
using MarketplaceEngine.Infrastructure.Events;

var listingEvent = new ListingCreatedEvent
{
    ListingId = Guid.NewGuid(),
    SellerId = Guid.NewGuid(),
    Title = "Premium Widget",
    Category = "Electronics"
};

Console.WriteLine($"Event ID: {listingEvent.EventId}");
Console.WriteLine($"Occurred At: {listingEvent.OccurredAt}");
Console.WriteLine($"Listing ID: {listingEvent.ListingId}");
Console.WriteLine($"Seller ID: {listingEvent.SellerId}");
Console.WriteLine($"Title: {listingEvent.Title}");
Console.WriteLine($"Category: {listingEvent.Category}");
```

## ListingCreatedEventHandler

The `ListingCreatedEventHandler` processes `ListingCreatedEvent` notifications to perform essential post-listing operations such as search indexing, seller notifications, recommendation updates, and analytics logging. This handler is automatically invoked by the event bus when a new listing is created, ensuring that listings become immediately discoverable and that relevant parties are notified.

### Usage Example

```csharp
using MarketplaceEngine.Infrastructure.Events;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<ListingCreatedEventHandler>();
var handler = new ListingCreatedEventHandler(logger);

// Create and process the event
var listingEvent = new ListingCreatedEvent
{
    ListingId = Guid.NewGuid(),
    SellerId = Guid.NewGuid(),
    Title = "Premium Wireless Headphones",
    Category = "Electronics",
    Price = 199.99m,
    Description = "Noise-cancelling wireless headphones with 30-hour battery"
};

await handler.HandleAsync(listingEvent);

// Output: Processing listing creation: [listing-guid], Category: Electronics
```
