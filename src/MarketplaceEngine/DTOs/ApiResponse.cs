// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Standard API response wrapper for all endpoints.
/// Provides consistent structure for success and failure responses.
/// Includes metadata like timestamp and request ID for debugging.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public Dictionary<string, string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? RequestId { get; set; }

    private ApiResponse() { }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string? message = null, string? requestId = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            Timestamp = DateTime.UtcNow,
            RequestId = requestId
        };
    }

    /// <summary>
    /// Creates a failure response.
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string errorCode, string message, string? requestId = null, Dictionary<string, string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message,
            Errors = errors,
            Timestamp = DateTime.UtcNow,
            RequestId = requestId
        };
    }

    /// <summary>
    /// Creates a validation error response with field-level errors.
    /// </summary>
    public static ApiResponse<T> ValidationErrorResponse(Dictionary<string, string> fieldErrors, string? requestId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            ErrorCode = "VALIDATION_ERROR",
            Message = "One or more validation errors occurred",
            Errors = fieldErrors,
            Timestamp = DateTime.UtcNow,
            RequestId = requestId
        };
    }
}

/// <summary>
/// Non-generic API response for operations without return data.
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? RequestId { get; set; }

    private ApiResponse() { }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    public static ApiResponse SuccessResponse(string? message = null, string? requestId = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            Timestamp = DateTime.UtcNow,
            RequestId = requestId
        };
    }

    /// <summary>
    /// Creates a failure response.
    /// </summary>
    public static ApiResponse ErrorResponse(string errorCode, string message, string? requestId = null)
    {
        return new ApiResponse
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message,
            Timestamp = DateTime.UtcNow,
            RequestId = requestId
        };
    }
}

/// <summary>
/// Paged list response wrapper.
/// </summary>
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPages => (Total + PageSize - 1) / PageSize;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
