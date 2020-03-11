#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection;
using MarketplaceEngine.Data;
using MarketplaceEngine.Infrastructure.Caching;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Health check endpoints for monitoring application status and dependencies.
/// Provides both basic liveness and detailed readiness checks.
/// </summary>
[ApiController]
[Route("api/v1")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly CacheService _cacheService;

    public HealthController(ILogger<HealthController> logger, CacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Basic liveness check to determine if the service is running.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Health()
    {
        var response = new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = GetAssemblyVersion(),
            Uptime = GetUptime()
        };

        return Ok(response);
    }

    /// <summary>
    /// Detailed readiness check that validates critical dependencies.
    /// </summary>
    [HttpGet("health/ready")]
    [ProducesResponseType(typeof(DetailedHealthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReadinessCheck()
    {
        var response = new DetailedHealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = GetAssemblyVersion(),
            Uptime = GetUptime(),
            Dependencies = new Dictionary<string, string>
            {
                { "Database", CheckDatabase() },
                { "Cache", await CheckCacheAsync() },
                { "ExternalServices", CheckExternalServices() }
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Simple liveness probe for container orchestration systems.
    /// </summary>
    [HttpGet("health/live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult LivenessCheck()
    {
        // Simple check that returns 200 OK if the service is running
        return Ok();
    }

    private string GetAssemblyVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    }

    private string GetUptime()
    {
        var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime;
        return $"{uptime.TotalSeconds}s";
    }

    private string CheckDatabase()
    {
        // The application uses an in-memory data store instead of an external database.
        // Verify the store is reachable and initialized.
        try
        {
            var context = MarketplaceDbContext.GetInstance();
            return context.Users is not null ? "Connected" : "Disconnected";
        }
        catch
        {
            return "Disconnected";
        }
    }

    private async Task<string> CheckCacheAsync()
    {
        // Round-trips a probe value through the cache service to confirm it is functioning.
        try
        {
            var probeKey = $"health:probe:{Guid.NewGuid()}";
            await _cacheService.SetAsync(probeKey, true, TimeSpan.FromSeconds(5));
            var result = await _cacheService.GetAsync<bool>(probeKey);
            await _cacheService.RemoveAsync(probeKey);
            return result ? "Available" : "Unavailable";
        }
        catch
        {
            return "Unavailable";
        }
    }

    private string CheckExternalServices()
    {
        // No external service dependencies are configured for this deployment.
        return "No external dependencies configured";
    }
}

public class HealthResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Uptime { get; set; } = string.Empty;
}

public class DetailedHealthResponse : HealthResponse
{
    public Dictionary<string, string> Dependencies { get; set; } = new();
}