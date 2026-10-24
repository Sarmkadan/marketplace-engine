#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// A single recorded price for a listing at a point in time.
/// </summary>
public class PriceHistoryEntry
{
    /// <summary>The listing this price belongs to.</summary>
    public Guid ListingId { get; set; }

    /// <summary>The price recorded for the listing.</summary>
    public decimal Price { get; set; }

    /// <summary>When the price was recorded, in UTC.</summary>
    public DateTime ChangedAtUtc { get; set; }
}
