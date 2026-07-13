# Marketplace Engine

...

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
