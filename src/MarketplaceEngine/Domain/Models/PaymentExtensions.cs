#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.ValueObjects;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="Payment"/> that offer common payment operations
/// and calculations.
/// </summary>
public static class PaymentExtensions
{
    /// <summary>
    /// Calculates the net amount that will be paid to the seller after all fees.
    /// </summary>
    /// <param name="payment">The payment instance.</param>
    /// <returns>The net payout amount, or null if not available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when payment is null.</exception>
    public static Money? GetNetPayout(this Payment payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        return payment.SellerPayout;
    }

    /// <summary>
    /// Calculates the total fees deducted from the payment amount.
    /// </summary>
    /// <param name="payment">The payment instance.</param>
    /// <returns>The total fee amount, or null if not available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when payment is null.</exception>
    public static Money? GetTotalFees(this Payment payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        if (payment.Amount is null || payment.PlatformFee is null)
        {
            return null;
        }

        return payment.Amount.Subtract(payment.PlatformFee);
    }

    /// <summary>
    /// Determines whether the payment is refundable based on its current status.
    /// </summary>
    /// <param name="payment">The payment instance.</param>
    /// <returns>True if the payment can be refunded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when payment is null.</exception>
    public static bool IsRefundable(this Payment payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        return payment.Status is PaymentStatus.Completed or PaymentStatus.InEscrow;
    }

    /// <summary>
    /// Determines whether the payment is in a terminal state (completed, failed, cancelled, or refunded).
    /// </summary>
    /// <param name="payment">The payment instance.</param>
    /// <returns>True if the payment is in a terminal state; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when payment is null.</exception>
    public static bool IsTerminalState(this Payment payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        return payment.Status is PaymentStatus.Completed or PaymentStatus.Failed
            or PaymentStatus.Cancelled or PaymentStatus.Refunded;
    }
}