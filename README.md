# Marketplace Engine

## Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for an overview of the system:
project layout, the in-memory storage model, the DI/composition root, middleware
pipeline, event bus, background jobs, extension points and known limitations.


## MessageRepository

The `MessageRepository` class provides data access operations for message entities in the marketplace system. It handles CRUD operations for messages, conversation management, and various query methods for retrieving messages by different criteria including sender/recipient, listing context, read status, and pagination with both offset and cursor-based approaches.

### Usage Example

```csharp
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Domain.Entities;
using System;
using System.Threading.Tasks;

// Initialize message repository
var messageRepository = new MessageRepository();

// Add a new message to the system
var newMessage = await messageRepository.AddAsync(new Message
{
    SenderId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
    RecipientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
    Subject = "Interested in your listing",
    Content = "Hello! I'm interested in your premium wireless headphones. Could you provide more details about the condition and shipping options?",
    ListingId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
    IsRead = false,
    Status = MessageStatus.Sent,
    CreatedAt = DateTime.UtcNow
});

Console.WriteLine($"Message created: {newMessage.Id}");

// Get a message by ID
var retrievedMessage = await messageRepository.GetByIdAsync(newMessage.Id);
Console.WriteLine($"Retrieved message: {retrievedMessage.Subject}");

// Update a message status
var updatedMessage = await messageRepository.UpdateAsync(new Message
{
    Id = newMessage.Id,
    SenderId = newMessage.SenderId,
    RecipientId = newMessage.RecipientId,
    Subject = newMessage.Subject,
    Content = newMessage.Content,
    ListingId = newMessage.ListingId,
    IsRead = true, // Mark as read
    Status = MessageStatus.Delivered,
    CreatedAt = newMessage.CreatedAt,
    UpdatedAt = DateTime.UtcNow
});
Console.WriteLine($"Message marked as read: {updatedMessage.IsRead}");

// Get all messages (with optional filtering)
var allMessages = await messageRepository.GetAllAsync();
Console.WriteLine($"Total messages in system: {allMessages.Count}");

// Get received messages for a user
var receivedMessages = await messageRepository.GetReceivedMessagesAsync(Guid.Parse("22222222-2222-2222-2222-222222222222"));
Console.WriteLine($"User received {receivedMessages.Count} messages");

// Get sent messages for a user
var sentMessages = await messageRepository.GetSentMessagesAsync(Guid.Parse("11111111-1111-1111-1111-111111111111"));
Console.WriteLine($"User sent {sentMessages.Count} messages");

// Get unread messages for a user
var unreadMessages = await messageRepository.GetUnreadMessagesAsync(Guid.Parse("22222222-2222-2222-2222-222222222222"));
Console.WriteLine($"User has {unreadMessages.Count} unread messages");

// Get conversation between two users
var conversation = await messageRepository.GetConversationAsync(
    Guid.Parse("11111111-1111-1111-1111-111111111111"),
    Guid.Parse("22222222-2222-2222-2222-222222222222")
);
Console.WriteLine($"Conversation contains {conversation.Count} messages");

// Get messages about a specific listing
var listingMessages = await messageRepository.GetByListingIdAsync(Guid.Parse("33333333-3333-3333-3333-333333333333"));
Console.WriteLine($"Listing has {listingMessages.Count} messages");

// Get conversation about a specific listing between two users
var listingConversation = await messageRepository.GetConversationAboutListingAsync(
    Guid.Parse("11111111-1111-1111-1111-111111111111"),
    Guid.Parse("22222222-2222-2222-2222-222222222222"),
    Guid.Parse("33333333-3333-3333-3333-333333333333")
);
Console.WriteLine($"Listing conversation contains {listingConversation.Count} messages");

// Check if a message exists
var exists = await messageRepository.ExistsAsync(newMessage.Id);
Console.WriteLine($"Message exists: {exists}");

// Get total message count
var messageCount = await messageRepository.CountAsync();
Console.WriteLine($"Total messages: {messageCount}");

// Get conversation count for a user
var conversationCount = await messageRepository.GetConversationCountAsync(Guid.Parse("22222222-2222-2222-2222-222222222222"));
Console.WriteLine($"User has {conversationCount} conversations");

// Get flagged messages (inappropriate content)
var flaggedMessages = await messageRepository.GetFlaggedMessagesAsync();
Console.WriteLine($"System has {flaggedMessages.Count} flagged messages");

// Mark a message as read
await messageRepository.MarkAsReadAsync(newMessage.Id);
Console.WriteLine("Message marked as read");

// Get old messages for cleanup
var oldMessages = await messageRepository.GetOldMessagesAsync(DateTime.UtcNow.AddMonths(-6));
Console.WriteLine($"Found {oldMessages.Count} messages older than 6 months");

// Get paginated messages with total count
var (pagedMessages, totalCount) = await messageRepository.GetPagedAsync(
    page: 1,
    pageSize: 25
);
Console.WriteLine($"Page 1: {pagedMessages.Count} of {totalCount} total messages");

// Get paginated messages with cursor
var (cursorMessages, nextCursor) = await messageRepository.GetPagedByCursorAsync(
    pageSize: 25
);
Console.WriteLine($"Retrieved {cursorMessages.Count} messages with cursor: {nextCursor}");

// Delete a message
await messageRepository.DeleteAsync(newMessage.Id);
Console.WriteLine("Message deleted successfully");
```

## MarketplaceDbContext

The `MarketplaceDbContext` class serves as the in-memory database context, acting as the central storage for all marketplace entities such as users, categories, listings, messages, and payments. It implements the singleton pattern to ensure consistent data access throughout the application lifecycle and provides methods for data management, including clearing all entities or resetting to the default seeded state.

### Usage Example

```csharp
using MarketplaceEngine.Data;
using System;

// Get the singleton instance of the database context
var dbContext = MarketplaceDbContext.GetInstance();

// Access entities
var userCount = dbContext.Users.Count;
var listingCount = dbContext.Listings.Count;

Console.WriteLine($"Current user count: {userCount}");
Console.WriteLine($"Current listing count: {listingCount}");

// Get total count of all entities in the database
var totalEntities = dbContext.GetTotalEntityCount();
Console.WriteLine($"Total entity count: {totalEntities}");

// Clear all data from the database
dbContext.Clear();
Console.WriteLine($"Entities cleared. New total: {dbContext.GetTotalEntityCount()}");

// Reset the database to initial seeded data
dbContext.Reset();
Console.WriteLine($"Database reset. New total: {dbContext.GetTotalEntityCount()}");
```

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

## SearchService

The `SearchService` class provides comprehensive search functionality for the Marketplace Engine, enabling users to find listings, users, and categories through various search methods. It supports full-text search, tag-based search, category filtering, geospatial queries, and advanced search capabilities with pagination and sorting options. The service integrates with the recommendation system to provide personalized search results based on user preferences and behavior.

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Initialize search service (typically via dependency injection)
var searchService = new SearchService(listingRepository, userRepository, categoryRepository);

// Basic search for listings by keyword
var keywordResults = await searchService.SearchListingsAsync("wireless headphones");
Console.WriteLine($"Found {keywordResults.Count} listings matching 'wireless headphones'");

// Search listings by tags
var tagResults = await searchService.SearchByTagsAsync(new List<string> { "electronics", "audio", "bluetooth" });
Console.WriteLine($"Found {tagResults.Count} listings with specified tags");

// Find nearby listings (requires geospatial data)
var nearbyResults = await searchService.FindNearbyListingsAsync(
    latitude: 40.7128,
    longitude: -74.0060,
    radiusKm: 5.0
);
Console.WriteLine($"Found {nearbyResults.Count} listings within 5km radius");

// Search for users by name or email
var userResults = await searchService.SearchUsersAsync("John Doe");
Console.WriteLine($"Found {userResults.Count} users matching 'John Doe'");

// Get top sellers by rating
var topSellers = await searchService.GetTopSellersAsync(count: 10);
Console.WriteLine($"Top {topSellers.Count} sellers by rating:");
foreach (var seller in topSellers.Take(5))
{
    Console.WriteLine($"- {seller.DisplayName}: {seller.AverageRating:F1}/5 ({seller.ReviewCount} reviews)");
}

// Search listings by category with pagination
var (categoryResults, totalCount) = await searchService.SearchByCategoryAsync(
    categoryId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
    page: 1,
    pageSize: 25
);
Console.WriteLine($"Page 1: {categoryResults.Count} of {totalCount} listings in category");

// Advanced search with multiple criteria
var advancedResults = await searchService.AdvancedSearchAsync(new FullTextSearchRequest
{
    Query = "laptop",
    MinPrice = 500,
    MaxPrice = 2000,
    CategoryId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
    Tags = new List<string> { "electronics", "computers" },
    Page = 1,
    PageSize = 10
});
Console.WriteLine($"Advanced search found {advancedResults.items.Count} listings");

// Get trending listings (based on recent activity)
var trendingListings = await searchService.GetTrendingListingsAsync(count: 15);
Console.WriteLine($"Found {trendingListings.Count} trending listings");

// Get search suggestions based on popular queries
var suggestions = await searchService.GetSearchSuggestionsAsync("wire");
Console.WriteLine($"Search suggestions: {string.Join(", ", suggestions)}");
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

## RecommendationService

The `RecommendationService` class is the primary application-layer service that orchestrates the recommendation engine to deliver personalised listing feeds, similar-item panels, trending galleries, and category-affinity feeds. It acts as the single entry point for all recommendation concerns in the API layer, handling input validation, existence checks, result hydration, diversity enforcement, and event publication.

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.DTOs;
using MarketplaceEngine.Exceptions;
using System;
using System.Threading.Tasks;

// Initialize recommendation service (typically via dependency injection)
// var recommendationService = new RecommendationService(...);

// Example: Get personalised recommendations for a user
try
{
    var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var recommendations = await recommendationService.GetRecommendationsForUserAsync(userId, count: 15);
    
    Console.WriteLine($"Generated {recommendations.Items.Count} recommendations");
    Console.WriteLine($"Is personalised: {recommendations.IsPersonalised}");
    Console.WriteLine($"Strategy used: {recommendations.Strategy}");
    Console.WriteLine($"Generated at: {recommendations.GeneratedAt}");
    
    foreach (var item in recommendations.Items.Take(5))
    {
        Console.WriteLine($"- {item.Title}: {item.Price:C} (Score: {item.Score:P1})");
    }
}
catch (ResourceNotFoundException ex)
{
    Console.WriteLine($"User not found: {ex.Message}");
}

// Example: Get trending listings (public feed)
var trendingFeed = await recommendationService.GetTrendingListingsAsync(count: 20);
Console.WriteLine($"\nTrending feed contains {trendingFeed.Items.Count} items");

// Example: Get similar listings for a specific listing
var listingId = Guid.Parse("33333333-3333-3333-3333-333333333333");
var similarFeed = await recommendationService.GetSimilarListingsAsync(listingId, count: 8);
Console.WriteLine($"Similar items panel contains {similarFeed.Items.Count} items");

// Example: Get category-affinity recommendations
var affinityFeed = await recommendationService.GetAffinityRecommendationsAsync(userId, count: 12);
Console.WriteLine($"Category-affinity feed contains {affinityFeed.Items.Count} items");

// Example: Track user activity to improve future recommendations
await recommendationService.TrackUserActivityAsync(
    userId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    listingId: Guid.Parse("33333333-3333-3333-3333-333333333333"),
    signalType: SignalType.View
);
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

## SavedSearchCriteria

The `SavedSearchCriteria` class represents a user's saved search subscription that monitors new listings matching specific criteria. It stores search parameters like keywords, category, price limits, and tags to automatically match against new listings and notify users when matching items are added. Saved searches are persisted in memory and can be executed against the current active listings.

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Create a saved search for electronics under $500 with "wireless" in title/description
var searchCriteria = new SavedSearchCriteria
{
    UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
    Keywords = "wireless",
    CategoryId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), // Electronics
    MaxPrice = 500,
    Tags = new List<string> { "electronics", "audio", "bluetooth" },
    CreatedAt = DateTime.UtcNow
};

// Save the search subscription
var savedSearchAlertService = new SavedSearchAlertService(listingRepository);
var savedCriteria = savedSearchAlertService.Save(searchCriteria);

Console.WriteLine($"Saved search created: {savedCriteria.Id}");
Console.WriteLine($"User: {savedCriteria.UserId}");
Console.WriteLine($"Keywords: {savedCriteria.Keywords}");
Console.WriteLine($"Category: {savedCriteria.CategoryId}");
Console.WriteLine($"Max Price: {savedCriteria.MaxPrice:C}");
Console.WriteLine($"Tags: {string.Join(", ", savedCriteria.Tags)}");
Console.WriteLine($"Created: {savedCriteria.CreatedAt:yyyy-MM-dd}");

// Execute the search to find matching listings
var matchingListings = await savedSearchAlertService.ExecuteAsync(savedCriteria.Id);
Console.WriteLine($"\nFound {matchingListings.Count} matching listings");

// Get all saved searches for a user
var userSearches = savedSearchAlertService.GetForUser(Guid.Parse("11111111-1111-1111-1111-111111111111"));
Console.WriteLine($"\nUser has {userSearches.Count} saved searches");

// Remove a saved search
var isRemoved = savedSearchAlertService.Remove(savedCriteria.Id, 
    Guid.Parse("11111111-1111-1111-1111-111111111111"));
Console.WriteLine($"\nSearch removed: {isRemoved}");
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

## ReviewService

The `ReviewService` class provides essential functionality for managing user and listing reviews within the marketplace. It enables users to submit, retrieve, flag, and remove reviews, while also offering analytical insights like average scores and review distribution for sellers. This service acts as the central hub for all review-related operations in the application layer.

## SearchServiceTests

The `SearchServiceTests` class provides unit tests for the `SearchService` class, validating its search functionality across listings and users. It tests various search methods including full-text search, tag-based search, geospatial queries, category filtering, and advanced search capabilities with validation for input parameters and proper delegation to repository methods.

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Entities;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Initialize mock repositories
var listingRepoMock = new Mock<IListingRepository>();
var userRepoMock = new Mock<IUserRepository>();

// Create the service under test
var searchService = new SearchService(listingRepoMock.Object, userRepoMock.Object);

// Test 1: SearchListingsAsync throws ValidationException when provided with an empty search query
var act1 = async () => await searchService.SearchListingsAsync(" ");
await Assert.ThrowsAsync<ValidationException>(act1);

// Test 2: SearchListingsAsync throws ValidationException when provided with a single-character search query
var act2 = async () => await searchService.SearchListingsAsync("x");
await Assert.ThrowsAsync<ValidationException>(act2);

// Test 3: SearchListingsAsync with valid query delegates to repository
var listings = new List<Listing> { new Listing { Id = Guid.NewGuid(), Title = "Premium Headphones", Description = "High quality wireless headphones", Price = new Money(299.99m, "USD"), CategoryId = Guid.NewGuid(), Status = ListingStatus.Active } };
listingRepoMock.Setup(r => r.SearchAsync("headphones")).ReturnsAsync(listings);
var searchResults = await searchService.SearchListingsAsync("headphones");
Console.WriteLine($"Found {searchResults.Count} listings matching 'headphones'");

// Test 4: SearchByTagsAsync throws ValidationException when provided with an empty tag list
var act4 = async () => await searchService.SearchByTagsAsync(new List<string>());
await Assert.ThrowsAsync<ValidationException>(act4);

// Test 5: SearchByTagsAsync throws ValidationException when provided with a null tag list
var act5 = async () => await searchService.SearchByTagsAsync(null!);
await Assert.ThrowsAsync<ValidationException>(act5);

// Test 6: SearchByTagsAsync with valid tags delegates to repository
var tagResults = await searchService.SearchByTagsAsync(new List<string> { "electronics", "audio" });
Console.WriteLine($"Found {tagResults.Count} listings with specified tags");

// Test 7: FindNearbyListingsAsync throws ValidationException when latitude is below -90
var act7 = async () => await searchService.FindNearbyListingsAsync(-91, 0);
await Assert.ThrowsAsync<ValidationException>(act7);

// Test 8: FindNearbyListingsAsync throws ValidationException when longitude is below -180
var act8 = async () => await searchService.FindNearbyListingsAsync(0, -181);
await Assert.ThrowsAsync<ValidationException>(act8);

// Test 9: FindNearbyListingsAsync throws ValidationException when radius is below minimum
var act9 = async () => await searchService.FindNearbyListingsAsync(40, -74, 0.05);
await Assert.ThrowsAsync<ValidationException>(act9);

// Test 10: FindNearbyListingsAsync with valid parameters delegates to repository
var nearbyResults = await searchService.FindNearbyListingsAsync(40.7128, -74.0060, 5.0);
Console.WriteLine($"Found {nearbyResults.Count} listings within 5km radius of New York");

// Test 11: SearchByCategoryAsync throws ValidationException when provided with an empty category GUID
var act11 = async () => await searchService.SearchByCategoryAsync(Guid.Empty, 1, 20);
await Assert.ThrowsAsync<ValidationException>(act11);

// Test 12: SearchByCategoryAsync with valid category ID returns paginated results
var (categoryResults, totalCount) = await searchService.SearchByCategoryAsync(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), 1, 25);
Console.WriteLine($"Page 1: {categoryResults.Count} of {totalCount} listings in category");

// Test 13: AdvancedSearchAsync with keyword filter returns matching listings
var keywordResults = await searchService.AdvancedSearchAsync(keyword: "laptop");
Console.WriteLine($"Found {keywordResults.Count} listings matching keyword 'laptop'");

// Test 14: AdvancedSearchAsync with price range filter returns listings in range
var priceResults = await searchService.AdvancedSearchAsync(minPrice: 100, maxPrice: 500);
Console.WriteLine($"Found {priceResults.Count} listings in price range $100-$500");

// Test 15: AdvancedSearchAsync with category filter returns only matching category
var categoryId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
var categoryResults2 = await searchService.AdvancedSearchAsync(categoryId: categoryId);
Console.WriteLine($"Found {categoryResults2.Count} listings in specified category");
```

## ReviewServiceTests

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Entities;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

// Initialize mock repositories
var reviewRepoMock = new Mock<IReviewRepository>();
var userRepoMock = new Mock<IUserRepository>();
var listingRepoMock = new Mock<IListingRepository>();

// Create the service under test
var reviewService = new ReviewService(reviewRepoMock.Object, userRepoMock.Object, listingRepoMock.Object);

// Test 1: SubmitReviewAsync throws ResourceNotFoundException when reviewer is not found
var nonExistentReviewerId = Guid.NewGuid();
userRepoMock.Setup(r => r.GetByIdAsync(nonExistentReviewerId))
    .ReturnsAsync((User)null);

var act1 = async () => await reviewService.SubmitReviewAsync(
    reviewerId: nonExistentReviewerId,
    sellerId: Guid.NewGuid(),
    listingId: Guid.NewGuid(),
    score: 5,
    comment: "Great seller!"
);

await Assert.ThrowsAsync<ResourceNotFoundException>(act1);

// Test 2: SubmitReviewAsync throws UnauthorizedException when reviewer is inactive
var inactiveReviewerId = Guid.NewGuid();
var inactiveReviewer = new User { Id = inactiveReviewerId, IsActive = false };
userRepoMock.Setup(r => r.GetByIdAsync(inactiveReviewerId))
    .ReturnsAsync(inactiveReviewer);

var act2 = async () => await reviewService.SubmitReviewAsync(
    reviewerId: inactiveReviewerId,
    sellerId: Guid.NewGuid(),
    listingId: Guid.NewGuid(),
    score: 5,
    comment: "Great seller!"
);

await Assert.ThrowsAsync<UnauthorizedException>(act2);

// Test 3: SubmitReviewAsync throws MarketplaceException when reviewer is seller
var sellerReviewerId = Guid.NewGuid();
var sellerUser = new User { Id = sellerReviewerId, Role = UserRole.Seller };
userRepoMock.Setup(r => r.GetByIdAsync(sellerReviewerId))
    .ReturnsAsync(sellerUser);

var act3 = async () => await reviewService.SubmitReviewAsync(
    reviewerId: sellerReviewerId,
    sellerId: Guid.NewGuid(),
    listingId: Guid.NewGuid(),
    score: 5,
    comment: "Great seller!"
);

await Assert.ThrowsAsync<MarketplaceException>(act3);

// Test 4: SubmitReviewAsync throws DuplicateResourceException when duplicate review exists
var existingReviewerId = Guid.NewGuid();
var existingSellerId = Guid.NewGuid();
var existingListingId = Guid.NewGuid();
var existingReview = new Review { ReviewerId = existingReviewerId, SellerId = existingSellerId, ListingId = existingListingId };

reviewRepoMock.Setup(r => r.ExistsForTransactionAsync(existingReviewerId, existingSellerId, existingListingId))
    .ReturnsAsync(true);

var act4 = async () => await reviewService.SubmitReviewAsync(
    reviewerId: existingReviewerId,
    sellerId: existingSellerId,
    listingId: existingListingId,
    score: 5,
    comment: "Great seller!"
);

await Assert.ThrowsAsync<DuplicateResourceException>(act4);

// Test 5: SubmitReviewAsync with valid data creates review and updates seller rating
var validReviewerId = Guid.NewGuid();
var validSellerId = Guid.NewGuid();
var validListingId = Guid.NewGuid();
var newReview = new Review { Id = Guid.NewGuid(), ReviewerId = validReviewerId, SellerId = validSellerId, ListingId = validListingId, Score = 5 };

reviewRepoMock.Setup(r => r.AddAsync(It.IsAny<Review>()))
    .ReturnsAsync(newReview);
reviewRepoMock.Setup(r => r.GetAverageScoreAsync(validSellerId))
    .ReturnsAsync(4.8);

var createdReview = await reviewService.SubmitReviewAsync(
    reviewerId: validReviewerId,
    sellerId: validSellerId,
    listingId: validListingId,
    score: 5,
    comment: "Excellent transaction! Fast shipping and great communication."
);

Console.WriteLine($"Review created: {createdReview.Id}");

// Test 6: AddSellerReplyAsync throws UnauthorizedException when caller is not seller
var reviewOwnerId = Guid.NewGuid();
var nonSellerRequesterId = Guid.NewGuid();
var reviewWithSeller = new Review { Id = Guid.NewGuid(), SellerId = reviewOwnerId };

reviewRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
    .ReturnsAsync(reviewWithSeller);

var act6 = async () => await reviewService.AddSellerReplyAsync(
    reviewId: reviewWithSeller.Id,
    replyText: "Thank you for your review!",
    requesterId: nonSellerRequesterId
);

await Assert.ThrowsAsync<UnauthorizedException>(act6);

// Test 7: GetSellerStatsAsync returns correct average and distribution
var sellerStats = await reviewService.GetSellerStatsAsync(validSellerId);
Console.WriteLine($"Seller average rating: {sellerStats.AverageScore:F1}");
Console.WriteLine($"Total reviews: {sellerStats.TotalReviews}");
Console.WriteLine($"Distribution: 5-star: {sellerStats.Distribution.FiveStar}, 4-star: {sellerStats.Distribution.FourStar}, etc.");

// Test 8: RemoveReviewAsync throws UnauthorizedException when caller is not moderator
var reviewToRemove = new Review { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid() };
var regularUserId = Guid.NewGuid();

reviewRepoMock.Setup(r => r.GetByIdAsync(reviewToRemove.Id))
    .ReturnsAsync(reviewToRemove);

var act8 = async () => await reviewService.RemoveReviewAsync(
    reviewId: reviewToRemove.Id,
    requesterId: regularUserId
);

await Assert.ThrowsAsync<UnauthorizedException>(act8);
```

