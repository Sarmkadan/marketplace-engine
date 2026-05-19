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
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal PlatformFee { get; set; }
    public decimal SellerPayout { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? ExternalTransactionId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public PaymentDto() { }

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
