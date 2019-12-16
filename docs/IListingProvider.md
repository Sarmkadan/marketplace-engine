# IListingProvider

`IListingProvider` defines the contract for external marketplace integrations that supply product listings into the marketplace engine. It abstracts the retrieval, synchronization, and availability tracking of listings from third-party sources such as dropship suppliers, affiliate networks, or external inventory systems. Implementations are expected to handle pagination, attribute mapping, and the conversion of external data formats into the domain’s canonical listing model.

## API

### Properties

- **`ExternalId`** `string`  
  A unique identifier assigned by the external system to this provider instance. Used for tracing, configuration lookup, and correlating listings back to their source.

- **`Title`** `string`  
  Human-readable display name for the provider, typically shown in administrative interfaces or logs.

- **`Description`** `string`  
  Free-text summary of the provider’s purpose, coverage, or integration details.

- **`Price`** `decimal`  
  The default or representative price for listings obtained through this provider. May serve as a fallback when individual listings lack explicit pricing.

- **`Currency`** `string`  
  ISO 4217 currency code (e.g., `"USD"`, `"EUR"`) denoting the currency in which `Price` and all listing prices from this provider are denominated.

- **`StockQuantity`** `int`  
  Aggregate or default stock quantity value. Can indicate total available units across all listings or act as a fallback when per-listing stock data is absent.

- **`Category`** `string`  
  Default category assignment for listings sourced from this provider. Individual listings may override this via `Attributes`.

- **`ImageUrls`** `List<string>`  
  Collection of URLs pointing to product images. These may represent provider-level branding or default images applied when a listing lacks its own images.

- **`Attributes`** `Dictionary<string, string>`  
  Arbitrary key-value metadata associated with the provider. Commonly used to store credentials, API endpoint fragments, or mapping rules required by the synchronization service.

- **`DropshipProviderClient`** *(type not fully qualified in signature)*  
  The underlying HTTP or SDK client used to communicate with the external provider’s API. Exposed to allow custom request configuration, authentication injection, or direct calls outside the standard listing flow.

- **`Items`** `List<ExternalListingDto>`  
  The current page of raw listings retrieved from the external source. Populated after a call to `GetListingsAsync` and cleared or replaced on subsequent paginated requests.

- **`Total`** `int`  
  Total number of listings available from the external source for the current query context. Used to calculate pagination boundaries.

- **`Page`** `int`  
  Zero- or one-based index of the current page represented by `Items`. The exact base is determined by the provider implementation.

- **`ExternalListingSyncService`** *(type not fully qualified in signature)*  
  Service responsible for orchestrating the transformation of `ExternalListingDto` objects into `Domain.Models.Listing` domain entities. Handles mapping, deduplication, and persistence concerns.

### Methods

- **`GetListingsAsync()`** → `Task<List<ExternalListingDto>>`  
  Retrieves a batch of listings from the external source. The returned list populates the `Items` property and updates `Total` and `Page`.  
  *Returns:* A `List<ExternalListingDto>` representing the current page of results.  
  *Throws:* `HttpRequestException` on network failure, `InvalidOperationException` if the provider is not properly configured, and provider-specific exceptions wrapped in a custom external service exception type when the remote API returns an error response.

- **`GetListingAsync(string externalListingId)`** → `Task<ExternalListingDto?>`  
  Fetches a single listing by its external identifier.  
  *Parameters:* `externalListingId` — the provider-specific ID of the listing to retrieve.  
  *Returns:* The matching `ExternalListingDto`, or `null` if no listing with that ID exists or the provider cannot resolve it.  
  *Throws:* Same network and configuration exceptions as `GetListingsAsync`. Does not throw when the listing is simply absent; returns `null` instead.

- **`IsListingAvailableAsync(string externalListingId)`** → `Task<bool>`  
  Checks whether a specific listing is currently in stock and purchasable from the external source.  
  *Parameters:* `externalListingId` — the provider-specific ID to query.  
  *Returns:* `true` if the listing is available; `false` if out of stock, discontinued, or not found.  
  *Throws:* Network and configuration exceptions. A `false` result does not imply an error condition.

- **`SyncListingsAsync()`** → `Task<List<Domain.Models.Listing>>`  
  Orchestrates a full synchronization cycle: retrieves listings via `GetListingsAsync`, transforms them through `ExternalListingSyncService`, and returns domain-model listings ready for ingestion into the marketplace catalog.  
  *Returns:* A list of `Domain.Models.Listing` entities. May be empty if no listings are available.  
  *Throws:* Aggregates exceptions from retrieval and transformation steps. Partial failures may result in an `AggregateException` containing individual per-listing errors.

- **`UpdateAvailabilityAsync()`** → `Task`  
  Refreshes the availability status of all listings currently tracked from this provider. Typically invoked on a schedule or in response to a webhook. Does not return a value; side effects include updated stock quantities and availability flags on the domain listings.  
  *Throws:* Network and configuration exceptions. Implementations may suppress per-listing failures and log them rather than throwing.

## Usage

### Example 1: Basic paginated retrieval and sync

```csharp
IListingProvider provider = providerFactory.Create("supplier-abc");
provider.Page = 0;

while (true)
{
    var dtos = await provider.GetListingsAsync();
    if (dtos.Count == 0) break;

    var domainListings = await provider.SyncListingsAsync();
    await catalogIngestionService.UpsertAsync(domainListings);

    if (provider.Page * pageSize >= provider.Total) break;
    provider.Page++;
}
```

### Example 2: Targeted availability check and single-listing fetch

```csharp
IListingProvider provider = providerRegistry.ResolveByExternalId("dropship-xyz");
string externalId = "SKU-98765";

bool available = await provider.IsListingAvailableAsync(externalId);
if (!available)
{
    await notificationService.SendOutOfStockAlertAsync(provider.Title, externalId);
    return;
}

ExternalListingDto? listing = await provider.GetListingAsync(externalId);
if (listing is not null)
{
    var enriched = await pricingService.ApplyMarginAsync(listing);
    await offerPublisher.PublishAsync(enriched);
}
```

## Notes

- **Pagination semantics:** The `Page` property’s starting index (0 or 1) is implementation-defined. Callers should inspect `Total` and `Items.Count` rather than assuming a fixed page size. Some providers may return `Total` as an estimate, making exact page-boundary calculations unreliable.
- **Null returns vs. exceptions:** `GetListingAsync` returns `null` for a non-existent listing rather than throwing. Callers must null-check the result. In contrast, network or authentication failures always throw, allowing retry policies to be applied at a higher level.
- **Thread safety:** `IListingProvider` implementations are not guaranteed to be thread-safe. Properties such as `Items`, `Total`, and `Page` are mutated by `GetListingsAsync` and should not be read concurrently with retrieval operations. Isolate provider instances per synchronization job or use external locking when shared across threads.
- **Partial failure during sync:** `SyncListingsAsync` may process some listings successfully while others fail. The resulting `AggregateException` (when thrown) contains individual exceptions keyed by external ID. Callers can choose to commit successful listings and retry failures independently.
- **Stale availability data:** `UpdateAvailabilityAsync` performs a real-time check against the external source. Between calls, `StockQuantity` and per-listing availability flags may drift. Do not rely on cached values for order-confirmation workflows without a fresh availability call.
- **`DropshipProviderClient` exposure:** Direct manipulation of the client (e.g., altering base address or timeout) affects all subsequent provider operations. Changes should be made during initialization and treated as immutable for the lifetime of a synchronization session.
