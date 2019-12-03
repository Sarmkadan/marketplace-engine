using System.Text.Json;

namespace MarketplaceEngine.Infrastructure.Integration;

/// <summary>
/// Extension methods for WebhookEvent providing common webhook processing utilities.
/// </summary>
public static class WebhookEventExtensions
{
    /// <summary>
    /// Safely gets a strongly-typed value from the webhook data dictionary.
    /// Returns null if the key doesn't exist or the value is null.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <param name="webhookEvent">The webhook event</param>
    /// <param name="key">The key in the Data dictionary</param>
    /// <returns>The deserialized value or null</returns>
    public static T? GetData<T>(this WebhookEvent webhookEvent, string key)
    {
        if (webhookEvent.Data.TryGetValue(key, out var value) && value is not null)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(value.ToString()!);
            }
            catch (JsonException)
            {
                // Try direct cast for simple types
                if (value is T typedValue)
                {
                    return typedValue;
                }
            }
        }

        return default;
    }

    /// <summary>
    /// Safely gets a string value from the webhook data dictionary.
    /// Returns null if the key doesn't exist or the value is null.
    /// </summary>
    /// <param name="webhookEvent">The webhook event</param>
    /// <param name="key">The key in the Data dictionary</param>
    /// <returns>The string value or null</returns>
    public static string? GetDataString(this WebhookEvent webhookEvent, string key)
    {
        if (webhookEvent.Data.TryGetValue(key, out var value) && value is not null)
        {
            return value.ToString();
        }

        return null;
    }

    /// <summary>
    /// Safely gets a numeric value from the webhook data dictionary.
    /// Returns null if the key doesn't exist, the value is null, or conversion fails.
    /// </summary>
    /// <param name="webhookEvent">The webhook event</param>
    /// <param name="key">The key in the Data dictionary</param>
    /// <returns>The decimal value or null</returns>
    public static decimal? GetDataDecimal(this WebhookEvent webhookEvent, string key)
    {
        if (webhookEvent.Data.TryGetValue(key, out var value) && value is not null)
        {
            if (value is decimal decimalValue)
            {
                return decimalValue;
            }

            if (decimal.TryParse(value.ToString(), out var parsedValue))
            {
                return parsedValue;
            }
        }

        return null;
    }

    /// <summary>
    /// Safely gets a boolean value from the webhook data dictionary.
    /// Returns null if the key doesn't exist, the value is null, or conversion fails.
    /// </summary>
    /// <param name="webhookEvent">The webhook event</param>
    /// <param name="key">The key in the Data dictionary</param>
    /// <returns>The boolean value or null</returns>
    public static bool? GetDataBoolean(this WebhookEvent webhookEvent, string key)
    {
        if (webhookEvent.Data.TryGetValue(key, out var value) && value is not null)
        {
            if (value is bool boolValue)
            {
                return boolValue;
            }

            if (bool.TryParse(value.ToString(), out var parsedValue))
            {
                return parsedValue;
            }
        }

        return null;
    }
}
