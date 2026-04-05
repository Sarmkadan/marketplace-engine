#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace MarketplaceEngine.Recommendations;

/// <summary>
/// Thread-safe, in-memory store for user activity signals.
/// Maintains per-user interaction histories and a reverse index from listing to audience,
/// enabling both user-based and item-based collaborative filtering.
/// </summary>
/// <remarks>
/// Designed for single-instance deployments. In production, replace with a durable,
/// horizontally scalable backend — Redis Streams, Apache Cassandra, or InfluxDB
/// are all suitable depending on query and retention requirements.
/// </remarks>
public sealed class UserActivityTracker : IUserActivityTracker
{
    // userId → chronologically ordered list of interaction signals
    private readonly ConcurrentDictionary<Guid, List<UserActivitySignal>> _byUser = new();

    // listingId → set of user ids that have interacted with that listing
    private readonly ConcurrentDictionary<Guid, HashSet<Guid>> _byListing = new();

    private readonly RecommendationOptions _options;
    private readonly ILogger<UserActivityTracker> _logger;

    // Single coarse-grained write lock; acceptable given the in-memory nature of this store.
    // Replace with reader-writer semantics if read/write contention becomes measurable.
    private readonly object _writeLock = new();

    /// <summary>
    /// Initialises a new instance of <see cref="UserActivityTracker"/>.
    /// </summary>
    public UserActivityTracker(RecommendationOptions options, ILogger<UserActivityTracker> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task RecordAsync(UserActivitySignal signal, CancellationToken cancellationToken = default)
    {
        lock (_writeLock)
        {
            var userSignals = _byUser.GetOrAdd(signal.UserId, _ => []);
            userSignals.Add(signal);

            // Evict oldest signals when the per-user cap is exceeded (FIFO eviction)
            while (userSignals.Count > _options.MaxSignalsPerUser)
                userSignals.RemoveAt(0);

            _byListing
                .GetOrAdd(signal.ListingId, _ => new HashSet<Guid>())
                .Add(signal.UserId);
        }

        _logger.LogDebug(
            "Signal {SignalType} recorded — user {UserId}, listing {ListingId}",
            signal.SignalType, signal.UserId, signal.ListingId);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<UserActivitySignal>> GetUserHistoryAsync(
        Guid userId, TimeSpan? window = null, CancellationToken cancellationToken = default)
    {
        if (!_byUser.TryGetValue(userId, out var signals))
            return Task.FromResult<IReadOnlyList<UserActivitySignal>>([]);

        List<UserActivitySignal> snapshot;
        lock (_writeLock)
        {
            snapshot = window.HasValue
                ? signals.Where(s => s.OccurredAt >= DateTime.UtcNow - window.Value).ToList()
                : [.. signals];
        }

        return Task.FromResult<IReadOnlyList<UserActivitySignal>>(snapshot);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<Guid>> GetListingAudienceAsync(
        Guid listingId, CancellationToken cancellationToken = default)
    {
        if (!_byListing.TryGetValue(listingId, out var audience))
            return Task.FromResult<IReadOnlyList<Guid>>([]);

        List<Guid> snapshot;
        lock (_writeLock)
        {
            snapshot = [.. audience];
        }

        return Task.FromResult<IReadOnlyList<Guid>>(snapshot);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<UserActivitySignal>> GetSignalsInWindowAsync(
        TimeSpan window, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow - window;
        List<UserActivitySignal> result;

        lock (_writeLock)
        {
            result = _byUser.Values
                .SelectMany(signals => signals.Where(s => s.OccurredAt >= cutoff))
                .ToList();
        }

        _logger.LogDebug(
            "GetSignalsInWindowAsync: {Count} signals found within the last {Hours:F1} hours",
            result.Count, window.TotalHours);

        return Task.FromResult<IReadOnlyList<UserActivitySignal>>(result);
    }

    /// <summary>
    /// Total number of signals currently held across all users.
    /// Useful for health-check and observability endpoints.
    /// </summary>
    public int TotalSignalCount
    {
        get
        {
            lock (_writeLock)
                return _byUser.Values.Sum(l => l.Count);
        }
    }

    /// <summary>Number of distinct users who have at least one recorded signal.</summary>
    public int TrackedUserCount => _byUser.Count;

    /// <summary>Number of distinct listings present in the audience index.</summary>
    public int IndexedListingCount => _byListing.Count;
}
