#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MarketplaceEngine.Services;

/// <summary>
/// Calculates final prices including platform fees, discounts, and currency-aware totals.
/// Centralizes all pricing logic to keep it consistent across the marketplace.
/// </summary>
public class PricingEngine
{
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<PricingEngine> _logger;

    /// <summary>
    /// Platform commission rate applied to each transaction.
    /// </summary>
    private const decimal PlatformFeeRate = 0.05m;

    /// <summary>
    /// Maximum allowed discount percentage.
    /// </summary>
    private const decimal MaxDiscountPercent = 0.50m;

    public PricingEngine(IListingRepository listingRepository)
        : this(listingRepository, NullLogger<PricingEngine>.Instance)
    {
    }

    public PricingEngine(
        IListingRepository listingRepository,
        ILogger<PricingEngine> logger)
    {
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates the total price for a listing including platform fee.
    /// </summary>
    /// <param name="listingId">The listing to price.</param>
    /// <returns>A tuple of (buyerTotal, platformFee, sellerPayout).</returns>
    public async Task<(Money BuyerTotal, Money PlatformFee, Money SellerPayout)> CalculateTotalAsync(Guid listingId)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is null)
            throw new MarketplaceException($"Listing {listingId} not found.");

        var basePrice = listing.Price ?? new Money(0);
        return CalculateBreakdown(basePrice);
    }

    /// <summary>
    /// Calculates the price breakdown for a given base amount.
    /// </summary>
    /// <param name="basePrice">The listing's base price.</param>
    /// <returns>A tuple of (buyerTotal, platformFee, sellerPayout).</returns>
    public (Money BuyerTotal, Money PlatformFee, Money SellerPayout) CalculateBreakdown(Money basePrice)
    {
        ArgumentNullException.ThrowIfNull(basePrice);

        var feeAmount = Math.Round(basePrice.Amount * PlatformFeeRate, 2, MidpointRounding.AwayFromZero);
        var platformFee = new Money(feeAmount, basePrice.CurrencyCode);
        var sellerPayout = new Money(basePrice.Amount - feeAmount, basePrice.CurrencyCode);
        var buyerTotal = basePrice;

        _logger.LogDebug("Price breakdown: base={Base}, fee={Fee}, seller={Seller}",
            basePrice.Amount, feeAmount, sellerPayout.Amount);

        return (buyerTotal, platformFee, sellerPayout);
    }

    /// <summary>
    /// Applies a percentage discount to a listing price and returns the discounted breakdown.
    /// </summary>
    /// <param name="basePrice">Original price.</param>
    /// <param name="discountPercent">Discount as a decimal (0.0 to 0.5).</param>
    /// <returns>Price breakdown after discount.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when discount exceeds allowed range.</exception>
    public (Money DiscountedPrice, Money Savings, Money PlatformFee, Money SellerPayout) ApplyDiscount(
        Money basePrice, decimal discountPercent)
    {
        ArgumentNullException.ThrowIfNull(basePrice);

        if (discountPercent < 0 || discountPercent > MaxDiscountPercent)
            throw new ArgumentOutOfRangeException(nameof(discountPercent),
                $"Discount must be between 0 and {MaxDiscountPercent:P0}.");

        var savings = Math.Round(basePrice.Amount * discountPercent, 2, MidpointRounding.AwayFromZero);
        var discountedAmount = basePrice.Amount - savings;
        var discountedPrice = new Money(discountedAmount, basePrice.CurrencyCode);
        var savingsMoney = new Money(savings, basePrice.CurrencyCode);

        var (_, fee, payout) = CalculateBreakdown(discountedPrice);

        _logger.LogInformation("Discount applied: {Percent:P0} off {Base} = {Discounted}",
            discountPercent, basePrice.Amount, discountedAmount);

        return (discountedPrice, savingsMoney, fee, payout);
    }

    /// <summary>
    /// Calculates a bulk pricing estimate for purchasing multiple units.
    /// Applies a tiered discount: 5% for 5+, 10% for 10+, 15% for 25+ units.
    /// </summary>
    /// <param name="unitPrice">Price per unit.</param>
    /// <param name="quantity">Number of units.</param>
    /// <returns>Total price after bulk discount.</returns>
    public Money CalculateBulkPrice(Money unitPrice, int quantity)
    {
        ArgumentNullException.ThrowIfNull(unitPrice);
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        var discountRate = quantity switch
        {
            >= 25 => 0.15m,
            >= 10 => 0.10m,
            >= 5 => 0.05m,
            _ => 0m
        };

        var subtotal = unitPrice.Amount * quantity;
        var discount = Math.Round(subtotal * discountRate, 2, MidpointRounding.AwayFromZero);
        var total = subtotal - discount;

        _logger.LogDebug("Bulk price: {Qty} x {Unit} = {Subtotal}, discount {Rate:P0} = {Total}",
            quantity, unitPrice.Amount, subtotal, discountRate, total);

        return new Money(total, unitPrice.CurrencyCode);
    }
}
