namespace MarketplaceEngine.Recommendations;

/// <summary>
/// Provides extension methods for <see cref="RecommendationOptions"/>.
/// </summary>
public static class RecommendationOptionsExtensions
{
    /// <summary>
    /// Creates a read-only view of the recommendation options with sensitive information redacted.
    /// </summary>
    /// <param name="options">The recommendation options to create a view for.</param>
    /// <returns>A read-only view of the recommendation options.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    public static IReadOnlyRecommendationOptions CreateReadOnlyView(this RecommendationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new ReadOnlyRecommendationOptions(options);
    }

    private sealed class ReadOnlyRecommendationOptions : IReadOnlyRecommendationOptions
    {
        private readonly RecommendationOptions _options;

        public ReadOnlyRecommendationOptions(RecommendationOptions options)
        {
            _options = options;
        }

        public int MinOverlapForNeighbour => _options.MinOverlapForNeighbour;
        public int MaxNeighbours => _options.MaxNeighbours;
        public double MinSimilarityThreshold => _options.MinSimilarityThreshold;
        public int TrendingWindowHours => _options.TrendingWindowHours;
        public double ViewWeight => _options.ViewWeight;
        public double SaveWeight => _options.SaveWeight;
        public double EnquiryWeight => _options.EnquiryWeight;
        public double PurchaseWeight => _options.PurchaseWeight;
        public int UserFeedCacheTtlMinutes => _options.UserFeedCacheTtlMinutes;
        public int TrendingFeedCacheTtlMinutes => _options.TrendingFeedCacheTtlMinutes;
        public int ItemSimilarityCacheTtlMinutes => _options.ItemSimilarityCacheTtlMinutes;
        public int MaxSignalsPerUser => _options.MaxSignalsPerUser;
        public int ActivityHistoryDays => _options.ActivityHistoryDays;
        public int MinAffinitySignals => _options.MinAffinitySignals;
        public bool EnablePersonalisation => _options.EnablePersonalisation;
        public bool EnableDiversification => _options.EnableDiversification;
        public double MaxCategoryConcentration => _options.MaxCategoryConcentration;
    }

    public interface IReadOnlyRecommendationOptions
    {
        int MinOverlapForNeighbour { get; }
        int MaxNeighbours { get; }
        double MinSimilarityThreshold { get; }
        int TrendingWindowHours { get; }
        double ViewWeight { get; }
        double SaveWeight { get; }
        double EnquiryWeight { get; }
        double PurchaseWeight { get; }
        int UserFeedCacheTtlMinutes { get; }
        int TrendingFeedCacheTtlMinutes { get; }
        int ItemSimilarityCacheTtlMinutes { get; }
        int MaxSignalsPerUser { get; }
        int ActivityHistoryDays { get; }
        int MinAffinitySignals { get; }
        bool EnablePersonalisation { get; }
        bool EnableDiversification { get; }
        double MaxCategoryConcentration { get; }
    }

    /// <summary>
    /// Validates the recommendation options and throws an exception if they are invalid.
    /// </summary>
    /// <param name="options">The recommendation options to validate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="options"/> is invalid.</exception>
    public static void Validate(this RecommendationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.MinOverlapForNeighbour < 0)
        {
            throw new ArgumentException("MinOverlapForNeighbour must be non-negative.", nameof(options));
        }

        if (options.MaxNeighbours < 0)
        {
            throw new ArgumentException("MaxNeighbours must be non-negative.", nameof(options));
        }

        if (options.MinSimilarityThreshold < 0 || options.MinSimilarityThreshold > 1)
        {
            throw new ArgumentException("MinSimilarityThreshold must be between 0 and 1.", nameof(options));
        }
    }
}
