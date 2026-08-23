#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Data transfer object for a payment transaction.
/// </summary>
public class PaymentDto
{
    /// <summary>
    /// Unique identifier for the payment transaction.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the listing associated with the payment.
    /// </>
    public Guid ListingId { get; set; }

    /// <summary>
    /// Identifier of the buyer who made the payment.
    /// </summary>
    public Guid BuyerId { get; set; }

    /// <summary>
    /// Identifier of the seller who received the payment.
    /// </summary>
    public Guid SellerId { get; set; }

    /// <summary>
    /// Amount of the payment transaction.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code for the payment (e.g., USD, EUR).
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Platform fee charged for the transaction.
    /// </summary>
    public decimal PlatformFee { get; set; }

    /// <summary>
    /// Amount paid out to the seller after platform fees.
    /// </summary>
    public decimal SellerPayout { get; set; }

    /// <summary>
    /// Current status of the payment (e.g., Pending, Completed, Failed).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Payment method used for the transaction (nullable).
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// External transaction ID from the payment provider (nullable).
    /// </summary>
    public string? ExternalTransactionId { get; set; }

    /// <summary>
    /// Reason for payment failure (nullable).
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Date and time when the payment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the payment was completed (nullable).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Initializes a new instance of the PaymentDto class.
    /// </summary>
    public PaymentDto() { }

    /// <summary>
    /// Initializes a new instance of the PaymentDto class from a Payment domain model.
    /// </summary>
    /// <param name="payment">The Payment domain model to convert from.</param>
    public PaymentDto(Payment payment)
    {
        Id = payment.Id;
        ListingId = payment.ListingId;
        BuyerId = payment.BuyerId;
        SellerId = payment.SellerId;
        Amount = payment.Amount.Amount;
        Currency = payment.Amount.CurrencyCode;
        PlatformFee = payment.PlatformFee?.Amount ?? 0;
        SellerPayout = payment.SellerPayout?.Amount ?? 0;
        Status = payment.Status.ToString();
        PaymentMethod = payment.PaymentMethod;
        ExternalTransactionId = payment.ExternalTransactionId;
        FailureReason = payment.FailureReason;
        CreatedAt = payment.CreatedAt;
        CompletedAt = payment.CompletedAt;
    }
}

/// <summary>
/// Request DTO for initiating a new payment.
/// </summary>
public class InitiatePaymentRequest
{
    public Guid ListingId { get; set; }
    public Guid BuyerId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
}

/// <summary>
/// Request DTO for completing a payment after provider confirmation.
/// </summary>
public class CompletePaymentRequest
{
    public string ExternalTransactionId { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for refunding a completed payment.
/// </summary>
public class RefundPaymentRequest
{
    public string Reason { get; set; } = string.Empty;
}
