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
/// The single entry point that registers every marketplace service and options object.
/// </summary>
public static class MarketplaceEngineExtensions
{
    /// <summary>
    /// Registers all marketplace services, search, recommendations and the <see cref="MarketplaceOptions"/> tree.
    /// <para>
    /// The options are bound from the <c>Marketplace</c> section and validated once when the host starts
    /// (<c>ValidateOnStart</c>). A missing section or any invalid key stops startup with a single
    /// <see cref="Microsoft.Extensions.Options.OptionsValidationException"/> that lists every problem.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The application configuration that contains the <c>Marketplace</c> section.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configuration"/> is null.</exception>
    public static IServiceCollection AddMarketplaceEngine(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddMarketplaceServices();
        services.AddFullTextSearch();
        services.AddRecommendationEngine(configuration);
        services.AddMarketplaceHealthChecks();

        // One root options object is validated at startup, so a single failure reports every invalid key.
        services.AddOptions<MarketplaceOptions>()
            .Bind(configuration.GetSection(MarketplaceOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<MarketplaceOptions>>(new MarketplaceOptionsValidator(configuration));

        // Expose each subsection as its own IOptions<T>, read from the validated root.
        services.AddSingleton<IOptions<PricingOptions>>(provider =>
            Options.Create(provider.GetRequiredService<IOptions<MarketplaceOptions>>().Value.Pricing));
        services.AddSingleton<IOptions<PaymentGatewayOptions>>(provider =>
            Options.Create(provider.GetRequiredService<IOptions<MarketplaceOptions>>().Value.PaymentGateway));
        services.AddSingleton<IOptions<InventoryOptions>>(provider =>
            Options.Create(provider.GetRequiredService<IOptions<MarketplaceOptions>>().Value.Inventory));

        services.AddSingleton<PricingEngine>();

        return services;
    }
}
