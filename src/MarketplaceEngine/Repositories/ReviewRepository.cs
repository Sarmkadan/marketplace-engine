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

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewRepository"/> class
    /// using the shared <see cref="MarketplaceDbContext"/> instance.
    /// </summary>
    public ReviewRepository()
    {
        _context = MarketplaceDbContext.GetInstance();
    }

    /// <summary>
    /// Gets a review by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the review.</param>
    /// <returns>The matching review, or <c>null</c> if no review with the specified identifier exists.</returns>
    public async Task<Review?> GetByIdAsync(Guid id)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Reviews.FirstOrDefault(r => r.Id == id);
        }
    }

    /// <summary>
    /// Gets all reviews.
    /// </summary>
    /// <returns>A list containing all reviews.</returns>
    public async Task<List<Review>> GetAllAsync()
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Reviews.ToList();
        }
    }

    /// <summary>
    /// Adds a new review to the repository.
    /// </summary>
    /// <param name="entity">The review to add.</param>
    /// <returns>The added review with its generated identifier and creation timestamp.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is <c>null</c>.</exception>
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

    /// <summary>
    /// Updates an existing review in the repository.
    /// </summary>
    /// <param name="entity">The review containing the updated values.</param>
    /// <returns>The updated review.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is <c>null</c>.</exception>
    /// <exception cref="ResourceNotFoundException">Thrown when no review with the specified identifier exists.</exception>
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

    /// <summary>
    /// Deletes a review by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the review to delete.</param>
    /// <exception cref="ResourceNotFoundException">Thrown when no review with the specified identifier exists.</exception>
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

    /// <summary>
    /// Determines whether a review with the specified identifier exists.
    /// </summary>
    /// <param name="id">The unique identifier of the review.</param>
    /// <returns><c>true</c> if a review with the specified identifier exists; otherwise, <c>false</c>.</returns>
    public async Task<bool> ExistsAsync(Guid id)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Reviews.Any(r => r.Id == id);
        }
    }

    /// <summary>
    /// Gets the total number of reviews in the repository.
    /// </summary>
    /// <returns>The total number of reviews.</returns>
    public async Task<int> CountAsync()
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Reviews.Count;
        }
    }

    /// <summary>
    /// Gets all reviews written by a specific reviewer.
    /// </summary>
    /// <param name="reviewerId">The unique identifier of the reviewer.</param>
    /// <returns>A list of reviews written by the specified reviewer.</returns>
    public async Task<List<Review>> GetByReviewerIdAsync(Guid reviewerId)
    {
        await Task.Delay(5);
        lock (_lock)
        {
            return _context.Reviews.Where(r => r.ReviewerId == reviewerId).ToList();
        }
    }

    /// <summary>
    /// Gets all active reviews for a specific seller, ordered by creation date descending.
    /// </summary>
    /// <param name="sellerId">The unique identifier of the seller.</param>
    /// <returns>A list of active reviews for the specified seller.</returns>
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

    /// <summary>
    /// Gets all active reviews for a specific listing, ordered by creation date descending.
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing.</param>
    /// <returns>A list of active reviews for the specified listing.</returns>
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

    /// <summary>
    /// Determines whether a review already exists for the specified transaction participants.
    /// </summary>
    /// <param name="reviewerId">The unique identifier of the reviewer.</param>
    /// <param name="sellerId">The unique identifier of the seller.</param>
    /// <param name="listingId">The unique identifier of the listing, or <c>null</c> if not applicable.</param>
    /// <returns><c>true</c> if a matching review exists; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Gets the average score of all active reviews for a specific seller.
    /// </summary>
    /// <param name="sellerId">The unique identifier of the seller.</param>
    /// <returns>The average score, or <c>0</c> if the seller has no active reviews.</returns>
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

    /// <summary>
    /// Gets a page of active reviews for a specific seller, ordered by creation date descending.
    /// </summary>
    /// <param name="sellerId">The unique identifier of the seller.</param>
    /// <param name="pageNumber">The one-based page number to retrieve. Values below 1 are clamped to 1.</param>
    /// <param name="pageSize">The number of reviews per page. Values below 1 are clamped to 20, and values above 100 are clamped to 100.</param>
    /// <returns>A tuple containing the requested page of reviews and the total number of matching reviews.</returns>
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