## ValueObjectTests

The `ValueObjectTests` class provides unit tests for the Marketplace Engine's value objects, ensuring they maintain business invariants and handle edge cases correctly. It tests the `Money`, `Rating`, and `Location` value objects with validation for negative amounts, currency mismatches, score ranges, and coordinate-based distance calculations.

### Usage Example

```csharp
using MarketplaceEngine.Domain.ValueObjects;
using System;

// Test Money value object behavior
var price1 = new Money(50m, "USD");
var price2 = new Money(30m, "USD");

// Add two Money objects with same currency
var sum = price1.Add(price2);
Console.WriteLine($"Sum: {sum.Amount} {sum.CurrencyCode}"); // Sum: 80 USD

// Test Money validation - negative amount throws
try
{
    var invalidPrice = new Money(-10m, "USD");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Caught expected exception: {ex.Message}");
}

// Test Money with different currencies
var usd = new Money(100m, "USD");
var eur = new Money(100m, "EUR");

try
{
    var mixed = usd.Add(eur);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Caught expected exception: {ex.Message}");
}

// Test Money multiplication
var price = new Money(99.99m, "USD");
var zeroPrice = price.Multiply(0m);
Console.WriteLine($"Zero multiplication result: {zeroPrice.Amount} {zeroPrice.CurrencyCode}"); // Zero multiplication result: 0 USD

// Test Rating value object
var rating = new Rating(4, totalReviews: 5);
Console.WriteLine($"Initial rating: Score={rating.Score}, TotalReviews={rating.TotalReviews}"); // Score=4, TotalReviews=5

// Add a review to increment total reviews
var updatedRating = rating.AddReview(5);
Console.WriteLine($"Updated rating: Score={updatedRating.Score}, TotalReviews={updatedRating.TotalReviews}"); // Score=4, TotalReviews=6

// Test Rating validation - score above 5 throws
try
{
    var invalidRating = new Rating(6);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Caught expected exception: {ex.Message}");
}

// Test Location value object
var london = new Location("London", "England", "GB");
var paris = new Location("Paris", "Ile-de-France", "FR");

// Distance calculation returns null when coordinates are not available
var distance = london.DistanceTo(paris);
Console.WriteLine($"Distance between locations: {distance}"); // Distance between locations: 

// Test Location validation - three-letter country code throws
try
{
    var invalidLocation = new Location("New York", "NY", "USA");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Caught expected exception: {ex.Message}");
}
```

## SellerDashboardServiceTests

The `SellerDashboardServiceTests` class provides unit tests for the `SellerDashboardService` class, validating its dashboard functionality for sellers. It tests various scenarios including dashboard retrieval for valid and invalid sellers, revenue calculation with completed payments, and listing statistics with view count tracking.

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Entities;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

// Initialize mock repositories
var userRepoMock = new Mock<IUserRepository>();
var listingRepoMock = new Mock<IListingRepository>();
var paymentRepoMock = new Mock<IPaymentRepository>();
var reviewRepoMock = new Mock<IReviewRepository>();
var messageRepoMock = new Mock<IMessageRepository>();

// Create the service under test
var sellerDashboardService = new SellerDashboardService(
    userRepoMock.Object,
    listingRepoMock.Object,
    paymentRepoMock.Object,
    reviewRepoMock.Object,
    messageRepoMock.Object
);

// Test 1: GetDashboardAsync throws ResourceNotFoundException when seller is not found
var nonExistentSellerId = Guid.NewGuid();
userRepoMock.Setup(r => r.GetByIdAsync(nonExistentSellerId))
    .ReturnsAsync((User)null);

var act1 = async () => await sellerDashboardService.GetDashboardAsync(nonExistentSellerId);
await Assert.ThrowsAsync<ResourceNotFoundException>(act1);

// Test 2: GetDashboardAsync returns correct metrics for valid seller
var sellerId = Guid.NewGuid();
var seller = new User { Id = sellerId, FullName = "Jane Seller", IsActive = true, TotalSales = 3 };
var listings = new List<Listing>
{
    new Listing { Id = Guid.NewGuid(), SellerId = sellerId, Status = ListingStatus.Active, Price = new Money(100m) },
    new Listing { Id = Guid.NewGuid(), SellerId = sellerId, Status = ListingStatus.Active, Price = new Money(200m) },
    new Listing { Id = Guid.NewGuid(), SellerId = sellerId, Status = ListingStatus.Inactive, Price = new Money(50m) }
};
var completedPayment = new Payment
{
    Id = Guid.NewGuid(),
    SellerId = sellerId,
    Status = PaymentStatus.Completed,
    Amount = new Money(100m),
    PlatformFee = new Money(5m),
    SellerPayout = new Money(95m)
};

userRepoMock.Setup(r => r.GetByIdAsync(sellerId)).ReturnsAsync(seller);
listingRepoMock.Setup(r => r.GetBySellerIdAsync(sellerId)).ReturnsAsync(listings);
paymentRepoMock.Setup(r => r.GetBySellerIdAsync(sellerId)).ReturnsAsync(new[] { completedPayment });
reviewRepoMock.Setup(r => r.GetBySellerIdAsync(sellerId)).ReturnsAsync(new List<Review>());
messageRepoMock.Setup(r => r.GetUnreadMessagesAsync(sellerId)).ReturnsAsync(new List<Message>());

var dashboard = await sellerDashboardService.GetDashboardAsync(sellerId);
Console.WriteLine($"Seller Dashboard for: {dashboard.SellerName}");
Console.WriteLine($"Performance: {dashboard.ActiveListings} active listings | {dashboard.TotalSales} sales");
Console.WriteLine($"Revenue: ${dashboard.TotalRevenue:N2} total");

// Test 3: GetRevenueAsync returns monthly breakdown for completed payments
var revenue = await sellerDashboardService.GetRevenueAsync(sellerId);
Console.WriteLine($"\nRevenue breakdown: ${revenue.TotalGrossRevenue:N2} gross | ${revenue.TotalNetRevenue:N2} net");

// Test 4: GetListingStatsAsync returns top listings by view count
var stats = await sellerDashboardService.GetListingStatsAsync(sellerId);
Console.WriteLine($"Listing stats: {stats.ActiveListings} active listings | {stats.TotalViews} total views");
foreach (var listing in stats.TopListings)
{
    Console.WriteLine($"- {listing.Title}: {listing.ViewCount} views");
}
```

## UserServiceTests

The `UserServiceTests` class contains unit tests for the `UserService` class, verifying that it correctly handles user registration, email verification, account management, profile updates, and premium account features. It tests various business rules including email uniqueness validation, name length requirements, email verification token validation, premium account eligibility criteria, and account status validation.

## ListingServiceExtendedTests

The `ListingServiceExtendedTests` class provides extended unit tests for the `ListingService` class, covering advanced scenarios, edge cases, and integration-style validations that complement the core unit tests. It tests various business rules including validation for negative prices, short titles, missing images, authorization checks, visibility control, view tracking, interest recording, and featured listing management.

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Entities;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

// Initialize mock repositories
var userRepoMock = new Mock<IUserRepository>();

// Create the service under test
var userService = new UserService(userRepoMock.Object);

// Test 1: RegisterUserAsync returns a created user when provided with a unique email
var createdUser = new User
{
    Id = Guid.NewGuid(),
    Email = "new@example.com",
    FullName = "Alice Smith",
    IsActive = true,
    IsVerified = false
};

userRepoMock.Setup(r => r.GetByEmailAsync("new@example.com"))
    .ReturnsAsync((User)null);
userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
    .ReturnsAsync(createdUser);

var result = await userService.RegisterUserAsync("new@example.com", "Alice Smith");

Console.WriteLine($"User registered: {result.Id} - {result.Email}");

// Test 2: RegisterUserAsync throws DuplicateResourceException when provided with a duplicate email
var existing = new User { Id = Guid.NewGuid(), Email = "taken@example.com", FullName = "Bob Jones" };
userRepoMock.Setup(r => r.GetByEmailAsync("taken@example.com"))
    .ReturnsAsync(existing);

var act2 = async () => await userService.RegisterUserAsync("taken@example.com", "Carol Green");
await Assert.ThrowsAsync<DuplicateResourceException>(act2);

// Test 3: RegisterUserAsync throws ArgumentException when provided with a short full name
userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
    .ReturnsAsync((User)null);

var act3 = async () => await userService.RegisterUserAsync("x@example.com", "A");
await Assert.ThrowsAsync<ArgumentException>(act3);

// Test 4: GetUserAsync throws ResourceNotFoundException when the user is not found
var missingId = Guid.NewGuid();
userRepoMock.Setup(r => r.GetByIdAsync(missingId))
    .ReturnsAsync((User)null);

var act4 = async () => await userService.GetUserAsync(missingId);
await Assert.ThrowsAsync<ResourceNotFoundException>(act4);

// Test 5: GetUserAsync returns the user when the user exists
var userId = Guid.NewGuid();
var user = new User { Id = userId, Email = "u@example.com", FullName = "Test User" };
userRepoMock.Setup(r => r.GetByIdAsync(userId))
    .ReturnsAsync(user);

var retrievedUser = await userService.GetUserAsync(userId);
Console.WriteLine($"Retrieved user: {retrievedUser.DisplayName} ({retrievedUser.Email})");

// Test 6: VerifyEmailAsync returns true when provided with a valid token
var userWithToken = new User { Id = userId, Email = "u@example.com", FullName = "Test User" };
userWithToken.GenerateVerificationToken();

userRepoMock.Setup(r => r.GetByIdAsync(userId))
    .ReturnsAsync(userWithToken);
userRepoMock.Setup(r => r.UpdateAsync(userWithToken))
    .ReturnsAsync(userWithToken);

var verificationResult = await userService.VerifyEmailAsync(userId, userWithToken.VerificationToken!);
Console.WriteLine($"Email verified: {verificationResult}");

// Test 7: VerifyEmailAsync returns false when provided with the wrong token
var wrongTokenResult = await userService.VerifyEmailAsync(userId, "wrong-token");
Console.WriteLine($"Wrong token result: {wrongTokenResult}");

// Test 8: VerifyEmailAsync returns false when provided with an expired token
var expiredUser = new User
{
    Id = userId,
    Email = "u@example.com",
    FullName = "Test User",
    VerificationToken = "some-token",
    VerificationExpiry = DateTime.UtcNow.AddMinutes(-1) // already expired
};
userRepoMock.Setup(r => r.GetByIdAsync(userId))
    .ReturnsAsync(expiredUser);

var expiredTokenResult = await userService.VerifyEmailAsync(userId, "some-token");
Console.WriteLine($"Expired token result: {expiredTokenResult}");

// Test 9: PromoteToPremiumAsync throws InvalidOperationException when the user has insufficient sales
var lowSalesUser = new User
{
    Id = userId,
    Email = "u@example.com",
    FullName = "Low Sales User",
    TotalSales = 3,
    Rating = new Rating(5, 10)
};
userRepoMock.Setup(r => r.GetByIdAsync(userId))
    .ReturnsAsync(lowSalesUser);

var act9 = async () => await userService.PromoteToPremiumAsync(userId);
await Assert.ThrowsAsync<InvalidOperationException>(act9);

// Test 10: PromoteToPremiumAsync throws InvalidOperationException when the user's rating is below the threshold
var lowRatingUser = new User
{
    Id = userId,
    Email = "u@example.com",
    FullName = "Low Rating User",
    TotalSales = 10,
    Rating = new Rating(3, 10)
};
userRepoMock.Setup(r => r.GetByIdAsync(userId))
    .ReturnsAsync(lowRatingUser);

var act10 = async () => await userService.PromoteToPremiumAsync(userId);
await Assert.ThrowsAsync<InvalidOperationException>(act10);

// Test 11: PromoteToPremiumAsync throws InvalidOperationException when the user has no rating
var noRatingUser = new User
{
    Id = userId,
    Email = "u@example.com",
    FullName = "No Rating User",
    TotalSales = 10,
    Rating = null
};
userRepoMock.Setup(r => r.GetByIdAsync(userId))
    .ReturnsAsync(noRatingUser);

var act11 = async () => await userService.PromoteToPremiumAsync(userId);
await Assert.ThrowsAsync<InvalidOperationException>(act11);

// Test 12: PromoteToPremiumAsync promotes the user when they are eligible
var eligibleUser = new User
{
    Id = userId,
    Email = "u@example.com",
    FullName = "Good Seller",
    TotalSales = 10,
    Rating = new Rating(5, 20),
    Role = UserRole.User
};
userRepoMock.Setup(r => r.GetByIdAsync(userId))
    .ReturnsAsync(eligibleUser);
userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>()))
    .ReturnsAsync(eligibleUser);

var promotedUser = await userService.PromoteToPremiumAsync(userId);
Console.WriteLine($"User promoted to: {promotedUser.Role}");

// Test 13: DeactivateAccountAsync sets the user's active status to false when the user exists
var activeUser = new User { Id = userId, Email = "u@example.com", FullName = "Active User", IsActive = true };
userRepoMock.Setup(r => r.GetByIdAsync(userId))
    .ReturnsAsync(activeUser);
userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>()))
    .ReturnsAsync((User u) => u);

var deactivatedUser = await userService.DeactivateAccountAsync(userId);
Console.WriteLine($"Account deactivated: {deactivatedUser.IsActive}");

// Test 14: ValidateUserAccessAsync throws UnauthorizedException when the user is inactive
var inactiveUser = new User
{
    Id = userId,
    Email = "u@example.com",
    FullName = "Inactive User",
    IsActive = false,
    IsVerified = true
};
userRepoMock.Setup(r => r.GetByIdAsync(userId))
    .ReturnsAsync(inactiveUser);

var act14 = async () => await userService.ValidateUserAccessAsync(userId);
await Assert.ThrowsAsync<UnauthorizedException>(act14);

// Test 15: ValidateUserAccessAsync throws UnauthorizedException when the user is not verified
var unverifiedUser = new User
{
    Id = userId,
    Email = "u@example.com",
    FullName = "Unverified User",
    IsActive = true,
    IsVerified = false
};
userRepoMock.Setup(r => r.GetByIdAsync(userId))
    .ReturnsAsync(unverifiedUser);

var act15 = async () => await userService.ValidateUserAccessAsync(userId);
await Assert.ThrowsAsync<UnauthorizedException>(act15);

// Test 16: ValidateUserAccessAsync does not throw when the user is active and verified
var goodUser = new User
{
    Id = userId,
    Email = "u@example.com",
    FullName = "Good User",
    IsActive = true,
    IsVerified = true
};
userRepoMock.Setup(r => r.GetByIdAsync(userId))
    .ReturnsAsync(goodUser);

var act16 = async () => await userService.ValidateUserAccessAsync(userId);
await Assert.ThrowsAsync<UnauthorizedException>(act16);

// Test 17: UpdateProfileAsync updates the user's name when provided with a new full name
var userForUpdate = new User { Id = userId, Email = "u@example.com", FullName = "Old Name" };
userRepoMock.Setup(r => r.GetByIdAsync(userId))
    .ReturnsAsync(userForUpdate);
userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>()))
    .ReturnsAsync((User u) => u);

var updatedUser = await userService.UpdateProfileAsync(userId, fullName: "New Name");
Console.WriteLine($"Profile updated: {updatedUser.FullName}");

// Test 18: UpdateProfileAsync clears the user's phone when provided with blank input
var userWithPhone = new User
{
    Id = userId,
    Email = "u@example.com",
    FullName = "Test User",
    Phone = "+1234567890"
};
userRepoMock.Setup(r => r.GetByIdAsync(userId))
    .ReturnsAsync(userWithPhone);

var phoneClearedUser = await userService.UpdateProfileAsync(userId, phone: " ");
Console.WriteLine($"Phone cleared: {phoneClearedUser.Phone}");

// Test 19: RecordSaleAsync increments the user's total sales count
var sellerUser = new User
{
    Id = userId,
    Email = "u@example.com",
    FullName = "Seller User",
    TotalSales = 4
};
userRepoMock.Setup(r => r.GetByIdAsync(userId))
    .ReturnsAsync(sellerUser);
userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>()))
    .ReturnsAsync((User u) => u);

var userWithSale = await userService.RecordSaleAsync(userId);
Console.WriteLine($"Total sales incremented: {userWithSale.TotalSales}");
```

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Entities;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

// Initialize mock repositories
var reviewRepoMock = new Mock<IReviewRepository>();
var userRepoMock = new Mock<IUserRepository>();
var listingRepoMock = new Mock<IListingRepository>();

// Create the service under test
var reviewService = new ReviewService(reviewRepoMock.Object, userRepoMock.Object, listingRepoMock.Object);

// Test 1: SubmitReviewAsync throws ResourceNotFoundException when reviewer is not found
var nonExistentReviewerId = Guid.NewGuid();
userRepoMock.Setup(r => r.GetByIdAsync(nonExistentReviewerId))
    .ReturnsAsync((User)null);

var act1 = async () => await reviewService.SubmitReviewAsync(
    reviewerId: nonExistentReviewerId,
    sellerId: Guid.NewGuid(),
    listingId: Guid.NewGuid(),
    score: 5,
    comment: "Great seller!"
);

await Assert.ThrowsAsync<ResourceNotFoundException>(act1);

// Test 2: SubmitReviewAsync throws UnauthorizedException when reviewer is inactive
var inactiveReviewerId = Guid.NewGuid();
var inactiveReviewer = new User { Id = inactiveReviewerId, IsActive = false };
userRepoMock.Setup(r => r.GetByIdAsync(inactiveReviewerId))
    .ReturnsAsync(inactiveReviewer);

var act2 = async () => await reviewService.SubmitReviewAsync(
    reviewerId: inactiveReviewerId,
    sellerId: Guid.NewGuid(),
    listingId: Guid.NewGuid(),
    score: 5,
    comment: "Great seller!"
);

await Assert.ThrowsAsync<UnauthorizedException>(act2);

// Test 3: SubmitReviewAsync throws MarketplaceException when reviewer is seller
var sellerReviewerId = Guid.NewGuid();
var sellerUser = new User { Id = sellerReviewerId, Role = UserRole.Seller };
userRepoMock.Setup(r => r.GetByIdAsync(sellerReviewerId))
    .ReturnsAsync(sellerUser);

var act3 = async () => await reviewService.SubmitReviewAsync(
    reviewerId: sellerReviewerId,
    sellerId: Guid.NewGuid(),
    listingId: Guid.NewGuid(),
    score: 5,
    comment: "Great seller!"
);

await Assert.ThrowsAsync<MarketplaceException>(act3);

// Test 4: SubmitReviewAsync throws DuplicateResourceException when duplicate review exists
var existingReviewerId = Guid.NewGuid();
var existingSellerId = Guid.NewGuid();
var existingListingId = Guid.NewGuid();
var existingReview = new Review { ReviewerId = existingReviewerId, SellerId = existingSellerId, ListingId = existingListingId };

