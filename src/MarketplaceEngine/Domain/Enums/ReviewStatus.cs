#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.Enums;

/// <summary>
/// Represents the moderation state of a review.
/// </summary>
public enum ReviewStatus
{
    /// <summary>Review is visible and active.</summary>
    Active = 1,

    /// <summary>Review is under moderation review.</summary>
    UnderReview = 2,

    /// <summary>Review has been removed by a moderator.</summary>
    Removed = 3
}
