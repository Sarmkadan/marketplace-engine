#nullable enable

using Microsoft.AspNetCore.Mvc;
using MarketplaceEngine.DTOs;
using System.Security.Claims;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Extension methods for <see cref="UsersController"/> providing additional functionality
/// and convenience methods for common operations on user profiles.
/// </summary>
public static class UsersControllerExtensions
{
    /// <summary>
    /// Retrieves the current user's ID from the authenticated user principal.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <returns>The current user's ID if authenticated; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    public static Guid? GetCurrentUserId(this UsersController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (controller.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is not null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    /// <summary>
    /// Retrieves the current user's profile with caching.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <returns>The user profile if authenticated; otherwise, an unauthorized result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    public static async Task<IActionResult> GetCurrentUserProfile(this UsersController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var userId = controller.GetCurrentUserId();

        return userId is null
            ? new UnauthorizedResult()
            : await controller.GetUserProfile(userId.Value);
    }

    /// <summary>
    /// Retrieves the current user's seller metrics.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <returns>The seller metrics if authenticated; otherwise, an unauthorized result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    public static async Task<IActionResult> GetCurrentUserSellerMetrics(this UsersController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var userId = controller.GetCurrentUserId();

        return userId is null
            ? new UnauthorizedResult()
            : await controller.GetSellerMetrics(userId.Value);
    }

    /// <summary>
    /// Updates the current user's profile.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="request">The update request containing display name and other profile data.</param>
    /// <returns>The updated user profile if successful; otherwise, an error result.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="controller"/> is <see langword="null"/>.
    /// <para>-or-</para>
    /// <paramref name="request"/> is <see langword="null"/>.
    /// </exception>
    public static async Task<IActionResult> UpdateCurrentUserProfile(
        this UsersController controller,
        UpdateUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(request);

        var userId = controller.GetCurrentUserId();

        return userId is null
            ? new UnauthorizedResult()
            : string.IsNullOrWhiteSpace(request.DisplayName)
                ? new BadRequestObjectResult("Display name is required")
                : await controller.UpdateUserProfile(userId.Value, request);
    }
}