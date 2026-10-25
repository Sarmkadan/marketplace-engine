#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Extension methods for <see cref="SellerListingPerformanceDto"/>.
/// </summary>
public static class SellerListingPerformanceDtoExtensions
{
    /// <summary>
    /// Calculates the conversion rate as a percentage of total interest to total views,
    /// rounded to two decimal places. Uses the same formula as the seller dashboard service.
    /// </summary>
    /// <param name="dto">The listing performance summary to calculate the conversion rate for.</param>
    /// <returns>The conversion rate percentage, or 0 when there are no views.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dto"/> is <see langword="null"/>.</exception>
    public static double CalculateConversionRate(this SellerListingPerformanceDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.TotalViews <= 0)
        {
            return 0;
        }

        return Math.Round((double)dto.TotalInterestCount / dto.TotalViews * 100, 2);
    }

    /// <summary>
    /// Gets the total number of listings, both active and inactive.
    /// </summary>
    /// <param name="dto">The listing performance summary to count listings for.</param>
    /// <returns>The sum of active and inactive listings.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dto"/> is <see langword="null"/>.</exception>
    public static int GetTotalListings(this SellerListingPerformanceDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return dto.ActiveListings + dto.InactiveListings;
    }
}
