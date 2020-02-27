using System;
using System.Collections.Generic;

namespace MarketplaceEngine.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="MarketplaceException"/> to enhance error handling and formatting.
/// </summary>
public static class MarketplaceExceptionExtensions
{
    /// <summary>
    /// Determines whether the exception contains validation errors.
    /// </summary>
    /// <param name="exception">The exception to check. Cannot be null.</param>
    /// <returns>True if validation errors exist; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool HasValidationErrors(this MarketplaceException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.ValidationErrors is { Count: > 0 };
    }

    /// <summary>
    /// Formats the error message including the error code if present.
    /// </summary>
    /// <param name="exception">The exception containing the error message. Cannot be null.</param>
    /// <returns>A formatted error message string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string GetErrorMessage(this MarketplaceException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return $"{exception.Message} {(exception.ErrorCode is not null ? $"Error Code: {exception.ErrorCode}" : string.Empty)}";
    }

    /// <summary>
    /// Gets the validation errors dictionary, returning an empty dictionary if null.
    /// </summary>
    /// <param name="exception">The exception containing validation errors. Cannot be null.</param>
    /// <returns>A dictionary of validation errors, or empty dictionary if none exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static Dictionary<string, string[]> GetValidationErrors(this MarketplaceException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.ValidationErrors ?? new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Converts the exception to a formatted error string including validation errors if present.
    /// </summary>
    /// <param name="exception">The exception to convert. Cannot be null.</param>
    /// <returns>A formatted error string suitable for logging or display.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string ToErrorString(this MarketplaceException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var errorString = exception.GetErrorMessage();
        if (exception.HasValidationErrors())
        {
            errorString += Environment.NewLine + "Validation Errors:";
            foreach (var error in exception.GetValidationErrors())
            {
                errorString += Environment.NewLine + $" {error.Key}: {string.Join(", ", error.Value)}";
            }
        }
        return errorString;
    }
}