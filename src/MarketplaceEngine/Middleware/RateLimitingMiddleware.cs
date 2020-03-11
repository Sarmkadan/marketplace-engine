#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Net;
using System.Threading;

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
        // Get client IP address - prefers the X-Forwarded-For header for proxied requests,
        // falling back to the direct connection address
        var clientIp = GetClientIp(context);

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

    private static string GetClientIp(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            // X-Forwarded-For may contain a comma-separated chain of proxies;
            // the first entry is the original client.
            var candidate = forwardedFor.Split(',')[0].Trim();
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate;
            }
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static bool IsRateLimitAllowed(string clientIp)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddMinutes(-WindowSizeMinutes);

        var bucket = RateLimitBuckets.AddOrUpdate(
            clientIp,
            _ => new RateLimitBucket { WindowStart = now, RequestCount = 1 },
            (_, existing) =>
            {
                // If window has expired, start a fresh bucket
                if (existing.WindowStart < windowStart)
                {
                    return new RateLimitBucket { WindowStart = now, RequestCount = 1 };
                }

                // Atomically increment the shared counter; the update factory
                // may be invoked multiple times under contention, so mutating
                // the existing instance without an atomic operation would lose counts.
                existing.Increment();
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
        private int _requestCount;

        public DateTime WindowStart { get; set; }

        public int RequestCount
        {
            get => _requestCount;
            set => _requestCount = value;
        }

        public void Increment() => Interlocked.Increment(ref _requestCount);
    }
}
