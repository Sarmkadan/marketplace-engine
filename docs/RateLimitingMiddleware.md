# RateLimitingMiddleware
The `RateLimitingMiddleware` class is designed to handle rate limiting in the marketplace-engine project. It provides a mechanism to limit the number of requests within a specified time window, helping to prevent abuse and ensure fair usage of the system.

## API
### `public RateLimitingMiddleware`
The constructor initializes a new instance of the `RateLimitingMiddleware` class.

### `public async Task InvokeAsync`
This method invokes the middleware asynchronously, allowing it to process incoming requests and enforce rate limiting rules. It does not take any parameters and returns a `Task` object, indicating the completion of the asynchronous operation. If the rate limit is exceeded, it may throw an exception or return an error response.

### `public DateTime WindowStart`
This property represents the start of the time window during which the rate limiting is enforced. It returns a `DateTime` object indicating the beginning of the window.

### `public int RequestCount`
This property returns the number of requests made within the current time window. It provides a way to track the request count and enforce rate limiting rules.

## Usage
The following examples demonstrate how to use the `RateLimitingMiddleware` class in a C# application:
```csharp
// Example 1: Creating a new instance of RateLimitingMiddleware
var middleware = new RateLimitingMiddleware();
await middleware.InvokeAsync();

// Example 2: Using RateLimitingMiddleware in a request pipeline
var pipeline = new RequestPipeline();
pipeline.UseMiddleware<RateLimitingMiddleware>();
await pipeline.InvokeAsync(context);
```

## Notes
When using the `RateLimitingMiddleware` class, consider the following edge cases and thread-safety remarks:
- The `WindowStart` property may not be thread-safe, as multiple threads may access and modify it concurrently. Synchronization mechanisms, such as locks or atomic operations, may be necessary to ensure thread safety.
- If the rate limit is exceeded, the `InvokeAsync` method may throw an exception or return an error response. It is essential to handle these scenarios properly in the application code.
- The `RequestCount` property may not be reset automatically when the time window expires. The application may need to manually reset the request count or implement a mechanism to handle window expiration.
