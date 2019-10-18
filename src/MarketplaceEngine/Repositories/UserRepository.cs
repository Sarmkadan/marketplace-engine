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

    public UserRepository()
    {
        _context = MarketplaceDbContext.GetInstance();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        await Task.Delay(5);
        return _context.Users.FirstOrDefault(u => u.Id == id);
    }

    public async Task<List<User>> GetAllAsync()
    {
        await Task.Delay(5);
        return _context.Users.ToList();
    }

    public async Task<User> AddAsync(User entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (await EmailExistsAsync(entity.Email))
            throw new DuplicateResourceException(ResourceType, "email", entity.Email);

        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        _context.Users.Add(entity);

        await Task.Delay(5);
        return entity;
    }

    public async Task<User> UpdateAsync(User entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var existing = _context.Users.FirstOrDefault(u => u.Id == entity.Id);
        if (existing == null)
            throw new ResourceNotFoundException(ResourceType, entity.Id);

        entity.UpdatedAt = DateTime.UtcNow;
        var index = _context.Users.IndexOf(existing);
        _context.Users[index] = entity;

        await Task.Delay(5);
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user == null)
            throw new ResourceNotFoundException(ResourceType, id);

        _context.Users.Remove(user);
        await Task.Delay(5);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        await Task.Delay(5);
        return _context.Users.Any(u => u.Id == id);
    }

    public async Task<int> CountAsync()
    {
        await Task.Delay(5);
        return _context.Users.Count;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        await Task.Delay(5);
        return _context.Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<User>> GetActiveUsersAsync()
    {
        await Task.Delay(5);
        return _context.Users.Where(u => u.IsActive).ToList();
    }

    public async Task<List<User>> GetVerifiedUsersAsync()
    {
        await Task.Delay(5);
        return _context.Users.Where(u => u.IsVerified).ToList();
    }

    public async Task<List<User>> GetByRoleAsync(int roleId)
    {
        await Task.Delay(5);
        return _context.Users.Where(u => (int)u.Role == roleId).ToList();
    }

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

    public async Task<List<User>> GetTopSellersAsync(int limit = 10)
    {
        await Task.Delay(5);
        return _context.Users
            .Where(u => u.IsActive && u.IsVerified && u.TotalSales > 0)
            .OrderByDescending(u => u.Rating != null ? u.Rating.AverageRating : 0)
            .ThenByDescending(u => u.TotalSales)
            .Take(limit)
            .ToList();
    }

    public async Task<List<User>> GetByLocationAsync(string city, string countryCode)
    {
        await Task.Delay(5);
        return _context.Users
            .Where(u => u.Location != null &&
                       u.Location.City.Equals(city, StringComparison.OrdinalIgnoreCase) &&
                       u.Location.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        await Task.Delay(5);
        return _context.Users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<User?> GetByVerificationTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        await Task.Delay(5);
        return _context.Users.FirstOrDefault(u => u.VerificationToken == token &&
                                                   u.VerificationExpiry > DateTime.UtcNow);
    }

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

    public async Task UpdateLastActivityAsync(Guid userId)
    {
        var user = await GetByIdAsync(userId);
        if (user != null)
        {
            user.UpdateLastActivity();
            await UpdateAsync(user);
        }
    }
}
