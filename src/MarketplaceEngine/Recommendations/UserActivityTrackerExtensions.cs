namespace MarketplaceEngine.Recommendations;

/// <summary>
/// Extension methods for <see cref="UserActivityTracker"/>.
/// </summary>
public static class UserActivityTrackerExtensions
{
    /// <summary>
    /// Records user activity asynchronously.
    /// </summary>
    /// <param name="tracker">The <see cref="UserActivityTracker"/> instance.</param>
    /// <param name="signal">The user activity signal to record.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tracker"/> or <paramref name="signal"/> is null.</exception>
    public static async Task RecordAsync(this UserActivityTracker tracker, UserActivitySignal signal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tracker);
        ArgumentNullException.ThrowIfNull(signal);

        await tracker.RecordAsync(signal, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the user activity history for the specified user within a time window.
    /// </summary>
    /// <param name="tracker">The <see cref="UserActivityTracker"/> instance.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="window">The time window.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of user activity signals.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tracker"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="window"/> is negative.</exception>
    public static async Task<IReadOnlyList<UserActivitySignal>> GetUserActivityHistoryInWindowAsync(this UserActivityTracker tracker, Guid userId, TimeSpan window, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tracker);
        if (window < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(window), window, "Time window cannot be negative.");
        }

        var history = await tracker.GetUserHistoryAsync(userId, window, cancellationToken).ConfigureAwait(false);
        return history;
    }

    /// <summary>
    /// Gets the audience for a listing.
    /// </summary>
    /// <param name="tracker">The <see cref="UserActivityTracker"/> instance.</param>
    /// <param name="listingId">The ID of the listing.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of user IDs.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tracker"/> is null.</exception>
    public static async Task<IReadOnlyList<Guid>> GetListingAudienceAsync(this UserActivityTracker tracker, Guid listingId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tracker);

        var audience = await tracker.GetListingAudienceAsync(listingId, cancellationToken).ConfigureAwait(false);
        return audience;
    }
}
