#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MarketplaceEngine.Services;

/// <summary>
/// Tracks stock levels for marketplace listings, handling reservations
/// and automatic deactivation when inventory runs out.
/// </summary>
public class InventoryTracker : IDisposable
{
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<InventoryTracker> _logger;
    private readonly ConcurrentDictionary<Guid, int> _stockLevels = new();
    private readonly ConcurrentDictionary<Guid, int> _reservations = new();
    private readonly SemaphoreSlim _inventoryLock = new(1, 1);
    private bool _disposed;

    public InventoryTracker(IListingRepository listingRepository)
        : this(listingRepository, NullLogger<InventoryTracker>.Instance)
    {
    }

    public InventoryTracker(
        IListingRepository listingRepository,
        ILogger<InventoryTracker> logger)
    {
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sets the stock level for a listing. If zero, the listing is deactivated.
    /// </summary>
    /// <param name="listingId">The listing to update.</param>
    /// <param name="quantity">Available stock quantity.</param>
    public async Task SetStockAsync(Guid listingId, int quantity)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Stock cannot be negative.");

        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing is null)
            throw new MarketplaceException($"Listing {listingId} not found.");

        _stockLevels[listingId] = quantity;

        if (quantity == 0 && listing.Status == ListingStatus.Active)
        {
            listing.Status = ListingStatus.Inactive;
            await _listingRepository.UpdateAsync(listing);
            InventoryTrackerLog.ListingDeactivatedOutOfStock(_logger, listingId);
        }
        else
        {
            InventoryTrackerLog.StockSet(_logger, listingId, quantity);
        }
    }

    /// <summary>
    /// Returns available stock (total minus reservations) for a listing.
    /// </summary>
    /// <param name="listingId">The listing to check.</param>
    /// <returns>Available (unreserved) quantity.</returns>
    public int GetAvailableStock(Guid listingId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var total = _stockLevels.GetValueOrDefault(listingId, 0);
        var reserved = _reservations.GetValueOrDefault(listingId, 0);
        return Math.Max(0, total - reserved);
    }

    /// <summary>
    /// Returns the IDs of tracked listings whose available stock is at or below the threshold.
    /// Only listings with a stock level set through <see cref="SetStockAsync"/> are considered.
    /// </summary>
    /// <param name="threshold">Available stock level at or below which a listing counts as low.</param>
    /// <returns>The IDs of low-stock tracked listings.</returns>
    public IReadOnlyList<Guid> GetLowStockListingIds(int threshold)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _stockLevels.Keys
            .Where(listingId => GetAvailableStock(listingId) <= threshold)
            .ToList();
    }

    /// <summary>
    /// Reserves stock for a pending order. Fails if insufficient inventory.
    /// </summary>
    /// <param name="listingId">The listing to reserve stock for.</param>
    /// <param name="quantity">Units to reserve.</param>
    /// <returns>True if reservation succeeded, false if insufficient stock.</returns>
    public async Task<bool> ReserveStockAsync(Guid listingId, int quantity)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Reservation quantity must be positive.");

        await _inventoryLock.WaitAsync();
        try
        {
            var available = GetAvailableStock(listingId);
            if (available < quantity)
            {
                InventoryTrackerLog.StockReservationFailed(_logger, listingId, quantity, available);
                return false;
            }

            _reservations.AddOrUpdate(listingId, quantity, (_, existing) => existing + quantity);
            InventoryTrackerLog.StockReserved(_logger, quantity, listingId);
            return true;
        }
        finally
        {
            _inventoryLock.Release();
        }
    }

    /// <summary>
    /// Commits a reservation, reducing actual stock. Called after order confirmation.
    /// </summary>
    /// <param name="listingId">The listing whose reservation to commit.</param>
    /// <param name="quantity">Units to deduct from stock.</param>
    public async Task CommitReservationAsync(Guid listingId, int quantity)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Commit quantity must be positive.");

        await _inventoryLock.WaitAsync();
        try
        {
            var reserved = _reservations.GetValueOrDefault(listingId, 0);
            if (reserved < quantity)
                throw new MarketplaceException($"Cannot commit {quantity} units: only {reserved} reserved for {listingId}.");

            _reservations.AddOrUpdate(listingId, 0, (_, existing) => existing - quantity);
            _stockLevels.AddOrUpdate(listingId, 0, (_, existing) => Math.Max(0, existing - quantity));

            var remaining = GetAvailableStock(listingId);
            InventoryTrackerLog.StockCommitted(_logger, quantity, listingId, remaining);

            if (_stockLevels.GetValueOrDefault(listingId, 0) == 0)
            {
                await SetStockAsync(listingId, 0);
            }
        }
        finally
        {
            _inventoryLock.Release();
        }
    }

    /// <summary>
    /// Releases a reservation, making the stock available again. Called on order cancellation.
    /// </summary>
    /// <param name="listingId">The listing whose reservation to release.</param>
    /// <param name="quantity">Units to release.</param>
    public void ReleaseReservation(Guid listingId, int quantity)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Release quantity must be positive.");

        _reservations.AddOrUpdate(listingId, 0, (_, existing) => Math.Max(0, existing - quantity));
        InventoryTrackerLog.StockReleased(_logger, quantity, listingId);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;
        _inventoryLock.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
