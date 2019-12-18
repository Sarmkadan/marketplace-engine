# MarketplaceConfiguration
The `MarketplaceConfiguration` type is a central configuration class in the `marketplace-engine` project, providing settings that control various aspects of the marketplace's behavior, including caching, rate limiting, and API interactions. It serves as a single point of configuration for the engine, allowing for easy management and modification of these settings.

## API
The `MarketplaceConfiguration` type exposes several public members that can be used to configure the marketplace engine:
* `Enabled`: A boolean indicating whether the marketplace is enabled.
* `DefaultTtlMinutes`: An integer specifying the default time-to-live (TTL) in minutes for cached data.
* `MaxCacheSizeMb`: An integer specifying the maximum size of the cache in megabytes.
* `ListingCacheTtlMinutes`, `UserCacheTtlMinutes`, `CategoryCacheTtlMinutes`, `SearchResultCacheTtlMinutes`: Integers specifying the TTL in minutes for cached listings, users, categories, and search results, respectively.
* `MaxRequestsPerMinute` and `MaxRequestsPerHour`: Integers specifying the maximum number of requests allowed per minute and per hour, respectively.
* `ExemptPaths`: An array of strings specifying paths that are exempt from rate limiting.
* `PollingIntervalMs`: An integer specifying the polling interval in milliseconds.
* `MaxConcurrentJobs`: An integer specifying the maximum number of concurrent jobs.
* `JobTimeoutSeconds`: An integer specifying the job timeout in seconds.
* `DropshipApiBaseUrl` and `DropshipApiKey`: Strings specifying the base URL and API key for the dropship API, respectively.
* `ApiTimeoutSeconds`: An integer specifying the API timeout in seconds.
* `MaxRetries`: An integer specifying the maximum number of retries.
* `RetryDelayMs`: An integer specifying the retry delay in milliseconds.

## Usage
Here are two examples of using the `MarketplaceConfiguration` type in C#:
```csharp
// Example 1: Configuring caching settings
var config = new MarketplaceConfiguration
{
    Enabled = true,
    DefaultTtlMinutes = 30,
    MaxCacheSizeMb = 1024,
    ListingCacheTtlMinutes = 60,
    UserCacheTtlMinutes = 30,
    CategoryCacheTtlMinutes = 60,
    SearchResultCacheTtlMinutes = 30
};

// Example 2: Configuring API settings
var config = new MarketplaceConfiguration
{
    DropshipApiBaseUrl = "https://api.dropship.com",
    DropshipApiKey = "YOUR_API_KEY",
    ApiTimeoutSeconds = 30,
    MaxRetries = 3,
    RetryDelayMs = 500
};
```

## Notes
When using the `MarketplaceConfiguration` type, note the following:
* The `Enabled` property controls whether the marketplace is enabled, and should be set to `true` to enable the engine.
* The caching settings (`DefaultTtlMinutes`, `MaxCacheSizeMb`, etc.) should be carefully configured to balance performance and data freshness.
* The rate limiting settings (`MaxRequestsPerMinute`, `MaxRequestsPerHour`, etc.) should be configured to prevent abuse and ensure fair usage.
* The API settings (`DropshipApiBaseUrl`, `DropshipApiKey`, etc.) should be configured to match the dropship API's requirements.
* The `MarketplaceConfiguration` type is not thread-safe, and should be accessed and modified in a thread-safe manner to prevent data corruption or other concurrency issues.
* The `ExemptPaths` array should be carefully configured to ensure that only intended paths are exempt from rate limiting.
* The `PollingIntervalMs`, `MaxConcurrentJobs`, and `JobTimeoutSeconds` settings should be configured to balance performance and resource usage.
