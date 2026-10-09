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

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewService"/> class.
    /// </summary>
    /// <param name="reviewRepository">Repository used to persist and query reviews.</param>
    /// <param name="userRepository">Repository used to look up reviewers, sellers, and moderators.</param>
    /// <param name="listingRepository">Repository used to validate listing references.</param>
    /// <exception cref="ArgumentNullException">Thrown when any repository argument is <c>null</c>.</exception>
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
    /// <param name="reviewerId">ID of the user submitting the review.</param>
    /// <param name="sellerId">ID of the seller being reviewed.</param>
    /// <param name="score">Rating score for the review.</param>
    /// <param name="comment">Review text. Must not be <c>null</c>.</param>
    /// <param name="listingId">Optional ID of the listing the review relates to.</param>
    /// <returns>The created review.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="comment"/> is <c>null</c>.</exception>
    /// <exception cref="ResourceNotFoundException">Thrown when the reviewer, seller, or listing does not exist.</exception>
    /// <exception cref="UnauthorizedException">Thrown when the reviewer is not active.</exception>
    /// <exception cref="MarketplaceException">Thrown when a seller attempts to review themselves.</exception>
    /// <exception cref="DuplicateResourceException">Thrown when a review for this reviewer, seller, and listing already exists.</exception>
    public async Task<Review> SubmitReviewAsync(
        Guid reviewerId,
        Guid sellerId,
        int score,
        string comment,
        Guid? listingId = null)
    {
        ArgumentNullException.ThrowIfNull(comment);

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
    /// <param name="reviewId">ID of the review to reply to.</param>
    /// <param name="sellerId">ID of the seller attempting to reply.</param>
    /// <param name="reply">Reply text. Must not be <c>null</c>.</param>
    /// <returns>The updated review.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="reply"/> is <c>null</c>.</exception>
    /// <exception cref="ResourceNotFoundException">Thrown when the review does not exist.</exception>
    /// <exception cref="UnauthorizedException">Thrown when <paramref name="sellerId"/> is not the seller of the review.</exception>
    public async Task<Review> AddSellerReplyAsync(Guid reviewId, Guid sellerId, string reply)
    {
        ArgumentNullException.ThrowIfNull(reply);

        var review = await GetReviewOrThrowAsync(reviewId);

        if (review.SellerId != sellerId)
            throw new UnauthorizedException(sellerId, "reply to this review");

        review.AddSellerReply(reply);
        return await _reviewRepository.UpdateAsync(review);
    }

    /// <summary>
    /// Retrieves a review by its ID.
    /// </summary>
    /// <param name="reviewId">ID of the review to retrieve.</param>
    /// <returns>The matching review.</returns>
    /// <exception cref="ResourceNotFoundException">Thrown when the review does not exist.</exception>
    public async Task<Review> GetReviewAsync(Guid reviewId)
    {
        return await GetReviewOrThrowAsync(reviewId);
    }

    /// <summary>
    /// Retrieves paginated reviews for a seller.
    /// </summary>
    /// <param name="sellerId">ID of the seller whose reviews are requested.</param>
    /// <param name="pageNumber">One-based page number. Defaults to 1.</param>
    /// <param name="pageSize">Number of reviews per page. Defaults to 20.</param>
    /// <returns>A tuple containing the reviews on the requested page and the total review count.</returns>
    /// <exception cref="ResourceNotFoundException">Thrown when the seller does not exist.</exception>
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
    /// <param name="listingId">ID of the listing whose reviews are requested.</param>
    /// <returns>All reviews associated with the listing.</returns>
    /// <exception cref="ResourceNotFoundException">Thrown when the listing does not exist.</exception>
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
    /// <param name="sellerId">ID of the seller whose statistics are requested.</param>
    /// <returns>
    /// A tuple containing the average score (rounded to two decimals, 0 when there are no reviews),
    /// the total review count, and a distribution map from each score (1-5) to its number of reviews.
    /// </returns>
    /// <exception cref="ResourceNotFoundException">Thrown when the seller does not exist.</exception>
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
    /// <param name="reviewId">ID of the review to flag.</param>
    /// <returns>The updated review.</returns>
    /// <exception cref="ResourceNotFoundException">Thrown when the review does not exist.</exception>
    public async Task<Review> FlagReviewAsync(Guid reviewId)
    {
        var review = await GetReviewOrThrowAsync(reviewId);
        review.FlagForReview();
        return await _reviewRepository.UpdateAsync(review);
    }

    /// <summary>
    /// Removes a review (moderator action). Updates the seller's rating afterwards.
    /// </summary>
    /// <param name="reviewId">ID of the review to remove.</param>
    /// <param name="moderatorId">ID of the user performing the removal. Must be a moderator or administrator.</param>
    /// <returns>The updated review.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the user does not exist or lacks the moderator or administrator role.</exception>
    /// <exception cref="ResourceNotFoundException">Thrown when the review does not exist.</exception>
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

    /// <summary>
    /// Recalculates and persists the seller's aggregate rating based on current reviews.
    /// Does nothing when the seller has no reviews.
    /// </summary>
    /// <param name="sellerId">ID of the seller whose rating is recalculated.</param>
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

    /// <summary>
    /// Loads a review by ID or throws if it does not exist.
    /// </summary>
    /// <param name="reviewId">ID of the review to load.</param>
    /// <returns>The matching review.</returns>
    /// <exception cref="ResourceNotFoundException">Thrown when the review does not exist.</exception>
    private async Task<Review> GetReviewOrThrowAsync(Guid reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review is null)
            throw new ResourceNotFoundException("Review", reviewId);

        return review;
    }
}
