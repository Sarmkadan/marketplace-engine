#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.ValueObjects;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Provides common status and display helpers for <see cref="Listing"/> instances.
/// </summary>
public static class ListingExtensions
{
    /// <summary>
    /// Determines whether the listing is currently active in the marketplace.
    /// </summary>
    /// <param name="listing">The listing to evaluate.</param>
    /// <returns><see langword="true"/> when the listing has an active status; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="listing"/> is <see langword="null"/>.</exception>
    public static bool IsActive(this Listing listing)
    {
        ArgumentNullException.ThrowIfNull(listing);
        return listing.Status == ListingStatus.Active;
    }

    /// <summary>
    /// Gets a display title containing the listing title and, when available, its formatted price.
    /// </summary>
    /// <param name="listing">The listing to format.</param>
    /// <returns>The listing title, followed by its price when one is available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="listing"/> is <see langword="null"/>.</exception>
    public static string GetDisplayTitle(this Listing listing)
    {
        ArgumentNullException.ThrowIfNull(listing);

        return listing.Price is null
            ? listing.Title
            : $"{listing.Title} - {listing.Price.ToStringWithSymbol()}";
    }
}
