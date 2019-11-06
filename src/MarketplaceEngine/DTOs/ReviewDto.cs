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
    public Guid Id { get; set; }
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public Guid SellerId { get; set; }
    public Guid? ListingId { get; set; }
    public int Score { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? SellerReply { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ReviewDto() { }

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
