#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using MarketplaceEngine.Domain.Enums;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Extension methods for ModerationReport providing additional functionality
/// </summary>
public static class ModerationReportExtensions
{
    /// <summary>
    /// Determines if the report is actionable based on status and priority.
    /// Reports are actionable if they are pending/in review and have priority >= 3.
    /// </summary>
    /// <param name="report">The moderation report to check</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null</exception>
    /// <returns>True if the report requires immediate attention</returns>
    public static bool IsActionable(this ModerationReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return report.IsPending() && report.Priority >= 3;
    }

    /// <summary>
    /// Gets the target description for display purposes, combining type and identifier.
    /// </summary>
    /// <param name="report">The moderation report</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null</exception>
    /// <returns>Formatted target description or null if no target</returns>
    public static string? GetTargetDescription(this ModerationReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (report.TargetUserId.HasValue && report.TargetUserId != Guid.Empty)
        {
            return $"User: {report.TargetUser?.FullName ?? report.TargetUserId.ToString()}";
        }

        if (report.TargetListingId.HasValue && report.TargetListingId != Guid.Empty)
        {
            return $"Listing: {report.TargetListing?.Title ?? report.TargetListingId.ToString()}";
        }

        if (report.TargetMessageId.HasValue && report.TargetMessageId != Guid.Empty)
        {
            return $"Message: {report.TargetMessageId}";
        }

        return null;
    }

    /// <summary>
    /// Calculates the age of the report in hours since creation.
    /// </summary>
    /// <param name="report">The moderation report</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null</exception>
    /// <returns>Age in hours, or 0 if not yet created</returns>
    public static double GetAgeInHours(this ModerationReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (report.CreatedAt == default)
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        var age = now - report.CreatedAt;
        return age.TotalHours;
    }

    /// <summary>
    /// Determines if the report is overdue based on priority and age.
    /// High priority reports become overdue faster than low priority ones.
    /// </summary>
    /// <param name="report">The moderation report</param>
    /// <param name="currentTime">Optional current time for testing; defaults to DateTime.UtcNow</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null</exception>
    /// <returns>True if the report is overdue</returns>
    public static bool IsOverdue(this ModerationReport report, DateTime? currentTime = null)
    {
        ArgumentNullException.ThrowIfNull(report);

        var now = currentTime ?? DateTime.UtcNow;
        var ageHours = report.GetAgeInHours();

        return report.IsPending() && ageHours > GetOverdueThresholdHours(report.Priority);
    }

    private static int GetOverdueThresholdHours(int priority) => priority switch
    {
        5 => 1,   // Urgent: 1 hour
        4 => 4,   // Critical: 4 hours
        3 => 12,  // High: 12 hours
        2 => 48,  // Medium: 48 hours
        _ => 168  // Low: 1 week
    };
}