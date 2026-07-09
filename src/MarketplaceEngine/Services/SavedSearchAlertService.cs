namespace MarketplaceEngine.Services;

using System.Collections.Concurrent;
using System.Linq;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Repositories;

/// <summary>A saved search subscription owned by a user.</summary>
public sealed record SavedSearchCriteria
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid UserId { get; init; }
    public string? Keywords { get; init; }
    public Guid? CategoryId { get; init; }
    public decimal? MaxPrice { get; init; }
    public List<string> Tags { get; init; } = [];
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>In-memory saved-search registry that matches new listings against user subscriptions.</summary>
public sealed class SavedSearchAlertService
{
    private readonly IListingRepository _listingRepository;
    private readonly ConcurrentDictionary<Guid, SavedSearchCriteria> _searches = new();

    public SavedSearchAlertService(IListingRepository listingRepository)
    {
        _listingRepository = listingRepository;
    }

    /// <summary>Registers a saved search. Throws ArgumentException if no criterion is set (all filters null/empty).</summary>
    public SavedSearchCriteria Save(SavedSearchCriteria criteria)
    {
        // Validate that at least one criterion is set
        bool hasKeyword = !string.IsNullOrWhiteSpace(criteria.Keywords);
        bool hasCategory = criteria.CategoryId.HasValue;
        bool hasMaxPrice = criteria.MaxPrice.HasValue;
        bool hasTags = criteria.Tags != null && criteria.Tags.Any(t => !string.IsNullOrWhiteSpace(t));

        if (!hasKeyword && !hasCategory && !hasMaxPrice && !hasTags)
        {
            throw new ArgumentException("At least one search criterion must be specified.", nameof(criteria));
        }

        _searches[criteria.Id] = criteria;
        return criteria;
    }

    /// <summary>Removes a saved search; returns false if not found or owned by another user.</summary>
    public bool Remove(Guid searchId, Guid userId)
    {
        if (_searches.TryGetValue(searchId, out var existing))
        {
            if (existing.UserId != userId)
            {
                return false;
            }

            return _searches.TryRemove(searchId, out _);
        }

        return false;
    }

    /// <summary>All saved searches belonging to a user, newest first.</summary>
    public IReadOnlyList<SavedSearchCriteria> GetForUser(Guid userId)
    {
        return _searches.Values
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
    }

    /// <summary>Returns true if the listing satisfies every non-null criterion (keywords: case-insensitive substring of Title or Description; tags: any overlap).</summary>
    public bool Matches(SavedSearchCriteria criteria, Listing listing)
    {
        // Keywords
        if (!string.IsNullOrWhiteSpace(criteria.Keywords))
        {
            var keyword = criteria.Keywords!;
            if (!listing.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
                !listing.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // Category
        if (criteria.CategoryId.HasValue && listing.CategoryId != criteria.CategoryId.Value)
        {
            return false;
        }

        // MaxPrice
        if (criteria.MaxPrice.HasValue)
        {
            if (listing.Price == null || listing.Price.Amount > criteria.MaxPrice.Value)
            {
                return false;
            }
        }

        // Tags
        if (criteria.Tags != null && criteria.Tags.Any())
        {
            var tagMatch = criteria.Tags.Any(t =>
                listing.Tags.Contains(t, StringComparer.OrdinalIgnoreCase));
            if (!tagMatch)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Distinct user ids whose saved searches match this listing.</summary>
    public IReadOnlyList<Guid> FindUsersToNotify(Listing listing)
    {
        return _searches.Values
            .Where(c => Matches(c, listing))
            .Select(c => c.UserId)
            .Distinct()
            .ToList();
    }

    /// <summary>Runs a saved search against current active listings via the repository.</summary>
    public async Task<IReadOnlyList<Listing>> ExecuteAsync(Guid searchId)
    {
        if (!_searches.TryGetValue(searchId, out var criteria))
        {
            return [];
        }

        var activeListings = await _listingRepository.GetActiveListingsAsync();
        var matched = activeListings
            .Where(l => Matches(criteria, l))
            .ToList();

        return matched;
    }
}
