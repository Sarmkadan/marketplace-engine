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

    public PaymentRepository()
    {
        _context = MarketplaceDbContext.GetInstance();
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.FirstOrDefault(p => p.Id == id);
        }
    }

    public async Task<List<Payment>> GetAllAsync()
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.ToList();
        }
    }

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

    public async Task<bool> ExistsAsync(Guid id)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.Any(p => p.Id == id);
        }
    }

    public async Task<int> CountAsync()
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.Count;
        }
    }

    public async Task<List<Payment>> GetByBuyerIdAsync(Guid buyerId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.Where(p => p.BuyerId == buyerId).ToList();
        }
    }

    public async Task<List<Payment>> GetBySellerIdAsync(Guid sellerId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.Where(p => p.SellerId == sellerId).ToList();
        }
    }

    public async Task<List<Payment>> GetByListingIdAsync(Guid listingId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.Where(p => p.ListingId == listingId).ToList();
        }
    }

    public async Task<List<Payment>> GetByStatusAsync(PaymentStatus status)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Payments.Where(p => p.Status == status).ToList();
        }
    }

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
