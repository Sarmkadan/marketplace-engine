#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.Enums;

/// <summary>
/// Provides visibility and display helpers for <see cref="ReviewStatus"/> values.
/// </summary>
public static class ReviewStatusExtensions
{
    /// <summary>
    /// Determines whether a review with the status is visible to the public.
    /// Only active reviews are shown; reviews under moderation or removed are hidden.
    /// </summary>
    /// <param name="status">The status to evaluate.</param>
    /// <returns><see langword="true"/> when the status is visible; otherwise, <see langword="false"/>.</returns>
    public static bool IsVisible(this ReviewStatus status)
    {
        return status == ReviewStatus.Active;
    }

    /// <summary>
    /// Gets a human-readable display name for the status.
    /// </summary>
    /// <param name="status">The status to format.</param>
    /// <returns>The display name of the status.</returns>
    public static string ToDisplayName(this ReviewStatus status)
    {
        return status switch
        {
            ReviewStatus.Active => "Active",
            ReviewStatus.UnderReview => "Under Review",
            ReviewStatus.Removed => "Removed",
            _ => status.ToString()
        };
    }
}
