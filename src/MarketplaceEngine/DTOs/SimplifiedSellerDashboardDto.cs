#nullable enable

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Simplified dashboard view containing only the most essential metrics for quick overview.
/// </summary>
public class SimplifiedSellerDashboardDto
{
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public int ActiveListings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingPayout { get; set; }
    public double AverageRating { get; set; }
    public DateTime? LastActivityAt { get; set; }
}