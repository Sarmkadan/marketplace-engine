namespace MarketplaceEngine.Services;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MarketplaceEngine.Domain.ValueObjects;

/// <summary>One recorded price point for a listing.</summary>
public sealed record PricePoint(Money Price, DateTime RecordedAt);

/// <summary>Aggregate price statistics for a listing.</summary>
public sealed record PriceStatistics(decimal Min, decimal Max, decimal Average, int ChangeCount, string CurrencyCode);

/// <summary>Tracks per-listing price history in memory and detects price drops.</summary>
public sealed class PriceHistoryTracker
{
    private readonly ConcurrentDictionary<Guid, List<PricePoint>> _history = new();

    /// <summary>Records a price. Ignores the call (returns false) if equal to the latest recorded price; throws ArgumentException on currency mismatch with existing history.</summary>
    public bool RecordPrice(Guid listingId, Money price)
    {
        if (_history.TryGetValue(listingId, out var history))
        {
            if (history.Count > 0 && history.Last().Price.Equals(price))
                return false;

            if (!history.Any() || history.Last().Price.CurrencyCode == price.CurrencyCode)
            {
                history.Add(new PricePoint(price, DateTime.UtcNow));
                return true;
            }
            else
            {
                throw new ArgumentException("Currency mismatch with existing history", nameof(price));
            }
        }
        else
        {
            _history.TryAdd(listingId, new List<PricePoint> { new PricePoint(price, DateTime.UtcNow) });
            return true;
        }
    }

    /// <summary>Chronological history, oldest first; empty list if unknown listing.</summary>
    public IReadOnlyList<PricePoint> GetHistory(Guid listingId)
    {
        if (_history.TryGetValue(listingId, out var history))
            return history.AsReadOnly();
        return new List<PricePoint>().AsReadOnly();
    }

    /// <summary>Min/max/average over history; null if no history.</summary>
    public PriceStatistics? GetStatistics(Guid listingId)
    {
        if (_history.TryGetValue(listingId, out var history) && history.Any())
        {
            var min = history.Min(p => p.Price.Amount);
            var max = history.Max(p => p.Price.Amount);
            var average = history.Average(p => p.Price.Amount);
            var currencyCode = history.First().Price.CurrencyCode;
            return new PriceStatistics(min, max, average, history.Count, currencyCode);
        }
        return null;
    }

    /// <summary>Percentage drop (0..100) from previous price to latest; 0 if fewer than 2 points or price rose.</summary>
    public decimal GetLatestDropPercent(Guid listingId)
    {
        if (_history.TryGetValue(listingId, out var history) && history.Count >= 2)
        {
            var previousPrice = history[history.Count - 2].Price.Amount;
            var latestPrice = history.Last().Price.Amount;
            if (latestPrice < previousPrice)
                return ((previousPrice - latestPrice) / previousPrice) * 100;
        }
        return 0;
    }

    /// <summary>True if latest drop percent >= threshold (threshold must be > 0, else ArgumentOutOfRangeException).</summary>
    public bool HasPriceDrop(Guid listingId, decimal thresholdPercent = 10m)
    {
        if (thresholdPercent <= 0)
            throw new ArgumentOutOfRangeException(nameof(thresholdPercent), "Threshold must be greater than 0");
        var dropPercent = GetLatestDropPercent(listingId);
        return dropPercent >= thresholdPercent;
    }

    /// <summary>Removes history older than maxAge across all listings; returns number of removed points.</summary>
    public int Prune(TimeSpan maxAge)
    {
        var removedCount = 0;
        var cutoff = DateTime.UtcNow - maxAge;
        foreach (var listingId in _history.Keys.ToList())
        {
            var history = _history[listingId];
            var pointsToRemove = history.TakeWhile(p => p.RecordedAt < cutoff).ToList();
            if (pointsToRemove.Any())
            {
                removedCount += pointsToRemove.Count;
                history.RemoveAll(p => pointsToRemove.Contains(p));
                if (history.Count == 0)
                    _history.TryRemove(listingId, out _);
            }
        }
        return removedCount;
    }
}