reviewRepoMock.Setup(r => r.ExistsForTransactionAsync(existingReviewerId, existingSellerId, existingListingId))
    .ReturnsAsync(true);

var act4 = async () => await reviewService.SubmitReviewAsync(
    reviewerId: existingReviewerId,
    sellerId: existingSellerId,
    listingId: existingListingId,
    score: 5,
    comment: "Great seller!"
);

await Assert.ThrowsAsync<DuplicateResourceException>(act4);

// Test 5: SubmitReviewAsync with valid data creates review and updates seller rating
var validReviewerId = Guid.NewGuid();
var validSellerId = Guid.NewGuid();
var validListingId = Guid.NewGuid();
var newReview = new Review { Id = Guid.NewGuid(), ReviewerId = validReviewerId, SellerId = validSellerId, ListingId = validListingId, Score = 5 };

reviewRepoMock.Setup(r => r.AddAsync(It.IsAny<Review>()))
    .ReturnsAsync(newReview);
reviewRepoMock.Setup(r => r.GetAverageScoreAsync(validSellerId))
    .ReturnsAsync(4.8);

var createdReview = await reviewService.SubmitReviewAsync(
    reviewerId: validReviewerId,
    sellerId: validSellerId,
    listingId: validListingId,
    score: 5,
    comment: "Excellent transaction! Fast shipping and great communication."
);

Console.WriteLine($"Review created: {createdReview.Id}");

// Test 6: AddSellerReplyAsync throws UnauthorizedException when caller is not seller
var reviewOwnerId = Guid.NewGuid();
var nonSellerRequesterId = Guid.NewGuid();
var reviewWithSeller = new Review { Id = Guid.NewGuid(), SellerId = reviewOwnerId };

reviewRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
    .ReturnsAsync(reviewWithSeller);

var act6 = async () => await reviewService.AddSellerReplyAsync(
    reviewId: reviewWithSeller.Id,
    replyText: "Thank you for your review!",
    requesterId: nonSellerRequesterId
);

await Assert.ThrowsAsync<UnauthorizedException>(act6);

// Test 7: GetSellerStatsAsync returns correct average and distribution
var sellerStats = await reviewService.GetSellerStatsAsync(validSellerId);
Console.WriteLine($"Seller average rating: {sellerStats.AverageScore:F1}");
Console.WriteLine($"Total reviews: {sellerStats.TotalReviews}");
Console.WriteLine($"Distribution: 5-star: {sellerStats.Distribution.FiveStar}, 4-star: {sellerStats.Distribution.FourStar}, etc.");

// Test 8: RemoveReviewAsync throws UnauthorizedException when caller is not moderator
var reviewToRemove = new Review { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid() };
var regularUserId = Guid.NewGuid();

reviewRepoMock.Setup(r => r.GetByIdAsync(reviewToRemove.Id))
    .ReturnsAsync(reviewToRemove);

var act8 = async () => await reviewService.RemoveReviewAsync(
    reviewId: reviewToRemove.Id,
    requesterId: regularUserId
);

await Assert.ThrowsAsync<UnauthorizedException>(act8);
```

## CollaborativeFilteringEngineExtensions

The `CollaborativeFilteringEngineExtensions` class provides extension methods for the collaborative filtering recommendation engine that enable personalized listing recommendations, similar-item discovery, trending content identification, and user affinity calculations. These extensions integrate with the core recommendation system to provide flexible APIs for generating various types of recommendation feeds based on user behavior and listing interactions.

### Usage Example

```csharp
using MarketplaceEngine.Recommendations;
using System;
using System.Linq;
using System.Threading.Tasks;

// Initialize the recommendation engine
var recommendationEngine = new CollaborativeFilteringEngine();

// Compute personalized recommendations for a specific user
var userRecommendations = await recommendationEngine.ComputeForUserAsync(
    userId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    count: 15
);

Console.WriteLine($"Generated {userRecommendations.Count} personalized recommendations");
foreach (var item in userRecommendations.Take(5))
{
    Console.WriteLine($"- {item.Title}: {item.Score:P1}");
}

// Compute similar listings for a specific listing
var listingId = Guid.Parse("33333333-3333-3333-3333-333333333333");
var similarListings = await recommendationEngine.ComputeSimilarAsync(
    listingId: listingId,
    count: 10
);

Console.WriteLine($"Found {similarListings.Count} similar listings");

// Compute trending listings (popular items)
var trendingListings = await recommendationEngine.ComputeTrendingAsync(
    count: 20,
    timeWindowHours: 72
);

Console.WriteLine($"Top {trendingListings.Count} trending listings");

// Compute recommendations based on user affinity (category preferences)
var affinityRecommendations = await recommendationEngine.ComputeByAffinityAsync(
    userId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    count: 12
);

Console.WriteLine($"Generated {affinityRecommendations.Count} affinity-based recommendations");

// Record user activity to improve future recommendations
await recommendationEngine.RecordSignalAsync(
    userId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    listingId: listingId,
    signalType: SignalType.View
);

Console.WriteLine("User activity recorded successfully");

// Compute recommendations for multiple users at once (batch processing)
var userIds = new[] {
    Guid.Parse("11111111-1111-1111-1111-111111111111"),
    Guid.Parse("22222222-2222-2222-2222-222222222222"),
    Guid.Parse("33333333-3333-3333-3333-333333333333")
};

var batchRecommendations = await recommendationEngine.ComputeForUsersAsync(
    userIds: userIds,
    count: 8
);

foreach (var userRec in batchRecommendations)
{
    Console.WriteLine($"User {userRec.Key} has {userRec.Value.Count} recommendations");
}

// Compute similar listings for multiple listings at once (batch processing)
var listingIds = new[] {
    Guid.Parse("33333333-3333-3333-3333-333333333333"),
    Guid.Parse("44444444-4444-4444-4444-444444444444"),
    Guid.Parse("55555555-5555-5555-5555-555555555555")
};

var batchSimilar = await recommendationEngine.ComputeSimilarForListingsAsync(
    listingIds: listingIds,
    count: 5
);

foreach (var listingSim in batchSimilar)
{
    Console.WriteLine($"Listing {listingSim.Key} has {listingSim.Value.Count} similar items");
}
```

## RecommendationsController

The `RecommendationsController` class provides RESTful API endpoints for the collaborative filtering recommendation engine. It exposes personalised feeds, similar-item panels, trending galleries, category-affinity recommendations, and activity tracking. The controller integrates with the `RecommendationService` to deliver personalised content based on user behavior and listing interactions.

### Usage Example

```csharp
using MarketplaceEngine.Controllers;
using MarketplaceEngine.DTOs;
using MarketplaceEngine.Recommendations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// Initialize HTTP client for API calls
var client = new HttpClient { BaseAddress = new Uri("https://api.marketplace.example.com") };

// Get trending listings (public feed)
var trendingResponse = await client.GetAsync("/api/v2/recommendations/trending?count=15");
var trendingFeed = await trendingResponse.Content.ReadFromJsonAsync<RecommendationFeedDto>();
Console.WriteLine($"Trending feed contains {trendingFeed.Items.Count} items");

// Get personalised recommendations for a user
var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
var userResponse = await client.GetAsync($"/api/v2/recommendations/users/{userId}?count=20");
var userFeed = await userResponse.Content.ReadFromJsonAsync<RecommendationFeedDto>();
Console.WriteLine($"Personalised feed generated {userFeed.Items.Count} recommendations");
Console.WriteLine($"Is personalised: {userFeed.IsPersonalised}");

// Get category-affinity recommendations for a user
var affinityResponse = await client.GetAsync($"/api/v2/recommendations/users/{userId}/affinity?count=15");
var affinityFeed = await affinityResponse.Content.ReadFromJsonAsync<RecommendationFeedDto>();
Console.WriteLine($"Category-affinity feed contains {affinityFeed.Items.Count} items");

// Get similar listings for a specific listing
var listingId = Guid.Parse("33333333-3333-3333-3333-333333333333");
var similarResponse = await client.GetAsync($"/api/v2/recommendations/listings/{listingId}/similar?count=10");
var similarFeed = await similarResponse.Content.ReadFromJsonAsync<RecommendationFeedDto>();
Console.WriteLine($"Similar items panel contains {similarFeed.Items.Count} items");

// Track user activity to improve future recommendations
await client.PostAsJsonAsync("/api/v2/recommendations/track", new TrackActivityRequest
{
    UserId = userId,
    ListingId = listingId,
    SignalType = SignalType.View.ToString()
});
Console.WriteLine("User activity tracked successfully");

// Get diagnostics snapshot for monitoring
var diagnosticsResponse = await client.GetAsync("/api/v2/recommendations/diagnostics");
var diagnostics = await diagnosticsResponse.Content.ReadFromJsonAsync<RecommendationDiagnosticsReport>();
Console.WriteLine($"Diagnostics: {diagnostics.TrackerStats.TotalSignals} signals tracked");
```

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Initialize review service (typically via dependency injection)
// var reviewService = new ReviewService(reviewRepository, ...);

// Submit a new review
var review = await reviewService.SubmitReviewAsync(
    reviewerId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    sellerId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
    listingId: Guid.Parse("33333333-3333-3333-3333-333333333333"),
    score: 5,
    comment: "Great experience!"
);

// Add a seller reply
await reviewService.AddSellerReplyAsync(review.Id, "Thank you!");

// Get seller reviews
var (reviews, total) = await reviewService.GetSellerReviewsAsync(Guid.Parse("22222222-2222-2222-2222-222222222222"));

// Get seller stats
var (averageScore, totalReviews, distribution) = await reviewService.GetSellerStatsAsync(Guid.Parse("22222222-2222-2222-2222-222222222222"));
Console.WriteLine($"Average Score: {averageScore:F1}, Total Reviews: {totalReviews}");
```

## MarketplaceWorkflowTests

The `MarketplaceWorkflowTests` class provides integration tests that exercise complete multi-service workflows using real in-memory repositories and a shared database context. It validates end-to-end scenarios including listing lifecycle management, messaging workflows, user promotion paths, search functionality, concurrent operations, email verification, pagination correctness, and user deactivation behavior.

### Usage Example

```csharp
using MarketplaceEngine.Tests;
using System;
using System.Threading.Tasks;

// Initialize the test suite (resets database automatically)
var workflowTests = new MarketplaceWorkflowTests();

// Test 1: Full listing lifecycle - create, search, delist
try
{
    await workflowTests.FullListingLifecycle_CreateSearchDelistAndVerify();
    Console.WriteLine("✓ Full listing lifecycle test passed");
}
finally
{
    workflowTests.Dispose();
}

// Test 2: Complete messaging workflow - send, read, reply, delete
try
{
    await workflowTests.FullMessagingWorkflow_SendReadReplyAndDelete();
    Console.WriteLine("✓ Messaging workflow test passed");
}
finally
{
    workflowTests.Dispose();
}

// Test 3: User registration to premium promotion
try
{
    await workflowTests.UserRegistrationToPremiumPromotion_WhenEligible_Succeeds();
    Console.WriteLine("✓ Premium promotion test passed");
}
finally
{
    workflowTests.Dispose();
}

// Test 4: Advanced search with filters
try
{
    await workflowTests.AdvancedSearch_WithPriceAndCategoryFilters_ReturnsCorrectListings();
    Console.WriteLine("✓ Advanced search test passed");
}
finally
{
    workflowTests.Dispose();
}

// Test 5: Concurrent operations safety
try
{
    await workflowTests.ConcurrentListingCreation_AllListingsPersistedWithoutDataCorruption();
    Console.WriteLine("✓ Concurrent operations test passed");
}
finally
{
    workflowTests.Dispose();
}

// Test 6: Email verification workflow
try
{
    await workflowTests.EmailVerification_WithCorrectToken_VerifiesUser();
    Console.WriteLine("✓ Email verification test passed");
}
finally
{
    workflowTests.Dispose();
}

// Test 7: Pagination correctness
try
{
    await workflowTests.PaginatedListings_SecondPage_ReturnsDistinctItemsFromFirstPage();
    Console.WriteLine("✓ Pagination test passed");
}
finally
{
    workflowTests.Dispose();
}

// Test 8: Category search with pagination
try
{
    await workflowTests.SearchByCategory_TotalMatchesBothPages();
    Console.WriteLine("✓ Category search test passed");
}
finally
{
    workflowTests.Dispose();
}

// Test 9: User deactivation blocks actions
try
{
    await workflowTests.DeactivatedUser_CannotCreateNewListings();
    Console.WriteLine("✓ Deactivation test passed");
}
finally
{
    workflowTests.Dispose();
}
```

## ReviewRepository

The `ReviewRepository` class provides data access operations for managing reviews within the marketplace system. It handles CRUD operations for review entities, including querying by reviewer, seller, or listing ID, checking for existing transactions, calculating average scores, and supporting paginated retrieval of reviews.

### Usage Example

```csharp
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Domain.Models;
using System;
using System.Threading.Tasks;

// Initialize review repository
var reviewRepository = new ReviewRepository();

// Add a new review
var newReview = await reviewRepository.AddAsync(new Review
{
    ReviewerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
    SellerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
    ListingId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
    Score = 5,
    Comment = "Excellent seller!"
});

Console.WriteLine($"Review added: {newReview.Id}");

// Get review by ID
var review = await reviewRepository.GetByIdAsync(newReview.Id);
Console.WriteLine($"Retrieved review score: {review?.Score}");

// Get average score for a seller
var averageScore = await reviewRepository.GetAverageScoreAsync(Guid.Parse("22222222-2222-2222-2222-222222222222"));
Console.WriteLine($"Seller average score: {averageScore:F1}");

// Get paginated reviews for a seller
var (reviews, total) = await reviewRepository.GetPagedBySellerAsync(
    sellerId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
    pageNumber: 1,
    pageSize: 10
);
Console.WriteLine($"Page 1 contains {reviews.Count} of {total} total reviews for the seller");

// Check if a review exists for a transaction
var exists = await reviewRepository.ExistsForTransactionAsync(
    reviewerId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    sellerId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
    listingId: Guid.Parse("33333333-3333-3333-3333-333333333333")
);
Console.WriteLine($"Review exists for transaction: {exists}");

// Update a review
newReview.Comment = "Updated comment: Excellent seller, fast shipping!";
await reviewRepository.UpdateAsync(newReview);
Console.WriteLine("Review updated successfully");

// Delete a review
await reviewRepository.DeleteAsync(newReview.Id);
Console.WriteLine("Review deleted");
```

## FullTextSearchRequest

The `FullTextSearchRequest` class represents a full-text search request with optional filters and pagination. It supports searching listings by query text, category, price range, tags, condition, and featured status, with configurable pagination for efficient result browsing.

### Usage Example

```csharp
using MarketplaceEngine.DTOs;
using System;
using System.Collections.Generic;

// Create a full-text search request for electronics under $500
var searchRequest = new FullTextSearchRequest
{
    Query = "wireless headphones",
    CategoryId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
    MaxPrice = 500,
    Tags = new List<string> { "electronics", "audio" },
    Condition = "new",
    FeaturedOnly = false,
    Page = 1,
    PageSize = 25
};

Console.WriteLine($"Searching for: {searchRequest.Query}");
Console.WriteLine($"Category: {searchRequest.CategoryId}");
Console.WriteLine($"Price range: ${searchRequest.MinPrice ?? 0} - ${searchRequest.MaxPrice ?? 0}");
Console.WriteLine($"Tags: {string.Join(", ", searchRequest.Tags ?? new List<string>())}");
Console.WriteLine($"Condition: {searchRequest.Condition ?? "any"}");
Console.WriteLine($"Featured only: {searchRequest.FeaturedOnly}");
Console.WriteLine($"Pagination: Page {searchRequest.Page}, {searchRequest.PageSize} per page");

// Create a search request with price range only
var budgetSearch = new FullTextSearchRequest
{
    Query = "laptop",
    MinPrice = 300,
    MaxPrice = 800,
    Page = 2,
    PageSize = 10
};
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

## ListingDto

The `ListingDto` class represents a product listing in the marketplace. It contains all essential information about a listing including the title, description, price, seller details, category, status, view count, and timestamps for creation and updates. This DTO is used throughout the application for displaying listings, creating new listings, and updating existing ones.

### Usage Example

```csharp
using MarketplaceEngine.DTOs;
using System;
using System.Collections.Generic;

