#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Constants;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;

namespace MarketplaceEngine.Services;

/// <summary>
/// Service for searching marketplace listings and users.
/// </summary>
public class SearchService
{
    private readonly IListingRepository _listingRepository;
    private readonly IUserRepository _userRepository;

    public SearchService(IListingRepository listingRepository, IUserRepository userRepository)
    {
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    // Searches listings by keyword
    public async Task<List<Listing>> SearchListingsAsync(string query)
    {
        ValidateSearchQuery(query);
        return await _listingRepository.SearchAsync(query).ConfigureAwait(false);
    }

    // Searches listings by tags
    public async Task<List<Listing>> SearchByTagsAsync(List<string> tags)
    {
        if (tags is null || tags.Count == 0)
            throw new ValidationException("At least one tag is required for search");

        return await _listingRepository.GetByTagsAsync(tags).ConfigureAwait(false);
    }

    // Finds nearby listings
    public async Task<List<Listing>> FindNearbyListingsAsync(double latitude, double longitude, double radiusKm = 10)
    {
        if (latitude < -90 || latitude > 90)
            throw new ValidationException("Latitude", "Latitude must be between -90 and 90");

        if (longitude < -180 || longitude > 180)
            throw new ValidationException("Longitude", "Longitude must be between -180 and 180");

        if (radiusKm < 0.1 || radiusKm > 500)
            throw new ValidationException("Radius", "Search radius must be between 0.1 and 500 km");

        return await _listingRepository.GetNearbyAsync(latitude, longitude, radiusKm).ConfigureAwait(false);
    }

    // Searches users by name or email
    public async Task<List<User>> SearchUsersAsync(string query)
    {
        ValidateSearchQuery(query);
        return await _userRepository.SearchAsync(query).ConfigureAwait(false);
    }

    // Finds top sellers by rating
    public async Task<List<User>> GetTopSellersAsync(int limit = 10)
    {
        if (limit < 1 || limit > 50)
            limit = 10;

        return await _userRepository.GetTopSellersAsync(limit).ConfigureAwait(false);
    }

    /// <summary>
    /// Searches listings by category with pagination support.
    /// </summary>
    /// <param name="categoryId">The category to search within.</param>
    /// <param name="pageNumber">Page number (1-based). Values below 1 are clamped to 1.</param>
    /// <param name="pageSize">Items per page. Clamped to [1, MaxPageSize].</param>
    /// <returns>A tuple of the paged listing items and the total count before pagination.</returns>
    /// <exception cref="ValidationException">Thrown when <paramref name="categoryId"/> is empty.</exception>
    public async Task<(List<Listing> items, int total)> SearchByCategoryAsync(Guid categoryId, int pageNumber, int pageSize)
    {
        if (categoryId == Guid.Empty)
            throw new ValidationException("CategoryId", "Category ID cannot be empty");

        var listings = await _listingRepository.GetByCategoryIdAsync(categoryId).ConfigureAwait(false);

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = AppConstants.DefaultPageSize;
        if (pageSize > AppConstants.MaxPageSize) pageSize = AppConstants.MaxPageSize;

        var total = listings.Count;
        var items = listings
            .OrderByDescending(l => l.PublishedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (items, total);
    }

    // Advanced search with multiple filters
    public async Task<List<Listing>> AdvancedSearchAsync(string? keyword = null, Guid? categoryId = null,
        decimal? minPrice = null, decimal? maxPrice = null, List<string>? tags = null)
    {
        var allListings = await _listingRepository.GetActiveListingsAsync().ConfigureAwait(false);

        // Apply keyword filter
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var searchTerm = keyword.ToLowerInvariant();
            allListings = allListings
                .Where(l => l.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           l.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Apply category filter
        if (categoryId.HasValue && categoryId.Value != Guid.Empty)
        {
            allListings = allListings.Where(l => l.CategoryId == categoryId.Value).ToList();
        }

        // Apply price range filter
        if (minPrice.HasValue || maxPrice.HasValue)
        {
            allListings = allListings
                .Where(l => l.Price is not null &&
                           (!minPrice.HasValue || l.Price.Amount >= minPrice.Value) &&
                           (!maxPrice.HasValue || l.Price.Amount <= maxPrice.Value))
                .ToList();
        }

        // Apply tags filter
        if (tags is not null && tags.Count > 0)
        {
            var normalizedTags = tags.Select(t => t.ToLowerInvariant()).ToList();
            allListings = allListings
                .Where(l => l.Tags.Any(t => normalizedTags.Contains(t)))
                .ToList();
        }

        return allListings.OrderByDescending(l => l.ViewCount).ToList();
    }

    // Gets trending searches based on view count
    public async Task<List<Listing>> GetTrendingListingsAsync(int limit = 20)
    {
        if (limit < 1 || limit > 100)
            limit = 20;

        var allListings = await _listingRepository.GetActiveListingsAsync().ConfigureAwait(false);
        return allListings
            .OrderByDescending(l => l.ViewCount)
            .ThenByDescending(l => l.InterestCount)
            .Take(limit)
            .ToList();
    }

    // Autocomplete suggestions for search
    public async Task<List<string>> GetSearchSuggestionsAsync(string prefix, int limit = 10)
    {
        ValidateSearchQuery(prefix);

        var listings = await _listingRepository.GetActiveListingsAsync().ConfigureAwait(false);
        var suggestions = listings
            .Where(l => l.Title.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Title)
            .Distinct()
            .Take(limit)
            .ToList();

        return suggestions;
    }

    private void ValidateSearchQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ValidationException("Query", "Search query cannot be empty");

        if (query.Length < AppConstants.SearchMinQueryLength)
            throw new ValidationException("Query", $"Search query must be at least {AppConstants.SearchMinQueryLength} characters");

        if (query.Length > AppConstants.SearchMaxQueryLength)
            throw new ValidationException("Query", $"Search query cannot exceed {AppConstants.SearchMaxQueryLength} characters");
    }
}
