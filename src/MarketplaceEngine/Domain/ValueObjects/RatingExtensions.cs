namespace MarketplaceEngine.Domain.ValueObjects;

/// <summary>
/// Extension methods for <see cref="Rating"/>.
/// </summary>
public static class RatingExtensions
{
    /// <summary>
    /// Determines if a rating has improved compared to another rating.
    /// </summary>
    /// <param name="rating">The current rating.</param>
    /// <param name="otherRating">The other rating to compare with.</param>
    /// <returns>true if the rating has improved; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rating"/> or <paramref name="otherRating"/> is null.</exception>
    public static bool HasImproved(this Rating rating, Rating otherRating)
    {
        ArgumentNullException.ThrowIfNull(rating);
        ArgumentNullException.ThrowIfNull(otherRating);

        return rating.AverageRating > otherRating.AverageRating;
    }

    /// <summary>
    /// Calculates the rating percentile compared to a list of ratings.
    /// </summary>
    /// <param name="rating">The rating to calculate the percentile for.</param>
    /// <param name="ratings">The list of ratings to compare with.</param>
    /// <returns>The rating percentile.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="ratings"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="ratings"/> is empty or if <paramref name="rating"/> is not found in the list.</exception>
    public static double CalculatePercentile(this Rating rating, IReadOnlyList<Rating> ratings)
    {
        ArgumentNullException.ThrowIfNull(rating);
        ArgumentNullException.ThrowIfNull(ratings);

        if (ratings.Count == 0)
        {
            throw new ArgumentException("Ratings list cannot be empty.", nameof(ratings));
        }

        var sortedRatings = ratings.OrderBy(r => r.AverageRating).ToList();
        var index = sortedRatings.IndexOf(rating);

        if (index == -1)
        {
            throw new ArgumentException("Rating not found in the provided list.", nameof(rating));
        }

        return ((double)index / (sortedRatings.Count - 1)) * 100;
    }

    /// <summary>
    /// Gets a rating description based on its average rating.
    /// </summary>
    /// <param name="rating">The rating to get the description for.</param>
    /// <returns>A string describing the rating.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rating"/> is null.</exception>
    public static string GetDescription(this Rating rating)
    {
        ArgumentNullException.ThrowIfNull(rating);

        return rating.AverageRating switch
        {
            < 2 => "Poor",
            < 4 => "Average",
            _ => "Excellent"
        };
    }
}