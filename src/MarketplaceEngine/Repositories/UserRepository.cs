#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Data;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Exceptions;

namespace MarketplaceEngine.Repositories;

/// <summary>
/// Repository for user persistence and retrieval operations.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly MarketplaceDbContext _context;
    private const string ResourceType = "User";

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    public UserRepository()
    {
        _context = MarketplaceDbContext.GetInstance();
    }

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The matching user, or <see langword="null"/> if no user is found.</returns>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        await Task.Delay(5);
        return _context.Users.FirstOrDefault(u => u.Id == id);
    }

    /// <summary>
    /// Retrieves all users.
    /// </summary>
    /// <returns>A list containing all users.</returns>
    public async Task<List<User>> GetAllAsync()
    {
        await Task.Delay(5);
        return _context.Users.ToList();
    }

    /// <summary>
    /// Adds a user to the repository.
    /// </summary>
    /// <param name="entity">The user to add.</param>
    /// <returns>The added user with its generated identifier and creation timestamp.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entity"/> is <see langword="null"/>.</exception>
    /// <exception cref="DuplicateResourceException">A user with the same email address already exists.</exception>
    public async Task<User> AddAsync(User entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        if (await EmailExistsAsync(entity.Email))
            throw new DuplicateResourceException(ResourceType, "email", entity.Email);

        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        _context.Users.Add(entity);

        await Task.Delay(5);
        return entity;
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="entity">The user containing the updated values.</param>
    /// <returns>The updated user.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entity"/> is <see langword="null"/>.</exception>
    /// <exception cref="ResourceNotFoundException">No user with the specified identifier exists.</exception>
    public async Task<User> UpdateAsync(User entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var existing = _context.Users.FirstOrDefault(u => u.Id == entity.Id);
        if (existing is null)
            throw new ResourceNotFoundException(ResourceType, entity.Id);

        entity.UpdatedAt = DateTime.UtcNow;
        var index = _context.Users.IndexOf(existing);
        _context.Users[index] = entity;

        await Task.Delay(5);
        return entity;
    }

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ResourceNotFoundException">No user with the specified identifier exists.</exception>
    public async Task DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user is null)
            throw new ResourceNotFoundException(ResourceType, id);

        _context.Users.Remove(user);
        await Task.Delay(5);
    }

    /// <summary>
    /// Determines whether a user with the specified identifier exists.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns><see langword="true"/> if the user exists; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> ExistsAsync(Guid id)
    {
        await Task.Delay(5);
        return _context.Users.Any(u => u.Id == id);
    }

    /// <summary>
    /// Gets the total number of users.
    /// </summary>
    /// <returns>The number of users in the repository.</returns>
    public async Task<int> CountAsync()
    {
        await Task.Delay(5);
        return _context.Users.Count;
    }

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The matching user, or <see langword="null"/> if the email is invalid or no user is found.</returns>
    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        await Task.Delay(5);
        return _context.Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Retrieves all active users.
    /// </summary>
    /// <returns>A list of active users.</returns>
    public async Task<List<User>> GetActiveUsersAsync()
    {
        await Task.Delay(5);
        return _context.Users.Where(u => u.IsActive).ToList();
    }

    /// <summary>
    /// Retrieves all verified users.
    /// </summary>
    /// <returns>A list of verified users.</returns>
    public async Task<List<User>> GetVerifiedUsersAsync()
    {
        await Task.Delay(5);
        return _context.Users.Where(u => u.IsVerified).ToList();
    }

    /// <summary>
    /// Retrieves users assigned to a specified role.
    /// </summary>
    /// <param name="roleId">The numeric role identifier.</param>
    /// <returns>A list of users assigned to the role.</returns>
    public async Task<List<User>> GetByRoleAsync(int roleId)
    {
        await Task.Delay(5);
        return _context.Users.Where(u => (int)u.Role == roleId).ToList();
    }

    /// <summary>
    /// Searches for users by full name or email address.
    /// </summary>
    /// <param name="query">The search text.</param>
    /// <returns>A list of matching users, or an empty list if the query is blank.</returns>
    public async Task<List<User>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<User>();

        await Task.Delay(5);
        var searchTerm = query.ToLowerInvariant();
        return _context.Users
            .Where(u => u.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Retrieves the highest-rated active and verified sellers.
    /// </summary>
    /// <param name="limit">The maximum number of sellers to return.</param>
    /// <returns>A list of top sellers ordered by rating and total sales.</returns>
    public async Task<List<User>> GetTopSellersAsync(int limit = 10)
    {
        await Task.Delay(5);
        return _context.Users
            .Where(u => u.IsActive && u.IsVerified && u.TotalSales > 0)
            .OrderByDescending(u => u.Rating is not null ? u.Rating.AverageRating : 0)
            .ThenByDescending(u => u.TotalSales)
            .Take(limit)
            .ToList();
    }

    /// <summary>
    /// Retrieves users in a specified city and country.
    /// </summary>
    /// <param name="city">The city to match.</param>
    /// <param name="countryCode">The country code to match.</param>
    /// <returns>A list of users in the specified location.</returns>
    public async Task<List<User>> GetByLocationAsync(string city, string countryCode)
    {
        await Task.Delay(5);
        return _context.Users
            .Where(u => u.Location is not null &&
                       u.Location.City.Equals(city, StringComparison.OrdinalIgnoreCase) &&
                       u.Location.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Determines whether a user has the specified email address.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <returns><see langword="true"/> if the email address exists; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> EmailExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        await Task.Delay(5);
        return _context.Users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Retrieves a user with a valid verification token.
    /// </summary>
    /// <param name="token">The verification token to search for.</param>
    /// <returns>The matching user, or <see langword="null"/> if the token is invalid, expired, or not found.</returns>
    public async Task<User?> GetByVerificationTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        await Task.Delay(5);
        return _context.Users.FirstOrDefault(u => u.VerificationToken == token &&
                                                   u.VerificationExpiry > DateTime.UtcNow);
    }

    /// <summary>
    /// Retrieves a page of users ordered by creation time in descending order.
    /// </summary>
    /// <param name="pageNumber">The one-based page number. Values below one are treated as one.</param>
    /// <param name="pageSize">The page size, constrained to a value from 1 through 100.</param>
    /// <returns>The users in the requested page and the total number of users.</returns>
    public async Task<(List<User> items, int total)> GetPagedAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        await Task.Delay(5);
        var allUsers = _context.Users.OrderByDescending(u => u.CreatedAt).ToList();
        var total = allUsers.Count;
        var items = allUsers.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return (items, total);
    }

    /// <summary>
    /// Updates the last activity timestamp for a user when the user exists.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task UpdateLastActivityAsync(Guid userId)
    {
        var user = await GetByIdAsync(userId);
        if (user is not null)
        {
            user.UpdateLastActivity();
            await UpdateAsync(user);
        }
    }
}
