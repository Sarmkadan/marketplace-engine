#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Provides rounding and formatting helpers for <see cref="MarketplaceAverageMetrics"/> instances.
/// </summary>
public static class MarketplaceAverageMetricsExtensions
{
    /// <summary>
    /// Maximum number of decimal places accepted by the rounding and formatting helpers.
    /// </summary>
    private const int MaxDecimalPlaces = 15;

    /// <summary>
    /// Rounds the average revenue to the specified number of decimal places.
    /// </summary>
    /// <param name="metrics">The metrics to round.</param>
    /// <param name="decimals">The number of decimal places (0 to 15). Defaults to 2.</param>
    /// <returns>The average revenue rounded away from zero at midpoints.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="decimals"/> is outside 0 to 15.</exception>
    public static decimal RoundAverageRevenue(this MarketplaceAverageMetrics metrics, int decimals = 2)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ValidateDecimals(decimals);

        return Math.Round(metrics.AverageRevenue, decimals, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Rounds the average rating to the specified number of decimal places.
    /// </summary>
    /// <param name="metrics">The metrics to round.</param>
    /// <param name="decimals">The number of decimal places (0 to 15). Defaults to 1.</param>
    /// <returns>The average rating rounded away from zero at midpoints.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="decimals"/> is outside 0 to 15.</exception>
    public static double RoundAverageRating(this MarketplaceAverageMetrics metrics, int decimals = 1)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ValidateDecimals(decimals);

        return Math.Round(metrics.AverageRating, decimals, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Formats the average revenue as a currency string.
    /// </summary>
    /// <param name="metrics">The metrics to format.</param>
    /// <param name="provider">The format provider. Defaults to <see cref="CultureInfo.InvariantCulture"/>.</param>
    /// <returns>The average revenue formatted with the "C2" currency format.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is <see langword="null"/>.</exception>
    public static string FormatAverageRevenue(this MarketplaceAverageMetrics metrics, IFormatProvider? provider = null)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        return metrics.AverageRevenue.ToString("C2", provider ?? CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats the average rating as a fixed-point string.
    /// </summary>
    /// <param name="metrics">The metrics to format.</param>
    /// <param name="decimals">The number of decimal places (0 to 15). Defaults to 1.</param>
    /// <returns>The average rating formatted with the requested number of decimal places.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="decimals"/> is outside 0 to 15.</exception>
    public static string FormatAverageRating(this MarketplaceAverageMetrics metrics, int decimals = 1)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ValidateDecimals(decimals);

        return metrics.AverageRating.ToString("F" + decimals.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Validates that the number of decimal places is within the supported range.
    /// </summary>
    /// <param name="decimals">The number of decimal places to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="decimals"/> is outside 0 to 15.</exception>
    private static void ValidateDecimals(int decimals)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(decimals);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(decimals, MaxDecimalPlaces);
    }
}
