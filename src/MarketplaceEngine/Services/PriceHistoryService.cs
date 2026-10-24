#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Services;

using System.Collections.Concurrent;
using MarketplaceEngine.Domain.Models;

/// <summary>
/// In-memory price history per listing. Registered as a singleton, like the rest of the in-memory services.
/// </summary>
public sealed class PriceHistoryService
{
    private readonly ConcurrentDictionary<Guid, List<PriceHistoryEntry>> _history = new();

    /// <summary>
    /// Appends a price point for the listing, stamped with the current UTC time.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the price is negative.</exception>
    public void Record(Guid listingId, decimal price)
    {
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

        var entry = new PriceHistoryEntry
        {
            ListingId = listingId,
            Price = price,
            ChangedAtUtc = DateTime.UtcNow
        };

        var history = _history.GetOrAdd(listingId, _ => new List<PriceHistoryEntry>());
        lock (history)
        {
            history.Add(entry);
        }
    }

    /// <summary>
    /// Returns a snapshot of the listing's price history, oldest first. Empty if the listing has no history.
    /// </summary>
    public IReadOnlyList<PriceHistoryEntry> GetHistory(Guid listingId)
    {
        if (!_history.TryGetValue(listingId, out var history))
            return Array.Empty<PriceHistoryEntry>();

        lock (history)
        {
            return history.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Percentage change from the first recorded price to the last one.
    /// Returns null if there are fewer than two points or the first price is zero (change is undefined).
    /// </summary>
    public decimal? PercentChange(Guid listingId)
    {
        if (!_history.TryGetValue(listingId, out var history))
            return null;

        lock (history)
        {
            if (history.Count < 2)
                return null;

            var first = history[0].Price;
            var last = history[^1].Price;
            if (first == 0)
                return null;

            return (last - first) / first * 100m;
        }
    }
}
