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
    // Retrieves user by email
    Task<User?> GetByEmailAsync(string email);

    // Retrieves active users only
    Task<List<User>> GetActiveUsersAsync();

    // Retrieves verified users
    Task<List<User>> GetVerifiedUsersAsync();

    // Retrieves users by role
    Task<List<User>> GetByRoleAsync(int roleId);

    // Searches users by name or email
    Task<List<User>> SearchAsync(string query);

    // Retrieves top sellers by rating
    Task<List<User>> GetTopSellersAsync(int limit = 10);

    // Retrieves users with listings in a location
    Task<List<User>> GetByLocationAsync(string city, string countryCode);

    // Checks if email exists
    Task<bool> EmailExistsAsync(string email);

    // Gets user by verification token
    Task<User?> GetByVerificationTokenAsync(string token);

    // Retrieves paginated users
    Task<(List<User> items, int total)> GetPagedAsync(int pageNumber, int pageSize);

    // Updates user last activity
    Task UpdateLastActivityAsync(Guid userId);
}
