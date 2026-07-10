#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using Moq;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Extension methods for <see cref="ListingServiceExtendedTests"/> to provide additional test utilities
/// and helper methods for common test scenarios.
/// </summary>
public static class ListingServiceExtendedTestsExtensions
{
    /// <summary>
    /// Creates a mock setup for CreateListingAsync that returns a valid listing.
    /// </summary>
    /// <param name="mock">The mock repository to setup</param>
    /// <param name="sellerId">The seller ID</param>
    /// <param name="listingId">Optional listing ID</param>
    /// <returns>The configured mock</returns>
    public static Mock<IListingRepository> SetupCreateListingAsync(
        this Mock<IListingRepository> mock,
        Guid sellerId,
        Guid? listingId = null)
    {
        var listing = new Listing
        {
            Id = listingId ?? Guid.NewGuid(),
            SellerId = sellerId,
            CategoryId = Guid.NewGuid(),
            Title = "Test Listing",
            Description = "Test description with sufficient length",
            Price = new Money(99.99m, "USD"),
            Status = Domain.Enums.ListingStatus.Active,
            ImageUrls = ["https://example.com/image.jpg"],
            PublishedAt = DateTime.UtcNow
        };

        mock.Setup(r => r.AddAsync(It.IsAny<Listing>()))
            .ReturnsAsync(listing);

        return mock;
    }

    /// <summary>
    /// Creates a mock setup for UpdateListingAsync that returns an updated listing.
    /// </summary>
    /// <param name="mock">The mock repository to setup</param>
    /// <param name="listingId">The listing ID to update</param>
    /// <param name="sellerId">The seller ID</param>
    /// <param name="title">Optional new title</param>
    /// <param name="price">Optional new price</param>
    /// <returns>The configured mock</returns>
    public static Mock<IListingRepository> SetupUpdateListingAsync(
        this Mock<IListingRepository> mock,
        Guid listingId,
        Guid sellerId,
        string? title = null,
        decimal? price = null)
    {
        var listing = new Listing
        {
            Id = listingId,
            SellerId = sellerId,
            CategoryId = Guid.NewGuid(),
            Title = title ?? "Original Title",
            Description = "Test description",
            Price = new Money(price ?? 99.99m, "USD"),
            Status = Domain.Enums.ListingStatus.Active,
            ImageUrls = ["https://example.com/image.jpg"],
            PublishedAt = DateTime.UtcNow
        };

        mock.Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        mock.Setup(r => r.UpdateAsync(It.IsAny<Listing>()))
            .ReturnsAsync((Listing l) => l);

        return mock;
    }

    /// <summary>
    /// Creates a mock setup for GetByIdAsync that returns a listing with the specified status.
    /// </summary>
    /// <param name="mock">The mock repository to setup</param>
    /// <param name="listingId">The listing ID</param>
    /// <param name="status">The listing status to return</param>
    /// <returns>The configured mock</returns>
    public static Mock<IListingRepository> SetupGetListingWithStatusAsync(
        this Mock<IListingRepository> mock,
        Guid listingId,
        Domain.Enums.ListingStatus status)
    {
        var listing = new Listing
        {
            Id = listingId,
            SellerId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Title = "Test Listing",
            Description = "Test description",
            Price = new Money(99.99m, "USD"),
            Status = status,
            ImageUrls = ["https://example.com/image.jpg"],
            PublishedAt = DateTime.UtcNow
        };

        mock.Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        return mock;
    }

    /// <summary>
    /// Creates a mock setup for GetFeaturedListingsAsync that returns a collection of listings.
    /// </summary>
    /// <param name="mock">The mock repository to setup</param>
    /// <param name="listings">The listings to return</param>
    /// <param name="limit">The limit parameter</param>
    /// <returns>The configured mock</returns>
    public static Mock<IListingRepository> SetupGetFeaturedListingsAsync(
        this Mock<IListingRepository> mock,
        List<Listing> listings,
        int limit = 10)
    {
        mock.Setup(r => r.GetFeaturedListingsAsync(limit))
            .ReturnsAsync(listings);

        return mock;
    }

    /// <summary>
    /// Creates a mock setup for GetBySellerIdAsync that returns listings for a seller.
    /// </summary>
    /// <param name="mock">The mock repository to setup</param>
    /// <param name="sellerId">The seller ID</param>
    /// <param name="listings">The listings to return</param>
    /// <returns>The configured mock</returns>
    public static Mock<IListingRepository> SetupGetSellerListingsAsync(
        this Mock<IListingRepository> mock,
        Guid sellerId,
        List<Listing> listings)
    {
        mock.Setup(r => r.GetBySellerIdAsync(sellerId))
            .ReturnsAsync(listings);

        return mock;
    }

