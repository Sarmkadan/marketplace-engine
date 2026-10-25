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
/// Manages the full lifecycle of payment transactions in the marketplace.
/// Integrates with external payment providers via an abstraction layer,
/// allowing real provider implementations to be swapped in without
/// changing business logic.
/// </summary>
public class PaymentService
{
    // Default currency for new payments
    private const string DefaultCurrencyCode = "USD";

    // Resource type names used in ResourceNotFoundException
    private const string PaymentResourceType = "Payment";
    private const string ListingResourceType = "Listing";
    private const string UserResourceType = "User";

    // Parameter names used in ValidationException
    private const string PaymentMethodParam = "PaymentMethod";
    private const string ReasonParam = "Reason";

    // Action descriptions used in UnauthorizedException
    private const string MakePaymentsAction = "make payments";
    private const string ActOnPaymentAction = "act on this payment";

    // Validation messages
    private const string PaymentMethodRequiredMessage = "Payment method is required.";
    private const string RefundReasonRequiredMessage = "Refund reason is required.";
    private const string ListingNoPriceMessage = "Listing has no price set.";
    private const string SellerSelfPurchaseMessage = "A seller cannot purchase their own listing.";

    // Log message templates
    private const string PaymentInitiatedLogMessage =
        "Payment initiated: {PaymentId}, listing {ListingId}, buyer {BuyerId}, amount {Amount} {Currency}";
    private const string PaymentCompletedLogMessage =
        "Payment completed: {PaymentId}, external transaction {ExternalTransactionId}";
    private const string PaymentEscrowLogMessage = "Payment moved to escrow: {PaymentId}";
    private const string PaymentEscrowReleasedLogMessage = "Payment escrow released: {PaymentId}";
    private const string PaymentFailedLogMessage = "Payment failed: {PaymentId}, reason {Reason}";
    private const string PaymentCancelledLogMessage = "Payment cancelled: {PaymentId}";
    private const string PaymentRefundedLogMessage = "Payment refunded: {PaymentId}";

