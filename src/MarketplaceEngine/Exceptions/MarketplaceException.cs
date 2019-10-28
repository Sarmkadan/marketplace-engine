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
    public string? ErrorCode { get; }
    public Dictionary<string, string[]>? ValidationErrors { get; }

    public MarketplaceException(string message, string? errorCode = null)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public MarketplaceException(string message, Exception? innerException, string? errorCode = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public MarketplaceException(string message, Dictionary<string, string[]> validationErrors, string? errorCode = null)
        : base(message)
    {
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
    }

    // Creates exception with formatted message
    public static MarketplaceException CreateWithContext(string message, string context)
        => new($"{message} [{context}]");
}
