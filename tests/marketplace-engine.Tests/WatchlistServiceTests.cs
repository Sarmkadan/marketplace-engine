#nullable enable

using FluentAssertions;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Recommendations;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Moq;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Tests for the <see cref="WatchlistService"/> class.
/// </summary>
public class WatchlistServiceTests
{
    private readonly Mock<IListingRepository> _listingRepoMock;
    private readonly Mock<IRecommendationEngine> _recommendationEngineMock;
    private readonly WatchlistService _sut;

    public WatchlistServiceTests()
    {
        _listingRepoMock = new Mock<IListingRepository>();
        _recommendationEngineMock = new Mock<IRecommendationEngine>();
        _listingRepoMock.Setup(r => r.IncrementInterestCountAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        _recommendationEngineMock.Setup(r => r.RecordSignalAsync(
                It.IsAny<UserActivitySignal>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _sut = new WatchlistService(_listingRepoMock.Object, _recommendationEngineMock.Object);
    }

    [Fact]
    public async Task AddAsync_WhenListingIsAddedTwice_ReturnsTrueThenFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        _listingRepoMock.Setup(r => r.ExistsAsync(listingId)).ReturnsAsync(true);

        // Act
        var firstResult = await _sut.AddAsync(userId, listingId);
        var secondResult = await _sut.AddAsync(userId, listingId);

        // Assert
        firstResult.Should().BeTrue();
        secondResult.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_WhenListingDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        _listingRepoMock.Setup(r => r.ExistsAsync(listingId)).ReturnsAsync(false);

        // Act
        var act = async () => await _sut.AddAsync(Guid.NewGuid(), listingId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{listingId}*");
    }

    [Fact]
    public async Task AddAsync_WhenListingExists_IncrementsInterestAndRecordsOneSaveSignal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        _listingRepoMock.Setup(r => r.ExistsAsync(listingId)).ReturnsAsync(true);

        // Act
        await _sut.AddAsync(userId, listingId);
        await _sut.AddAsync(userId, listingId);

        // Assert
        _listingRepoMock.Verify(r => r.IncrementInterestCountAsync(listingId), Times.Once);
        _recommendationEngineMock.Verify(
            r => r.RecordSignalAsync(
                It.Is<UserActivitySignal>(signal =>
                    signal.UserId == userId &&
                    signal.ListingId == listingId &&
                    signal.SignalType == SignalType.Save),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_WhenNotWatchedReturnsFalse_AndAfterAddReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        _listingRepoMock.Setup(r => r.ExistsAsync(listingId)).ReturnsAsync(true);

        // Act
        var beforeAdd = await _sut.RemoveAsync(userId, listingId);
        await _sut.AddAsync(userId, listingId);
        var afterAdd = await _sut.RemoveAsync(userId, listingId);

        // Assert
        beforeAdd.Should().BeFalse();
        afterAdd.Should().BeTrue();
    }

    [Fact]
    public async Task IsWatching_ReflectsAddAndRemove()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        _listingRepoMock.Setup(r => r.ExistsAsync(listingId)).ReturnsAsync(true);

        // Act and assert
        _sut.IsWatching(userId, listingId).Should().BeFalse();
        await _sut.AddAsync(userId, listingId);
        _sut.IsWatching(userId, listingId).Should().BeTrue();
        await _sut.RemoveAsync(userId, listingId);
        _sut.IsWatching(userId, listingId).Should().BeFalse();
    }

    [Fact]
    public async Task GetWatchedListingsAsync_WhenRepositoryNoLongerReturnsListing_SkipsIt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var availableListing = new Listing { Id = Guid.NewGuid() };
        var removedListingId = Guid.NewGuid();
        _listingRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
        await _sut.AddAsync(userId, availableListing.Id);
        await _sut.AddAsync(userId, removedListingId);
        _listingRepoMock.Setup(r => r.GetByIdAsync(availableListing.Id)).ReturnsAsync(availableListing);
        _listingRepoMock.Setup(r => r.GetByIdAsync(removedListingId)).ReturnsAsync((Listing?)null);

        // Act
        var result = await _sut.GetWatchedListingsAsync(userId);

        // Assert
        result.Should().ContainSingle().Which.Should().BeSameAs(availableListing);
    }

    [Fact]
    public async Task GetWatcherCount_WhenMultipleUsersWatchListing_CountsAllUsers()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        _listingRepoMock.Setup(r => r.ExistsAsync(listingId)).ReturnsAsync(true);

        // Act
        await _sut.AddAsync(Guid.NewGuid(), listingId);
        await _sut.AddAsync(Guid.NewGuid(), listingId);
        await _sut.AddAsync(Guid.NewGuid(), listingId);

        // Assert
        _sut.GetWatcherCount(listingId).Should().Be(3);
    }
}