    /// <summary>
    /// Creates a mock setup for IncrementViewCountAsync that tracks view count increments.
    /// </summary>
    /// <param name="mock">The mock repository to setup</param>
    /// <param name="listingId">The listing ID to track</param>
    /// <param name="initialViews">Optional initial view count</param>
    /// <returns>The configured mock</returns>
    public static Mock<IListingRepository> SetupIncrementViewCountAsync(
        this Mock<IListingRepository> mock,
        Guid listingId,
        int initialViews = 0)
    {
        var listing = new Listing
        {
            Id = listingId,
            SellerId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Title = "Test Listing",
            Description = "Test description",
            Price = new Money(99.99m, "USD"),
            Status = Domain.Enums.ListingStatus.Active,
            ImageUrls = ["https://example.com/image.jpg"],
            PublishedAt = DateTime.UtcNow,
            ViewCount = initialViews
        };

        mock.Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        mock.Setup(r => r.IncrementViewCountAsync(listingId))
            .Returns(Task.CompletedTask)
            .Callback(() => listing.ViewCount++);

        return mock;
    }

    /// <summary>
    /// Creates a mock setup for IncrementInterestCountAsync that tracks interest count increments.
    /// </summary>
    /// <param name="mock">The mock repository to setup</param>
    /// <param name="listingId">The listing ID to track</param>
    /// <param name="initialInterests">Optional initial interest count</param>
    /// <returns>The configured mock</returns>
    public static Mock<IListingRepository> SetupIncrementInterestCountAsync(
        this Mock<IListingRepository> mock,
        Guid listingId,
        int initialInterests = 0)
    {
        var listing = new Listing
        {
            Id = listingId,
            SellerId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Title = "Test Listing",
            Description = "Test description",
            Price = new Money(99.99m, "USD"),
            Status = Domain.Enums.ListingStatus.Active,
            ImageUrls = ["https://example.com/image.jpg"],
            PublishedAt = DateTime.UtcNow,
            InterestCount = initialInterests
        };

        mock.Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        mock.Setup(r => r.IncrementInterestCountAsync(listingId))
            .Returns(Task.CompletedTask)
            .Callback(() => listing.InterestCount++);

        return mock;
    }

    /// <summary>
    /// Creates a mock setup for SetListingVisibilityAsync that tracks visibility changes.
    /// </summary>
    /// <param name="mock">The mock repository to setup</param>
    /// <param name="listingId">The listing ID</param>
    /// <param name="isPublished">Whether the listing should be published</param>
    /// <returns>The configured mock</returns>
    public static Mock<IListingRepository> SetupSetListingVisibilityAsync(
        this Mock<IListingRepository> mock,
        Guid listingId,
        bool isPublished)
    {
        var listing = new Listing
        {
            Id = listingId,
            SellerId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Title = "Test Listing",
            Description = "Test description",
            Price = new Money(99.99m, "USD"),
            Status = isPublished
                ? Domain.Enums.ListingStatus.Active
                : Domain.Enums.ListingStatus.Inactive,
            ImageUrls = ["https://example.com/image.jpg"],
            PublishedAt = isPublished ? DateTime.UtcNow : null
        };

        mock.Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        mock.Setup(r => r.UpdateAsync(It.IsAny<Listing>()))
            .ReturnsAsync((Listing l) => l);

        return mock;
    }

    /// <summary>
    /// Creates an active seller user for testing.
    /// </summary>
    /// <param name="id">Optional seller ID</param>
    /// <returns>A configured user</returns>
    public static User CreateActiveSeller(this User? _, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Email = "seller@example.com",
        FullName = "Test Seller",
        IsActive = true,
        Role = Domain.Enums.UserRole.User,
        TotalListings = 0
    };

    /// <summary>
    /// Creates an active listing for testing.
    /// </summary>
    /// <param name="sellerId">Optional seller ID</param>
    /// <param name="listingId">Optional listing ID</param>
    /// <param name="price">Optional price</param>
    /// <returns>A configured listing</returns>
    public static Listing CreateActiveListing(
        this Listing? _,
        Guid? sellerId = null,
        Guid? listingId = null,
        decimal? price = null) => new()
    {
        Id = listingId ?? Guid.NewGuid(),
        SellerId = sellerId ?? Guid.NewGuid(),
        CategoryId = Guid.NewGuid(),
        Title = "Valid Listing Title",
        Description = "This is a valid description with enough characters to pass validation.",
        Price = new Money(price ?? 99.99m, "USD"),
        Status = Domain.Enums.ListingStatus.Active,
        ImageUrls = ["https://example.com/image.jpg"],
        PublishedAt = DateTime.UtcNow,
        ViewCount = 0,
        InterestCount = 0
    };

    /// <summary>
    /// Creates an administrator user for testing.
    /// </summary>
    /// <param name="id">Optional admin ID</param>
    /// <returns>A configured administrator user</returns>
    public static User CreateAdminUser(this User? _, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Email = "admin@example.com",
        FullName = "Test Administrator",
        IsActive = true,
        Role = Domain.Enums.UserRole.Administrator,
        TotalListings = 0
    };
}