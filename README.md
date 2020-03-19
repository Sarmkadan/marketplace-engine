# Marketplace Engine

## MarketplaceConfiguration

The `MarketplaceConfiguration` class provides centralized configuration settings for the Marketplace Engine, including cache management, API rate limiting, background job scheduling, and dropshipping integration. It consolidates common application parameters into a single, strongly-typed configuration object.

### Usage Example

```csharp
using MarketplaceEngine.Infrastructure.Configuration;

var config = new MarketplaceConfiguration
{
    // Cache settings
    DefaultTtlMinutes = 30,
    MaxCacheSizeMb = 512,
    ListingCacheTtlMinutes = 15,
    UserCacheTtlMinutes = 60,
    CategoryCacheTtlMinutes = 120,
    SearchResultCacheTtlMinutes = 10,
    
    // Rate limiting
    MaxRequestsPerMinute = 1000,
    MaxRequestsPerHour = 60000,
    ExemptPaths = new[] { "/api/health", "/api/status" },
    
    // Background jobs
    PollingIntervalMs = 5000,
    MaxConcurrentJobs = 10,
    JobTimeoutSeconds = 300,
    
    // Dropshipping integration
    DropshipApiBaseUrl = "https://api.dropship.example.com",
    DropshipApiKey = "your-api-key-here",
    ApiTimeoutSeconds = 30,
    MaxRetries = 3,
    RetryDelayMs = 1000
};

Console.WriteLine($"Cache TTL: {config.DefaultTtlMinutes} minutes");
Console.WriteLine($"Max cache size: {config.MaxCacheSizeMb} MB");
```

## IBackgroundJob

The `IBackgroundJob` interface defines a contract for background jobs that can be executed asynchronously. It provides a way to enqueue jobs, start the background worker thread, and stop the worker thread.

### Usage Example

```csharp
using MarketplaceEngine.Infrastructure.Background;

var job = new SearchIndexingJob();
job.ExecuteAsync().Wait();

var queue = new BackgroundJobQueue();
queue.Enqueue(job);
queue.Start();
queue.StopAsync().Wait();

Console.WriteLine($"Queue size: {queue.GetQueueSize()}");
```

## CacheService

The `CacheService` class provides an in-memory caching implementation for the Marketplace Engine. It supports asynchronous get/set operations with configurable expiration, cache statistics tracking, and bulk operations for cache management. The service is designed to reduce database load and improve API response times by caching frequently accessed data.

### Usage Example

```csharp
using MarketplaceEngine.Infrastructure.Caching;
using System;

// Initialize cache service with default settings
var cacheService = new CacheService("MarketplaceCache");

// Store and retrieve a product listing
var productId = "prod-12345";
var cacheKey = $"product:{productId}";

// Set a product in cache with 5 minute expiration
await cacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(5));

// Retrieve the product from cache
var cachedProduct = await cacheService.GetAsync<Product>(cacheKey);

if (cachedProduct != null)
{
    Console.WriteLine($"Retrieved product {cachedProduct.Name} from cache");
}
else
{
    Console.WriteLine("Product not found in cache");
}

// Remove a specific item from cache
await cacheService.RemoveAsync(cacheKey);

// Clear all items from cache
await cacheService.ClearAsync();

// Get cache statistics
var stats = await cacheService.GetStatisticsAsync();
Console.WriteLine($"Cache contains {stats.TotalItems} items, using {stats.TotalMemoryMb} MB");
```

## IOutputFormatter

The `IOutputFormatter` interface defines a contract for formatting marketplace listings into different output formats such as JSON, CSV, and XML. It provides methods to format both individual listings and collections of listings, enabling flexible data export and API response generation.

### Usage Example

```csharp
using MarketplaceEngine.Infrastructure.Formatters;
using MarketplaceEngine.DTOs;
using System;
using System.Collections.Generic;

// Create sample listings
var listings = new List<ListingDto>
{
    new ListingDto
    {
        Id = 1,
        Title = "Premium Widget",
        Description = "High quality widget for all purposes",
        Price = 29.99m,
        SellerId = 101,
        SellerName = "Acme Corp",
        CategoryId = 5,
        Status = "Active",
        ViewCount = 150,
        CreatedAt = DateTime.UtcNow
    },
    new ListingDto
    {
        Id = 2,
        Title = "Basic Gadget",
        Description = "Simple gadget for everyday use",
        Price = 9.99m,
        SellerId = 102,
        SellerName = "Tech Solutions",
        CategoryId = 3,
        Status = "Active",
        ViewCount = 75,
        CreatedAt = DateTime.UtcNow.AddDays(-1)
    }
};

// Create formatter factory
var factory = new FormatterFactory();

// Get a JSON formatter
var jsonFormatter = factory.GetFormatter(OutputFormat.Json);

// Format a single listing
var singleListingJson = jsonFormatter.FormatListing(listings[0]);
Console.WriteLine("Single listing (JSON):");
Console.WriteLine(singleListingJson);

// Format multiple listings
var multipleListingsJson = jsonFormatter.FormatListings(listings);
Console.WriteLine("\nMultiple listings (JSON):");
Console.WriteLine(multipleListingsJson);

// Get a CSV formatter
var csvFormatter = factory.GetFormatter(OutputFormat.Csv);
var csvOutput = csvFormatter.FormatListings(listings);
Console.WriteLine("\nMultiple listings (CSV):");
Console.WriteLine(csvOutput);

// Get an XML formatter
var xmlFormatter = factory.GetFormatter(OutputFormat.Xml);
var xmlOutput = xmlFormatter.FormatListings(listings);
Console.WriteLine("\nMultiple listings (XML):");
Console.WriteLine(xmlOutput);
```

