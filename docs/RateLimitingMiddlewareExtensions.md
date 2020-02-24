# RateLimitingMiddlewareExtensions

Extension methods for configuring and managing rate limiting in ASP.NET Core applications. These methods provide middleware integration and runtime inspection of request counts within sliding windows to enforce or monitor rate limits.

## API

### `UseRateLimiting`

Applies the rate limiting middleware to the HTTP request pipeline. The middleware tracks requests per client and enforces rate limits based on a sliding window algorithm.

No parameters. Returns the `IApplicationBuilder` for method chaining.

### `GetCurrentRequestCount`

Gets the number of requests received in the current sliding window for the current client.

Returns the current request count as an integer.

### `GetRequestCount`

Gets the number of requests received in the sliding window for the specified client.

Parameter:
- `clientId`: A string identifying the client.

Returns the request count as an integer.

### `IsCurrentRateLimitExceeded`

Determines whether the current request count for the current client exceeds the configured rate limit threshold.

Returns `true` if the current request count exceeds the limit; otherwise, `false`.

### `IsRateLimitExceeded`

Determines whether the request count for the specified client exceeds the configured rate limit threshold.

Parameter:
- `clientId`: A string identifying the client.

Returns `true` if the request count exceeds the limit; otherwise, `false`.

### `GetCurrentRateLimitWindow`

Retrieves the start timestamp and request count of the current sliding window for the current client.

Returns a tuple `(DateTime WindowStart, int RequestCount)` representing the window start time and the number of requests within that window.

### `GetRateLimitWindow`

Retrieves the start timestamp and request count of the sliding window for the specified client.

Parameter:
- `clientId`: A string identifying the client.

Returns a tuple `(DateTime WindowStart, int RequestCount)` representing the window start time and the number of requests within that window.

### `ResetCurrentRateLimit`

Resets the rate limiting state for the current client, clearing the request count and window.

Returns `true` if the reset was successful; otherwise, `false`.

### `ResetRateLimit`

Resets the rate limiting state for the specified client, clearing the request count and window.

Parameter:
- `clientId`: A string identifying the client.

Returns `true` if the reset was successful; otherwise, `false`.

## Usage

### Example 1: Basic Rate Limiting Middleware Integration
