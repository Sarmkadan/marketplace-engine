#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace MarketplaceEngine.Services;

/// <summary>
/// Compile-time generated log messages for <see cref="PaymentGateway"/>.
/// Payment data is logged by identifier only; card details and API keys must never be passed here.
/// </summary>
internal static partial class PaymentGatewayLog
{
    [LoggerMessage(EventId = 4000, Level = LogLevel.Information,
        Message = "Payment authorization attempted: OrderId={OrderId}, IdempotencyKey={IdempotencyKey}")]
    public static partial void AuthorizationAttempted(ILogger logger, Guid orderId, Guid idempotencyKey);

    [LoggerMessage(EventId = 4001, Level = LogLevel.Information,
        Message = "Payment authorization succeeded: OrderId={OrderId}, IdempotencyKey={IdempotencyKey}, TransactionId={TransactionId}")]
    public static partial void AuthorizationSucceeded(ILogger logger, Guid orderId, Guid idempotencyKey, string? transactionId);

    [LoggerMessage(EventId = 4002, Level = LogLevel.Warning,
        Message = "Payment authorization failed: OrderId={OrderId}, IdempotencyKey={IdempotencyKey}, DeclineCode={DeclineCode}")]
    public static partial void AuthorizationDeclined(ILogger logger, Guid orderId, Guid idempotencyKey, string declineCode);

    [LoggerMessage(EventId = 4003, Level = LogLevel.Warning,
        Message = "Payment authorization failed with unexpected error: OrderId={OrderId}, IdempotencyKey={IdempotencyKey}")]
    public static partial void AuthorizationErrored(ILogger logger, Exception exception, Guid orderId, Guid idempotencyKey);

    [LoggerMessage(EventId = 4004, Level = LogLevel.Information,
        Message = "Payment captured: OrderId={OrderId}, Amount={Amount} {Currency}")]
    public static partial void CaptureSucceeded(ILogger logger, Guid orderId, decimal amount, string currency);

    [LoggerMessage(EventId = 4005, Level = LogLevel.Warning,
        Message = "Payment capture failed: OrderId={OrderId}")]
    public static partial void CaptureFailed(ILogger logger, Exception exception, Guid orderId);

    [LoggerMessage(EventId = 4006, Level = LogLevel.Information,
        Message = "Payment refunded: OrderId={OrderId}, Reason={Reason}")]
    public static partial void RefundSucceeded(ILogger logger, Guid orderId, string reason);

    [LoggerMessage(EventId = 4007, Level = LogLevel.Warning,
        Message = "Payment refund failed: OrderId={OrderId}")]
    public static partial void RefundFailed(ILogger logger, Exception exception, Guid orderId);
}
