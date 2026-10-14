#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Strongly-typed pricing settings, bound from the <c>Marketplace:Pricing</c> configuration section.
/// Validated at startup by the data annotations on this class and by <see cref="PricingOptionsValidator"/>.
/// </summary>
public class PricingOptions
{
    /// <summary>
    /// Name of the configuration section these options are bound from.
    /// </summary>
    public const string SectionName = MarketplaceOptions.SectionName + ":Pricing";

    /// <summary>
    /// Default name of the <see cref="MidpointRounding"/> mode used for monetary rounding.
    /// </summary>
    public const string DefaultRoundingMode = nameof(MidpointRounding.AwayFromZero);

    /// <summary>
    /// Default maximum discount, in percent.
    /// </summary>
    public const decimal DefaultMaxDiscountPercent = 50m;

    /// <summary>
    /// Default ISO-4217 currency code.
    /// </summary>
    public const string DefaultCurrencyCode = "USD";

    /// <summary>
    /// Default sales tax rate as a decimal fraction. Must be between 0 and 1.
    /// </summary>
    public decimal DefaultTaxRate { get; set; } = 0m;

    /// <summary>
    /// Name of a <see cref="MidpointRounding"/> member (for example <c>AwayFromZero</c> or <c>ToEven</c>).
    /// Case-sensitive. Required.
    /// </summary>
    [Required(ErrorMessage = SectionName + ":RoundingMode is required.")]
    public string RoundingMode { get; set; } = DefaultRoundingMode;

    /// <summary>
    /// Maximum discount that <see cref="PricingEngine"/> accepts, in percent. Must be between 0 and 100.
    /// </summary>
    public decimal MaxDiscountPercent { get; set; } = DefaultMaxDiscountPercent;

    /// <summary>
    /// Three-letter uppercase ISO-4217 currency code used for totals. Required.
    /// </summary>
    [Required(ErrorMessage = SectionName + ":Currency is required.")]
    [RegularExpression("^[A-Z]{3}$", ErrorMessage = SectionName + ":Currency must be a 3-letter uppercase ISO-4217 code.")]
    public string Currency { get; set; } = DefaultCurrencyCode;
}
