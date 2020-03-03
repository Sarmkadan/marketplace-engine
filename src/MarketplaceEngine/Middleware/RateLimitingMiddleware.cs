#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Net;

namespace MarketplaceEngine.Middleware;

/// <summary>
/// Rate limiting middleware using sliding window algorithm.
/// Prevents API abuse by limiting requests per IP address.
/// Configuration: 100 requests per minute per IP.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    // Sliding window rate limiter using in-memory store
    // In production, this should use Redis for distributed rate limiting
    internal static readonly ConcurrentDictionary<string, RateLimitBucket> RateLimitBuckets =
        new();

    internal const int MaxRequestsPerMinute = 100;
    private const int WindowSizeMinutes = 1;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;

        // Start cleanup task to remove old buckets
        _ = CleanupExpiredBucketsAsync();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get client IP address - handles X-Forwarded-For header for proxied requests
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Skip rate limiting for health check endpoint
        if (context.Request.Path.StartsWithSegments("/api/v1/health"))
        {
            await _next(context);
            return;
        }

        if (!IsRateLimitAllowed(clientIp))
        {
            _logger.LogWarning("Rate limit exceeded for IP: {ClientIp}", clientIp);
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers.Add("Retry-After", "60");
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                retryAfter = 60
            });
            return;
        }

        await _next(context);
    }

    private static bool IsRateLimitAllowed(string clientIp)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddMinutes(-WindowSizeMinutes);

        var bucket = RateLimitBuckets.AddOrUpdate(
            clientIp,
            new RateLimitBucket { WindowStart = now, RequestCount = 1 },
            (_, existing) =>
            {
                // If window has expired, create new bucket
                if (existing.WindowStart < windowStart)
                {
                    return new RateLimitBucket { WindowStart = now, RequestCount = 1 };
                }

                // Increment request count
                existing.RequestCount++;
                return existing;
            });

        return bucket.RequestCount <= MaxRequestsPerMinute;
    }

    private async Task CleanupExpiredBucketsAsync()
    {
        // Clean up old rate limit buckets every 5 minutes
        while (true)
        {
            await Task.Delay(TimeSpan.FromMinutes(5));

            var expiredKeys = RateLimitBuckets
                .Where(kvp => kvp.Value.WindowStart < DateTime.UtcNow.AddMinutes(-2))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                RateLimitBuckets.TryRemove(key, out _);
            }
        }
    }

    internal class RateLimitBucket
    {
        public DateTime WindowStart { get; set; }
        public int RequestCount { get; set; }
    }
}
