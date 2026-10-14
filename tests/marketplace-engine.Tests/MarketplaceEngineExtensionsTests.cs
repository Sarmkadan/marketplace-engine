#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Tests for <see cref="MarketplaceEngineExtensions.AddMarketplaceEngine"/> startup validation.
/// Verifies that every invalid or missing <c>Marketplace</c> key is reported in one <see cref="OptionsValidationException"/>.
/// </summary>
[Collection("Database")]
public class MarketplaceEngineExtensionsTests
{
    /// <summary>
    /// Builds a host that registers the full marketplace engine from the given settings.
    /// </summary>
    private static IHost BuildHost(IDictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        return new HostBuilder()
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddMarketplaceEngine(configuration);
            })
            .Build();
    }

    /// <summary>
    /// Verifies that an empty configuration fails startup once, with a message that names the missing section and every missing required key.
    /// </summary>
    [Fact]
    public async Task StartAsync_WithEmptyConfiguration_ThrowsSingleAggregatedErrorListingAllMissingKeys()
    {
        using var host = BuildHost(new Dictionary<string, string?>());

        Func<Task> act = () => host.StartAsync();

        var exception = await act.Should().ThrowAsync<OptionsValidationException>();

        // One exception carries one block that lists every problem.
        exception.Which.Failures.Should().ContainSingle();
        exception.Which.Message.Should().StartWith("Invalid 'Marketplace' configuration (3 problem(s)):");
        exception.Which.Message.Should().Contain("  - Configuration section 'Marketplace' is missing.");
        exception.Which.Message.Should().Contain("  - Marketplace:PaymentGateway:ApiKey is required.");
        exception.Which.Message.Should().Contain("  - Marketplace:PaymentGateway:BaseUrl is required.");
    }

    /// <summary>
    /// Verifies that a valid configuration starts, and that each subsection is exposed as its own <c>IOptions</c> value.
    /// </summary>
    [Fact]
    public async Task StartAsync_WithValidConfiguration_StartsAndExposesEachSection()
    {
        using var host = BuildHost(new Dictionary<string, string?>
        {
            ["Marketplace:PaymentGateway:ApiKey"] = "test-api-key",
            ["Marketplace:PaymentGateway:BaseUrl"] = "https://api.payments.example.com",
            ["Marketplace:Inventory:LowStockThreshold"] = "3",
            ["Marketplace:Pricing:Currency"] = "EUR"
        });

        await host.StartAsync();

        var paymentGateway = host.Services.GetRequiredService<IOptions<PaymentGatewayOptions>>().Value;
        paymentGateway.BaseUrl.Should().Be("https://api.payments.example.com");

        var inventory = host.Services.GetRequiredService<IOptions<InventoryOptions>>().Value;
        inventory.LowStockThreshold.Should().Be(3);
        inventory.ReservationTtlMinutes.Should().Be(InventoryOptions.DefaultReservationTtlMinutes);

        var pricing = host.Services.GetRequiredService<IOptions<PricingOptions>>().Value;
        pricing.Currency.Should().Be("EUR");

        await host.StopAsync();
    }
}
