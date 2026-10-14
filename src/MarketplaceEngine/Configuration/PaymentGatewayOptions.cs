#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Strongly-typed settings for the external payment gateway, bound from the
/// <c>Marketplace:PaymentGateway</c> configuration section. Validated at startup by
/// <see cref="PaymentGatewayOptionsValidator"/>.
/// </summary>
public class PaymentGatewayOptions
{
    /// <summary>
    /// Name of the configuration section these options are bound from.
    /// </summary>
    public const string SectionName = MarketplaceOptions.SectionName + ":PaymentGateway";

    /// <summary>
    /// Default request timeout in seconds.
    /// </summary>
    public const int DefaultTimeoutSeconds = 30;

    /// <summary>
    /// Default number of retry attempts for a failed gateway call.
    /// </summary>
    public const int DefaultMaxRetries = 3;

    /// <summary>
    /// Default ISO-4217 currency code.
    /// </summary>
    public const string DefaultCurrency = "USD";

    /// <summary>
    /// API key used to authenticate with the payment provider. Required; supply it through
    /// configuration sources that are not committed to source control (environment variables, user secrets).
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Absolute HTTPS base URL of the payment provider API. Required.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Request timeout in seconds. Must be between 1 and 120.
    /// </summary>
    public int TimeoutSeconds { get; set; } = DefaultTimeoutSeconds;

    /// <summary>
    /// Number of retry attempts for a failed gateway call. Must be between 0 and 10.
    /// </summary>
    public int MaxRetries { get; set; } = DefaultMaxRetries;

    /// <summary>
    /// Three-letter ISO-4217 currency code used for payments (for example <c>USD</c>).
    /// </summary>
    public string Currency { get; set; } = DefaultCurrency;
}
