#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Moq;
using Xunit;

namespace MarketplaceEngine.Tests;

public class ListingServiceExtendedTests
{
    private readonly Mock<IListingRepository> _listingRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly ListingService _sut;

    public ListingServiceExtendedTests()
    {
        _listingRepoMock = new Mock<IListingRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _sut = new ListingService(_listingRepoMock.Object, _userRepoMock.Object);
    }

    private static User ActiveSeller(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Email = "seller@example.com",
        FullName = "Test Seller",
        IsActive = true,
        Role = UserRole.User
    };

    private static Listing ActiveListing(Guid? sellerId = null, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        SellerId = sellerId ?? Guid.NewGuid(),
        CategoryId = Guid.NewGuid(),
        Title = "Valid Listing Title",
        Description = "This is a valid description with enough characters.",
        Price = new Money(99m, "USD"),
        Status = ListingStatus.Active,
        ImageUrls = ["https://img.example.com/img.jpg"],
        PublishedAt = DateTime.UtcNow
    };

    // ── CreateListingAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateListingAsync_WithValidData_ReturnsCreatedListing()
    {
        var seller = ActiveSeller();
        var categoryId = Guid.NewGuid();
        var expected = ActiveListing(seller.Id);

        _userRepoMock.Setup(r => r.GetByIdAsync(seller.Id)).ReturnsAsync(seller);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(seller);
        _listingRepoMock.Setup(r => r.AddAsync(It.IsAny<Listing>())).ReturnsAsync(expected);

        var result = await _sut.CreateListingAsync(
            seller.Id,
            "Valid Listing Title",
            "This is a valid description with enough characters.",
            99m, "USD", categoryId,
            ["https://img.example.com/img.jpg"]);

        result.Should().NotBeNull();
        result.SellerId.Should().Be(seller.Id);
        _userRepoMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.TotalListings == 1)), Times.Once);
    }

    [Fact]
    public async Task CreateListingAsync_WithNegativePrice_ThrowsArgumentException()
    {
        var seller = ActiveSeller();
        _userRepoMock.Setup(r => r.GetByIdAsync(seller.Id)).ReturnsAsync(seller);

        var act = async () => await _sut.CreateListingAsync(
            seller.Id,
            "Valid Title Here",
            "This is a valid description with enough characters.",
            -1m, "USD", Guid.NewGuid(),
            ["https://img.example.com/img.jpg"]);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateListingAsync_WithShortTitle_ThrowsArgumentException()
    {
        var seller = ActiveSeller();
        _userRepoMock.Setup(r => r.GetByIdAsync(seller.Id)).ReturnsAsync(seller);

        var act = async () => await _sut.CreateListingAsync(
            seller.Id,
            "Hi",
            "This is a valid description with enough characters.",
            99m, "USD", Guid.NewGuid(),
            ["https://img.example.com/img.jpg"]);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Title*");
    }

    [Fact]
    public async Task CreateListingAsync_WithNoImages_ThrowsArgumentException()
    {
        var seller = ActiveSeller();
        _userRepoMock.Setup(r => r.GetByIdAsync(seller.Id)).ReturnsAsync(seller);

        var act = async () => await _sut.CreateListingAsync(
            seller.Id,
            "Valid Title Here",
            "This is a valid description with enough characters.",
            99m, "USD", Guid.NewGuid(),
            new List<string>());

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*image*");
    }

    // ── UpdateListingAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task UpdateListingAsync_WhenListingNotFound_ThrowsResourceNotFoundException()
    {
        var listingId = Guid.NewGuid();
        _listingRepoMock.Setup(r => r.GetByIdAsync(listingId)).ReturnsAsync((Listing?)null);

        var act = async () => await _sut.UpdateListingAsync(listingId, Guid.NewGuid(), title: "New Title Here");

        await act.Should().ThrowAsync<ResourceNotFoundException>().WithMessage("*Listing*not found*");
    }

    [Fact]
    public async Task UpdateListingAsync_WhenCallerIsNotSeller_ThrowsUnauthorizedException()
    {
        var listing = ActiveListing();
        _listingRepoMock.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);

        var differentUserId = Guid.NewGuid();
        var act = async () => await _sut.UpdateListingAsync(listing.Id, differentUserId, title: "New Title Here");

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task UpdateListingAsync_WithNewTitle_ReturnsUpdatedListing()
    {
        var listing = ActiveListing();
        _listingRepoMock.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);
        _listingRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Listing>())).ReturnsAsync((Listing l) => l);

        var (result, _) = await _sut.UpdateListingAsync(listing.Id, listing.SellerId, title: "Updated Listing Title");

        result.Title.Should().Be("Updated Listing Title");
    }

    [Fact]
    public async Task UpdateListingAsync_WithCategoryChange_ReturnsPreviousCategoryId()
    {
        var originalCategoryId = Guid.NewGuid();
        var listing = ActiveListing();
        listing.CategoryId = originalCategoryId;
        var newCategoryId = Guid.NewGuid();

        _listingRepoMock.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);
        _listingRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Listing>())).ReturnsAsync((Listing l) => l);

        var (_, previousCategoryId) = await _sut.UpdateListingAsync(
            listing.Id, listing.SellerId, categoryId: newCategoryId);

        previousCategoryId.Should().Be(originalCategoryId);
    }

    // ── SetListingVisibilityAsync ──────────────────────────────────────────

    [Fact]
    public async Task SetListingVisibilityAsync_WhenListingNotFound_ThrowsResourceNotFoundException()
    {
        var listingId = Guid.NewGuid();
        _listingRepoMock.Setup(r => r.GetByIdAsync(listingId)).ReturnsAsync((Listing?)null);

        var act = async () => await _sut.SetListingVisibilityAsync(listingId, Guid.NewGuid(), false);

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task SetListingVisibilityAsync_WhenCallerIsNotOwner_ThrowsUnauthorizedException()
    {
        var listing = ActiveListing();
        _listingRepoMock.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);

        var act = async () =>
            await _sut.SetListingVisibilityAsync(listing.Id, Guid.NewGuid(), false);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task SetListingVisibilityAsync_WhenUnpublishing_SetsStatusInactive()
    {
        var listing = ActiveListing();
        _listingRepoMock.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);
        _listingRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Listing>())).ReturnsAsync((Listing l) => l);

        var result = await _sut.SetListingVisibilityAsync(listing.Id, listing.SellerId, false);

        result.Status.Should().Be(ListingStatus.Inactive);
    }

    // ── GetListingWithViewAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetListingWithViewAsync_WhenListingNotFound_ThrowsResourceNotFoundException()
    {
        var listingId = Guid.NewGuid();
        _listingRepoMock.Setup(r => r.GetByIdAsync(listingId)).ReturnsAsync((Listing?)null);

        var act = async () => await _sut.GetListingWithViewAsync(listingId);

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task GetListingWithViewAsync_WhenListingExists_IncrementsViewCount()
    {
        var listing = ActiveListing();
        _listingRepoMock.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);
        _listingRepoMock.Setup(r => r.IncrementViewCountAsync(listing.Id)).Returns(Task.CompletedTask);

        var result = await _sut.GetListingWithViewAsync(listing.Id);

        result.Should().NotBeNull();
        _listingRepoMock.Verify(r => r.IncrementViewCountAsync(listing.Id), Times.Once);
    }

    // ── RecordInterestAsync ────────────────────────────────────────────────

    [Fact]
    public async Task RecordInterestAsync_WhenListingNotFound_ThrowsResourceNotFoundException()
    {
        var listingId = Guid.NewGuid();
        _listingRepoMock.Setup(r => r.GetByIdAsync(listingId)).ReturnsAsync((Listing?)null);

        var act = async () => await _sut.RecordInterestAsync(listingId);

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task RecordInterestAsync_WhenListingExists_IncrementsInterestCount()
    {
        var listing = ActiveListing();
        _listingRepoMock.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);
        _listingRepoMock.Setup(r => r.IncrementInterestCountAsync(listing.Id)).Returns(Task.CompletedTask);

        await _sut.RecordInterestAsync(listing.Id);

        _listingRepoMock.Verify(r => r.IncrementInterestCountAsync(listing.Id), Times.Once);
    }

    // ── GetSellerListingsAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetSellerListingsAsync_WhenSellerNotFound_ThrowsResourceNotFoundException()
    {
        var sellerId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(sellerId)).ReturnsAsync((User?)null);

        var act = async () => await _sut.GetSellerListingsAsync(sellerId);

        await act.Should().ThrowAsync<ResourceNotFoundException>().WithMessage("*User*not found*");
    }

    [Fact]
    public async Task GetSellerListingsAsync_WhenSellerExists_ReturnsListings()
    {
        var seller = ActiveSeller();
        var listings = new List<Listing>
        {
            ActiveListing(seller.Id),
            ActiveListing(seller.Id)
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(seller.Id)).ReturnsAsync(seller);
        _listingRepoMock.Setup(r => r.GetBySellerIdAsync(seller.Id)).ReturnsAsync(listings);

        var result = await _sut.GetSellerListingsAsync(seller.Id);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(l => l.SellerId.Should().Be(seller.Id));
    }

    // ── GetFeaturedListingsAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetFeaturedListingsAsync_WithNegativeLimit_ClampsToDefault()
    {
        var featured = new List<Listing> { ActiveListing() };
        _listingRepoMock.Setup(r => r.GetFeaturedListingsAsync(10)).ReturnsAsync(featured);

        var result = await _sut.GetFeaturedListingsAsync(-5);

        _listingRepoMock.Verify(r => r.GetFeaturedListingsAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetFeaturedListingsAsync_WithLimitAbove100_ClampsToDefault()
    {
        var featured = new List<Listing>();
        _listingRepoMock.Setup(r => r.GetFeaturedListingsAsync(10)).ReturnsAsync(featured);

        await _sut.GetFeaturedListingsAsync(200);

        _listingRepoMock.Verify(r => r.GetFeaturedListingsAsync(10), Times.Once);
    }

    // ── MarkAsFeaturedAsync ────────────────────────────────────────────────

    [Fact]
    public async Task MarkAsFeaturedAsync_WhenAdminAndListingExists_SetsFeaturedTrue()
    {
        var adminId = Guid.NewGuid();
        var admin = ActiveSeller(adminId);
        admin.Role = UserRole.Administrator;

        var listing = ActiveListing();

        _userRepoMock.Setup(r => r.GetByIdAsync(adminId)).ReturnsAsync(admin);
        _listingRepoMock.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);
        _listingRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Listing>())).ReturnsAsync((Listing l) => l);

        var result = await _sut.MarkAsFeaturedAsync(listing.Id, adminId);

        result.IsFeatured.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsFeaturedAsync_WhenListingNotFound_ThrowsResourceNotFoundException()
    {
        var adminId = Guid.NewGuid();
        var admin = ActiveSeller(adminId);
        admin.Role = UserRole.Administrator;
        var listingId = Guid.NewGuid();

        _userRepoMock.Setup(r => r.GetByIdAsync(adminId)).ReturnsAsync(admin);
        _listingRepoMock.Setup(r => r.GetByIdAsync(listingId)).ReturnsAsync((Listing?)null);

        var act = async () => await _sut.MarkAsFeaturedAsync(listingId, adminId);

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }
}
