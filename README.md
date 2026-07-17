# Marketplace Engine

## Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for an overview of the system:
project layout, the in-memory storage model, the DI/composition root, middleware
pipeline, event bus, background jobs, extension points and known limitations.


## Rating

The `Rating` class is an immutable value object that represents a user or listing rating. It encapsulates the score and review count, providing methods to calculate a weighted average rating and add new reviews while maintaining data integrity.

### Usage Example

```csharp
using MarketplaceEngine.Domain.ValueObjects;
using System;

// Initialize a new rating for a listing or user
var rating = new Rating(score: 5, totalReviews: 10);

// Add a new review score
var updatedRating = rating.AddReview(4);

Console.WriteLine($"Original Rating: {rating}");
Console.WriteLine($"Updated Rating: {updatedRating}");
Console.WriteLine($"Average: {updatedRating.AverageRating:F2}");
Console.WriteLine($"Score: {updatedRating.Score}");
Console.WriteLine($"Total Reviews: {updatedRating.TotalReviews}");

// Compare ratings
var anotherRating = new Rating(5, 10);
Console.WriteLine($"Ratings are equal: {rating.Equals(anotherRating)}");
```

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

## SellerDashboardDto

The `SellerDashboardDto` class provides a comprehensive overview of a seller's performance and activity within the marketplace. It aggregates key metrics such as active listings, sales performance, revenue breakdown, customer feedback, and communication status to give sellers actionable insights into their business health and growth opportunities.

### Usage Example

```csharp
using MarketplaceEngine.DTOs;
using System;
using System.Collections.Generic;

// Create a seller dashboard for a successful seller
var dashboard = new SellerDashboardDto
{
    SellerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
    SellerName = "Acme Corp",
    ActiveListings = 42,
    TotalListings = 87,
    TotalSales = 156,
    TotalRevenue = 12456.78m,
    TotalGrossRevenue = 14567.89m,
    TotalPlatformFees = 2111.11m,
    TotalNetRevenue = 12456.78m,
    PendingPayout = 892.34m,
    AverageRating = 4.7,
    TotalReviews = 234,
    UnreadMessages = 3,
    LastActivityAt = DateTime.UtcNow.AddHours(-2),
    MonthlyBreakdown = new List<MonthlyRevenueDto>
    {
        new MonthlyRevenueDto { Year = 2024, Month = 6, GrossRevenue = 4500.00m },
        new MonthlyRevenueDto { Year = 2024, Month = 5, GrossRevenue = 5200.50m },
        new MonthlyRevenueDto { Year = 2024, Month = 4, GrossRevenue = 3800.75m }
    }
};

Console.WriteLine($"Seller Dashboard for: {dashboard.SellerName}");
Console.WriteLine($"Performance: {dashboard.ActiveListings} active listings | {dashboard.TotalSales} sales");
Console.WriteLine($"Revenue: ${dashboard.TotalRevenue:N2} total | ${dashboard.PendingPayout:N2} pending");
Console.WriteLine($"Rating: {dashboard.AverageRating:F1}/5 ({dashboard.TotalReviews} reviews)");
Console.WriteLine($"Messages: {dashboard.UnreadMessages} unread");
Console.WriteLine($"Last activity: {dashboard.LastActivityAt?.ToString("yyyy-MM-dd HH:mm")}");

// Display monthly breakdown
Console.WriteLine("\nMonthly Revenue Breakdown:");
foreach (var month in dashboard.MonthlyBreakdown.OrderByDescending(m => m.Year).ThenByDescending(m => m.Month))
{
    Console.WriteLine($"  {new DateTime(month.Year, month.Month, 1):yyyy MMM}: ${month.GrossRevenue:N2}");
}
```

## RecommendationDto

The `RecommendationDto` class represents a single recommendation item in the marketplace recommendation system. It encapsulates all essential information about a recommended listing including the listing details, recommendation score, reasoning, and metadata about the recommendation strategy and generation time. This DTO is used to serialize recommendation results for API responses and client consumption.

### Usage Example

