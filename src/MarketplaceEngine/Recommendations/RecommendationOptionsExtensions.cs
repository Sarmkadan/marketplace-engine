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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is <see langword="null"/>.</exception>
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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if any of the following conditions are violated:
    /// <list type="bullet">
    ///   <item><description><see cref="RecommendationOptions.MinOverlapForNeighbour"/> must be non-negative.</description></item>
    ///   <item><description><see cref="RecommendationOptions.MaxNeighbours"/> must be non-negative.</description></item>
    ///   <item><description><see cref="RecommendationOptions.MinSimilarityThreshold"/> must be between 0 and 1.</description></item>
    ///   <item><description><see cref="RecommendationOptions.TrendingWindowHours"/> must be positive.</description></item>
    ///   <item><description>All weight values (<see cref="RecommendationOptions.ViewWeight"/>, <see cref="RecommendationOptions.SaveWeight"/>, <see cref="RecommendationOptions.EnquiryWeight"/>, <see cref="RecommendationOptions.PurchaseWeight"/>) must be non-negative.</description></item>
    ///   <item><description>All cache TTL values must be non-negative.</description></item>
    ///   <item><description><see cref="RecommendationOptions.MaxSignalsPerUser"/> must be positive.</description></item>
    ///   <item><description><see cref="RecommendationOptions.ActivityHistoryDays"/> must be positive.</description></item>
    ///   <item><description><see cref="RecommendationOptions.MinAffinitySignals"/> must be non-negative.</description></item>
    ///   <item><description><see cref="RecommendationOptions.MaxCategoryConcentration"/> must be between 0 and 1 (exclusive).</description></item>
    /// </list>
    /// </exception>
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

        if (options.TrendingWindowHours <= 0)
        {
            throw new ArgumentException("TrendingWindowHours must be positive.", nameof(options));
        }

        if (options.ViewWeight < 0)
        {
            throw new ArgumentException("ViewWeight must be non-negative.", nameof(options));
        }

        if (options.SaveWeight < 0)
        {
            throw new ArgumentException("SaveWeight must be non-negative.", nameof(options));
        }

        if (options.EnquiryWeight < 0)
        {
            throw new ArgumentException("EnquiryWeight must be non-negative.", nameof(options));
        }

        if (options.PurchaseWeight < 0)
        {
            throw new ArgumentException("PurchaseWeight must be non-negative.", nameof(options));
        }

        if (options.UserFeedCacheTtlMinutes < 0)
        {
            throw new ArgumentException("UserFeedCacheTtlMinutes must be non-negative.", nameof(options));
        }

        if (options.TrendingFeedCacheTtlMinutes < 0)
        {
            throw new ArgumentException("TrendingFeedCacheTtlMinutes must be non-negative.", nameof(options));
        }

        if (options.ItemSimilarityCacheTtlMinutes < 0)
        {
            throw new ArgumentException("ItemSimilarityCacheTtlMinutes must be non-negative.", nameof(options));
        }

        if (options.MaxSignalsPerUser <= 0)
        {
            throw new ArgumentException("MaxSignalsPerUser must be positive.", nameof(options));
        }

        if (options.ActivityHistoryDays <= 0)
        {
            throw new ArgumentException("ActivityHistoryDays must be positive.", nameof(options));
        }

        if (options.MinAffinitySignals < 0)
        {
            throw new ArgumentException("MinAffinitySignals must be non-negative.", nameof(options));
        }

        if (options.MaxCategoryConcentration <= 0 || options.MaxCategoryConcentration > 1)
        {
            throw new ArgumentException("MaxCategoryConcentration must be between 0 and 1 (exclusive).", nameof(options));
        }
    }
}