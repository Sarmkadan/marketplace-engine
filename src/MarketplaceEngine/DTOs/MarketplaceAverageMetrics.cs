#nullable enable

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Marketplace average metrics for comparison purposes.
/// </summary>
public class MarketplaceAverageMetrics
{
    public decimal AverageRevenue { get; set; }
    public int AverageListings { get; set; }
    public double AverageRating { get; set; }
}