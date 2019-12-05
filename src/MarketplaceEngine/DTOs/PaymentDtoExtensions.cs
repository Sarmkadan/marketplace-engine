#nullable enable

using System;
using System.Globalization;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Extension methods for PaymentDto providing common payment-related operations.
/// </summary>
public static class PaymentDtoExtensions
{
    /// <summary>
    /// Calculates the platform revenue for this payment.
    /// </summary>
    /// <param name="payment">The payment DTO</param>
    /// <returns>The platform revenue amount</returns>
    public static decimal GetPlatformRevenue(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.PlatformFee;
    }

    /// <summary>
    /// Calculates the seller payout for this payment.
    /// </summary>
    /// <param name="payment">The payment DTO</param>
    /// <returns>The seller payout amount</returns>
    public static decimal GetSellerPayout(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.SellerPayout;
    }

    /// <summary>
    /// Gets the total transaction amount (Amount + PlatformFee).
    /// </summary>
    /// <param name="payment">The payment DTO</param>
    /// <returns>The total transaction amount</returns>
    public static decimal GetTotalAmount(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.Amount + payment.PlatformFee;
    }

    /// <summary>
    /// Checks if the payment has been completed.
    /// </summary>
    /// <param name="payment">The payment DTO</param>
    /// <returns>True if payment is completed; otherwise false</returns>
    public static bool IsCompleted(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) && payment.CompletedAt.HasValue;
    }

    /// <summary>
    /// Checks if the payment has failed.
    /// </summary>
    /// <param name="payment">The payment DTO</param>
    /// <returns>True if payment failed; otherwise false</returns>
    public static bool IsFailed(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(payment.FailureReason);
    }

    /// <summary>
    /// Checks if the payment is pending.
    /// </summary>
    /// <param name="payment">The payment DTO</param>
    /// <returns>True if payment is pending; otherwise false</returns>
    public static bool IsPending(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) && !payment.CompletedAt.HasValue;
    }

    /// <summary>
    /// Gets a formatted currency string for the payment amount.
    /// </summary>
    /// <param name="payment">The payment DTO</param>
    /// <returns>Formatted currency string (e.g., "$125.50 USD")</returns>
    public static string FormatAmount(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return $"{FormatCurrency(payment.Amount)} {payment.Currency}";
    }

    /// <summary>
    /// Gets a formatted currency string for the platform fee.
    /// </summary>
    /// <param name="payment">The payment DTO</param>
    /// <returns>Formatted currency string for platform fee</returns>
    public static string FormatPlatformFee(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return $"{FormatCurrency(payment.PlatformFee)} {payment.Currency}";
    }

    /// <summary>
    /// Gets a formatted currency string for the seller payout.
    /// </summary>
    /// <param name="payment">The payment DTO</param>
    /// <returns>Formatted currency string for seller payout</returns>
    public static string FormatSellerPayout(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return $"{FormatCurrency(payment.SellerPayout)} {payment.Currency}";
    }

    /// <summary>
    /// Gets the time elapsed since payment was created.
    /// </summary>
    /// <param name="payment">The payment DTO</param>
    /// <returns>TimeSpan representing time since creation</returns>
    public static TimeSpan GetTimeSinceCreation(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return DateTime.UtcNow - payment.CreatedAt;
    }

    /// <summary>
    /// Gets a human-readable string representing when the payment was created.
    /// </summary>
    /// <param name="payment">The payment DTO</param>
    /// <returns>Formatted date string</returns>
    public static string GetCreatedAtString(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets a human-readable string representing when the payment was completed (if completed).
    /// </summary>
    /// <param name="payment">The payment DTO</param>
    /// <returns>Formatted date string or null if not completed</returns>
    public static string? GetCompletedAtString(this PaymentDto payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a decimal amount as currency with 2 decimal places.
    /// </summary>
    /// <param name="amount">The amount to format</param>
    /// <returns>Formatted currency string (e.g., "$125.50")</returns>
    private static string FormatCurrency(decimal amount)
    {
        return amount.ToString("C", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets the payment status as a normalized enum value.
    /// </summary>
    /// <param name="payment">The payment DTO</param>
    /// <returns>PaymentStatus enum value</returns>
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
}

