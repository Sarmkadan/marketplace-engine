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

## IRecommendationEngine

The `IRecommendationEngine` interface defines the contract for recommendation engine implementations in the Marketplace Engine. It supports multiple recommendation strategies including collaborative filtering, item-based similarity, category-affinity, and trending-based recommendations with graceful fallbacks for cold-start scenarios. Implementations record user signals and compute personalized recommendations that power the marketplace's feed and discovery features.

### Usage Example

```csharp
using MarketplaceEngine.Recommendations;
using System;
using System.Threading;

// Initialize the recommendation engine with required dependencies
// (In a real application, these would be injected via DI container)
var options = RecommendationOptions.CreateDefault();
var cacheService = new CacheService("RecommendationsCache");
var activityTracker = new UserActivityTracker(options, null);
var listingRepository = new ListingRepository(); // Mock implementation
var logger = new Logger<CollaborativeFilteringEngine>(null);

var engine = new CollaborativeFilteringEngine(
    activityTracker,
    listingRepository,
    cacheService,
    options,
    logger);

// Record user activity signals to build recommendation data
var userId = Guid.NewGuid();
var listingId = Guid.NewGuid();

await engine.RecordSignalAsync(new UserActivitySignal
{
    UserId = userId,
    ListingId = listingId,
    SignalType = SignalType.View,
    OccurredAt = DateTime.UtcNow
});

// Get personalized recommendations for a user
var userRecommendations = await engine.ComputeForUserAsync(userId, 10);
Console.WriteLine($"Found {userRecommendations.Count} personalized recommendations");

// Get similar listings to a specific listing
var similarListings = await engine.ComputeSimilarAsync(listingId, 5);
Console.WriteLine($"Found {similarListings.Count} similar listings");

// Get trending listings based on recent activity
var trendingListings = await engine.ComputeTrendingAsync(10);
Console.WriteLine($"Found {trendingListings.Count} trending listings");

// Get recommendations based on category affinity
var affinityRecommendations = await engine.ComputeByAffinityAsync(userId, 8);
Console.WriteLine($"Found {affinityRecommendations.Count} affinity-based recommendations");
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

## CollaborativeFilteringEngine

The `CollaborativeFilteringEngine` class implements a recommendation engine that combines user-based collaborative filtering, item-based collaborative filtering, category-affinity scoring, and popularity-based trending to provide personalized recommendations. It uses cosine similarity on interaction vectors for user-based recommendations, co-interaction co-occurrence for item similarity, weighted category preferences for affinity scoring, and signal velocity for trending recommendations.

### Usage Example

```csharp
using MarketplaceEngine.Recommendations;
using MarketplaceEngine.DTOs;
using System;
using System.Threading;

// Initialize the recommendation engine with required dependencies
// (In a real application, these would be injected via DI container)
var options = RecommendationOptions.CreateDefault();
var cacheService = new CacheService("RecommendationsCache");
var activityTracker = new UserActivityTracker(options, null);
var listingRepository = new ListingRepository(); // Mock implementation
var logger = new Logger<CollaborativeFilteringEngine>(null);

var engine = new CollaborativeFilteringEngine(
    activityTracker,
    listingRepository,
    cacheService,
    options,
    logger);

// Record user activity signals
var userId = Guid.NewGuid();
var listingId = Guid.NewGuid();

await engine.RecordSignalAsync(new UserActivitySignal
{
    UserId = userId,
    ListingId = listingId,
    SignalType = SignalType.View,
    OccurredAt = DateTime.UtcNow
});

// Get personalized recommendations for a user
var userRecommendations = await engine.ComputeForUserAsync(userId, 10);
Console.WriteLine($"Found {userRecommendations.Count} personalized recommendations");

// Get similar listings to a specific listing
var similarListings = await engine.ComputeSimilarAsync(listingId, 5);
Console.WriteLine($"Found {similarListings.Count} similar listings");

// Get trending listings
var trendingListings = await engine.ComputeTrendingAsync(10);
Console.WriteLine($"Found {trendingListings.Count} trending listings");

// Get recommendations based on category affinity
var affinityRecommendations = await engine.ComputeByAffinityAsync(userId, 8);
Console.WriteLine($"Found {affinityRecommendations.Count} affinity-based recommendations");

