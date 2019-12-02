namespace MarketplaceEngine.Services;

using System.Collections.Concurrent;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Recommendations;
using MarketplaceEngine.Repositories;

/// <summary>In-memory per-user watchlist of listing ids with recommendation-signal integration.</summary>
public sealed class WatchlistService
{
    private readonly IListingRepository _listingRepository;
    private readonly IRecommendationEngine _recommendationEngine;
    private readonly ConcurrentDictionary<Guid, HashSet<Guid>> _watchlists = new();

    public WatchlistService(IListingRepository listingRepository, IRecommendationEngine recommendationEngine)
    {
        _listingRepository = listingRepository;
        _recommendationEngine = recommendationEngine;
    }

    /// <summary>Adds listing to user's watchlist. Returns false if already watched. Verifies listing exists (throws KeyNotFoundException otherwise), increments interest count, and records a Save signal.</summary>
    public async Task<bool> AddAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default)
    {
        if (!await _listingRepository.ExistsAsync(listingId))
        {
            throw new KeyNotFoundException($"Listing with ID {listingId} not found.");
        }

        var userWatchlist = _watchlists.GetOrAdd(userId, _ => new HashSet<Guid>());

        lock (userWatchlist)
        {
            if (!userWatchlist.Add(listingId))
            {
                return false;
            }
        }

        await _listingRepository.IncrementInterestCountAsync(listingId);

        var signal = new UserActivitySignal
        {
            UserId = userId,
            ListingId = listingId,
            SignalType = SignalType.Save
        };

        await _recommendationEngine.RecordSignalAsync(signal, cancellationToken);

        return true;
    }

    /// <summary>Removes listing from user's watchlist; returns false if it was not watched.</summary>
    public Task<bool> RemoveAsync(Guid userId, Guid listingId)
    {
        if (_watchlists.TryGetValue(userId, out var userWatchlist))
        {
            lock (userWatchlist)
            {
                return Task.FromResult(userWatchlist.Remove(listingId));
            }
        }

        return Task.FromResult(false);
    }

    /// <summary>True if the user watches the listing.</summary>
    public bool IsWatching(Guid userId, Guid listingId)
    {
        if (_watchlists.TryGetValue(userId, out var userWatchlist))
        {
            lock (userWatchlist)
            {
                return userWatchlist.Contains(listingId);
            }
        }

        return false;
    }

    /// <summary>Resolves the user's watched listings via the repository, skipping ids that no longer exist.</summary>
    public async Task<IReadOnlyList<Listing>> GetWatchedListingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = new List<Listing>();

        if (_watchlists.TryGetValue(userId, out var userWatchlist))
        {
            Guid[] listingIds;
            lock (userWatchlist)
            {
                listingIds = userWatchlist.ToArray();
            }

            foreach (var id in listingIds)
            {
                var listing = await _listingRepository.GetByIdAsync(id);
                if (listing != null)
                {
                    result.Add(listing);
                }
            }
        }

        return result.AsReadOnly();
    }

    /// <summary>How many users currently watch the listing.</summary>
    public int GetWatcherCount(Guid listingId)
    {
        int count = 0;
        foreach (var userWatchlist in _watchlists.Values)
        {
            lock (userWatchlist)
            {
                if (userWatchlist.Contains(listingId))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>Ids of all users watching the listing (for notifications).</summary>
    public IReadOnlyList<Guid> GetWatchers(Guid listingId)
    {
        var watchers = new List<Guid>();

        foreach (var kvp in _watchlists)
        {
            lock (kvp.Value)
            {
                if (kvp.Value.Contains(listingId))
                {
                    watchers.Add(kvp.Key);
                }
            }
        }

        return watchers.AsReadOnly();
    }
}
