#nullable enable

using Microsoft.AspNetCore.Mvc;
using MarketplaceEngine.DTOs;
using System.Security.Claims;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Extension methods for UsersController providing additional functionality
/// and convenience methods for common operations.
/// </summary>
public static class UsersControllerExtensions
{
    /// <summary>
    /// Retrieves the current user's ID from the authenticated user principal.
    /// Returns null if user is not authenticated.
    /// </summary>
    public static Guid? GetCurrentUserId(this UsersController controller)
    {
        if (controller.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    /// <summary>
    /// Retrieves the current user's profile with caching.
    /// Returns null if user is not authenticated.
    /// </summary>
    public static async Task<IActionResult> GetCurrentUserProfile(this UsersController controller)
    {
        var userId = controller.GetCurrentUserId();

        if (userId == null)
        {
            return new UnauthorizedResult();
        }

        return await controller.GetUserProfile(userId.Value);
    }

    /// <summary>
    /// Retrieves the current user's seller metrics.
    /// Returns null if user is not authenticated.
    /// </summary>
    public static async Task<IActionResult> GetCurrentUserSellerMetrics(this UsersController controller)
    {
        var userId = controller.GetCurrentUserId();

        if (userId == null)
        {
            return new UnauthorizedResult();
        }

        return await controller.GetSellerMetrics(userId.Value);
    }

    /// <summary>
    /// Updates the current user's profile.
    /// Returns BadRequest if user is not authenticated or display name is invalid.
    /// </summary>
    public static async Task<IActionResult> UpdateCurrentUserProfile(
        this UsersController controller,
        UpdateUserRequest request)
    {
        var userId = controller.GetCurrentUserId();

        if (userId == null)
        {
            return new UnauthorizedResult();
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return new BadRequestObjectResult("Display name is required");
        }

        return await controller.UpdateUserProfile(userId.Value, request);
    }
}