```csharp
using MarketplaceEngine.DTOs;
using System;
using System.Collections.Generic;

// Create a single recommendation for a user
var recommendation = new RecommendationDto
{
    ListingId = Guid.NewGuid(),
    Title = "Wireless Bluetooth Headphones",
    Price = 129.99m,
    Currency = "USD",
    ThumbnailUrl = "https://example.com/images/headphones.jpg",
    Score = 0.92,
    Reason = "Based on your recent purchase of premium audio equipment",
    CategoryId = Guid.NewGuid(),
    SellerId = Guid.NewGuid(),
    UserId = Guid.NewGuid(),
    BasedOnListingId = Guid.NewGuid(),
    Count = 1,
    Strategy = RecommendationStrategy.CollaborativeFiltering,
    IsPersonalised = true,
    GeneratedAt = DateTime.UtcNow
};

Console.WriteLine($"Recommendation: {recommendation.Title}");
Console.WriteLine($"Score: {recommendation.Score:P1}");
Console.WriteLine($"Reason: {recommendation.Reason}");
Console.WriteLine($"Strategy: {recommendation.Strategy}");

// Create a collection of recommendations
var recommendations = new List<RecommendationDto>
{
    new RecommendationDto
    {
        ListingId = Guid.NewGuid(),
        Title = "Smart Watch with Heart Rate Monitor",
        Price = 199.99m,
        Currency = "USD",
        ThumbnailUrl = "https://example.com/images/smartwatch.jpg",
        Score = 0.87,
        Reason = "Similar customers also purchased this item",
        CategoryId = Guid.NewGuid(),
        SellerId = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        Count = 1,
        Strategy = RecommendationStrategy.ItemSimilarity,
        IsPersonalised = true,
        GeneratedAt = DateTime.UtcNow
    },
    new RecommendationDto
    {
        ListingId = Guid.NewGuid(),
        Title = "Portable Bluetooth Speaker",
        Price = 89.99m,
        Currency = "USD",
        ThumbnailUrl = "https://example.com/images/speaker.jpg",
        Score = 0.78,
        Reason = "Trending in your category",
        CategoryId = Guid.NewGuid(),
        SellerId = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        Count = 1,
        Strategy = RecommendationStrategy.Trending,
        IsPersonalised = true,
        GeneratedAt = DateTime.UtcNow
    }
};

Console.WriteLine($"Generated {recommendations.Count} recommendations");
Console.WriteLine($"Total estimated value: {recommendations.Sum(r => r.Price):C}");
```

## ReviewDto

The `ReviewDto` class is a data transfer object that represents a review submitted by a user about a seller or listing. It contains all essential information about the review including the reviewer details, rating score, comment text, status, timestamps, and optional seller reply. This DTO is used in API responses to display review information to users and calculate seller ratings.

### Usage Example

```csharp
using MarketplaceEngine.DTOs;
using System;

// Create a review DTO for a seller
var reviewDto = new ReviewDto
{
    Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
    ReviewerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
    ReviewerName = "John Doe",
    SellerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
    ListingId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
    Score = 5,
    Comment = "Excellent seller! Fast shipping and great communication.",
    Status = "Approved",
    SellerReply = "Thank you for your positive review! We appreciate your business.",
    CreatedAt = DateTime.UtcNow.AddDays(-2),
    UpdatedAt = DateTime.UtcNow.AddHours(-1)
};

// Display review information
Console.WriteLine($"Review ID: {reviewDto.Id}");
Console.WriteLine($"Reviewer: {reviewDto.RevieweeName} ({reviewDto.RevieweeId})");
Console.WriteLine($"Score: {reviewDto.Score}/5");
Console.WriteLine($"Comment: {reviewDto.Comment}");
Console.WriteLine($"Status: {reviewDto.Status}");
Console.WriteLine($"Created: {reviewDto.CreatedAt:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"Last updated: {reviewDto.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never updated"}");
Console.WriteLine($"Seller reply: {reviewDto.SellerReply ?? "None"}");

// Create a collection of reviews for a seller
var sellerReviews = new List<ReviewDto>
{
    new ReviewDto
    {
        Id = Guid.NewGuid(),
        ReviewerId = Guid.NewGuid(),
        ReviewerName = "Alice Johnson",
        SellerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        ListingId = Guid.NewGuid(),
        Score = 5,
        Comment = "Perfect transaction! Item exactly as described.",
        Status = "Approved",
        CreatedAt = DateTime.UtcNow.AddDays(-5)
    },
    new ReviewDto
    {
        Id = Guid.NewGuid(),
        ReviewerId = Guid.NewGuid(),
        ReviewerName = "Bob Smith",
        SellerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        ListingId = Guid.NewGuid(),
        Score = 4,
        Comment = "Good seller, but shipping took longer than expected.",
        Status = "Approved",
        CreatedAt = DateTime.UtcNow.AddDays(-3)
    },
    new ReviewDto
    {
        Id = Guid.NewGuid(),
        ReviewerId = Guid.NewGuid(),
        ReviewerName = "Charlie Brown",
        SellerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        ListingId = null,  // Review for seller without specific listing
        Score = 3,
        Comment = "Average experience. Item met basic expectations.",
        Status = "Approved",
        CreatedAt = DateTime.UtcNow.AddDays(-1)
    }
};

// Calculate average rating from reviews
var averageScore = sellerReviews.Average(r => r.Score);
Console.WriteLine($"\nAverage rating: {averageScore:F1}/5 stars");
Console.WriteLine($"Total reviews: {sellerReviews.Count}");
```

## ModerationReportDto

The `ModerationReportDto` class is a basic data transfer object that represents a moderation report submitted by users to flag inappropriate content or user behavior. It contains essential information about the report including identifiers for the reported content/user, the reporter, the reason for reporting, and creation timestamp. This DTO is used in API responses to display moderation reports in lists and dashboards.

### Usage Example

