#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Data;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Exceptions;

namespace MarketplaceEngine.Repositories;

/// <summary>
/// Repository for listing persistence and retrieval operations.
/// </summary>
public class ListingRepository : IListingRepository
{
    private readonly MarketplaceDbContext _context;
    private const string ResourceType = "Listing";
    private static readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ListingRepository"/> class.
    /// </summary>
    public ListingRepository()
    {
        _context = MarketplaceDbContext.GetInstance();
    }

    /// <summary>
    /// Retrieves a listing by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the listing.</param>
    /// <returns>The matching listing, or <c>null</c> if no listing with the specified identifier exists.</returns>
    public async Task<Listing?> GetByIdAsync(Guid id)
    {
        await Task.Delay(5); // Simulate async operation
        lock (_lock)
        {
            return _context.Listings.FirstOrDefault(l => l.Id == id);
        }
    }

    /// <summary>
    /// Retrieves all listings.
    /// </summary>
    /// <returns>A list containing all listings.</returns>
    public async Task<List<Listing>> GetAllAsync()
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Listings.ToList();
        }
    }

    /// <summary>
    /// Adds a new listing to the repository.
    /// </summary>
    /// <param name="entity">The listing to add.</param>
    /// <returns>The added listing with its generated identifier and creation timestamp.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is <c>null</c>.</exception>
    public async Task<Listing> AddAsync(Listing entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        lock (_lock)
        {
            _context.Listings.Add(entity);
        }

        await Task.Delay(5);
        return entity;
    }

    /// <summary>
    /// Updates an existing listing in the repository.
    /// </summary>
    /// <param name="entity">The listing with updated values.</param>
    /// <returns>The updated listing.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is <c>null</c>.</exception>
    /// <exception cref="ResourceNotFoundException">Thrown when no listing with the specified identifier exists.</exception>
    public async Task<Listing> UpdateAsync(Listing entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        Listing? existing;
        lock (_lock)
        {
            existing = _context.Listings.FirstOrDefault(l => l.Id == entity.Id);
            if (existing is null)
                throw new ResourceNotFoundException(ResourceType, entity.Id);

            entity.UpdatedAt = DateTime.UtcNow;
            var index = _context.Listings.IndexOf(existing);
            _context.Listings[index] = entity; // Hotfix: Ensure atomic update of listing in shared collection
        }

        await Task.Delay(5);
        return entity;
    }

    /// <summary>
    /// Deletes a listing from the repository.
    /// </summary>
    /// <param name="id">The unique identifier of the listing to delete.</param>
    /// <exception cref="ResourceNotFoundException">Thrown when no listing with the specified identifier exists.</exception>
    public async Task DeleteAsync(Guid id)
    {
        Listing? listing;
        lock (_lock)
        {
            listing = _context.Listings.FirstOrDefault(l => l.Id == id);
            if (listing is null)
                throw new ResourceNotFoundException(ResourceType, id);

            _context.Listings.Remove(listing);
        }

        await Task.Delay(5);
    }

    /// <summary>
    /// Determines whether a listing with the specified identifier exists.
    /// </summary>
    /// <param name="id">The unique identifier to check.</param>
    /// <returns><c>true</c> if a listing with the specified identifier exists; otherwise, <c>false</c>.</returns>
    public async Task<bool> ExistsAsync(Guid id)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Listings.Any(l => l.Id == id);
        }
    }

    /// <summary>
    /// Gets the total number of listings in the repository.
    /// </summary>
    /// <returns>The total count of listings.</returns>
    public async Task<int> CountAsync()
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Listings.Count;
        }
    }

    /// <summary>
    /// Retrieves all listings belonging to the specified seller.
    /// </summary>
    /// <param name="sellerId">The unique identifier of the seller.</param>
    /// <returns>A list of listings belonging to the specified seller.</returns>
    public async Task<List<Listing>> GetBySellerIdAsync(Guid sellerId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Listings.Where(l => l.SellerId == sellerId).ToList();
        }
    }

    /// <summary>
    /// Retrieves all listings in the specified category.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the category.</param>
    /// <returns>A list of listings in the specified category.</returns>
    public async Task<List<Listing>> GetByCategoryIdAsync(Guid categoryId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Listings.Where(l => l.CategoryId == categoryId).ToList();
        }
    }

    /// <summary>
    /// Retrieves all listings with the specified status.
    /// </summary>
    /// <param name="status">The listing status to filter by.</param>
    /// <returns>A list of listings with the specified status.</returns>
    public async Task<List<Listing>> GetByStatusAsync(ListingStatus status)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Listings.Where(l => l.Status == status).ToList();
        }
    }

    /// <summary>
    /// Retrieves all active listings that have been published, ordered by most recently published first.
    /// </summary>
    /// <returns>A list of active, published listings.</returns>
    public async Task<List<Listing>> GetActiveListingsAsync()
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Listings
                .Where(l => l.Status == ListingStatus.Active && l.PublishedAt.HasValue)
                .OrderByDescending(l => l.PublishedAt)
                .ToList();
        }
    }

    /// <summary>
    /// Retrieves a limited number of featured, active listings ordered by view count.
    /// </summary>
    /// <param name="limit">The maximum number of listings to return. Defaults to 10.</param>
    /// <returns>A list of featured, active listings.</returns>
    public async Task<List<Listing>> GetFeaturedListingsAsync(int limit = 10)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Listings
                .Where(l => l.IsFeatured && l.Status == ListingStatus.Active)
                .OrderByDescending(l => l.ViewCount)
                .Take(limit)
                .ToList();
        }
    }

    /// <summary>
    /// Retrieves active listings published within the specified number of days, ordered by most recently published first.
    /// </summary>
    /// <param name="days">The number of days to look back. Defaults to 7.</param>
    /// <returns>A list of recent, active listings.</returns>
    public async Task<List<Listing>> GetRecentListingsAsync(int days = 7)
    {
        await Task.Delay(5);
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        lock (_lock)
        {
            return _context.Listings
                .Where(l => l.PublishedAt >= cutoffDate && l.Status == ListingStatus.Active)
                .OrderByDescending(l => l.PublishedAt)
                .ToList();
        }
    }

    /// <summary>
    /// Searches for active listings matching the specified query in title or description.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <returns>A list of active listings matching the search query.</returns>
    public async Task<List<Listing>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<Listing>();

        await Task.Delay(5);
        var searchTerm = query.ToLowerInvariant();
        lock (_lock)
        {
            return _context.Listings
                .Where(l => l.Status == ListingStatus.Active &&
                           (l.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            l.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(l => l.ViewCount)
                .ToList();
        }
    }

    /// <summary>
    /// Retrieves active listings that contain any of the specified tags.
    /// </summary>
    /// <param name="tags">A list of tags to search for.</param>
    /// <returns>A list of active listings containing any of the specified tags.</returns>
    public async Task<List<Listing>> GetByTagsAsync(List<string> tags)
    {
        if (tags is null || tags.Count == 0)
            return new List<Listing>();

        await Task.Delay(5);
        var normalizedTags = tags.Select(t => t.ToLowerInvariant()).ToList();
        lock (_lock)
        {
            return _context.Listings
                .Where(l => l.Tags.Any(t => normalizedTags.Contains(t)) && l.Status == ListingStatus.Active)
                .ToList();
        }
    }

    /// <summary>
    /// Retrieves active listings within the specified geographic radius.
    /// </summary>
    /// <param name="latitude">The latitude of the center point.</param>
    /// <param name="longitude">The longitude of the center point.</param>
    /// <param name="radiusKm">The radius in kilometers from the center point.</param>
    /// <returns>A list of active listings within the specified radius.</returns>
    public async Task<List<Listing>> GetNearbyAsync(double latitude, double longitude, double radiusKm)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Listings
                .Where(l => l.Location is not null &&
                           l.Status == ListingStatus.Active &&
                           l.Location.DistanceTo(new Domain.ValueObjects.Location(
                               "Temp", "Temp", "US", null, latitude, longitude)) <= radiusKm)
                .ToList();
        }
    }

    /// <summary>
    /// Retrieves a paged list of active listings.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A tuple containing the list of listings for the specified page and the total count.</returns>
    public async Task<(List<Listing> items, int total)> GetPagedAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        await Task.Delay(5);
        List<Listing> allListings;
        lock (_lock)
        {
            allListings = _context.Listings
                .Where(l => l.Status == ListingStatus.Active)
                .OrderByDescending(l => l.PublishedAt)
                .ToList();
        }

        var total = allListings.Count;
        var items = allListings
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (items, total);
    }

    /// <summary>
    /// Increments the view count of the specified listing.
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing.</param>
    public async Task IncrementViewCountAsync(Guid listingId)
    {
        var listing = await GetByIdAsync(listingId);
        if (listing is not null)
        {
            listing.RecordView();
            await UpdateAsync(listing);
        }
    }

    /// <summary>
    /// Increments the interest count of the specified listing.
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing.</param>
    public async Task IncrementInterestCountAsync(Guid listingId)
    {
        var listing = await GetByIdAsync(listingId);
        if (listing is not null)
        {
            listing.RecordInterest();
            await UpdateAsync(listing);
        }
    }
}
