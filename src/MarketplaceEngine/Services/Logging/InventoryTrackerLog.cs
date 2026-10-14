#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace MarketplaceEngine.Services;

/// <summary>
/// Compile-time generated log messages for <see cref="InventoryTracker"/>.
/// </summary>
internal static partial class InventoryTrackerLog
{
    [LoggerMessage(EventId = 2000, Level = LogLevel.Information,
        Message = "Stock set for {ListingId}: {Quantity}")]
    public static partial void StockSet(ILogger logger, Guid listingId, int quantity);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Warning,
        Message = "Listing {ListingId} deactivated: out of stock")]
    public static partial void ListingDeactivatedOutOfStock(ILogger logger, Guid listingId);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning,
        Message = "Stock reservation failed for {ListingId}: requested {RequestedQuantity}, available {AvailableQuantity}")]
    public static partial void StockReservationFailed(ILogger logger, Guid listingId, int requestedQuantity, int availableQuantity);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Information,
        Message = "Stock reserved: {ReservedQuantity} units for {ListingId}")]
    public static partial void StockReserved(ILogger logger, int reservedQuantity, Guid listingId);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Information,
        Message = "Stock committed: {CommittedQuantity} units for {ListingId}, remaining: {RemainingQuantity}")]
    public static partial void StockCommitted(ILogger logger, int committedQuantity, Guid listingId, int remainingQuantity);

    [LoggerMessage(EventId = 2005, Level = LogLevel.Information,
        Message = "Stock released: {ReleasedQuantity} reserved units for {ListingId}")]
    public static partial void StockReleased(ILogger logger, int releasedQuantity, Guid listingId);
}
