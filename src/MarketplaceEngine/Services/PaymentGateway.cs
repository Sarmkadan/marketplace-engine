#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MarketplaceEngine.Services;

/// <summary>
/// Abstracts external payment provider integration, handling charge authorization,
/// capture, and refund operations. Currently uses an in-process simulation
/// that can be replaced with a real provider (Stripe, PayPal, etc.).
/// </summary>
public class PaymentGateway
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentGateway> _logger;

    /// <summary>
    /// Simulated failure rate for testing resilience (0.0 = never fail).
    /// </summary>
    private readonly double _simulatedFailureRate;

    public PaymentGateway(IPaymentRepository paymentRepository)
        : this(paymentRepository, NullLogger<PaymentGateway>.Instance, 0.0)
    {
    }

    public PaymentGateway(
        IPaymentRepository paymentRepository,
        ILogger<PaymentGateway> logger,
        double simulatedFailureRate = 0.0)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _simulatedFailureRate = Math.Clamp(simulatedFailureRate, 0.0, 1.0);
    }

    /// <summary>
    /// Authorizes a payment, transitioning it from Pending to Processing.
    /// In a real implementation this would call the provider's authorize endpoint.
    /// </summary>
    /// <param name="paymentId">The payment to authorize.</param>
    /// <returns>The authorized payment with an external transaction ID.</returns>
    public async Task<Payment> AuthorizeAsync(Guid paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment is null)
            throw new MarketplaceException($"Payment {paymentId} not found.");

        if (payment.Status != PaymentStatus.Pending)
            throw new MarketplaceException($"Payment {paymentId} cannot be authorized (status: {payment.Status}).");

        if (ShouldSimulateFailure())
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = "Authorization declined by provider (simulated).";
            payment.UpdatedAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);

            _logger.LogWarning("Payment {PaymentId} authorization failed (simulated)", paymentId);
            return payment;
        }

        payment.Status = PaymentStatus.Processing;
        payment.ExternalTransactionId = $"txn_{Guid.NewGuid():N}"[..24];
        payment.UpdatedAt = DateTime.UtcNow;
        await _paymentRepository.UpdateAsync(payment);

        _logger.LogInformation("Payment {PaymentId} authorized, txn: {TxnId}",
            paymentId, payment.ExternalTransactionId);

        return payment;
    }

    /// <summary>
    /// Captures an authorized payment, completing the charge.
    /// </summary>
    /// <param name="paymentId">The payment to capture.</param>
    /// <returns>The completed payment.</returns>
    public async Task<Payment> CaptureAsync(Guid paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment is null)
            throw new MarketplaceException($"Payment {paymentId} not found.");

        if (payment.Status != PaymentStatus.Processing)
            throw new MarketplaceException($"Payment {paymentId} must be authorized before capture (status: {payment.Status}).");

        payment.Status = PaymentStatus.Completed;
        payment.CompletedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        // Calculate platform fee and seller payout
        var feeAmount = Math.Round(payment.Amount.Amount * 0.05m, 2, MidpointRounding.AwayFromZero);
        payment.PlatformFee = new Money(feeAmount, payment.Amount.CurrencyCode);
        payment.SellerPayout = new Money(payment.Amount.Amount - feeAmount, payment.Amount.CurrencyCode);

        await _paymentRepository.UpdateAsync(payment);

        _logger.LogInformation("Payment {PaymentId} captured: {Amount} {Currency}",
            paymentId, payment.Amount.Amount, payment.Amount.CurrencyCode);

        return payment;
    }

    /// <summary>
    /// Processes a full refund for a completed payment.
    /// </summary>
    /// <param name="paymentId">The payment to refund.</param>
    /// <param name="reason">Reason for the refund.</param>
    /// <returns>The refunded payment.</returns>
    public async Task<Payment> RefundAsync(Guid paymentId, string reason)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment is null)
            throw new MarketplaceException($"Payment {paymentId} not found.");

        if (payment.Status != PaymentStatus.Completed)
            throw new MarketplaceException($"Only completed payments can be refunded (status: {payment.Status}).");

        payment.Status = PaymentStatus.Refunded;
        payment.RefundReason = reason;
        payment.RefundedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        await _paymentRepository.UpdateAsync(payment);

        _logger.LogInformation("Payment {PaymentId} refunded: {Reason}", paymentId, reason);
        return payment;
    }

    /// <summary>
    /// Checks the current status of a payment by its external transaction ID.
    /// </summary>
    /// <param name="externalTransactionId">The provider's transaction identifier.</param>
    /// <returns>The payment if found, null otherwise.</returns>
    public async Task<Payment?> GetByTransactionIdAsync(string externalTransactionId)
    {
        if (string.IsNullOrWhiteSpace(externalTransactionId))
            return null;

        var payments = await _paymentRepository.GetAllAsync();
        return payments.FirstOrDefault(p =>
            string.Equals(p.ExternalTransactionId, externalTransactionId, StringComparison.Ordinal));
    }

    private bool ShouldSimulateFailure()
    {
        if (_simulatedFailureRate <= 0.0)
            return false;

        return Random.Shared.NextDouble() < _simulatedFailureRate;
    }
}
