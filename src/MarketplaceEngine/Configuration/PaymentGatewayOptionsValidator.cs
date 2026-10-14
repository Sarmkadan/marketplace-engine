#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Validates <see cref="PaymentGatewayOptions"/> and reports every invalid field in a single failure,
/// so a misconfigured host fails at startup with a complete list of problems.
/// </summary>
public sealed class PaymentGatewayOptionsValidator : IValidateOptions<PaymentGatewayOptions>
{
    /// <summary>
    /// Smallest accepted <see cref="PaymentGatewayOptions.TimeoutSeconds"/>.
    /// </summary>
    public const int MinTimeoutSeconds = 1;

    /// <summary>
    /// Largest accepted <see cref="PaymentGatewayOptions.TimeoutSeconds"/>.
    /// </summary>
    public const int MaxTimeoutSeconds = 120;

    /// <summary>
    /// Smallest accepted <see cref="PaymentGatewayOptions.MaxRetries"/>.
    /// </summary>
    public const int MinMaxRetries = 0;

    /// <summary>
    /// Largest accepted <see cref="PaymentGatewayOptions.MaxRetries"/>.
    /// </summary>
    public const int MaxMaxRetries = 10;

    /// <summary>
    /// Length of an ISO-4217 currency code.
    /// </summary>
    public const int CurrencyCodeLength = 3;

    /// <summary>
    /// Validates the payment gateway options.
    /// </summary>
    /// <param name="name">The options instance name, if any; not used.</param>
    /// <param name="options">The options to validate.</param>
    /// <returns>Success when all fields are valid; otherwise a failure listing each invalid field.</returns>
    public ValidateOptionsResult Validate(string? name, PaymentGatewayOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            failures.Add($"{PaymentGatewayOptions.SectionName}:ApiKey is required.");

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            failures.Add($"{PaymentGatewayOptions.SectionName}:BaseUrl is required.");
        else if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri) || baseUri.Scheme != Uri.UriSchemeHttps)
            failures.Add($"{PaymentGatewayOptions.SectionName}:BaseUrl must be an absolute HTTPS URL (was '{options.BaseUrl}').");

        if (options.TimeoutSeconds < MinTimeoutSeconds || options.TimeoutSeconds > MaxTimeoutSeconds)
            failures.Add($"{PaymentGatewayOptions.SectionName}:TimeoutSeconds must be between {MinTimeoutSeconds} and {MaxTimeoutSeconds} (was {options.TimeoutSeconds}).");

        if (options.MaxRetries < MinMaxRetries || options.MaxRetries > MaxMaxRetries)
            failures.Add($"{PaymentGatewayOptions.SectionName}:MaxRetries must be between {MinMaxRetries} and {MaxMaxRetries} (was {options.MaxRetries}).");

        if (!IsIso4217Format(options.Currency))
            failures.Add($"{PaymentGatewayOptions.SectionName}:Currency must be a {CurrencyCodeLength}-letter uppercase ISO-4217 code (was '{options.Currency}').");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    // Checks the shape of an ISO-4217 code (three ASCII uppercase letters). It does not
    // check the code against the official list of currencies.
    private static bool IsIso4217Format(string? currency)
    {
        if (currency is null || currency.Length != CurrencyCodeLength)
            return false;

        foreach (var c in currency)
        {
            if (c is < 'A' or > 'Z')
                return false;
        }

        return true;
    }
}
