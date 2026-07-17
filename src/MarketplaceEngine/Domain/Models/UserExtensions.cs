using System;
using System.Globalization;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Extension methods for <see cref="User"/>.
/// </summary>
public static class UserExtensions
{
    /// <summary>
    /// Gets a display name for the user.
    /// </summary>
    /// <param name="user">The user instance.</param>
    /// <returns>
    /// The <see cref="User.FullName"/> if it is not <c>null</c> or whitespace; otherwise the <see cref="User.Email"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c>.</exception>
    public static string GetDisplayName(this User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName;
    }

    /// <summary>
    /// Determines whether the user's profile contains all required optional information.
    /// </summary>
    /// <param name="user">The user instance.</param>
    /// <returns>
    /// <c>true</c> if <see cref="User.Phone"/>, <see cref="User.ProfileImageUrl"/>, <see cref="User.Bio"/>
    /// and <see cref="User.Location"/> are all populated; otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c>.</exception>
    public static bool IsProfileComplete(this User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return !string.IsNullOrWhiteSpace(user.Phone)
            && !string.IsNullOrWhiteSpace(user.ProfileImageUrl)
            && !string.IsNullOrWhiteSpace(user.Bio)
            && user.Location is not null;
    }

    /// <summary>
    /// Calculates the number of whole days that have elapsed since the user was last active.
    /// </summary>
    /// <param name="user">The user instance.</param>
    /// <returns>
    /// The number of days since <see cref="User.LastActiveAt"/>, or <c>null</c> if the user has never been active.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c>.</exception>
    public static int? GetDaysSinceLastActive(this User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return user.LastActiveAt.HasValue
            ? (int)Math.Floor((DateTime.UtcNow - user.LastActiveAt.Value).TotalDays)
            : null;
    }

    /// <summary>
    /// Generates a concise, human‑readable summary of the user.
    /// </summary>
    /// <param name="user">The user instance.</param>
    /// <returns>A string containing key information about the user.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c>.</exception>
    public static string GetSummary(this User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var rating = user.Rating?.ToString() ?? "No rating";
        var location = user.Location?.ToString() ?? "No location";

        return string.Format(
            CultureInfo.InvariantCulture,
            "User {0} ({1}) – Role: {2}, Verified: {3}, Listings: {4}, Sales: {5}, Rating: {6}, Location: {7}",
            user.Id,
            user.GetDisplayName(),
            user.Role,
            user.IsVerified,
            user.TotalListings,
            user.TotalSales,
            rating,
            location);
    }
}
