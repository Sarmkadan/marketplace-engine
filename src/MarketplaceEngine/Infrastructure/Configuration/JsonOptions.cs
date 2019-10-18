// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Infrastructure.Configuration;

/// <summary>
/// Configures JSON serialization options for API responses.
/// Ensures consistent formatting across all endpoints.
/// </summary>
public static class JsonOptions
{
    public static JsonSerializerOptions GetDefaultOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
    }

    public static JsonSerializerOptions GetPrettyOptions()
    {
        var options = GetDefaultOptions();
        options.WriteIndented = true;
        return options;
    }
}
