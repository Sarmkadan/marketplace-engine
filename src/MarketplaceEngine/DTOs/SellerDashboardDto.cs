#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.DTOs;

/// <summary>
/// High-level overview metrics for the seller dashboard.
/// </summary>
public class SellerDashboardDto
{
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public int ActiveListings { get; set; }
    public int TotalListings { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingPayout { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int UnreadMessages { get; set; }
    public DateTime? LastActivityAt { get; set; }
}

/// <summary>
/// Revenue breakdown for the seller dashboard.
/// </summary>
public class SellerRevenueDto
{
    public Guid SellerId { get; set; }
    public decimal TotalGrossRevenue { get; set; }
    public decimal TotalPlatformFees { get; set; }
    public decimal TotalNetRevenue { get; set; }
    public decimal PendingPayout { get; set; }
    public List<MonthlyRevenueDto> MonthlyBreakdown { get; set; } = [];
}

/// <summary>
/// Monthly revenue figures for chart rendering.
/// </summary>
public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel => $"{Year}-{Month:D2}";
    public decimal GrossRevenue { get; set; }
    public decimal NetRevenue { get; set; }
    public int SalesCount { get; set; }
}

/// <summary>
/// Performance metrics for the seller's listings.
/// </summary>
public class SellerListingStatsDto
{
    public Guid SellerId { get; set; }
    public int ActiveListings { get; set; }
    public int InactiveListings { get; set; }
    public int FeaturedListings { get; set; }
    public long TotalViews { get; set; }
    public long TotalInterestCount { get; set; }
    public List<TopListingDto> TopListings { get; set; } = [];
}

/// <summary>
/// Summary for a single listing shown in dashboard rankings.
/// </summary>
public class TopListingDto
{
    public Guid ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int InterestCount { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
}
