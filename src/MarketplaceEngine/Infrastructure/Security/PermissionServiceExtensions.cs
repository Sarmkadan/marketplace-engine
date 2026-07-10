using MarketplaceEngine.Domain.Enums;

namespace MarketplaceEngine.Infrastructure.Security;

public static class PermissionServiceExtensions
{
    /// <summary>
    /// Checks if a user has the Administrator role.
    /// </summary>
    public static bool IsAdministrator(this PermissionService permissionService, UserRole userRole)
    {
        ArgumentNullException.ThrowIfNull(permissionService);
        return userRole == UserRole.Administrator;
    }

    /// <summary>
    /// Checks if a user can edit any listing (regardless of ownership).
    /// </summary>
    public static bool CanEditAnyListing(this PermissionService permissionService, UserRole userRole)
    {
        ArgumentNullException.ThrowIfNull(permissionService);
        return permissionService.HasRole(userRole, UserRole.Administrator);
    }

    /// <summary>
    /// Checks if a user can moderate listings (delete or edit any listing).
    /// </summary>
    public static bool CanModerateListings(this PermissionService permissionService, UserRole userRole)
    {
        ArgumentNullException.ThrowIfNull(permissionService);
        return permissionService.CanModerate(userRole);
    }

    /// <summary>
    /// Checks if a user can perform actions on their own listings (edit/delete).
    /// </summary>
    public static bool CanManageOwnListings(this PermissionService permissionService, UserRole userRole, Guid listingSellerId, Guid userId)
    {
        ArgumentNullException.ThrowIfNull(permissionService);

        // Administrator can manage any listing
        if (permissionService.HasRole(userRole, UserRole.Administrator))
        {
            return true;
        }

        // Only seller can manage their own listings
        return (userRole == UserRole.User || userRole == UserRole.PremiumSeller) && listingSellerId == userId;
    }
}
