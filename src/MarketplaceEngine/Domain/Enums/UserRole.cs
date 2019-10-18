// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.Enums;

/// <summary>
/// Defines user roles within the marketplace ecosystem.
/// </summary>
public enum UserRole
{
    /// <summary>Regular marketplace user (buyer/seller).</summary>
    User = 1,

    /// <summary>Marketplace administrator with moderation capabilities.</summary>
    Administrator = 2,

    /// <summary>Support staff member.</summary>
    Support = 3,

    /// <summary>Content moderator.</summary>
    Moderator = 4,

    /// <summary>Premium seller with special privileges.</summary>
    PremiumSeller = 5
}
