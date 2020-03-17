# Marketplace Engine

...

## ApiToken

Represents an API token, which is a unique identifier for a user's authentication session. It contains a token value, user ID, issuance date, expiration date, and a list of scopes that define the token's permissions.

### Usage Example

```csharp
using MarketplaceEngine.Infrastructure.Security;

var tokenService = new TokenService(ILogger<TokenService>.CreateLogger(), "secret");
var token = tokenService.GenerateToken(Guid.NewGuid(), new List<string> { "read", "write" });

Console.WriteLine($"Token: {token.Token}");
Console.WriteLine($"User ID: {token.UserId}");
Console.WriteLine($"Issued At: {token.IssuedAt}");
Console.WriteLine($"Expires At: {token.ExpiresAt}");
Console.WriteLine($"Scopes: {string.Join(", ", token.Scopes)}");

if (tokenService.IsTokenValid(token))
{
    Console.WriteLine("Token is valid");
}
else
{
    Console.WriteLine("Token is invalid");
}

tokenService.RevokeToken(token.Token);
```

## SellerListingPerformanceDto

Represents a seller's listing performance metrics, including active and inactive listings, featured listings, total views, total interest count, engagement rate, conversion rate, and top listings.

### Usage Example

```csharp
using MarketplaceEngine.DTOs;

var sellerListingPerformance = new SellerListingPerformanceDto
{
    SellerId = Guid.NewGuid(),
    ActiveListings = 10,
    InactiveListings = 5,
    FeaturedListings = 2,
    TotalViews = 1000,
    TotalInterestCount = 500,
    EngagementRate = 0.5,
    ConversionRate = 0.2,
    TopListings = new List<TopListingDto>
    {
        new TopListingDto { ListingId = Guid.NewGuid(), Views = 200, InterestCount = 100 },
        new TopListingDto { ListingId = Guid.NewGuid(), Views = 150, InterestCount = 75 },
        new TopListingDto { ListingId = Guid.NewGuid(), Views = 300, InterestCount = 150 }
    }
};

Console.WriteLine($"Seller ID: {sellerListingPerformance.SellerId}");
Console.WriteLine($"Active Listings: {sellerListingPerformance.ActiveListings}");
Console.WriteLine($"Inactive Listings: {sellerListingPerformance.InactiveListings}");
Console.WriteLine($"Featured Listings: {sellerListingPerformance.FeaturedListings}");
Console.WriteLine($"Total Views: {sellerListingPerformance.TotalViews}");
Console.WriteLine($"Total Interest Count: {sellerListingPerformance.TotalInterestCount}");
Console.WriteLine($"Engagement Rate: {sellerListingPerformance.EngagementRate}");
Console.WriteLine($"Conversion Rate: {sellerListingPerformance.ConversionRate}");
Console.WriteLine($"Top Listings:");
foreach (var topListing in sellerListingPerformance.TopListings)
{
    Console.WriteLine($"  - Listing ID: {topListing.ListingId}, Views: {topListing.Views}, Interest Count: {topListing.InterestCount}");
}
```

## ModerationServiceExtensions

The `ModerationServiceExtensions` class provides a set of extension methods for moderation-related tasks, including reporting, assigning, and processing moderation reports.

### Usage Example

```csharp
using MarketplaceEngine.Services;

var reports = await ModerationServiceExtensions.ReportListingsBatchAsync(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });
Console.WriteLine($"Reports: {reports.Count}");

var assignedReports = await ModerationServiceExtensions.AssignReportsToModeratorAsync(reports, Guid.NewGuid());
Console.WriteLine($"Assigned Reports: {assignedReports.Count}");

var processedReports = await ModerationServiceExtensions.ProcessReportsBatchAsync(assignedReports);
Console.WriteLine($"Processed Reports: {processedReports.Count}");
```

## ListingServiceBenchmarks

The `ListingServiceBenchmarks` class measures performance of listing creation and search operations in the marketplace engine. It provides benchmarking capabilities for critical listing workflows.

### Usage Example

```csharp
using MarketplaceEngine.Benchmarks;

var benchmarks = new ListingServiceBenchmarks();
benchmarks.Setup();
await benchmarks.CreateListingBenchmark();
await benchmarks.SearchListingsBenchmark();
ListingServiceBenchmarks.Main();
```

## PermissionService

The `PermissionService` class provides methods for checking user permissions and roles, implementing role-based access control (RBAC). It allows you to verify if a user has a specific role, can edit or delete listings, moderate, create listings, message other users, or submit reports.

### Usage Example

```csharp
using MarketplaceEngine.Infrastructure.Security;
using Microsoft.Extensions.Logging;

var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<PermissionService>();
var permissionService = new PermissionService(logger);

var userRole = UserRole.Administrator;
var requiredRole = UserRole.Administrator;
var listingSellerId = Guid.NewGuid();
var userId = Guid.NewGuid();
var recipientId = Guid.NewGuid();

Console.WriteLine($"Has Role: {permissionService.HasRole(userRole, requiredRole)}");
Console.WriteLine($"Can Edit Listing: {permissionService.CanEditListing(userRole, listingSellerId, userId)}");
Console.WriteLine($"Can Delete Listing: {permissionService.CanDeleteListing(userRole, listingSellerId, userId)}");
Console.WriteLine($"Can Moderate: {permissionService.CanModerate(userRole)}");
Console.WriteLine($"Can Create Listing: {permissionService.CanCreateListing(userRole)}");
Console.WriteLine($"Can Message: {permissionService.CanMessage(userRole, recipientId, userId)}");
Console.WriteLine($"Can Submit Report: {permissionService.CanSubmitReport(userRole)}");
```

## IEvent

The `IEvent` interface defines the contract for all events in the marketplace engine's event bus system. Events represent occurrences within the application that different components can react to asynchronously. Each event includes a unique identifier, timestamp, and event type information.

### Usage Example

```csharp
using MarketplaceEngine.Infrastructure.Events;
using Microsoft.Extensions.Logging;
using System;

// Define a custom event that implements IEvent
public class ListingCreatedEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => nameof(ListingCreatedEvent);
    
    public Guid ListingId { get; set; }
    public Guid SellerId { get; set; }
    public string Title { get; set; } = string.Empty;
}

// Create event bus and subscribe to events
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<EventBus>();
var eventBus = new EventBus(logger);

// Subscribe a handler to ListingCreatedEvent
Func<ListingCreatedEvent, Task> handler = async (@event) =>
{
    Console.WriteLine($"Handling ListingCreatedEvent: {{{nameof(@event.ListingId)}={@event.ListingId}, {nameof(@event.Title)}=@event.Title}}");
    await Task.CompletedTask;
};

eventBus.Subscribe<ListingCreatedEvent>(handler);

// Publish an event
var listingEvent = new ListingCreatedEvent
{
    ListingId = Guid.NewGuid(),
    SellerId = Guid.NewGuid(),
    Title = "Premium Widget"
};

await eventBus.PublishAsync(listingEvent);

// Unsubscribe when done
eventBus.Unsubscribe<ListingCreatedEvent>(handler);

// Or unsubscribe all handlers for a specific event type
eventBus.UnsubscribeAll<ListingCreatedEvent>();
```
