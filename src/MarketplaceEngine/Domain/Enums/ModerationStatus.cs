// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.Enums;

/// <summary>
/// Represents the status of a moderation report or decision.
/// </summary>
public enum ModerationStatus
{
    /// <summary>Report is pending review.</summary>
    Pending = 1,

    /// <summary>Report is currently being reviewed.</summary>
    InReview = 2,

    /// <summary>Report has been approved/validated.</summary>
    Approved = 3,

    /// <summary>Report has been rejected/dismissed.</summary>
    Rejected = 4,

    /// <summary>Moderation resulted in content removal.</summary>
    ContentRemoved = 5,

    /// <summary>Moderation resulted in user suspension.</summary>
    UserSuspended = 6,

    /// <summary>Moderation resulted in user ban.</summary>
    UserBanned = 7
}
