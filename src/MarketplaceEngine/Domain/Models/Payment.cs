#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.ValueObjects;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Represents a payment transaction between a buyer and seller for a listing.
/// </summary>
public class Payment
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }
    public Guid BuyerId { get; set; }
    public User? Buyer { get; set; }
    public Guid SellerId { get; set; }
    public User? Seller { get; set; }
    public Money Amount { get; set; } = new Money(0);
    public Money? PlatformFee { get; set; }
    public Money? SellerPayout { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? ExternalTransactionId { get; set; }
    public string? PaymentMethod { get; set; }
    public string? FailureReason { get; set; }
    public string? RefundReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];

    // Platform fee rate (5%)
    private const decimal PlatformFeeRate = 0.05m;

    // Calculates platform fee and seller payout based on amount
    public void CalculateFees()
    {
        var fee = Math.Round(Amount.Amount * PlatformFeeRate, 2);
        PlatformFee = new Money(fee, Amount.CurrencyCode);
        SellerPayout = new Money(Amount.Amount - fee, Amount.CurrencyCode);
    }

    // Transitions the payment to Processing state
    public void StartProcessing()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot start processing a payment in '{Status}' status.");

        Status = PaymentStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    // Marks the payment as successfully completed
    public void Complete(string externalTransactionId)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Cannot complete a payment in '{Status}' status.");

        if (string.IsNullOrWhiteSpace(externalTransactionId))
            throw new ArgumentException("External transaction ID is required to complete a payment.", nameof(externalTransactionId));

        ExternalTransactionId = externalTransactionId;
        Status = PaymentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Marks the payment as failed with an optional reason
    public void Fail(string reason)
    {
        if (Status == PaymentStatus.Completed || Status == PaymentStatus.Refunded)
            throw new InvalidOperationException($"Cannot fail a payment in '{Status}' status.");

        FailureReason = reason;
        Status = PaymentStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    // Cancels a pending or processing payment
    public void Cancel()
    {
        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Cannot cancel a payment in '{Status}' status.");

        Status = PaymentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    // Issues a refund for a completed payment
    public void Refund(string reason)
    {
        if (Status != PaymentStatus.Completed && Status != PaymentStatus.InEscrow)
            throw new InvalidOperationException($"Cannot refund a payment in '{Status}' status.");

        RefundReason = reason;
        Status = PaymentStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Moves funds to escrow until the buyer confirms delivery
    public void MoveToEscrow()
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Cannot move a payment to escrow from '{Status}' status.");

        Status = PaymentStatus.InEscrow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Releases escrow funds to the seller
    public void ReleaseEscrow(string externalTransactionId)
    {
        if (Status != PaymentStatus.InEscrow)
            throw new InvalidOperationException($"Cannot release escrow for a payment in '{Status}' status.");

        ExternalTransactionId = externalTransactionId;
        Status = PaymentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
