#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Root of all marketplace settings, bound from the <c>Marketplace</c> configuration section.
/// Each property maps to a subsection: <c>Marketplace:Pricing</c>, <c>Marketplace:PaymentGateway</c>
/// and <c>Marketplace:Inventory</c>. The whole tree is validated once at startup by
/// <see cref="MarketplaceOptionsValidator"/>, so every invalid key is reported together.
/// </summary>
public class MarketplaceOptions
{
    /// <summary>
    /// Name of the root configuration section.
    /// </summary>
    public const string SectionName = "Marketplace";

    /// <summary>
    /// Pricing settings, from the <c>Marketplace:Pricing</c> subsection.
    /// </summary>
    public PricingOptions Pricing { get; set; } = new();

    /// <summary>
    /// Payment gateway settings, from the <c>Marketplace:PaymentGateway</c> subsection.
    /// </summary>
    public PaymentGatewayOptions PaymentGateway { get; set; } = new();

    /// <summary>
    /// Inventory settings, from the <c>Marketplace:Inventory</c> subsection.
    /// </summary>
    public InventoryOptions Inventory { get; set; } = new();
}
