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
```
