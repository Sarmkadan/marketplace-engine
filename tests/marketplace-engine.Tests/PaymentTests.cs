#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Tests for the <see cref="Payment"/> domain model fee calculation.
/// </summary>
public class PaymentTests
{
    /// <summary>
    /// Tests that CalculateFees rounds a half-cent fee away from zero, as configured for monetary rounding.
    /// </summary>
    [Fact]
    public void CalculateFees_WithHalfCentFee_RoundsAwayFromZero()
    {
        // Arrange: 10.10 * 5% = 0.505, which banker's rounding would turn into 0.50
        var payment = new Payment { Amount = new Money(10.10m, "USD") };

        // Act
        payment.CalculateFees();

        // Assert
        payment.PlatformFee!.Amount.Should().Be(0.51m);
        payment.SellerPayout!.Amount.Should().Be(9.59m);
        (payment.PlatformFee.Amount + payment.SellerPayout.Amount).Should().Be(payment.Amount.Amount);
    }
}
