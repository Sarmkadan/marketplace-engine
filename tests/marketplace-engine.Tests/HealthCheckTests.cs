#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Configuration;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Infrastructure.HealthChecks;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Tests for the payment gateway and inventory health checks and their registration via
/// <see cref="HealthCheckExtensions.AddMarketplaceHealthChecks"/>.
/// </summary>
[Collection("Database")]
public class HealthCheckTests
{
    private const string SecretApiKey = "super-secret-api-key";

    /// <summary>
    /// Payment gateway options with every required field set.
    /// </summary>
    private static PaymentGatewayOptions ValidPaymentOptions() => new()
    {
        ApiKey = SecretApiKey,
        BaseUrl = "https://api.payments.example.com",
        TimeoutSeconds = 30,
        MaxRetries = 3,
        Currency = "USD"
    };

    [Fact]
    public async Task PaymentGatewayHealthCheck_ReturnsHealthy_WhenConfigurationIsValid()
    {
        var sut = new PaymentGatewayHealthCheck(Options.Create(ValidPaymentOptions()));

        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task PaymentGatewayHealthCheck_ReturnsUnhealthy_WhenApiKeyIsMissing()
    {
        var options = ValidPaymentOptions();
        options.ApiKey = string.Empty;
        var sut = new PaymentGatewayHealthCheck(Options.Create(options));

        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
        var failures = (string[])result.Data[PaymentGatewayHealthCheck.FailuresKey];
        failures.Should().ContainSingle(f => f.Contains("ApiKey"));
    }

    [Fact]
    public async Task PaymentGatewayHealthCheck_ReturnsUnhealthy_WhenBaseUrlIsNotHttps()
    {
        var options = ValidPaymentOptions();
        options.BaseUrl = "http://api.payments.example.com";
        var sut = new PaymentGatewayHealthCheck(Options.Create(options));

        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
        var failures = (string[])result.Data[PaymentGatewayHealthCheck.FailuresKey];
        failures.Should().ContainSingle(f => f.Contains("BaseUrl"));
    }

    [Fact]
    public async Task PaymentGatewayHealthCheck_NeverExposesApiKey()
    {
        var options = ValidPaymentOptions();
        options.BaseUrl = "not-a-url";
        var sut = new PaymentGatewayHealthCheck(Options.Create(options));

        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().NotContain(SecretApiKey);
        foreach (var failure in (string[])result.Data[PaymentGatewayHealthCheck.FailuresKey])
            failure.Should().NotContain(SecretApiKey);
    }

    [Fact]
    public async Task InventoryHealthCheck_ReturnsHealthy_WithLowStockCount_WhenStoreIsReachable()
    {
        // Create the listings explicitly: the shared store may be cleared by other tests in the collection.
        var repository = new ListingRepository();
        var lowStockListing = await repository.AddAsync(new Listing { Id = Guid.NewGuid(), Title = "Low stock" });
        var healthyStockListing = await repository.AddAsync(new Listing { Id = Guid.NewGuid(), Title = "Healthy stock" });

        var tracker = new InventoryTracker(repository);
        await tracker.SetStockAsync(lowStockListing.Id, 2);
        await tracker.SetStockAsync(healthyStockListing.Id, 50);

        var sut = new InventoryHealthCheck(
            repository,
            tracker,
            Options.Create(new InventoryOptions { LowStockThreshold = 5 }));

        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data[InventoryHealthCheck.LowStockCountKey].Should().Be(1);
    }

    [Fact]
    public async Task InventoryHealthCheck_ReturnsHealthy_WithZeroLowStock_WhenNoStockIsTracked()
    {
        var sut = new InventoryHealthCheck(
            new ListingRepository(),
            new InventoryTracker(new ListingRepository()),
            Options.Create(new InventoryOptions()));

        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data[InventoryHealthCheck.LowStockCountKey].Should().Be(0);
    }

    [Fact]
    public async Task InventoryHealthCheck_ReturnsUnhealthy_WhenStoreThrows()
    {
        var repository = new Mock<IListingRepository>();
        repository.Setup(r => r.GetAllAsync()).ThrowsAsync(new InvalidOperationException("store down"));

        var sut = new InventoryHealthCheck(
            repository.Object,
            new InventoryTracker(repository.Object),
            Options.Create(new InventoryOptions()));

        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Exception.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task AddMarketplaceHealthChecks_RegistersBothChecks_AndReportsHealthy()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMarketplaceServices();
        services.AddSingleton(Options.Create(ValidPaymentOptions()));
        services.AddSingleton(Options.Create(new InventoryOptions()));
        services.AddMarketplaceHealthChecks();

        await using var provider = services.BuildServiceProvider();
        var report = await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync();

        report.Status.Should().Be(HealthStatus.Healthy);
        report.Entries.Should().ContainKeys("payment-gateway", "inventory");
    }
}
