#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Validates the business rules of <see cref="PricingOptions"/> that data annotations cannot express:
/// the tax-rate and discount ranges, and the <see cref="MidpointRounding"/> member name.
/// Reports every invalid field in a single failure.
/// </summary>
public sealed class PricingOptionsValidator : IValidateOptions<PricingOptions>
{
    /// <summary>
    /// Smallest accepted <see cref="PricingOptions.DefaultTaxRate"/>.
    /// </summary>
    public const decimal MinTaxRate = 0m;

    /// <summary>
    /// Largest accepted <see cref="PricingOptions.DefaultTaxRate"/>.
    /// </summary>
    public const decimal MaxTaxRate = 1m;

    /// <summary>
    /// Smallest accepted <see cref="PricingOptions.MaxDiscountPercent"/>.
    /// </summary>
    public const decimal MinMaxDiscountPercent = 0m;

    /// <summary>
    /// Largest accepted <see cref="PricingOptions.MaxDiscountPercent"/>.
    /// </summary>
    public const decimal MaxMaxDiscountPercent = 100m;

    /// <summary>
    /// Validates the pricing options.
    /// </summary>
    /// <param name="name">The options instance name, if any; not used.</param>
    /// <param name="options">The options to validate.</param>
    /// <returns>Success when all rules hold; otherwise a failure listing each invalid field.</returns>
    public ValidateOptionsResult Validate(string? name, PricingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (options.DefaultTaxRate < MinTaxRate || options.DefaultTaxRate > MaxTaxRate)
            failures.Add($"{PricingOptions.SectionName}:DefaultTaxRate must be between {MinTaxRate} and {MaxTaxRate} (was {options.DefaultTaxRate}).");

        if (options.MaxDiscountPercent < MinMaxDiscountPercent || options.MaxDiscountPercent > MaxMaxDiscountPercent)
            failures.Add($"{PricingOptions.SectionName}:MaxDiscountPercent must be between {MinMaxDiscountPercent} and {MaxMaxDiscountPercent} (was {options.MaxDiscountPercent}).");

        if (!IsKnownRoundingMode(options.RoundingMode))
            failures.Add($"{PricingOptions.SectionName}:RoundingMode must be one of: {string.Join(", ", Enum.GetNames<MidpointRounding>())} (was '{options.RoundingMode}').");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    // Exact, case-sensitive match against the MidpointRounding member names. Numeric strings are rejected.
    private static bool IsKnownRoundingMode(string? roundingMode) =>
        roundingMode is not null && Enum.GetNames<MidpointRounding>().Contains(roundingMode, StringComparer.Ordinal);
}
