#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Data transfer object for a seller review.
/// </summary>
public class ReviewDto
{
    /// <summary>
    /// Unique identifier for the review.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the reviewer who wrote the review.
    /// </summary>
    public Guid ReviewerId { get; set; }

    /// <summary>
    /// Name of the reviewer.
    /// </summary>
    public string ReviewerName { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the seller who received the review.
    /// </summary>
    public Guid SellerId { get; set; }

    /// <summary>
    /// Identifier of the listing that was reviewed (nullable).
    /// </summary>
    public Guid? ListingId { get; set; }

    /// <summary>
    /// Score given by the reviewer (1-5).
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Comment left by the reviewer.
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the review (e.g., Pending, Approved, Rejected).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Reply left by the seller (nullable).
    /// </summary>
    public string? SellerReply { get; set; }

    /// <summary>
    /// Date and time when the review was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the review was last updated (nullable).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Initializes a new instance of the ReviewDto class.
    /// </summary>
    public ReviewDto() { }

    /// <summary>
    /// Initializes a new instance of the ReviewDto class from a Review domain model.
    /// </summary>
    /// <param name="review">The Review domain model to convert from.</param>
    public ReviewDto(Review review)
    {
        Id = review.Id;
        ReviewerId = review.ReviewerId;
        ReviewerName = review.Reviewer?.FullName ?? string.Empty;
        SellerId = review.SellerId;
        ListingId = review.ListingId;
        Score = review.Score;
        Comment = review.Comment;
        Status = review.Status.ToString();
        SellerReply = review.SellerReply;
        CreatedAt = review.CreatedAt;
        UpdatedAt = review.UpdatedAt;
    }
}

/// <summary>
/// Summary statistics for a seller's reviews.
/// </summary>
public class ReviewSummaryDto
{
    public Guid SellerId { get; set; }
    public double AverageScore { get; set; }
    public int TotalReviews { get; set; }

    /// <summary>Distribution of scores: key is the score (1-5), value is the count.</summary>
    public Dictionary<int, int> ScoreDistribution { get; set; } = [];
}

/// <summary>
/// Request DTO for submitting a new review.
/// </summary>
public class CreateReviewRequest
{
    public Guid ReviewerId { get; set; }
    public Guid SellerId { get; set; }
    public Guid? ListingId { get; set; }
    public int Score { get; set; }
    public string Comment { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for a seller to reply to a review.
/// </summary>
public class SellerReplyRequest
{
    public Guid SellerId { get; set; }
    public string Reply { get; set; } = string.Empty;
}
