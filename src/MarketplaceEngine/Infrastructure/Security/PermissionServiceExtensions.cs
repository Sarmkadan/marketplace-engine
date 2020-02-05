using MarketplaceEngine.Domain.Enums;

namespace MarketplaceEngine.Infrastructure.Security;

/// <summary>
/// Provides extension methods for <see cref="PermissionService"/> to simplify permission checks.
/// </summary>
public static class PermissionServiceExtensions
{
    /// <summary>
    /// Checks if a user has the Administrator role.
    /// </summary>
    /// <param name="permissionService">The permission service instance.</param>
    /// <param name="userRole">The user role to check.</param>
    /// <returns>True if the user is an Administrator; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="permissionService"/> is null.</exception>
    public static bool IsAdministrator(this PermissionService permissionService, UserRole userRole)
    {
        ArgumentNullException.ThrowIfNull(permissionService);
        ArgumentNullException.ThrowIfNull(userRole);

        return userRole == UserRole.Administrator;
    }

    /// <summary>
    /// Checks if a user can edit any listing (regardless of ownership).
    /// </summary>
    /// <param name="permissionService">The permission service instance.</param>
    /// <param name="userRole">The user role to check.</param>
    /// <returns>True if the user has Administrator role; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="permissionService"/> is null.</exception>
    public static bool CanEditAnyListing(this PermissionService permissionService, UserRole userRole)
    {
        ArgumentNullException.ThrowIfNull(permissionService);
        ArgumentNullException.ThrowIfNull(userRole);

        return permissionService.HasRole(userRole, UserRole.Administrator);
    }

    /// <summary>
    /// Checks if a user can moderate listings (delete or edit any listing).
    /// </summary>
    /// <param name="permissionService">The permission service instance.</param>
    /// <param name="userRole">The user role to check.</param>
    /// <returns>True if the user can moderate listings; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="permissionService"/> is null.</exception>
    public static bool CanModerateListings(this PermissionService permissionService, UserRole userRole)
    {
        ArgumentNullException.ThrowIfNull(permissionService);
        ArgumentNullException.ThrowIfNull(userRole);

        return permissionService.CanModerate(userRole);
    }

    /// <summary>
    /// Checks if a user can perform actions on their own listings (edit/delete).
    /// </summary>
    /// <param name="permissionService">The permission service instance.</param>
    /// <param name="userRole">The user role to check.</param>
    /// <param name="listingSellerId">The ID of the listing seller.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <returns>True if the user can manage the listing; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="permissionService"/> is null.</exception>
    public static bool CanManageOwnListings(this PermissionService permissionService, UserRole userRole, Guid listingSellerId, Guid userId)
    {
        ArgumentNullException.ThrowIfNull(permissionService);
        ArgumentNullException.ThrowIfNull(userRole);

        // Administrator can manage any listing
        if (permissionService.HasRole(userRole, UserRole.Administrator))
        {
            return true;
        }

        // Only seller (User or PremiumSeller) can manage their own listings
        return userRole is UserRole.User or UserRole.PremiumSeller && listingSellerId == userId;
    }
}