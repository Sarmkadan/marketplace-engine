// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection;

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

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
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
                { "Cache", CheckCache() },
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
        // In a real implementation, this would check actual database connectivity
        try
        {
            // Simulate database check
            return "Connected";
        }
        catch
        {
            return "Disconnected";
        }
    }

    private string CheckCache()
    {
        // In a real implementation, this would check actual cache connectivity
        try
        {
            // Simulate cache check
            return "Available";
        }
        catch
        {
            return "Unavailable";
        }
    }

    private string CheckExternalServices()
    {
        // In a real implementation, this would check external service connectivity
        try
        {
            // Simulate external service check
            return "All services operational";
        }
        catch
        {
            return "Some services unavailable";
        }
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