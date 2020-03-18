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

## HttpClientService

`HttpClientService` is a thin wrapper around `HttpClient` that adds built‑in retry logic, timeout handling, and simple logging for external API calls. It exposes convenient generic methods for the common HTTP verbs and lets callers configure authentication and custom headers in a fluent way.

### Usage Example

```csharp
using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using MarketplaceEngine.Infrastructure.Integration;

// Define a response DTO for demonstration
public class SampleResponse
{
    public string? Message { get; set; }
    public int Code { get; set; }
}

// Top‑level statements (C# 9+)
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger<HttpClientService> logger = loggerFactory.CreateLogger<HttpClientService>();

using var httpClient = new HttpClient();
var api = new HttpClientService(httpClient, logger);

// Configure headers
api.SetAuthorizationHeader("Bearer", "your-token-here");
api.AddHeader("X-Correlation-Id", Guid.NewGuid().ToString());

// GET request
SampleResponse? getResult = await api.GetAsync<SampleResponse>("https://api.example.com/status");

// POST request
var postPayload = new { Name = "NewItem", Quantity = 10 };
var postResult = await api.PostAsync<SampleResponse>("https://api.example.com/items", postPayload);

// PUT request
var putPayload = new { Name = "UpdatedItem", Quantity = 20 };
var putResult = await api.PutAsync<SampleResponse>("https://api.example.com/items/1", putPayload);

// DELETE request
await api.DeleteAsync("https://api.example.com/items/1");
```

## WebhookEvent

The `WebhookEvent` represents an incoming webhook notification from an external system. It contains metadata about the event, including the event type, timestamp, and data payload.

### Usage Example

```csharp
using MarketplaceEngine.Infrastructure.Integration;

var webhookEvent = new WebhookEvent
{
    EventType = "payment.completed",
    Timestamp = DateTime.UtcNow,
    Data = new Dictionary<string, object?>
    {
        ["transactionId"] = "TX12345",
        ["amount"] = 19.99m
    }
};

Console.WriteLine($"Event Type: {webhookEvent.EventType}");
Console.WriteLine($"Timestamp: {webhookEvent.Timestamp}");
Console.WriteLine($"Data: {JsonSerializer.Serialize(webhookEvent.Data)}");
```
