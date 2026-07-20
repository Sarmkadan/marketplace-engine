#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.Enums;

/// <summary>
/// Represents the current status of a marketplace listing.
/// </summary>
public enum ListingStatus
{
    /// <summary>Listing is active and visible in the marketplace.</summary>
    Active = 1,

    /// <summary>Listing is temporarily inactive.</summary>
    Inactive = 2,

    /// <summary>Listing is under moderation review.</summary>
    UnderReview = 3,

    /// <summary>Listing has been flagged as inappropriate.</summary>
    Flagged = 4,

    /// <summary>Listing has been sold or delisted.</summary>
    Delisted = 5,

    /// <summary>Listing is archived after being completed.</summary>
    Archived = 6,

    /// <summary>Listing is saved as a draft and not yet published.</summary>
    Draft = 7
}
