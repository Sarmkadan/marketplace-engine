# PricePoint

A `PricePoint` represents a recorded price for an item at a specific point in time, typically used for tracking price history and calculating price changes in a marketplace context. It is part of a system that monitors price fluctuations, computes statistics, and determines whether a price drop has occurred.

## API

### `public sealed record PricePoint`

The primary record type representing a single price point in time.

### `public sealed record PriceStatistics`

A record type containing aggregated price statistics derived from a collection of `PricePoint` records.

### `public bool RecordPrice`

Gets a value indicating whether this price point should be recorded in the price history. This may be `false` if the price is invalid, unchanged, or otherwise ineligible for historical tracking.

### `public IReadOnlyList<PricePoint> GetHistory()`

Retrieves the complete historical record of price points for the associated item.

- **Returns**: An immutable list of `PricePoint` instances ordered chronologically, representing all recorded prices.
- **Throws**: `InvalidOperationException` if the price history cannot be accessed (e.g., due to a corrupted or missing data source).

### `public PriceStatistics? GetStatistics()`

Computes and returns aggregated price statistics based on the recorded history.

- **Returns**: A `PriceStatistics` object containing metrics such as average price, minimum, maximum, and standard deviation. Returns `null` if there is insufficient data (e.g., fewer than two price points).
- **Throws**: `InvalidOperationException` if the underlying data store is unavailable or corrupted.

### `public decimal GetLatestDropPercent()`

Calculates the percentage drop from the previous price point to the most recent one.

- **Returns**: The percentage decrease from the prior price to the current price. Returns `0m` if there is no prior price or if the price has increased.
- **Throws**: `InvalidOperationException` if no historical data exists to compute the drop.

### `public bool HasPriceDrop`

Determines whether the current price point represents a drop compared to the previous recorded price.

- **Returns**: `true` if the current price is lower than the immediately preceding price; otherwise, `false`.
- **Throws**: `InvalidOperationException` if no prior price exists to compare against.

### `public int Prune(int maxHistory)`

Reduces the stored price history to the most recent `maxHistory` entries.

- **Parameters**:
  - `maxHistory`: The maximum number of most recent price points to retain.
- **Returns**: The number of entries removed from the history.
- **Throws**: `ArgumentOutOfRangeException` if `maxHistory` is negative. Throws `InvalidOperationException` if the pruning operation fails due to data access issues.

## Usage

### Example 1: Tracking Price History and Detecting Drops
