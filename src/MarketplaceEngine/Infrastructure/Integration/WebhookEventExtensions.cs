using System.Globalization;
using System.Text.Json;

namespace MarketplaceEngine.Infrastructure.Integration;

/// <summary>
/// Extension methods for <see cref="WebhookEvent"/> providing common webhook processing utilities.
/// </summary>
public static class WebhookEventExtensions
{
    /// <summary>
    /// Safely gets a strongly-typed value from the webhook data dictionary.
    /// Returns null if the webhook event, key, or value is null, or if deserialization fails.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <param name="webhookEvent">The webhook event.</param>
    /// <param name="key">The key in the Data dictionary.</param>
    /// <returns>The deserialized value or null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="webhookEvent"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null.</exception>
    public static T? GetData<T>(this WebhookEvent webhookEvent, string key)
    {
        ArgumentNullException.ThrowIfNull(webhookEvent);
        ArgumentNullException.ThrowIfNull(key);

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
    /// Returns null if the webhook event, key, or value is null.
    /// </summary>
    /// <param name="webhookEvent">The webhook event.</param>
    /// <param name="key">The key in the Data dictionary.</param>
    /// <returns>The string value or null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="webhookEvent"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null.</exception>
    public static string? GetDataString(this WebhookEvent webhookEvent, string key)
    {
        ArgumentNullException.ThrowIfNull(webhookEvent);
        ArgumentNullException.ThrowIfNull(key);

        return webhookEvent.Data.TryGetValue(key, out var value) && value is not null
            ? value.ToString()
            : null;
    }

    /// <summary>
    /// Safely gets a numeric value from the webhook data dictionary.
    /// Returns null if the webhook event, key, or value is null, or if conversion fails.
    /// </summary>
    /// <param name="webhookEvent">The webhook event.</param>
    /// <param name="key">The key in the Data dictionary.</param>
    /// <returns>The decimal value or null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="webhookEvent"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null.</exception>
    public static decimal? GetDataDecimal(this WebhookEvent webhookEvent, string key)
    {
        ArgumentNullException.ThrowIfNull(webhookEvent);
        ArgumentNullException.ThrowIfNull(key);

        if (webhookEvent.Data.TryGetValue(key, out var value) && value is not null)
        {
            return value switch
            {
                decimal decimalValue => decimalValue,
                string strValue => decimal.TryParse(strValue, CultureInfo.InvariantCulture, out var parsedValue) ? parsedValue : null,
                _ => decimal.TryParse(value.ToString(), CultureInfo.InvariantCulture, out var parsedValue) ? parsedValue : null
            };
        }

        return null;
    }

    /// <summary>
    /// Safely gets a boolean value from the webhook data dictionary.
    /// Returns null if the webhook event, key, or value is null, or if conversion fails.
    /// </summary>
    /// <param name="webhookEvent">The webhook event.</param>
    /// <param name="key">The key in the Data dictionary.</param>
    /// <returns>The boolean value or null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="webhookEvent"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null.</exception>
    public static bool? GetDataBoolean(this WebhookEvent webhookEvent, string key)
    {
        ArgumentNullException.ThrowIfNull(webhookEvent);
        ArgumentNullException.ThrowIfNull(key);

        if (webhookEvent.Data.TryGetValue(key, out var value) && value is not null)
        {
            return value switch
            {
                bool boolValue => boolValue,
                string strValue => bool.TryParse(strValue, out var parsedValue) ? parsedValue : null,
                _ => bool.TryParse(value.ToString(), out var parsedValue) ? parsedValue : null
            };
        }

        return null;
    }
}
