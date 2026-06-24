#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;

namespace MarketplaceEngine.Services;

/// <summary>
/// Service for managing marketplace listings.
/// </summary>
public class ListingService
{
    private readonly IListingRepository _listingRepository;
    private readonly IUserRepository _userRepository;

    public ListingService(IListingRepository listingRepository, IUserRepository userRepository)
    {
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Creates and publishes a new listing.
    /// </summary>
    /// <param name="sellerId">The ID of the user creating the listing.</param>
    /// <param name="title">Listing title.</param>
    /// <param name="description">Listing description.</param>
    /// <param name="price">Listing price.</param>
    /// <param name="currency">Listing currency code.</param>
    /// <param name="categoryId">The category ID for the listing.</param>
    /// <param name="imageUrls">List of image URLs.</param>
    /// <returns>The newly created listing.</returns>
    /// <exception cref="ResourceNotFoundException">Thrown if seller does not exist.</exception>
    /// <exception cref="UnauthorizedException">Thrown if seller is not active.</exception>
    public async Task<Listing> CreateListingAsync(Guid sellerId, string title, string description,
        decimal price, string currency, Guid categoryId, List<string> imageUrls)
    {
        var seller = await _userRepository.GetByIdAsync(sellerId);
        if (seller is null)
            throw new ResourceNotFoundException("User", sellerId);

        if (!seller.IsActive)
            throw new UnauthorizedException(sellerId, "create listings");

        var listing = new Listing
        {
            SellerId = sellerId,
            CategoryId = categoryId,
            Title = title,
            Description = description,
            Price = new Money(price, currency),
            ImageUrls = imageUrls,
            Status = ListingStatus.Active
        };

        listing.ValidateForPublishing();
        listing.Publish();

        var created = await _listingRepository.AddAsync(listing);
        seller.TotalListings++;
        await _userRepository.UpdateAsync(seller);

        return created;
    }

    /// <summary>
    /// Updates an existing listing.
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing.</param>
    /// <param name="requesterId">The ID of the user requesting the update.</param>
    /// <param name="title">Optional new title.</param>
    /// <param name="description">Optional new description.</param>
    /// <param name="price">Optional new price.</param>
    /// <param name="categoryId">Optional new category ID.</param>
    /// <returns>A tuple containing the updated listing and the previous category ID.</returns>
    /// <exception cref="ResourceNotFoundException">Thrown if listing does not exist.</exception>
    /// <exception cref="UnauthorizedException">Thrown if user is not the owner.</exception>
    public async Task<(Listing listing, Guid previousCategoryId)> UpdateListingAsync(Guid listingId, Guid requesterId,
        string? title = null, string? description = null, Money? price = null, Guid? categoryId = null)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        if (listing.SellerId != requesterId)
            throw new UnauthorizedException(requesterId, "update this listing");

        var previousCategoryId = listing.CategoryId;

        if (!string.IsNullOrEmpty(title))
            listing.Title = title;

        if (!string.IsNullOrEmpty(description))
            listing.Description = description;

        if (price is not null)
            listing.Price = price;

        if (categoryId.HasValue && categoryId.Value != Guid.Empty)
            listing.CategoryId = categoryId.Value;

        listing.ValidateForPublishing();
        var updated = await _listingRepository.UpdateAsync(listing);
        return (updated, previousCategoryId);
    }

    /// <summary>
    /// Sets the visibility (published status) of a listing.
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing.</param>
    /// <param name="requesterId">The ID of the user requesting the change.</param>
    /// <param name="isVisible">Whether the listing should be visible (published).</param>
    /// <returns>The updated listing.</returns>
    /// <exception cref="ResourceNotFoundException">Thrown if listing does not exist.</exception>
    /// <exception cref="UnauthorizedException">Thrown if user is not the owner.</exception>
    public async Task<Listing> SetListingVisibilityAsync(Guid listingId, Guid requesterId, bool isVisible)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        if (listing.SellerId != requesterId)
            throw new UnauthorizedException(requesterId, "modify this listing's visibility");

        if (isVisible)
            listing.Publish();
        else
            listing.Unpublish();

