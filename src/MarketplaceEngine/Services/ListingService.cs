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

    // Creates and publishes a new listing
    public async Task<Listing> CreateListingAsync(Guid sellerId, string title, string description,
        decimal price, string currency, Guid categoryId, List<string> imageUrls)
    {
        var seller = await _userRepository.GetByIdAsync(sellerId).ConfigureAwait(false);
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

        var created = await _listingRepository.AddAsync(listing).ConfigureAwait(false);
        seller.TotalListings++;
        await _userRepository.UpdateAsync(seller).ConfigureAwait(false);

        return created;
    }

    // Updates an existing listing
    public async Task<Listing> UpdateListingAsync(Guid listingId, Guid requesterId, string? title = null,
        string? description = null, Money? price = null)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId).ConfigureAwait(false);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        if (listing.SellerId != requesterId)
            throw new UnauthorizedException(requesterId, "update this listing");

        if (!string.IsNullOrEmpty(title))
            listing.Title = title;

        if (!string.IsNullOrEmpty(description))
            listing.Description = description;

        if (price is not null)
            listing.Price = price;

        listing.ValidateForPublishing();
        return await _listingRepository.UpdateAsync(listing).ConfigureAwait(false);
    }

    // Publishes or unpublishes a listing
    public async Task<Listing> SetListingVisibilityAsync(Guid listingId, Guid requesterId, bool isVisible)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId).ConfigureAwait(false);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        if (listing.SellerId != requesterId)
            throw new UnauthorizedException(requesterId, "modify this listing's visibility");

        if (isVisible)
            listing.Publish();
        else
            listing.Unpublish();

        return await _listingRepository.UpdateAsync(listing).ConfigureAwait(false);
    }

    // Retrieves a listing and records the view
    public async Task<Listing> GetListingWithViewAsync(Guid listingId)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId).ConfigureAwait(false);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        await _listingRepository.IncrementViewCountAsync(listingId).ConfigureAwait(false);
        return listing;
    }

    // Records user interest in a listing
    public async Task<Listing> RecordInterestAsync(Guid listingId)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId).ConfigureAwait(false);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        await _listingRepository.IncrementInterestCountAsync(listingId).ConfigureAwait(false);
        return listing;
    }

    // Marks listing as sold/delisted
    public async Task<Listing> DelistListingAsync(Guid listingId, Guid requesterId)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId).ConfigureAwait(false);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        if (listing.SellerId != requesterId)
            throw new UnauthorizedException(requesterId, "delist this listing");

        listing.Delist();
        return await _listingRepository.UpdateAsync(listing).ConfigureAwait(false);
    }

    // Retrieves listings by seller
    public async Task<List<Listing>> GetSellerListingsAsync(Guid sellerId)
    {
        var seller = await _userRepository.GetByIdAsync(sellerId).ConfigureAwait(false);
        if (seller is null)
            throw new ResourceNotFoundException("User", sellerId);

        return await _listingRepository.GetBySellerIdAsync(sellerId).ConfigureAwait(false);
    }

    // Retrieves featured listings
    public async Task<List<Listing>> GetFeaturedListingsAsync(int limit = 10)
    {
        if (limit < 1 || limit > 100)
            limit = 10;

        return await _listingRepository.GetFeaturedListingsAsync(limit).ConfigureAwait(false);
    }

    // Retrieves recent listings
    public async Task<List<Listing>> GetRecentListingsAsync(int days = 7)
    {
        if (days < 1 || days > 365)
            days = 7;

        return await _listingRepository.GetRecentListingsAsync(days).ConfigureAwait(false);
    }

    // Gets paginated listings
    public async Task<(List<Listing> items, int total)> GetPaginatedListingsAsync(int pageNumber, int pageSize)
    {
        return await _listingRepository.GetPagedAsync(pageNumber, pageSize).ConfigureAwait(false);
    }

    // Marks listing as featured (admin only)
    public async Task<Listing> MarkAsFeaturedAsync(Guid listingId, Guid adminId)
    {
        var admin = await _userRepository.GetByIdAsync(adminId).ConfigureAwait(false);
        if (admin is null || admin.Role != UserRole.Administrator)
            throw new UnauthorizedException(adminId, "feature listings");

        var listing = await _listingRepository.GetByIdAsync(listingId).ConfigureAwait(false);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        listing.MarkAsFeatured();
        return await _listingRepository.UpdateAsync(listing).ConfigureAwait(false);
    }

    // Gets total listing count
    public async Task<int> GetTotalListingCountAsync()
    {
        return await _listingRepository.CountAsync().ConfigureAwait(false);
    }
}
