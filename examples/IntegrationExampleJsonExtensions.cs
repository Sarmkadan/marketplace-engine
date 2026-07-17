#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarketplaceEngine.Examples;

/// <summary>
/// Provides System.Text.Json serialization extensions for the <see cref="IntegrationExample"/> type.
/// Enables serialization and deserialization of integration example metadata with camelCase naming convention.
/// </summary>
public static class IntegrationExampleJsonExtensions
{
    /// <summary>
    /// Gets the JSON serialization options with camelCase naming convention for IntegrationExample instances.
    /// </summary>
    /// <remarks>
    /// Uses Web defaults with camelCase property naming, ignores null values when writing,
    /// and does not indent JSON output by default.
    /// </remarks>
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes an IntegrationExample instance to a JSON string.
    /// </summary>
    /// <param name="value">The IntegrationExample instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the IntegrationExample.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static string ToJson(this IntegrationExample? value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes an IntegrationExample from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An IntegrationExample instance, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static IntegrationExample? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<IntegrationExample>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize an IntegrationExample from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The resulting IntegrationExample instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string? json, out IntegrationExample? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<IntegrationExample>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}