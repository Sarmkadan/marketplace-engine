using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MarketplaceEngine.Tests
{
    public class OrderProcessorTests
    {
        private readonly Mock<IPaymentRepository> _mockPaymentRepository;
        private readonly Mock<IListingRepository> _mockListingRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ILogger<OrderProcessor>> _mockLogger;
        private readonly OrderProcessor _orderProcessor;

        public OrderProcessorTests()
        {
            _mockPaymentRepository = new Mock<IPaymentRepository>();
            _mockListingRepository = new Mock<IListingRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<OrderProcessor>>();

            _orderProcessor = new OrderProcessor(
                _mockPaymentRepository.Object,
                _mockListingRepository.Object,
                _mockUserRepository.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task PlaceOrderAsync_WithValidInput_ReturnsPayment()
        {
            // Arrange
            var listingId = Guid.NewGuid();
            var buyerId = Guid.NewGuid();
            var sellerId = Guid.NewGuid();
            var paymentMethod = "card";

            var listing = new Listing
            {
                Id = listingId,
                SellerId = sellerId,
                Status = ListingStatus.Active,
                Price = new Money(100, "USD")
            };

            var buyer = new User { Id = buyerId };

            _mockListingRepository.Setup(r => r.GetByIdAsync(listingId)).ReturnsAsync(listing);
            _mockUserRepository.Setup(r => r.GetByIdAsync(buyerId)).ReturnsAsync(buyer);

            // Act
            var result = await _orderProcessor.PlaceOrderAsync(listingId, buyerId, paymentMethod);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(listingId, result.ListingId);
            Assert.Equal(buyerId, result.BuyerId);
            Assert.Equal(sellerId, result.SellerId);
            Assert.Equal(PaymentStatus.Pending, result.Status);
            Assert.Equal(paymentMethod, result.PaymentMethod);
        }

        [Fact]
        public async Task PlaceOrderAsync_WithEmptyListingId_ThrowsArgumentException()
        {
            // Arrange
            var listingId = Guid.Empty;
            var buyerId = Guid.NewGuid();
            var paymentMethod = "card";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _orderProcessor.PlaceOrderAsync(listingId, buyerId, paymentMethod));
        }

        [Fact]
        public async Task PlaceOrderAsync_WithEmptyBuyerId_ThrowsArgumentException()
        {
            // Arrange
            var listingId = Guid.NewGuid();
            var buyerId = Guid.Empty;
            var paymentMethod = "card";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _orderProcessor.PlaceOrderAsync(listingId, buyerId, paymentMethod));
        }

        [Fact]
        public async Task PlaceOrderAsync_WithEmptyPaymentMethod_ThrowsArgumentException()
        {
            // Arrange
            var listingId = Guid.NewGuid();
            var buyerId = Guid.NewGuid();
            var paymentMethod = string.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _orderProcessor.PlaceOrderAsync(listingId, buyerId, paymentMethod));
        }

        [Fact]
        public async Task ConfirmOrderAsync_WithValidInput_ReturnsPayment()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var listingId = Guid.NewGuid();

            var payment = new Payment
            {
                Id = paymentId,
                ListingId = listingId,
                Status = PaymentStatus.Pending
            };

            var listing = new Listing
            {
                Id = listingId,
                Status = ListingStatus.Active
            };

            _mockPaymentRepository.Setup(r => r.GetByIdAsync(paymentId)).ReturnsAsync(payment);
            _mockListingRepository.Setup(r => r.GetByIdAsync(listingId)).ReturnsAsync(listing);

            // Act
            var result = await _orderProcessor.ConfirmOrderAsync(paymentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PaymentStatus.Completed, result.Status);
        }

        [Fact]
        public async Task ConfirmOrderAsync_WithEmptyPaymentId_ThrowsArgumentException()
        {
            // Arrange
            var paymentId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _orderProcessor.ConfirmOrderAsync(paymentId));
        }

        [Fact]
        public async Task CancelOrderAsync_WithValidInput_ReturnsPayment()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var reason = "Customer requested cancellation";

            var payment = new Payment
            {
                Id = paymentId,
                Status = PaymentStatus.Pending
            };

            _mockPaymentRepository.Setup(r => r.GetByIdAsync(paymentId)).ReturnsAsync(payment);

            // Act
            var result = await _orderProcessor.CancelOrderAsync(paymentId, reason);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PaymentStatus.Cancelled, result.Status);
            Assert.Equal(reason, result.FailureReason);
        }

        [Fact]
        public async Task CancelOrderAsync_WithEmptyPaymentId_ThrowsArgumentException()
        {
            // Arrange
            var paymentId = Guid.Empty;
            var reason = "Customer requested cancellation";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _orderProcessor.CancelOrderAsync(paymentId, reason));
        }

        [Fact]
        public async Task CancelOrderAsync_WithEmptyReason_ThrowsArgumentException()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var reason = string.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _orderProcessor.CancelOrderAsync(paymentId, reason));
        }

        [Fact]
        public async Task GetOrdersByBuyerAsync_WithValidInput_ReturnsPayments()
        {
            // Arrange
            var buyerId = Guid.NewGuid();
            var payment1 = new Payment { Id = Guid.NewGuid(), BuyerId = buyerId, CreatedAt = DateTime.UtcNow };
            var payment2 = new Payment { Id = Guid.NewGuid(), BuyerId = buyerId, CreatedAt = DateTime.UtcNow.AddDays(-1) };

            var payments = new List<Payment> { payment1, payment2 };

            _mockPaymentRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(payments);

            // Act
            var result = await _orderProcessor.GetOrdersByBuyerAsync(buyerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(payment1.Id, result[0].Id);
            Assert.Equal(payment2.Id, result[1].Id);
        }

        [Fact]
        public async Task GetOrdersByBuyerAsync_WithEmptyBuyerId_ThrowsArgumentException()
        {
            // Arrange
            var buyerId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _orderProcessor.GetOrdersByBuyerAsync(buyerId));
        }
    }
}
