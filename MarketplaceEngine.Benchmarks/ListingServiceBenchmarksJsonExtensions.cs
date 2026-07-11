using System.Text.Json;

namespace MarketplaceEngine.Benchmarks;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ListingServiceBenchmarks"/>.
/// </summary>
public static class ListingServiceBenchmarksJsonExtensions
{
    /// <summary>
    /// Gets the JSON serialization options with camelCase naming convention.
    /// </summary>
    /// <remarks>
    /// This instance is immutable and thread-safe.
    /// </remarks>
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="ListingServiceBenchmarks"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The benchmarks instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the benchmarks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this ListingServiceBenchmarks value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ListingServiceBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized benchmarks instance, or null if JSON is empty or whitespace.</returns>
    public static ListingServiceBenchmarks? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ListingServiceBenchmarks>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ListingServiceBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized benchmarks instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out ListingServiceBenchmarks? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ListingServiceBenchmarks>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}