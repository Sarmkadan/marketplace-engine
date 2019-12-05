#nullable enable

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Comparison dashboard showing this seller's metrics against marketplace averages.
/// </summary>
public class SellerComparisonDashboardDto
{
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;

    // Absolute values
    public int ActiveListings { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AverageRating { get; set; }

    // Marketplace averages
    public decimal MarketplaceAverageRevenue { get; set; }
    public int MarketplaceAverageListings { get; set; }
    public double MarketplaceAverageRating { get; set; }

    // Relative performance
    public double RevenueAboveAverage { get; set; }
    public double ListingsAboveAverage { get; set; }
    public double RatingDifference { get; set; }

    // Status indicators
    public bool IsRevenueAboveAverage { get; set; }
    public bool IsListingsAboveAverage { get; set; }
    public bool IsRatingAboveAverage { get; set; }
}