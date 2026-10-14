#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Configuration;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Tests that <see cref="PricingEngine"/> reads its discount ceiling, rounding mode, and currency from <see cref="PricingOptions"/>.
/// </summary>
public class PricingEngineOptionsTests
{
    private readonly Mock<IListingRepository> _listingRepoMock = new();

    /// <summary>
    /// Creates an engine that uses the given options.
    /// </summary>
    private PricingEngine CreateEngine(PricingOptions options) =>
        new(_listingRepoMock.Object, Options.Create(options), NullLogger<PricingEngine>.Instance);

    [Fact]
    public void ApplyDiscount_DefaultOptions_AllowsFiftyPercent()
    {
        var engine = CreateEngine(new PricingOptions());

        var (discounted, savings, _, _) = engine.ApplyDiscount(new Money(100m, "USD"), 0.50m);

        discounted.Amount.Should().Be(50m);
        savings.Amount.Should().Be(50m);
    }

    [Fact]
    public void ApplyDiscount_DefaultOptions_RejectsAboveFiftyPercent()
    {
        var engine = CreateEngine(new PricingOptions());

        var act = () => engine.ApplyDiscount(new Money(100m, "USD"), 0.51m);

        act.Should().Throw<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("discountPercent");
    }

    [Fact]
    public void ApplyDiscount_ConfiguredMaxDiscount_RejectsAboveConfiguredMaximum()
    {
        var engine = CreateEngine(new PricingOptions { MaxDiscountPercent = 20m });

        engine.ApplyDiscount(new Money(100m, "USD"), 0.20m).DiscountedPrice.Amount.Should().Be(80m);

        var act = () => engine.ApplyDiscount(new Money(100m, "USD"), 0.21m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ApplyDiscount_ConfiguredMaxDiscountAboveDefault_AllowsDiscountAboveFiftyPercent()
    {
        var engine = CreateEngine(new PricingOptions { MaxDiscountPercent = 80m });

        var (discounted, _, _, _) = engine.ApplyDiscount(new Money(100m, "USD"), 0.75m);

        discounted.Amount.Should().Be(25m);
    }

    [Fact]
    public void CalculateBreakdown_DefaultRoundingMode_RoundsMidpointAwayFromZero()
    {
        var engine = CreateEngine(new PricingOptions());

        // 10.10 * 5% = 0.505, which rounds to 0.51 with AwayFromZero
        var (_, fee, payout) = engine.CalculateBreakdown(new Money(10.10m, "USD"));

        fee.Amount.Should().Be(0.51m);
        payout.Amount.Should().Be(9.59m);
    }

    [Fact]
    public void CalculateBreakdown_ToEvenRoundingMode_RoundsMidpointToEven()
    {
        var engine = CreateEngine(new PricingOptions { RoundingMode = nameof(MidpointRounding.ToEven) });

        // 10.10 * 5% = 0.505, which rounds to 0.50 with ToEven
        var (_, fee, payout) = engine.CalculateBreakdown(new Money(10.10m, "USD"));

        fee.Amount.Should().Be(0.50m);
        payout.Amount.Should().Be(9.60m);
    }

    [Fact]
    public void CalculateBulkPrice_ToEvenRoundingMode_RoundsDiscountToEven()
    {
        var engine = CreateEngine(new PricingOptions { RoundingMode = nameof(MidpointRounding.ToEven) });

        // 5 units at 0.10 = 0.50 subtotal; 5% discount = 0.025, which rounds to 0.02 with ToEven
        var total = engine.CalculateBulkPrice(new Money(0.10m, "USD"), 5);

        total.Amount.Should().Be(0.48m);
    }

    [Fact]
    public void Constructor_UnknownRoundingMode_ThrowsArgumentException()
    {
        var act = () => CreateEngine(new PricingOptions { RoundingMode = "Banker" });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task CalculateTotalAsync_ListingWithoutPrice_UsesConfiguredCurrency()
    {
        var listingId = Guid.NewGuid();
        _listingRepoMock.Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(new Listing { Id = listingId, Price = null });
        var engine = CreateEngine(new PricingOptions { Currency = "EUR" });

        var (buyerTotal, _, _) = await engine.CalculateTotalAsync(listingId);

        buyerTotal.Amount.Should().Be(0m);
        buyerTotal.CurrencyCode.Should().Be("EUR");
    }

    [Fact]
    public void Constructor_NullOptions_Throws()
    {
        var act = () => new PricingEngine(_listingRepoMock.Object, null!, NullLogger<PricingEngine>.Instance);

        act.Should().Throw<ArgumentNullException>();
    }
}
