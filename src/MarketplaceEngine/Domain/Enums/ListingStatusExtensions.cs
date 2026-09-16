#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.Enums;

/// <summary>
/// Provides common status and display helpers for <see cref="ListingStatus"/> values.
/// </summary>
public static class ListingStatusExtensions
{
    /// <summary>
    /// Determines whether the status represents a terminal state from which a listing
    /// can no longer transition back to an active or reviewable state.
    /// </summary>
    /// <param name="status">The status to evaluate.</param>
    /// <returns><see langword="true"/> when the status is terminal; otherwise, <see langword="false"/>.</returns>
    public static bool IsTerminal(this ListingStatus status)
    {
        return status is ListingStatus.Delisted or ListingStatus.Archived;
    }

    /// <summary>
    /// Gets a human-readable display name for the status.
    /// </summary>
    /// <param name="status">The status to format.</param>
    /// <returns>The display name of the status.</returns>
    public static string ToDisplayName(this ListingStatus status)
    {
        return status switch
        {
            ListingStatus.Active => "Active",
            ListingStatus.Inactive => "Inactive",
            ListingStatus.UnderReview => "Under Review",
            ListingStatus.Flagged => "Flagged",
            ListingStatus.Delisted => "Delisted",
            ListingStatus.Archived => "Archived",
            ListingStatus.Draft => "Draft",
            _ => status.ToString()
        };
    }
}