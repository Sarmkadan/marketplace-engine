#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Validates the whole <see cref="MarketplaceOptions"/> tree and aggregates every problem into one failure.
/// It reports a missing <c>Marketplace</c> section first, then each invalid or missing key in
/// <see cref="PricingOptions"/>, <see cref="PaymentGatewayOptions"/> and <see cref="InventoryOptions"/>,
/// so an operator can fix the whole configuration in one pass.
/// </summary>
public sealed class MarketplaceOptionsValidator : IValidateOptions<MarketplaceOptions>
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarketplaceOptionsValidator"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration, used to detect a missing <c>Marketplace</c> section.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public MarketplaceOptionsValidator(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Validates the marketplace options and collects all failures.
    /// </summary>
    /// <param name="name">The options instance name, if any; not used.</param>
    /// <param name="options">The options to validate.</param>
    /// <returns>Success when every section is valid; otherwise one failure per invalid or missing key.</returns>
    public ValidateOptionsResult Validate(string? name, MarketplaceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (!_configuration.GetSection(MarketplaceOptions.SectionName).Exists())
        {
            failures.Add($"Configuration section '{MarketplaceOptions.SectionName}' is missing. " +
                $"Add a '{MarketplaceOptions.SectionName}' section to appsettings.json or set its values with environment variables " +
                $"(for example {MarketplaceOptions.SectionName}__{nameof(MarketplaceOptions.PaymentGateway)}__{nameof(PaymentGatewayOptions.ApiKey)}).");
        }

        AddDataAnnotationFailures(failures, options.Pricing);
        AddResultFailures(failures, new PricingOptionsValidator().Validate(null, options.Pricing));
        AddResultFailures(failures, new PaymentGatewayOptionsValidator().Validate(null, options.PaymentGateway));
        AddResultFailures(failures, new InventoryOptionsValidator().Validate(null, options.Inventory));

        if (failures.Count == 0)
            return ValidateOptionsResult.Success;

        // Report everything as one readable block. OptionsValidationException would otherwise join
        // separate failures with "; " on a single line.
        var report = new StringBuilder()
            .AppendLine($"Invalid '{MarketplaceOptions.SectionName}' configuration ({failures.Count} problem(s)):");
        foreach (var failure in failures)
            report.AppendLine($"  - {failure}");

        return ValidateOptionsResult.Fail(report.ToString().TrimEnd());
    }

    // Runs the data annotations on a nested section. ValidateDataAnnotations() only checks the root object.
    private static void AddDataAnnotationFailures<T>(List<string> failures, T instance) where T : notnull
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);

        failures.AddRange(results.Select(result => result.ErrorMessage ?? "Invalid configuration value."));
    }

    private static void AddResultFailures(List<string> failures, ValidateOptionsResult result)
    {
        if (result.Failed)
            failures.AddRange(result.Failures);
    }
}
