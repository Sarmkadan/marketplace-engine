#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Configuration;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace MarketplaceEngine.Infrastructure.HealthChecks;

/// <summary>
/// Reports whether the listing store is reachable and how many listings are low on stock.
/// Low stock is informational: it is reported in the data but does not make the check unhealthy.
/// </summary>
public sealed class InventoryHealthCheck : IHealthCheck
{
    /// <summary>
    /// Data key that holds the number of low-stock listings found in the store.
    /// </summary>
    public const string LowStockCountKey = "lowStockCount";

    private readonly IListingRepository _listingRepository;
    private readonly InventoryTracker _inventoryTracker;
    private readonly IOptions<InventoryOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryHealthCheck"/> class.
    /// </summary>
    /// <param name="listingRepository">Repository used to read the listings held in the store.</param>
    /// <param name="inventoryTracker">Tracker that holds the stock levels for listings.</param>
    /// <param name="options">Inventory options that supply the low-stock threshold.</param>
    /// <exception cref="ArgumentNullException">A parameter is <see langword="null"/>.</exception>
    public InventoryHealthCheck(
        IListingRepository listingRepository,
        InventoryTracker inventoryTracker,
        IOptions<InventoryOptions> options)
    {
        _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
        _inventoryTracker = inventoryTracker ?? throw new ArgumentNullException(nameof(inventoryTracker));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Reads the listing store and counts the listings at or below the low-stock threshold.
    /// </summary>
    /// <returns>Healthy with the low-stock count, or Unhealthy when the store cannot be read.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var listings = await _listingRepository.GetAllAsync();
            var lowStockIds = _inventoryTracker
                .GetLowStockListingIds(_options.Value.LowStockThreshold)
                .ToHashSet();

            var lowStockCount = listings.Count(listing => lowStockIds.Contains(listing.Id));

            return HealthCheckResult.Healthy(
                "Inventory store is reachable.",
                data: new Dictionary<string, object> { [LowStockCountKey] = lowStockCount });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("Inventory store is not reachable.", ex);
        }
    }
}
