#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Extension methods for configuring health check endpoints in the application.
/// Provides fluent API for mapping health check endpoints with proper documentation and validation.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Maps a simple health check endpoint at GET /api/v1/health
    /// </summary>
    /// <param name="app">The web application to extend.</param>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/></exception>
    public static void MapHealthCheckEndpoint(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet(
                "/api/v1/health",
                () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
            .WithName("Health Check")
            .WithTags("Health")
            .WithSummary("Check if the API is running")
            .WithDescription("Returns a simple health status response with current timestamp.");
    }

    /// <summary>
    /// Maps readiness and liveness health check endpoints at:
    /// - GET /api/v1/health/ready (readiness check with dependency validation)
    /// - GET /api/v1/health/live (liveness check for container orchestration)
    /// </summary>
    /// <param name="app">The web application to extend.</param>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/></exception>
    public static void MapHealthChecks(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet(
                "/api/v1/health/ready",
                (HttpContext context) =>
                {
                    var controller = context.RequestServices.GetRequiredService<Controllers.HealthController>();
                    return controller.ReadinessCheck();
                })
            .WithName("Readiness Check")
            .WithTags("Health")
            .WithSummary("Check if the API is ready to accept traffic")
            .WithDescription("Performs comprehensive readiness check including database, cache, and external service validation.");

        app.MapGet(
                "/api/v1/health/live",
                (HttpContext context) =>
                {
                    var controller = context.RequestServices.GetRequiredService<Controllers.HealthController>();
                    return controller.LivenessCheck();
                })
            .WithName("Liveness Check")
            .WithTags("Health")
            .WithSummary("Check if the API process is running")
            .WithDescription("Simple liveness probe that returns 200 OK if the service is responsive. Does not validate external dependencies.");
    }
}
