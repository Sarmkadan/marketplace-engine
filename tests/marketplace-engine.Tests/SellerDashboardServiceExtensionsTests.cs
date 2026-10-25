#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Moq;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Tests for the <see cref="SellerDashboardServiceExtensions"/> revenue summary calculations.
/// </summary>
public class SellerDashboardServiceExtensionsTests
{
    /// <summary>
    /// Tests that the profit margin is net revenue over gross revenue, without deducting platform fees twice.
    /// </summary>
    [Fact]
    public async Task GetRevenueSummaryAsync_ProfitMargin_DoesNotDeductFeesTwice()
    {
        // Arrange: gross 300, fees 15 (5%), net 285 (already net of fees)
        var sellerId = Guid.NewGuid();
        var userRepo = new Mock<IUserRepository>();
        var paymentRepo = new Mock<IPaymentRepository>();
        userRepo.Setup(r => r.GetByIdAsync(sellerId)).ReturnsAsync(new User { Id = sellerId, FullName = "Test Seller" });
        paymentRepo.Setup(r => r.GetBySellerIdAsync(sellerId)).ReturnsAsync(new List<Payment>
        {
            new Payment
            {
                Id = Guid.NewGuid(),
                SellerId = sellerId,
                Status = PaymentStatus.Completed,
                Amount = new Money(300m),
                PlatformFee = new Money(15m),
                SellerPayout = new Money(285m),
                CompletedAt = DateTime.UtcNow
            }
        });

        var sut = new SellerDashboardService(
            userRepo.Object,
            Mock.Of<IListingRepository>(),
            paymentRepo.Object,
            Mock.Of<IReviewRepository>(),
            Mock.Of<IMessageRepository>());

        // Act
        var summary = await sut.GetRevenueSummaryAsync(sellerId);

        // Assert
        summary.PlatformFeePercentage.Should().Be(5d);
        summary.ProfitMarginPercentage.Should().Be(95d);
    }
}
