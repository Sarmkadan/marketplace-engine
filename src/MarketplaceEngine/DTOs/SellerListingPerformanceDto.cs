#nullable enable

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Listing performance summary with calculated engagement rate and conversion metrics.
/// </summary>
public class SellerListingPerformanceDto
{
    public Guid SellerId { get; set; }
    public int ActiveListings { get; set; }
    public int InactiveListings { get; set; }
    public int FeaturedListings { get; set; }
    public long TotalViews { get; set; }
    public long TotalInterestCount { get; set; }
    public double EngagementRate { get; set; }
    public double ConversionRate { get; set; }
    public List<TopListingDto> TopListings { get; set; } = [];
}