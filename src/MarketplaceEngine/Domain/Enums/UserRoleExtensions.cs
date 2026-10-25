#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.Enums;

/// <summary>
/// Provides common role and display helpers for <see cref="UserRole"/> values.
/// </summary>
public static class UserRoleExtensions
{
    /// <summary>
    /// Determines whether the role is the marketplace administrator role.
    /// </summary>
    /// <param name="role">The role to evaluate.</param>
    /// <returns><see langword="true"/> when the role is <see cref="UserRole.Administrator"/>; otherwise, <see langword="false"/>.</returns>
    public static bool IsAdmin(this UserRole role)
    {
        return role == UserRole.Administrator;
    }

    /// <summary>
    /// Gets a human-readable display name for the role.
    /// </summary>
    /// <param name="role">The role to format.</param>
    /// <returns>The display name of the role.</returns>
    public static string ToDisplayName(this UserRole role)
    {
        return role switch
        {
            UserRole.User => "User",
            UserRole.Administrator => "Administrator",
            UserRole.Support => "Support",
            UserRole.Moderator => "Moderator",
            UserRole.PremiumSeller => "Premium Seller",
            _ => role.ToString()
        };
    }
}
