#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.Enums;

/// <summary>
/// Represents the processing state of a payment transaction.
/// </summary>
public enum PaymentStatus
{
    /// <summary>Payment has been created and is awaiting processing.</summary>
    Pending = 1,

    /// <summary>Payment is currently being processed by the payment provider.</summary>
    Processing = 2,

    /// <summary>Payment was completed successfully.</summary>
    Completed = 3,

    /// <summary>Payment failed due to an error or decline.</summary>
    Failed = 4,

    /// <summary>Payment was cancelled by the buyer or seller.</summary>
    Cancelled = 5,

    /// <summary>Payment has been refunded to the buyer.</summary>
    Refunded = 6,

    /// <summary>Payment is held in escrow pending delivery confirmation.</summary>
    InEscrow = 7
}
