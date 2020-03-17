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
