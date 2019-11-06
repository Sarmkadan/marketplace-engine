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

namespace MarketplaceEngine.Services;

/// <summary>
/// Manages the full lifecycle of payment transactions in the marketplace.
/// Integrates with external payment providers via an abstraction layer,
/// allowing real provider implementations to be swapped in without
/// changing business logic.
/// </summary>
public class PaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IUserRepository _userRepository;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IListingRepository listingRepository,
        IUserRepository userRepository)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Initiates a new payment for a listing purchase.
    /// Validates that the listing is active and the buyer is not the seller.
    /// </summary>
    public async Task<Payment> InitiatePaymentAsync(
        Guid listingId,
        Guid buyerId,
        string paymentMethod,
        string currency = "USD")
    {
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        if (listing.Status != ListingStatus.Active)
            throw new MarketplaceException($"Listing '{listingId}' is not available for purchase.");

        if (listing.Price is null)
            throw new MarketplaceException("Listing has no price set.");

        var buyer = await _userRepository.GetByIdAsync(buyerId);
        if (buyer is null)
            throw new ResourceNotFoundException("User", buyerId);

        if (!buyer.IsActive)
            throw new UnauthorizedException(buyerId, "make payments");

        if (listing.SellerId == buyerId)
            throw new MarketplaceException("A seller cannot purchase their own listing.");

        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new Exceptions.ValidationException("PaymentMethod", "Payment method is required.");

        var amount = new Money(listing.Price.Amount, currency);
        var payment = new Payment
        {
            ListingId = listingId,
            BuyerId = buyerId,
            SellerId = listing.SellerId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            Status = PaymentStatus.Pending
        };

        return await _paymentRepository.AddAsync(payment);
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

        return payment;
    }

    /// <summary>
    /// Moves a processing payment into escrow until delivery is confirmed.
    /// </summary>
    public async Task<Payment> MoveToEscrowAsync(Guid paymentId)
    {
        var payment = await GetPaymentOrThrowAsync(paymentId);
        payment.MoveToEscrow();
        return await _paymentRepository.UpdateAsync(payment);
    }

    /// <summary>
    /// Releases escrowed funds to the seller after buyer delivery confirmation.
    /// </summary>
    public async Task<Payment> ReleaseEscrowAsync(Guid paymentId, string externalTransactionId)
    {
        var payment = await GetPaymentOrThrowAsync(paymentId);
        payment.ReleaseEscrow(externalTransactionId);

        var seller = await _userRepository.GetByIdAsync(payment.SellerId);
        if (seller is not null)
        {
            seller.RecordSale();
            await _userRepository.UpdateAsync(seller);
        }

        return await _paymentRepository.UpdateAsync(payment);
    }

    /// <summary>
    /// Marks a payment as failed with a descriptive reason.
    /// </summary>
    public async Task<Payment> FailPaymentAsync(Guid paymentId, string reason)
    {
        var payment = await GetPaymentOrThrowAsync(paymentId);
        payment.Fail(reason);
        return await _paymentRepository.UpdateAsync(payment);
    }

    /// <summary>
    /// Cancels a pending or processing payment.
    /// </summary>
    public async Task<Payment> CancelPaymentAsync(Guid paymentId, Guid requesterId)
    {
        var payment = await GetPaymentOrThrowAsync(paymentId);
        EnsureIsBuyer(payment, requesterId);
        payment.Cancel();
        return await _paymentRepository.UpdateAsync(payment);
    }

    /// <summary>
    /// Refunds a completed payment to the buyer.
    /// </summary>
    public async Task<Payment> RefundPaymentAsync(Guid paymentId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new Exceptions.ValidationException("Reason", "Refund reason is required.");

        var payment = await GetPaymentOrThrowAsync(paymentId);
        payment.Refund(reason);
        return await _paymentRepository.UpdateAsync(payment);
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
            throw new ResourceNotFoundException("User", buyerId);

        return await _paymentRepository.GetByBuyerIdAsync(buyerId);
    }

    /// <summary>
    /// Retrieves all payments received by a specific seller.
    /// </summary>
    public async Task<List<Payment>> GetSellerPaymentsAsync(Guid sellerId)
    {
        var seller = await _userRepository.GetByIdAsync(sellerId);
        if (seller is null)
            throw new ResourceNotFoundException("User", sellerId);

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
            throw new ResourceNotFoundException("Payment", paymentId);

        return payment;
    }

    private static void EnsureIsBuyer(Payment payment, Guid requesterId)
    {
        if (payment.BuyerId != requesterId)
            throw new UnauthorizedException(requesterId, "act on this payment");
    }
}