// Create a new listing for a premium product
var listing = new ListingDto
{
    Id = Guid.NewGuid(),
    Title = "Premium Wireless Headphones",
    Description = "Noise-cancelling wireless headphones with 30-hour battery life, Bluetooth 5.0, and premium sound quality. Includes carrying case and 2-year warranty.",
    Price = 299.99m,
    SellerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
    SellerName = "AudioTech Solutions",
    CategoryId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
    Status = "Active",
    ViewCount = 156,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

// Display listing information
Console.WriteLine($"Listing: {listing.Title}");
Console.WriteLine($"Price: {listing.Price:C}");
Console.WriteLine($"Seller: {listing.SellerName}");
Console.WriteLine($"Category: {listing.CategoryId}");
Console.WriteLine($"Status: {listing.Status}");
Console.WriteLine($"Views: {listing.ViewCount}");
Console.WriteLine($"Created: {listing.CreatedAt:yyyy-MM-dd}");
Console.WriteLine($"Last updated: {listing.UpdatedAt?.ToString("yyyy-MM-dd") ?? "Never"}");

// Create a collection of listings for a category page
var categoryListings = new List<ListingDto>
{
    new ListingDto
    {
        Id = Guid.NewGuid(),
        Title = "Wireless Bluetooth Speaker",
        Description = "Portable wireless speaker with 20W output, IPX7 waterproof rating, 15-hour playtime.",
        Price = 89.99m,
        SellerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        SellerName = "SoundWave Audio",
        CategoryId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
        Status = "Active",
        ViewCount = 234,
        CreatedAt = DateTime.UtcNow.AddDays(-3),
        UpdatedAt = DateTime.UtcNow.AddDays(-1)
    },
    new ListingDto
    {
        Id = Guid.NewGuid(),
        Title = "Smart Watch with Heart Rate Monitor",
        Description = "Fitness tracker with heart rate monitoring, GPS, water resistance up to 50m, and smartphone notifications.",
        Price = 199.99m,
        SellerId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        SellerName = "FitTech Wearables",
        CategoryId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
        Status = "Active",
        ViewCount = 412,
        CreatedAt = DateTime.UtcNow.AddDays(-7),
        UpdatedAt = DateTime.UtcNow.AddDays(-2)
    }
};

// Display category listings
Console.WriteLine($"\nFound {categoryListings.Count} listings in category:");
foreach (var item in categoryListings.OrderBy(l => l.Price))
{
    Console.WriteLine($"- {item.Title}: {item.Price:C} ({item.ViewCount} views)");
}

// Update listing price
var listingToUpdate = categoryListings[0];
listingToUpdate.Price = 79.99m;
listingToUpdate.UpdatedAt = DateTime.UtcNow;

Console.WriteLine($"\nUpdated price for {listingToUpdate.Title}: {listingToUpdate.Price:C}");
```

## ListingService

The `ListingService` class provides core functionality for managing product listings within the marketplace. It handles listing creation, updates, visibility control, interest tracking, delisting, and retrieval operations including seller-specific, featured, recent, and paginated listings. The service also manages listing statistics and featured status.

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Initialize listing service (typically via dependency injection)
var listingService = new ListingService(listingRepository, categoryRepository);

// Create a new listing
var newListing = await listingService.CreateListingAsync(
    sellerId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    title: "Premium Wireless Headphones",
    description: "Noise-cancelling wireless headphones with 30-hour battery life",
    price: 299.99m,
    categoryId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
    isFeatured: true
);

Console.WriteLine($"Created listing: {newListing.Id} - {newListing.Title}");

// Update a listing (returns previous category ID)
var (updatedListing, previousCategoryId) = await listingService.UpdateListingAsync(
    listingId: newListing.Id,
    newTitle: "Premium Wireless Headphones - Updated",
    newDescription: "Noise-cancelling wireless headphones with 30-hour battery life and Bluetooth 5.0",
    newPrice: 349.99m,
    newCategoryId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")
);

Console.WriteLine($"Updated listing from category {previousCategoryId} to {updatedListing.CategoryId}");

// Set listing visibility
var visibleListing = await listingService.SetListingVisibilityAsync(
    listingId: newListing.Id,
    isVisible: false
);

Console.WriteLine($"Listing visibility set to: {visibleListing.IsVisible}");

// Get listing with view tracking
var listingWithView = await listingService.GetListingWithViewAsync(newListing.Id);
Console.WriteLine($"Listing views: {listingWithView.ViewCount}");

// Record user interest in a listing
var interestedListing = await listingService.RecordInterestAsync(
    listingId: newListing.Id,
    userId: Guid.Parse("22222222-2222-2222-2222-222222222222")
);

Console.WriteLine($"User interest recorded for listing");

// Get all listings for a specific seller
var sellerListings = await listingService.GetSellerListingsAsync(
    Guid.Parse("11111111-1111-1111-1111-111111111111")
);
Console.WriteLine($"Seller has {sellerListings.Count} active listings");

// Get featured listings
var featuredListings = await listingService.GetFeaturedListingsAsync(count: 10);
Console.WriteLine($"Found {featuredListings.Count} featured listings");

// Get recent listings
var recentListings = await listingService.GetRecentListingsAsync(count: 20);
Console.WriteLine($"Found {recentListings.Count} recent listings");

// Get paginated listings with total count
var (paginatedListings, totalCount) = await listingService.GetPaginatedListingsAsync(
    page: 1,
    pageSize: 25,
    categoryId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")
);
Console.WriteLine($"Page 1: {paginatedListings.Count} of {totalCount} total listings");

// Mark a listing as featured
var featuredListing = await listingService.MarkAsFeaturedAsync(
    listingId: newListing.Id,
    featuredUntil: DateTime.UtcNow.AddDays(7)
);
Console.WriteLine($"Listing marked as featured until: {featuredListing.FeaturedUntil}");

// Get total listing count
var totalListings = await listingService.GetTotalListingCountAsync();
Console.WriteLine($"Total listings in marketplace: {totalListings}");

// Delist a listing
var delistedListing = await listingService.DelistListingAsync(newListing.Id);
Console.WriteLine($"Listing delisted: {delistedListing.IsActive}");
```

## ListingServiceTests

The `ListingServiceTests` class provides unit tests for the `ListingService` class, verifying that it correctly handles various business rules and authorization scenarios. It tests listing creation with validation for seller existence and active status, delisting authorization based on ownership, and featured listing marking authorization based on admin privileges.

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Entities;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

// Initialize mock repositories
var listingRepoMock = new Mock<IListingRepository>();
var userRepoMock = new Mock<IUserRepository>();

// Create the service under test
var listingService = new ListingService(listingRepoMock.Object, userRepoMock.Object);

// Test 1: CreateListingAsync throws ResourceNotFoundException when seller is not found
var nonExistentSellerId = Guid.NewGuid();
userRepoMock.Setup(r => r.GetByIdAsync(nonExistentSellerId))
    .ReturnsAsync((User)null);

var act1 = async () => await listingService.CreateListingAsync(
    sellerId: nonExistentSellerId,
    title: "Quality Acoustic Guitar",
    description: "A well-maintained acoustic guitar suitable for beginners and intermediate players.",
    price: 149.99m,
    currency: "USD",
    categoryId: Guid.NewGuid(),
    imageUrls: new[] { "https://img.example.com/guitar.jpg" }
);

await Assert.ThrowsAsync<ResourceNotFoundException>(act1);

// Test 2: CreateListingAsync throws UnauthorizedException when seller is inactive
var inactiveSellerId = Guid.NewGuid();
var inactiveSeller = new User { Id = inactiveSellerId, IsActive = false };
userRepoMock.Setup(r => r.GetByIdAsync(inactiveSellerId))
    .ReturnsAsync(inactiveSeller);

var act2 = async () => await listingService.CreateListingAsync(
    sellerId: inactiveSellerId,
    title: "Quality Acoustic Guitar",
    description: "A well-maintained acoustic guitar suitable for beginners and intermediate players.",
    price: 149.99m,
    currency: "USD",
    categoryId: Guid.NewGuid(),
    imageUrls: new[] { "https://img.example.com/guitar.jpg" }
);

await Assert.ThrowsAsync<UnauthorizedException>(act2);

// Test 3: DelistListingAsync throws UnauthorizedException when caller is not the seller
var ownerId = Guid.NewGuid();
var requesterId = Guid.NewGuid();
var listingId = Guid.NewGuid();

var listing = new Listing { Id = listingId, SellerId = ownerId, Status = ListingStatus.Active };
listingRepoMock.Setup(r => r.GetByIdAsync(listingId))
    .ReturnsAsync(listing);

var act3 = async () => await listingService.DelistListingAsync(listingId, requesterId);

await Assert.ThrowsAsync<UnauthorizedException>(act3);

// Test 4: MarkAsFeaturedAsync throws UnauthorizedException when caller is not an admin
var nonAdminId = Guid.NewGuid();
var regularUser = new User { Id = nonAdminId, Role = UserRole.User };
userRepoMock.Setup(r => r.GetByIdAsync(nonAdminId))
    .ReturnsAsync(regularUser);

var act4 = async () => await listingService.MarkAsFeaturedAsync(listingId, nonAdminId);

await Assert.ThrowsAsync<UnauthorizedException>(act4);
```

## PaymentServiceTests

The `PaymentServiceTests` class contains unit tests for the `PaymentService` class, verifying that it correctly handles various payment operations and authorization scenarios. It tests payment initiation with validation for listing existence, listing status, and buyer identity; payment cancellation authorization; refund validation; and payment completion with listing delisting.

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Entities;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

// Initialize mock repositories
var paymentRepoMock = new Mock<IPaymentRepository>();
var listingRepoMock = new Mock<IListingRepository>();
var userRepoMock = new Mock<IUserRepository>();

// Create the service under test
var paymentService = new PaymentService(
    paymentRepoMock.Object,
    listingRepoMock.Object,
    userRepoMock.Object
);

// Test 1: InitiatePaymentAsync throws ResourceNotFoundException when listing is not found
var nonExistentListingId = Guid.NewGuid();
listingRepoMock.Setup(r => r.GetByIdAsync(nonExistentListingId))
    .ReturnsAsync((Listing)null);

var act1 = async () => await paymentService.InitiatePaymentAsync(
    listingId: nonExistentListingId,
    buyerId: Guid.NewGuid(),
    paymentMethod: "credit_card"
);

await Assert.ThrowsAsync<ResourceNotFoundException>(act1);

// Test 2: InitiatePaymentAsync throws MarketplaceException when listing is inactive
var inactiveListingId = Guid.NewGuid();
var inactiveListing = new Listing {
    Id = inactiveListingId,
    Status = ListingStatus.Inactive,
    Price = new Money(100m)
};
listingRepoMock.Setup(r => r.GetByIdAsync(inactiveListingId))
    .ReturnsAsync(inactiveListing);

var act2 = async () => await paymentService.InitiatePaymentAsync(
    listingId: inactiveListingId,
    buyerId: Guid.NewGuid(),
    paymentMethod: "credit_card"
);

await Assert.ThrowsAsync<MarketplaceException>(act2);

// Test 3: InitiatePaymentAsync throws MarketplaceException when buyer is seller
var sellerId = Guid.NewGuid();
var listingId = Guid.NewGuid();
var sellerListing = new Listing {
    Id = listingId,
    SellerId = sellerId,
    Status = ListingStatus.Active,
    Price = new Money(200m)
};
var sellerUser = new User { Id = sellerId, IsActive = true };

listingRepoMock.Setup(r => r.GetByIdAsync(listingId))
    .ReturnsAsync(sellerListing);
userRepoMock.Setup(r => r.GetByIdAsync(sellerId))
    .ReturnsAsync(sellerUser);

var act3 = async () => await paymentService.InitiatePaymentAsync(
    listingId: listingId,
    buyerId: sellerId,
    paymentMethod: "credit_card"
);

await Assert.ThrowsAsync<MarketplaceException>(act3);

// Test 4: InitiatePaymentAsync with valid data creates payment
var validListingId = Guid.NewGuid();
var validBuyerId = Guid.NewGuid();
var validSellerId = Guid.NewGuid();
var validListing = new Listing {
    Id = validListingId,
    SellerId = validSellerId,
    Status = ListingStatus.Active,
    Price = new Money(150m)
};
var validBuyer = new User { Id = validBuyerId, IsActive = true };

listingRepoMock.Setup(r => r.GetByIdAsync(validListingId))
    .ReturnsAsync(validListing);
userRepoMock.Setup(r => r.GetByIdAsync(validBuyerId))
    .ReturnsAsync(validBuyer);
paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>()))
    .ReturnsAsync((Payment p) => { p.Id = Guid.NewGuid(); return p; });

var payment = await paymentService.InitiatePaymentAsync(
    listingId: validListingId,
    buyerId: validBuyerId,
    paymentMethod: "bank_transfer"
);

Assert.NotNull(payment);
Assert.Equal(validBuyerId, payment.BuyerId);
Assert.Equal(validSellerId, payment.SellerId);
Assert.Equal(PaymentStatus.Pending, payment.Status);

// Test 5: CancelPaymentAsync throws UnauthorizedException when caller is not buyer
var paymentId = Guid.NewGuid();
var otherUserId = Guid.NewGuid();
var pendingPayment = new Payment {
    Id = paymentId,
    BuyerId = validBuyerId,
    Status = PaymentStatus.Pending,
    Amount = new Money(150m)
};

paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId))
    .ReturnsAsync(pendingPayment);

var act5 = async () => await paymentService.CancelPaymentAsync(
    paymentId: paymentId,
    requesterId: otherUserId
);

await Assert.ThrowsAsync<UnauthorizedException>(act5);

// Test 6: RefundPaymentAsync throws InvalidOperationException when payment is pending
var pendingPaymentForRefund = new Payment {
    Id = paymentId,
    BuyerId = validBuyerId,
    Status = PaymentStatus.Pending,
    Amount = new Money(200m)
};

paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId))
    .ReturnsAsync(pendingPaymentForRefund);

var act6 = async () => await paymentService.RefundPaymentAsync(
    paymentId: paymentId,
    reason: "Changed mind"
);

await Assert.ThrowsAsync<InvalidOperationException>(act6);

// Test 7: CompletePaymentAsync with valid transaction ID marks listing as delisted
var processingPaymentId = Guid.NewGuid();
var processingPayment = new Payment {
    Id = processingPaymentId,
    BuyerId = validBuyerId,
    SellerId = validSellerId,
    ListingId = validListingId,
    Status = PaymentStatus.Processing,
    Amount = new Money(150m)
};
var activeListing = new Listing {
    Id = validListingId,
    SellerId = validSellerId,
    Status = ListingStatus.Active,
    Price = new Money(150m)
};
var seller = new User { Id = validSellerId, IsActive = true };

paymentRepoMock.Setup(r => r.GetByIdAsync(processingPaymentId))
    .ReturnsAsync(processingPayment);
paymentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
    .ReturnsAsync((Payment p) => p);
listingRepoMock.Setup(r => r.GetByIdAsync(validListingId))
    .ReturnsAsync(activeListing);
listingRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Listing>()))
    .ReturnsAsync((Listing l) => l);
userRepoMock.Setup(r => r.GetByIdAsync(validSellerId))
    .ReturnsAsync(seller);
userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>()))
    .ReturnsAsync((User u) => u);

var completedPayment = await paymentService.CompletePaymentAsync(
    paymentId: processingPaymentId,
    transactionId: "txn_abc123"
);

Assert.Equal(PaymentStatus.Completed, completedPayment.Status);
Assert.Equal(ListingStatus.Delisted, activeListing.Status);
```

## ListingsController

The `ListingsController` class provides RESTful API endpoints for managing marketplace listings. It handles CRUD operations for listings, search functionality, and integrates with caching services to improve performance. The controller supports pagination, individual listing retrieval, creation, updates, and full-text search capabilities.

### Usage Example

```csharp
using MarketplaceEngine.Controllers;
using MarketplaceEngine.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// Initialize HTTP client for API calls
var client = new HttpClient { BaseAddress = new Uri("https://api.marketplace.example.com") };

// Get paginated listings (cached for 5 minutes)
var response = await client.GetAsync("/api/v1/listings?page=1&pageSize=20");
var listingsPage = await response.Content.ReadFromJsonAsync<PaginatedResponse<ListingDto>>();
Console.WriteLine($"Found {listingsPage.Total} total listings, showing page {listingsPage.Page}");

// Get a specific listing by ID (cached for 2 minutes)
var listingResponse = await client.GetAsync($"/api/v1/listings/{Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")}");
var listing = await listingResponse.Content.ReadFromJsonAsync<ListingDto>();
Console.WriteLine($"Retrieved listing: {listing.Title} for {listing.Price:C}");

// Create a new listing
var newListing = new CreateListingRequest
{
    Title = "Premium Wireless Headphones",
    Description = "Noise-cancelling wireless headphones with 30-hour battery life",
    Price = 299.99m,
    SellerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
    CategoryId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
    ImageUrls = new List<string> { "https://example.com/image1.jpg" }
};

var createResponse = await client.PostAsJsonAsync("/api/v1/listings", newListing);
var createdListing = await createResponse.Content.ReadFromJsonAsync<ListingDto>();
Console.WriteLine($"Created listing: {createdListing.Id} - {createdListing.Title}");

// Update an existing listing
var updateRequest = new UpdateListingRequest
{
    Title = "Premium Wireless Headphones - Updated",
    Description = "Noise-cancelling wireless headphones with 30-hour battery life and Bluetooth 5.0",
    Price = 349.99m,
    CategoryId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")
};

var updateResponse = await client.PutAsJsonAsync(
    $"/api/v1/listings/{createdListing.Id}",
    updateRequest
);
var updatedListing = await updateResponse.Content.ReadFromJsonAsync<ListingDto>();
Console.WriteLine($"Updated listing: {updatedListing.Title}");

// Search listings
var searchResponse = await client.GetAsync("/api/v1/listings/search?q=wireless&limit=10");
var searchResults = await searchResponse.Content.ReadFromJsonAsync<SearchResultDto>();
Console.WriteLine($"Search found {searchResults.Results.Count} listings matching 'wireless'");
```

## MessagesController

The `MessagesController` class provides RESTful API endpoints for managing user-to-user messaging within the marketplace. It handles conversation management, message retrieval, sending, marking messages as read, and deletion. The controller integrates with the `MessagingService` for business logic and uses cursor-based pagination for conversations to prevent duplicate messages when new messages are added concurrently.


### Usage Example

```csharp
using MarketplaceEngine.Controllers;
using MarketplaceEngine.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// Initialize HTTP client for API calls
var client = new HttpClient { BaseAddress = new Uri("https://api.marketplace.example.com") };

// Get conversations for a user (cached)
var conversationsResponse = await client.GetAsync($"/api/v1/messages/conversations/{Guid.Parse("11111111-1111-1111-1111-111111111111")}");
var conversations = await conversationsResponse.Content.ReadFromJsonAsync<List<ConversationDto>>();
Console.WriteLine($"User has {conversations.Count} conversations");

// Get messages in a conversation with cursor-based pagination
var messagesResponse = await client.GetAsync($"/api/v1/messages/conversations/{Guid.Parse("22222222-2222-2222-2222-222222222222")}/messages?pageSize=25");
var messagesPage = await messagesResponse.Content.ReadFromJsonAsync<CursorPaginatedResponse<MessageDto>>();
Console.WriteLine($"Found {messagesPage.Items.Count} messages in conversation");

// Send a new message
var newMessage = new SendMessageRequest
{
  SenderId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
  RecipientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
  Content = "Hello! I'm interested in your premium wireless headphones. Could you provide more details about the condition and shipping options?"
};

var sendResponse = await client.PostAsJsonAsync("/api/v1/messages", newMessage);
var sentMessage = await sendResponse.Content.ReadFromJsonAsync<MessageDto>();
Console.WriteLine($"Message sent: {sentMessage.Id} - {sentMessage.Body}");

// Get a specific message by ID
var messageResponse = await client.GetAsync($"/api/v1/messages/{sentMessage.Id}");
var message = await messageResponse.Content.ReadFromJsonAsync<MessageDto>();
Console.WriteLine($"Retrieved message: {message.Body}");

// Mark a message as read
var markReadResponse = await client.PutAsync($"/api/v1/messages/{sentMessage.Id}/read", null);
Console.WriteLine("Message marked as read");

// Delete a message
var deleteResponse = await client.DeleteAsync($"/api/v1/messages/{sentMessage.Id}");
Console.WriteLine("Message deleted successfully");
```

## PaymentsController

The `PaymentsController` class provides RESTful API endpoints for managing the complete payment lifecycle in the marketplace. It handles payment initiation, processing, escrow management, completion, cancellation, and refunds. The controller integrates with the `PaymentService` to process transactions and manage funds throughout the purchase workflow, ensuring secure handling of buyer funds and seller payouts.

### Usage Example

```csharp
using MarketplaceEngine.Controllers;
using MarketplaceEngine.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// Initialize HTTP client for API calls
var client = new HttpClient { BaseAddress = new Uri("https://api.marketplace.example.com") };

// Initiate a new payment for a listing purchase
var initiateRequest = new InitiatePaymentRequest
{
  ListingId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
  BuyerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
  PaymentMethod = "CreditCard",
  Currency = "USD"
};

var initiateResponse = await client.PostAsJsonAsync("/api/v1/payments", initiateRequest);
var initiatedPayment = await initiateResponse.Content.ReadFromJsonAsync<PaymentDto>();
Console.WriteLine($"Payment initiated: {initiatedPayment.Id} - {initiatedPayment.Amount:C}");

// Get payment details by ID
var paymentResponse = await client.GetAsync($"/api/v1/payments/{initiatedPayment.Id}");
var payment = await paymentResponse.Content.ReadFromJsonAsync<PaymentDto>();
Console.WriteLine($"Retrieved payment: Status={payment.Status}, Amount={payment.Amount:C}");

// Start processing the payment (transition to processing state)
var processResponse = await client.PostAsync($"/api/v1/payments/{initiatedPayment.Id}/process?requesterId={Guid.Parse("11111111-1111-1111-1111-111111111111")}", null);
var processingPayment = await processResponse.Content.ReadFromJsonAsync<PaymentDto>();
Console.WriteLine($"Payment processing started: {processingPayment.Status}");

// Complete the payment after external provider confirmation
var completeRequest = new CompletePaymentRequest
{
  ExternalTransactionId = "txn_1234567890"
};

var completeResponse = await client.PostAsJsonAsync($"/api/v1/payments/{initiatedPayment.Id}/complete", completeRequest);
var completedPayment = await completeResponse.Content.ReadFromJsonAsync<PaymentDto>();
Console.WriteLine($"Payment completed: {completedPayment.Status}");

// Move payment to escrow (funds held until delivery confirmed)
var escrowResponse = await client.PostAsync($"/api/v1/payments/{initiatedPayment.Id}/escrow", null);
var escrowPayment = await escrowResponse.Content.ReadFromJsonAsync<PaymentDto>();
Console.WriteLine($"Payment moved to escrow: {escrowPayment.Status}");

// Release funds to seller after successful delivery confirmation
var releaseRequest = new CompletePaymentRequest
{
  ExternalTransactionId = "payout_9876543210"
};

var releaseResponse = await client.PostAsJsonAsync($"/api/v1/payments/{initiatedPayment.Id}/escrow/release", releaseRequest);
var releasedPayment = await releaseResponse.Content.ReadFromJsonAsync<PaymentDto>();
Console.WriteLine($"Funds released to seller: {releasedPayment.Status}");

// Get all payments made by a specific buyer
var buyerPaymentsResponse = await client.GetAsync($"/api/v1/payments/buyer/{Guid.Parse("11111111-1111-1111-1111-111111111111")}");
var buyerPayments = await buyerPaymentsResponse.Content.ReadFromJsonAsync<List<PaymentDto>>();
Console.WriteLine($"Buyer has {buyerPayments.Count} payments");

// Get all payments received by a specific seller
var sellerPaymentsResponse = await client.GetAsync($"/api/v1/payments/seller/{Guid.Parse("22222222-2222-2222-2222-222222222222")}");
var sellerPayments = await sellerPaymentsResponse.Content.ReadFromJsonAsync<List<PaymentDto>>();
Console.WriteLine($"Seller has {sellerPayments.Count} payments");

// Cancel a pending payment
var cancelResponse = await client.PostAsync($"/api/v1/payments/{initiatedPayment.Id}/cancel?requesterId={Guid.Parse("11111111-1111-1111-1111-111111111111")}", null);
var cancelledPayment = await cancelResponse.Content.ReadFromJsonAsync<PaymentDto>();
Console.WriteLine($"Payment cancelled: {cancelledPayment.Status}");

// Refund a completed payment
var refundRequest = new RefundPaymentRequest
{
  Reason = "Buyer requested refund due to item not as described"
};

var refundResponse = await client.PostAsJsonAsync($"/api/v1/payments/{initiatedPayment.Id}/refund", refundRequest);
var refundedPayment = await refundResponse.Content.ReadFromJsonAsync<PaymentDto>();
Console.WriteLine($"Payment refunded: {refundedPayment.Status}");
```

## UserService

The `UserService` class provides core user management functionality for the Marketplace Engine, handling user registration, authentication, profile management, account status operations, and user statistics. It manages the complete user lifecycle from initial registration through account deactivation and reactivation, including email verification and premium account features.

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Entities;
using System;
using System.Threading.Tasks;

// Initialize user service (typically via dependency injection)
var userService = new UserService(userRepository, emailService);

// Register a new user
var newUser = await userService.RegisterUserAsync(
    email: "user@example.com",
    password: "SecurePassword123!",
    displayName: "TechEnthusiast",
    role: UserRole.Buyer
);

Console.WriteLine($"User registered: {newUser.Id} - {newUser.Email}");

// Get user by ID
var user = await userService.GetUserAsync(newUser.Id);
Console.WriteLine($"Retrieved user: {user.DisplayName} ({user.Email})");

// Get user by email
var userByEmail = await userService.GetUserByEmailAsync("user@example.com");
Console.WriteLine($"Found user by email: {userByEmail.DisplayName}");

// Update user profile
var updatedUser = await userService.UpdateProfileAsync(
    userId: newUser.Id,
    displayName: "TechEnthusiast Pro",
    bio: "Technology enthusiast and gadget collector",
    avatarUrl: "https://example.com/avatars/tech-enthusiast.jpg"
);
Console.WriteLine($"Profile updated: {updatedUser.DisplayName}");

// Verify email (after user clicks verification link)
var verifiedUser = await userService.VerifyEmailAsync(
    userId: newUser.Id,
    token: "email-verification-token-from-email"
);
Console.WriteLine($"Email verified: {verifiedUser.EmailVerified}");

// Resend verification token
var resentToken = await userService.ResendVerificationTokenAsync(newUser.Id);
Console.WriteLine($"Verification token resent to: {newUser.Email}");

// Promote user to premium account
var premiumUser = await userService.PromoteToPremiumAsync(
    userId: newUser.Id,
    premiumUntil: DateTime.UtcNow.AddYears(1)
);
Console.WriteLine($"Premium status: {premiumUser.IsPremium} until {premiumUser.PremiumUntil}");

// Deactivate account (soft delete)
var deactivatedUser = await userService.DeactivateAccountAsync(newUser.Id);
Console.WriteLine($"Account deactivated: {deactivatedUser.IsActive}");

// Reactivate account
var reactivatedUser = await userService.ReactivateAccountAsync(newUser.Id);
Console.WriteLine($"Account reactivated: {reactivatedUser.IsActive}");

// Record a sale for user statistics
var userWithSale = await userService.RecordSaleAsync(newUser.Id);
Console.WriteLine($"Sale recorded. Total sales: {userWithSale.TotalSales}");

// Update user rating
var userWithRating = await userService.UpdateRatingAsync(
    userId: newUser.Id,
    newRating: 4.8,
    newReviewCount: 15
);
Console.WriteLine($"Rating updated: {userWithRating.AverageRating}/5 ({userWithRating.TotalReviews} reviews)");

// Get top sellers by rating
var topSellers = await userService.GetTopSellersAsync(count: 10);
Console.WriteLine($"Top {topSellers.Count} sellers:");
foreach (var seller in topSellers.Take(3))
{
    Console.WriteLine($"- {seller.DisplayName}: {seller.AverageRating:F1}/5");
}

// Get paginated users with total count
var (usersPage, totalCount) = await userService.GetPaginatedUsersAsync(
    page: 1,
    pageSize: 25,
    role: UserRole.Seller
);
Console.WriteLine($"Page 1: {usersPage.Count} of {totalCount} sellers");

// Update user's last activity timestamp
await userService.UpdateLastActivityAsync(newUser.Id);
Console.WriteLine("Last activity updated");

// Get verified user count
var verifiedCount = await userService.GetVerifiedUserCountAsync();
Console.WriteLine($"Verified users: {verifiedCount}");

// Get active user count
var activeCount = await userService.GetActiveUserCountAsync();
Console.WriteLine($"Active users: {activeCount}");

// Validate user access (for authorization checks)
await userService.ValidateUserAccessAsync(newUser.Id);
Console.WriteLine("User access validated");

// Get public profile (visible to other users)
var publicProfile = await userService.GetPublicProfileAsync(newUser.Id);
Console.WriteLine($"Public profile: {publicProfile.DisplayName} - {publicProfile.Bio}");
```

## UserRepository

The `UserRepository` class provides data access operations for user entities in the Marketplace Engine. It handles CRUD operations for users, including querying by ID, email, role, verification status, location, and active status. The repository also provides methods for searching users, checking email existence, and retrieving paginated user lists.

### Usage Example

```csharp
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Domain.Models;
using System;
using System.Threading.Tasks;

// Initialize user repository
var userRepository = new UserRepository();

// Add a new user to the system
var newUser = await userRepository.AddAsync(new User
{
    Email = "john.doe@example.com",
    FullName = "John Doe",
    PasswordHash = "hashed-password",
    Role = UserRole.Buyer,
    IsActive = true,
    IsVerified = false,
    EmailVerified = false,
    CreatedAt = DateTime.UtcNow
});

Console.WriteLine($"User created: {newUser.Id} - {newUser.Email}");

// Get a user by ID
var user = await userRepository.GetByIdAsync(newUser.Id);
Console.WriteLine($"Retrieved user: {user?.FullName} ({user?.Email})");

// Get all users in the system
var allUsers = await userRepository.GetAllAsync();
Console.WriteLine($"Total users in system: {allUsers.Count}");

// Update a user's profile
user.FullName = "John Doe Updated";
user.Bio = "Technology enthusiast and software developer";
var updatedUser = await userRepository.UpdateAsync(user);
Console.WriteLine($"User updated: {updatedUser.FullName}");

// Check if a user exists
var exists = await userRepository.ExistsAsync(newUser.Id);
Console.WriteLine($"User exists: {exists}");

// Get total user count
var userCount = await userRepository.CountAsync();
Console.WriteLine($"Total users: {userCount}");

// Get user by email
var userByEmail = await userRepository.GetByEmailAsync("john.doe@example.com");
Console.WriteLine($"Found user by email: {userByEmail?.FullName}");

// Get all active users
var activeUsers = await userRepository.GetActiveUsersAsync();
Console.WriteLine($"Active users: {activeUsers.Count}");

// Get all verified users
var verifiedUsers = await userRepository.GetVerifiedUsersAsync();
Console.WriteLine($"Verified users: {verifiedUsers.Count}");

// Get users by role (e.g., UserRole.Seller = 2)
var sellers = await userRepository.GetByRoleAsync(2);
Console.WriteLine($"Sellers: {sellers.Count}");

// Search for users by name or email
var searchResults = await userRepository.SearchAsync("John");
Console.WriteLine($"Search results: {searchResults.Count} users found");

// Get top sellers by rating and sales
var topSellers = await userRepository.GetTopSellersAsync(limit: 10);
Console.WriteLine($"Top sellers: {topSellers.Count} sellers found");

## UtilityTests

The `UtilityTests` class provides comprehensive utility functions for common marketplace operations including input validation, text processing, and data formatting. It includes methods for email validation, price validation, input sanitization, text truncation, URL-friendly slug generation, email masking, pagination calculations, and various string utilities that help maintain data quality and consistency across the marketplace system.

### Usage Example

```csharp
using MarketplaceEngine.Tests;
using System;

// Initialize utility tests helper
var utilityTests = new UtilityTests();

// Validate email addresses
var validEmail = utilityTests.IsValidEmail_VariousInputs_ReturnsExpectedResult("test@example.com");
Console.WriteLine($"Valid email test: {validEmail}");

var invalidEmail = utilityTests.IsValidEmail_VariousInputs_ReturnsExpectedResult("invalid-email");
Console.WriteLine($"Invalid email test: {invalidEmail}");

// Validate price minimum threshold
var validPrice = utilityTests.IsValidPrice_BelowMinimum_ReturnsFalse(25.00m);
Console.WriteLine($"Price validation (25.00): {validPrice}");

var invalidPrice = utilityTests.IsValidPrice_BelowMinimum_ReturnsFalse(0.50m);
Console.WriteLine($"Price validation (0.50): {invalidPrice}");

// Sanitize user input to remove control characters
var sanitizedInput = utilityTests.SanitizeInput_WithNullControlCharacters_RemovesThem("Hello WorldTest");
Console.WriteLine($"Sanitized input: {sanitizedInput}");

// Truncate text with ellipsis
var truncatedText = utilityTests.Truncate_WhenTextExceedsMaxLength_TruncatesAndAppendsEllipsis(
    "This is a very long text that needs to be truncated",
    20
);
Console.WriteLine($"Truncated text: {truncatedText}");

// Convert text to URL-friendly slug
var slug = utilityTests.ToSlug_WithSpecialCharactersAndSpaces_ReturnsUrlFriendlySlug(
    "Marketplace & E-commerce Platform - 2024 Edition!"
);
Console.WriteLine($"Generated slug: {slug}");

// Mask email addresses for privacy
var maskedEmail = utilityTests.MaskEmail_WithTypicalEmail_MasksLocalPartAndPreservesDomain(
    "user.name@example.com"
);
Console.WriteLine($"Masked email: {maskedEmail}");

// Calculate pagination offsets
var offset = utilityTests.CalculateOffset_ForPage2WithSize10(2, 10);
Console.WriteLine($"Offset for page 2 (size 10): {offset}");

// Calculate total pages with ceiling
var totalPages = utilityTests.CalculateTotalPages_WithNonDivisibleTotal_CeilsUp(123, 10);
Console.WriteLine($"Total pages for 123 items (page size 10): {totalPages}");
```

// Get users by location
var usersInNY = await userRepository.GetByLocationAsync("New York", "US");
Console.WriteLine($"Users in New York, US: {usersInNY.Count}");

// Check if email exists (useful for registration validation)
var emailExists = await userRepository.EmailExistsAsync("john.doe@example.com");
Console.WriteLine($"Email exists: {emailExists}");

// Get user by verification token (for email verification flow)
var userByToken = await userRepository.GetByVerificationTokenAsync("verification-token-here");
Console.WriteLine($"User by token: {userByToken?.Email}");

// Get paginated users with total count
var (pagedUsers, totalCount) = await userRepository.GetPagedAsync(pageNumber: 1, pageSize: 25);
Console.WriteLine($"Page 1: {pagedUsers.Count} of {totalCount} total users");

// Update user's last activity timestamp
await userRepository.UpdateLastActivityAsync(newUser.Id);
Console.WriteLine("Last activity updated");

// Delete a user (soft delete via repository)
await userRepository.DeleteAsync(newUser.Id);
Console.WriteLine("User deleted");
```

## ModerationController

The `ModerationController` class provides RESTful API endpoints for managing moderation tasks including report review and action enforcement. It handles the complete moderation workflow from report creation through review and resolution. The controller integrates with the `ModerationService` to process reports and enforce actions against listings or users, while managing cache invalidation to ensure data consistency after moderation actions.

### Usage Example

```csharp
using MarketplaceEngine.Controllers;
using MarketplaceEngine.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// Initialize HTTP client for API calls
var client = new HttpClient { BaseAddress = new Uri("https://api.marketplace.example.com") };

// Create a moderation report for inappropriate content
var reportRequest = new CreateReportRequest
{
    TargetListingId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), // Listing to report
    TargetUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), // Optional: user to report
    ReporterId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), // User reporting the content
    Reason = "This listing contains inappropriate content and violates our terms of service"
};

