#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Provides extension methods for <see cref="ApiResponse{T}"/> and <see cref="ApiResponse"/>
/// to enhance functionality with common operations like mapping, validation, and conversion.
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Maps the data payload of a successful <see cref="ApiResponse{T}"/> to a new type.
    /// Returns a new <see cref="ApiResponse"/> with the mapped data if successful, or propagates the error response.
    /// </summary>
    /// <typeparam name="TSource">The source data type.</typeparam>
    /// <typeparam name="TResult">The result data type.</typeparam>
    /// <param name="response">The API response to map.</param>
    /// <param name="mapper">The mapping function to apply to the data payload.</param>
    /// <returns>
    /// A new <see cref="ApiResponse{T}"/> with mapped data if the source response is successful,
    /// or the original error response if the source response failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> or <paramref name="mapper"/> is null.</exception>
    public static ApiResponse<TResult> Map<TSource, TResult>(
        this ApiResponse<TSource> response,
        Func<TSource, TResult> mapper)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(mapper);

        if (!response.Success)
        {
            return ApiResponse<TResult>.ErrorResponse(
                response.ErrorCode ?? "UNKNOWN_ERROR",
                response.Message ?? "An error occurred",
                response.RequestId,
                response.Errors);
        }

        return response.Data is null
            ? ApiResponse<TResult>.SuccessResponse(default, response.Message, response.RequestId)
            : ApiResponse<TResult>.SuccessResponse(mapper(response.Data), response.Message, response.RequestId);
    }

    /// <summary>
    /// Maps the data payload of a successful <see cref="ApiResponse"/> to a new <see cref="ApiResponse{T}"/>.
    /// Useful for chaining operations that require typed responses.
    /// </summary>
    /// <typeparam name="TResult">The result data type.</typeparam>
    /// <param name="response">The API response to map.</param>
    /// <param name="data">The data to include in the new response.</param>
    /// <returns>
    /// A new <see cref="ApiResponse{T}"/> with the provided data if the source response is successful,
    /// or an error response if the source response failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> is null.</exception>
    public static ApiResponse<TResult> Map<TResult>(
        this ApiResponse response,
        TResult data)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (!response.Success)
        {
            return ApiResponse<TResult>.ErrorResponse(
                response.ErrorCode ?? "UNKNOWN_ERROR",
                response.Message ?? "An error occurred",
                response.RequestId);
        }

        return ApiResponse<TResult>.SuccessResponse(data, response.Message, response.RequestId);
    }

    /// <summary>
    /// Converts a successful <see cref="ApiResponse{T}"/> to a <see cref="PagedResponse{T}"/> with a single item.
    /// Useful for unifying paginated and non-paginated responses in client code.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    /// <param name="response">The API response to convert.</param>
    /// <returns>
    /// A <see cref="PagedResponse{T}"/> containing the response data as a single item,
    /// or an empty paged response if the source response failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> is null.</exception>
    public static PagedResponse<T> ToPagedResponse<T>(this ApiResponse<T> response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (!response.Success)
        {
            var result = new PagedResponse<T>();
            result.Items = []; // Ensure empty list instead of null
            return result;
        }

        return new PagedResponse<T>
        {
            Items = [response.Data],
            Page = 1,
            PageSize = 1,
            Total = 1
        };
    }

    /// <summary>
    /// Adds or updates the RequestId in an <see cref="ApiResponse"/> or <see cref="ApiResponse{T}"/>.
    /// Useful for propagating correlation IDs through service boundaries.
    /// </summary>
    /// <typeparam name="T">The data type for generic responses.</typeparam>
    /// <param name="response">The API response to update.</param>
    /// <param name="requestId">The request ID to set.</param>
    /// <returns>A new response with the updated RequestId.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="requestId"/> is null or empty.</exception>
    public static ApiResponse<T> WithRequestId<T>(
        this ApiResponse<T> response,
        string requestId)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrEmpty(requestId);

        var result = ApiResponse<T>.SuccessResponse(
            response.Data,
            response.Message,
            requestId);

        result.Success = response.Success;
        result.ErrorCode = response.ErrorCode;
        result.Errors = response.Errors;
        result.Timestamp = response.Timestamp;

        return result;
    }

    /// <summary>
    /// Adds or updates the RequestId in a non-generic <see cref="ApiResponse"/>.
    /// </summary>
    /// <param name="response">The API response to update.</param>
    /// <param name="requestId">The request ID to set.</param>
    /// <returns>A new response with the updated RequestId.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="requestId"/> is null or empty.</exception>
    public static ApiResponse WithRequestId(
        this ApiResponse response,
        string requestId)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrEmpty(requestId);

        var result = ApiResponse.SuccessResponse(
            response.Message,
            requestId);

        result.Success = response.Success;
        result.ErrorCode = response.ErrorCode;
        result.Timestamp = response.Timestamp;

        return result;
    }

    /// <summary>
    /// Creates a validation error response with a single field error.
    /// Convenience method for common validation scenarios.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    /// <param name="fieldName">The name of the field with validation error.</param>
    /// <param name="errorMessage">The validation error message.</param>
    /// <param name="requestId">Optional request ID for correlation.</param>
    /// <returns>A new validation error response.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName"/> is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errorMessage"/> is null or empty.</exception>
    public static ApiResponse<T> FieldValidationError<T>(
        string fieldName,
        string errorMessage,
        string? requestId = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);

        var errors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { fieldName, errorMessage }
        };

        return ApiResponse<T>.ValidationErrorResponse(errors, requestId);
    }

    /// <summary>
    /// Creates a validation error response with multiple field errors.
    /// Convenience method for batch validation scenarios.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    /// <param name="fieldErrors">Dictionary of field names to error messages.</param>
    /// <param name="requestId">Optional request ID for correlation.</param>
    /// <returns>A new validation error response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldErrors"/> is null.</exception>
    public static ApiResponse<T> FieldValidationError<T>(
        Dictionary<string, string> fieldErrors,
        string? requestId = null)
    {
        ArgumentNullException.ThrowIfNull(fieldErrors);

        return ApiResponse<T>.ValidationErrorResponse(fieldErrors, requestId);
    }
}