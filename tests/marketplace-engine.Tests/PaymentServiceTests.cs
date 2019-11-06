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

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<IListingRepository> _listingRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _listingRepoMock = new Mock<IListingRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _sut = new PaymentService(_paymentRepoMock.Object, _listingRepoMock.Object, _userRepoMock.Object);
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenListingNotFound_ThrowsResourceNotFoundException()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        _listingRepoMock.Setup(r => r.GetByIdAsync(listingId)).ReturnsAsync((Listing?)null);

        // Act
        var act = async () => await _sut.InitiatePaymentAsync(listingId, Guid.NewGuid(), "card");

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>().WithMessage("*Listing*not found*");
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenListingIsNotActive_ThrowsMarketplaceException()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var listing = new Listing { Id = listingId, Status = ListingStatus.Inactive, Price = new Money(50m) };
        _listingRepoMock.Setup(r => r.GetByIdAsync(listingId)).ReturnsAsync(listing);

        // Act
        var act = async () => await _sut.InitiatePaymentAsync(listingId, Guid.NewGuid(), "card");

        // Assert
        await act.Should().ThrowAsync<MarketplaceException>();
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenBuyerIsSeller_ThrowsMarketplaceException()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var listing = new Listing { Id = listingId, SellerId = sellerId, Status = ListingStatus.Active, Price = new Money(100m) };
        var buyer = new User { Id = sellerId, IsActive = true };

        _listingRepoMock.Setup(r => r.GetByIdAsync(listingId)).ReturnsAsync(listing);
        _userRepoMock.Setup(r => r.GetByIdAsync(sellerId)).ReturnsAsync(buyer);

        // Act
        var act = async () => await _sut.InitiatePaymentAsync(listingId, sellerId, "card");

        // Assert
        await act.Should().ThrowAsync<MarketplaceException>().WithMessage("*cannot purchase their own listing*");
    }

    [Fact]
    public async Task InitiatePaymentAsync_WithValidData_CreatesPayment()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var listing = new Listing { Id = listingId, SellerId = sellerId, Status = ListingStatus.Active, Price = new Money(200m) };
        var buyer = new User { Id = buyerId, IsActive = true };

        _listingRepoMock.Setup(r => r.GetByIdAsync(listingId)).ReturnsAsync(listing);
        _userRepoMock.Setup(r => r.GetByIdAsync(buyerId)).ReturnsAsync(buyer);
        _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => { p.Id = Guid.NewGuid(); return p; });

        // Act
        var payment = await _sut.InitiatePaymentAsync(listingId, buyerId, "bank_transfer");

        // Assert
        payment.Should().NotBeNull();
        payment.BuyerId.Should().Be(buyerId);
        payment.SellerId.Should().Be(sellerId);
        payment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task CancelPaymentAsync_WhenCallerIsNotBuyer_ThrowsUnauthorizedException()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var payment = new Payment { Id = paymentId, BuyerId = buyerId, Status = PaymentStatus.Pending, Amount = new Money(50m) };

        _paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId)).ReturnsAsync(payment);

        // Act
        var act = async () => await _sut.CancelPaymentAsync(paymentId, otherId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task RefundPaymentAsync_WhenPaymentIsPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = new Payment { Id = paymentId, BuyerId = Guid.NewGuid(), Status = PaymentStatus.Pending, Amount = new Money(100m) };

        _paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId)).ReturnsAsync(payment);

        // Act
        var act = async () => await _sut.RefundPaymentAsync(paymentId, "Changed mind");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CompletePaymentAsync_WithValidTransactionId_MarksListingAsDelisted()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            BuyerId = Guid.NewGuid(),
            SellerId = sellerId,
            ListingId = listingId,
            Status = PaymentStatus.Processing,
            Amount = new Money(150m)
        };
        var listing = new Listing { Id = listingId, SellerId = sellerId, Status = ListingStatus.Active, Price = new Money(150m) };
        var seller = new User { Id = sellerId, IsActive = true };

        _paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId)).ReturnsAsync(payment);
        _paymentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);
        _listingRepoMock.Setup(r => r.GetByIdAsync(listingId)).ReturnsAsync(listing);
        _listingRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Listing>())).ReturnsAsync((Listing l) => l);
        _userRepoMock.Setup(r => r.GetByIdAsync(sellerId)).ReturnsAsync(seller);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        // Act
        var completed = await _sut.CompletePaymentAsync(paymentId, "txn_abc123");

        // Assert
        completed.Status.Should().Be(PaymentStatus.Completed);
        listing.Status.Should().Be(ListingStatus.Delisted);
    }
}
