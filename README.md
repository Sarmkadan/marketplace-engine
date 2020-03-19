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

...