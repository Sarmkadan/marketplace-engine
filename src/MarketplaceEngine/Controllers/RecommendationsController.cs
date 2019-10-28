#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.DTOs;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Recommendations;
using MarketplaceEngine.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Phase 2 controller surface for the collaborative filtering recommendation engine.
/// Exposes personalised feeds, similar-item panels, trending galleries,
/// category-affinity recommendations, activity tracking, and a diagnostics snapshot.
/// </summary>
/// <remarks>
/// This controller wraps <see cref="RecommendationService"/> and mirrors the minimal-API
/// endpoints registered by <c>RecommendationExtensions.MapRecommendationEndpoints</c>,
/// adding richer OpenAPI metadata, typed <see cref="ProducesResponseTypeAttribute"/> annotations,
/// and an additional diagnostics endpoint not available in the v1 surface.
/// </remarks>
[ApiController]
[Route("api/v2/recommendations")]
[Produces("application/json")]
public class RecommendationsController : ControllerBase
{
    private readonly RecommendationService _service;
    private readonly IUserActivityTracker _activityTracker;
    private readonly ILogger<RecommendationsController> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="RecommendationsController"/>.
    /// </summary>
    public RecommendationsController(
        RecommendationService service,
        IUserActivityTracker activityTracker,
        ILogger<RecommendationsController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _activityTracker = activityTracker ?? throw new ArgumentNullException(nameof(activityTracker));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Returns globally trending listings based on recent signal velocity within the
    /// configured trending window. Suitable for public discovery surfaces and unauthenticated users.
    /// Falls back to listing-model view counts when no activity signals exist.
    /// </summary>
    /// <param name="count">Maximum number of results (1–100, default 20).</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    [HttpGet("trending")]
    [ProducesResponseType(typeof(RecommendationFeedDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrending(
        [FromQuery] int count = 20,
        CancellationToken cancellationToken = default)
    {
        var feed = await _service.GetTrendingListingsAsync(count, cancellationToken);
        return Ok(feed);
    }

    /// <summary>
    /// Returns a personalised recommendation feed for the specified user using collaborative
    /// filtering. Automatically falls back to trending content for cold-start users who have
    /// fewer interactions than the configured minimum-overlap threshold.
    /// </summary>
    /// <param name="userId">Identifier of the user requesting recommendations.</param>
    /// <param name="count">Maximum number of results (1–100, default 20).</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    [HttpGet("users/{userId:guid}")]
    [ProducesResponseType(typeof(RecommendationFeedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForUser(
        Guid userId,
        [FromQuery] int count = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var feed = await _service.GetRecommendationsForUserAsync(userId, count, cancellationToken);
            return Ok(feed);
        }
        catch (ResourceNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Returns category-affinity recommendations derived from the user's demonstrated category
    /// preferences. Each listing is scored as a blend of normalised category affinity (70 %)
    /// and listing popularity (30 %). Falls back to trending for cold-start users.
    /// </summary>
    /// <param name="userId">Identifier of the user requesting recommendations.</param>
    /// <param name="count">Maximum number of results (1–100, default 20).</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    [HttpGet("users/{userId:guid}/affinity")]
    [ProducesResponseType(typeof(RecommendationFeedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByAffinity(
        Guid userId,
        [FromQuery] int count = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var feed = await _service.GetAffinityRecommendationsAsync(userId, count, cancellationToken);
            return Ok(feed);
        }
        catch (ResourceNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Returns listings similar to the specified anchor listing using co-interaction analysis.
    /// Suitable for "You may also like" panels on listing detail pages.
    /// Falls back to trending when the anchor listing has no recorded audience.
    /// </summary>
    /// <param name="listingId">Identifier of the anchor listing.</param>
    /// <param name="count">Maximum number of results (1–50, default 10).</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    [HttpGet("listings/{listingId:guid}/similar")]
    [ProducesResponseType(typeof(RecommendationFeedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSimilar(
        Guid listingId,
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var feed = await _service.GetSimilarListingsAsync(listingId, count, cancellationToken);
            return Ok(feed);
        }
        catch (ResourceNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Records a behavioural signal to improve future personalised recommendations.
    /// Call this endpoint whenever a user views, saves, enquires about, or purchases a listing.
    /// The signal is persisted synchronously and relevant recommendation caches are invalidated.
    /// </summary>
    /// <param name="request">The activity signal payload.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    [HttpPost("track")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TrackActivity(
        [FromBody] TrackActivityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<SignalType>(request.SignalType, ignoreCase: true, out var signalType))
        {
            return BadRequest(new
            {
                error = $"Unknown signal type '{request.SignalType}'.",
                validValues = Enum.GetNames<SignalType>()
            });
        }

        await _service.TrackUserActivityAsync(
            request.UserId, request.ListingId, signalType, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Returns a live diagnostic snapshot of the recommendation engine's runtime state,
    /// including activity tracker counters and signal store statistics.
    /// Intended for use by observability dashboards and operations tooling.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    [HttpGet("diagnostics")]
    [ProducesResponseType(typeof(RecommendationDiagnosticsReport), StatusCodes.Status200OK)]
    public Task<IActionResult> GetDiagnostics(CancellationToken cancellationToken = default)
    {
        var trackerStats = _activityTracker is UserActivityTracker concrete
            ? new ActivityTrackerStats(
                concrete.TotalSignalCount,
                concrete.TrackedUserCount,
                concrete.IndexedListingCount)
            : ActivityTrackerStats.Unknown;

        var report = new RecommendationDiagnosticsReport
        {
            TrackerStats = trackerStats,
            GeneratedAt = DateTime.UtcNow
        };

        _logger.LogDebug(
            "Diagnostics snapshot: {Signals} signals, {Users} users, {Listings} listings indexed",
            trackerStats.TotalSignals, trackerStats.TrackedUsers, trackerStats.IndexedListings);

        return Task.FromResult<IActionResult>(Ok(report));
    }
}
