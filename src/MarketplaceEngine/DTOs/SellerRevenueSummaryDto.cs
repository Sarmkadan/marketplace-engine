#nullable enable

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Revenue summary with calculated platform fee percentage and profit margin.
/// </summary>
public class SellerRevenueSummaryDto
{
    public Guid SellerId { get; set; }
    public decimal TotalGrossRevenue { get; set; }
    public decimal TotalNetRevenue { get; set; }
    public decimal TotalPlatformFees { get; set; }
    public double PlatformFeePercentage { get; set; }
    public double ProfitMarginPercentage { get; set; }
    public decimal PendingPayout { get; set; }
    public List<MonthlyRevenueDto> MonthlyBreakdown { get; set; } = [];
}