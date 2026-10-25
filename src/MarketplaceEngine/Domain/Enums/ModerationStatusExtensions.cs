#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.Enums;

/// <summary>
/// Provides common state and display helpers for <see cref="ModerationStatus"/> values.
/// </summary>
public static class ModerationStatusExtensions
{
    /// <summary>
    /// Determines whether the status means the report still awaits a moderation decision,
    /// either not yet picked up (<see cref="ModerationStatus.Pending"/>) or currently under
    /// review (<see cref="ModerationStatus.InReview"/>).
    /// </summary>
    /// <param name="status">The status to evaluate.</param>
    /// <returns><see langword="true"/> when the status is pending or in review; otherwise, <see langword="false"/>.</returns>
    public static bool IsPending(this ModerationStatus status)
    {
        return status is ModerationStatus.Pending or ModerationStatus.InReview;
    }

    /// <summary>
    /// Gets a human-readable display name for the status.
    /// </summary>
    /// <param name="status">The status to format.</param>
    /// <returns>The display name of the status.</returns>
    public static string ToDisplayName(this ModerationStatus status)
    {
        return status switch
        {
            ModerationStatus.Pending => "Pending",
            ModerationStatus.InReview => "In Review",
            ModerationStatus.Approved => "Approved",
            ModerationStatus.Rejected => "Rejected",
            ModerationStatus.ContentRemoved => "Content Removed",
            ModerationStatus.UserSuspended => "User Suspended",
            ModerationStatus.UserBanned => "User Banned",
            _ => status.ToString()
        };
    }
}
