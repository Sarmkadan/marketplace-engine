#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Infrastructure.Configuration;

/// <summary>
/// Centralized configuration for marketplace infrastructure settings.
/// Includes caching policies, rate limits, timeouts, and security settings.
/// </summary>
public class MarketplaceConfiguration
{
    /// <summary>
    /// Caching configuration.
    /// </summary>
    public class CachingConfig
    {
        public bool Enabled { get; set; } = true;
        public int DefaultTtlMinutes { get; set; } = 5;
        public int MaxCacheSizeMb { get; set; } = 100;
        public int ListingCacheTtlMinutes { get; set; } = 2;
        public int UserCacheTtlMinutes { get; set; } = 15;
        public int CategoryCacheTtlMinutes { get; set; } = 30;
        public int SearchResultCacheTtlMinutes { get; set; } = 10;
    }

    /// <summary>
    /// Rate limiting configuration.
    /// </summary>
    public class RateLimitConfig
    {
        public bool Enabled { get; set; } = true;
        public int MaxRequestsPerMinute { get; set; } = 100;
        public int MaxRequestsPerHour { get; set; } = 5000;
        public string[] ExemptPaths { get; set; } = new[] { "/api/v1/health" };
    }

    /// <summary>
    /// Background job queue configuration.
    /// </summary>
    public class BackgroundJobConfig
    {
        public bool Enabled { get; set; } = true;
        public int PollingIntervalMs { get; set; } = 100;
        public int MaxConcurrentJobs { get; set; } = 5;
        public int JobTimeoutSeconds { get; set; } = 300;
    }

    /// <summary>
    /// External API integration configuration.
    /// </summary>
    public class IntegrationConfig
    {
        public string DropshipApiBaseUrl { get; set; } = "https://api.dropship-provider.com";
        public string DropshipApiKey { get; set; } = "";
        public int ApiTimeoutSeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelayMs { get; set; } = 1000;
    }

    /// <summary>
    /// Security configuration.
    /// </summary>
    public class SecurityConfig
    {
        public string TokenSecret { get; set; } = "";
        public string WebhookSecret { get; set; } = "";
        public int TokenExpirationDays { get; set; } = 30;
        public bool RequireHttpsRedirect { get; set; } = true;
        public bool EnableCors { get; set; } = true;
        public string[] AllowedOrigins { get; set; } = new[] { "*" };
    }

    /// <summary>
    /// Logging configuration.
    /// </summary>
    public class LoggingConfig
    {
        public bool LogRequests { get; set; } = true;
        public bool LogResponses { get; set; } = false;
        public int SlowRequestThresholdMs { get; set; } = 1000;
        public bool LogSlowRequests { get; set; } = true;
    }

    public CachingConfig Caching { get; set; } = new();
    public RateLimitConfig RateLimit { get; set; } = new();
    public BackgroundJobConfig BackgroundJobs { get; set; } = new();
    public IntegrationConfig Integration { get; set; } = new();
    public SecurityConfig Security { get; set; } = new();
    public LoggingConfig Logging { get; set; } = new();

    /// <summary>
    /// Creates default configuration.
    /// </summary>
    public static MarketplaceConfiguration CreateDefault()
    {
        return new MarketplaceConfiguration();
    }

    /// <summary>
    /// Creates configuration from IConfiguration (appsettings.json).
    /// </summary>
    public static MarketplaceConfiguration CreateFromConfiguration(IConfiguration config)
    {
        var marketplaceConfig = new MarketplaceConfiguration();

        // Bind sections from configuration
        var cachingSection = config.GetSection("Marketplace:Caching");
        if (cachingSection.Exists())
        {
            cachingSection.Bind(marketplaceConfig.Caching);
        }

        var rateLimitSection = config.GetSection("Marketplace:RateLimit");
        if (rateLimitSection.Exists())
        {
            rateLimitSection.Bind(marketplaceConfig.RateLimit);
        }

        var integrationSection = config.GetSection("Marketplace:Integration");
        if (integrationSection.Exists())
        {
            integrationSection.Bind(marketplaceConfig.Integration);
        }

        var securitySection = config.GetSection("Marketplace:Security");
        if (securitySection.Exists())
        {
            securitySection.Bind(marketplaceConfig.Security);
        }

        return marketplaceConfig;
    }

    /// <summary>
    /// Validates configuration and logs warnings for suspicious values.
    /// </summary>
    public void Validate(ILogger logger)
    {
        if (!Caching.Enabled)
            logger.LogWarning("Caching is disabled - performance may be impacted");

        if (!RateLimit.Enabled)
            logger.LogWarning("Rate limiting is disabled - API is vulnerable to abuse");

        if (RateLimit.MaxRequestsPerMinute < 10)
            logger.LogWarning("Rate limit is very strict - legitimate traffic may be blocked");

        if (string.IsNullOrEmpty(Security.TokenSecret))
            logger.LogError("Security token secret is not configured");

        if (string.IsNullOrEmpty(Security.WebhookSecret))
            logger.LogError("Webhook secret is not configured");

        logger.LogInformation("Marketplace configuration validated successfully");
    }
}

/// <summary>
/// Extension methods for adding configuration to DI container.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Registers marketplace configuration in DI container.
    /// </summary>
    public static IServiceCollection AddMarketplaceConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var config = MarketplaceConfiguration.CreateFromConfiguration(configuration);
        services.AddSingleton(config);

        return services;
    }
}
