using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace MarketplaceEngine.Infrastructure.Configuration;

internal static class StringExtensions
{
    public static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value) || char.IsLower(value[0]))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="MarketplaceConfiguration"/>.
/// </summary>
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
                            "Caching" => "caching",
                            "RateLimit" => "rateLimit",
                            "BackgroundJobs" => "backgroundJobs",
                            "Integration" => "integration",
                            "Security" => "security",
                            "Logging" => "logging",
                            _ => property.Name.ToCamelCase()
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

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="MarketplaceConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static MarketplaceConfiguration FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json.Trim());

        return JsonSerializer.Deserialize<MarketplaceConfiguration>(json, _jsonOptions)
               ?? throw new JsonException("Deserialization returned null for non-null input");
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="MarketplaceConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized configuration if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out MarketplaceConfiguration? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<MarketplaceConfiguration>(json.Trim(), _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}