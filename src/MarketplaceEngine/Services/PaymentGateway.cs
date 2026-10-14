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
/// Defines the contract for a payment provider that handles authorization, capture, and refund operations.
/// </summary>
public interface IPaymentProvider
{
    /// <summary>
    /// Authorizes a payment.
    /// </summary>
    /// <param name="payment">The payment to authorize.</param>
    /// <returns>The authorized payment.</returns>
    Task<Payment> AuthorizeAsync(Payment payment);

    /// <summary>
    /// Captures an authorized payment.
    /// </summary>
    /// <param name="payment">The payment to capture.</param>
    /// <returns>The captured payment.</returns>
    Task<Payment> CaptureAsync(Payment payment);

    /// <summary>
    /// Refunds a captured payment.
    /// </summary>
    /// <param name="payment">The payment to refund.</param>
    /// <param name="reason">Reason for the refund.</param>
    /// <returns>The refunded payment.</returns>
    Task<Payment> RefundAsync(Payment payment, string reason);
}

/// <summary>
/// Default payment provider that simulates success or failure based on a configured failure rate.
/// Used when no real provider is configured.
/// </summary>
public class DefaultPaymentProvider : IPaymentProvider
{
    private const string TransactionIdPrefix = "txn_";
    private const int TransactionIdLength = 24;
    private const string SimulatedDeclineReason = "Authorization declined by provider (simulated).";
    private const string SimulatedDeclineCode = "simulated_decline";
    private const double MinSimulatedFailureRate = 0.0;
    private const double MaxSimulatedFailureRate = 1.0;

    private readonly double _failureRate;

    public DefaultPaymentProvider(double failureRate = 0.0)
    {
        _failureRate = Math.Clamp(failureRate, MinSimulatedFailureRate, MaxSimulatedFailureRate);
    }

    public async Task<Payment> AuthorizeAsync(Payment payment)
    {
        if (ShouldSimulateFailure())
        {
            // Do not update payment state on failure - let caller handle retries
            throw new MarketplaceException(SimulatedDeclineReason, SimulatedDeclineCode);
        }

        payment.Status = PaymentStatus.Processing;
        payment.ExternalTransactionId = $"{TransactionIdPrefix}{Guid.NewGuid():N}"[..TransactionIdLength];
        payment.UpdatedAt = DateTime.UtcNow;
        return payment;
    }

    public async Task<Payment> CaptureAsync(Payment payment)
    {
        if (payment.Status != PaymentStatus.Processing)
            throw new MarketplaceException($"Payment must be in Processing state to capture (current state: {payment.Status}).");

        payment.Status = PaymentStatus.Completed;
        payment.CompletedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        return payment;
    }

    public async Task<Payment> RefundAsync(Payment payment, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ValidationException("Reason", "Refund reason is required.");

        if (payment.Status != PaymentStatus.Completed)
            throw new MarketplaceException($"Only completed payments can be refunded (current state: {payment.Status}).");

        payment.Status = PaymentStatus.Refunded;
        payment.RefundReason = reason;
        payment.RefundedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        return payment;
    }

    private bool ShouldSimulateFailure()
    {
        if (_failureRate <= 0.0)
            return false;

        return Random.Shared.NextDouble() < _failureRate;
    }
}

