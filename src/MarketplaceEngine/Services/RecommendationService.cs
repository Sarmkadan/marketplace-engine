#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.DTOs;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Infrastructure.Events;
using MarketplaceEngine.Recommendations;
using MarketplaceEngine.Repositories;

namespace MarketplaceEngine.Services;

/// <summary>
/// Application-layer service that orchestrates the recommendation engine to deliver
/// personalised listing feeds, similar-item panels, trending galleries, and
/// category-affinity feeds.
/// </summary>
/// <remarks>
/// This service is the single entry point for all recommendation concerns in the API layer.
/// It handles input validation, existence checks, result hydration, diversity enforcement,
/// and event publication. Controllers and endpoint handlers should not access
/// <see cref="IRecommendationEngine"/> directly.
/// </remarks>
public class RecommendationService
{
    private readonly IRecommendationEngine _engine;
    private readonly IListingRepository _listingRepository;
    private readonly IUserRepository _userRepository;
    private readonly EventBus _eventBus;
    private readonly RecommendationOptions _options;
    private readonly ILogger<RecommendationService> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="RecommendationService"/>.
    /// </summary>
    public RecommendationService(
        IRecommendationEngine engine,
        IListingRepository listingRepository,
        IUserRepository userRepository,
        EventBus eventBus,
        RecommendationOptions options,
        ILogger<RecommendationService> logger)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Returns a personalised recommendation feed for the specified user.
    /// Automatically falls back to trending content when personalisation data is insufficient
    /// or when the user has fewer interactions than the configured overlap threshold.
    /// </summary>
    /// <param name="userId">The identifier of the requesting user.</param>
    /// <param name="count">Maximum number of recommendations to return. Clamped to [1, 100].</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <exception cref="ResourceNotFoundException">Thrown when the user does not exist.</exception>
    public async Task<RecommendationFeedDto> GetRecommendationsForUserAsync(
        Guid userId, int count = 20, CancellationToken cancellationToken = default)
    {
        count = Math.Clamp(count, 1, 100);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new ResourceNotFoundException("User", userId);

        _logger.LogInformation(
            "Generating personalised recommendations for user {UserId} (count={Count})", userId, count);

        var scored = await _engine.ComputeForUserAsync(userId, count, cancellationToken);
        var (items, strategyUsed) = await HydrateAndDiversifyAsync(scored, cancellationToken);

        var isPersonalised = scored.Count > 0 && scored[0].Reason != RecommendationReason.Trending;

        return new RecommendationFeedDto
        {
            Items = items,
            IsPersonalised = isPersonalised,
            Strategy = strategyUsed,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Returns a category-affinity recommendation feed tailored to the specified user's
    /// demonstrated category preferences. Each listing is scored by a blend of category
    /// affinity weight (70 %) and listing-level popularity (30 %).
    /// Falls back to trending when the user has fewer categorised signals than the
    /// configured <see cref="RecommendationOptions.MinAffinitySignals"/> threshold.
    /// </summary>
    /// <param name="userId">The identifier of the requesting user.</param>
    /// <param name="count">Maximum number of recommendations to return. Clamped to [1, 100].</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <exception cref="ResourceNotFoundException">Thrown when the user does not exist.</exception>
    public async Task<RecommendationFeedDto> GetAffinityRecommendationsAsync(
        Guid userId, int count = 20, CancellationToken cancellationToken = default)
    {
        count = Math.Clamp(count, 1, 100);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new ResourceNotFoundException("User", userId);

        _logger.LogInformation(
            "Generating category-affinity recommendations for user {UserId} (count={Count})", userId, count);

        var scored = await _engine.ComputeByAffinityAsync(userId, count, cancellationToken);
        var (items, strategyUsed) = await HydrateAndDiversifyAsync(scored, cancellationToken);

        var isPersonalised = scored.Count > 0 && scored[0].Reason == RecommendationReason.CategoryAffinity;

        return new RecommendationFeedDto
        {
            Items = items,
            IsPersonalised = isPersonalised,
            Strategy = strategyUsed,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Returns a similar-items panel suitable for listing detail pages.
    /// Similarity is derived from co-interaction patterns among the anchor listing's audience.
    /// </summary>
    /// <param name="listingId">The anchor listing from which to derive similarity.</param>
    /// <param name="count">Maximum number of similar listings to return. Clamped to [1, 50].</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <exception cref="ResourceNotFoundException">Thrown when the listing does not exist.</exception>
    public async Task<RecommendationFeedDto> GetSimilarListingsAsync(
        Guid listingId, int count = 10, CancellationToken cancellationToken = default)
    {
        count = Math.Clamp(count, 1, 50);

        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is null)
            throw new ResourceNotFoundException("Listing", listingId);

        _logger.LogDebug("Computing similar-item panel for listing {ListingId}", listingId);

        var scored = await _engine.ComputeSimilarAsync(listingId, count, cancellationToken);
        var (items, _) = await HydrateAndDiversifyAsync(scored, cancellationToken);

        return new RecommendationFeedDto
        {
            Items = items,
            IsPersonalised = false,
            Strategy = nameof(RecommendationStrategy.ItemBased),
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Returns the globally trending listings feed.
    /// This surface is publicly accessible and does not require authentication.
    /// Trending velocity is derived from actual signal data within the configured look-back window.
    /// </summary>
    /// <param name="count">Maximum number of trending listings to return. Clamped to [1, 100].</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    public async Task<RecommendationFeedDto> GetTrendingListingsAsync(
        int count = 20, CancellationToken cancellationToken = default)
    {
        count = Math.Clamp(count, 1, 100);

        var scored = await _engine.ComputeTrendingAsync(count, cancellationToken);
        var (items, _) = await HydrateAndDiversifyAsync(scored, cancellationToken);

        return new RecommendationFeedDto
        {
            Items = items,
            IsPersonalised = false,
            Strategy = nameof(RecommendationStrategy.Trending),
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Records a behavioural signal and notifies downstream systems via the event bus.
    /// Call this whenever a user views, saves, enquires about, or purchases a listing
    /// to keep the recommendation model current.
    /// </summary>
    /// <param name="userId">The user performing the action.</param>
    /// <param name="listingId">The listing being interacted with.</param>
    /// <param name="signalType">The nature of the interaction.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    public async Task TrackUserActivityAsync(
        Guid userId, Guid listingId, SignalType signalType,
        CancellationToken cancellationToken = default)
    {
        // Resolve the listing's category for category-affinity scoring (best-effort).
        var listing = await _listingRepository.GetByIdAsync(listingId);

        var signal = new UserActivitySignal
        {
            UserId = userId,
            ListingId = listingId,
            CategoryId = listing?.CategoryId,
            SignalType = signalType,
            Weight = (double)signalType,
            OccurredAt = DateTime.UtcNow
        };

        await _engine.RecordSignalAsync(signal, cancellationToken);

        await _eventBus.PublishAsync(new UserActivityRecordedEvent
        {
            UserId = userId,
            ListingId = listingId,
            SignalType = signalType.ToString()
        });

        _logger.LogDebug(
            "Tracked {SignalType} signal for user {UserId} on listing {ListingId}",
            signalType, userId, listingId);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<(List<RecommendationDto> items, string strategy)> HydrateAndDiversifyAsync(
        IReadOnlyList<ScoredListing> scored, CancellationToken cancellationToken)
    {
        var strategy = scored.Count > 0
            ? scored[0].Reason.ToString()
            : nameof(RecommendationStrategy.Trending);

        var hydrated = new List<RecommendationDto>(scored.Count);

        foreach (var score in scored)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var listing = await _listingRepository.GetByIdAsync(score.ListingId);
            if (listing is null) continue;

            hydrated.Add(new RecommendationDto(listing, score));
        }

        if (_options.EnableDiversification && hydrated.Count > 1)
            hydrated = ApplyDiversification(hydrated);

        return (hydrated, strategy);
    }

    private List<RecommendationDto> ApplyDiversification(List<RecommendationDto> items)
    {
        int maxPerCategory = Math.Max(1,
            (int)Math.Ceiling(items.Count * _options.MaxCategoryConcentration));

        var categoryCounts = new Dictionary<Guid, int>();
        var diversified = new List<RecommendationDto>(items.Count);

        foreach (var item in items)
        {
            categoryCounts.TryGetValue(item.CategoryId, out var seen);
            if (seen >= maxPerCategory) continue;

            diversified.Add(item);
            categoryCounts[item.CategoryId] = seen + 1;
        }

        return diversified;
    }
}