var createReportResponse = await client.PostAsJsonAsync("/api/v1/moderation/reports", reportRequest);
var createdReport = await createReportResponse.Content.ReadFromJsonAsync<ModerationReportDto>();
Console.WriteLine($"Report created: {createdReport.Id} - Status: {createdReport.Status}");

// Get pending reports for moderator review (not cached - real-time updates)
var pendingReportsResponse = await client.GetAsync("/api/v1/moderation/reports?page=1&pageSize=20");
var pendingReports = await pendingReportsResponse.Content.ReadFromJsonAsync<PaginatedResponse<ModerationReportDto>>();
Console.WriteLine($"Found {pendingReports.Total} pending reports");

// Get detailed report information including related entities
var reportDetailsResponse = await client.GetAsync($"/api/v1/moderation/reports/{createdReport.Id}");
var reportDetails = await reportDetailsResponse.Content.ReadFromJsonAsync<ModerationReportDetailsDto>();
Console.WriteLine($"Report details: {reportDetails.Reason}");
Console.WriteLine($"Target listing: {reportDetails.TargetListingId}");
Console.WriteLine($"Target user: {reportDetails.TargetUserId}");

// Approve a report and take moderation action
var approveRequest = new ApproveReportRequest
{
    ActionNotes = "Listing removed due to violation of content policy - inappropriate images detected"
};

var approveResponse = await client.PostAsJsonAsync($"/api/v1/moderation/reports/{createdReport.Id}/approve", approveRequest);
var approveResult = await approveResponse.Content.ReadFromJsonAsync<object>();
Console.WriteLine($"Report approved: {approveResult}");

// Get moderation statistics for dashboard
var statsResponse = await client.GetAsync("/api/v1/moderation/statistics");
var statistics = await statsResponse.Content.ReadFromJsonAsync<ModerationStatsDto>();
Console.WriteLine($"Moderation Statistics:");
Console.WriteLine($"- Pending reports: {statistics.PendingReports}");
Console.WriteLine($"- Approved reports: {statistics.ApprovedReports}");
Console.WriteLine($"- Rejected reports: {statistics.RejectedReports}");
Console.WriteLine($"- Average resolution time: {statistics.AverageResolutionTime} hours");

// Bulk moderation action (moderator/admin only)
var bulkRequest = new BulkModerationRequest
{
    ListingIds = new List<Guid> {
        Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
        Guid.Parse("4fa85f64-5717-4562-b3fc-2c963f66afa7")
    },
    Action = "remove" // Action can be: approve, remove, escalate
};

// Note: In real usage, include X-User-Role header: "Moderator" or "Administrator"
var bulkResponse = await client.PostAsJsonAsync("/api/v1/moderation/bulk", bulkRequest);
var bulkResult = await bulkResponse.Content.ReadFromJsonAsync<BulkModerationResponse>();
Console.WriteLine($"Bulk action completed. Success: {bulkResult.Results.Count(r => r.Success)}/{bulkResult.Results.Count}");
```

## HealthController

The `HealthController` class provides health monitoring endpoints for the Marketplace Engine application. It implements both basic liveness checks for container orchestration systems and detailed readiness checks that validate critical dependencies including database connectivity and cache availability. The controller returns comprehensive status information including service version, uptime metrics, and dependency health.

### Usage Example

```csharp
using MarketplaceEngine.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// Initialize test client for API calls
var client = new HttpClient { BaseAddress = new Uri("https://api.marketplace.example.com") };

// Basic health check (liveness probe)
var healthResponse = await client.GetAsync("/api/v1/health");
var healthData = await healthResponse.Content.ReadFromJsonAsync<HealthController.HealthResponse>();

Console.WriteLine($"Health Status: {healthData.Status}");
Console.WriteLine($"Version: {healthData.Version}");
Console.WriteLine($"Uptime: {healthData.Uptime}");
Console.WriteLine($"Timestamp: {healthData.Timestamp:yyyy-MM-dd HH:mm:ss}");

// Detailed readiness check with dependency validation
var readinessResponse = await client.GetAsync("/api/v1/health/ready");
var readinessData = await readinessResponse.Content.ReadFromJsonAsync<HealthController.DetailedHealthResponse>();

Console.WriteLine($"\nReadiness Status: {readinessData.Status}");
Console.WriteLine($"Dependencies:");
foreach (var dependency in readinessData.Dependencies)
{
    Console.WriteLine($"  - {dependency.Key}: {dependency.Value}");
}

// Simple liveness probe for container orchestration
var livenessResponse = await client.GetAsync("/api/v1/health/live");
Console.WriteLine($"\nLiveness check status: {(int)livenessResponse.StatusCode} - {(livenessResponse.IsSuccessStatusCode ? "Service available" : "Service unavailable")}");
```

## CategoriesController

The `CategoriesController` class provides RESTful API endpoints for managing product categories in the marketplace. It handles CRUD operations for categories, category hierarchy management, and retrieval of category statistics. The controller supports retrieving individual categories, category trees, listing counts, and category-specific statistics including average prices and view metrics.

### Usage Example

```csharp
using MarketplaceEngine.Controllers;
using MarketplaceEngine.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// Initialize HTTP client for API calls
var client = new HttpClient { BaseAddress = new Uri("https://api.marketplace.example.com") };

// Get all categories with hierarchy
var categoriesResponse = await client.GetAsync("/api/v1/categories/tree");
var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<CategoryDto>>();
Console.WriteLine($"Found {categories.Count} root categories");

// Get a specific category by ID
var categoryId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
var categoryResponse = await client.GetAsync($"/api/v1/categories/{categoryId}");
var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryDto>();
Console.WriteLine($"Retrieved category: {category.Name} ({category.Description})");
Console.WriteLine($"Listing count: {category.ListingCount}");
Console.WriteLine($"Subcategories: {category.SubCategories?.Count ?? 0}");

// Get category statistics
var statsResponse = await client.GetAsync($"/api/v1/categories/{categoryId}/statistics");
var statistics = await statsResponse.Content.ReadFromJsonAsync<CategoryStatisticsDto>();
Console.WriteLine($"Category statistics:");
Console.WriteLine($"- Total listings: {statistics.TotalListings}");
Console.WriteLine($"- Average price: {statistics.AveragePrice:C}");
Console.WriteLine($"- Total views: {statistics.TotalViews}");
Console.WriteLine($"- Average views: {statistics.AverageViews:F1}");

// Get category listings
var listingsResponse = await client.GetAsync($"/api/v1/categories/{categoryId}/listings");
var listings = await listingsResponse.Content.ReadFromJsonAsync<List<ListingDto>>();
Console.WriteLine($"Found {listings.Count} listings in category");

// Create a new category (admin only)
var newCategory = new CreateCategoryRequest
{
    Name = "Smart Home Devices",
    Description = "Smart home automation devices and accessories",
    ParentCategoryId = categoryId
};

var createResponse = await client.PostAsJsonAsync("/api/v1/categories", newCategory);
var createdCategory = await createResponse.Content.ReadFromJsonAsync<CategoryDto>();
Console.WriteLine($"Created category: {createdCategory.Id} - {createdCategory.Name}");

// Update an existing category (admin only)
var updateRequest = new UpdateCategoryRequest
{
    Name = "Smart Home & IoT Devices",
    Description = "Smart home devices, IoT gadgets, and home automation equipment"
};

var updateResponse = await client.PutAsJsonAsync($"/api/v1/categories/{createdCategory.Id}", updateRequest);
var updatedCategory = await updateResponse.Content.ReadFromJsonAsync<CategoryDto>();
Console.WriteLine($"Updated category: {updatedCategory.Name}");
```

## ModerationController

The `ModerationController` class provides RESTful API endpoints for managing moderation tasks including report review and action enforcement. It handles the complete moderation workflow from report creation through review and resolution. The controller integrates with the `ModerationService` to process reports and enforce actions against listings or users, while managing cache invalidation to ensure data consistency after moderation actions.

### Usage Example

```csharp
using MarketplaceEngine.Controllers;
using MarketplaceEngine.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// Initialize HTTP client for API calls
var client = new HttpClient { BaseAddress = new Uri("https://api.marketplace.example.com") };

// Create a moderation report for inappropriate content
var reportRequest = new CreateReportRequest
{
    TargetListingId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), // Listing to report
    TargetUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), // Optional: user to report
    ReporterId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), // User reporting the content
    Reason = "This listing contains inappropriate content and violates our terms of service"
};

var createReportResponse = await client.PostAsJsonAsync("/api/v1/moderation/reports", reportRequest);
var createdReport = await createReportResponse.Content.ReadFromJsonAsync<ModerationReportDto>();
Console.WriteLine($"Report created: {createdReport.Id} - Status: {createdReport.Status}");

// Get pending reports for moderator review (not cached - real-time updates)
var pendingReportsResponse = await client.GetAsync("/api/v1/moderation/reports?page=1&pageSize=20");
var pendingReports = await pendingReportsResponse.Content.ReadFromJsonAsync<PaginatedResponse<ModerationReportDto>>();
Console.WriteLine($"Found {pendingReports.Total} pending reports");

// Get detailed report information including related entities
var reportDetailsResponse = await client.GetAsync($"/api/v1/moderation/reports/{createdReport.Id}");
var reportDetails = await reportDetailsResponse.Content.ReadFromJsonAsync<ModerationReportDetailsDto>();
Console.WriteLine($"Report details: {reportDetails.Reason}");
Console.WriteLine($"Target listing: {reportDetails.TargetListingId}");
Console.WriteLine($"Target user: {reportDetails.TargetUserId}");

// Approve a report and take moderation action
var approveRequest = new ApproveReportRequest
{
    ActionNotes = "Listing removed due to violation of content policy - inappropriate images detected"
};

var approveResponse = await client.PostAsJsonAsync($"/api/v1/moderation/reports/{createdReport.Id}/approve", approveRequest);
var approveResult = await approveResponse.Content.ReadFromJsonAsync<object>();
Console.WriteLine($"Report approved: {approveResult}");

// Get moderation statistics for dashboard
var statsResponse = await client.GetAsync("/api/v1/moderation/statistics");
var statistics = await statsResponse.Content.ReadFromJsonAsync<ModerationStatsDto>();
Console.WriteLine($"Moderation Statistics:");
Console.WriteLine($"- Pending reports: {statistics.PendingReports}");
Console.WriteLine($"- Approved reports: {statistics.ApprovedReports}");
Console.WriteLine($"- Rejected reports: {statistics.RejectedReports}");
Console.WriteLine($"- Average resolution time: {statistics.AverageResolutionTime} hours");

// Bulk moderation action (moderator/admin only)
var bulkRequest = new BulkModerationRequest
{
    ListingIds = new List<Guid> {
        Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
        Guid.Parse("4fa85f64-5717-4562-b3fc-2c963f66afa7")
    },
    Action = "remove" // Action can be: approve, remove, escalate
};

