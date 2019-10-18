// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using System.Text.Json;
using MarketplaceEngine.Exceptions;

namespace MarketplaceEngine.Middleware;

/// <summary>
/// Centralized error handling middleware that catches all exceptions
/// and returns appropriate HTTP responses with consistent error format.
/// Prevents sensitive error details from leaking in production.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        // Map domain exceptions to appropriate HTTP status codes
        // This centralization prevents exception handling scattered across controllers
        switch (exception)
        {
            case ResourceNotFoundException ex:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Code = "RESOURCE_NOT_FOUND";
                response.Message = ex.Message;
                break;

            case UnauthorizedException ex:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Code = "UNAUTHORIZED";
                response.Message = ex.Message;
                break;

            case ValidationException ex:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = "VALIDATION_ERROR";
                response.Message = ex.Message;
                if (ex.ValidationErrors != null)
                {
                    response.Details = ex.ValidationErrors.ToDictionary(
                        kvp => kvp.Key,
                        kvp => (object)string.Join(", ", kvp.Value)
                    );
                }
                break;

            case DuplicateResourceException ex:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Code = "DUPLICATE_RESOURCE";
                response.Message = ex.Message;
                break;

            case MarketplaceException ex:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = "MARKETPLACE_ERROR";
                response.Message = ex.Message;
                break;

            default:
                // Generic error handling for unexpected exceptions
                // In production, we don't expose the actual exception message
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Code = "INTERNAL_SERVER_ERROR";
                response.Message = "An unexpected error occurred";
                break;
        }

        response.Timestamp = DateTime.UtcNow;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsJsonAsync(response, options);
    }
}

/// <summary>
/// Standard error response format used across the API.
/// Provides consistent structure for client error handling.
/// </summary>
public class ErrorResponse
{
    public string Code { get; set; } = "ERROR";
    public string Message { get; set; } = "An error occurred";
    public Dictionary<string, object>? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
