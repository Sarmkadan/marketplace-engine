# ErrorHandlingMiddleware

A middleware component for ASP.NET Core that standardizes error handling across the marketplace-engine API. It intercepts exceptions, converts them into a structured error response, and ensures consistent error formatting before sending the response to the client.

## API

### `public ErrorHandlingMiddleware`

The constructor for the middleware. Initializes a new instance of the `ErrorHandlingMiddleware` class.

### `public async Task InvokeAsync(HttpContext context, RequestDelegate next)`

Invokes the middleware to process an HTTP request.

- **Parameters**
  - `context`: The `HttpContext` for the current HTTP request.
  - `next`: The delegate representing the next middleware in the pipeline.
- **Return Value**
  - A `Task` representing the asynchronous operation.
- **Exceptions**
  - Throws `ArgumentNullException` if `context` is `null`.
  - Throws `ArgumentNullException` if `next` is `null`.

### `public string Code`

Gets the error code associated with the error. This code is used to uniquely identify the type of error that occurred.

- **Return Value**
  - A string representing the error code.

### `public string Message`

Gets the error message describing what went wrong.

- **Return Value**
  - A string representing the error message.

### `public Dictionary<string, object>? Details`

Gets additional details about the error, if available. This dictionary may contain extra context such as validation failures or internal state.

- **Return Value**
  - A `Dictionary<string, object>` containing additional error details, or `null` if no details are available.

### `public DateTime Timestamp`

Gets the timestamp when the error occurred.

- **Return Value**
  - A `DateTime` representing the time the error was captured.

## Usage

```csharp
// Example 1: Basic usage in a minimal API
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapGet("/error", () => throw new InvalidOperationException("Something went wrong"));

app.Run();

// Example 2: Using with exception filters
public class CustomException : Exception
{
    public string ErrorCode { get; } = "CUSTOM_ERROR";
}

var app = WebApplication.CreateBuilder(args).Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapGet("/custom-error", () => throw new CustomException());

app.Run();
```

## Notes

- The middleware is designed to be thread-safe as it does not maintain any mutable state between requests. Each invocation operates on the provided `HttpContext` and does not share data across threads.
- If the `Details` dictionary is populated, ensure that all values are serializable to JSON to avoid serialization errors when returning the error response.
- The `Timestamp` is captured at the moment the error is processed, not when the exception was originally thrown, which may introduce slight timing discrepancies in logs.
- The middleware assumes that the `HttpContext.Response` has not been started before invocation. If the response has already been written to, the middleware may fail to send the error response correctly.