// Note: In real usage, include X-User-Role header: "Moderator" or "Administrator"
var bulkResponse = await client.PostAsJsonAsync("/api/v1/moderation/bulk", bulkRequest);
var bulkResult = await bulkResponse.Content.ReadFromJsonAsync<BulkModerationResponse>();
Console.WriteLine($"Bulk action completed. Success: {bulkResult.Results.Count(r => r.Success)}/{bulkResult.Results.Count}");
```

## HealthController

The `HealthController` class provides health monitoring endpoints for the Marketplace Engine application. It implements both basic liveness checks for container orchestration systems and detailed readiness checks that validate critical dependencies including database connectivity and cache availability. The controller returns comprehensive status information including service version, uptime metrics, and dependency health.

### Usage Example

```csharp
using MarketplaceEngine.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// Initialize test client for API calls
var client = new HttpClient { BaseAddress = new Uri("https://api.marketplace.example.com") };

// Basic health check (liveness probe)
var healthResponse = await client.GetAsync("/api/v1/health");
var healthData = await healthResponse.Content.ReadFromJsonAsync<HealthController.HealthResponse>();

Console.WriteLine($"Health Status: {healthData.Status}");
Console.WriteLine($"Version: {healthData.Version}");
Console.WriteLine($"Uptime: {healthData.Uptime}");
Console.WriteLine($"Timestamp: {healthData.Timestamp:yyyy-MM-dd HH:mm:ss}");

// Detailed readiness check with dependency validation
var readinessResponse = await client.GetAsync("/api/v1/health/ready");
var readinessData = await readinessResponse.Content.ReadFromJsonAsync<HealthController.DetailedHealthResponse>();

Console.WriteLine($"\nReadiness Status: {readinessData.Status}");
Console.WriteLine($"Dependencies:");
foreach (var dependency in readinessData.Dependencies)
{
    Console.WriteLine($"  - {dependency.Key}: {dependency.Value}");
}

// Simple liveness probe for container orchestration
var livenessResponse = await client.GetAsync("/api/v1/health/live");
Console.WriteLine($"\nLiveness check status: {(int)livenessResponse.StatusCode} - {(livenessResponse.IsSuccessStatusCode ? "Service available" : "Service unavailable")}");
```

## CategoriesController

The `CategoriesController` class provides RESTful API endpoints for managing product categories in the marketplace. It handles CRUD operations for categories, category hierarchy management, and retrieval of category statistics. The controller supports retrieving individual categories, category trees, listing counts, and category-specific statistics including average prices and view metrics.

### Usage Example

```csharp
using MarketplaceEngine.Controllers;
using MarketplaceEngine.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// Initialize HTTP client for API calls
var client = new HttpClient { BaseAddress = new Uri("https://api.marketplace.example.com") };

// Get all categories with hierarchy
var categoriesResponse = await client.GetAsync("/api/v1/categories/tree");
var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<CategoryDto>>();
Console.WriteLine($"Found {categories.Count} root categories");

// Get a specific category by ID
var categoryId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
var categoryResponse = await client.GetAsync($"/api/v1/categories/{categoryId}");
var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryDto>();
Console.WriteLine($"Retrieved category: {category.Name} ({category.Description})");
Console.WriteLine($"Listing count: {category.ListingCount}");
Console.WriteLine($"Subcategories: {category.SubCategories?.Count ?? 0}");

// Get category statistics
var statsResponse = await client.GetAsync($"/api/v1/categories/{categoryId}/statistics");
var statistics = await statsResponse.Content.ReadFromJsonAsync<CategoryStatisticsDto>();
Console.WriteLine($"Category statistics:");
Console.WriteLine($"- Total listings: {statistics.TotalListings}");
Console.WriteLine($"- Average price: {statistics.AveragePrice:C}");
Console.WriteLine($"- Total views: {statistics.TotalViews}");
Console.WriteLine($"- Average views: {statistics.AverageViews:F1}");

// Get category listings
var listingsResponse = await client.GetAsync($"/api/v1/categories/{categoryId}/listings");
var listings = await listingsResponse.Content.ReadFromJsonAsync<List<ListingDto>>();
Console.WriteLine($"Found {listings.Count} listings in category");

// Create a new category (admin only)
var newCategory = new CreateCategoryRequest
{
    Name = "Smart Home Devices",
    Description = "Smart home automation devices and accessories",
    ParentCategoryId = categoryId
};

var createResponse = await client.PostAsJsonAsync("/api/v1/categories", newCategory);
var createdCategory = await createResponse.Content.ReadFromJsonAsync<CategoryDto>();
Console.WriteLine($"Created category: {createdCategory.Id} - {createdCategory.Name}");

// Update an existing category (admin only)
var updateRequest = new UpdateCategoryRequest
{
    Name = "Smart Home & IoT Devices",
    Description = "Smart home devices, IoT gadgets, and home automation equipment"
};

var updateResponse = await client.PutAsJsonAsync($"/api/v1/categories/{createdCategory.Id}", updateRequest);
var updatedCategory = await updateResponse.Content.ReadFromJsonAsync<CategoryDto>();
Console.WriteLine($"Updated category: {updatedCategory.Name}");
```

## UsersController

The `UsersController` class provides RESTful API endpoints for managing user profiles, authentication, and seller reputation. It handles user profile retrieval, seller metrics, top seller rankings, profile updates, and email verification. The controller integrates with the `UserService` for business logic and uses caching to improve performance for infrequently changing user data.

### Usage Example

```csharp
using MarketplaceEngine.Controllers;
using MarketplaceEngine.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// Initialize HTTP client for API calls
var client = new HttpClient { BaseAddress = new Uri("https://api.marketplace.example.com") };

// Get user profile (cached for 15 minutes)
var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
var profileResponse = await client.GetAsync($"/api/v1/users/{userId}");
var profile = await profileResponse.Content.ReadFromJsonAsync<UserDto>();
Console.WriteLine($"User Profile: {profile.DisplayName}");
Console.WriteLine($"Email: {profile.Email}");
Console.WriteLine($"Rating: {profile.AverageRating:F1}/5 ({profile.TotalReviews} reviews)");
Console.WriteLine($"Member since: {profile.CreatedAt:yyyy-MM-dd}");

// Get seller metrics (cached for 10 minutes)
var metricsResponse = await client.GetAsync($"/api/v1/users/{userId}/seller-metrics");
var metrics = await metricsResponse.Content.ReadFromJsonAsync<SellerMetricsDto>();
Console.WriteLine($"\nSeller Metrics:");
Console.WriteLine($"- Average Rating: {metrics.AverageRating:F1}/5");
Console.WriteLine($"- Total Reviews: {metrics.TotalReviews}");
Console.WriteLine($"- Total Sales: {metrics.TotalSales}");
Console.WriteLine($"- Response Time: {metrics.ResponseTime}");

// Get top sellers (cached for 30 minutes)
var topSellersResponse = await client.GetAsync("/api/v1/users/top-sellers?limit=10");
var topSellers = await topSellersResponse.Content.ReadFromJsonAsync<List<SellerRankingDto>>();
Console.WriteLine($"\nTop {topSellers.Count} Sellers:");
foreach (var seller in topSellers.Take(5))
{
    Console.WriteLine($"- Rank #{seller.Rank}: {seller.DisplayName} ({seller.AverageRating:F1}/5)");
}

// Update user profile
var updateRequest = new UpdateUserRequest
{
    DisplayName = "TechEnthusiast Pro",
    Bio = "Technology enthusiast and gadget collector"
};

var updateResponse = await client.PutAsJsonAsync($"/api/v1/users/{userId}", updateRequest);
var updatedProfile = await updateResponse.Content.ReadFromJsonAsync<UserDto>();
Console.WriteLine($"\nProfile updated: {updatedProfile.DisplayName}");

// Verify user email
var verifyResponse = await client.PostAsync($"/api/v1/users/{userId}/verify-email", null);
Console.WriteLine("Email verification initiated");
```

## ReviewsController

The `ReviewsController` class provides RESTful API endpoints for managing buyer reviews, seller replies, review retrieval, moderation actions, and aggregated rating statistics. It handles review submissions, retrieval of seller and listing reviews, seller reply functionality, review flagging for moderation, and review removal by moderators. The controller integrates with the `ReviewService` to provide comprehensive review management capabilities.

### Usage Example

```csharp
using MarketplaceEngine.Controllers;
using MarketplaceEngine.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// Initialize HTTP client for API calls
var client = new HttpClient { BaseAddress = new Uri("https://api.marketplace.example.com") };

// Submit a new review for a seller
var newReview = new CreateReviewRequest
{
    ReviewerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
    SellerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
    ListingId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
    Score = 5,
    Comment = "Excellent seller! Fast shipping and great communication."
};

var createResponse = await client.PostAsJsonAsync("/api/v1/reviews", newReview);
var createdReview = await createResponse.Content.ReadFromJsonAsync<ReviewDto>();
Console.WriteLine($"Review created: {createdReview.Id} - Score: {createdReview.Score}/5");

// Get a specific review by ID
var reviewResponse = await client.GetAsync($"/api/v1/reviews/{createdReview.Id}");
var review = await reviewResponse.Content.ReadFromJsonAsync<ReviewDto>();
Console.WriteLine($"Retrieved review: {review.Comment}");

// Get all reviews for a seller (paginated)
var sellerReviewsResponse = await client.GetAsync($"/api/v1/reviews/seller/{Guid.Parse("22222222-2222-2222-2222-222222222222")}?page=1&pageSize=20");
var sellerReviews = await sellerReviewsResponse.Content.ReadFromJsonAsync<PaginatedResponse<ReviewDto>>();
Console.WriteLine($"Seller has {sellerReviews.Total} total reviews, showing page {sellerReviews.Page}");

// Get all reviews for a specific listing
var listingReviewsResponse = await client.GetAsync($"/api/v1/reviews/listing/{Guid.Parse("33333333-3333-3333-3333-333333333333")}");
var listingReviews = await listingReviewsResponse.Content.ReadFromJsonAsync<List<ReviewDto>>();
Console.WriteLine($"Listing has {listingReviews.Count} reviews");

// Get seller rating summary
var summaryResponse = await client.GetAsync($"/api/v1/reviews/seller/{Guid.Parse("22222222-2222-2222-2222-222222222222")}/summary");
var summary = await summaryResponse.Content.ReadFromJsonAsync<ReviewSummaryDto>();
Console.WriteLine($"Seller average score: {summary.AverageScore:F1}/5 stars");
Console.WriteLine($"Total reviews: {summary.TotalReviews}");

// Add a seller reply to a review
var replyRequest = new SellerReplyRequest
{
    SellerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
    Reply = "Thank you for your positive review! We appreciate your business."
};

var replyResponse = await client.PostAsJsonAsync($"/api/v1/reviews/{createdReview.Id}/reply", replyRequest);
var reviewWithReply = await replyResponse.Content.ReadFromJsonAsync<ReviewDto>();
Console.WriteLine($"Seller reply added: {reviewWithReply.SellerReply}");

// Flag a review for moderator review
var flagResponse = await client.PostAsync($"/api/v1/reviews/{createdReview.Id}/flag", null);
var flaggedReview = await flagResponse.Content.ReadFromJsonAsync<ReviewDto>();
Console.WriteLine($"Review flagged for moderation");

// Remove a review (requires moderator role)
var removeResponse = await client.DeleteAsync($"/api/v1/reviews/{createdReview.Id}?moderatorId={Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")}");
var removedReview = await removeResponse.Content.ReadFromJsonAsync<ReviewDto>();
Console.WriteLine($"Review removed by moderator");
```

## CategoryService

The `CategoryService` class provides comprehensive category management functionality for the Marketplace Engine. It handles category hierarchy operations, including creating, updating, and organizing categories into parent-child relationships. The service supports retrieving categories by ID, searching categories, managing category trees for navigation, and retrieving popular categories based on listing activity.

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

// Initialize category service
var categoryService = new CategoryService();

// Get all active categories
var allCategories = await categoryService.GetAllCategoriesAsync();
Console.WriteLine($"Found {allCategories.Count} active categories");

// Get category by ID
var category = await categoryService.GetCategoryAsync(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));
Console.WriteLine($"Retrieved category: {category.Name}");

// Get root categories (categories without parent)
var rootCategories = await categoryService.GetRootCategoriesAsync();
Console.WriteLine($"Found {rootCategories.Count} root categories");

// Get subcategories for a parent category
var electronicsCategoryId = rootCategories.First().Id;
var subCategories = await categoryService.GetSubCategoriesAsync(electronicsCategoryId);
Console.WriteLine($"Found {subCategories.Count} subcategories under {rootCategories.First().Name}");

// Get category with full hierarchy (parent + children)
var categoryWithHierarchy = await categoryService.GetCategoryHierarchyAsync(electronicsCategoryId);
Console.WriteLine($"Category hierarchy retrieved: {categoryWithHierarchy.Name} with {categoryWithHierarchy.SubCategories.Count} subcategories");

// Create a new category
var newCategory = await categoryService.CreateCategoryAsync(
    name: "Smart Home Devices",
    description: "Smart home automation devices and accessories",
    parentCategoryId: electronicsCategoryId
);
Console.WriteLine($"Created new category: {newCategory.Name}");

// Update category information
var updatedCategory = await categoryService.UpdateCategoryAsync(
    categoryId: newCategory.Id,
    name: "Smart Home & IoT Devices",
    description: "Smart home devices, IoT gadgets, and home automation equipment"
);
Console.WriteLine($"Updated category: {updatedCategory.Name}");

// Deactivate a category (soft delete)
var deactivatedCategory = await categoryService.DeactivateCategoryAsync(newCategory.Id);
Console.WriteLine($"Category deactivated: {deactivatedCategory.IsActive}");

// Get category tree structure (root categories with nested subcategories)
var categoryTree = await categoryService.GetCategoryTreeAsync();
Console.WriteLine($"Category tree contains {categoryTree.Count} root categories");
foreach (var root in categoryTree)
{
    Console.WriteLine($"- {root.Name} ({root.SubCategories.Count} subcategories, {root.ListingCount} listings)");
}

// Get category tree with specific depth
var deepCategoryTree = await categoryService.GetCategoryTreeAsync(depth: 3);
Console.WriteLine($"Deep category tree retrieved with depth 3");

// Search categories by name
var searchResults = await categoryService.SearchCategoriesAsync("electronics");
Console.WriteLine($"Search for 'electronics' found {searchResults.Count} categories");

// Get category by slug
var categoryBySlug = await categoryService.GetBySlugAsync("electronics");
Console.WriteLine($"Retrieved category by slug 'electronics': {categoryBySlug.Name}");

// Get top hot categories by listing count
var hotCategories = await categoryService.GetHotCategoriesAsync(limit: 5);
Console.WriteLine($"Top {hotCategories.Count} hot categories:");
foreach (var hotCat in hotCategories)
{
    Console.WriteLine($"- {hotCat.Name}: {hotCat.ListingCount} listings");
}
```

## WatchlistService

The `WatchlistService` class provides in-memory per-user watchlist functionality for tracking user interest in specific listings. It maintains watchlists that integrate with the recommendation system by recording Save signals when listings are added to watchlists, enabling personalized recommendations based on user preferences. The service supports adding/removing listings from watchlists, checking if a user is watching a listing, retrieving watched listings, and getting watcher counts for notifications.

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

// Initialize watchlist service (typically via dependency injection)
var watchlistService = new WatchlistService(listingRepository, recommendationEngine);

// Add a listing to user's watchlist
var wasAdded = await watchlistService.AddAsync(
    userId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    listingId: Guid.Parse("33333333-3333-3333-3333-333333333333")
);
Console.WriteLine($"Listing added to watchlist: {wasAdded}");

// Check if user is watching a listing
var isWatching = watchlistService.IsWatching(
    userId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    listingId: Guid.Parse("33333333-3333-3333-3333-333333333333")
);
Console.WriteLine($"User is watching listing: {isWatching}");

// Remove a listing from watchlist
var wasRemoved = await watchlistService.RemoveAsync(
    userId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    listingId: Guid.Parse("33333333-3333-3333-3333-333333333333")
);
Console.WriteLine($"Listing removed from watchlist: {wasRemoved}");

// Get all listings watched by a user
var watchedListings = await watchlistService.GetWatchedListingsAsync(
    userId: Guid.Parse("11111111-1111-1111-1111-111111111111")
);
Console.WriteLine($"User is watching {watchedListings.Count} listings");
foreach (var listing in watchedListings.Take(5))
{
    Console.WriteLine($"- {listing.Title}: {listing.Price:C}");
}

// Get how many users are watching a specific listing
var watcherCount = watchlistService.GetWatcherCount(
    listingId: Guid.Parse("33333333-3333-3333-3333-333333333333")
);
Console.WriteLine($"Listing is watched by {watcherCount} users");

// Get all user IDs watching a specific listing (for notifications)
var watchers = watchlistService.GetWatchers(
    listingId: Guid.Parse("33333333-3333-3333-3333-333333333333")
);
Console.WriteLine($"Listing watchers: {string.Join(", ", watchers.Take(10))}");

// Add multiple listings to watchlist
var listingIds = new[] {
    Guid.Parse("33333333-3333-3333-3333-333333333333"),
    Guid.Parse("44444444-4444-4444-4444-444444444444"),
    Guid.Parse("55555555-5555-5555-5555-555555555555")
};
foreach (var id in listingIds)
{
    await watchlistService.AddAsync(
        userId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
        listingId: id
    );
}
Console.WriteLine("Multiple listings added to watchlist");

// Check if listing exists before adding (to avoid KeyNotFoundException)
var listingExists = await listingRepository.ExistsAsync(
    Guid.Parse("33333333-3333-3333-3333-333333333333")
);
if (listingExists)
{
    await watchlistService.AddAsync(
        userId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
        listingId: Guid.Parse("33333333-3333-3333-3333-333333333333")
    );
}
```

## UserDto

The `UserDto` class represents a user in the marketplace system. It contains essential user information including identification, profile details, seller metrics, and account status. This DTO is used throughout the application for user profiles, seller dashboards, and API responses.

### Usage Example

```csharp
using MarketplaceEngine.DTOs;
using System;

// Create a user DTO for a new seller
var userDto = new UserDto
{
    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
    Email = "seller@example.com",
    DisplayName = "TechSeller Pro",
    Bio = "Premium electronics seller with 5+ years of experience",
    Role = "Seller",
    AverageRating = 4.8,
    ReviewCount = 125,
    EmailVerified = true,
    CreatedAt = DateTime.UtcNow.AddDays(-90),
    UpdatedAt = DateTime.UtcNow
};

// Display user information
Console.WriteLine($"User: {userDto.DisplayName}");
Console.WriteLine($"Email: {userDto.Email}");
Console.WriteLine($"Role: {userDto.Role}");
Console.WriteLine($"Rating: {userDto.AverageRating:F1}/5 ({userDto.ReviewCount} reviews)");
Console.WriteLine($"Email verified: {(userDto.EmailVerified ? "Yes" : "No")}");
Console.WriteLine($"Member since: {userDto.CreatedAt:yyyy-MM-dd}");
Console.WriteLine($"Last updated: {userDto.UpdatedAt?.ToString("yyyy-MM-dd") ?? "Never"}");

// Create a seller profile with additional metrics
var sellerProfile = new UserDto
{
    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
    DisplayName = "Premium Goods Store",
    Bio = "Specializing in high-quality electronics and accessories",
    Role = "Seller",
    AverageRating = 4.9,
    ReviewCount = 243,
    EmailVerified = true,
    CreatedAt = DateTime.UtcNow.AddDays(-180),
    UpdatedAt = DateTime.UtcNow,
    UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
    AverageRating = 4.9,
    TotalReviews = 243,
    TotalSales = 456,
    ResponseTime = "Within 24 hours",
    Rank = 15
};

