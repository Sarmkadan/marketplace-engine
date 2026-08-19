#nullable enable
using FluentAssertions;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.DTOs;
using Xunit;

namespace MarketplaceEngine.Tests;

public class PaymentDtoExtensionsTests
{
    [Fact]
    public void IsRefundable_ShouldReturnTrue_WhenCompletedWithinWindow()
    {
        var payment = new PaymentDto
        {
            Status = "Completed",
            CompletedAt = DateTime.UtcNow.AddHours(-1)
        };
        var window = TimeSpan.FromHours(2);

        payment.IsRefundable(window).Should().BeTrue();
    }

    [Fact]
    public void IsRefundable_ShouldReturnFalse_WhenCompletedOutsideWindow()
    {
        var payment = new PaymentDto
        {
            Status = "Completed",
            CompletedAt = DateTime.UtcNow.AddHours(-3)
        };
        var window = TimeSpan.FromHours(2);

        payment.IsRefundable(window).Should().BeFalse();
    }

    [Fact]
    public void NetAmount_ShouldCalculateCorrectly()
    {
        var payment = new PaymentDto { Amount = 100m };
        var feeRate = 0.05m;

        payment.NetAmount(feeRate).Should().Be(95m);
    }

    [Fact]
    public void CanTransitionTo_PendingToProcessing_ShouldBeTrue()
    {
        var payment = new PaymentDto { Status = "Pending" };
        payment.CanTransitionTo(PaymentStatus.Processing).Should().BeTrue();
    }

    [Fact]
    public void CanTransitionTo_PendingToCompleted_ShouldBeFalse()
    {
        var payment = new PaymentDto { Status = "Pending" };
        payment.CanTransitionTo(PaymentStatus.Completed).Should().BeFalse();
    }

    [Fact]
    public void CanTransitionTo_CompletedToRefunded_ShouldBeTrue()
    {
        var payment = new PaymentDto { Status = "Completed", CompletedAt = DateTime.UtcNow };
        payment.CanTransitionTo(PaymentStatus.Refunded).Should().BeTrue();
    }
}
