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

public class ReviewServiceTests
{
    private readonly Mock<IReviewRepository> _reviewRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IListingRepository> _listingRepoMock;
    private readonly ReviewService _sut;

    public ReviewServiceTests()
    {
        _reviewRepoMock = new Mock<IReviewRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _listingRepoMock = new Mock<IListingRepository>();
        _sut = new ReviewService(_reviewRepoMock.Object, _userRepoMock.Object, _listingRepoMock.Object);
    }

    [Fact]
    public async Task SubmitReviewAsync_WhenReviewerNotFound_ThrowsResourceNotFoundException()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(reviewerId)).ReturnsAsync((User?)null);

        // Act
        var act = async () => await _sut.SubmitReviewAsync(reviewerId, Guid.NewGuid(), 5, "Great seller, fast shipping!");

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>().WithMessage("*User*not found*");
    }

    [Fact]
    public async Task SubmitReviewAsync_WhenReviewerIsInactive_ThrowsUnauthorizedException()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var reviewer = new User { Id = reviewerId, IsActive = false };
        _userRepoMock.Setup(r => r.GetByIdAsync(reviewerId)).ReturnsAsync(reviewer);

        // Act
        var act = async () => await _sut.SubmitReviewAsync(reviewerId, Guid.NewGuid(), 4, "Good transaction overall.");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task SubmitReviewAsync_WhenReviewerIsSeller_ThrowsMarketplaceException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = true };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var act = async () => await _sut.SubmitReviewAsync(userId, userId, 5, "Reviewing myself!");

        // Assert
        await act.Should().ThrowAsync<MarketplaceException>().WithMessage("*cannot review themselves*");
    }

    [Fact]
    public async Task SubmitReviewAsync_WhenDuplicateReview_ThrowsDuplicateResourceException()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var reviewer = new User { Id = reviewerId, IsActive = true };
        var seller = new User { Id = sellerId, IsActive = true };

        _userRepoMock.Setup(r => r.GetByIdAsync(reviewerId)).ReturnsAsync(reviewer);
        _userRepoMock.Setup(r => r.GetByIdAsync(sellerId)).ReturnsAsync(seller);
        _reviewRepoMock.Setup(r => r.ExistsForTransactionAsync(reviewerId, sellerId, null)).ReturnsAsync(true);

        // Act
        var act = async () => await _sut.SubmitReviewAsync(reviewerId, sellerId, 4, "Duplicate review attempt.");

        // Assert
        await act.Should().ThrowAsync<DuplicateResourceException>();
    }

    [Fact]
    public async Task SubmitReviewAsync_WithValidData_CreatesReviewAndUpdatesSellerRating()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var reviewer = new User { Id = reviewerId, IsActive = true };
        var seller = new User { Id = sellerId, IsActive = true };

        _userRepoMock.Setup(r => r.GetByIdAsync(reviewerId)).ReturnsAsync(reviewer);
        _userRepoMock.Setup(r => r.GetByIdAsync(sellerId)).ReturnsAsync(seller);
        _reviewRepoMock.Setup(r => r.ExistsForTransactionAsync(reviewerId, sellerId, null)).ReturnsAsync(false);
        _reviewRepoMock.Setup(r => r.AddAsync(It.IsAny<Review>()))
            .ReturnsAsync((Review rv) => { rv.Id = Guid.NewGuid(); return rv; });
        _reviewRepoMock.Setup(r => r.GetBySellerIdAsync(sellerId))
            .ReturnsAsync([new Review { Score = 5, Status = ReviewStatus.Active, ReviewerId = reviewerId, SellerId = sellerId }]);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        // Act
        var review = await _sut.SubmitReviewAsync(reviewerId, sellerId, 5, "Excellent item, highly recommended!");

        // Assert
        review.Should().NotBeNull();
        review.Score.Should().Be(5);
        review.ReviewerId.Should().Be(reviewerId);
        _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task AddSellerReplyAsync_WhenCallerIsNotSeller_ThrowsUnauthorizedException()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var actualSellerId = Guid.NewGuid();
        var wrongSellerId = Guid.NewGuid();
        var review = new Review { Id = reviewId, SellerId = actualSellerId, ReviewerId = Guid.NewGuid(), Score = 3, Comment = "Average." };

        _reviewRepoMock.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);

        // Act
        var act = async () => await _sut.AddSellerReplyAsync(reviewId, wrongSellerId, "Thanks for your feedback!");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task GetSellerStatsAsync_ReturnsCorrectAverageAndDistribution()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var seller = new User { Id = sellerId, IsActive = true };
        var reviews = new List<Review>
        {
            new Review { Score = 5, Status = ReviewStatus.Active, SellerId = sellerId, ReviewerId = Guid.NewGuid() },
            new Review { Score = 4, Status = ReviewStatus.Active, SellerId = sellerId, ReviewerId = Guid.NewGuid() },
            new Review { Score = 3, Status = ReviewStatus.Active, SellerId = sellerId, ReviewerId = Guid.NewGuid() }
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(sellerId)).ReturnsAsync(seller);
        _reviewRepoMock.Setup(r => r.GetBySellerIdAsync(sellerId)).ReturnsAsync(reviews);

        // Act
        var (average, total, distribution) = await _sut.GetSellerStatsAsync(sellerId);

        // Assert
        average.Should().Be(4.0);
        total.Should().Be(3);
        distribution[5].Should().Be(1);
        distribution[4].Should().Be(1);
        distribution[3].Should().Be(1);
        distribution[1].Should().Be(0);
    }

    [Fact]
    public async Task RemoveReviewAsync_WhenCallerIsNotModerator_ThrowsUnauthorizedException()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var regularUserId = Guid.NewGuid();
        var regularUser = new User { Id = regularUserId, Role = UserRole.User };

        _userRepoMock.Setup(r => r.GetByIdAsync(regularUserId)).ReturnsAsync(regularUser);

        // Act
        var act = async () => await _sut.RemoveReviewAsync(reviewId, regularUserId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
