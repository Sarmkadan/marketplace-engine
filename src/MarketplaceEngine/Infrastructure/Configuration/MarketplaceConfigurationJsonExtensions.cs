using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace MarketplaceEngine.Infrastructure.Configuration;

public static class MarketplaceConfigurationJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        {
            Modifiers = { static options =>
            {
                if (options.Type == typeof(MarketplaceConfiguration))
                {
                    foreach (var property in options.Properties)
                    {
                        property.Name = property.Name switch
                        {
                            "MarketplaceUrl" => "marketplaceUrl",
                            "ApiEndpoint" => "apiEndpoint",
                            "MaxConcurrentRequests" => "maxConcurrentRequests",
                            "CacheDurationMinutes" => "cacheDurationMinutes",
                            "EnableLogging" => "enableLogging",
                            _ => property.Name
                        };
                    }
                }
            } }
        }
    };

    /// <summary>
    /// Serializes the <see cref="MarketplaceConfiguration"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The configuration to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this MarketplaceConfiguration value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="MarketplaceConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized configuration, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static MarketplaceConfiguration? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<MarketplaceConfiguration>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="MarketplaceConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized configuration if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out MarketplaceConfiguration? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<MarketplaceConfiguration>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}