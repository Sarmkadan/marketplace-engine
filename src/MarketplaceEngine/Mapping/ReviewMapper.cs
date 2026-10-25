#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.DTOs;

namespace MarketplaceEngine.Mapping;

/// <summary>
/// Maps <see cref="Review"/> domain models to <see cref="ReviewDto"/> API responses.
/// </summary>
public static class ReviewMapper
{
    /// <summary>
    /// Converts a single review into its API representation.
    /// </summary>
    /// <param name="review">The review domain model.</param>
    /// <returns>The review DTO.</returns>
    public static ReviewDto ToDto(Review review) => new ReviewDto(review);

    /// <summary>
    /// Converts a sequence of reviews into API representations, preserving order.
    /// </summary>
    /// <param name="reviews">The review domain models.</param>
    /// <returns>A materialized list of review DTOs.</returns>
    public static List<ReviewDto> ToDtoList(IEnumerable<Review> reviews) =>
        reviews.Select(ToDto).ToList();
}
