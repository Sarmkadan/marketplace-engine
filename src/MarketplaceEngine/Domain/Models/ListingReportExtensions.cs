#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Provides extension methods for the <see cref="ListingReport"/> class to support
/// summary formatting and display of listing reports.
/// </summary>
public static class ListingReportExtensions
{
    private const string Ellipsis = "...";

    /// <summary>
    /// Builds a single-line, human-readable summary of the report.
    /// </summary>
    /// <param name="report">The listing report.</param>
    /// <returns>A summary in the form "Listing {id} reported by {reporter} at {timestamp} UTC: {reason}".</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    public static string FormatSummary(this ListingReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        return $"Listing {report.ListingId} reported by {report.ReporterUserId} at {report.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC: {report.Reason}";
    }

    /// <summary>
    /// Returns the report reason, truncated to <paramref name="maxLength"/> characters.
    /// Truncated reasons end with an ellipsis when there is room for it.
    /// </summary>
    /// <param name="report">The listing report.</param>
    /// <param name="maxLength">The maximum length of the returned text. Must not be negative.</param>
    /// <returns>The reason itself if it fits, otherwise a truncated copy.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is negative.</exception>
    public static string GetTruncatedReason(this ListingReport report, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        var reason = report.Reason ?? string.Empty;
        if (reason.Length <= maxLength)
        {
            return reason;
        }

        if (maxLength <= Ellipsis.Length)
        {
            return reason.Substring(0, maxLength);
        }

        return string.Concat(reason.AsSpan(0, maxLength - Ellipsis.Length), Ellipsis);
    }

    /// <summary>
    /// Calculates how many hours have passed since the report was created.
    /// </summary>
    /// <param name="report">The listing report.</param>
    /// <param name="currentTimeUtc">Optional current UTC time; defaults to <see cref="DateTime.UtcNow"/>.</param>
    /// <returns>The age in hours, or 0 if the report has no creation timestamp.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    public static double GetAgeInHours(this ListingReport report, DateTime? currentTimeUtc = null)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (report.CreatedAtUtc == default)
        {
            return 0;
        }

        var now = currentTimeUtc ?? DateTime.UtcNow;
        return (now - report.CreatedAtUtc).TotalHours;
    }
}
