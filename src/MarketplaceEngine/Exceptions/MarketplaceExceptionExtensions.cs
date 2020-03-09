using System;
using System.Collections.Generic;

namespace MarketplaceEngine.Exceptions;

/// <summary>
/// Provides a set of extension methods for <see cref="MarketplaceException"/> to simplify
/// error handling, validation error inspection, and formatted error string creation.
/// </summary>
public static class MarketplaceExceptionExtensions
{
    /// <summary>
    /// Returns <c>true</c> if the provided <see cref="MarketplaceException"/> contains any validation errors; otherwise <c>false</c>.
    /// </summary>
    /// <param name="exception">The <see cref="MarketplaceException"/> instance to inspect. Must not be <c>null</c>.</param>
    /// <returns>
    /// <c>true</c> when <c>exception.ValidationErrors</c> is non‑null and contains at least one entry; otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
    public static bool HasValidationErrors(this MarketplaceException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.ValidationErrors is { Count: > 0 };
    }

    /// <summary>
    /// Creates a single‑line error message that combines the exception's <c>Message</c> with its <c>ErrorCode</c> when an error code is present.
    /// </summary>
    /// <param name="exception">The <see cref="MarketplaceException"/> containing the error information. Must not be <c>null</c>.</param>
    /// <returns>
    /// A formatted string containing the original <c>Message</c> and, if <c>ErrorCode</c> is not <c>null</c>,
    /// the text <c>"Error Code: {ErrorCode}"</c> appended to it.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
    public static string GetErrorMessage(this MarketplaceException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return $"{exception.Message} {(exception.ErrorCode is not null ? $"Error Code: {exception.ErrorCode}" : string.Empty)}";
    }

    /// <summary>
    /// Retrieves the <c>ValidationErrors</c> dictionary from the exception, guaranteeing a non‑null result.
    /// </summary>
    /// <param name="exception">The <see cref="MarketplaceException"/> containing validation errors. Must not be <c>null</c>.</param>
    /// <returns>
    /// The <c>ValidationErrors</c> dictionary if it is set; otherwise a new empty <see cref="Dictionary{TKey,TValue}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
    public static Dictionary<string, string[]> GetValidationErrors(this MarketplaceException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.ValidationErrors ?? new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Generates a multi‑line error description for the exception, including the base error message
    /// and any validation errors formatted for readability.
    /// </summary>
    /// <param name="exception">The <see cref="MarketplaceException"/> to convert. Must not be <c>null</c>.</param>
    /// <returns>
    /// A string suitable for logging or display that contains the formatted error message and,
    /// when validation errors exist, a list of those errors each on its own line.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
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
