#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace MarketplaceEngine.Infrastructure.Caching;

/// <summary>
/// Extension methods for <see cref="CacheService"/> providing additional caching functionality.
/// </summary>
public static class CacheServiceExtensions
{
    /// <summary>
    /// Attempts to get a value from cache, and if not found, executes the provided function
    /// to generate the value and stores it in cache.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="cacheService">The cache service instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="valueFactory">The function to execute if the value is not in cache.</param>
    /// <param name="ttl">Optional time-to-live for the cached value. Defaults to 5 minutes.</param>
    /// <returns>The cached or newly generated value.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cacheService"/> or <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="valueFactory"/> is null.</exception>
    public static async Task<T> GetOrCreateAsync<T>(
        this CacheService cacheService,
        string key,
        Func<Task<T>> valueFactory,
        TimeSpan? ttl = null)
    {
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valueFactory);

        var cachedValue = await cacheService.GetAsync<T>(key);
        if (cachedValue is not null)
        {
            return cachedValue;
        }

        var value = await valueFactory();
        await cacheService.SetAsync(key, value, ttl);
        return value;
    }

    /// <summary>
    /// Attempts to get a value from cache, and if not found, executes the provided function
    /// to generate the value and stores it in cache.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="cacheService">The cache service instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="valueFactory">The function to execute if the value is not in cache.</param>
    /// <param name="ttl">Optional time-to-live for the cached value. Defaults to 5 minutes.</param>
    /// <returns>The cached or newly generated value.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cacheService"/> or <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="valueFactory"/> is null.</exception>
    public static async Task<T> GetOrCreateAsync<T>(
        this CacheService cacheService,
        string key,
        Func<T> valueFactory,
        TimeSpan? ttl = null)
    {
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valueFactory);

        var cachedValue = await cacheService.GetAsync<T>(key);
        if (cachedValue is not null)
        {
            return cachedValue;
        }

        var value = valueFactory();
        await cacheService.SetAsync(key, value, ttl);
        return value;
    }

    /// <summary>
    /// Gets all cache keys that match the specified pattern.
    /// </summary>
    /// <param name="cacheService">The cache service instance.</param>
    /// <param name="pattern">The search pattern (supports wildcards like "*user*" or "user:*").</param>
    /// <returns>Read-only list of matching cache keys.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cacheService"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pattern"/> is null.</exception>
    public static async Task<IReadOnlyList<string>> GetKeysAsync(
        this CacheService cacheService,
        string pattern)
    {
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(pattern);

        if (string.IsNullOrWhiteSpace(pattern))
        {
            return Array.Empty<string>();
        }

        var matchingKeys = (cacheService
            .GetType()
            .GetField("_cache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(cacheService) as System.Collections.Concurrent.ConcurrentDictionary<string, object>)?.Keys
            .Where(k => k.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matchingKeys?.AsReadOnly() ?? Array.Empty<string>().AsReadOnly();
    }

    /// <summary>
    /// Gets cache statistics formatted as a human-readable string.
    /// </summary>
    /// <param name="cacheService">The cache service instance.</param>
    /// <param name="format">Optional format string. Use "mb" for megabytes, "gb" for gigabytes.</param>
    /// <returns>Formatted statistics string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cacheService"/> is null.</exception>
    public static async Task<string> GetStatisticsStringAsync(
        this CacheService cacheService,
        string? format = null)
    {
        ArgumentNullException.ThrowIfNull(cacheService);

        var stats = await cacheService.GetStatisticsAsync();

        var memoryFormat = format?.ToLowerInvariant() switch
        {
            "gb" => $"{stats.TotalMemoryMb / 1024.0:F2} GB",
            "mb" or null => $"{stats.TotalMemoryMb} MB",
            _ => $"{stats.TotalMemoryMb} MB"
        };

        var oldestItemFormatted = stats.OldestItemAge.TotalDays >= 1
            ? $"{stats.OldestItemAge.TotalDays:F1} days"
            : stats.OldestItemAge.TotalHours >= 1
                ? $"{stats.OldestItemAge.TotalHours:F1} hours"
                : stats.OldestItemAge.TotalMinutes >= 1
                    ? $"{stats.OldestItemAge.TotalMinutes:F1} minutes"
                    : $"{stats.OldestItemAge.TotalSeconds:F1} seconds";

        return $"Cache Statistics:\n" +
               $"  Items: {stats.TotalItems}\n" +
               $"  Memory: {memoryFormat}\n" +
               $"  Oldest Item: {oldestItemFormatted}\n" +
               $"  Hit Rate: N/A";
    }
}