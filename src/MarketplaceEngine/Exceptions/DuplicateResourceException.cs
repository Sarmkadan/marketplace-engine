// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Exceptions;

/// <summary>
/// Thrown when attempting to create a resource that already exists.
/// </summary>
public class DuplicateResourceException : MarketplaceException
{
    public string ResourceType { get; }
    public string FieldName { get; }

    public DuplicateResourceException(string resourceType, string fieldName, string value)
        : base($"{resourceType} with {fieldName} '{value}' already exists", "DUPLICATE_RESOURCE")
    {
        ResourceType = resourceType;
        FieldName = fieldName;
    }
}
