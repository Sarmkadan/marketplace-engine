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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="otherRating"/> is null.</exception>
    public static bool HasImproved(this Rating rating, Rating otherRating)
    {
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
    /// <exception cref="ArgumentException">Thrown if <paramref name="ratings"/> is empty.</exception>
    public static double CalculatePercentile(this Rating rating, IReadOnlyList<Rating> ratings)
    {
        ArgumentNullException.ThrowIfNull(ratings);
        if (ratings.Count == 0)
        {
            throw new ArgumentException("Ratings list cannot be empty.", nameof(ratings));
        }

        var sortedRatings = ratings.OrderBy(r => r.AverageRating).ToList();
        var index = sortedRatings.IndexOf(rating);

        return ((double)index / (sortedRatings.Count - 1)) * 100;
    }

    /// <summary>
    /// Gets a rating description based on its average rating.
    /// </summary>
    /// <param name="rating">The rating to get the description for.</param>
    /// <returns>A string describing the rating.</returns>
    public static string GetDescription(this Rating rating)
    {
        if (rating.AverageRating < 2)
        {
            return "Poor";
        }
        else if (rating.AverageRating < 4)
        {
            return "Average";
        }
        else
        {
            return "Excellent";
        }
    }
}