// Record additional signals to refine future recommendations
await engine.RecordSignalAsync(new UserActivitySignal
{
    UserId = userId,
    ListingId = Guid.NewGuid(), // Different listing
    SignalType = SignalType.Purchase,
    OccurredAt = DateTime.UtcNow.AddMinutes(-15)
});
```

## Category

The `Category` class represents a marketplace category for organizing listings hierarchically. It supports parent-child relationships, maintains listing counts, and provides methods for managing subcategories and validating category data. Categories are essential for navigation, search filtering, and recommendation engines.

### Usage Example

```csharp
using MarketplaceEngine.Domain.Models;
using System;

// Create a parent category
var electronics = new Category
{
    Id = Guid.NewGuid(),
    Name = "Electronics",
    Description = "Electronic devices and components",
    IconUrl = "/icons/electronics.png",
    DisplayOrder = 1,
    IsActive = true,
    CreatedAt = DateTime.UtcNow
};

// Validate and initialize the category
electronics.ValidateAndInitialize();

// Create a subcategory
var smartphones = new Category
{
    Id = Guid.NewGuid(),
    Name = "Smartphones",
    Description = "Mobile phones and accessories",
    IconUrl = "/icons/smartphones.png",
    DisplayOrder = 1,
    IsActive = true,
    CreatedAt = DateTime.UtcNow
};

smartphones.ValidateAndInitialize();

// Add subcategory to parent
electronics.AddSubCategory(smartphones);

// Create a listing and add it to the category
var phoneListing = new Listing
{
    Id = Guid.NewGuid(),
    Title = "iPhone 15 Pro",
    Description = "Latest smartphone with advanced features",
    Price = 999.99m,
    SellerId = Guid.NewGuid(),
    Status = ListingStatus.Active,
    CreatedAt = DateTime.UtcNow
};

// Add listing to category
category.Listings.Add(phoneListing);
category.IncrementListingCount();

// Check if category has active listings
var hasActive = category.HasActiveListings();
Console.WriteLine($"Category has active listings: {hasActive}");

// Get full category path
var fullPath = category.GetFullPath();
Console.WriteLine($"Category path: {fullPath}");

// Remove a subcategory
var removed = category.RemoveSubCategory(smartphones.Id);
Console.WriteLine($"Subcategory removed: {removed}");
```

## Listing

The `Listing` class represents a product or service listing in the marketplace. It encapsulates all essential information about a marketplace item including seller details, pricing, location, media, tags, engagement metrics, and status tracking. Listings are the core entity for buying and selling activities in the marketplace.

### Usage Example

```csharp
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using System;

// Create a new listing
var listing = new Listing
{
    Id = Guid.NewGuid(),
    SellerId = Guid.NewGuid(),
    Title = "Premium Mountain Bike",
    Description = "Hardtail mountain bike with 29-inch wheels, hydraulic disc brakes, and 12-speed drivetrain. Excellent condition, barely used.",
    Price = new Money(799.99m, "USD"),
    CategoryId = Guid.NewGuid(),
    Status = ListingStatus.Active,
    Location = new Location("New York", "NY", "10001", "Manhattan", "123 Main St"),
    ImageUrls = new List<string>
    {
        "https://example.com/images/bike-1.jpg",
        "https://example.com/images/bike-2.jpg"
    },
    Tags = new List<string> { "bike", "mountain", "outdoor", "sports" },
    IsFeatured = true,
    DueDate = DateTime.UtcNow.AddDays(30),
    Condition = "Like New"
};

// Publish the listing
listing.Publish();

Console.WriteLine($"Listing published: {listing.PublishedAt}");
Console.WriteLine($"Status: {listing.Status}");

// Record user engagement
listing.RecordView();
listing.RecordInterest();

// Add additional tags
listing.AddTag("cycling");
listing.AddTag("recreational");

// Add more images
listing.AddImage("https://example.com/images/bike-3.jpg");

// Mark as featured
listing.MarkAsFeatured();

Console.WriteLine($"View count: {listing.ViewCount}");
Console.WriteLine($"Interest count: {listing.InterestCount}");
Console.WriteLine($"Tags: {string.Join(", ", listing.Tags)}");
Console.WriteLine($"Is featured: {listing.IsFeatured}");

// Archive when sold
listing.Delist();
```

## Review

The `Review` class represents a customer's feedback on a listing, allowing reviewers to rate their experience, provide comments, and flag content for moderation. It manages the review lifecycle including seller replies, status tracking, and validation logic to ensure high-quality, authentic feedback within the marketplace.

### Usage Example

```csharp
using MarketplaceEngine.Domain.Models;
using System;

