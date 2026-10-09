#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Exceptions;

/// <summary>
/// Base exception for all marketplace-related errors.
/// </summary>
public class MarketplaceException : Exception
{
    /// <summary>
    /// Machine-readable error code that identifies the failure category, or <c>null</c> if none was supplied.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Field-level validation errors keyed by field name, or <c>null</c> if the exception is not a validation failure.
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance with a message and an optional error code.
    /// </summary>
    /// <param name="message">Human-readable description of the error.</param>
    /// <param name="errorCode">Optional machine-readable error code.</param>
    public MarketplaceException(string message, string? errorCode = null)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance with a message, an inner exception, and an optional error code.
    /// </summary>
    /// <param name="message">Human-readable description of the error.</param>
    /// <param name="innerException">The exception that caused this error, or <c>null</c>.</param>
    /// <param name="errorCode">Optional machine-readable error code.</param>
    public MarketplaceException(string message, Exception? innerException, string? errorCode = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance with a message, field-level validation errors, and an optional error code.
    /// </summary>
    /// <param name="message">Human-readable description of the error.</param>
    /// <param name="validationErrors">Validation messages keyed by field name.</param>
    /// <param name="errorCode">Optional machine-readable error code.</param>
    public MarketplaceException(string message, Dictionary<string, string[]> validationErrors, string? errorCode = null)
        : base(message)
    {
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Creates a new exception whose message is <paramref name="message"/> followed by the context in square brackets.
    /// </summary>
    /// <param name="message">Human-readable description of the error.</param>
    /// <param name="context">Additional context appended to the message in square brackets.</param>
    /// <returns>A new <see cref="MarketplaceException"/> with the formatted message.</returns>
    public static MarketplaceException CreateWithContext(string message, string context)
        => new($"{message} [{context}]");
}
