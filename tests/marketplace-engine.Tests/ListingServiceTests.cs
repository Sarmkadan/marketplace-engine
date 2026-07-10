#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Moq;
using Xunit;

/// <summary>
/// Tests for the ListingService class.
/// </summary>
public class ListingServiceTests
{
    /// <summary>
    /// Mock objects for the ListingRepository and UserRepository.
    /// </summary>
    private readonly Mock<IListingRepository> _listingRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly ListingService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListingServiceTests"/> class.
    /// </summary>
    public ListingServiceTests()
    {
        _listingRepoMock = new Mock<IListingRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _sut = new ListingService(_listingRepoMock.Object, _userRepoMock.Object);
    }

    /// <summary>
    /// Tests that CreateListingAsync throws a ResourceNotFoundException when the seller is not found.
    /// </summary>
    [Fact]
    public async Task CreateListingAsync_WhenSellerNotFound_ThrowsResourceNotFoundException()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _sut.CreateListingAsync(
            sellerId,
            "Quality Acoustic Guitar",
            "A well-maintained acoustic guitar suitable for beginners and intermediate players.",
            149.99m, "USD", Guid.NewGuid(),
            ["https://img.example.com/guitar.jpg"]);

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>()
            .WithMessage("*User*not found*");
    }

    /// <summary>
    /// Tests that CreateListingAsync throws an UnauthorizedException when the seller is inactive.
    /// </summary>
    [Fact]
    public async Task CreateListingAsync_WhenSellerIsInactive_ThrowsUnauthorizedException()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var inactiveSeller = new User { Id = sellerId, IsActive = false };
        _userRepoMock.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync(inactiveSeller);

        // Act
        var act = async () => await _sut.CreateListingAsync(
            sellerId,
            "Quality Acoustic Guitar",
            "A well-maintained acoustic guitar suitable for beginners and intermediate players.",
            149.99m, "USD", Guid.NewGuid(),
            ["https://img.example.com/guitar.jpg"]);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    /// <summary>
    /// Tests that DelistListingAsync throws an UnauthorizedException when the caller is not the seller.
    /// </summary>
    [Fact]
    public async Task DelistListingAsync_WhenCallerIsNotSeller_ThrowsUnauthorizedException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var listing = new Listing { Id = listingId, SellerId = ownerId, Status = ListingStatus.Active };
        _listingRepoMock.Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        // Act
        var act = async () => await _sut.DelistListingAsync(listingId, requesterId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    /// <summary>
    /// Tests that MarkAsFeaturedAsync throws an UnauthorizedException when the caller is not an admin.
    /// </summary>
    [Fact]
    public async Task MarkAsFeaturedAsync_WhenCallerIsNotAdmin_ThrowsUnauthorizedException()
    {
        // Arrange
        var nonAdminId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var regularUser = new User { Id = nonAdminId, Role = UserRole.User };
        _userRepoMock.Setup(r => r.GetByIdAsync(nonAdminId))
            .ReturnsAsync(regularUser);

        // Act
        var act = async () => await _sut.MarkAsFeaturedAsync(listingId, nonAdminId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
