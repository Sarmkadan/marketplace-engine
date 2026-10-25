#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.Enums;

/// <summary>
/// Provides common state and display helpers for <see cref="PaymentStatus"/> values.
/// </summary>
public static class PaymentStatusExtensions
{
    /// <summary>
    /// Determines whether the status is final, meaning the payment has reached an outcome
    /// (completed, failed, cancelled, or refunded) and is no longer being processed.
    /// </summary>
    /// <param name="status">The status to evaluate.</param>
    /// <returns><see langword="true"/> when the status is final; otherwise, <see langword="false"/>.</returns>
    public static bool IsFinal(this PaymentStatus status)
    {
        return status is PaymentStatus.Completed or PaymentStatus.Failed
            or PaymentStatus.Cancelled or PaymentStatus.Refunded;
    }

    /// <summary>
    /// Gets a human-readable display name for the status.
    /// </summary>
    /// <param name="status">The status to format.</param>
    /// <returns>The display name of the status.</returns>
    public static string ToDisplayName(this PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.Pending => "Pending",
            PaymentStatus.Processing => "Processing",
            PaymentStatus.Completed => "Completed",
            PaymentStatus.Failed => "Failed",
            PaymentStatus.Cancelled => "Cancelled",
            PaymentStatus.Refunded => "Refunded",
            PaymentStatus.InEscrow => "In Escrow",
            _ => status.ToString()
        };
    }
}
