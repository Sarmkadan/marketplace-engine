#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketplaceEngine.Infrastructure.Configuration;

/// <summary>
/// Extension methods that provide convenient, read‑only accessors and helpers
/// for <see cref="MarketplaceConfiguration"/>.
/// </summary>
public static class MarketplaceConfigurationExtensions
{
    /// <summary>
    /// Returns the time‑to‑live (TTL) in minutes for the specified cache.
    /// </summary>
    /// <param name="configuration">The marketplace configuration.</param>
    /// <param name="cacheName">
    /// The name of the cache. Supported values (case‑insensitive) are:
    /// <c>default</c>, <c>listing</c>, <c>user</c>, <c>category</c>, <c>search</c>.
    /// </param>
    /// <returns>The TTL in minutes for the requested cache.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="cacheName"/> is <c>null</c>, empty or whitespace.</exception>
    /// <exception cref="KeyNotFoundException">The <paramref name="cacheName"/> is not recognised.</exception>
    public static int GetCacheTtl(this MarketplaceConfiguration configuration, string cacheName)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheName);

        return cacheName.Trim().ToLowerInvariant() switch
        {
            "default"   => configuration.Caching.DefaultTtlMinutes,
            "listing"   => configuration.Caching.ListingCacheTtlMinutes,
            "user"      => configuration.Caching.UserCacheTtlMinutes,
            "category"  => configuration.Caching.CategoryCacheTtlMinutes,
            "search"    => configuration.Caching.SearchResultCacheTtlMinutes,
            _ => throw new KeyNotFoundException($"Cache name '{cacheName}' is not recognised.")
        };
    }

    /// <summary>
    /// Determines whether the supplied request path is exempt from rate‑limiting.
    /// </summary>
    /// <param name="configuration">The marketplace configuration.</param>
    /// <param name="requestPath">The request path to test (e.g. <c>/api/v1/health</c>).</param>
    /// <returns><c>true</c> if the path is listed in <c>RateLimit.ExemptPaths</c>; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="requestPath"/> is <c>null</c>, empty or whitespace.</exception>
    public static bool IsRateLimitExempt(this MarketplaceConfiguration configuration, string requestPath)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestPath);

        return (configuration.RateLimit.ExemptPaths ?? Array.Empty<string>()).Contains(
            requestPath.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Builds a read‑only collection of HTTP headers required for calls to the
    /// configured dropship integration API.
    /// </summary>
    /// <param name="configuration">The marketplace configuration.</param>
    /// <returns>
    /// An <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing at least the
    /// <c>Authorization</c> header when a key is configured; otherwise an empty
    /// dictionary.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <c>null</c>.</exception>
    public static IReadOnlyDictionary<string, string> GetIntegrationHeaders(this MarketplaceConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return string.IsNullOrWhiteSpace(configuration.Integration.DropshipApiKey)
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Authorization"] = $"Bearer {configuration.Integration.DropshipApiKey}"
            };
    }

    /// <summary>
    /// Retrieves the effective rate‑limit values as a tuple.
    /// </summary>
    /// <param name="configuration">The marketplace configuration.</param>
    /// <returns>
    /// A tuple containing <c>RequestsPerMinute</c> and <c>RequestsPerHour</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <c>null</c>.</exception>
    public static (int RequestsPerMinute, int RequestsPerHour) GetEffectiveRateLimit(this MarketplaceConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return (configuration.RateLimit.MaxRequestsPerMinute, configuration.RateLimit.MaxRequestsPerHour);
    }
}