public class PaymentGateway
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentGateway> _logger;
    private readonly IPaymentProvider _provider;

    /// <summary>
    /// Creates a gateway with no logging and the default payment provider (no simulated failures).
    /// </summary>
    /// <param name="paymentRepository">Repository used to load and persist payments.</param>
    public PaymentGateway(IPaymentRepository paymentRepository)
        : this(paymentRepository, NullLogger<PaymentGateway>.Instance, new DefaultPaymentProvider())
    {
    }

    /// <summary>
    /// Creates a gateway with the given logger and the default payment provider (no simulated failures).
    /// </summary>
    /// <param name="paymentRepository">Repository used to load and persist payments.</param>
    /// <param name="logger">Logger for authorization, capture and refund events.</param>
    public PaymentGateway(IPaymentRepository paymentRepository, ILogger<PaymentGateway> logger)
        : this(paymentRepository, logger, new DefaultPaymentProvider())
    {
    }

    /// <summary>
    /// Creates a gateway with the given logger and payment provider.
    /// </summary>
    /// <param name="paymentRepository">Repository used to load and persist payments.</param>
    /// <param name="logger">Logger for authorization, capture and refund events.</param>
    /// <param name="provider">Payment provider to use for authorization, capture, and refund operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="paymentRepository"/>, <paramref name="logger"/>, or <paramref name="provider"/> is null.</exception>
    public PaymentGateway(
        IPaymentRepository paymentRepository,
        ILogger<PaymentGateway> logger,
        IPaymentProvider provider)
    {
        ArgumentNullException.ThrowIfNull(paymentRepository);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(provider);

        _paymentRepository = paymentRepository;
        _logger = logger;
        _provider = provider;
    }

    /// <summary>
    /// Authorizes a payment, transitioning it from Pending to Processing.
    /// In a real implementation this would call the provider's authorize endpoint.
    /// </summary>
    /// <param name="paymentId">The payment to authorize.</param>
    /// <returns>The authorized payment with an external transaction ID.</returns>
    /// <exception cref="MarketplaceException">Thrown when the payment is not found, not in Pending state, or authorization fails.</exception>
    public async Task<Payment> AuthorizeAsync(Guid paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment is null)
            throw new MarketplaceException($"Payment {paymentId} not found.");

        if (payment.Status != PaymentStatus.Pending)
            throw new MarketplaceException($"Payment {paymentId} cannot be authorized (status: {payment.Status}).");

        // The payment ID doubles as the idempotency key: authorization only ever moves a Pending payment forward.
        var idempotencyKey = payment.Id;
        PaymentGatewayLog.AuthorizationAttempted(_logger, paymentId, idempotencyKey);

        try
        {
            var authorizedPayment = await _provider.AuthorizeAsync(payment);
            await _paymentRepository.UpdateAsync(authorizedPayment);

            PaymentGatewayLog.AuthorizationSucceeded(_logger, paymentId, idempotencyKey, authorizedPayment.ExternalTransactionId);

            return authorizedPayment;
        }
        catch (MarketplaceException ex) when (ex.Message == "Authorization declined by provider (simulated).")
        {
            PaymentGatewayLog.AuthorizationDeclined(_logger, paymentId, idempotencyKey, ex.ErrorCode ?? "unknown");
            throw; // Re-throw the specific decline exception
        }
        catch (Exception ex)
        {
            PaymentGatewayLog.AuthorizationErrored(_logger, ex, paymentId, idempotencyKey);
            throw; // Re-throw any other exceptions
        }
    }

    /// <summary>
    /// Captures an authorized payment, completing the charge.
    /// </summary>
    /// <param name="paymentId">The payment to capture.</param>
    /// <returns>The completed payment.</returns>
    /// <exception cref="MarketplaceException">Thrown when the payment is not found or not in Processing state.</exception>
    public async Task<Payment> CaptureAsync(Guid paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment is null)
            throw new MarketplaceException($"Payment {paymentId} not found.");

        if (payment.Status != PaymentStatus.Processing)
            throw new MarketplaceException($"Payment {paymentId} must be authorized before capture (status: {payment.Status}).");

        try
        {
            var capturedPayment = await _provider.CaptureAsync(payment);
            await _paymentRepository.UpdateAsync(capturedPayment);

            PaymentGatewayLog.CaptureSucceeded(_logger, paymentId, capturedPayment.Amount.Amount, capturedPayment.Amount.CurrencyCode);

            return capturedPayment;
        }
        catch (Exception ex)
        {
            PaymentGatewayLog.CaptureFailed(_logger, ex, paymentId);
            throw;
        }
    }

    /// <summary>
    /// Processes a full refund for a completed payment.
    /// </summary>
    /// <param name="paymentId">The payment to refund.</param>
    /// <param name="reason">Reason for the refund.</param>
    /// <returns>The refunded payment.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="reason"/> is null.</exception>
    /// <exception cref="MarketplaceException">Thrown when the payment is not found or not in Completed state.</exception>
    public async Task<Payment> RefundAsync(Guid paymentId, string reason)
    {
        ArgumentNullException.ThrowIfNull(reason);

        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment is null)
            throw new MarketplaceException($"Payment {paymentId} not found.");

        if (payment.Status != PaymentStatus.Completed)
            throw new MarketplaceException($"Only completed payments can be refunded (status: {payment.Status}).");

        try
        {
            var refundedPayment = await _provider.RefundAsync(payment, reason);
            await _paymentRepository.UpdateAsync(refundedPayment);

            PaymentGatewayLog.RefundSucceeded(_logger, paymentId, reason);

            return refundedPayment;
        }
        catch (Exception ex)
        {
            PaymentGatewayLog.RefundFailed(_logger, ex, paymentId);
            throw;
        }
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
}
