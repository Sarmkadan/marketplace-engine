#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Extension methods that register the pricing options and the <see cref="PricingEngine"/> that reads them.
/// </summary>
public static class PricingExtensions
{
    /// <summary>
    /// Binds <see cref="PricingOptions"/> from the <c>Pricing</c> configuration section, validates them with
    /// data annotations and <see cref="PricingOptionsValidator"/>, and checks them when the host starts.
    /// An invalid configuration stops startup with an <see cref="OptionsValidationException"/> that names each invalid field.
    /// Also registers <see cref="PricingEngine"/> as a singleton.
    /// </summary>
    /// <param name="services">The service collection to add the options to.</param>
    /// <param name="configuration">The application configuration that contains the <c>Pricing</c> section.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configuration"/> is null.</exception>
    public static IServiceCollection AddPricingOptions(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<PricingOptions>()
            .Bind(configuration.GetSection(PricingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<PricingOptions>, PricingOptionsValidator>();
        services.AddSingleton<PricingEngine>();

        return services;
    }
}