```csharp
using MarketplaceEngine.DTOs;
using System;

// Create a moderation report DTO for a flagged listing
var reportDto = new ModerationReportDto
{
    Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
    ListingId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
    UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
    ReporterUserId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
    Reason = "This listing contains inappropriate content",
    Status = "Pending",
    CreatedAt = DateTime.UtcNow.AddHours(-2)
};

// Display report information
Console.WriteLine($"Report ID: {reportDto.Id}");
Console.WriteLine($"Reported Listing: {reportDto.ListingId}");
Console.WriteLine($"Reported User: {reportDto.UserId}");
Console.WriteLine($"Reported by: {reportDto.ReporterUserId}");
Console.WriteLine($"Reason: {reportDto.Reason}");
Console.WriteLine($"Status: {reportDto.Status}");
Console.WriteLine($"Created: {reportDto.CreatedAt:yyyy-MM-dd HH:mm:ss}");

// Create a collection of moderation reports for a dashboard
var reports = new List<ModerationReportDto>
{
    new ModerationReportDto
    {
        Id = Guid.NewGuid(),
        ListingId = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        ReporterUserId = Guid.NewGuid(),
        Reason = "Spam content detected",
        Status = "Pending",
        CreatedAt = DateTime.UtcNow.AddMinutes(-30)
    },
    new ModerationReportDto
    {
        Id = Guid.NewGuid(),
        ListingId = Guid.NewGuid(),
        UserId = null,
        ReporterUserId = Guid.NewGuid(),
        Reason = "Seller not responding to messages",
        Status = "InReview",
        CreatedAt = DateTime.UtcNow.AddHours(-1)
    },
    new ModerationReportDto
    {
        Id = Guid.NewGuid(),
        ListingId = null,
        UserId = Guid.NewGuid(),
        ReporterUserId = Guid.NewGuid(),
        Reason = "User making inappropriate comments",
        Status = "Approved",
        CreatedAt = DateTime.UtcNow.AddDays(-1)
    }
};

// Display dashboard statistics
var pendingCount = reports.Count(r => r.Status == "Pending");
var inReviewCount = reports.Count(r => r.Status == "InReview");
Console.WriteLine($"\nModeration Dashboard:");
Console.WriteLine($"Total reports: {reports.Count}");
Console.WriteLine($"Pending: {pendingCount}");
Console.WriteLine($"In review: {inReviewCount}");
```

## MessageDto

The `MessageDto` class is a data transfer object used for API responses when retrieving messages between users in the marketplace. It represents a simplified view of a message containing the message content, sender and recipient information, read status, and creation timestamp. This DTO is commonly used in messaging APIs to provide a clean, serialized format for message data.

### Usage Example

```csharp
using MarketplaceEngine.DTOs;
using System;

// Create a message DTO for a conversation
var messageDto = new MessageDto
{
    Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
    SenderId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
    RecipientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
    Content = "Hello! I'm interested in your listing. Could you provide more details about the condition?",
    IsRead = false,
    CreatedAt = DateTime.UtcNow.AddMinutes(-30)
};

// Display message information
Console.WriteLine($"Message ID: {messageDto.Id}");
Console.WriteLine($"From: {messageDto.SenderId}");
Console.WriteLine($"To: {messageDto.RecipientId}");
Console.WriteLine($"Content: {messageDto.Content}");
Console.WriteLine($"Status: {(messageDto.IsRead ? "Read" : "Unread")}");
Console.WriteLine($"Sent: {messageDto.CreatedAt:yyyy-MM-dd HH:mm:ss}");

// Create a collection of message DTOs for a conversation thread
var conversation = new List<MessageDto>
{
    new MessageDto
    {
        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        SenderId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        RecipientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        Content = "Hi there! I have a question about your item.",
        IsRead = true,
        CreatedAt = DateTime.UtcNow.AddHours(-2)
    },
    new MessageDto
    {
        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        SenderId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        RecipientId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Content = "Sure! What would you like to know?",
        IsRead = true,
        CreatedAt = DateTime.UtcNow.AddHours(-1)
    },
    messageDto // The new message
};

Console.WriteLine($"\nConversation contains {conversation.Count} messages");
Console.WriteLine($"Unread messages: {conversation.Count(m => !m.IsRead)}");
```

## ConversationDto

The `ConversationDto` class provides a summary view of a conversation between a user and another participant in the marketplace. It includes information about the other user, the last message in the conversation, and the count of unread messages. This DTO is useful for displaying conversation previews in user interfaces and listing conversations in a compact format.

### Usage Example

```csharp
using MarketplaceEngine.DTOs;
using System;

// Create a conversation summary
var conversationDto = new ConversationDto
{
    ConversationId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
    OtherUserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
    OtherUserName = "Jane Smith",
    LastMessage = "I'm interested in your bike. Is it still available?",
    LastMessageAt = DateTime.UtcNow.AddMinutes(-15),
    UnreadCount = 1
};

// Display conversation information
Console.WriteLine($"Conversation with: {conversationDto.OtherUserName}");
Console.WriteLine($"Last message: {conversationDto.LastMessage}");
Console.WriteLine($"Last activity: {conversationDto.LastMessageAt:yyyy-MM-dd HH:mm}");
Console.WriteLine($"Unread messages: {conversationDto.UnreadCount}");
```

