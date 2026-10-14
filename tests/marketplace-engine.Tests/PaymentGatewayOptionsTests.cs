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
/// Tests for <see cref="PaymentGatewayOptions"/> validation and startup behavior.
/// Verifies that a misconfigured host fails at start with <see cref="OptionsValidationException"/>.
/// </summary>
[Collection("Database")]
public class PaymentGatewayOptionsTests
{
    private const string Prefix = PaymentGatewayOptions.SectionName + ":";

    private readonly PaymentGatewayOptionsValidator _sut = new();

    /// <summary>
    /// Builds a host that registers only the payment gateway options.
    /// </summary>
    private static IHost BuildHost(IDictionary<string, string?> settings) =>
        new HostBuilder()
            .ConfigureAppConfiguration(config => config.AddInMemoryCollection(settings))
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddPaymentGatewayOptions();
            })
            .Build();

    /// <summary>
    /// Settings for a fully valid configuration.
    /// </summary>
    private static Dictionary<string, string?> ValidSettings() => new()
    {
        [Prefix + nameof(PaymentGatewayOptions.ApiKey)] = "test-api-key",
        [Prefix + nameof(PaymentGatewayOptions.BaseUrl)] = "https://api.payments.example.com",
        [Prefix + nameof(PaymentGatewayOptions.TimeoutSeconds)] = "30",
        [Prefix + nameof(PaymentGatewayOptions.MaxRetries)] = "3",
        [Prefix + nameof(PaymentGatewayOptions.Currency)] = "USD"
    };

    private static PaymentGatewayOptions ValidOptions() => new()
    {
        ApiKey = "test-api-key",
        BaseUrl = "https://api.payments.example.com",
        TimeoutSeconds = 30,
        MaxRetries = 3,
        Currency = "USD"
    };

    [Fact]
    public void Validate_ValidOptions_Succeeds()
    {
        var result = _sut.Validate(null, ValidOptions());

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(120)]
    public void Validate_TimeoutAtBoundary_Succeeds(int timeout)
    {
        var options = ValidOptions();
        options.TimeoutSeconds = timeout;

        _sut.Validate(null, options).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(121)]
    public void Validate_TimeoutOutsideRange_FailsNamingTimeoutSeconds(int timeout)
    {
        var options = ValidOptions();
        options.TimeoutSeconds = timeout;

        var result = _sut.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("TimeoutSeconds");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void Validate_MaxRetriesAtBoundary_Succeeds(int retries)
    {
        var options = ValidOptions();
        options.MaxRetries = retries;

        _sut.Validate(null, options).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void Validate_MaxRetriesOutsideRange_FailsNamingMaxRetries(int retries)
    {
        var options = ValidOptions();
        options.MaxRetries = retries;

        var result = _sut.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("MaxRetries");
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    public void Validate_ThreeLetterUppercaseCurrency_Succeeds(string currency)
    {
        var options = ValidOptions();
        options.Currency = currency;

        _sut.Validate(null, options).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDX")]
    [InlineData("usd")]
    [InlineData("U5D")]
    public void Validate_CurrencyNotThreeUppercaseLetters_FailsNamingCurrency(string currency)
    {
        var options = ValidOptions();
        options.Currency = currency;

        var result = _sut.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("Currency");
    }

    [Theory]
    [InlineData("http://api.payments.example.com")]
    [InlineData("api.payments.example.com")]
    [InlineData("ftp://api.payments.example.com")]
    public void Validate_BaseUrlNotAbsoluteHttps_FailsNamingBaseUrl(string baseUrl)
    {
        var options = ValidOptions();
        options.BaseUrl = baseUrl;

        var result = _sut.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("BaseUrl");
    }

    [Fact]
    public void Validate_EveryFieldInvalid_ReportsEachFieldOnce()
    {
        var options = new PaymentGatewayOptions
        {
            ApiKey = " ",
            BaseUrl = "",
            TimeoutSeconds = 0,
            MaxRetries = 99,
            Currency = "usd"
        };

        var result = _sut.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().HaveCount(5);
        result.Failures.Should().Contain(f => f.Contains("ApiKey"));
        result.Failures.Should().Contain(f => f.Contains("BaseUrl"));
        result.Failures.Should().Contain(f => f.Contains("TimeoutSeconds"));
        result.Failures.Should().Contain(f => f.Contains("MaxRetries"));
        result.Failures.Should().Contain(f => f.Contains("Currency"));
    }

    [Fact]
    public async Task StartAsync_ValidConfiguration_Starts()
    {
        using var host = BuildHost(ValidSettings());

        Func<Task> act = () => host.StartAsync();

        await act.Should().NotThrowAsync();
        await host.StopAsync();
    }

    [Fact]
    public void Build_MisconfiguredHost_DoesNotThrowUntilStart()
    {
        var settings = ValidSettings();
        settings[Prefix + nameof(PaymentGatewayOptions.ApiKey)] = "";

        // Building the host only registers services; validation runs when the host starts.
        Action build = () => BuildHost(settings).Dispose();

        build.Should().NotThrow();
    }

    [Fact]
    public async Task StartAsync_MissingApiKey_ThrowsOptionsValidationExceptionNamingApiKey()
    {
        var settings = ValidSettings();
        settings.Remove(Prefix + nameof(PaymentGatewayOptions.ApiKey));
        using var host = BuildHost(settings);

        Func<Task> act = () => host.StartAsync();

        await act.Should().ThrowAsync<OptionsValidationException>()
            .WithMessage("*ApiKey*");
    }

    [Theory]
    [InlineData(nameof(PaymentGatewayOptions.ApiKey), "", "ApiKey")]
    [InlineData(nameof(PaymentGatewayOptions.BaseUrl), "http://api.payments.example.com", "BaseUrl")]
    [InlineData(nameof(PaymentGatewayOptions.TimeoutSeconds), "121", "TimeoutSeconds")]
    [InlineData(nameof(PaymentGatewayOptions.MaxRetries), "11", "MaxRetries")]
    [InlineData(nameof(PaymentGatewayOptions.Currency), "US", "Currency")]
    public async Task StartAsync_InvalidField_ThrowsOptionsValidationExceptionNamingField(
        string key, string value, string expectedField)
    {
        var settings = ValidSettings();
        settings[Prefix + key] = value;
        using var host = BuildHost(settings);

        Func<Task> act = () => host.StartAsync();

        await act.Should().ThrowAsync<OptionsValidationException>()
            .WithMessage($"*{expectedField}*");
    }

    [Fact]
    public async Task StartAsync_MultipleInvalidFields_MessageNamesEachField()
    {
        var settings = ValidSettings();
        settings[Prefix + nameof(PaymentGatewayOptions.ApiKey)] = "";
        settings[Prefix + nameof(PaymentGatewayOptions.TimeoutSeconds)] = "0";
        settings[Prefix + nameof(PaymentGatewayOptions.Currency)] = "US";
        using var host = BuildHost(settings);

        Func<Task> act = () => host.StartAsync();

        var assertion = await act.Should().ThrowAsync<OptionsValidationException>();
        assertion.Which.Message.Should().Contain("ApiKey")
            .And.Contain("TimeoutSeconds")
            .And.Contain("Currency");
    }

    [Fact]
    public async Task StartAsync_WithMarketplaceServicesAndMissingApiKey_FailsAtStartNotOnFirstUse()
    {
        // Uses the real application wiring so the check covers what Program.cs registers.
        var settings = ValidSettings();
        settings[Prefix + nameof(PaymentGatewayOptions.ApiKey)] = "";
        using var host = new HostBuilder()
            .ConfigureAppConfiguration(config => config.AddInMemoryCollection(settings))
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddMarketplaceServices();
            })
            .Build();

        Func<Task> act = () => host.StartAsync();

        await act.Should().ThrowAsync<OptionsValidationException>()
            .WithMessage("*ApiKey*");
    }
}
