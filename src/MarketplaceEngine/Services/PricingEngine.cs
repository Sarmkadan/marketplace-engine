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
/// Calculation order: Base price → Discounts → Platform fee → Final totals.
/// Rounding: All monetary values rounded to 2 decimal places using MidpointRounding.AwayFromZero.
/// </summary>
public class PricingEngine
{
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<PricingEngine> _logger;

    /// <summary>
    /// Platform commission rate applied to each transaction (5%).
    /// </summary>
    private const decimal PlatformFeeRate = 0.05m;

    /// <summary>
    /// Maximum allowed discount percentage (50%).
    /// </summary>
    private const decimal MaxDiscountPercent = 0.50m;

    /// <summary>
    /// Initializes a new instance of the <see cref="PricingEngine"/> class.
    /// </summary>
    /// <param name="listingRepository">Repository for accessing listing data.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="listingRepository"/> is null.</exception>
    public PricingEngine(IListingRepository listingRepository)
        : this(listingRepository, NullLogger<PricingEngine>.Instance)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PricingEngine"/> class with a logger.
    /// </summary>
    /// <param name="listingRepository">Repository for accessing listing data.</param>
    /// <param name="logger">Logger for pricing operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="listingRepository"/> or <paramref name="logger"/> is null.</exception>
    public PricingEngine(
        IListingRepository listingRepository,
        ILogger<PricingEngine> logger)
    {
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates the total price for a listing including platform fee.
    /// Calculation flow: Get listing base price → Apply platform fee (5%) → Return breakdown.
    /// Rounding: Platform fee rounded to 2 decimal places using MidpointRounding.AwayFromZero.
    /// </summary>
    /// <param name="listingId">The listing to price.</param>
    /// <returns>A tuple containing:
    /// <list type="bullet">
    /// <item><description>BuyerTotal: Amount the buyer pays (base price)</description></item>
    /// <item><description>PlatformFee: Platform commission (5% of base price)</description></item>
    /// <item><description>SellerPayout: Amount the seller receives (base price minus platform fee)</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="MarketplaceException">Thrown when the listing with <paramref name="listingId"/> is not found.</exception>
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
    /// Calculation flow: Base price → Calculate platform fee (5%) → Compute seller payout → Return totals.
    /// Rounding: Platform fee rounded to 2 decimal places using MidpointRounding.AwayFromZero.
    /// </summary>
    /// <param name="basePrice">The listing's base price.</param>
    /// <returns>A tuple containing:
    /// <list type="bullet">
    /// <item><description>BuyerTotal: Amount the buyer pays (equals base price)</description></item>
    /// <item><description>PlatformFee: Platform commission (5% of base price)</description></item>
    /// <item><description>SellerPayout: Amount the seller receives (base price minus platform fee)</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="basePrice"/> is null.</exception>
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
    /// Calculation flow: Base price → Apply discount → Calculate platform fee on discounted price → Return breakdown.
    /// Rounding: Discount amount and platform fee rounded to 2 decimal places using MidpointRounding.AwayFromZero.
    /// </summary>
    /// <param name="basePrice">Original price before discount.</param>
    /// <param name="discountPercent">Discount as a decimal (0.0 to 0.5 representing 0% to 50%).</param>
    /// <returns>A tuple containing:
    /// <list type="bullet">
    /// <item><description>DiscountedPrice: Price after discount is applied</description></item>
    /// <item><description>Savings: Amount saved from the discount</description></item>
    /// <item><description>PlatformFee: Platform commission (5% of discounted price)</description></item>
    /// <item><description>SellerPayout: Amount the seller receives (discounted price minus platform fee)</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="basePrice"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="discountPercent"/> is less than 0 or greater than 0.5.</exception>
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
    /// Calculation flow: Calculate subtotal → Apply tiered discount → Return total.
    /// Tiered discounts: 5% for 5+ units, 10% for 10+ units, 15% for 25+ units.
    /// Rounding: Discount amount rounded to 2 decimal places using MidpointRounding.AwayFromZero.
    /// </summary>
    /// <param name="unitPrice">Price per unit.</param>
    /// <param name="quantity">Number of units (must be positive).</param>
    /// <returns>Total price after applying the appropriate bulk discount.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="unitPrice"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quantity"/> is less than or equal to zero.</exception>
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
