#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MarketplaceEngine.Domain.Enums;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Provides extension methods for the <see cref="Review"/> class to enhance review processing,
/// validation, and analysis capabilities.
/// </summary>
public static class ReviewExtensions
{
    /// <summary>
    /// Determines whether the review is considered positive based on its score.
    /// </summary>
    /// <param name="review">The review instance.</param>
    /// <returns>True if score is 4 or 5; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="review"/> is null.</exception>
    public static bool IsPositive(this Review review)
    {
        ArgumentNullException.ThrowIfNull(review);
        return review.Score >= 4;
    }

    /// <summary>
    /// Determines whether the review is considered negative based on its score.
    /// </summary>
    /// <param name="review">The review instance.</param>
    /// <returns>True if score is 1, 2, or 3; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="review"/> is null.</exception>
    public static bool IsNegative(this Review review)
    {
        ArgumentNullException.ThrowIfNull(review);
        return review.Score <= 3;
    }

    /// <summary>
    /// Gets the age of the review in days.
    /// </summary>
    /// <param name="review">The review instance.</param>
    /// <returns>The number of days since the review was created.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="review"/> is null.</exception>
    public static int GetAgeInDays(this Review review)
    {
        ArgumentNullException.ThrowIfNull(review);
        var now = DateTime.UtcNow;
        return (int)(now - review.CreatedAt).TotalDays;
    }

    /// <summary>
    /// Determines whether the review has been replied to by the seller.
    /// </summary>
    /// <param name="review">The review instance.</param>
    /// <returns>True if seller has replied; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="review"/> is null.</exception>
    public static bool HasSellerReply(this Review review)
    {
        ArgumentNullException.ThrowIfNull(review);
        return review.SellerReply is not null;
    }

    /// <summary>
    /// Gets the review status as a formatted string.
    /// </summary>
    /// <param name="review">The review instance.</param>
    /// <returns>A human-readable status string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="review"/> is null.</exception>
    public static string GetStatusString(this Review review)
    {
        ArgumentNullException.ThrowIfNull(review);
        return review.Status switch
        {
            ReviewStatus.Active => "Active",
            ReviewStatus.UnderReview => "Under Review",
            ReviewStatus.Removed => "Removed",
            _ => review.Status.ToString()
        };
    }

    /// <summary>
    /// Gets the review score as a formatted percentage (e.g., "80%" for score 4).
    /// </summary>
    /// <param name="review">The review instance.</param>
    /// <returns>A formatted percentage string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="review"/> is null.</exception>
    public static string GetScorePercentage(this Review review)
    {
        ArgumentNullException.ThrowIfNull(review);
        return $"{review.Score * 20}%";
    }

    /// <summary>
    /// Determines whether the review is recent (created within the specified number of days).
    /// </summary>
    /// <param name="review">The review instance.</param>
    /// <param name="days">Number of days to consider as recent. Must be greater than 0.</param>
    /// <returns>True if review is recent; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="review"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="days"/> is less than or equal to 0.</exception>
    public static bool IsRecent(this Review review, int days)
    {
        ArgumentNullException.ThrowIfNull(review);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(days);

        var threshold = DateTime.UtcNow.AddDays(-days);
        return review.CreatedAt >= threshold;
    }

    /// <summary>
    /// Gets a summary of the review comment (first 50 characters followed by ellipsis if longer).
    /// </summary>
    /// <param name="review">The review instance.</param>
    /// <returns>A truncated comment summary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="review"/> is null.</exception>
    public static string GetCommentSummary(this Review review)
    {
        ArgumentNullException.ThrowIfNull(review);

        const int maxLength = 50;
        return review.Comment.Length <= maxLength
            ? review.Comment
            : review.Comment[..maxLength] + "...";
    }
}