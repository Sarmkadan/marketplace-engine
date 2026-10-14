#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MarketplaceEngine.Services;

/// <summary>
/// Compile-time generated log messages for <see cref="OrderProcessor"/>.
/// An "order" is the <see cref="Domain.Models.Payment"/> it creates, so <c>OrderId</c> is the payment ID.
/// </summary>
internal static partial class OrderProcessorLog
{
    [LoggerMessage(EventId = 5000, Level = LogLevel.Information,
        Message = "Order created: OrderId={OrderId}, ListingId={ListingId}, BuyerId={BuyerId}")]
    public static partial void OrderCreated(ILogger logger, Guid orderId, Guid listingId, Guid buyerId);

    [LoggerMessage(EventId = 5001, Level = LogLevel.Warning,
        Message = "Invalid listing ID: {ListingId}")]
    public static partial void InvalidListingId(ILogger logger, Guid listingId);

    [LoggerMessage(EventId = 5002, Level = LogLevel.Warning,
        Message = "Invalid buyer ID: {BuyerId}")]
    public static partial void InvalidBuyerId(ILogger logger, Guid buyerId);

    [LoggerMessage(EventId = 5003, Level = LogLevel.Warning,
        Message = "Invalid payment method: {PaymentMethod}")]
    public static partial void InvalidPaymentMethod(ILogger logger, string? paymentMethod);

    [LoggerMessage(EventId = 5004, Level = LogLevel.Error,
        Message = "Listing {ListingId} not found")]
    public static partial void ListingNotFound(ILogger logger, Guid listingId);

    [LoggerMessage(EventId = 5005, Level = LogLevel.Warning,
        Message = "Listing {ListingId} is not available for purchase (status: {Status})")]
    public static partial void ListingNotAvailable(ILogger logger, Guid listingId, ListingStatus status);

    [LoggerMessage(EventId = 5006, Level = LogLevel.Warning,
        Message = "Buyer {BuyerId} cannot purchase their own listing {ListingId}")]
    public static partial void SelfPurchaseRejected(ILogger logger, Guid buyerId, Guid listingId);

    [LoggerMessage(EventId = 5007, Level = LogLevel.Error,
        Message = "Buyer {BuyerId} not found")]
    public static partial void BuyerNotFound(ILogger logger, Guid buyerId);

    [LoggerMessage(EventId = 5008, Level = LogLevel.Error,
        Message = "Payment {OrderId} not found")]
    public static partial void OrderNotFound(ILogger logger, Guid orderId);

    [LoggerMessage(EventId = 5009, Level = LogLevel.Warning,
        Message = "Payment {OrderId} cannot be confirmed (status: {Status})")]
    public static partial void OrderNotConfirmable(ILogger logger, Guid orderId, PaymentStatus status);

    [LoggerMessage(EventId = 5010, Level = LogLevel.Information,
        Message = "Listing {ListingId} status updated to Delisted")]
    public static partial void ListingDelisted(ILogger logger, Guid listingId);

    [LoggerMessage(EventId = 5011, Level = LogLevel.Information,
        Message = "Order confirmed: OrderId={OrderId}, status=Completed")]
    public static partial void OrderConfirmed(ILogger logger, Guid orderId);

    [LoggerMessage(EventId = 5012, Level = LogLevel.Warning,
        Message = "Completed orders must be refunded, not cancelled. OrderId={OrderId}")]
    public static partial void CompletedOrderCancelRejected(ILogger logger, Guid orderId);

    [LoggerMessage(EventId = 5013, Level = LogLevel.Warning,
        Message = "Order is already cancelled. OrderId={OrderId}")]
    public static partial void OrderAlreadyCancelled(ILogger logger, Guid orderId);

    [LoggerMessage(EventId = 5014, Level = LogLevel.Information,
        Message = "Order cancelled: OrderId={OrderId}, reason: {Reason}")]
    public static partial void OrderCancelled(ILogger logger, Guid orderId, string reason);

    [LoggerMessage(EventId = 5016, Level = LogLevel.Warning,
        Message = "Invalid order ID: {OrderId}")]
    public static partial void InvalidOrderId(ILogger logger, Guid orderId);

    [LoggerMessage(EventId = 5017, Level = LogLevel.Warning,
        Message = "Invalid cancellation reason: {Reason}")]
    public static partial void InvalidCancellationReason(ILogger logger, string? reason);

    [LoggerMessage(EventId = 5015, Level = LogLevel.Information,
        Message = "Retrieved {Count} orders for buyer {BuyerId}")]
    public static partial void OrdersRetrieved(ILogger logger, int count, Guid buyerId);
}
