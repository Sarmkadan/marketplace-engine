#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.Repositories;

/// <summary>
/// Repository interface for user-specific operations.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The user if found, otherwise null.</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Retrieves a list of active users.
    /// </summary>
    /// <returns>A list of currently active users.</returns>
    Task<List<User>> GetActiveUsersAsync();

    /// <summary>
    /// Retrieves a list of verified users.
    /// </summary>
    /// <returns>A list of verified users.</returns>
    Task<List<User>> GetVerifiedUsersAsync();

    /// <summary>
    /// Retrieves users filtered by their role.
    /// </summary>
    /// <param name="roleId">The role ID to filter by.</param>
    /// <returns>A list of users with the specified role.</returns>
    Task<List<User>> GetByRoleAsync(int roleId);

    /// <summary>
    /// Searches for users by name or email.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <returns>A list of users matching the query.</returns>
    Task<List<User>> SearchAsync(string query);

    /// <summary>
    /// Retrieves the top-rated sellers.
    /// </summary>
    /// <param name="limit">The maximum number of sellers to return.</param>
    /// <returns>A list of top sellers.</returns>
    Task<List<User>> GetTopSellersAsync(int limit = 10);

    /// <summary>
    /// Retrieves users with listings in a specific location.
    /// </summary>
    /// <param name="city">The city name.</param>
    /// <param name="countryCode">The country code.</param>
    /// <returns>A list of users in the specified location.</returns>
    Task<List<User>> GetByLocationAsync(string city, string countryCode);

    /// <summary>
    /// Checks if a user with the specified email exists.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <returns>True if the email exists, otherwise false.</returns>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Retrieves a user by their verification token.
    /// </summary>
    /// <param name="token">The verification token.</param>
    /// <returns>The user if found, otherwise null.</returns>
    Task<User?> GetByVerificationTokenAsync(string token);

    /// <summary>
    /// Retrieves a paginated list of users.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of users per page.</param>
    /// <returns>A tuple containing the list of users and the total count.</returns>
    Task<(List<User> items, int total)> GetPagedAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Updates the last activity timestamp for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    Task UpdateLastActivityAsync(Guid userId);
}
