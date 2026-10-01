#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MarketplaceEngine.Services;

/// <summary>
/// Provides a unified catalog view over marketplace listings,
/// supporting filtering, category browsing, and featured product selection.
/// </summary>
public class ProductCatalog
{
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<ProductCatalog> _logger;

    public ProductCatalog(IListingRepository listingRepository)
        : this(listingRepository, NullLogger<ProductCatalog>.Instance)
    {
    }

    public ProductCatalog(
        IListingRepository listingRepository,
        ILogger<ProductCatalog> logger)
    {
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Returns all active listings in the catalog, optionally filtered by category.
    /// </summary>
    /// <param name="categoryId">Optional category filter.</param>
    /// <returns>Active listings matching the filter.</returns>
    public async Task<IReadOnlyList<Listing>> BrowseAsync(Guid? categoryId = null)
    {
        var listings = await _listingRepository.GetAllAsync();
        var query = listings.Where(l => l.Status == ListingStatus.Active);

        if (categoryId.HasValue)
            query = query.Where(l => l.CategoryId == categoryId.Value);

        var result = query.OrderByDescending(l => l.CreatedAt).ToList().AsReadOnly();
        _logger.LogDebug("Catalog browse returned {Count} listings (category: {CategoryId})",
            result.Count, categoryId?.ToString() ?? "all");

        return result;
    }

    /// <summary>
    /// Searches listings by title and description using case-insensitive substring matching.
    /// </summary>
    /// <param name="searchTerm">The text to search for.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <returns>Matching active listings ranked by relevance (title match first).</returns>
    public async Task<IReadOnlyList<Listing>> SearchAsync(string searchTerm, int maxResults = 50)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Array.Empty<Listing>().AsReadOnly();

        var listings = await _listingRepository.GetAllAsync();
        var term = searchTerm.Trim();

        var results = listings
            .Where(l => l.Status == ListingStatus.Active)
            .Where(l =>
                l.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                l.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                l.Tags.Any(t => t.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(l => l.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            .ThenByDescending(l => l.ViewCount)
            .Take(maxResults)
            .ToList()
            .AsReadOnly();

        _logger.LogDebug("Catalog search for '{Term}' returned {Count} results", term, results.Count);
        return results;
    }

    /// <summary>
    /// Returns the most viewed active listings as featured products.
    /// </summary>
    /// <param name="count">Number of featured products to return.</param>
    /// <returns>Top listings by view count.</returns>
    public async Task<IReadOnlyList<Listing>> GetFeaturedAsync(int count = 10)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");

        var listings = await _listingRepository.GetAllAsync();
        return listings
            .Where(l => l.Status == ListingStatus.Active)
            .OrderByDescending(l => l.ViewCount)
            .ThenByDescending(l => l.InterestCount)
            .Take(count)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Returns a single listing by ID, incrementing its view counter.
    /// </summary>
    /// <param name="listingId">The listing to retrieve.</param>
    /// <returns>The listing with updated view count, or null if not found.</returns>
    public async Task<Listing?> GetProductDetailAsync(Guid listingId)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is null)
            return null;

        listing.ViewCount++;
        await _listingRepository.UpdateAsync(listing);

        _logger.LogDebug("Product detail viewed: {ListingId} (views: {ViewCount})", listingId, listing.ViewCount);
        return listing;
    }

    /// <summary>
    /// Returns listings from a specific seller that are currently active.
    /// </summary>
    /// <param name="sellerId">The seller whose products to list.</param>
    /// <returns>Active listings belonging to the seller.</returns>
    public async Task<IReadOnlyList<Listing>> GetBySellerAsync(Guid sellerId)
    {
        var listings = await _listingRepository.GetAllAsync();
        return listings
            .Where(l => l.SellerId == sellerId && l.Status == ListingStatus.Active)
            .OrderByDescending(l => l.CreatedAt)
            .ToList()
            .AsReadOnly();
    }
}
