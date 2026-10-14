#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Configuration;

/// <summary>
/// Strongly-typed inventory settings, bound from the <c>Marketplace:Inventory</c> configuration section.
/// Validated at startup by <see cref="InventoryOptionsValidator"/> as part of <see cref="MarketplaceOptions"/>.
/// </summary>
public class InventoryOptions
{
    /// <summary>
    /// Name of the configuration section these options are bound from.
    /// </summary>
    public const string SectionName = MarketplaceOptions.SectionName + ":Inventory";

    /// <summary>
    /// Default stock level at or below which a listing is reported as low on stock.
    /// </summary>
    public const int DefaultLowStockThreshold = 5;

    /// <summary>
    /// Default number of minutes an inventory reservation is held before it expires.
    /// </summary>
    public const int DefaultReservationTtlMinutes = 15;

    /// <summary>
    /// Stock level at or below which a listing counts as low on stock. Must be zero or greater.
    /// </summary>
    public int LowStockThreshold { get; set; } = DefaultLowStockThreshold;

    /// <summary>
    /// Lifetime of an inventory reservation, in minutes. Must be between 1 and 1440 (one day).
    /// </summary>
    public int ReservationTtlMinutes { get; set; } = DefaultReservationTtlMinutes;
}