        return await _listingRepository.UpdateAsync(listing);
    }

    /// <summary>
    /// Retrieves a listing and records the view.
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing.</param>
    /// <returns>The retrieved listing.</returns>
    /// <exception cref="ResourceNotFoundException">Thrown if listing does not exist.</exception>
    public async Task<Listing> GetListingWithViewAsync(Guid listingId)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        await _listingRepository.IncrementViewCountAsync(listingId);
        return listing;
    }

    /// <summary>
    /// Records user interest in a listing.
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing.</param>
    /// <returns>The updated listing.</returns>
    /// <exception cref="ResourceNotFoundException">Thrown if listing does not exist.</exception>
    public async Task<Listing> RecordInterestAsync(Guid listingId)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        await _listingRepository.IncrementInterestCountAsync(listingId);
        return listing;
    }

    /// <summary>
    /// Marks a listing as sold or delisted.
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing.</param>
    /// <param name="requesterId">The ID of the user requesting the delist.</param>
    /// <returns>The updated listing.</returns>
    /// <exception cref="ResourceNotFoundException">Thrown if listing does not exist.</exception>
    /// <exception cref="UnauthorizedException">Thrown if user is not the owner.</exception>
    public async Task<Listing> DelistListingAsync(Guid listingId, Guid requesterId)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        if (listing.SellerId != requesterId)
            throw new UnauthorizedException(requesterId, "delist this listing");

        listing.Delist();
        return await _listingRepository.UpdateAsync(listing);
    }

    /// <summary>
    /// Retrieves all listings belonging to a specific seller.
    /// </summary>
    /// <param name="sellerId">The unique identifier of the seller.</param>
    /// <returns>A list of the seller's listings.</returns>
    /// <exception cref="ResourceNotFoundException">Thrown if seller does not exist.</exception>
    public async Task<List<Listing>> GetSellerListingsAsync(Guid sellerId)
    {
        var seller = await _userRepository.GetByIdAsync(sellerId);
        if (seller is null)
            throw new ResourceNotFoundException("User", sellerId);

        return await _listingRepository.GetBySellerIdAsync(sellerId);
    }

    /// <summary>
    /// Retrieves featured listings.
    /// </summary>
    /// <param name="limit">The maximum number of featured listings to return.</param>
    /// <returns>A list of featured listings.</returns>
    public async Task<List<Listing>> GetFeaturedListingsAsync(int limit = 10)
    {
        if (limit < 1 || limit > 100)
            limit = 10;

        return await _listingRepository.GetFeaturedListingsAsync(limit);
    }

    /// <summary>
    /// Retrieves recent listings.
    /// </summary>
    /// <param name="days">The number of days to look back for recent listings.</param>
    /// <returns>A list of recent listings.</returns>
    public async Task<List<Listing>> GetRecentListingsAsync(int days = 7)
    {
        if (days < 1 || days > 365)
            days = 7;

        return await _listingRepository.GetRecentListingsAsync(days);
    }

    /// <summary>
    /// Gets a paginated list of listings.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of listings per page.</param>
    /// <returns>A tuple containing the list of listings and the total count.</returns>
    public async Task<(List<Listing> items, int total)> GetPaginatedListingsAsync(int pageNumber, int pageSize)
    {
        return await _listingRepository.GetPagedAsync(pageNumber, pageSize);
    }

    /// <summary>
    /// Marks a listing as featured (Administrator only).
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing.</param>
    /// <param name="adminId">The ID of the administrator.</param>
    /// <returns>The updated listing.</returns>
    /// <exception cref="UnauthorizedException">Thrown if requester is not an administrator.</exception>
    /// <exception cref="ResourceNotFoundException">Thrown if listing does not exist.</exception>
    public async Task<Listing> MarkAsFeaturedAsync(Guid listingId, Guid adminId)
    {
        var admin = await _userRepository.GetByIdAsync(adminId);
        if (admin is null || admin.Role != UserRole.Administrator)
            throw new UnauthorizedException(adminId, "feature listings");

        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        listing.MarkAsFeatured();
        return await _listingRepository.UpdateAsync(listing);
    }

    /// <summary>
    /// Gets the total count of all listings.
    /// </summary>
    /// <returns>The total number of listings.</returns>
    public async Task<int> GetTotalListingCountAsync()
    {
        return await _listingRepository.CountAsync();
    }
}
