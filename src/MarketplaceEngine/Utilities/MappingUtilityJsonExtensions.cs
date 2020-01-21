#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarketplaceEngine.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for MappingUtility.
/// Enables easy serialization and deserialization of MappingUtility-related data.
/// </summary>
public static class MappingUtilityJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes MappingUtility metadata to a JSON string representation.
    /// Since MappingUtility is a static class with no state, this returns metadata
    /// about the available mapping methods.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of MappingUtility metadata.</returns>
    public static string ToJson(bool indented = false)
    {
        var mappingMetadata = new
        {
            Type = nameof(MappingUtility),
            Description = "Centralized mapping utility for converting between domain models and DTOs",
            Mappings = new object[]
            {
                new { Method = "ToDto", Target = "ListingDto", Description = "Maps Listing to ListingDto" },
                new { Method = "ToDto", Target = "UserDto", Description = "Maps User to UserDto" },
                new { Method = "ToDto", Target = "MessageDto", Description = "Maps Message to MessageDto" },
                new { Method = "ToDto", Target = "ModerationReportDto", Description = "Maps ModerationReport to ModerationReportDto" },
                new { Method = "ToListingDtos", Target = "List<ListingDto>", Description = "Maps multiple Listings to DTOs" },
                new { Method = "ToUserDtos", Target = "List<UserDto>", Description = "Maps multiple Users to DTOs" },
                new { Method = "ToMessageDtos", Target = "List<MessageDto>", Description = "Maps multiple Messages to DTOs" }
            }
        };

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(mappingMetadata, options);
    }

    /// <summary>
    /// Deserializes a JSON string.
    /// Since MappingUtility is a static class with no state, this always returns null.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>Always null since MappingUtility is static and has no state.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static object? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<object>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            throw;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string.
    /// Since MappingUtility is a static class with no state, this always returns false with value set to null.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives null since MappingUtility is static.</param>
    /// <returns>False since MappingUtility cannot be deserialized to.</returns>
    public static bool TryFromJson(string json, out object? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<object>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}