#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Extension methods that register the payment gateway options.
/// </summary>
public static class PaymentGatewayExtensions
{
    /// <summary>
    /// Binds <see cref="PaymentGatewayOptions"/> from the <c>PaymentGateway</c> configuration section
    /// and validates them when the host starts. An invalid configuration stops startup with an
    /// <see cref="OptionsValidationException"/> that names each invalid field, instead of failing later on the first payment.
    /// </summary>
    /// <param name="services">The service collection to add the options to.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddPaymentGatewayOptions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<PaymentGatewayOptions>()
            .BindConfiguration(PaymentGatewayOptions.SectionName)
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<PaymentGatewayOptions>, PaymentGatewayOptionsValidator>();

        return services;
    }
}
