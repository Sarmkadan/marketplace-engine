#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Data;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Exceptions;

namespace MarketplaceEngine.Repositories;

/// <summary>
/// In-memory repository for payment persistence and retrieval.
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly MarketplaceDbContext _context;
    private const string ResourceType = "Payment";
    private static readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentRepository"/> class.
    /// </summary>
    public PaymentRepository()
    {
        _context = MarketplaceDbContext.GetInstance();
    }

    /// <summary>
    /// Retrieves a payment by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the payment.</param>
    /// <returns>The matching payment, or <see langword="null"/> if no payment is found.</returns>
    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.FirstOrDefault(p => p.Id == id);
        }
    }

    /// <summary>
    /// Retrieves all payments.
    /// </summary>
    /// <returns>A list containing all payments.</returns>
    public async Task<List<Payment>> GetAllAsync()
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.ToList();
        }
    }

    /// <summary>
    /// Adds a payment to the repository.
    /// </summary>
    /// <param name="entity">The payment to add.</param>
    /// <returns>The added payment with its generated identifier, creation timestamp, and calculated fees.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entity"/> is <see langword="null"/>.</exception>
    public async Task<Payment> AddAsync(Payment entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CalculateFees();
        lock (_lock)
        {
            _context.Payments.Add(entity);
        }

        await Task.Delay(5);
        return entity;
    }

    /// <summary>
    /// Updates an existing payment.
    /// </summary>
    /// <param name="entity">The payment containing the updated values.</param>
    /// <returns>The updated payment.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entity"/> is <see langword="null"/>.</exception>
    /// <exception cref="ResourceNotFoundException">No payment with the specified identifier exists.</exception>
    public async Task<Payment> UpdateAsync(Payment entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        lock (_lock)
        {
            var existing = _context.Payments.FirstOrDefault(p => p.Id == entity.Id);
            if (existing is null)
                throw new ResourceNotFoundException(ResourceType, entity.Id);

            entity.UpdatedAt = DateTime.UtcNow;
            var index = _context.Payments.IndexOf(existing);
            _context.Payments[index] = entity;
        }

        await Task.Delay(5);
        return entity;
    }

    /// <summary>
    /// Deletes a payment by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the payment to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ResourceNotFoundException">No payment with the specified identifier exists.</exception>
    public async Task DeleteAsync(Guid id)
    {
        lock (_lock)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.Id == id);
            if (payment is null)
                throw new ResourceNotFoundException(ResourceType, id);

            _context.Payments.Remove(payment);
        }

        await Task.Delay(5);
    }

    /// <summary>
    /// Determines whether a payment with the specified identifier exists.
    /// </summary>
    /// <param name="id">The unique identifier of the payment.</param>
    /// <returns><see langword="true"/> if the payment exists; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> ExistsAsync(Guid id)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.Any(p => p.Id == id);
        }
    }

    /// <summary>
    /// Gets the total number of payments.
    /// </summary>
    /// <returns>The number of payments in the repository.</returns>
    public async Task<int> CountAsync()
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.Count;
        }
    }

    /// <summary>
    /// Retrieves payments made by a specified buyer.
    /// </summary>
    /// <param name="buyerId">The unique identifier of the buyer.</param>
    /// <returns>A list of payments made by the buyer.</returns>
    public async Task<List<Payment>> GetByBuyerIdAsync(Guid buyerId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.Where(p => p.BuyerId == buyerId).ToList();
        }
    }

    /// <summary>
    /// Retrieves payments received by a specified seller.
    /// </summary>
    /// <param name="sellerId">The unique identifier of the seller.</param>
    /// <returns>A list of payments received by the seller.</returns>
    public async Task<List<Payment>> GetBySellerIdAsync(Guid sellerId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.Where(p => p.SellerId == sellerId).ToList();
        }
    }

    /// <summary>
    /// Retrieves payments associated with a specified listing.
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing.</param>
    /// <returns>A list of payments associated with the listing.</returns>
    public async Task<List<Payment>> GetByListingIdAsync(Guid listingId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.Where(p => p.ListingId == listingId).ToList();
        }
    }

    /// <summary>
    /// Retrieves payments with a specified status.
    /// </summary>
    /// <param name="status">The payment status to match.</param>
    /// <returns>A list of payments with the specified status.</returns>
    public async Task<List<Payment>> GetByStatusAsync(PaymentStatus status)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.Where(p => p.Status == status).ToList();
        }
    }

    /// <summary>
    /// Retrieves a page of payments ordered from newest to oldest.
    /// </summary>
    /// <param name="pageNumber">The one-based page number. Values less than one are treated as one.</param>
    /// <param name="pageSize">The requested page size. Values are constrained to the range from 1 through 100.</param>
    /// <returns>The payments in the requested page and the total number of payments.</returns>
    public async Task<(List<Payment> items, int total)> GetPagedAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        await Task.Delay(5);
        List<Payment> all;
        lock (_lock)
        {
            all = _context.Payments
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }

        var total = all.Count;
        var items = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return (items, total);
    }

    /// <summary>
    /// Calculates the total revenue from completed payments for a specified seller.
    /// </summary>
    /// <param name="sellerId">The unique identifier of the seller.</param>
    /// <returns>The sum of the seller payouts from completed payments.</returns>
    public async Task<decimal> GetTotalRevenueAsync(Guid sellerId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments
                .Where(p => p.SellerId == sellerId && p.Status == PaymentStatus.Completed)
                .Sum(p => p.SellerPayout?.Amount ?? 0);
        }
    }
}
