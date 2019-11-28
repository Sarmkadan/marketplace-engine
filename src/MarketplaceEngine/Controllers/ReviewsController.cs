#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using MarketplaceEngine.DTOs;
using MarketplaceEngine.Services;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Handles buyer review submissions, seller replies, review retrieval,
/// moderation actions, and aggregated rating statistics.
/// </summary>
[ApiController]
[Route("api/v1/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly ReviewService _reviewService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(ReviewService reviewService, ILogger<ReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    /// <summary>
    /// Submits a new review for a seller. A reviewer may only submit one review
    /// per seller per listing.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        _logger.LogInformation("Review submission by {ReviewerId} for seller {SellerId}", request.ReviewerId, request.SellerId);

        if (request.Score < 1 || request.Score > 5)
            return BadRequest("Score must be between 1 and 5.");

        if (string.IsNullOrWhiteSpace(request.Comment))
            return BadRequest("Comment is required.");

        var review = await _reviewService.SubmitReviewAsync(
            request.ReviewerId, request.SellerId, request.Score, request.Comment, request.ListingId);

        return CreatedAtAction(nameof(GetReview), new { id = review.Id }, new ReviewDto(review));
    }

    /// <summary>
    /// Retrieves a review by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReview(Guid id)
    {
        var review = await _reviewService.GetReviewAsync(id);
        return Ok(new ReviewDto(review));
    }

    /// <summary>
    /// Retrieves paginated reviews for a seller.
    /// </summary>
    [HttpGet("seller/{sellerId}")]
    [ProducesResponseType(typeof(PaginatedResponse<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSellerReviews(
        Guid sellerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest("Invalid pagination parameters.");

        var (items, total) = await _reviewService.GetSellerReviewsAsync(sellerId, page, pageSize);
        var response = new PaginatedResponse<ReviewDto>
        {
            Items = items.Select(r => new ReviewDto(r)).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = total
        };

        return Ok(response);
    }

    /// <summary>
    /// Retrieves all reviews for a specific listing.
    /// </summary>
    [HttpGet("listing/{listingId}")]
    [ProducesResponseType(typeof(List<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListingReviews(Guid listingId)
    {
        var reviews = await _reviewService.GetListingReviewsAsync(listingId);
        return Ok(reviews.Select(r => new ReviewDto(r)).ToList());
    }

    /// <summary>
    /// Returns aggregated rating statistics for a seller including average score
    /// and per-score distribution counts.
    /// </summary>
    [HttpGet("seller/{sellerId}/summary")]
    [ProducesResponseType(typeof(ReviewSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSellerSummary(Guid sellerId)
    {
        var (average, total, distribution) = await _reviewService.GetSellerStatsAsync(sellerId);
        var summary = new ReviewSummaryDto
        {
            SellerId = sellerId,
            AverageScore = average,
            TotalReviews = total,
            ScoreDistribution = distribution
        };

        return Ok(summary);
    }

    /// <summary>
    /// Allows a seller to add a reply to a review about them.
    /// </summary>
    [HttpPost("{id}/reply")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddSellerReply(Guid id, [FromBody] SellerReplyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reply))
            return BadRequest("Reply content is required.");

        var review = await _reviewService.AddSellerReplyAsync(id, request.SellerId, request.Reply);
        return Ok(new ReviewDto(review));
    }

    /// <summary>
    /// Flags a review for moderator review.
    /// </summary>
    [HttpPost("{id}/flag")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FlagReview(Guid id)
    {
        var review = await _reviewService.FlagReviewAsync(id);
        return Ok(new ReviewDto(review));
    }

    /// <summary>
    /// Removes a review. Requires moderator or administrator role.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveReview(Guid id, [FromQuery] Guid moderatorId)
    {
        var review = await _reviewService.RemoveReviewAsync(id, moderatorId);
        return Ok(new ReviewDto(review));
    }
}
