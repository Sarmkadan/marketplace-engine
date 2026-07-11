#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MarketplaceEngine.Middleware;

/// <summary>
/// Extension methods for RateLimitingMiddleware providing additional functionality
/// and convenience methods for rate limiting operations.
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>
    /// Adds rate limiting middleware to the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The IApplicationBuilder instance</param>
    /// <returns>The IApplicationBuilder instance for method chaining</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is null.</exception>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<RateLimitingMiddleware>();
    }

    /// <summary>
    /// Gets the current request count for the client IP address from the current request context.
    /// </summary>
    /// <param name="middleware">The RateLimitingMiddleware instance</param>
    /// <param name="context">The HttpContext containing the client IP</param>
    /// <returns>The current request count, or 0 if not tracked</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> or <paramref name="context"/> is null.</exception>
    public static int GetCurrentRequestCount(this RateLimitingMiddleware middleware, HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(context);

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return middleware.GetRequestCount(clientIp);
    }

    /// <summary>
    /// Gets the current request count for a specific IP address.
    /// </summary>
    /// <param name="middleware">The RateLimitingMiddleware instance</param>
    /// <param name="clientIp">The client IP address to check</param>
    /// <returns>The current request count, or 0 if not tracked</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="clientIp"/> is null or whitespace.</exception>
    public static int GetRequestCount(this RateLimitingMiddleware middleware, string clientIp)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientIp, nameof(clientIp));

        var bucket = RateLimitingMiddleware.RateLimitBuckets.GetValueOrDefault(clientIp);
        return bucket?.RequestCount ?? 0;
    }

    /// <summary>
    /// Checks if the current request's IP address has exceeded its rate limit.
    /// </summary>
    /// <param name="middleware">The RateLimitingMiddleware instance</param>
    /// <param name="context">The HttpContext containing the client IP</param>
    /// <returns>True if rate limit is exceeded, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> or <paramref name="context"/> is null.</exception>
    public static bool IsCurrentRateLimitExceeded(this RateLimitingMiddleware middleware, HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(context);

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return middleware.IsRateLimitExceeded(clientIp);
    }

    /// <summary>
    /// Checks if a specific IP address has exceeded its rate limit.
    /// </summary>
    /// <param name="middleware">The RateLimitingMiddleware instance</param>
    /// <param name="clientIp">The client IP address to check</param>
    /// <returns>True if rate limit is exceeded, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="clientIp"/> is null or whitespace.</exception>
    public static bool IsRateLimitExceeded(this RateLimitingMiddleware middleware, string clientIp)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientIp, nameof(clientIp));

        return middleware.GetRequestCount(clientIp) > RateLimitingMiddleware.MaxRequestsPerMinute;
    }

    /// <summary>
    /// Gets the current rate limit window information for the client IP address from the current request context.
    /// </summary>
    /// <param name="middleware">The RateLimitingMiddleware instance</param>
    /// <param name="context">The HttpContext containing the client IP</param>
    /// <returns>A tuple containing WindowStart and RequestCount, or default values if not tracked</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> or <paramref name="context"/> is null.</exception>
    public static (DateTime WindowStart, int RequestCount) GetCurrentRateLimitWindow(this RateLimitingMiddleware middleware, HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(context);

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return middleware.GetRateLimitWindow(clientIp);
    }

    /// <summary>
    /// Gets the current rate limit window information for a specific IP address.
    /// </summary>
    /// <param name="middleware">The RateLimitingMiddleware instance</param>
    /// <param name="clientIp">The client IP address to check</param>
    /// <returns>A tuple containing WindowStart and RequestCount, or default values if not tracked</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="clientIp"/> is null or whitespace.</exception>
    public static (DateTime WindowStart, int RequestCount) GetRateLimitWindow(this RateLimitingMiddleware middleware, string clientIp)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientIp, nameof(clientIp));

        var bucket = RateLimitingMiddleware.RateLimitBuckets.GetValueOrDefault(clientIp);
        return bucket is null
            ? (DateTime.MinValue, 0)
            : (bucket.WindowStart, bucket.RequestCount);
    }

    /// <summary>
    /// Resets the rate limit counter for the current request's IP address.
    /// Useful for testing or administrative purposes.
    /// </summary>
    /// <param name="middleware">The RateLimitingMiddleware instance</param>
    /// <param name="context">The HttpContext containing the client IP</param>
    /// <returns>True if the bucket was found and removed, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> or <paramref name="context"/> is null.</exception>
    public static bool ResetCurrentRateLimit(this RateLimitingMiddleware middleware, HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(context);

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return middleware.ResetRateLimit(clientIp);
    }

    /// <summary>
    /// Resets the rate limit counter for a specific IP address.
    /// Useful for testing or administrative purposes.
    /// </summary>
    /// <param name="middleware">The RateLimitingMiddleware instance</param>
    /// <param name="clientIp">The client IP address to reset</param>
    /// <returns>True if the bucket was found and removed, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="clientIp"/> is null or whitespace.</exception>
    public static bool ResetRateLimit(this RateLimitingMiddleware middleware, string clientIp)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientIp, nameof(clientIp));

        return RateLimitingMiddleware.RateLimitBuckets.TryRemove(clientIp, out _);
    }
}
