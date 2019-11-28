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
/// In-memory repository for review persistence and retrieval.
/// </summary>
public class ReviewRepository : IReviewRepository
{
    private readonly MarketplaceDbContext _context;
    private const string ResourceType = "Review";
    private static readonly object _lock = new();

    public ReviewRepository()
    {
        _context = MarketplaceDbContext.GetInstance();
    }

    public async Task<Review?> GetByIdAsync(Guid id)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Reviews.FirstOrDefault(r => r.Id == id);
        }
    }

    public async Task<List<Review>> GetAllAsync()
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Reviews.ToList();
        }
    }

    public async Task<Review> AddAsync(Review entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        lock (_lock)
        {
            _context.Reviews.Add(entity);
        }

        await Task.Delay(5);
        return entity;
    }

    public async Task<Review> UpdateAsync(Review entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        lock (_lock)
        {
            var existing = _context.Reviews.FirstOrDefault(r => r.Id == entity.Id);
            if (existing is null)
                throw new ResourceNotFoundException(ResourceType, entity.Id);

            entity.UpdatedAt = DateTime.UtcNow;
            var index = _context.Reviews.IndexOf(existing);
            _context.Reviews[index] = entity;
        }

        await Task.Delay(5);
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        lock (_lock)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.Id == id);
            if (review is null)
                throw new ResourceNotFoundException(ResourceType, id);

            _context.Reviews.Remove(review);
        }

        await Task.Delay(5);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Reviews.Any(r => r.Id == id);
        }
    }

    public async Task<int> CountAsync()
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Reviews.Count;
        }
    }

    public async Task<List<Review>> GetByReviewerIdAsync(Guid reviewerId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Reviews.Where(r => r.ReviewerId == reviewerId).ToList();
        }
    }

    public async Task<List<Review>> GetBySellerIdAsync(Guid sellerId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Reviews
                .Where(r => r.SellerId == sellerId && r.Status == ReviewStatus.Active)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }
    }

    public async Task<List<Review>> GetByListingIdAsync(Guid listingId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Reviews
                .Where(r => r.ListingId == listingId && r.Status == ReviewStatus.Active)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }
    }

    public async Task<bool> ExistsForTransactionAsync(Guid reviewerId, Guid sellerId, Guid? listingId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Reviews.Any(r =>
                r.ReviewerId == reviewerId &&
                r.SellerId == sellerId &&
                r.ListingId == listingId);
        }
    }

    public async Task<double> GetAverageScoreAsync(Guid sellerId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            var scores = _context.Reviews
                .Where(r => r.SellerId == sellerId && r.Status == ReviewStatus.Active)
                .Select(r => r.Score)
                .ToList();

            return scores.Count == 0 ? 0 : scores.Average();
        }
    }

    public async Task<(List<Review> items, int total)> GetPagedBySellerAsync(Guid sellerId, int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        await Task.Delay(5);
        List<Review> all;
        lock (_lock)
        {
            all = _context.Reviews
                .Where(r => r.SellerId == sellerId && r.Status == ReviewStatus.Active)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        var total = all.Count;
        var items = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return (items, total);
    }
}
