#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

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

/// <summary>
/// Contains unit tests for the <see cref="PaymentService"/> class.
/// Tests various payment operations including initiation, cancellation, refund, and completion.
/// </summary>
public class PaymentServiceTests
{
    /// <summary>
    /// Mock repository for payment operations.
    /// </summary>
    private readonly Mock<IPaymentRepository> _paymentRepoMock;

    /// <summary>
    /// Mock repository for listing operations.
    /// </summary>
    private readonly Mock<IListingRepository> _listingRepoMock;

    /// <summary>
    /// Mock repository for user operations.
    /// </summary>
    private readonly Mock<IUserRepository> _userRepoMock;

    /// <summary>
    /// System under test - the PaymentService instance being tested.
    /// </summary>
    private readonly PaymentService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentServiceTests"/> class.
    /// Sets up mock repositories and creates the PaymentService instance for testing.
    /// </summary>
    public PaymentServiceTests()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _listingRepoMock = new Mock<IListingRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _sut = new PaymentService(_paymentRepoMock.Object, _listingRepoMock.Object, _userRepoMock.Object);
    }

    /// <summary>
    /// Tests that initiating payment with a non-existent listing throws ResourceNotFoundException.
    /// </summary>
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

    /// <summary>
    /// Tests that initiating payment with an inactive listing throws MarketplaceException.
    /// </summary>
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

    /// <summary>
    /// Tests that a buyer cannot purchase their own listing and throws MarketplaceException.
    /// </summary>
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

    /// <summary>
    /// Tests that valid payment initiation creates a payment record with Pending status.
    /// </summary>
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

    /// <summary>
    /// Tests that canceling a payment by a non-buyer throws UnauthorizedException.
    /// </summary>
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

    /// <summary>
    /// Tests that refunding a pending payment throws InvalidOperationException.
    /// </summary>
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

    /// <summary>
    /// Tests that completing a payment with a valid transaction ID marks the listing as delisted.
    /// </summary>
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

/// <summary>
/// Represents test data and configurations for PaymentService testing.
/// This class holds input values used by the PaymentServiceTests test methods.
/// </summary>
public class PaymentServiceTestsData
{
    /// <summary>
    /// Listing ID used for InitiatePaymentAsync_WhenListingNotFound_ThrowsResourceNotFoundException tests.
    /// </summary>
    public Guid ListingIdForNotFoundTest { get; set; }

    /// <summary>
    /// Buyer ID used for InitiatePaymentAsync_WhenListingNotFound_ThrowsResourceNotFoundException tests.
    /// </summary>
    public Guid BuyerIdForNotFoundTest { get; set; }

    /// <summary>
    /// Payment method used for InitiatePaymentAsync_WhenListingNotFound_ThrowsResourceNotFoundException tests.
    /// </summary>
    public string? PaymentMethodForNotFoundTest { get; set; }

    /// <summary>
    /// Listing ID used for InitiatePaymentAsync_WhenListingIsNotActive_ThrowsMarketplaceException tests.
    /// </summary>
    public Guid ListingIdForInactiveTest { get; set; }

    /// <summary>
    /// Listing with inactive status used for InitiatePaymentAsync_WhenListingIsNotActive_ThrowsMarketplaceException tests.
    /// </summary>
    public Listing? InactiveListing { get; set; }

    /// <summary>
    /// Buyer ID used for InitiatePaymentAsync_WhenBuyerIsSeller_ThrowsMarketplaceException tests.
    /// </summary>
    public Guid BuyerIdForSellerTest { get; set; }

    /// <summary>
    /// Seller ID used for InitiatePaymentAsync_WhenBuyerIsSeller_ThrowsMarketplaceException tests.
    /// </summary>
    public Guid SellerIdForSellerTest { get; set; }

    /// <summary>
    /// Listing with seller ID matching buyer used for InitiatePaymentAsync_WhenBuyerIsSeller_ThrowsMarketplaceException tests.
    /// </summary>
    public Listing? ListingWithMatchingSeller { get; set; }

    /// <summary>
    /// Buyer ID used for InitiatePaymentAsync_WithValidData_CreatesPayment tests.
    /// </summary>
    public Guid BuyerIdForValidTest { get; set; }

    /// <summary>
    /// Seller ID used for InitiatePaymentAsync_WithValidData_CreatesPayment tests.
    /// </summary>
    public Guid SellerIdForValidTest { get; set; }

    /// <summary>
    /// Listing ID used for InitiatePaymentAsync_WithValidData_CreatesPayment tests.
    /// </summary>
    public Guid ListingIdForValidTest { get; set; }

    /// <summary>
    /// Active listing used for InitiatePaymentAsync_WithValidData_CreatesPayment tests.
    /// </summary>
    public Listing? ActiveListing { get; set; }

    /// <summary>
    /// Payment ID used for CancelPaymentAsync_WhenCallerIsNotBuyer_ThrowsUnauthorizedException tests.
    /// </summary>
    public Guid PaymentIdForCancelTest { get; set; }

    /// <summary>
    /// Buyer ID used for CancelPaymentAsync_WhenCallerIsNotBuyer_ThrowsUnauthorizedException tests.
    /// </summary>
    public Guid BuyerIdForCancelTest { get; set; }

    /// <summary>
    /// Other user ID (not buyer) used for CancelPaymentAsync_WhenCallerIsNotBuyer_ThrowsUnauthorizedException tests.
    /// </summary>
    public Guid OtherUserIdForCancelTest { get; set; }

    /// <summary>
    /// Pending payment used for CancelPaymentAsync_WhenCallerIsNotBuyer_ThrowsUnauthorizedException tests.
    /// </summary>
    public Payment? PendingPayment { get; set; }

    /// <summary>
    /// Payment ID used for RefundPaymentAsync_WhenPaymentIsPending_ThrowsInvalidOperationException tests.
    /// </summary>
    public Guid PaymentIdForRefundTest { get; set; }

    /// <summary>
    /// Pending payment used for RefundPaymentAsync_WhenPaymentIsPending_ThrowsInvalidOperationException tests.
    /// </summary>
    public Payment? PendingPaymentForRefundTest { get; set; }

    /// <summary>
    /// Payment ID used for CompletePaymentAsync_WithValidTransactionId_MarksListingAsDelisted tests.
    /// </summary>
    public Guid PaymentIdForCompleteTest { get; set; }

    /// <summary>
    /// Seller ID used for CompletePaymentAsync_WithValidTransactionId_MarksListingAsDelisted tests.
    /// </summary>
    public Guid SellerIdForCompleteTest { get; set; }

    /// <summary>
    /// Listing ID used for CompletePaymentAsync_WithValidTransactionId_MarksListingAsDelisted tests.
    /// </summary>
    public Guid ListingIdForCompleteTest { get; set; }

    /// <summary>
    /// Processing payment used for CompletePaymentAsync_WithValidTransactionId_MarksListingAsDelisted tests.
    /// </summary>
    public Payment? ProcessingPayment { get; set; }

    /// <summary>
    /// Active listing used for CompletePaymentAsync_WithValidTransactionId_MarksListingAsDelisted tests.
    /// </summary>
    public Listing? ActiveListingForCompleteTest { get; set; }

    /// <summary>
    /// Valid external transaction ID used for CompletePaymentAsync_WithValidTransactionId_MarksListingAsDelisted tests.
    /// </summary>
    public string? ValidTransactionId { get; set; }

    /// <summary>
    /// Reason text used for RefundPaymentAsync tests.
    /// </summary>
    public string? RefundReason { get; set; }
}