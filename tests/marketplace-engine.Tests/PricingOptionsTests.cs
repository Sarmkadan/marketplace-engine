#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Configuration;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Tests for <see cref="PricingOptions"/> validation and startup behavior.
/// Verifies that a misconfigured <c>Pricing</c> section fails the host at start with <see cref="OptionsValidationException"/>.
/// </summary>
[Collection("Database")]
public class PricingOptionsTests
{
    private const string Prefix = PricingOptions.SectionName + ":";

    private readonly PricingOptionsValidator _sut = new();

    /// <summary>
    /// Builds a host that registers the pricing options and a mocked listing repository for <see cref="Services.PricingEngine"/>.
    /// </summary>
    private static IHost BuildHost(IDictionary<string, string?> settings) =>
        new HostBuilder()
            .ConfigureAppConfiguration(config => config.AddInMemoryCollection(settings))
            .ConfigureServices((context, services) =>
            {
                services.AddLogging();
                services.AddSingleton(new Mock<IListingRepository>().Object);
                services.AddPricingOptions(context.Configuration);
            })
            .Build();

    /// <summary>
    /// Settings for a fully valid <c>Pricing</c> section.
    /// </summary>
    private static Dictionary<string, string?> ValidSettings() => new()
    {
        [Prefix + nameof(PricingOptions.DefaultTaxRate)] = "0.08",
        [Prefix + nameof(PricingOptions.RoundingMode)] = "ToEven",
        [Prefix + nameof(PricingOptions.MaxDiscountPercent)] = "25",
        [Prefix + nameof(PricingOptions.Currency)] = "EUR"
    };

    /// <summary>
    /// Options with every field set to a valid value.
    /// </summary>
    private static PricingOptions ValidOptions() => new()
    {
        DefaultTaxRate = 0.08m,
        RoundingMode = "ToEven",
        MaxDiscountPercent = 25m,
        Currency = "EUR"
    };

    [Fact]
    public void Defaults_AreValid()
    {
        _sut.Validate(null, new PricingOptions()).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ValidOptions_Succeeds()
    {
        _sut.Validate(null, ValidOptions()).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.08)]
    [InlineData(1.0)]
    public void Validate_TaxRateInsideRange_Succeeds(double rate)
    {
        var options = ValidOptions();
        options.DefaultTaxRate = (decimal)rate;

        _sut.Validate(null, options).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(8)]
    public void Validate_TaxRateOutsideRange_FailsNamingDefaultTaxRate(double rate)
    {
        var options = ValidOptions();
        options.DefaultTaxRate = (decimal)rate;

        var result = _sut.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("DefaultTaxRate");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_MaxDiscountPercentInsideRange_Succeeds(int percent)
    {
        var options = ValidOptions();
        options.MaxDiscountPercent = percent;

        _sut.Validate(null, options).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100.5)]
    public void Validate_MaxDiscountPercentOutsideRange_FailsNamingMaxDiscountPercent(double percent)
    {
        var options = ValidOptions();
        options.MaxDiscountPercent = (decimal)percent;

        var result = _sut.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("MaxDiscountPercent");
    }

    [Theory]
    [InlineData("AwayFromZero")]
    [InlineData("ToEven")]
    [InlineData("ToZero")]
    [InlineData("ToNegativeInfinity")]
    [InlineData("ToPositiveInfinity")]
    public void Validate_KnownRoundingMode_Succeeds(string mode)
    {
        var options = ValidOptions();
        options.RoundingMode = mode;

        _sut.Validate(null, options).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("Banker")]
    [InlineData("awayfromzero")]
    [InlineData("1")]
    [InlineData("")]
    public void Validate_UnknownRoundingMode_FailsNamingRoundingMode(string mode)
    {
        var options = ValidOptions();
        options.RoundingMode = mode;

        var result = _sut.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("RoundingMode");
    }

