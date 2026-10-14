#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace MarketplaceEngine.Services;

/// <summary>
/// Compile-time generated log messages for <see cref="PricingEngine"/>.
/// </summary>
internal static partial class PricingEngineLog
{
    [LoggerMessage(EventId = 3000, Level = LogLevel.Debug,
        Message = "Price breakdown: base={BasePrice}, fee={PlatformFee}, seller={SellerPayout}")]
    public static partial void BreakdownCalculated(ILogger logger, decimal basePrice, decimal platformFee, decimal sellerPayout);

    [LoggerMessage(EventId = 3001, Level = LogLevel.Information,
        Message = "Discount applied: {DiscountPercent:P0} off {BasePrice} = {DiscountedPrice}")]
    public static partial void DiscountApplied(ILogger logger, decimal discountPercent, decimal basePrice, decimal discountedPrice);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Debug,
        Message = "Bulk price: {Quantity} x {UnitPrice} = {Subtotal}, discount {DiscountRate:P0} = {Total}")]
    public static partial void BulkPriceCalculated(ILogger logger, int quantity, decimal unitPrice, decimal subtotal, decimal discountRate, decimal total);
}
