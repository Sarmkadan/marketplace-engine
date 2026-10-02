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
/// Orchestrates order lifecycle from placement through fulfillment,
/// coordinating between payment processing, inventory checks, and seller notifications.
/// </summary>
public class OrderProcessor : IDisposable
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<OrderProcessor> _logger;
    private readonly SemaphoreSlim _processingLock = new(1, 1);
    private bool _disposed;

    public OrderProcessor(
        IPaymentRepository paymentRepository,
        IListingRepository listingRepository,
        IUserRepository userRepository)
        : this(paymentRepository, listingRepository, userRepository, NullLogger<OrderProcessor>.Instance)
    {
    }

    public OrderProcessor(
        IPaymentRepository paymentRepository,
        IListingRepository listingRepository,
        IUserRepository userRepository,
        ILogger<OrderProcessor> logger)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Places a new order for a listing, validating buyer eligibility and listing availability.
    /// </summary>
    /// <param name="listingId">The listing to purchase.</param>
    /// <param name="buyerId">The buyer placing the order.</param>
    /// <param name="paymentMethod">Payment method identifier (e.g. "card", "paypal").</param>
    /// <returns>The created payment representing the order.</returns>
    /// <exception cref="MarketplaceException">Thrown when listing is unavailable or buyer is the seller.</exception>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid.</exception>
    public async Task<Payment> PlaceOrderAsync(Guid listingId, Guid buyerId, string paymentMethod)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Validate input parameters
        if (listingId == Guid.Empty)
        {
            _logger.LogWarning("Invalid listing ID: {ListingId}", listingId);
            throw new ArgumentException("Listing ID cannot be empty.", nameof(listingId));
        }

        if (buyerId == Guid.Empty)
        {
            _logger.LogWarning("Invalid buyer ID: {BuyerId}", buyerId);
            throw new ArgumentException("Buyer ID cannot be empty.", nameof(buyerId));
        }

        if (string.IsNullOrWhiteSpace(paymentMethod))
        {
            _logger.LogWarning("Invalid payment method: {PaymentMethod}", paymentMethod);
            throw new ArgumentException("Payment method cannot be null or empty.", nameof(paymentMethod));
        }

        await _processingLock.WaitAsync();
        try
        {
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing is null)
            {
                _logger.LogError("Listing {ListingId} not found", listingId);
                throw new MarketplaceException($"Listing {listingId} not found.");
            }

            if (listing.Status != ListingStatus.Active)
            {
                _logger.LogWarning("Listing {ListingId} is not available for purchase (status: {Status})", listingId, listing.Status);
                throw new MarketplaceException($"Listing {listingId} is not available for purchase (status: {listing.Status}).");
            }

            if (listing.SellerId == buyerId)
            {
                _logger.LogWarning("Buyer {BuyerId} cannot purchase their own listing {ListingId}", buyerId, listingId);
                throw new MarketplaceException("Buyer cannot purchase their own listing.");
            }

            var buyer = await _userRepository.GetByIdAsync(buyerId);
            if (buyer is null)
            {
                _logger.LogError("Buyer {BuyerId} not found", buyerId);
                throw new MarketplaceException($"Buyer {buyerId} not found.");
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                ListingId = listingId,
                BuyerId = buyerId,
                SellerId = listing.SellerId,
                Amount = listing.Price ?? new Money(0),
                Status = PaymentStatus.Pending,
                PaymentMethod = paymentMethod,
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);
            _logger.LogInformation("Order placed: Payment {PaymentId} for Listing {ListingId} by Buyer {BuyerId}",
                payment.Id, listingId, buyerId);

            return payment;
        }
        finally
        {
            _processingLock.Release();
        }
    }

    /// <summary>
    /// Confirms an order after successful payment, updating listing status to delisted.
    /// </summary>
    /// <param name="paymentId">The payment/order to confirm.</param>
    /// <returns>The updated payment with completed status.</returns>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid.</exception>
    public async Task<Payment> ConfirmOrderAsync(Guid paymentId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Validate input parameters
        if (paymentId == Guid.Empty)
        {
            _logger.LogWarning("Invalid payment ID: {PaymentId}", paymentId);
            throw new ArgumentException("Payment ID cannot be empty.", nameof(paymentId));
        }

        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment is null)
        {
            _logger.LogError("Payment {PaymentId} not found", paymentId);
            throw new MarketplaceException($"Payment {paymentId} not found.");
        }

        if (payment.Status != PaymentStatus.Pending && payment.Status != PaymentStatus.Processing)
        {
            _logger.LogWarning("Payment {PaymentId} cannot be confirmed (status: {Status})", paymentId, payment.Status);
            throw new MarketplaceException($"Payment {paymentId} cannot be confirmed (status: {payment.Status}).");
        }

        using (_logger.BeginScope("PaymentId: {PaymentId}", paymentId))
        {
            payment.Status = PaymentStatus.Completed;
            payment.CompletedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            var listing = await _listingRepository.GetByIdAsync(payment.ListingId);
            if (listing is not null)
            {
                listing.Status = ListingStatus.Delisted;
                await _listingRepository.UpdateAsync(listing);
                _logger.LogInformation("Listing {ListingId} status updated to Delisted", listing.Id);
            }

            await _paymentRepository.UpdateAsync(payment);
            _logger.LogInformation("Order confirmed: Payment {PaymentId} completed", paymentId);
        }

        return payment;
    }

    /// <summary>
    /// Cancels a pending order, returning the listing to active status.
    /// </summary>
    /// <param name="paymentId">The payment/order to cancel.</param>
    /// <param name="reason">Reason for cancellation.</param>
    /// <returns>The updated payment with cancelled status.</returns>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid.</exception>
    public async Task<Payment> CancelOrderAsync(Guid paymentId, string reason)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Validate input parameters
        if (paymentId == Guid.Empty)
        {
            _logger.LogWarning("Invalid payment ID: {PaymentId}", paymentId);
            throw new ArgumentException("Payment ID cannot be empty.", nameof(paymentId));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            _logger.LogWarning("Invalid cancellation reason: {Reason}", reason);
            throw new ArgumentException("Cancellation reason cannot be null or empty.", nameof(reason));
        }

        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment is null)
        {
            _logger.LogError("Payment {PaymentId} not found", paymentId);
            throw new MarketplaceException($"Payment {paymentId} not found.");
        }

        if (payment.Status == PaymentStatus.Completed)
        {
            _logger.LogWarning("Completed orders must be refunded, not cancelled. PaymentId: {PaymentId}", paymentId);
            throw new MarketplaceException("Completed orders must be refunded, not cancelled.");
        }

        if (payment.Status == PaymentStatus.Cancelled)
        {
            _logger.LogWarning("Order is already cancelled. PaymentId: {PaymentId}", paymentId);
            throw new MarketplaceException("Order is already cancelled.");
        }

        using (_logger.BeginScope("PaymentId: {PaymentId}", paymentId))
        {
            payment.Status = PaymentStatus.Cancelled;
            payment.FailureReason = reason;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);
            _logger.LogInformation("Order cancelled: Payment {PaymentId}, reason: {Reason}", paymentId, reason);
        }

        return payment;
    }

    /// <summary>
    /// Retrieves all orders for a specific buyer, sorted by creation date descending.
    /// </summary>
    /// <param name="buyerId">The buyer whose orders to retrieve.</param>
    /// <returns>List of payments representing the buyer's orders.</returns>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid.</exception>
    public async Task<IReadOnlyList<Payment>> GetOrdersByBuyerAsync(Guid buyerId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Validate input parameters
        if (buyerId == Guid.Empty)
        {
            _logger.LogWarning("Invalid buyer ID: {BuyerId}", buyerId);
            throw new ArgumentException("Buyer ID cannot be empty.", nameof(buyerId));
        }

        var allPayments = await _paymentRepository.GetAllAsync();
        var orders = allPayments
            .Where(p => p.BuyerId == buyerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToList()
            .AsReadOnly();

        _logger.LogInformation("Retrieved {Count} orders for buyer {BuyerId}", orders.Count, buyerId);
        return orders;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _processingLock.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