    [Fact]
    public void Validate_AllFieldsInvalid_ReportsEveryFailure()
    {
        var options = new PricingOptions
        {
            DefaultTaxRate = 2m,
            MaxDiscountPercent = 150m,
            RoundingMode = "Bogus",
            Currency = "US"
        };

        var result = _sut.Validate(null, options);

        result.Failed.Should().BeTrue();
        // The validator covers tax, discount, and rounding; the data annotations cover Currency.
        result.Failures.Should().HaveCount(3);
        result.Failures.Should().Contain(f => f.Contains("DefaultTaxRate"));
        result.Failures.Should().Contain(f => f.Contains("MaxDiscountPercent"));
        result.Failures.Should().Contain(f => f.Contains("RoundingMode"));
    }

    [Fact]
    public void Validate_NullOptions_Throws()
    {
        var act = () => _sut.Validate(null, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Host_ValidConfiguration_StartsAndBindsValues()
    {
        using var host = BuildHost(ValidSettings());

        var act = () => host.StartAsync();

        await act.Should().NotThrowAsync();

        var options = host.Services.GetRequiredService<IOptions<PricingOptions>>().Value;
        options.DefaultTaxRate.Should().Be(0.08m);
        options.RoundingMode.Should().Be("ToEven");
        options.MaxDiscountPercent.Should().Be(25m);
        options.Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task Host_TaxRateOutOfRange_FailsStartupNamingField()
    {
        var settings = ValidSettings();
        settings[Prefix + nameof(PricingOptions.DefaultTaxRate)] = "1.5";
        using var host = BuildHost(settings);

        var act = () => host.StartAsync();

        var exception = (await act.Should().ThrowAsync<OptionsValidationException>()).Which;
        exception.Failures.Should().Contain(f => f.Contains("DefaultTaxRate"));
    }

    [Fact]
    public async Task Host_UnknownRoundingMode_FailsStartupNamingField()
    {
        var settings = ValidSettings();
        settings[Prefix + nameof(PricingOptions.RoundingMode)] = "Banker";
        using var host = BuildHost(settings);

        var act = () => host.StartAsync();

        var exception = (await act.Should().ThrowAsync<OptionsValidationException>()).Which;
        exception.Failures.Should().Contain(f => f.Contains("RoundingMode"));
    }

    [Fact]
    public async Task Host_MaxDiscountPercentOutOfRange_FailsStartupNamingField()
    {
        var settings = ValidSettings();
        settings[Prefix + nameof(PricingOptions.MaxDiscountPercent)] = "101";
        using var host = BuildHost(settings);

        var act = () => host.StartAsync();

        var exception = (await act.Should().ThrowAsync<OptionsValidationException>()).Which;
        exception.Failures.Should().Contain(f => f.Contains("MaxDiscountPercent"));
    }

    [Theory]
    [InlineData("usd")]
    [InlineData("US")]
    [InlineData("")]
    public async Task Host_CurrencyNotThreeUppercaseLetters_FailsStartupNamingField(string currency)
    {
        var settings = ValidSettings();
        settings[Prefix + nameof(PricingOptions.Currency)] = currency;
        using var host = BuildHost(settings);

        var act = () => host.StartAsync();

        var exception = (await act.Should().ThrowAsync<OptionsValidationException>()).Which;
        exception.Failures.Should().Contain(f => f.Contains("Currency"));
    }

    [Fact]
    public async Task Host_MissingSection_UsesDefaultsAndStarts()
    {
        using var host = BuildHost(new Dictionary<string, string?>());

        var act = () => host.StartAsync();

        await act.Should().NotThrowAsync();
        var options = host.Services.GetRequiredService<IOptions<PricingOptions>>().Value;
        options.MaxDiscountPercent.Should().Be(PricingOptions.DefaultMaxDiscountPercent);
        options.RoundingMode.Should().Be(PricingOptions.DefaultRoundingMode);
        options.Currency.Should().Be(PricingOptions.DefaultCurrencyCode);
    }

    [Fact]
    public async Task Host_ResolvesPricingEngineWithConfiguredOptions()
    {
        using var host = BuildHost(ValidSettings());
        await host.StartAsync();

        var engine = host.Services.GetRequiredService<PricingEngine>();

        engine.ApplyDiscount(new Money(100m, "EUR"), 0.25m).DiscountedPrice.Amount.Should().Be(75m);
        var act = () => engine.ApplyDiscount(new Money(100m, "EUR"), 0.26m);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
