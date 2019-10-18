// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Text.Json;

namespace MarketplaceEngine.Infrastructure.Caching;

/// <summary>
/// In-memory caching service for storing frequently accessed data.
/// In production, this would be replaced with Redis for distributed caching.
/// Implements TTL (Time-To-Live) for automatic cache expiration.
/// </summary>
public class CacheService
{
    private readonly ConcurrentDictionary<string, CacheItem> _cache = new();
    private readonly ILogger<CacheService> _logger;
    private readonly CancellationTokenSource _cleanupTokenSource = new();

    public CacheService(ILogger<CacheService> logger)
    {
        _logger = logger;
        // Start background cleanup task to remove expired items
        _ = StartCleanupTaskAsync();
    }

    /// <summary>
    /// Retrieves a cached value by key.
    /// Returns null if key doesn't exist or has expired.
    /// </summary>
    public async Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return default;

        if (!_cache.TryGetValue(key, out var item))
        {
            _logger.LogDebug("Cache miss for key: {Key}", key);
            return default;
        }

        // Check if item has expired
        if (item.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogDebug("Cache item expired: {Key}", key);
            _cache.TryRemove(key, out _);
            return default;
        }

        _logger.LogDebug("Cache hit for key: {Key}", key);

        // Deserialize JSON back to object
        if (item.Value is JsonElement jsonElement)
        {
            return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
        }

        return (T?)item.Value;
    }

    /// <summary>
    /// Stores a value in cache with optional TTL.
    /// Default TTL is 5 minutes if not specified.
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        if (string.IsNullOrWhiteSpace(key) || value == null)
            return;

        var expiresAt = DateTime.UtcNow.Add(ttl ?? TimeSpan.FromMinutes(5));

        var cacheItem = new CacheItem
        {
            Key = key,
            Value = value,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        _cache.AddOrUpdate(key, cacheItem, (_, _) => cacheItem);

        _logger.LogDebug("Cache set for key: {Key}, expires at: {ExpiresAt}", key, expiresAt);
    }

    /// <summary>
    /// Removes a specific key from cache.
    /// </summary>
    public async Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        // Support wildcard removal (e.g., "user:*")
        if (key.EndsWith("*"))
        {
            var prefix = key[..^1];
            var keysToRemove = _cache.Keys.Where(k => k.StartsWith(prefix)).ToList();

            foreach (var k in keysToRemove)
            {
                _cache.TryRemove(k, out _);
            }

            _logger.LogDebug("Removed {Count} cache items matching pattern: {Pattern}", keysToRemove.Count, key);
            return;
        }

        if (_cache.TryRemove(key, out _))
        {
            _logger.LogDebug("Cache item removed: {Key}", key);
        }
    }

    /// <summary>
    /// Clears all items from cache.
    /// Use with caution in production.
    /// </summary>
    public async Task ClearAsync()
    {
        var count = _cache.Count;
        _cache.Clear();
        _logger.LogWarning("Cache cleared, {Count} items removed", count);
    }

    /// <summary>
    /// Gets cache statistics (size, hit rate, etc).
    /// </summary>
    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        var stats = new CacheStatistics
        {
            TotalItems = _cache.Count,
            TotalMemoryMb = GetEstimatedMemoryUsage() / (1024 * 1024),
            OldestItemAge = GetOldestItemAge()
        };

        return stats;
    }

    private double GetEstimatedMemoryUsage()
    {
        // Rough estimation of memory usage
        return _cache.Values.Sum(item =>
            (item.Key?.Length ?? 0) +
            (item.Value?.ToString()?.Length ?? 0) +
            64); // Base object overhead
    }

    private TimeSpan GetOldestItemAge()
    {
        if (_cache.Count == 0)
            return TimeSpan.Zero;

        var oldest = _cache.Values.Min(item => item.CreatedAt);
        return DateTime.UtcNow - oldest;
    }

    private async Task StartCleanupTaskAsync()
    {
        // Periodically clean up expired items every 60 seconds
        while (!_cleanupTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(60), _cleanupTokenSource.Token);

                var expiredKeys = _cache
                    .Where(kvp => kvp.Value.ExpiresAt < DateTime.UtcNow)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    _cache.TryRemove(key, out _);
                }

                if (expiredKeys.Count > 0)
                {
                    _logger.LogDebug("Cleanup removed {Count} expired cache items", expiredKeys.Count);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when service is shutting down
                break;
            }
        }
    }

    private class CacheItem
    {
        public string Key { get; set; } = string.Empty;
        public object? Value { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

/// <summary>
/// Statistics about cache performance.
/// </summary>
public class CacheStatistics
{
    public int TotalItems { get; set; }
    public long TotalMemoryMb { get; set; }
    public TimeSpan OldestItemAge { get; set; }
}
