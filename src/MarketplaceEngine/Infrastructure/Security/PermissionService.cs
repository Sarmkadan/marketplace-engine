#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;

namespace MarketplaceEngine.Infrastructure.Security;

/// <summary>
/// Service for checking user permissions and roles.
/// Implements role-based access control (RBAC).
/// </summary>
public class PermissionService
{
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(ILogger<PermissionService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Checks if a user has a specific role.
    /// </summary>
    public bool HasRole(UserRole userRole, UserRole requiredRole)
    {
        // Admin has all permissions
        if (userRole == UserRole.Admin)
            return true;

        return userRole == requiredRole;
    }

    /// <summary>
    /// Checks if a user can perform an action on a listing.
    /// </summary>
    public bool CanEditListing(UserRole userRole, Guid listingSellerId, Guid userId)
    {
        // Admin can edit any listing
        if (userRole == UserRole.Admin)
            return true;

        // Only seller can edit their own listings
        if (userRole == UserRole.Seller && listingSellerId == userId)
            return true;

        _logger.LogWarning("User {UserId} denied edit permission for listing {ListingId}", userId, listingSellerId);
        return false;
    }

    /// <summary>
    /// Checks if a user can delete a listing.
    /// </summary>
    public bool CanDeleteListing(UserRole userRole, Guid listingSellerId, Guid userId)
    {
        // Admin can delete any listing
        if (userRole == UserRole.Admin)
            return true;

        // Only seller can delete their own listings
        if (userRole == UserRole.Seller && listingSellerId == userId)
            return true;

        return false;
    }

    /// <summary>
    /// Checks if a user can access moderation features.
    /// </summary>
    public bool CanModerate(UserRole userRole)
    {
        return userRole == UserRole.Admin || userRole == UserRole.Moderator;
    }

    /// <summary>
    /// Checks if a user can create listings.
    /// </summary>
    public bool CanCreateListing(UserRole userRole)
    {
        return userRole == UserRole.Seller || userRole == UserRole.Admin;
    }

    /// <summary>
    /// Checks if a user can message another user.
    /// </summary>
    public bool CanMessage(UserRole userRole, Guid recipientId, Guid userId)
    {
        // Can't message yourself
        if (recipientId == userId)
        {
            _logger.LogWarning("User {UserId} attempted to message themselves", userId);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a user can submit a moderation report.
    /// </summary>
    public bool CanSubmitReport(UserRole userRole)
    {
        // All users can report, but only if not spam-flagged
        // In production, check user's spam flag/trust score
        return true;
    }
}
