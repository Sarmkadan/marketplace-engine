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

    public ListingRepository()
    {
        _context = MarketplaceDbContext.GetInstance();
    }

    public async Task<Listing?> GetByIdAsync(Guid id)
    {
        await Task.Delay(5); // Simulate async operation
        lock (_lock)
        {
            return _context.Listings.FirstOrDefault(l => l.Id == id);
        }
    }

    public async Task<List<Listing>> GetAllAsync()
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Listings.ToList();
        }
    }

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

    public async Task<bool> ExistsAsync(Guid id)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Listings.Any(l => l.Id == id);
        }
    }

    public async Task<int> CountAsync()
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Listings.Count;
        }
    }

    public async Task<List<Listing>> GetBySellerIdAsync(Guid sellerId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Listings.Where(l => l.SellerId == sellerId).ToList();
        }
    }

    public async Task<List<Listing>> GetByCategoryIdAsync(Guid categoryId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Listings.Where(l => l.CategoryId == categoryId).ToList();
        }
    }

    public async Task<List<Listing>> GetByStatusAsync(ListingStatus status)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Listings.Where(l => l.Status == status).ToList();
        }
    }

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

    public async Task IncrementViewCountAsync(Guid listingId)
    {
        var listing = await GetByIdAsync(listingId);
        if (listing is not null)
        {
            listing.RecordView();
            await UpdateAsync(listing);
        }
    }

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
