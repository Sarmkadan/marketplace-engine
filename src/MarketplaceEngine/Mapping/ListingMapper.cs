#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.DTOs;

namespace MarketplaceEngine.Mapping;

/// <summary>
/// Maps <see cref="Listing"/> domain models to <see cref="ListingDto"/> API responses.
/// </summary>
public static class ListingMapper
{
    /// <summary>
    /// Converts a single listing into its API representation.
    /// </summary>
    /// <param name="listing">The listing domain model.</param>
    /// <returns>The listing DTO.</returns>
    public static ListingDto ToDto(Listing listing) => new ListingDto(listing);

    /// <summary>
    /// Converts a sequence of listings into API representations, preserving order.
    /// </summary>
    /// <param name="listings">The listing domain models.</param>
    /// <returns>A materialized list of listing DTOs.</returns>
    public static List<ListingDto> ToDtoList(IEnumerable<Listing> listings) =>
        listings.Select(ToDto).ToList();
}
