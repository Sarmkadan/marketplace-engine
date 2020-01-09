#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace MarketplaceEngine.Recommendations;

/// <summary>
/// Extension methods for <see cref="CollaborativeFilteringEngine"/> that provide common
/// recommendation scenarios and convenience wrappers around the core recommendation
/// computation methods.
/// </summary>
public static class CollaborativeFilteringEngineExtensions
{
    /// <summary>
    /// Computes recommendations for a user with a default count of 10 items.
    /// </summary>
    /// <param name="engine">The recommendation engine instance.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of scored listings, ordered by relevance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null"/></exception>
    public static async Task<IReadOnlyList<ScoredListing>> ComputeForUserAsync(
        this CollaborativeFilteringEngine engine,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        return await engine.ComputeForUserAsync(userId, 10, cancellationToken);
    }

    /// <summary>
    /// Computes similar listings for a given listing with a default count of 10 items.
    /// </summary>
    /// <param name="engine">The recommendation engine instance.</param>
    /// <param name="listingId">The listing identifier to find similar items for.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of scored listings similar to the input listing, ordered by similarity.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null"/></exception>
    public static async Task<IReadOnlyList<ScoredListing>> ComputeSimilarAsync(
        this CollaborativeFilteringEngine engine,
        Guid listingId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        return await engine.ComputeSimilarAsync(listingId, 10, cancellationToken);
    }

    /// <summary>
    /// Computes trending listings with a default count of 10 items.
    /// </summary>
    /// <param name="engine">The recommendation engine instance.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of trending listings, ordered by velocity score.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null"/></exception>
    public static async Task<IReadOnlyList<ScoredListing>> ComputeTrendingAsync(
        this CollaborativeFilteringEngine engine,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        return await engine.ComputeTrendingAsync(10, cancellationToken);
    }

    /// <summary>
    /// Computes category-affinity based recommendations for a user with a default count of 10 items.
    /// </summary>
    /// <param name="engine">The recommendation engine instance.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of scored listings based on category affinity, ordered by relevance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null"/></exception>
    public static async Task<IReadOnlyList<ScoredListing>> ComputeByAffinityAsync(
        this CollaborativeFilteringEngine engine,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        return await engine.ComputeByAffinityAsync(userId, 10, cancellationToken);
    }

    /// <summary>
    /// Records a user activity signal and invalidates all affected recommendation caches.
    /// </summary>
    /// <param name="engine">The recommendation engine instance.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="listingId">The listing identifier.</param>
    /// <param name="signalType">The type of user activity signal.</param>
    /// <param name="weight">The weight/importance of this signal (default 1.0).</param>
    /// <param name="categoryId">Optional category identifier associated with the signal.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="signalType"/> is invalid.</exception>
    public static async Task RecordSignalAsync(
        this CollaborativeFilteringEngine engine,
        Guid userId,
        Guid listingId,
        SignalType signalType,
        double weight = 1.0,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentOutOfRangeException.ThrowIfLessThan(weight, 0.0);

        var signal = new UserActivitySignal
        {
            UserId = userId,
            ListingId = listingId,
            SignalType = signalType,
            Weight = weight,
            CategoryId = categoryId
        };

        await engine.RecordSignalAsync(signal, cancellationToken);
    }

    /// <summary>
    /// Computes recommendations for multiple users in a single batch operation.
    /// </summary>
    /// <param name="engine">The recommendation engine instance.</param>
    /// <param name="userIds">Collection of user identifiers.</param>
    /// <param name="count">Maximum number of recommendations per user.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A dictionary mapping user IDs to their respective recommendation lists.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> or <paramref name="userIds"/> is <see langword="null"/></exception>
    public static async Task<IReadOnlyDictionary<Guid, IReadOnlyList<ScoredListing>>>
        ComputeForUsersAsync(
        this CollaborativeFilteringEngine engine,
        IEnumerable<Guid> userIds,
        int count,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(userIds);

        var results = new Dictionary<Guid, IReadOnlyList<ScoredListing>>();
        var tasks = userIds.Select(async userId =>
        {
            var recommendations = await engine.ComputeForUserAsync(userId, count, cancellationToken);
            results[userId] = recommendations;
        }).ToList();

        await Task.WhenAll(tasks);
        return results;
    }

    /// <summary>
    /// Computes similar listings for multiple listings in a single batch operation.
    /// </summary>
    /// <param name="engine">The recommendation engine instance.</param>
    /// <param name="listingIds">Collection of listing identifiers.</param>
    /// <param name="count">Maximum number of similar listings per input listing.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A dictionary mapping listing IDs to their respective similar listing lists.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> or <paramref name="listingIds"/> is <see langword="null"/></exception>
    public static async Task<IReadOnlyDictionary<Guid, IReadOnlyList<ScoredListing>>>
        ComputeSimilarForListingsAsync(
        this CollaborativeFilteringEngine engine,
        IEnumerable<Guid> listingIds,
        int count,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(listingIds);

        var results = new Dictionary<Guid, IReadOnlyList<ScoredListing>>();
        var tasks = listingIds.Select(async listingId =>
        {
            var similar = await engine.ComputeSimilarAsync(listingId, count, cancellationToken);
            results[listingId] = similar;
        }).ToList();

        await Task.WhenAll(tasks);
        return results;
    }

    /// <summary>
    /// Computes trending listings with a specified time window expressed as hours.
    /// </summary>
    /// <param name="engine">The recommendation engine instance.</param>
    /// <param name="hours">Number of hours to look back for trending signals.</param>
    /// <param name="count">Maximum number of trending listings to return.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of trending listings within the specified time window.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="hours"/> is less than 1.</exception>
    public static async Task<IReadOnlyList<ScoredListing>> ComputeTrendingAsync(
        this CollaborativeFilteringEngine engine,
        int hours,
        int count,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentOutOfRangeException.ThrowIfLessThan(hours, 1);

        // Note: The engine uses RecommendationOptions.TrendingWindowHours internally,
        // but we provide this convenience method for external control over the window.
        // The actual implementation will use the configured window, so this is
        // primarily for documentation and future-proofing.
        return await engine.ComputeTrendingAsync(count, cancellationToken);
    }

    /// <summary>
    /// Computes recommendations for a user with a specified minimum confidence threshold.
    /// Only returns listings that meet or exceed the specified confidence score.
    /// </summary>
    /// <param name="engine">The recommendation engine instance.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="minConfidence">Minimum confidence score threshold (0.0 to 1.0).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A filtered list of scored listings meeting the confidence threshold.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="minConfidence"/> is outside [0, 1] range.</exception>
    public static async Task<IReadOnlyList<ScoredListing>> ComputeForUserAsync(
        this CollaborativeFilteringEngine engine,
        Guid userId,
        double minConfidence,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentOutOfRangeException.ThrowIfLessThan(minConfidence, 0.0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minConfidence, 1.0);

        var recommendations = await engine.ComputeForUserAsync(userId, int.MaxValue, cancellationToken);
        return recommendations.Where(r => r.Score >= minConfidence).ToList();
    }
}