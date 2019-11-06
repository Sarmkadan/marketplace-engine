#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.DTOs;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Moq;
using Xunit;

namespace MarketplaceEngine.Tests;

public class SellerDashboardServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IListingRepository> _listingRepoMock;
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<IReviewRepository> _reviewRepoMock;
    private readonly Mock<IMessageRepository> _messageRepoMock;
    private readonly SellerDashboardService _sut;

    public SellerDashboardServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _listingRepoMock = new Mock<IListingRepository>();
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _reviewRepoMock = new Mock<IReviewRepository>();
        _messageRepoMock = new Mock<IMessageRepository>();

        _sut = new SellerDashboardService(
            _userRepoMock.Object,
            _listingRepoMock.Object,
            _paymentRepoMock.Object,
            _reviewRepoMock.Object,
            _messageRepoMock.Object);
    }

    [Fact]
    public async Task GetDashboardAsync_WhenSellerNotFound_ThrowsResourceNotFoundException()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(sellerId)).ReturnsAsync((User?)null);

        // Act
        var act = async () => await _sut.GetDashboardAsync(sellerId);

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>().WithMessage("*User*not found*");
    }

    [Fact]
    public async Task GetDashboardAsync_WithValidSeller_ReturnsCorrectMetrics()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var seller = new User { Id = sellerId, FullName = "Jane Seller", IsActive = true, TotalSales = 3 };
        var listings = new List<Listing>
        {
            new Listing { Id = Guid.NewGuid(), SellerId = sellerId, Status = ListingStatus.Active, Price = new Money(100m) },
            new Listing { Id = Guid.NewGuid(), SellerId = sellerId, Status = ListingStatus.Active, Price = new Money(200m) },
            new Listing { Id = Guid.NewGuid(), SellerId = sellerId, Status = ListingStatus.Inactive, Price = new Money(50m) }
        };
        var completedPayment = new Payment
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            Status = PaymentStatus.Completed,
            Amount = new Money(100m),
            SellerPayout = new Money(95m)
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(sellerId)).ReturnsAsync(seller);
        _listingRepoMock.Setup(r => r.GetBySellerIdAsync(sellerId)).ReturnsAsync(listings);
        _paymentRepoMock.Setup(r => r.GetBySellerIdAsync(sellerId)).ReturnsAsync([completedPayment]);
        _reviewRepoMock.Setup(r => r.GetBySellerIdAsync(sellerId)).ReturnsAsync([]);
        _messageRepoMock.Setup(r => r.GetUnreadMessagesAsync(sellerId)).ReturnsAsync([]);

        // Act
        var dashboard = await _sut.GetDashboardAsync(sellerId);

        // Assert
        dashboard.Should().NotBeNull();
        dashboard.SellerId.Should().Be(sellerId);
        dashboard.SellerName.Should().Be("Jane Seller");
        dashboard.ActiveListings.Should().Be(2);
        dashboard.TotalListings.Should().Be(3);
        dashboard.TotalRevenue.Should().Be(95m);
        dashboard.TotalSales.Should().Be(3);
    }

    [Fact]
    public async Task GetRevenueAsync_WithCompletedPayments_ReturnsMonthlyBreakdown()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var seller = new User { Id = sellerId, FullName = "Test Seller", IsActive = true };
        var now = DateTime.UtcNow;
        var payments = new List<Payment>
        {
            new Payment
            {
                Id = Guid.NewGuid(),
                SellerId = sellerId,
                Status = PaymentStatus.Completed,
                Amount = new Money(200m),
                PlatformFee = new Money(10m),
                SellerPayout = new Money(190m),
                CompletedAt = now
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                SellerId = sellerId,
                Status = PaymentStatus.Completed,
                Amount = new Money(100m),
                PlatformFee = new Money(5m),
                SellerPayout = new Money(95m),
                CompletedAt = now
            }
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(sellerId)).ReturnsAsync(seller);
        _paymentRepoMock.Setup(r => r.GetBySellerIdAsync(sellerId)).ReturnsAsync(payments);

        // Act
        var revenue = await _sut.GetRevenueAsync(sellerId);

        // Assert
        revenue.TotalGrossRevenue.Should().Be(300m);
        revenue.TotalPlatformFees.Should().Be(15m);
        revenue.TotalNetRevenue.Should().Be(285m);
        revenue.MonthlyBreakdown.Should().HaveCount(1);
        revenue.MonthlyBreakdown[0].SalesCount.Should().Be(2);
    }

    [Fact]
    public async Task GetListingStatsAsync_ReturnsTopListingsByViewCount()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var seller = new User { Id = sellerId, FullName = "Test Seller", IsActive = true };
        var listings = new List<Listing>
        {
            new Listing { Id = Guid.NewGuid(), Title = "Popular Item", SellerId = sellerId, Status = ListingStatus.Active, ViewCount = 500, Price = new Money(50m) },
            new Listing { Id = Guid.NewGuid(), Title = "Less Viewed", SellerId = sellerId, Status = ListingStatus.Active, ViewCount = 10, Price = new Money(20m) }
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(sellerId)).ReturnsAsync(seller);
        _listingRepoMock.Setup(r => r.GetBySellerIdAsync(sellerId)).ReturnsAsync(listings);

        // Act
        var stats = await _sut.GetListingStatsAsync(sellerId);

        // Assert
        stats.ActiveListings.Should().Be(2);
        stats.TotalViews.Should().Be(510);
        stats.TopListings.Should().HaveCount(2);
        stats.TopListings[0].Title.Should().Be("Popular Item");
    }
}
