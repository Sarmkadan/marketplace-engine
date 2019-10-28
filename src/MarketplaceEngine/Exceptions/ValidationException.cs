#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Exceptions;

/// <summary>
/// Thrown when validation of data fails.
/// </summary>
public class ValidationException : MarketplaceException
{
    public ValidationException(string message)
        : base(message, "VALIDATION_FAILED")
    {
    }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("Validation failed", errors, "VALIDATION_FAILED")
    {
    }

    public ValidationException(string fieldName, string message)
        : base(message, new Dictionary<string, string[]> { { fieldName, new[] { message } } }, "VALIDATION_FAILED")
    {
    }
}
