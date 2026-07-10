#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        return app.UseMiddleware<RateLimitingMiddleware>();
    }

    /// <summary>
    /// Gets the current request count for the client IP address from the current request context.
    /// </summary>
    /// <param name="middleware">The RateLimitingMiddleware instance</param>
    /// <param name="context">The HttpContext containing the client IP</param>
    /// <returns>The current request count, or 0 if not tracked</returns>
    public static int GetCurrentRequestCount(this RateLimitingMiddleware middleware, HttpContext context)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return middleware.GetRequestCount(clientIp);
    }

    /// <summary>
    /// Gets the current request count for a specific IP address.
    /// </summary>
    /// <param name="middleware">The RateLimitingMiddleware instance</param>
    /// <param name="clientIp">The client IP address to check</param>
    /// <returns>The current request count, or 0 if not tracked</returns>
    public static int GetRequestCount(this RateLimitingMiddleware middleware, string clientIp)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        if (string.IsNullOrWhiteSpace(clientIp))
        {
            throw new ArgumentException("Client IP cannot be null or empty", nameof(clientIp));
        }

        // Use reflection to access the private RateLimitBuckets dictionary
        var bucketsField = typeof(RateLimitingMiddleware).GetField("_requestCount",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        if (bucketsField?.GetValue(null) is ConcurrentDictionary<string, int> requestCounts)
        {
            if (requestCounts.TryGetValue(clientIp, out var count))
            {
                return count;
            }
        }

        return 0;
    }

    /// <summary>
    /// Checks if the current request's IP address has exceeded its rate limit.
    /// </summary>
    /// <param name="middleware">The RateLimitingMiddleware instance</param>
    /// <param name="context">The HttpContext containing the client IP</param>
    /// <returns>True if rate limit is exceeded, false otherwise</returns>
    public static bool IsCurrentRateLimitExceeded(this RateLimitingMiddleware middleware, HttpContext context)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return middleware.IsRateLimitExceeded(clientIp);
    }

    /// <summary>
    /// Checks if a specific IP address has exceeded its rate limit.
    /// </summary>
    /// <param name="middleware">The RateLimitingMiddleware instance</param>
    /// <param name="clientIp">The client IP address to check</param>
    /// <returns>True if rate limit is exceeded, false otherwise</returns>
    public static bool IsRateLimitExceeded(this RateLimitingMiddleware middleware, string clientIp)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        // Default max requests per minute from the middleware
        const int maxRequestsPerMinute = 100;
        return middleware.GetRequestCount(clientIp) > maxRequestsPerMinute;
    }

    /// <summary>
    /// Gets the current rate limit window information for the client IP address from the current request context.
    /// </summary>
    /// <param name="middleware">The RateLimitingMiddleware instance</param>
    /// <param name="context">The HttpContext containing the client IP</param>
    /// <returns>A tuple containing WindowStart and RequestCount, or default values if not tracked</returns>
    public static (DateTime WindowStart, int RequestCount) GetCurrentRateLimitWindow(this RateLimitingMiddleware middleware, HttpContext context)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return middleware.GetRateLimitWindow(clientIp);
    }

    /// <summary>
    /// Gets the current rate limit window information for a specific IP address.
    /// </summary>
    /// <param name="middleware">The RateLimitingMiddleware instance</param>
    /// <param name="clientIp">The client IP address to check</param>
    /// <returns>A tuple containing WindowStart and RequestCount, or default values if not tracked</returns>
    public static (DateTime WindowStart, int RequestCount) GetRateLimitWindow(this RateLimitingMiddleware middleware, string clientIp)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        if (string.IsNullOrWhiteSpace(clientIp))
        {
            throw new ArgumentException("Client IP cannot be null or empty", nameof(clientIp));
        }

        // Use reflection to access the private rate limit tracking
        var bucketsField = typeof(RateLimitingMiddleware).GetField("_requestCount",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        if (bucketsField?.GetValue(null) is ConcurrentDictionary<string, int> requestCounts)
        {
            if (requestCounts.TryGetValue(clientIp, out var count))
            {
                // Window start is approximately now minus 1 minute based on the middleware logic
                var windowStart = DateTime.UtcNow.AddMinutes(-1);
                return (windowStart, count);
            }
        }

        return (DateTime.MinValue, 0);
    }

    /// <summary>
    /// Resets the rate limit counter for the current request's IP address.
    /// Useful for testing or administrative purposes.
    /// </summary>
    /// <param name="middleware">The RateLimitingMiddleware instance</param>
    /// <param name="context">The HttpContext containing the client IP</param>
    /// <returns>True if the bucket was found and removed, false otherwise</returns>
    public static bool ResetCurrentRateLimit(this RateLimitingMiddleware middleware, HttpContext context)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

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
    public static bool ResetRateLimit(this RateLimitingMiddleware middleware, string clientIp)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        if (string.IsNullOrWhiteSpace(clientIp))
        {
            throw new ArgumentException("Client IP cannot be null or empty", nameof(clientIp));
        }

        // Use reflection to access and modify the private dictionary
        var bucketsField = typeof(RateLimitingMiddleware).GetField("_requestCount",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        if (bucketsField?.GetValue(null) is ConcurrentDictionary<string, int> requestCounts)
        {
            return requestCounts.TryRemove(clientIp, out _);
        }

        return false;
    }
}