// Create a new review
var review = new Review
{
    Id = Guid.NewGuid(),
    ReviewerId = Guid.NewGuid(),
    SellerId = Guid.NewGuid(),
    ListingId = Guid.NewGuid(),
    Score = 5,
    Comment = "Great service and fast shipping!",
    Status = ReviewStatus.Pending,
    CreatedAt = DateTime.UtcNow
};

// Validate the review content
review.ValidateReview();

// Add a seller reply
review.AddSellerReply("Thank you for your kind words!");

// Flag for review if necessary
review.FlagForReview();

// Mark the review as removed/hidden if it violates policies
review.Remove();

Console.WriteLine($"Review status: {review.Status}");
Console.WriteLine($"Seller reply: {review.SellerReply}");
```

## Payment

The `Payment` class represents a financial transaction within the marketplace, capturing essential details such as the parties involved, the amount, and the transaction's lifecycle status. It tracks the payment from creation through to completion, refund, or failure, ensuring auditability and traceability using metadata and external transaction identifiers.

### Usage Example

```csharp
using MarketplaceEngine.Domain.Models;
using System;
using System.Collections.Generic;

// Create a new payment instance
var payment = new Payment
{
    Id = Guid.NewGuid(),
    ListingId = Guid.NewGuid(),
    BuyerId = Guid.NewGuid(),
    SellerId = Guid.NewGuid(),
    Amount = new Money(100.00m, "USD"),
    Status = PaymentStatus.Pending,
    CreatedAt = DateTime.UtcNow,
    Metadata = new Dictionary<string, string>
    {
        { "Reference", "INV-1001" },
        { "Channel", "Web" }
    }
};

Console.WriteLine($"Payment created: {payment.Id}");
Console.WriteLine($"Amount: {payment.Amount.Value} {payment.Amount.Currency}");
Console.WriteLine($"Status: {payment.Status}");
```

## User

The `User` class represents a marketplace user who can act as both a buyer and seller. It encapsulates user profile information, authentication state, activity tracking, and seller statistics. The class provides methods for email verification, profile validation, account management, and activity tracking.

### Usage Example

```csharp
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using System;

// Create a new user
var user = new User
{
    Id = Guid.NewGuid(),
    Email = "john.doe@example.com",
    FullName = "John Doe",
    Phone = "+1-555-123-4567",
    ProfileImageUrl = "/avatars/john-doe.jpg",
    Bio = "Experienced software developer and tech enthusiast",
    Role = UserRole.Seller,
    Location = new Location("San Francisco", "CA", "94105", "Downtown", "123 Market St"),
    IsVerified = true,
    IsActive = true,
    TotalListings = 25,
    TotalSales = 42,
    CreatedAt = DateTime.UtcNow.AddDays(-30)
};

// Validate user profile and email
user.ValidateProfile();
user.ValidateEmail();

// Record user activity
user.UpdateLastActivity();

// Promote to premium seller if eligible
if (user.TotalSales >= 10)
{
    user.PromoteToPremiumSeller();
}

// Record a successful sale
user.RecordSale();

// Generate verification token for email confirmation
user.GenerateVerificationToken();

Console.WriteLine($"User: {user.FullName} ({user.Email})");
Console.WriteLine($"Role: {user.Role}");
Console.WriteLine($"Total listings: {user.TotalListings}");
Console.WriteLine($"Total sales: {user.TotalSales}");
Console.WriteLine($"Is verified: {user.IsVerified}");
```

## ModerationReport

The `ModerationReport` class represents a submission reporting a violation of platform guidelines, such as inappropriate user behavior, harmful content in listings, or message abuse. It tracks the reporter, the target (user, listing, or message), the reason for the report, and the current administrative status for review.

### Usage Example

```csharp
using MarketplaceEngine.Domain.Models;
using System;

// Create a new moderation report for a user violation
var moderationReport = new ModerationReport
{
    Id = Guid.NewGuid(),
    ReporterId = Guid.NewGuid(),
    TargetUserId = Guid.NewGuid(),
    Reason = "Harassment",
    Details = "User sent offensive messages.",
    Status = ModerationStatus.Pending,
    Priority = 2,
    CreatedAt = DateTime.UtcNow
};

Console.WriteLine($"Report ID: {moderationReport.Id}");
Console.WriteLine($"Report Status: {moderationReport.Status}");
```

