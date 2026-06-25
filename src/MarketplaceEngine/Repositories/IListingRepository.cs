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
    /// <summary>
    /// Retrieves listings by seller ID.
    /// </summary>
    /// <param name="sellerId">The unique identifier of the seller.</param>
    /// <returns>A list of listings belonging to the specified seller.</returns>
    Task<List<Listing>> GetBySellerIdAsync(Guid sellerId);

    /// <summary>
    /// Retrieves listings by category ID.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the category.</param>
    /// <returns>A list of listings belonging to the specified category.</returns>
    Task<List<Listing>> GetByCategoryIdAsync(Guid categoryId);

    /// <summary>
    /// Retrieves listings by status.
    /// </summary>
    /// <param name="status">The listing status to filter by.</param>
    /// <returns>A list of listings with the specified status.</returns>
    Task<List<Listing>> GetByStatusAsync(ListingStatus status);

    /// <summary>
    /// Retrieves all active listings.
    /// </summary>
    /// <returns>A list of currently active listings.</returns>
    Task<List<Listing>> GetActiveListingsAsync();

    /// <summary>
    /// Retrieves featured listings.
    /// </summary>
    /// <param name="limit">The maximum number of listings to return.</param>
    /// <returns>A list of featured listings.</returns>
    Task<List<Listing>> GetFeaturedListingsAsync(int limit = 10);

    /// <summary>
    /// Retrieves recent listings created within a specified number of days.
    /// </summary>
    /// <param name="days">The number of days to look back.</param>
    /// <returns>A list of listings created within the specified timeframe.</returns>
    Task<List<Listing>> GetRecentListingsAsync(int days = 7);

    /// <summary>
    /// Searches listings by title and description.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <returns>A list of listings matching the search query.</returns>
    Task<List<Listing>> SearchAsync(string query);

    /// <summary>
    /// Retrieves listings by multiple tags.
    /// </summary>
    /// <param name="tags">The list of tags to filter by.</param>
    /// <returns>A list of listings containing any of the specified tags.</returns>
    Task<List<Listing>> GetByTagsAsync(List<string> tags);

    /// <summary>
    /// Retrieves listings within a specified location radius.
    /// </summary>
    /// <param name="latitude">The latitude of the center point.</param>
    /// <param name="longitude">The longitude of the center point.</param>
    /// <param name="radiusKm">The radius in kilometers.</param>
    /// <returns>A list of listings within the specified radius.</returns>
    Task<List<Listing>> GetNearbyAsync(double latitude, double longitude, double radiusKm);

    /// <summary>
    /// Retrieves paginated listings.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A tuple containing the list of items and the total count.</returns>
    Task<(List<Listing> items, int total)> GetPagedAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Increments the view count for a listing.
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing.</param>
    Task IncrementViewCountAsync(Guid listingId);

    /// <summary>
    /// Increments the interest count for a listing.
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing.</param>
    Task IncrementInterestCountAsync(Guid listingId);
}
