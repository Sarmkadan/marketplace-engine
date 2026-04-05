#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using MarketplaceEngine.Infrastructure.Caching;
using MarketplaceEngine.Repositories;

namespace MarketplaceEngine.Recommendations;

/// <summary>
/// In-process recommendation engine combining three complementary strategies:
/// user-based collaborative filtering (cosine similarity on interaction vectors),
/// item-based collaborative filtering (co-interaction co-occurrence scoring),
/// category-affinity scoring (weighted category preference blended with popularity),
/// and a popularity-based trending fallback for cold-start and low-data scenarios.
/// </summary>
/// <remarks>
/// Designed for single-instance deployments and early-growth phases.
/// For high-traffic production environments, replace with a dedicated ML pipeline such as
/// Azure Personalizer, TensorFlow Recommenders, or Spark ALS with a Redis-backed signal store.
/// The trending computation uses actual signal velocity from the activity tracker, constrained
/// to the <see cref="RecommendationOptions.TrendingWindowHours"/> look-back window, and falls
/// back to listing-model counters when no in-window signals exist.
/// </remarks>
public sealed class CollaborativeFilteringEngine : IRecommendationEngine
{
    private readonly IUserActivityTracker _activityTracker;
    private readonly IListingRepository _listingRepository;
    private readonly CacheService _cache;
    private readonly RecommendationOptions _options;
    private readonly ILogger<CollaborativeFilteringEngine> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="CollaborativeFilteringEngine"/>.
    /// </summary>
    public CollaborativeFilteringEngine(
        IUserActivityTracker activityTracker,
        IListingRepository listingRepository,
        CacheService cache,
        RecommendationOptions options,
        ILogger<CollaborativeFilteringEngine> logger)
    {
        _activityTracker = activityTracker ?? throw new ArgumentNullException(nameof(activityTracker));
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScoredListing>> ComputeForUserAsync(
        Guid userId, int count, CancellationToken cancellationToken = default)
    {
        if (!_options.EnablePersonalisation)
            return await ComputeTrendingAsync(count, cancellationToken);

        var cacheKey = $"rec:user:{userId}:{count}";
        var cached = await _cache.GetAsync<List<ScoredListing>>(cacheKey);
        if (cached is { Count: > 0 })
        {
            _logger.LogDebug("Recommendation cache hit for user {UserId}", userId);
            return cached;
        }

        var history = await _activityTracker.GetUserHistoryAsync(
            userId, TimeSpan.FromDays(_options.ActivityHistoryDays), cancellationToken);

        if (history.Count < _options.MinOverlapForNeighbour)
        {
            _logger.LogDebug(
                "Insufficient history for user {UserId} ({Count} signals); falling back to trending",
                userId, history.Count);
            return await ComputeTrendingAsync(count, cancellationToken);
        }

        var interacted = history.Select(s => s.ListingId).ToHashSet();
        var candidates = await BuildCandidateScoresAsync(userId, history, interacted, cancellationToken);

        if (candidates.Count == 0)
        {
            _logger.LogDebug("No CF candidates for user {UserId}; falling back to trending", userId);
            return await ComputeTrendingAsync(count, cancellationToken);
        }

        var results = SelectTopN(candidates, count, RecommendationReason.CollaborativeFiltering);
        await _cache.SetAsync(cacheKey, results, TimeSpan.FromMinutes(_options.UserFeedCacheTtlMinutes));

        _logger.LogInformation("CF produced {Count} candidates for user {UserId}", results.Count, userId);
        return results;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScoredListing>> ComputeSimilarAsync(
        Guid listingId, int count, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"rec:similar:{listingId}:{count}";
        var cached = await _cache.GetAsync<List<ScoredListing>>(cacheKey);
        if (cached is { Count: > 0 })
            return cached;

        var audience = await _activityTracker.GetListingAudienceAsync(listingId, cancellationToken);
        if (audience.Count == 0)
            return await ComputeTrendingAsync(count, cancellationToken);

        var coOccurrence = new ConcurrentDictionary<Guid, double>();

        await Parallel.ForEachAsync(audience, cancellationToken, async (viewerId, ct) =>
        {
            var signals = await _activityTracker.GetUserHistoryAsync(viewerId, null, ct);
            foreach (var s in signals.Where(s => s.ListingId != listingId))
                coOccurrence.AddOrUpdate(s.ListingId, s.Weight, (_, current) => current + s.Weight);
        });

        var results = SelectTopN(
            new Dictionary<Guid, double>(coOccurrence), count, RecommendationReason.ItemSimilarity);

        await _cache.SetAsync(cacheKey, results,
            TimeSpan.FromMinutes(_options.ItemSimilarityCacheTtlMinutes));

        return results;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Signal velocity is computed from the activity tracker using the configured
    /// <see cref="RecommendationOptions.TrendingWindowHours"/> look-back period.
    /// When no in-window signals exist (cold start), the method falls back to
    /// listing-model <c>ViewCount</c> and <c>InterestCount</c> counters.
    /// </remarks>
    public async Task<IReadOnlyList<ScoredListing>> ComputeTrendingAsync(
        int count, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"rec:trending:{count}";
        var cached = await _cache.GetAsync<List<ScoredListing>>(cacheKey);
        if (cached is { Count: > 0 })
            return cached;

        var window = TimeSpan.FromHours(_options.TrendingWindowHours);
        var recentSignals = await _activityTracker.GetSignalsInWindowAsync(window, cancellationToken);

        Dictionary<Guid, double> velocityScores;

        if (recentSignals.Count > 0)
        {
            // Compute per-listing velocity from actual interaction signals within the trending window.
            // Each signal type carries its configured weight so high-intent interactions
            // (Enquiry, Purchase) disproportionately lift a listing's trend score.
            velocityScores = new Dictionary<Guid, double>(recentSignals.Count / 2);
            foreach (var signal in recentSignals)
            {
                var weight = signal.SignalType switch
                {
                    SignalType.View     => _options.ViewWeight,
                    SignalType.Save     => _options.SaveWeight,
                    SignalType.Enquiry  => _options.EnquiryWeight,
                    SignalType.Purchase => _options.PurchaseWeight,
                    _                  => 1.0
                };

                velocityScores.TryGetValue(signal.ListingId, out var current);
                velocityScores[signal.ListingId] = current + weight;
            }

            _logger.LogDebug(
                "Trending velocity computed from {SignalCount} signals across {ListingCount} listings",
                recentSignals.Count, velocityScores.Count);
        }
        else
        {
            // Cold-start fallback: derive proxy scores from listing-model counters.
            var listings = await _listingRepository.GetActiveListingsAsync();
            velocityScores = listings.ToDictionary(
                l => l.Id,
                l => (l.ViewCount * _options.ViewWeight) + (l.InterestCount * _options.SaveWeight));

            _logger.LogDebug(
                "No in-window signals found; using listing counters for trending ({Count} listings)",
                listings.Count);
        }

        var results = SelectTopN(velocityScores, count, RecommendationReason.Trending);
        await _cache.SetAsync(cacheKey, results,
            TimeSpan.FromMinutes(_options.TrendingFeedCacheTtlMinutes));

        return results;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Builds a normalised category-preference vector from the user's interaction history.
    /// Each category weight is the sum of signal weights for interactions within that category.
    /// Candidate listings are drawn from active inventory, excluding already-interacted items,
    /// and scored as: <c>0.7 × normAffinity + 0.3 × popularityProxy</c>, where
    /// <c>popularityProxy</c> is the listing's view/save velocity capped at 1.0.
    /// </remarks>
    public async Task<IReadOnlyList<ScoredListing>> ComputeByAffinityAsync(
        Guid userId, int count, CancellationToken cancellationToken = default)
    {
        if (!_options.EnablePersonalisation)
            return await ComputeTrendingAsync(count, cancellationToken);

        var cacheKey = $"rec:affinity:{userId}:{count}";
        var cached = await _cache.GetAsync<List<ScoredListing>>(cacheKey);
        if (cached is { Count: > 0 })
            return cached;

        var history = await _activityTracker.GetUserHistoryAsync(
            userId, TimeSpan.FromDays(_options.ActivityHistoryDays), cancellationToken);

        // Require a minimum number of categorised signals before attempting affinity scoring.
        var categorisedSignals = history.Where(s => s.CategoryId.HasValue).ToList();
        if (categorisedSignals.Count < _options.MinAffinitySignals)
        {
            _logger.LogDebug(
                "Insufficient categorised signals for user {UserId} ({Count}/{Min}); falling back to trending",
                userId, categorisedSignals.Count, _options.MinAffinitySignals);
            return await ComputeTrendingAsync(count, cancellationToken);
        }

        // Build weighted category-preference vector.
        var categoryWeights = new Dictionary<Guid, double>();
        foreach (var signal in categorisedSignals)
        {
            var catId = signal.CategoryId!.Value;
            categoryWeights.TryGetValue(catId, out var w);
            categoryWeights[catId] = w + signal.Weight;
        }

        var maxCatWeight = categoryWeights.Values.Max();
        if (maxCatWeight == 0.0) maxCatWeight = 1.0;

        var interacted = history.Select(s => s.ListingId).ToHashSet();
        var allActiveListings = await _listingRepository.GetActiveListingsAsync();

        // Score unseen listings that fall into a preferred category.
        // Popularity is normalised on a 1 000-interaction reference scale; values above this
        // cap at 1.0 so high-traffic listings don't overwhelm category affinity.
        var scores = new Dictionary<Guid, double>();
        foreach (var listing in allActiveListings.Where(l => !interacted.Contains(l.Id)))
        {
            if (!categoryWeights.TryGetValue(listing.CategoryId, out var catWeight))
                continue;

            var normAffinity = catWeight / maxCatWeight;
            var popularityProxy = Math.Min(
                (listing.ViewCount * _options.ViewWeight
               + listing.InterestCount * _options.SaveWeight) / 1_000.0,
                1.0);

            scores[listing.Id] = (normAffinity * 0.7) + (popularityProxy * 0.3);
        }

        if (scores.Count == 0)
        {
            _logger.LogDebug(
                "Category-affinity yielded no candidates for user {UserId}; falling back to trending",
                userId);
            return await ComputeTrendingAsync(count, cancellationToken);
        }

        var results = SelectTopN(scores, count, RecommendationReason.CategoryAffinity);
        await _cache.SetAsync(cacheKey, results,
            TimeSpan.FromMinutes(_options.UserFeedCacheTtlMinutes));

        _logger.LogInformation(
            "Affinity engine produced {Count} candidates for user {UserId} across {Categories} categories",
            results.Count, userId, categoryWeights.Count);

        return results;
    }

    /// <inheritdoc/>
    public async Task RecordSignalAsync(
        UserActivitySignal signal, CancellationToken cancellationToken = default)
    {
        await _activityTracker.RecordAsync(signal, cancellationToken);

        // Invalidate all cached feeds that could be affected by this new signal.
        await _cache.RemoveAsync($"rec:user:{signal.UserId}:*");
        await _cache.RemoveAsync($"rec:affinity:{signal.UserId}:*");
        await _cache.RemoveAsync($"rec:similar:{signal.ListingId}:*");

        _logger.LogDebug("Signal {Type} recorded; caches invalidated for user {UserId}",
            signal.SignalType, signal.UserId);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<Dictionary<Guid, double>> BuildCandidateScoresAsync(
        Guid userId,
        IReadOnlyList<UserActivitySignal> history,
        HashSet<Guid> interactedListings,
        CancellationToken cancellationToken)
    {
        var userVector = history.ToDictionary(s => s.ListingId, s => s.Weight);
        var scores = new ConcurrentDictionary<Guid, double>();

        // Cap the number of seed listings examined to bound worst-case complexity.
        await Parallel.ForEachAsync(
            interactedListings.Take(100), cancellationToken, async (lid, ct) =>
            {
                var peers = await _activityTracker.GetListingAudienceAsync(lid, ct);

                foreach (var neighbourId in peers
                             .Where(id => id != userId)
                             .Take(_options.MaxNeighbours))
                {
                    var neighbourHistory =
                        await _activityTracker.GetUserHistoryAsync(neighbourId, null, ct);

                    var similarity = CosineSimilarity(userVector, neighbourHistory);
                    if (similarity < _options.MinSimilarityThreshold)
                        continue;

                    // Propagate similarity-weighted scores to unseen listings.
                    foreach (var sig in neighbourHistory
                                 .Where(s => !interactedListings.Contains(s.ListingId)))
                    {
                        var contribution = similarity * sig.Weight;
                        scores.AddOrUpdate(
                            sig.ListingId,
                            contribution,
                            (_, existing) => existing + contribution);
                    }
                }
            });

        return new Dictionary<Guid, double>(scores);
    }

    private static List<ScoredListing> SelectTopN(
        Dictionary<Guid, double> scores, int count, RecommendationReason reason)
    {
        double max = scores.Values.DefaultIfEmpty(1.0).Max();
        if (max == 0.0) max = 1.0;

        return scores
            .OrderByDescending(kvp => kvp.Value)
            .Take(count)
            .Select(kvp => new ScoredListing(kvp.Key, kvp.Value / max, reason))
            .ToList();
    }

    /// <summary>
    /// Computes cosine similarity between a user's interaction vector and a neighbour's history.
    /// Returns 0 when either vector has zero magnitude.
    /// </summary>
    private static double CosineSimilarity(
        Dictionary<Guid, double> vectorA,
        IReadOnlyList<UserActivitySignal> historyB)
    {
        double dot = 0.0, magB = 0.0;
        double magA = vectorA.Values.Sum(v => v * v);

        foreach (var signal in historyB)
        {
            magB += signal.Weight * signal.Weight;
            if (vectorA.TryGetValue(signal.ListingId, out var a))
                dot += a * signal.Weight;
        }

        return (magA == 0.0 || magB == 0.0) ? 0.0 : dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }
}
