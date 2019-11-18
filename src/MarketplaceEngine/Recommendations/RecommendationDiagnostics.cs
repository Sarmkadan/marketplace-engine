// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Recommendations;

/// <summary>
/// A live diagnostic snapshot of the recommendation engine's runtime state.
/// Returned by the <c>GET /api/v2/recommendations/diagnostics</c> endpoint and
/// intended for consumption by observability dashboards, health-check tooling,
/// and on-call runbooks.
/// </summary>
public sealed record RecommendationDiagnosticsReport
{
    /// <summary>
    /// Current counters from the activity tracking store.
    /// Reflects the engine's effective data density at the moment of the snapshot.
    /// </summary>
    public required ActivityTrackerStats TrackerStats { get; init; }

    /// <summary>UTC timestamp at which this diagnostic snapshot was generated.</summary>
    public required DateTime GeneratedAt { get; init; }

    /// <summary>Human-readable health assessment derived from tracker statistics.</summary>
    public string HealthSummary => TrackerStats switch
    {
        { TotalSignals: 0 }                          => "Cold start — no interaction signals recorded yet.",
        { TotalSignals: < 100, TrackedUsers: < 10 }  => "Warming up — limited personalisation data available.",
        { TotalSignals: < 1_000, TrackedUsers: < 50 } => "Operational — basic personalisation active.",
        _                                             => "Healthy — sufficient signal density for collaborative filtering."
    };
}

/// <summary>
/// Counters derived from the <see cref="UserActivityTracker"/> at the time of a diagnostic snapshot.
/// Exposes the raw data density metrics that determine which recommendation strategies
/// will be active vs. falling back to trending for a given user.
/// </summary>
/// <param name="TotalSignals">Total interaction signals held in memory across all users.</param>
/// <param name="TrackedUsers">Number of distinct users with at least one recorded signal.</param>
/// <param name="IndexedListings">Number of distinct listings present in the audience index.</param>
public sealed record ActivityTrackerStats(int TotalSignals, int TrackedUsers, int IndexedListings)
{
    /// <summary>
    /// Sentinel value returned when the underlying tracker implementation does not expose
    /// diagnostic counters (e.g. a custom <see cref="IUserActivityTracker"/> implementation).
    /// </summary>
    public static readonly ActivityTrackerStats Unknown = new(0, 0, 0);

    /// <summary>
    /// Average number of signals recorded per tracked user.
    /// Returns <c>0</c> when no users are tracked.
    /// </summary>
    public double AverageSignalsPerUser =>
        TrackedUsers == 0 ? 0.0 : Math.Round((double)TotalSignals / TrackedUsers, 2);

    /// <summary>
    /// Average number of audience members per indexed listing.
    /// Returns <c>0</c> when no listings are indexed.
    /// Useful for estimating item-based CF coverage.
    /// </summary>
    public double AverageAudiencePerListing =>
        IndexedListings == 0 ? 0.0 : Math.Round((double)TrackedUsers / IndexedListings, 2);
}
