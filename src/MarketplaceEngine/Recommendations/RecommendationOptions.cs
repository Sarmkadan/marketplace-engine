#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Recommendations;

/// <summary>
/// Tuning parameters for the collaborative filtering recommendation engine.
/// All settings can be overridden via the <c>Marketplace:Recommendations</c>
/// section of <c>appsettings.json</c>.
/// </summary>
public sealed class RecommendationOptions
{
    // ── Collaborative filtering ──────────────────────────────────────────────

    /// <summary>
    /// Minimum number of co-interacted listings required before two users are treated as
    /// neighbours in the similarity graph. Lower values increase recall at the cost of precision.
    /// </summary>
    public int MinOverlapForNeighbour { get; set; } = 3;

    /// <summary>
    /// Maximum number of nearest neighbours evaluated per user per recommendation request.
    /// Increasing this improves coverage but adds computation time proportionally.
    /// </summary>
    public int MaxNeighbours { get; set; } = 50;

    /// <summary>
    /// Cosine similarity threshold below which a candidate neighbour is excluded from scoring.
    /// Valid range: [0, 1]. Values below 0.05 may introduce noise into recommendations.
    /// </summary>
    public double MinSimilarityThreshold { get; set; } = 0.1;

    // ── Trending ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Number of hours to look back when computing trending velocity.
    /// Shorter windows surface rapidly rising listings; longer windows favour consistent performers.
    /// </summary>
    public int TrendingWindowHours { get; set; } = 48;

    /// <summary>Relative weight applied to <see cref="SignalType.View"/> signals in trending calculations.</summary>
    public double ViewWeight { get; set; } = 1.0;

    /// <summary>Relative weight applied to <see cref="SignalType.Save"/> signals in trending calculations.</summary>
    public double SaveWeight { get; set; } = 3.0;

    /// <summary>Relative weight applied to <see cref="SignalType.Enquiry"/> signals in trending calculations.</summary>
    public double EnquiryWeight { get; set; } = 5.0;

    /// <summary>Relative weight applied to <see cref="SignalType.Purchase"/> signals in trending calculations.</summary>
    public double PurchaseWeight { get; set; } = 10.0;

    // ── Caching ───────────────────────────────────────────────────────────────

    /// <summary>
    /// How long (in minutes) a personalised recommendation feed is cached per user.
    /// Signals recorded within this window will not alter the currently served feed.
    /// </summary>
    public int UserFeedCacheTtlMinutes { get; set; } = 5;

    /// <summary>How long (in minutes) the global trending feed is cached.</summary>
    public int TrendingFeedCacheTtlMinutes { get; set; } = 10;

    /// <summary>How long (in minutes) item-similarity results are cached per anchor listing.</summary>
    public int ItemSimilarityCacheTtlMinutes { get; set; } = 30;

    // ── Activity store ────────────────────────────────────────────────────────

    /// <summary>
    /// Maximum number of activity signals retained per user.
    /// Oldest signals are evicted via FIFO when this cap is exceeded.
    /// </summary>
    public int MaxSignalsPerUser { get; set; } = 500;

    /// <summary>
    /// Number of days of interaction history included when building the user-item matrix.
    /// Signals older than this threshold are excluded from recommendation computations.
    /// </summary>
    public int ActivityHistoryDays { get; set; } = 90;

    // ── Category affinity ─────────────────────────────────────────────────────

    /// <summary>
    /// Minimum number of signals that include a category identifier required before
    /// category-affinity recommendations are attempted for a given user.
    /// Users below this threshold receive trending content instead.
    /// </summary>
    public int MinAffinitySignals { get; set; } = 2;

    // ── Feature flags ─────────────────────────────────────────────────────────

    /// <summary>
    /// Enables user-specific personalised recommendations.
    /// When <c>false</c>, all requests fall back to the non-personalised trending feed.
    /// </summary>
    public bool EnablePersonalisation { get; set; } = true;

    /// <summary>
    /// Enables diversity injection to prevent category over-concentration in feeds.
    /// When <c>true</c>, at most <see cref="MaxCategoryConcentration"/> of results
    /// may originate from any single category.
    /// </summary>
    public bool EnableDiversification { get; set; } = true;

    /// <summary>
    /// Maximum fraction of recommendations that may originate from one category
    /// when diversification is enabled. Valid range: (0, 1].
    /// </summary>
    public double MaxCategoryConcentration { get; set; } = 0.4;

    // ── Factory methods ───────────────────────────────────────────────────────

    /// <summary>Creates an instance populated with production-ready default values.</summary>
    public static RecommendationOptions CreateDefault() => new();

    /// <summary>
    /// Binds options from the <c>Marketplace:Recommendations</c> configuration section.
    /// Keys absent from configuration retain their default values.
    /// </summary>
    /// <param name="configuration">The application configuration root.</param>
    public static RecommendationOptions FromConfiguration(IConfiguration configuration)
    {
        var options = new RecommendationOptions();
        configuration.GetSection("Marketplace:Recommendations").Bind(options);
        return options;
    }
}
