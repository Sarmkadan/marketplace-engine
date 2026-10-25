#nullable enable
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
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromSeconds(60);
    private const int BaseObjectOverheadBytes = 64;
    private const int BytesPerMegabyte = 1024 * 1024;
    private const string WildcardSuffix = "*";

    private readonly ConcurrentDictionary<string, CacheItem> _cache = new();
    private readonly ILogger<CacheService> _logger;
    private readonly CancellationTokenSource _cleanupTokenSource = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheService"/> class
    /// and starts the background task that removes expired items.
    /// </summary>
    /// <param name="logger">Logger used for cache diagnostics.</param>
    public CacheService(ILogger<CacheService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        // Start background cleanup task to remove expired items
        _ = StartCleanupTaskAsync();
    }

    /// <summary>
    /// Retrieves a cached value by key.
    /// Returns null if key doesn't exist or has expired.
    /// </summary>
    /// <typeparam name="T">The type to which the cached value is returned.</typeparam>
    /// <param name="key">The cache key. Null, empty, or whitespace keys return the default value.</param>
    /// <returns>The cached value, or the default value of <typeparamref name="T"/> if not found or expired.</returns>
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
    /// <typeparam name="T">The type of the value to store.</typeparam>
    /// <param name="key">The cache key. Null, empty, or whitespace keys are ignored.</param>
    /// <param name="value">The value to store. Null values are ignored.</param>
    /// <param name="ttl">Optional time-to-live for the entry. Defaults to 5 minutes when null.</param>
    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        if (string.IsNullOrWhiteSpace(key) || value is null)
            return;

        var expiresAt = DateTime.UtcNow.Add(ttl ?? DefaultTtl);

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
    /// Keys ending with <c>*</c> remove every key that starts with the prefix before the wildcard.
    /// </summary>
    /// <param name="key">The key to remove, or a prefix pattern ending with <c>*</c>. Null, empty, or whitespace keys are ignored.</param>
    public async Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        // Support wildcard removal (e.g., "user:*")
        if (key.EndsWith(WildcardSuffix))
        {
            var prefix = key[..^WildcardSuffix.Length];
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
    /// <returns>A <see cref="CacheStatistics"/> snapshot of the current cache state.</returns>
    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        var stats = new CacheStatistics
        {
            TotalItems = _cache.Count,
            TotalMemoryMb = (long)(GetEstimatedMemoryUsage() / BytesPerMegabyte), // Hotfix: Explicit cast to long
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
            BaseObjectOverheadBytes); // Base object overhead
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
                await Task.Delay(CleanupInterval, _cleanupTokenSource.Token);

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
    /// <summary>
    /// Gets or sets the number of items currently held in the cache.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Gets or sets the rough estimated memory usage of the cache, in megabytes.
    /// </summary>
    public long TotalMemoryMb { get; set; }

    /// <summary>
    /// Gets or sets the age of the oldest item in the cache, or zero when the cache is empty.
    /// </summary>
    public TimeSpan OldestItemAge { get; set; }
}