## RecommendationOptions

The `RecommendationOptions` class provides configuration settings for the collaborative filtering recommendation engine. It controls parameters for user similarity calculations, trending algorithms, caching behavior, activity tracking limits, and feature flags that determine how personalized and diverse recommendation feeds should be. All settings can be overridden via the `Marketplace:Recommendations` section of `appsettings.json`.

### Usage Example

```csharp
using MarketplaceEngine.Recommendations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

// Create with default values
var options = RecommendationOptions.CreateDefault();

// Configure collaborative filtering
options.MinOverlapForNeighbour = 5;
options.MaxNeighbours = 100;
options.MinSimilarityThreshold = 0.15;

// Configure trending window and signal weights
options.TrendingWindowHours = 72;
options.ViewWeight = 1.0;
options.SaveWeight = 4.0;
options.EnquiryWeight = 6.0;
options.PurchaseWeight = 12.0;

// Configure caching durations
options.UserFeedCacheTtlMinutes = 10;
options.TrendingFeedCacheTtlMinutes = 15;
options.ItemSimilarityCacheTtlMinutes = 60;

// Configure activity tracking limits
options.MaxSignalsPerUser = 1000;
options.ActivityHistoryDays = 180;
options.MinAffinitySignals = 5;

// Configure feature flags
options.EnablePersonalisation = true;
options.EnableDiversification = true;
options.MaxCategoryConcentration = 0.35;

// Create from configuration (ASP.NET Core style)
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
var configuredOptions = RecommendationOptions.FromConfiguration(configuration);

// Register with DI container
var services = new ServiceCollection();
services.Configure<RecommendationOptions>(configuration.GetSection("Marketplace:Recommendations"));

Console.WriteLine($"Recommendation settings: MinOverlap={options.MinOverlapForNeighbour}, " +
                $"MaxNeighbours={options.MaxNeighbours}, " +
                $"TrendingWindow={options.TrendingWindowHours}h");
```

## UserActivityTracker

The `UserActivityTracker` class provides an in-memory store for tracking user activity signals across marketplace listings. It maintains chronological interaction histories for individual users and a reverse index mapping listings to their audiences, enabling both user-based and item-based collaborative filtering scenarios. The tracker is designed for single-instance deployments and includes configurable signal retention limits.

### Usage Example

```csharp
using MarketplaceEngine.Recommendations;
using System;

// Initialize activity tracker with recommendation options
var options = new RecommendationOptions
{
    MaxSignalsPerUser = 1000,
    SignalRetentionWindow = TimeSpan.FromDays(30)
};
var tracker = new UserActivityTracker(options, logger);

// Record user interactions with listings
var userId = Guid.NewGuid();
var listingId = Guid.NewGuid();

var viewSignal = new UserActivitySignal
{
    UserId = userId,
    ListingId = listingId,
    SignalType = UserActivityType.View,
    OccurredAt = DateTime.UtcNow
};

var purchaseSignal = new UserActivitySignal
{
    UserId = userId,
    ListingId = listingId,
    SignalType = UserActivityType.Purchase,
    OccurredAt = DateTime.UtcNow.AddSeconds(-30)
};

await tracker.RecordAsync(viewSignal);
await tracker.RecordAsync(purchaseSignal);

// Retrieve user interaction history
var userHistory = await tracker.GetUserHistoryAsync(userId);
Console.WriteLine($"User has {userHistory.Count} recorded interactions");

// Get audience for a specific listing
var audience = await tracker.GetListingAudienceAsync(listingId);
Console.WriteLine($"Listing has {audience.Count} unique viewers");

// Retrieve signals within a specific time window
var recentSignals = await tracker.GetSignalsInWindowAsync(TimeSpan.FromHours(1));
Console.WriteLine($"Found {recentSignals.Count} signals in the last hour");
```

## MarketplaceException

The `MarketplaceException` class is the base exception type for all marketplace-related errors in the Marketplace Engine. It extends the standard `Exception` class with additional properties for error tracking and validation error handling, making it ideal for scenarios like API validation failures, business rule violations, and service-level errors.

### Usage Example

```csharp
using MarketplaceEngine.Exceptions;
using System;
using System.Collections.Generic;

// Create a simple marketplace exception
var exception = new MarketplaceException("Product not found", "PRODUCT_NOT_FOUND");
Console.WriteLine($"Error: {exception.Message}, Code: {exception.ErrorCode}");

// Create an exception with validation errors
var validationErrors = new Dictionary<string, string[]>
{
    { "Price", new[] { "Price must be greater than 0", "Price must be a valid decimal" } },
    { "Title", new[] { "Title is required", "Title must be at least 5 characters" } }
};
var validationException = new MarketplaceException(
    "Product validation failed",
    validationErrors,
    "VALIDATION_ERROR"
);
Console.WriteLine($"Validation errors: {validationException.ValidationErrors?.Count}");

// Create an exception with inner exception
try
{
    // Some operation that can fail
}
catch (Exception ex)
{
    var wrappedException = new MarketplaceException(
        "Failed to process product listing",
        ex,
        "PROCESSING_ERROR"
    );
}

// Create exception with context using the static factory method
var contextException = MarketplaceException.CreateWithContext(
    "Invalid category specified",
    "category:electronics"
);
Console.WriteLine($"Context exception: {contextException.Message}");
```

...