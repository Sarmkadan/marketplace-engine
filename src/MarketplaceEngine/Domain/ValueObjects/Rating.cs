// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a user or listing rating.
/// </summary>
public sealed class Rating : IEquatable<Rating>
{
    public int Score { get; }
    public int TotalReviews { get; }
    public double AverageRating { get; }

    public Rating(int score, int totalReviews = 0)
    {
        // Validates score is between 1-5 and total reviews is non-negative
        if (score < 1 || score > 5)
            throw new ArgumentException("Score must be between 1 and 5", nameof(score));

        if (totalReviews < 0)
            throw new ArgumentException("Total reviews cannot be negative", nameof(totalReviews));

        Score = score;
        TotalReviews = totalReviews;
        AverageRating = CalculateAverageRating();
    }

    // Calculates weighted average rating based on score and review count
    private double CalculateAverageRating()
    {
        if (TotalReviews == 0)
            return Score;

        // Bayesian average: ((total_reviews * score) + (confidence * 3.5)) / (total_reviews + confidence)
        const double confidenceThreshold = 10;
        return (TotalReviews * Score + confidenceThreshold * 3.5) / (TotalReviews + confidenceThreshold);
    }

    // Increments the rating with a new review score
    public Rating AddReview(int newScore)
    {
        if (newScore < 1 || newScore > 5)
            throw new ArgumentException("New score must be between 1 and 5", nameof(newScore));

        var totalScore = Score * TotalReviews + newScore;
        var newTotal = TotalReviews + 1;
        var newAverage = (double)totalScore / newTotal;

        return new Rating((int)Math.Round(newAverage), newTotal);
    }

    public bool Equals(Rating? other)
    {
        return other != null &&
               Score == other.Score &&
               TotalReviews == other.TotalReviews;
    }

    public override bool Equals(object? obj) => Equals(obj as Rating);

    public override int GetHashCode() => HashCode.Combine(Score, TotalReviews);

    public override string ToString() => $"{AverageRating:F2}/5 ({TotalReviews} reviews)";

    public static bool operator ==(Rating? left, Rating? right) => Equals(left, right);
    public static bool operator !=(Rating? left, Rating? right) => !Equals(left, right);
}
