#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace MarketplaceEngine.Services;

/// <summary>
/// Compile-time generated log messages for <see cref="ProductCatalog"/>.
/// </summary>
internal static partial class ProductCatalogLog
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Debug,
        Message = "Catalog browse returned {Count} listings (category: {CategoryId})")]
    public static partial void BrowseCompleted(ILogger logger, int count, string categoryId);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Debug,
        Message = "Catalog search for '{SearchTerm}' returned {Count} results")]
    public static partial void SearchCompleted(ILogger logger, string searchTerm, int count);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Debug,
        Message = "Product detail viewed: {ListingId} (views: {ViewCount})")]
    public static partial void ProductDetailViewed(ILogger logger, Guid listingId, int viewCount);
}