Console.WriteLine($"\nSeller Profile: {sellerProfile.DisplayName}");
Console.WriteLine($"Performance: {sellerProfile.TotalSales} sales | Rank #{sellerProfile.Rank}");
Console.WriteLine($"Response time: {sellerProfile.ResponseTime}");
```

## PaymentService

The `PaymentService` class handles all payment processing operations within the marketplace, including initiating payments, processing transactions, managing escrow funds, and handling refunds. It provides methods for both buyers and sellers to manage payment states throughout the transaction lifecycle, ensuring secure fund handling and transparent payment tracking.


### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Entities;
using System;
using System.Threading.Tasks;

// Initialize payment service
var paymentService = new PaymentService();

// Initiate a new payment for a listing purchase
var newPayment = await paymentService.InitiatePaymentAsync(
    buyerId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    sellerId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
    listingId: Guid.Parse("33333333-3333-3333-3333-333333333333"),
    amount: 129.99m,
    currency: "USD",
    paymentMethod: "CreditCard"
);

Console.WriteLine($"Payment initiated: {newPayment.Id}");
Console.WriteLine($"Amount: {newPayment.Amount:C} {newPayment.Currency}");
Console.WriteLine($"Status: {newPayment.Status}");

// Start processing the payment
var processingPayment = await paymentService.StartProcessingAsync(newPayment.Id);
Console.WriteLine($"Payment processing started: {processingPayment.Status}");

// Complete the payment
var completedPayment = await paymentService.CompletePaymentAsync(newPayment.Id);
Console.WriteLine($"Payment completed: {completedPayment.Status}");

// Move payment to escrow (funds held until delivery confirmed)
var escrowPayment = await paymentService.MoveToEscrowAsync(newPayment.Id);
Console.WriteLine($"Payment moved to escrow: {escrowPayment.Status}");

// Release funds to seller after successful delivery
var releasedPayment = await paymentService.ReleaseEscrowAsync(newPayment.Id);
Console.WriteLine($"Funds released to seller: {releasedPayment.Status}");

// Get payment details
var paymentDetails = await paymentService.GetPaymentAsync(newPayment.Id);
Console.WriteLine($"\nPayment Details:");
Console.WriteLine($"- Buyer: {paymentDetails.BuyerId}");
Console.WriteLine($"- Seller: {paymentDetails.SellerId}");
Console.WriteLine($"- Amount: {paymentDetails.Amount:C}");
Console.WriteLine($"- Status: {paymentDetails.Status}");

// Get all payments for a buyer
var buyerPayments = await paymentService.GetBuyerPaymentsAsync(
    Guid.Parse("11111111-1111-1111-1111-111111111111")
);
Console.WriteLine($"\nBuyer has {buyerPayments.Count} payments");

// Get all payments for a seller
var sellerPayments = await paymentService.GetSellerPaymentsAsync(
    Guid.Parse("22222222-2222-2222-2222-222222222222")
);
Console.WriteLine($"Seller has {sellerPayments.Count} payments");

// Get seller revenue summary
var sellerRevenue = await paymentService.GetSellerRevenueAsync(
    Guid.Parse("22222222-2222-2222-2222-222222222222")
);
Console.WriteLine($"Seller total revenue: {sellerRevenue:C}");

// Handle payment failure
var failedPayment = await paymentService.FailPaymentAsync(newPayment.Id);
Console.WriteLine($"Payment failed: {failedPayment.Status}");

// Cancel a payment before processing
var cancelledPayment = await paymentService.CancelPaymentAsync(newPayment.Id);
Console.WriteLine($"Payment cancelled: {cancelledPayment.Status}");

// Refund a completed payment
var refundedPayment = await paymentService.RefundPaymentAsync(newPayment.Id);
Console.WriteLine($"Payment refunded: {refundedPayment.Status}");
```

## MessagingService

The `MessagingService` class provides comprehensive messaging functionality for user-to-user communication within the marketplace. It handles sending messages, managing conversations, marking messages as read/unread, flagging inappropriate content, adding replies, and performing administrative tasks like cleaning up old messages. The service supports both direct user messaging and listing-specific conversations.


### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Initialize messaging service
var messagingService = new MessagingService(messageRepository, userRepository);

// Send a message between users
var newMessage = await messagingService.SendMessageAsync(
    senderId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    recipientId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
    subject: "Interested in your listing",
    body: "Hello! I'm interested in your premium wireless headphones. Could you provide more details about the condition and shipping options?",
    listingId: Guid.Parse("33333333-3333-3333-3333-333333333333")
);

Console.WriteLine($"Message sent: {newMessage.Id}");
Console.WriteLine($"Subject: {newMessage.Subject}");
Console.WriteLine($"Status: {newMessage.Status}");

// Get received messages for a user
var receivedMessages = await messagingService.GetReceivedMessagesAsync(
    Guid.Parse("22222222-2222-2222-2222-222222222222")
);
Console.WriteLine($"\nReceived {receivedMessages.Count} messages");

// Get unread messages
var unreadMessages = await messagingService.GetUnreadMessagesAsync(
    Guid.Parse("22222222-2222-2222-2222-222222222222")
);
Console.WriteLine($"Unread messages: {unreadMessages.Count}");

// Get conversation between two users
var conversation = await messagingService.GetConversationAsync(
    Guid.Parse("11111111-1111-1111-1111-111111111111"),
    Guid.Parse("22222222-2222-2222-2222-222222222222")
);
Console.WriteLine($"\nConversation contains {conversation.Count} messages");

// Mark message as read
var readMessage = await messagingService.MarkAsReadAsync(newMessage.Id);
Console.WriteLine($"Message marked as read: {readMessage.IsRead}");

// Add a reply to a message
var reply = await messagingService.AddReplyAsync(
    parentMessageId: newMessage.Id,
    senderId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
    body: "Thank you for your interest! The headphones are in excellent condition and I offer free shipping."
);
Console.WriteLine($"Reply sent: {reply.Id}");

// Get messages about a specific listing
var listingMessages = await messagingService.GetListingMessagesAsync(
    Guid.Parse("33333333-3333-3333-3333-333333333333")
);
Console.WriteLine($"\nListing has {listingMessages.Count} messages");

// Get paginated messages
var paginatedMessages = await messagingService.GetPaginatedMessagesAsync(
    userId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
    pageNumber: 1,
    pageSize: 25
);
Console.WriteLine($"\nPage 1 contains {paginatedMessages.items.Count} messages (total: {paginatedMessages.total})");

// Get conversation count
var conversationCount = await messagingService.GetConversationCountAsync(
    Guid.Parse("22222222-2222-2222-2222-222222222222")
);
Console.WriteLine($"User has {conversationCount} conversations");

// Flag inappropriate message (admin functionality)
var flaggedMessage = await messagingService.FlagMessageAsync(
    messageId: newMessage.Id,
    flaggerId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
);
Console.WriteLine($"Message flagged: {flaggedMessage.IsFlagged}");

// Delete a message
await messagingService.DeleteMessageAsync(
    messageId: newMessage.Id,
    requesterId: Guid.Parse("11111111-1111-1111-1111-111111111111")
);
Console.WriteLine("Message deleted successfully");
```


## PaymentRepository

The `PaymentRepository` class provides data access operations for payment entities within the marketplace system. It handles CRUD operations for payments, as well as query methods for retrieving payments by buyer, seller, listing, or status, along with support for pagination and total revenue calculation.

### Usage Example

```csharp
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Initialize payment repository
var paymentRepository = new PaymentRepository();

// Add a new payment
var newPayment = await paymentRepository.AddAsync(new Payment
{
    BuyerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
    SellerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
    ListingId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
    Amount = 129.99m,
    Status = "Pending",
    CreatedAt = DateTime.UtcNow
});

Console.WriteLine($"Payment added: {newPayment.Id}");

// Get payment by ID
var payment = await paymentRepository.GetByIdAsync(newPayment.Id);
Console.WriteLine($"Retrieved payment amount: {payment?.Amount:C}");

// Get total revenue for all payments
var totalRevenue = await paymentRepository.GetTotalRevenueAsync();
Console.WriteLine($"Total revenue: {totalRevenue:C}");

// Get payments for a buyer
var buyerPayments = await paymentRepository.GetByBuyerIdAsync(Guid.Parse("11111111-1111-1111-1111-111111111111"));
Console.WriteLine($"Buyer has {buyerPayments.Count} payments");

// Get payments by status
var pendingPayments = await paymentRepository.GetByStatusAsync("Pending");
Console.WriteLine($"Pending payments: {pendingPayments.Count}");

// Get paginated payments
var (payments, totalCount) = await paymentRepository.GetPagedAsync(
    page: 1,
    pageSize: 10
);
Console.WriteLine($"Page 1 contains {payments.Count} of {totalCount} total payments");

// Check if a payment exists
var exists = await paymentRepository.ExistsAsync(newPayment.Id);
Console.WriteLine($"Payment exists: {exists}");

// Update a payment
newPayment.Status = "Completed";
await paymentRepository.UpdateAsync(newPayment);
Console.WriteLine("Payment updated to Completed");

// Get payment count
var paymentCount = await paymentRepository.CountAsync();
Console.WriteLine($"Total payments: {paymentCount}");

// Delete a payment
await paymentRepository.DeleteAsync(newPayment.Id);
Console.WriteLine("Payment deleted");
```


## PricePoint

The `PricePoint` record represents a single price point in the price history tracking system. It stores the price value along with the timestamp when the price was recorded, enabling the system to track historical price changes and calculate statistics about price movements over time. PricePoint is used by the `PriceHistoryTracker` service to maintain a complete history of listing prices.

### Usage Example

```csharp
using MarketplaceEngine.Services;
using System;
using System.Linq;

// Initialize price history tracker for a listing
var tracker = new PriceHistoryTracker(listingId: Guid.Parse("11111111-1111-1111-1111-111111111111"));

// Record initial price
var pricePoint1 = tracker.RecordPrice(199.99m);
Console.WriteLine($"Recorded price: {pricePoint1.Price:C} at {pricePoint1.Timestamp}");

// Record price drop after a week
var pricePoint2 = tracker.RecordPrice(179.99m);
Console.WriteLine($"Recorded price: {pricePoint2.Price:C} at {pricePoint2.Timestamp}");

// Record another price change
var pricePoint3 = tracker.RecordPrice(159.99m);
Console.WriteLine($"Recorded price: {pricePoint3.Price:C} at {pricePoint3.Timestamp}");

// Get complete price history
var history = tracker.GetHistory();
Console.WriteLine($"\nPrice history contains {history.Count} entries:");
foreach (var point in history.OrderBy(p => p.Timestamp))
{
    Console.WriteLine($"- {point.Timestamp:yyyy-MM-dd}: {point.Price:C}");
}

// Get price statistics
var statistics = tracker.GetStatistics();
if (statistics != null)
{
    Console.WriteLine($"\nPrice Statistics:");
    Console.WriteLine($"- Average price: {statistics.AveragePrice:C}");
    Console.WriteLine($"- Highest price: {statistics.HighestPrice:C}");
    Console.WriteLine($"- Lowest price: {statistics.LowestPrice:C}");
    Console.WriteLine($"- Price range: {statistics.HighestPrice - statistics.LowestPrice:C}");
}

// Check for price drops
if (tracker.HasPriceDrop)
{
    Console.WriteLine($"\nPrice has dropped from initial price!");
    Console.WriteLine($"Latest drop percentage: {tracker.GetLatestDropPercent():P1}");
}

// Prune old price points (keep only last 100 entries)
var prunedCount = tracker.Prune(maxEntries: 100);
Console.WriteLine($"\nPruned {prunedCount} old price points, keeping {tracker.GetHistory().Count} most recent");
```

## ErrorHandlingMiddleware

The `ErrorHandlingMiddleware` class provides centralized exception handling for the Marketplace Engine API. It catches all unhandled exceptions, logs them appropriately, and returns consistent error responses to clients without exposing sensitive error details in production. This middleware prevents the need for scattered exception handling logic across controllers and ensures a uniform error format throughout the application.

### Usage Example

```csharp
using MarketplaceEngine.Middleware;
using MarketplaceEngine.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Register the middleware with dependency injection
builder.Services.AddScoped<ErrorHandlingMiddleware>();
builder.Services.AddLogging();

var app = builder.Build();

// Add the error handling middleware to the pipeline
// It should be placed early in the pipeline to catch all exceptions
app.UseMiddleware<ErrorHandlingMiddleware>();

// Other middleware components
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/listings/{id}", (Guid id) => 
{
    if (id == Guid.Empty)
    {
        throw new ResourceNotFoundException("Listing not found");
    }
    
    return Results.Ok(new { Id = id, Name = "Sample Listing" });
});

app.Run();
```

### Error Response Format

When an exception is caught, the middleware returns a standardized error response with the following structure:

```json
{
  "code": "ERROR_TYPE",
  "message": "Human-readable error message",
  "details": {
    "field1": "error details",
    "field2": "additional context"
  },
  "timestamp": "2024-07-18T12:34:56.789Z"
}
```

### Supported Exception Types

- `ResourceNotFoundException` → HTTP 404 with code "RESOURCE_NOT_FOUND"
- `UnauthorizedException` → HTTP 401 with code "UNAUTHORIZED"  
- `ValidationException` → HTTP 400 with code "VALIDATION_ERROR" (includes validation details)
- `DuplicateResourceException` → HTTP 409 with code "DUPLICATE_RESOURCE"
- `MarketplaceException` → HTTP 400 with code "MARKETPLACE_ERROR"
- All other exceptions → HTTP 500 with generic message (no stack traces exposed)


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


## ListingRepository

The `ListingRepository` class provides comprehensive data access operations for managing listing entities within the marketplace system. It handles core CRUD operations, advanced querying for filtering listings by criteria such as seller, category, status, or location, and utility methods for tracking activity like view and interest counts.

### Usage Example

```csharp
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Initialize listing repository
var listingRepository = new ListingRepository();

// Add a new listing
var newListing = await listingRepository.AddAsync(new Listing
{
    Title = "Premium Wireless Headphones",
    Description = "Noise-cancelling wireless headphones.",
    Price = 299.99m,
    SellerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
    CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
    Status = "Active",
    CreatedAt = DateTime.UtcNow
});

Console.WriteLine($"Listing added: {newListing.Id}");

// Get a listing by ID
var listing = await listingRepository.GetByIdAsync(newListing.Id);
Console.WriteLine($"Retrieved listing: {listing?.Title}");

// Update a listing
newListing.Price = 249.99m;
await listingRepository.UpdateAsync(newListing);
Console.WriteLine("Listing price updated.");

// Get listings by category
var electronics = await listingRepository.GetByCategoryIdAsync(Guid.Parse("22222222-2222-2222-2222-222222222222"));
Console.WriteLine($"Found {electronics.Count} electronics listings.");

// Increment view count
await listingRepository.IncrementViewCountAsync(newListing.Id);
Console.WriteLine("View count incremented.");

// Get paginated listings
var (items, total) = await listingRepository.GetPagedAsync(page: 1, pageSize: 10);
Console.WriteLine($"Retrieved page 1 of {total} total listings.");

// Check if listing exists
var exists = await listingRepository.ExistsAsync(newListing.Id);
Console.WriteLine($"Listing exists: {exists}");

// Delete a listing
await listingRepository.DeleteAsync(newListing.Id);
Console.WriteLine("Listing deleted.");

## EnumUtility

The `EnumUtility` class provides static utility methods for working with enumeration types in the Marketplace Engine. It includes functionality for getting enum descriptions, safe parsing, value/name extraction, dictionary conversion, and flag checking, enabling consistent enum handling across the application.

### Usage Example

```csharp
using MarketplaceEngine.Utilities;
using System;
using System.Linq;

// Define an enum with Description attributes
public enum ListingStatus
{
    [Description("Active listing available for purchase")]
    Active = 1,
    
    [Description("Listing temporarily unavailable")]
    Inactive = 2,
    
    [Description("Listing sold and completed")]
    Sold = 3,
    
    [Description("Listing removed by seller")]
    Removed = 4
}

// Get description from enum value
var activeDescription = EnumUtility.GetDescription(ListingStatus.Active);
Console.WriteLine($"Active status: {activeDescription}");

// Try parse enum from string (case-insensitive)
var parsedStatus = EnumUtility.TryParseEnum<ListingStatus>("inactive");
Console.WriteLine($"Parsed status: {parsedStatus}");

// Get all enum values
var allStatuses = EnumUtility.GetEnumValues<ListingStatus>();
Console.WriteLine($"Total statuses: {allStatuses.Count}");
foreach (var status in allStatuses)
{
    Console.WriteLine($"- {status} ({EnumUtility.GetDescription(status)})");
}

// Get all enum names
var statusNames = EnumUtility.GetEnumNames<ListingStatus>();
Console.WriteLine($"Status names: {string.Join(", ", statusNames)}");

// Convert enum to dictionary
var statusDictionary = EnumUtility.GetEnumDictionary<ListingStatus>();
Console.WriteLine("Status dictionary:");
foreach (var kvp in statusDictionary)
{
    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
}

// Define a flags enum
[Flags]
public enum UserPermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    Admin = 8
}

// Check if enum has specific flag
var userPermissions = UserPermissions.Read | UserPermissions.Write;
var hasWritePermission = EnumUtility.HasFlag(userPermissions, UserPermissions.Write);
Console.WriteLine($"User has write permission: {hasWritePermission}");
```

## StringUtility

The `StringUtility` class provides a collection of static helper methods for common string manipulation tasks throughout the Marketplace Engine. It includes utilities for text formatting, truncation, case conversion, slug generation, repetition, masking sensitive data, and removing special characters, making it easier to maintain consistent string handling across the application.

### Usage Example

```csharp
using MarketplaceEngine.Utilities;
using System;

// Truncate a long string to a maximum length
var longText = "This is a very long product description that needs to be shortened for display purposes";
var truncated = StringUtility.Truncate(longText, 50);
Console.WriteLine($"Truncated: {truncated}...");

// Convert a string to title case (e.g., "product listing" -> "Product Listing")
var titleCase = StringUtility.ToTitleCase("product listing");
Console.WriteLine($"Title case: {titleCase}");

// Generate a URL-friendly slug from a title
var slug = StringUtility.ToSlug("My Awesome Product Listing");
Console.WriteLine($"Slug: {slug}");

// Repeat a string multiple times
var repeated = StringUtility.Repeat("abc", 3);
Console.WriteLine($"Repeated: {repeated}");

// Check if a string contains any of multiple substrings
var containsAny = StringUtility.ContainsAny("Hello World", new[] {"hello", "world", "test"});
Console.WriteLine($"Contains any: {containsAny}");

// Mask an email address (shows first 3 chars + domain)
var maskedEmail = StringUtility.MaskEmail("user@example.com");
Console.WriteLine($"Masked email: {maskedEmail}");

// Mask a phone number (shows last 4 digits)
var maskedPhone = StringUtility.MaskPhoneNumber("+1 (555) 123-4567");
Console.WriteLine($"Masked phone: {maskedPhone}");

// Remove special characters from a string
var cleanText = StringUtility.RemoveSpecialCharacters("Product #1 - 2024!");
Console.WriteLine($"Clean text: {cleanText}");

// Generate a random string of specified length
var randomString = StringUtility.GenerateRandomString(10);
Console.WriteLine($"Random string: {randomString}");
```

## MappingUtility

The `MappingUtility` class provides a centralized mechanism for converting between domain model objects and their corresponding Data Transfer Objects (DTOs). This separation ensures that internal domain logic is decoupled from API representation, facilitating easier maintenance and API evolution.

### Usage Example

```csharp
using MarketplaceEngine.Utilities;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.DTOs;
using System;
using System.Collections.Generic;

// Mapping a single domain model to a DTO
var listing = new Listing { Id = Guid.NewGuid(), Title = "Sample Item" };
var listingDto = MappingUtility.ToDto(listing);
Console.WriteLine($"Mapped listing: {listingDto.Title}");

// Mapping a list of domain models to DTOs
var users = new List<User> { new User { Id = Guid.NewGuid(), Email = "test@example.com" } };
var userDtos = MappingUtility.ToUserDtos(users);
Console.WriteLine($"Mapped {userDtos.Count} users");

// Similarly for Messages and ModerationReports
var message = new Message { Id = Guid.NewGuid(), Body = "Hello" };
var messageDto = MappingUtility.ToDto(message);

var report = new ModerationReport { Id = Guid.NewGuid(), Reason = "Spam" };
var reportDto = MappingUtility.ToDto(report);
```
```

## DateTimeUtility

The `DateTimeUtility` class provides a comprehensive set of static methods for working with dates and times throughout the Marketplace Engine. It includes utilities for getting current UTC time, converting between time zones, calculating elapsed time, checking if dates fall within specific time windows, and determining day/week/month boundaries. These utilities help maintain consistent date/time handling across the application.

### Usage Example

```csharp
using MarketplaceEngine.Utilities;
using System;

// Get current UTC time
var currentUtcTime = DateTimeUtility.GetCurrentUtcTime();
Console.WriteLine($"Current UTC time: {currentUtcTime:yyyy-MM-dd HH:mm:ss}");

// Convert local time to UTC
var localTime = DateTime.Now;
var utcTime = DateTimeUtility.ToUtc(localTime);
Console.WriteLine($"Local time {localTime} converted to UTC: {utcTime}");

// Get elapsed time since a specific date
var pastDate = DateTime.UtcNow.AddDays(-7);
var elapsedTime = DateTimeUtility.GetElapsedTime(pastDate);
Console.WriteLine($"Elapsed time: {elapsedTime.TotalDays:F1} days");

// Get elapsed time as formatted string
var elapsedString = DateTimeUtility.GetElapsedTimeString(pastDate);
Console.WriteLine($"Elapsed time string: {elapsedString}");

// Check if a date is within the last 30 days
var recentDate = DateTime.UtcNow.AddDays(-15);
var isWithin30Days = DateTimeUtility.IsWithinDays(recentDate, 30);
Console.WriteLine($"Is within 30 days: {isWithin30Days}");

// Check if a date is within the last 2 hours
var recentHour = DateTime.UtcNow.AddMinutes(-90);
var isWithin2Hours = DateTimeUtility.IsWithinHours(recentHour, 2);
Console.WriteLine($"Is within 2 hours: {isWithin2Hours}");

