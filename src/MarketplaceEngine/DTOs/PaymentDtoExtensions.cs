#nullable enable

using System;
using System.Globalization;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Extension methods for <see cref="PaymentDto"/> providing common payment-related operations.
/// </summary>
public static class PaymentDtoExtensions
{
    /// <summary>
    /// Calculates the platform revenue for this payment.
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <returns>The platform revenue amount.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payment"/> is <see langword="null"/>.</exception>
    public static decimal GetPlatformRevenue(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.PlatformFee;
    }

    /// <summary>
    /// Calculates the seller payout for this payment.
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <returns>The seller payout amount.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payment"/> is <see langword="null"/>.</exception>
    public static decimal GetSellerPayout(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.SellerPayout;
    }

    /// <summary>
    /// Gets the total transaction amount (Amount + PlatformFee).
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <returns>The total transaction amount.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payment"/> is <see langword="null"/>.</exception>
    public static decimal GetTotalAmount(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.Amount + payment.PlatformFee;
    }

    /// <summary>
    /// Checks if the payment has been completed.
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <returns>True if payment is completed; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payment"/> is <see langword="null"/>.</exception>
    public static bool IsCompleted(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) && payment.CompletedAt.HasValue;
    }

    /// <summary>
    /// Checks if the payment has failed.
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <returns>True if payment failed; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payment"/> is <see langword="null"/>.</exception>
    public static bool IsFailed(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(payment.FailureReason);
    }

    /// <summary>
    /// Checks if the payment is pending.
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <returns>True if payment is pending; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payment"/> is <see langword="null"/>.</exception>
    public static bool IsPending(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) && !payment.CompletedAt.HasValue;
    }

    /// <summary>
    /// Gets a formatted currency string for the payment amount.
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <returns>Formatted currency string (e.g., "$125.50 USD").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payment"/> is <see langword="null"/>.</exception>
    public static string FormatAmount(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return $"{FormatCurrency(payment.Amount)} {payment.Currency}";
    }

    /// <summary>
    /// Gets a formatted currency string for the platform fee.
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <returns>Formatted currency string for platform fee.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payment"/> is <see langword="null"/>.</exception>
    public static string FormatPlatformFee(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return $"{FormatCurrency(payment.PlatformFee)} {payment.Currency}";
    }

    /// <summary>
    /// Gets a formatted currency string for the seller payout.
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <returns>Formatted currency string for seller payout.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payment"/> is <see langword="null"/>.</exception>
    public static string FormatSellerPayout(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return $"{FormatCurrency(payment.SellerPayout)} {payment.Currency}";
    }

    /// <summary>
    /// Gets the time elapsed since payment was created.
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <returns>TimeSpan representing time since creation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payment"/> is <see langword="null"/>.</exception>
    public static TimeSpan GetTimeSinceCreation(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return DateTime.UtcNow - payment.CreatedAt;
    }

    /// <summary>
    /// Gets a human-readable string representing when the payment was created.
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <returns>Formatted date string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payment"/> is <see langword="null"/>.</exception>
    public static string GetCreatedAtString(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets a human-readable string representing when the payment was completed (if completed).
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <returns>Formatted date string or null if not completed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payment"/> is <see langword="null"/>.</exception>
    public static string? GetCompletedAtString(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a decimal amount as currency with 2 decimal places.
    /// </summary>
    /// <param name="amount">The amount to format.</param>
    /// <returns>Formatted currency string (e.g., "$125.50").</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="amount"/> is negative.</exception>
    private static string FormatCurrency(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        }

        return amount.ToString("C2", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets the payment status as a normalized enum value.
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <returns>PaymentStatus enum value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payment"/> is <see langword="null"/>.</exception>
    public static MarketplaceEngine.Domain.Enums.PaymentStatus GetPaymentStatus(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        if (payment.IsCompleted())
        {
            return MarketplaceEngine.Domain.Enums.PaymentStatus.Completed;
        }

        if (payment.IsFailed())
        {
            return MarketplaceEngine.Domain.Enums.PaymentStatus.Failed;
        }

        if (payment.IsPending())
        {
            return MarketplaceEngine.Domain.Enums.PaymentStatus.Pending;
        }

        return MarketplaceEngine.Domain.Enums.PaymentStatus.Processing;
    }

    /// <summary>
    /// Checks if the payment is refundable within a given time window.
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <param name="window">The time window.</param>
    /// <returns>True if refundable; otherwise false.</returns>
    public static bool IsRefundable(this PaymentDto payment, TimeSpan window)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) && 
               payment.CompletedAt.HasValue &&
               (DateTime.UtcNow - payment.CompletedAt.Value) <= window;
    }

    /// <summary>
    /// Calculates the net amount after deducting the fee rate.
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <param name="feeRate">The fee rate as a decimal (e.g., 0.05 for 5%).</param>
    /// <returns>The net amount.</returns>
    public static decimal NetAmount(this PaymentDto payment, decimal feeRate)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.Amount * (1 - feeRate);
    }

    /// <summary>
    /// Validates if a status transition is allowed.
    /// </summary>
    /// <param name="payment">The payment DTO.</param>
    /// <param name="newStatus">The target status.</param>
    /// <returns>True if transition is allowed; otherwise false.</returns>
    public static bool CanTransitionTo(this PaymentDto payment, MarketplaceEngine.Domain.Enums.PaymentStatus newStatus)
    {
        ArgumentNullException.ThrowIfNull(payment);
        var currentStatus = payment.GetPaymentStatus();

        if (currentStatus == newStatus) return false;

        return currentStatus switch
        {
            MarketplaceEngine.Domain.Enums.PaymentStatus.Pending => 
                newStatus == MarketplaceEngine.Domain.Enums.PaymentStatus.Processing || 
                newStatus == MarketplaceEngine.Domain.Enums.PaymentStatus.Failed || 
                newStatus == MarketplaceEngine.Domain.Enums.PaymentStatus.Cancelled,
            
            MarketplaceEngine.Domain.Enums.PaymentStatus.Processing => 
                newStatus == MarketplaceEngine.Domain.Enums.PaymentStatus.Completed || 
                newStatus == MarketplaceEngine.Domain.Enums.PaymentStatus.Failed || 
                newStatus == MarketplaceEngine.Domain.Enums.PaymentStatus.InEscrow || 
                newStatus == MarketplaceEngine.Domain.Enums.PaymentStatus.Cancelled,

            MarketplaceEngine.Domain.Enums.PaymentStatus.InEscrow => 
                newStatus == MarketplaceEngine.Domain.Enums.PaymentStatus.Completed || 
                newStatus == MarketplaceEngine.Domain.Enums.PaymentStatus.Failed || 
                newStatus == MarketplaceEngine.Domain.Enums.PaymentStatus.Refunded,

            MarketplaceEngine.Domain.Enums.PaymentStatus.Completed => 
                newStatus == MarketplaceEngine.Domain.Enums.PaymentStatus.Refunded,

            _ => false
        };
    }
}