#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Exceptions;

/// <summary>
/// Thrown when user lacks authorization to perform an action.
/// </summary>
public class UnauthorizedException : MarketplaceException
{
    public Guid UserId { get; }
    public string Action { get; }

    public UnauthorizedException(Guid userId, string action)
        : base($"User {userId} is not authorized to {action}", "UNAUTHORIZED")
    {
        UserId = userId;
        Action = action;
    }

    public UnauthorizedException(string message)
        : base(message, "UNAUTHORIZED")
    {
        UserId = Guid.Empty;
        Action = string.Empty;
    }
}
