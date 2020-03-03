#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.DTOs;
using MarketplaceEngine.Exceptions;

namespace MarketplaceEngine.Services;

/// <summary>
/// Provides extension methods for <see cref="SellerDashboardService"/> to enhance seller dashboard functionality
/// with additional convenience methods and data transformations.
/// </summary>
public static class SellerDashboardServiceExtensions
{
    /// <summary>
    /// Gets a simplified dashboard view containing only the most essential metrics for quick overview.
    /// </summary>
    /// <param name="service">The dashboard service instance.</param>
    /// <param name="sellerId">The seller identifier.</param>
    /// <returns>Simplified dashboard with key metrics only.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="sellerId"/> is equal to <see cref="Guid"/>.Empty.</exception>
    public static async Task<SimplifiedSellerDashboardDto> GetSimplifiedDashboardAsync(
        this SellerDashboardService service,
        Guid sellerId)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (sellerId == Guid.Empty)
        {
            throw new ArgumentException("Value cannot be empty.", nameof(sellerId));
        }

        var dashboard = await service.GetDashboardAsync(sellerId);

        return new SimplifiedSellerDashboardDto
        {
            SellerId = dashboard.SellerId,
            SellerName = dashboard.SellerName,
            ActiveListings = dashboard.ActiveListings,
            TotalRevenue = dashboard.TotalRevenue,
            PendingPayout = dashboard.PendingPayout,
            AverageRating = dashboard.AverageRating,
            LastActivityAt = dashboard.LastActivityAt
        };
    }

    /// <summary>
    /// Gets revenue summary with calculated platform fee percentage and profit margin.
    /// </summary>
    /// <param name="service">The dashboard service instance.</param>
    /// <param name="sellerId">The seller identifier.</param>
    /// <returns>Revenue summary with financial metrics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="sellerId"/> is equal to <see cref="Guid"/>.Empty.</exception>
    public static async Task<SellerRevenueSummaryDto> GetRevenueSummaryAsync(
        this SellerDashboardService service,
        Guid sellerId)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (sellerId == Guid.Empty)
        {
            throw new ArgumentException("Value cannot be empty.", nameof(sellerId));
        }

        var revenue = await service.GetRevenueAsync(sellerId);

        var platformFeePercentage = revenue.TotalGrossRevenue > 0
            ? Math.Round((double)((revenue.TotalPlatformFees / revenue.TotalGrossRevenue) * 100), 2)
            : 0;

        var profitMarginPercentage = revenue.TotalNetRevenue > 0
            ? Math.Round((double)(((revenue.TotalNetRevenue - revenue.TotalPlatformFees) / revenue.TotalNetRevenue) * 100), 2)
            : 0;

        return new SellerRevenueSummaryDto
        {
            SellerId = revenue.SellerId,
            TotalGrossRevenue = revenue.TotalGrossRevenue,
            TotalNetRevenue = revenue.TotalNetRevenue,
            TotalPlatformFees = revenue.TotalPlatformFees,
            PlatformFeePercentage = platformFeePercentage,
            ProfitMarginPercentage = profitMarginPercentage,
            PendingPayout = revenue.PendingPayout,
            MonthlyBreakdown = revenue.MonthlyBreakdown
        };
    }

    /// <summary>
    /// Gets listing performance summary with calculated engagement rate and conversion metrics.
    /// </summary>
    /// <param name="service">The dashboard service instance.</param>
    /// <param name="sellerId">The seller identifier.</param>
    /// <returns>Listing performance summary with calculated metrics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="sellerId"/> is equal to <see cref="Guid"/>.Empty.</exception>
    public static async Task<SellerListingPerformanceDto> GetListingPerformanceAsync(
        this SellerDashboardService service,
        Guid sellerId)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (sellerId == Guid.Empty)
        {
            throw new ArgumentException("Value cannot be empty.", nameof(sellerId));
        }

        var stats = await service.GetListingStatsAsync(sellerId);

        var totalListings = stats.ActiveListings + stats.InactiveListings;
        var engagementRate = totalListings > 0
            ? Math.Round((double)stats.TotalViews / totalListings, 2)
            : 0;

        var conversionRate = stats.TotalViews > 0
            ? Math.Round((double)stats.TotalInterestCount / stats.TotalViews * 100, 2)
            : 0;

        return new SellerListingPerformanceDto
        {
            SellerId = stats.SellerId,
            ActiveListings = stats.ActiveListings,
            InactiveListings = stats.InactiveListings,
            FeaturedListings = stats.FeaturedListings,
            TotalViews = stats.TotalViews,
            TotalInterestCount = stats.TotalInterestCount,
            EngagementRate = engagementRate,
            ConversionRate = conversionRate,
            TopListings = stats.TopListings
        };
    }

    /// <summary>
    /// Gets a comparison dashboard showing this seller's metrics against marketplace averages.
    /// </summary>
    /// <param name="service">The dashboard service instance.</param>
    /// <param name="sellerId">The seller identifier.</param>
    /// <param name="marketplaceAverage">Marketplace average metrics.</param>
    /// <returns>Comparison dashboard with relative performance metrics.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="service"/> is <see langword="null"/>.
    /// <paramref name="marketplaceAverage"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="sellerId"/> is equal to <see cref="Guid"/>.Empty.</exception>
    public static async Task<SellerComparisonDashboardDto> GetComparisonDashboardAsync(
        this SellerDashboardService service,
        Guid sellerId,
        MarketplaceAverageMetrics marketplaceAverage)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(marketplaceAverage);
        if (sellerId == Guid.Empty)
        {
            throw new ArgumentException("Value cannot be empty.", nameof(sellerId));
        }

        var dashboard = await service.GetDashboardAsync(sellerId);
        var revenue = await service.GetRevenueAsync(sellerId);
        var stats = await service.GetListingStatsAsync(sellerId);

        var revenueAboveAverage = marketplaceAverage.AverageRevenue > 0
            ? Math.Round((double)(((revenue.TotalNetRevenue - marketplaceAverage.AverageRevenue) / marketplaceAverage.AverageRevenue) * 100), 2)
            : 0;

        var totalListings = stats.ActiveListings + stats.InactiveListings;
        var listingsAboveAverage = marketplaceAverage.AverageListings > 0
            ? Math.Round(((double)totalListings - marketplaceAverage.AverageListings) / marketplaceAverage.AverageListings * 100, 2)
            : 0;

        var ratingDifference = dashboard.AverageRating - marketplaceAverage.AverageRating;

        return new SellerComparisonDashboardDto
        {
            SellerId = dashboard.SellerId,
            SellerName = dashboard.SellerName,

            // Absolute values
            ActiveListings = dashboard.ActiveListings,
            TotalRevenue = revenue.TotalNetRevenue,
            AverageRating = dashboard.AverageRating,

            // Marketplace averages
            MarketplaceAverageRevenue = marketplaceAverage.AverageRevenue,
            MarketplaceAverageListings = marketplaceAverage.AverageListings,
            MarketplaceAverageRating = marketplaceAverage.AverageRating,

            // Relative performance
            RevenueAboveAverage = revenueAboveAverage,
            ListingsAboveAverage = listingsAboveAverage,
            RatingDifference = ratingDifference,

            // Status indicators
            IsRevenueAboveAverage = revenue.TotalNetRevenue > marketplaceAverage.AverageRevenue,
            IsListingsAboveAverage = totalListings > marketplaceAverage.AverageListings,
            IsRatingAboveAverage = dashboard.AverageRating > marketplaceAverage.AverageRating
        };
    }
}