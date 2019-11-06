#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Represents a buyer review for a seller or a listing transaction.
/// </summary>
public class Review
{
    public Guid Id { get; set; }

    /// <summary>The user who wrote this review.</summary>
    public Guid ReviewerId { get; set; }
    public User? Reviewer { get; set; }

    /// <summary>The seller being reviewed.</summary>
    public Guid SellerId { get; set; }
    public User? Seller { get; set; }

    /// <summary>The listing the transaction was about (optional).</summary>
    public Guid? ListingId { get; set; }
    public Listing? Listing { get; set; }

    /// <summary>Rating score from 1 (worst) to 5 (best).</summary>
    public int Score { get; set; }

    /// <summary>Written review content.</summary>
    public string Comment { get; set; } = string.Empty;

    public ReviewStatus Status { get; set; } = ReviewStatus.Active;

    /// <summary>Optional seller reply to the review.</summary>
    public string? SellerReply { get; set; }
    public DateTime? RepliedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Validates review content before submission
    public void ValidateReview()
    {
        if (Score < 1 || Score > 5)
            throw new ArgumentException("Score must be between 1 and 5.", nameof(Score));

        if (string.IsNullOrWhiteSpace(Comment) || Comment.Length < 10)
            throw new ArgumentException("Review comment must be at least 10 characters.", nameof(Comment));

        if (Comment.Length > 2000)
            throw new ArgumentException("Review comment cannot exceed 2000 characters.", nameof(Comment));

        if (ReviewerId == SellerId)
            throw new InvalidOperationException("A seller cannot review themselves.");
    }

    // Adds a seller reply to this review
    public void AddSellerReply(string reply)
    {
        if (string.IsNullOrWhiteSpace(reply) || reply.Length < 5)
            throw new ArgumentException("Seller reply must be at least 5 characters.", nameof(reply));

        if (reply.Length > 1000)
            throw new ArgumentException("Seller reply cannot exceed 1000 characters.", nameof(reply));

        if (SellerReply is not null)
            throw new InvalidOperationException("A reply has already been submitted for this review.");

        SellerReply = reply;
        RepliedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Flags review for moderation
    public void FlagForReview()
    {
        if (Status != ReviewStatus.Active)
            throw new InvalidOperationException("Only active reviews can be flagged.");

        Status = ReviewStatus.UnderReview;
        UpdatedAt = DateTime.UtcNow;
    }

    // Removes the review (moderator action)
    public void Remove()
    {
        Status = ReviewStatus.Removed;
        UpdatedAt = DateTime.UtcNow;
    }
}
