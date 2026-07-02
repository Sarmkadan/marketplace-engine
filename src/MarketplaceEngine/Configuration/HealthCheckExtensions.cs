#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Extension methods for adding health check endpoints to the application.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Maps a simple health check endpoint at GET /api/v1/health
    /// </summary>
    /// <param name="app">The web application to extend.</param>
    public static void MapHealthCheckEndpoint(this WebApplication app)
    {
        app.MapGet("/api/v1/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
            .WithName("Health Check")
            .WithTags("Health")
            .WithSummary("Check if the API is running")
            .WithDescription("Returns a simple health status response with current timestamp.");
    }
}
