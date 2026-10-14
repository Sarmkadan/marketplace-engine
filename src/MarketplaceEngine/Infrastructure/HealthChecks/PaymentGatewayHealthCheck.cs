#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace MarketplaceEngine.Infrastructure.HealthChecks;

/// <summary>
/// Reports whether the payment gateway configuration is present and valid.
/// Does not contact the provider, and never reports the API key.
/// </summary>
public sealed class PaymentGatewayHealthCheck : IHealthCheck
{
    /// <summary>
    /// Data key that holds the list of configuration failures when the check is unhealthy.
    /// </summary>
    public const string FailuresKey = "failures";

    private readonly IOptions<PaymentGatewayOptions> _options;
    private readonly PaymentGatewayOptionsValidator _validator = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentGatewayHealthCheck"/> class.
    /// </summary>
    /// <param name="options">The payment gateway options to inspect.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public PaymentGatewayHealthCheck(IOptions<PaymentGatewayOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Validates the payment gateway configuration.
    /// </summary>
    /// <returns>Healthy when the configuration is valid; otherwise Unhealthy with each failure listed.</returns>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var result = _validator.Validate(null, _options.Value);

        if (result.Succeeded)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Payment gateway configuration is present and valid."));
        }

        var failures = result.Failures.ToArray();
        return Task.FromResult(HealthCheckResult.Unhealthy(
            $"Payment gateway configuration is invalid ({failures.Length} problem(s)).",
            data: new Dictionary<string, object> { [FailuresKey] = failures }));
    }
}
