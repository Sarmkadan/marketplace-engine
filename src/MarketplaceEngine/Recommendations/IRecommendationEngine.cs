#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Recommendations;

/// <summary>
/// Defines the contract for recommendation engine implementations.
/// Supports collaborative filtering, item-based similarity, category-affinity,
/// and trending strategies with graceful cold-start fallbacks.
/// </summary>
public interface IRecommendationEngine
{
    /// <summary>
    /// Computes personalised recommendations for a given user based on their interaction history.
    /// Falls back to trending content when insufficient behavioural data is available.
    /// </summary>
    /// <param name="userId">The identifier of the user requesting recommendations.</param>
    /// <param name="count">Maximum number of results to return.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<IReadOnlyList<ScoredListing>> ComputeForUserAsync(
        Guid userId, int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes listings similar to a given anchor listing using item-based collaborative filtering.
    /// Derives similarity from the co-interaction patterns of the anchor's audience.
    /// </summary>
    /// <param name="listingId">The listing from which to derive similar items.</param>
    /// <param name="count">Maximum number of results to return.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<IReadOnlyList<ScoredListing>> ComputeSimilarAsync(
        Guid listingId, int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns globally trending listings based on recent signal velocity within the configured
    /// trending window. Falls back to listing-model counters when no activity signals exist.
    /// </summary>
    /// <param name="count">Maximum number of results to return.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<IReadOnlyList<ScoredListing>> ComputeTrendingAsync(
        int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes category-affinity recommendations for a user based on their demonstrated
    /// category preferences, blended with listing-level popularity signals.
    /// Falls back to trending when the user has fewer categorised signals than the configured minimum.
    /// </summary>
    /// <param name="userId">The identifier of the user requesting recommendations.</param>
    /// <param name="count">Maximum number of results to return.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<IReadOnlyList<ScoredListing>> ComputeByAffinityAsync(
        Guid userId, int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a behavioural signal that will improve future recommendations.
    /// Implementations should persist the signal and invalidate affected caches.
    /// </summary>
    /// <param name="signal">The signal emitted by the user interaction.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task RecordSignalAsync(UserActivitySignal signal, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines the contract for tracking and querying user behavioural signals.
/// Provides the raw interaction data consumed by the recommendation engine.
/// </summary>
public interface IUserActivityTracker
{
    /// <summary>
    /// Persists a user activity signal for future recommendation computations.
    /// </summary>
    /// <param name="signal">The signal to record.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task RecordAsync(UserActivitySignal signal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user's interaction history, optionally constrained to a sliding time window.
    /// </summary>
    /// <param name="userId">The user whose history to retrieve.</param>
    /// <param name="window">Optional time window; <c>null</c> returns all stored signals.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<IReadOnlyList<UserActivitySignal>> GetUserHistoryAsync(
        Guid userId, TimeSpan? window = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the identifiers of all users who have interacted with the specified listing.
    /// Used to build the co-interaction graph for item-based similarity computation.
    /// </summary>
    /// <param name="listingId">The listing whose audience to retrieve.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<IReadOnlyList<Guid>> GetListingAudienceAsync(
        Guid listingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all interaction signals recorded across all users within the given time window.
    /// Intended for computing global trending velocity from raw, unfiltered signal data.
    /// The result set may be large on high-traffic deployments; callers should constrain
    /// <paramref name="window"/> using the configured trending window setting.
    /// </summary>
    /// <param name="window">The look-back period for signal retrieval.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<IReadOnlyList<UserActivitySignal>> GetSignalsInWindowAsync(
        TimeSpan window, CancellationToken cancellationToken = default);
}

/// <summary>
/// A listing paired with its normalised relevance score and recommendation rationale.
/// Immutable; instances are produced by the engine and consumed by the service layer.
/// </summary>
/// <param name="ListingId">The unique identifier of the recommended listing.</param>
/// <param name="Score">Normalised relevance score in the range [0, 1]; higher means more relevant.</param>
/// <param name="Reason">Classification of why this item was recommended.</param>
public sealed record ScoredListing(Guid ListingId, double Score, RecommendationReason Reason);

/// <summary>
/// Classifies the primary reason a recommendation was generated.
/// Used for explainability, A/B testing attribution, and UX labelling.
/// </summary>
public enum RecommendationReason
{
    /// <summary>Users with similar interaction patterns also engaged with this listing.</summary>
    CollaborativeFiltering,

    /// <summary>Derived from co-interaction patterns with items the user has already viewed.</summary>
    ItemSimilarity,

    /// <summary>This listing is gaining momentum across the marketplace.</summary>
    Trending,

    /// <summary>Aligns with categories the user has shown a statistically significant preference for.</summary>
    CategoryAffinity,

    /// <summary>General recommendation used when no personalisation data is available.</summary>
    Editorial
}

/// <summary>
/// Represents a single behavioural interaction between a user and a listing.
/// Signals are the primary input used to build and update the recommendation model.
/// </summary>
public sealed record UserActivitySignal
{
    /// <summary>The user who performed the interaction.</summary>
    public required Guid UserId { get; init; }

    /// <summary>The listing that was interacted with.</summary>
    public required Guid ListingId { get; init; }

    /// <summary>Optional category of the listing at the time of interaction; aids category-affinity scoring.</summary>
    public Guid? CategoryId { get; init; }

    /// <summary>The type of interaction, which determines its base weight in the model.</summary>
    public required SignalType SignalType { get; init; }

    /// <summary>
    /// Explicit weight for this signal. Defaults to the numeric value of <see cref="SignalType"/>.
    /// Higher weights amplify the influence of this interaction on recommendations.
    /// </summary>
    public double Weight { get; init; } = 1.0;

    /// <summary>UTC timestamp at which the interaction occurred.</summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Enumerates the types of user interactions captured as recommendation signals.
/// Numeric values represent the default relative weight of each interaction type.
/// </summary>
public enum SignalType
{
    /// <summary>User viewed the listing detail page. Default weight: 1.</summary>
    View = 1,

    /// <summary>User saved or bookmarked the listing. Default weight: 3.</summary>
    Save = 3,

    /// <summary>User sent a message to the seller about the listing. Default weight: 5.</summary>
    Enquiry = 5,

    /// <summary>User completed a purchase. Default weight: 10.</summary>
    Purchase = 10
}