    private readonly IPaymentRepository _paymentRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IListingRepository listingRepository,
        IUserRepository userRepository)
        : this(paymentRepository, listingRepository, userRepository, NullLogger<PaymentService>.Instance)
    {
    }

    public PaymentService(
        IPaymentRepository paymentRepository,
        IListingRepository listingRepository,
        IUserRepository userRepository,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initiates a new payment for a listing purchase.
    /// Validates that the listing is active and the buyer is not the seller.
    /// </summary>
    public async Task<Payment> InitiatePaymentAsync(
        Guid listingId,
        Guid buyerId,
        string paymentMethod,
        string currency = DefaultCurrencyCode)
    {
        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new ValidationException(PaymentMethodParam, PaymentMethodRequiredMessage);

        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is null)
            throw new ResourceNotFoundException(ListingResourceType, listingId);

        if (listing.Status != ListingStatus.Active)
            throw new MarketplaceException($"Listing '{listingId}' is not available for purchase.");

        if (listing.Price is null)
            throw new MarketplaceException(ListingNoPriceMessage);

        if (!string.IsNullOrEmpty(currency) &&
            !currency.Equals(listing.Price.CurrencyCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new MarketplaceException(
                $"Payment currency '{currency}' does not match listing currency '{listing.Price.CurrencyCode}'.");
        }

        var buyer = await _userRepository.GetByIdAsync(buyerId);
        if (buyer is null)
            throw new ResourceNotFoundException(UserResourceType, buyerId);

        if (!buyer.IsActive)
            throw new UnauthorizedException(buyerId, MakePaymentsAction);

        if (listing.SellerId == buyerId)
            throw new MarketplaceException(SellerSelfPurchaseMessage);

        var amount = new Money(listing.Price.Amount, listing.Price.CurrencyCode);
        var payment = new Payment
        {
            ListingId = listingId,
            BuyerId = buyerId,
            SellerId = listing.SellerId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            Status = PaymentStatus.Pending
        };

        var createdPayment = await _paymentRepository.AddAsync(payment);
        _logger.LogInformation(
            PaymentInitiatedLogMessage,
            createdPayment.Id,
            createdPayment.ListingId,
            createdPayment.BuyerId,
            createdPayment.Amount.Amount,
            createdPayment.Amount.CurrencyCode);
        return createdPayment;
    }

    /// <summary>
    /// Transitions a pending payment to processing state (call before provider charge).
    /// </summary>
    public async Task<Payment> StartProcessingAsync(Guid paymentId, Guid requesterId)
    {
        var payment = await GetPaymentOrThrowAsync(paymentId);
        EnsureIsBuyer(payment, requesterId);

        payment.StartProcessing();
        return await _paymentRepository.UpdateAsync(payment);
    }

    /// <summary>
    /// Completes a payment after the external provider confirms the charge.
    /// Marks the listing as delisted and increments the seller's sale count.
    /// </summary>
    public async Task<Payment> CompletePaymentAsync(Guid paymentId, string externalTransactionId)
    {
        ArgumentNullException.ThrowIfNull(externalTransactionId);
        var payment = await GetPaymentOrThrowAsync(paymentId);
        payment.Complete(externalTransactionId);
        await _paymentRepository.UpdateAsync(payment);

        // Mark listing as sold
        var listing = await _listingRepository.GetByIdAsync(payment.ListingId);
        if (listing is not null)
        {
            listing.Delist();
            await _listingRepository.UpdateAsync(listing);
        }

        // Record sale on seller profile
        var seller = await _userRepository.GetByIdAsync(payment.SellerId);
        if (seller is not null)
        {
            seller.RecordSale();
            await _userRepository.UpdateAsync(seller);
        }

        _logger.LogInformation(
            PaymentCompletedLogMessage,
            payment.Id,
            externalTransactionId);
        return payment;
    }

    /// <summary>
    /// Moves a processing payment into escrow until delivery is confirmed.
    /// </summary>
    public async Task<Payment> MoveToEscrowAsync(Guid paymentId)
    {
        var payment = await GetPaymentOrThrowAsync(paymentId);
        payment.MoveToEscrow();
        var updatedPayment = await _paymentRepository.UpdateAsync(payment);
        _logger.LogInformation(PaymentEscrowLogMessage, updatedPayment.Id);
        return updatedPayment;
    }

    /// <summary>
    /// Releases escrowed funds to the seller after buyer delivery confirmation.
    /// </summary>
    public async Task<Payment> ReleaseEscrowAsync(Guid paymentId, string externalTransactionId)
    {
        ArgumentNullException.ThrowIfNull(externalTransactionId);
        var payment = await GetPaymentOrThrowAsync(paymentId);
        payment.ReleaseEscrow(externalTransactionId);

        var seller = await _userRepository.GetByIdAsync(payment.SellerId);
        if (seller is not null)
        {
            seller.RecordSale();
            await _userRepository.UpdateAsync(seller);
        }

        var updatedPayment = await _paymentRepository.UpdateAsync(payment);
        _logger.LogInformation(PaymentEscrowReleasedLogMessage, updatedPayment.Id);
        return updatedPayment;
    }

    /// <summary>
    /// Marks a payment as failed with a descriptive reason.
    /// </summary>
    public async Task<Payment> FailPaymentAsync(Guid paymentId, string reason)
    {
        ArgumentNullException.ThrowIfNull(reason);
        var payment = await GetPaymentOrThrowAsync(paymentId);
        payment.Fail(reason);
        var updatedPayment = await _paymentRepository.UpdateAsync(payment);
        _logger.LogWarning(PaymentFailedLogMessage, updatedPayment.Id, reason);
        return updatedPayment;
    }

    /// <summary>
    /// Cancels a pending or processing payment.
    /// </summary>
    public async Task<Payment> CancelPaymentAsync(Guid paymentId, Guid requesterId)
    {
        var payment = await GetPaymentOrThrowAsync(paymentId);
        EnsureIsBuyer(payment, requesterId);
        payment.Cancel();
        var updatedPayment = await _paymentRepository.UpdateAsync(payment);
        _logger.LogInformation(PaymentCancelledLogMessage, updatedPayment.Id);
        return updatedPayment;
    }

    /// <summary>
    /// Refunds a completed payment to the buyer.
    /// </summary>
    public async Task<Payment> RefundPaymentAsync(Guid paymentId, string reason)
    {
        ArgumentNullException.ThrowIfNull(reason);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ValidationException(ReasonParam, RefundReasonRequiredMessage);

        var payment = await GetPaymentOrThrowAsync(paymentId);
        payment.Refund(reason);
        var updatedPayment = await _paymentRepository.UpdateAsync(payment);
        _logger.LogInformation(PaymentRefundedLogMessage, updatedPayment.Id);
        return updatedPayment;
    }

    /// <summary>
    /// Retrieves a payment by ID.
    /// </summary>
    public async Task<Payment> GetPaymentAsync(Guid paymentId)
    {
        return await GetPaymentOrThrowAsync(paymentId);
    }

    /// <summary>
    /// Retrieves all payments made by a specific buyer.
    /// </summary>
    public async Task<List<Payment>> GetBuyerPaymentsAsync(Guid buyerId)
    {
        var buyer = await _userRepository.GetByIdAsync(buyerId);
        if (buyer is null)
            throw new ResourceNotFoundException(UserResourceType, buyerId);

        return await _paymentRepository.GetByBuyerIdAsync(buyerId);
    }

    /// <summary>
    /// Retrieves all payments received by a specific seller.
    /// </summary>
    public async Task<List<Payment>> GetSellerPaymentsAsync(Guid sellerId)
    {
        var seller = await _userRepository.GetByIdAsync(sellerId);
        if (seller is null)
            throw new ResourceNotFoundException(UserResourceType, sellerId);

        return await _paymentRepository.GetBySellerIdAsync(sellerId);
    }

    /// <summary>
    /// Retrieves total net revenue earned by a seller.
    /// </summary>
    public async Task<decimal> GetSellerRevenueAsync(Guid sellerId)
    {
        return await _paymentRepository.GetTotalRevenueAsync(sellerId);
    }

    private async Task<Payment> GetPaymentOrThrowAsync(Guid paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment is null)
            throw new ResourceNotFoundException(PaymentResourceType, paymentId);

        return payment;
    }

    private static void EnsureIsBuyer(Payment payment, Guid requesterId)
    {
        if (payment.BuyerId != requesterId)
            throw new UnauthorizedException(requesterId, ActOnPaymentAction);
    }
}
