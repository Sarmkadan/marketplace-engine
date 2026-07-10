#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="Payment"/> objects.
/// </summary>
public static class PaymentJsonExtensions
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    /// <summary>
    /// Serializes a <see cref="Payment"/> object to a JSON string.
    /// </summary>
    /// <param name="value">The payment to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the payment.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this Payment value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? GetIndentedOptions() : _options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="Payment"/> object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized payment, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static Payment? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<Payment>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="Payment"/> object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized payment, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out Payment? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<Payment>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static JsonSerializerOptions GetIndentedOptions()
    {
        var options = new JsonSerializerOptions(_options)
        {
            WriteIndented = true
        };
        return options;
    }
}