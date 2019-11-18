#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.Repositories;

/// <summary>
/// Repository interface for listing-specific operations.
/// </summary>
public interface IListingRepository : IRepository<Listing>
{
    // Retrieves listings by seller ID
    Task<List<Listing>> GetBySellerIdAsync(Guid sellerId);

    // Retrieves listings by category ID
    Task<List<Listing>> GetByCategoryIdAsync(Guid categoryId);

    // Retrieves listings by status
    Task<List<Listing>> GetByStatusAsync(ListingStatus status);

    // Retrieves active listings
    Task<List<Listing>> GetActiveListingsAsync();

    // Retrieves featured listings
    Task<List<Listing>> GetFeaturedListingsAsync(int limit = 10);

    // Retrieves recent listings
    Task<List<Listing>> GetRecentListingsAsync(int days = 7);

    // Searches listings by title and description
    Task<List<Listing>> SearchAsync(string query);

    // Retrieves listings by multiple tags
    Task<List<Listing>> GetByTagsAsync(List<string> tags);

    // Retrieves listings within location radius
    Task<List<Listing>> GetNearbyAsync(double latitude, double longitude, double radiusKm);

    // Retrieves paginated listings
    Task<(List<Listing> items, int total)> GetPagedAsync(int pageNumber, int pageSize);

    // Increments view count for a listing
    Task IncrementViewCountAsync(Guid listingId);

    // Increments interest count for a listing
    Task IncrementInterestCountAsync(Guid listingId);
}
