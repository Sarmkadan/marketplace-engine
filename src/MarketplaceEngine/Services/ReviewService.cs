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
/// Manages the lifecycle of buyer reviews for sellers and listings.
/// Enforces one-review-per-transaction rules, computes aggregate scores,
/// and keeps seller ratings in sync.
/// </summary>
public class ReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUserRepository _userRepository;
    private readonly IListingRepository _listingRepository;

    public ReviewService(
        IReviewRepository reviewRepository,
        IUserRepository userRepository,
        IListingRepository listingRepository)
    {
        _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
    }

    /// <summary>
    /// Submits a new review for a seller. Validates that the reviewer exists, is active,
    /// is not reviewing themselves, and has not already reviewed this seller/listing.
    /// Updates the seller's aggregate rating after submission.
    /// </summary>
    public async Task<Review> SubmitReviewAsync(
        Guid reviewerId,
        Guid sellerId,
        int score,
        string comment,
        Guid? listingId = null)
    {
        var reviewer = await _userRepository.GetByIdAsync(reviewerId);
        if (reviewer is null)
            throw new ResourceNotFoundException("User", reviewerId);

        if (!reviewer.IsActive)
            throw new UnauthorizedException(reviewerId, "submit reviews");

        var seller = await _userRepository.GetByIdAsync(sellerId);
        if (seller is null)
            throw new ResourceNotFoundException("User", sellerId);

        if (reviewerId == sellerId)
            throw new MarketplaceException("A seller cannot review themselves.");

        if (listingId.HasValue)
        {
            var listing = await _listingRepository.GetByIdAsync(listingId.Value);
            if (listing is null)
                throw new ResourceNotFoundException("Listing", listingId.Value);
        }

        var alreadyReviewed = await _reviewRepository.ExistsForTransactionAsync(reviewerId, sellerId, listingId);
        if (alreadyReviewed)
            throw new DuplicateResourceException("Review", "reviewer+seller+listing", "this combination");

        var review = new Review
        {
            ReviewerId = reviewerId,
            SellerId = sellerId,
            ListingId = listingId,
            Score = score,
            Comment = comment,
            Status = ReviewStatus.Active
        };

        review.ValidateReview();
        var created = await _reviewRepository.AddAsync(review);

        await UpdateSellerRatingAsync(sellerId);

        return created;
    }

    /// <summary>
    /// Adds a seller reply to an existing review. Only the seller of the listing may reply.
    /// </summary>
    public async Task<Review> AddSellerReplyAsync(Guid reviewId, Guid sellerId, string reply)
    {
        var review = await GetReviewOrThrowAsync(reviewId);

        if (review.SellerId != sellerId)
            throw new UnauthorizedException(sellerId, "reply to this review");

        review.AddSellerReply(reply);
        return await _reviewRepository.UpdateAsync(review);
    }

    /// <summary>
    /// Retrieves a review by its ID.
    /// </summary>
    public async Task<Review> GetReviewAsync(Guid reviewId)
    {
        return await GetReviewOrThrowAsync(reviewId);
    }

    /// <summary>
    /// Retrieves paginated reviews for a seller.
    /// </summary>
    public async Task<(List<Review> items, int total)> GetSellerReviewsAsync(
        Guid sellerId, int pageNumber = 1, int pageSize = 20)
    {
        var seller = await _userRepository.GetByIdAsync(sellerId);
        if (seller is null)
            throw new ResourceNotFoundException("User", sellerId);

        return await _reviewRepository.GetPagedBySellerAsync(sellerId, pageNumber, pageSize);
    }

    /// <summary>
    /// Retrieves all reviews for a specific listing.
    /// </summary>
    public async Task<List<Review>> GetListingReviewsAsync(Guid listingId)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        return await _reviewRepository.GetByListingIdAsync(listingId);
    }

    /// <summary>
    /// Returns aggregated review statistics for a seller.
    /// </summary>
    public async Task<(double averageScore, int total, Dictionary<int, int> distribution)> GetSellerStatsAsync(Guid sellerId)
    {
        var seller = await _userRepository.GetByIdAsync(sellerId);
        if (seller is null)
            throw new ResourceNotFoundException("User", sellerId);

        var reviews = await _reviewRepository.GetBySellerIdAsync(sellerId);
        var average = reviews.Count == 0 ? 0.0 : reviews.Average(r => r.Score);
        var distribution = Enumerable.Range(1, 5)
            .ToDictionary(s => s, s => reviews.Count(r => r.Score == s));

        return (Math.Round(average, 2), reviews.Count, distribution);
    }

    /// <summary>
    /// Flags a review for moderation.
    /// </summary>
    public async Task<Review> FlagReviewAsync(Guid reviewId)
    {
        var review = await GetReviewOrThrowAsync(reviewId);
        review.FlagForReview();
        return await _reviewRepository.UpdateAsync(review);
    }

    /// <summary>
    /// Removes a review (moderator action). Updates the seller's rating afterwards.
    /// </summary>
    public async Task<Review> RemoveReviewAsync(Guid reviewId, Guid moderatorId)
    {
        var moderator = await _userRepository.GetByIdAsync(moderatorId);
        if (moderator is null || (moderator.Role != UserRole.Moderator && moderator.Role != UserRole.Administrator))
            throw new UnauthorizedException(moderatorId, "remove reviews");

        var review = await GetReviewOrThrowAsync(reviewId);
        review.Remove();
        var updated = await _reviewRepository.UpdateAsync(review);

        await UpdateSellerRatingAsync(review.SellerId);

        return updated;
    }

    // Recalculates and persists the seller's aggregate rating based on current reviews
    private async Task UpdateSellerRatingAsync(Guid sellerId)
    {
        var reviews = await _reviewRepository.GetBySellerIdAsync(sellerId);
        if (reviews.Count == 0)
            return;

        var avgScore = (int)Math.Round(reviews.Average(r => r.Score));
        var newRating = new Rating(avgScore, reviews.Count);

        var seller = await _userRepository.GetByIdAsync(sellerId);
        if (seller is not null)
        {
            seller.UpdateRating(newRating);
            await _userRepository.UpdateAsync(seller);
        }
    }

    private async Task<Review> GetReviewOrThrowAsync(Guid reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review is null)
            throw new ResourceNotFoundException("Review", reviewId);

        return review;
    }
}
