#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Repositories;

/// <summary>
/// Generic repository interface for common CRUD operations.
/// </summary>
public interface IRepository<T> where T : class
{
    // Retrieves entity by ID
    Task<T?> GetByIdAsync(Guid id);

    // Retrieves all entities
    Task<List<T>> GetAllAsync();

    // Adds a new entity
    Task<T> AddAsync(T entity);

    // Updates an existing entity
    Task<T> UpdateAsync(T entity);

    // Deletes an entity
    Task DeleteAsync(Guid id);

    // Checks if entity with ID exists
    Task<bool> ExistsAsync(Guid id);

    // Gets total count of entities
    Task<int> CountAsync();
}