// Get start and end of current day
var dayStart = DateTimeUtility.GetDayStart();
var dayEnd = DateTimeUtility.GetDayEnd();
Console.WriteLine($"Day starts at: {dayStart:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"Day ends at: {dayEnd:yyyy-MM-dd HH:mm:ss}");

// Get start of current week (Monday)
var weekStart = DateTimeUtility.GetWeekStart();
Console.WriteLine($"Week starts on: {weekStart:yyyy-MM-dd} (Monday)");

// Get start of current month
var monthStart = DateTimeUtility.GetMonthStart();
Console.WriteLine($"Month starts on: {monthStart:yyyy-MM-dd}");

// Check if two dates are on the same day
var date1 = DateTime.UtcNow;
var date2 = DateTime.UtcNow.AddHours(2);
var isSameDay = DateTimeUtility.IsSameDay(date1, date2);
Console.WriteLine($"Same day: {isSameDay}");

// Convert DateTime to ISO 8601 string
var isoString = DateTimeUtility.ToIso8601String(DateTime.UtcNow);
Console.WriteLine($"ISO 8601 string: {isoString}");

// Get future time (30 minutes from now)
var futureTime = DateTimeUtility.GetFutureTime(TimeSpan.FromMinutes(30));
Console.WriteLine($"Future time: {futureTime:yyyy-MM-dd HH:mm:ss}");
```

## PaginationUtility

The `PaginationUtility` class provides standardized pagination calculation utilities used throughout the Marketplace Engine. It handles offset calculation, page parameter validation, total page calculation, and navigation between pages. This utility ensures consistent pagination behavior across all API endpoints and repository methods.

### Usage Example

```csharp
using MarketplaceEngine.Utilities;
using System;

// Calculate offset for database queries
var offset = PaginationUtility.CalculateOffset(page: 2, pageSize: 25);
Console.WriteLine($"Database offset for page 2: {offset}");

// Validate and normalize page parameters
int page = 0; // Invalid page
int pageSize = 200; // Too large
PaginationUtility.ValidatePageParameters(ref page, ref pageSize);
Console.WriteLine($"Normalized page: {page}, pageSize: {pageSize}");

// Calculate total pages for pagination metadata
var totalItems = 125;
var totalPages = PaginationUtility.CalculateTotalPages(totalItems, pageSize: 25);
Console.WriteLine($"Total pages for {totalItems} items with 25 per page: {totalPages}");

// Check if navigation links should be shown
var currentPage = 3;
var hasNext = PaginationUtility.HasNextPage(currentPage, pageSize: 25, totalItems: 125);
var hasPrevious = PaginationUtility.HasPreviousPage(currentPage);
Console.WriteLine($"Has next page: {hasNext}, has previous page: {hasPrevious}");

// Get next/previous page numbers
var nextPage = PaginationUtility.GetNextPage(currentPage, pageSize: 25, totalItems: 125);
var prevPage = PaginationUtility.GetPreviousPage(currentPage);
Console.WriteLine($"Next page: {nextPage}, previous page: {prevPage}");

// Get default and maximum page sizes
var defaultSize = PaginationUtility.GetDefaultPageSize();
var maxSize = PaginationUtility.GetMaxPageSize();
Console.WriteLine($"Default page size: {defaultSize}, maximum page size: {maxSize}");

// Use with PaginationInfo for complete pagination metadata
var paginationInfo = new PaginationInfo
{
    CurrentPage = 2,
    PageSize = 25,
    TotalItems = 125
};

Console.WriteLine($"Page {paginationInfo.CurrentPage} of {paginationInfo.TotalPages}");
Console.WriteLine($"Items: {paginationInfo.TotalItems}, Has next: {paginationInfo.HasNextPage}, Has previous: {paginationInfo.HasPreviousPage}");
```

## ValidationUtility

The `ValidationUtility` class provides comprehensive validation and sanitization utilities for the Marketplace Engine. It includes methods for validating email addresses, phone numbers, text content, URLs, prices, ratings, GUIDs, pagination parameters, and search queries. The utility also provides input sanitization to prevent XSS and injection attacks, ensuring data integrity and security throughout the application.


### Usage Example

```csharp
using MarketplaceEngine.Utilities;
using System;

// Validate an email address
var isValidEmail = ValidationUtility.IsValidEmail("user@example.com");
Console.WriteLine($"Email is valid: {isValidEmail}");

// Validate a phone number (supports various formats)
var isValidPhone = ValidationUtility.IsValidPhoneNumber("+1 (555) 123-4567");
Console.WriteLine($"Phone number is valid: {isValidPhone}");

// Validate text content (length and character restrictions)
var isValidText = ValidationUtility.IsValidText("This is a valid product description", maxLength: 500);
Console.WriteLine($"Text is valid: {isValidText}");

// Validate a URL
var isValidUrl = ValidationUtility.IsValidUrl("https://example.com/product/123");
Console.WriteLine($"URL is valid: {isValidUrl}");

// Validate a price (positive value)
var isValidPrice = ValidationUtility.IsValidPrice(199.99m);
Console.WriteLine($"Price is valid: {isValidPrice}");

// Validate a rating (between 0 and 5)
var isValidRating = ValidationUtility.IsValidRating(4.5);
Console.WriteLine($"Rating is valid: {isValidRating}");

// Validate a GUID
var isValidGuid = ValidationUtility.IsValidGuid(Guid.NewGuid());
Console.WriteLine($"GUID is valid: {isValidGuid}");

// Validate pagination parameters
var isValidPagination = ValidationUtility.IsValidPagination(page: 1, pageSize: 25);
Console.WriteLine($"Pagination is valid: {isValidPagination}");

// Sanitize user input to prevent XSS and injection
var userInput = "<script>alert('xss')</script><b>Hello</b>";
var sanitizedInput = ValidationUtility.SanitizeInput(userInput);
Console.WriteLine($"Sanitized input: {sanitizedInput}");

// Validate a search query
var isValidSearchQuery = ValidationUtility.IsValidSearchQuery("wireless headphones");
Console.WriteLine($"Search query is valid: {isValidSearchQuery}");

// Complete validation example for user registration
var email = "user@example.com";
var phone = "+1 (555) 123-4567";
var description = "Premium product description";
var website = "https://example.com";
var price = 299.99m;
var rating = 4.5;
var userId = Guid.NewGuid();
var page = 1;
var pageSize = 25;
var searchTerm = "laptop";

if (ValidationUtility.IsValidEmail(email) &&
    ValidationUtility.IsValidPhoneNumber(phone) &&
    ValidationUtility.IsValidText(description, maxLength: 1000) &&
    ValidationUtility.IsValidUrl(website) &&
    ValidationUtility.IsValidPrice(price) &&
    ValidationUtility.IsValidRating(rating) &&
    ValidationUtility.IsValidGuid(userId) &&
    ValidationUtility.IsValidPagination(page, pageSize) &&
    ValidationUtility.IsValidSearchQuery(searchTerm))
{
    Console.WriteLine("All validations passed!");
}
else
{
    Console.WriteLine("One or more validations failed!");
}

## ModerationServiceTests

The `ModerationServiceTests` class provides unit tests for the `ModerationService` class, verifying that it correctly handles various moderation workflows and authorization scenarios. It tests report submission with validation for reporter existence and status, duplicate prevention, report assignment to moderators, report status transitions (InReview, Approved, Rejected), user suspension functionality, and escalation logic for high-priority reports.

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Entities;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

// Initialize mock repositories
var reportRepoMock = new Mock<IModerationReportRepository>();
var userRepoMock = new Mock<IUserRepository>();
var listingRepoMock = new Mock<IListingRepository>();

// Create the service under test
var moderationService = new ModerationService(reportRepoMock.Object, userRepoMock.Object, listingRepoMock.Object);

// Test 1: ReportUserAsync throws ResourceNotFoundException when reporter is not found
var nonExistentReporterId = Guid.NewGuid();
userRepoMock.Setup(r => r.GetByIdAsync(nonExistentReporterId))
.ReturnsAsync((User)null);

var act1 = async () => await moderationService.ReportUserAsync(
reporterId: nonExistentReporterId,
reportedUserId: Guid.NewGuid(),
reason: "User making inappropriate comments"
);

await Assert.ThrowsAsync<ResourceNotFoundException>(act1);

// Test 2: ReportUserAsync throws UnauthorizedException when reporter is inactive
var inactiveReporterId = Guid.NewGuid();
var inactiveReporter = new User { Id = inactiveReporterId, IsActive = false };
userRepoMock.Setup(r => r.GetByIdAsync(inactiveReporterId))
.ReturnsAsync(inactiveReporter);

var act2 = async () => await moderationService.ReportUserAsync(
reporterId: inactiveReporterId,
reportedUserId: Guid.NewGuid(),
reason: "User making inappropriate comments"
);

await Assert.ThrowsAsync<UnauthorizedException>(act2);

// Test 3: ReportUserAsync throws UnauthorizedException when reporter email is not verified
var unverifiedReporterId = Guid.NewGuid();
var unverifiedReporter = new User { Id = unverifiedReporterId, IsActive = true, EmailVerified = false };
userRepoMock.Setup(r => r.GetByIdAsync(unverifiedReporterId))
.ReturnsAsync(unverifiedReporter);

var act3 = async () => await moderationService.ReportUserAsync(
reporterId: unverifiedReporterId,
reportedUserId: Guid.NewGuid(),
reason: "User making inappropriate comments"
);

await Assert.ThrowsAsync<UnauthorizedException>(act3);

// Test 4: ReportUserAsync throws ResourceNotFoundException when target user is not found
var validReporterId = Guid.NewGuid();
var activeReporter = new User { Id = validReporterId, IsActive = true, EmailVerified = true };
userRepoMock.Setup(r => r.GetByIdAsync(validReporterId))
.ReturnsAsync(activeReporter);
userRepoMock.Setup(r => r.GetByIdAsync(Guid.NewGuid()))
.ReturnsAsync((User)null);

var act4 = async () => await moderationService.ReportUserAsync(
reporterId: validReporterId,
reportedUserId: Guid.NewGuid(),
reason: "User making inappropriate comments"
);

await Assert.ThrowsAsync<ResourceNotFoundException>(act4);

// Test 5: ReportUserAsync with valid data returns submitted report
var targetUserId = Guid.NewGuid();
var targetUser = new User { Id = targetUserId, IsActive = true };
userRepoMock.Setup(r => r.GetByIdAsync(targetUserId))
.ReturnsAsync(targetUser);

var report = new ModerationReport { Id = Guid.NewGuid(), Status = ReportStatus.Pending };
reportRepoMock.Setup(r => r.AddAsync(It.IsAny<ModerationReport>()))
.ReturnsAsync(report);

var result = await moderationService.ReportUserAsync(
reporterId: validReporterId,
reportedUserId: targetUserId,
reason: "User making inappropriate comments"
);

Assert.NotNull(result);
Assert.Equal(ReportStatus.Pending, result.Status);

// Test 6: ReportUserAsync with short reason throws ArgumentException
var act6 = async () => await moderationService.ReportUserAsync(
reporterId: validReporterId,
reportedUserId: targetUserId,
reason: "Short"
);

await Assert.ThrowsAsync<ArgumentException>(act6);

// Test 7: ReportListingAsync throws ResourceNotFoundException when listing is not found
var validListingId = Guid.NewGuid();
listingRepoMock.Setup(r => r.GetByIdAsync(validListingId))
.ReturnsAsync((Listing)null);

var act7 = async () => await moderationService.ReportListingAsync(
reporterId: validReporterId,
listingId: validListingId,
reason: "Listing contains inappropriate content"
);

await Assert.ThrowsAsync<ResourceNotFoundException>(act7);

// Test 8: ReportListingAsync with valid data returns submitted report
var validListing = new Listing { Id = validListingId, SellerId = Guid.NewGuid() };
listingRepoMock.Setup(r => r.GetByIdAsync(validListingId))
.ReturnsAsync(validListing);

var listingReport = new ModerationReport { Id = Guid.NewGuid(), Status = ReportStatus.Pending };
reportRepoMock.Setup(r => r.AddAsync(It.IsAny<ModerationReport>()))
.ReturnsAsync(listingReport);

var listingResult = await moderationService.ReportListingAsync(
reporterId: validReporterId,
listingId: validListingId,
reason: "Listing contains inappropriate content"
);

Assert.NotNull(listingResult);
Assert.Equal(ReportStatus.Pending, listingResult.Status);

// Test 9: AssignReportAsync throws ResourceNotFoundException when moderator is not found
var nonExistentModeratorId = Guid.NewGuid();
userRepoMock.Setup(r => r.GetByIdAsync(nonExistentModeratorId))
.ReturnsAsync((User)null);

var reportToAssign = new ModerationReport { Id = Guid.NewGuid(), Status = ReportStatus.Pending };
reportRepoMock.Setup(r => r.GetByIdAsync(reportToAssign.Id))
.ReturnsAsync(reportToAssign);

var act9 = async () => await moderationService.AssignReportAsync(
reportId: reportToAssign.Id,
moderatorId: nonExistentModeratorId
);

await Assert.ThrowsAsync<ResourceNotFoundException>(act9);

// Test 10: AssignReportAsync throws UnauthorizedException when user is regular user
var regularUserId = Guid.NewGuid();
var regularUser = new User { Id = regularUserId, Role = UserRole.User };
userRepoMock.Setup(r => r.GetByIdAsync(regularUserId))
.ReturnsAsync(regularUser);

var act10 = async () => await moderationService.AssignReportAsync(
reportId: reportToAssign.Id,
moderatorId: regularUserId
);

await Assert.ThrowsAsync<UnauthorizedException>(act10);

// Test 11: AssignReportAsync when moderator is valid sets report in review
var moderatorId = Guid.NewGuid();
var moderator = new User { Id = moderatorId, Role = UserRole.Moderator };
userRepoMock.Setup(r => r.GetByIdAsync(moderatorId))
.ReturnsAsync(moderator);

var assignedReport = new ModerationReport { Id = Guid.NewGuid(), Status = ReportStatus.Pending };
reportRepoMock.Setup(r => r.GetByIdAsync(assignedReport.Id))
.ReturnsAsync(assignedReport);
reportRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ModerationReport>()))
.ReturnsAsync((ModerationReport r) => r);

var result11 = await moderationService.AssignReportAsync(
reportId: assignedReport.Id,
moderatorId: moderatorId
);

Assert.Equal(ReportStatus.InReview, result11.Status);

// Test 12: AssignReportAsync when administrator sets report in review
var adminId = Guid.NewGuid();
var admin = new User { Id = adminId, Role = UserRole.Administrator };
userRepoMock.Setup(r => r.GetByIdAsync(adminId))
.ReturnsAsync(admin);

var adminReport = new ModerationReport { Id = Guid.NewGuid(), Status = ReportStatus.Pending };
reportRepoMock.Setup(r => r.GetByIdAsync(adminReport.Id))
.ReturnsAsync(adminReport);

var result12 = await moderationService.AssignReportAsync(
reportId: adminReport.Id,
moderatorId: adminId
);

Assert.Equal(ReportStatus.InReview, result12.Status);

// Test 13: ApproveReportAsync throws InvalidOperationException when report is not assigned
var unassignedReport = new ModerationReport { Id = Guid.NewGuid(), Status = ReportStatus.Pending };
reportRepoMock.Setup(r => r.GetByIdAsync(unassignedReport.Id))
.ReturnsAsync(unassignedReport);

var act13 = async () => await moderationService.ApproveReportAsync(
reportId: unassignedReport.Id,
moderatorId: moderatorId
);

await Assert.ThrowsAsync<InvalidOperationException>(act13);

// Test 14: ApproveReportAsync when assigned sets status approved
var assignedReportForApproval = new ModerationReport { Id = Guid.NewGuid(), Status = ReportStatus.InReview, AssignedModeratorId = moderatorId };
reportRepoMock.Setup(r => r.GetByIdAsync(assignedReportForApproval.Id))
.ReturnsAsync(assignedReportForApproval);
reportRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ModerationReport>()))
.ReturnsAsync((ModerationReport r) => r);

var approvedReport = await moderationService.ApproveReportAsync(
reportId: assignedReportForApproval.Id,
moderatorId: moderatorId
);

Assert.Equal(ReportStatus.Approved, approvedReport.Status);

// Test 15: RejectReportAsync throws InvalidOperationException when report is not assigned
var unassignedReportForRejection = new ModerationReport { Id = Guid.NewGuid(), Status = ReportStatus.Pending };
reportRepoMock.Setup(r => r.GetByIdAsync(unassignedReportForRejection.Id))
.ReturnsAsync(unassignedReportForRejection);

var act15 = async () => await moderationService.RejectReportAsync(
reportId: unassignedReportForRejection.Id,
moderatorId: moderatorId
);

await Assert.ThrowsAsync<InvalidOperationException>(act15);

// Test 16: RejectReportAsync when assigned sets status rejected
var assignedReportForRejection = new ModerationReport { Id = Guid.NewGuid(), Status = ReportStatus.InReview, AssignedModeratorId = moderatorId };
reportRepoMock.Setup(r => r.GetByIdAsync(assignedReportForRejection.Id))
.ReturnsAsync(assignedReportForRejection);
reportRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ModerationReport>()))
.ReturnsAsync((ModerationReport r) => r);

var rejectedReport = await moderationService.RejectReportAsync(
reportId: assignedReportForRejection.Id,
moderatorId: moderatorId
);

Assert.Equal(ReportStatus.Rejected, rejectedReport.Status);

// Test 17: EscalateReportAsync when priority below 5 increases priority
var lowPriorityReport = new ModerationReport { Id = Guid.NewGuid(), Priority = 3 };
reportRepoMock.Setup(r => r.GetByIdAsync(lowPriorityReport.Id))
.ReturnsAsync(lowPriorityReport);
reportRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ModerationReport>()))
.ReturnsAsync((ModerationReport r) => r);

var escalatedReport = await moderationService.EscalateReportAsync(lowPriorityReport.Id);

Assert.Equal(4, escalatedReport.Priority);

// Test 18: EscalateReportAsync when already at max priority does not exceed 5
var maxPriorityReport = new ModerationReport { Id = Guid.NewGuid(), Priority = 5 };
reportRepoMock.Setup(r => r.GetByIdAsync(maxPriorityReport.Id))
.ReturnsAsync(maxPriorityReport);

var noChangeReport = await moderationService.EscalateReportAsync(maxPriorityReport.Id);

Assert.Equal(5, noChangeReport.Priority);

// Test 19: SuspendUserAsync when target user exists deactivates user
var userToSuspend = new User { Id = Guid.NewGuid(), IsActive = true };
userRepoMock.Setup(r => r.GetByIdAsync(userToSuspend.Id))
.ReturnsAsync(userToSuspend);
userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>()))
.ReturnsAsync((User u) => u);

var suspendedUser = await moderationService.SuspendUserAsync(userToSuspend.Id);

Assert.False(suspendedUser.IsActive);
```

## MessagingServiceTests

The `MessagingServiceTests` class provides comprehensive unit tests for the `MessagingService` class, covering all public API methods including message sending, retrieval, marking as read/unread, deletion, flagging, and reply functionality. It validates both success and error paths, ensuring proper exception handling for edge cases like non-existent senders, recipients, or messages.

### Usage Example

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Entities;
using MarketplaceEngine.Exceptions;
using System;
using System.Threading.Tasks;

// Initialize messaging service (typically via dependency injection)
var messagingService = new MessagingService(messageRepository, userRepository);

// Test sending a message between valid users
var senderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
var recipientId = Guid.Parse("22222222-2222-2222-2222-222222222222");

var message = await messagingService.SendMessageAsync(senderId, recipientId, 
    "Interested in your premium wireless headphones", 
    "Hello! I'm interested in your listing. Could you provide more details about condition and shipping?");

Console.WriteLine($"Message sent: {message.Id} - {message.Subject}");

// Test retrieving received messages for a user
var receivedMessages = await messagingService.GetReceivedMessagesAsync(recipientId);
Console.WriteLine($"User has {receivedMessages.Count} received messages");

// Test marking a message as read
var markedMessage = await messagingService.MarkAsReadAsync(message.Id);
Console.WriteLine($"Message marked as read: {markedMessage.IsRead}");

// Test marking a message as unread
var unmarkedMessage = await messagingService.MarkAsUnreadAsync(message.Id);
Console.WriteLine($"Message marked as unread: {unmarkedMessage.IsRead}");

// Test flagging a message
var flaggedMessage = await messagingService.FlagMessageAsync(message.Id, recipientId);
Console.WriteLine($"Message flagged: {flaggedMessage.IsFlagged}");

// Test adding a reply to a message
var reply = await messagingService.AddReplyAsync(message.Id, senderId, 
    "Thank you for your interest! The headphones are in excellent condition and ready to ship.");
Console.WriteLine($"Reply added: {reply.Subject} (prefixed with Re:)");

// Test deleting a message (when requester is the sender)
await messagingService.DeleteMessageAsync(message.Id, senderId);
Console.WriteLine("Message deleted successfully");

// Test error scenarios
try
{
    // This should throw ResourceNotFoundException
    await messagingService.SendMessageAsync(Guid.NewGuid(), recipientId, "Test", "Test content");
}
catch (ResourceNotFoundException ex)
{
    Console.WriteLine($"Expected exception caught: {ex.Message}");
}

try
{
    // This should throw ArgumentException (sender equals recipient)
    await messagingService.SendMessageAsync(senderId, senderId, "Test", "Test content");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Expected exception caught: {ex.Message}");
}

try
{
    // This should throw UnauthorizedException (requester is unrelated)
    await messagingService.DeleteMessageAsync(message.Id, Guid.NewGuid());
}
catch (UnauthorizedException ex)
{
    Console.WriteLine($"Expected exception caught: {ex.Message}");
}

// Test getting conversation between two users
var conversation = await messagingService.GetConversationAsync(senderId, recipientId);
Console.WriteLine($"Conversation contains {conversation.Count} messages");
```
```
