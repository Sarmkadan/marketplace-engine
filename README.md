# Marketplace Engine

...

## MessageDtoExtensions

Provides extension methods for `MessageDto` to simplify common operations and validations. Includes methods to verify message participants, get display names, and format message content.

### Usage Example

```csharp
using MarketplaceEngine.DTOs;

// Assuming you have a MessageDto instance
var message = new MessageDto
{
    SenderId = Guid.NewGuid(),
    RecipientId = Guid.NewGuid(),
    Content = "Is this item still available?"
};

// Verify message sender
bool isSentByUser = MessageDtoExtensions.IsSentBy(message, userId: Guid.NewGuid());

// Get display name for other participant
string displayName = MessageDtoExtensions.GetDisplayName(message, currentUserId: Guid.NewGuid());

// Format message content for display
string formattedContent = MessageDtoExtensions.FormatContent(message);

// Verify if message is between two specific users
bool isBetweenUsers = MessageDtoExtensions.IsBetweenUsers(message, userId1: Guid.NewGuid(), userId2: Guid.NewGuid());

Console.WriteLine($"Is sent by user: {isSentByUser}");
Console.WriteLine($"Display name: {displayName}");
Console.WriteLine($"Formatted content: {formattedContent}");
Console.WriteLine($"Is between users: {isBetweenUsers}");
```

## SellerComparisonDashboardDto

The `SellerComparisonDashboardDto` class provides a comprehensive overview of a seller's performance compared to marketplace averages. It includes metrics such as active listings, total revenue, average rating, and more.

### Usage Example

```csharp
using MarketplaceEngine.DTOs;

var sellerComparison = new SellerComparisonDashboardDto
{
    SellerId = Guid.NewGuid(),
    SellerName = "Example Seller",
    ActiveListings = 10,
    TotalRevenue = 1000.00m,
    AverageRating = 4.5,
    MarketplaceAverageRevenue = 800.00m,
    MarketplaceAverageListings = 8,
    MarketplaceAverageRating = 4.2,
    RevenueAboveAverage = 200.00,
    ListingsAboveAverage = 2,
    RatingDifference = 0.3,
    IsRevenueAboveAverage = true,
    IsListingsAboveAverage = true,
    IsRatingAboveAverage = true
};

Console.WriteLine($"Seller: {sellerComparison.SellerName} (ID: {sellerComparison.SellerId})");
Console.WriteLine($"Active Listings: {sellerComparison.ActiveListings} (Marketplace Average: {sellerComparison.MarketplaceAverageListings})");
Console.WriteLine($"Total Revenue: {sellerComparison.TotalRevenue:C} (Marketplace Average: {sellerComparison.MarketplaceAverageRevenue:C})");
Console.WriteLine($"Average Rating: {sellerComparison.AverageRating} (Marketplace Average: {sellerComparison.MarketplaceAverageRating})");
Console.WriteLine($"Revenue Above Average: {sellerComparison.RevenueAboveAverage:C}");
Console.WriteLine($"Listings Above Average: {sellerComparison.ListingsAboveAverage}");
Console.WriteLine($"Rating Difference: {sellerComparison.RatingDifference}");
Console.WriteLine($"Is Revenue Above Average: {sellerComparison.IsRevenueAboveAverage}");
Console.WriteLine($"Is Listings Above Average: {sellerComparison.IsListingsAboveAverage}");
Console.WriteLine($"Is Rating Above Average: {sellerComparison.IsRatingAboveAverage}");
```

## ListingCreatedEventExtensions

...
