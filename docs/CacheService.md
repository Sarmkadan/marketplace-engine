# CacheService

Provides a thread-safe, in-memory cache with support for generic typed entries, automatic expiration, and runtime statistics. Designed for use in the `marketplace-engine` project, it allows storing and retrieving objects by key, clearing the entire cache, and monitoring memory usage and item age.

## API

### `public CacheService()`
Initializes a new instance of the `CacheService` class with an empty cache.

### `public async Task<T?> GetAsync<T>(string key)`
Retrieves the cached value associated with the specified key.  
- **Type parameter `T`**: The expected type of the cached value.  
- **Parameter `key`**: The unique identifier of the cache entry.  
- **Returns**: The cached value cast to `T`, or `default(T)` if the key does not exist or the entry has expired.  
- **Throws**: `ArgumentNullException` if `key` is `null`.

### `public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)`
Stores a value in the cache under the given key.  
- **Type parameter `T`**: The type of the value being stored.  
- **Parameter `key`**: The unique identifier for the entry.  
- **Parameter `value`**: The object to cache.  
- **Parameter `expiration`**: Optional absolute or sliding expiration time. If `null`, the entry never expires.  
- **Throws**: `ArgumentNullException` if `key` is `null`.

### `public async Task RemoveAsync(string key)`
Removes the cache entry with the specified key.  
- **Parameter `key`**: The key of the entry to remove.  
- **Throws**: `ArgumentNullException` if `key` is `null`.

### `public async Task ClearAsync()`
Removes all entries from the cache.

### `public async Task<CacheStatistics> GetStatisticsAsync()`
Returns a snapshot of current cache statistics.  
- **Returns**: A `CacheStatistics` object containing the properties described below.

### `public string Key`
Gets the key of the most recently added or retrieved cache entry.  
- **Value**: The key string, or `null` if no operation has been performed.

### `public object? Value`
Gets the value of the most recently added or retrieved cache entry.  
- **Value**: The cached object, or `null` if no entry exists.

### `public DateTime ExpiresAt`
Gets the absolute expiration time of the most recently added or retrieved cache entry.  
- **Value**: A `DateTime` in UTC. If the entry never expires, returns `DateTime.MaxValue`.

### `public DateTime CreatedAt`
Gets the creation time (UTC) of the most recently added or retrieved cache entry.

### `public int TotalItems`
Gets the total number of entries currently in the cache (including expired but not yet purged items).

### `public long TotalMemoryMb`
Gets an estimate of the total memory consumed by cached entries, in megabytes.

### `public TimeSpan OldestItemAge`
Gets the age of the oldest non-expired entry in the cache.

## Usage

### Basic Get and Set

```csharp
using marketplace_engine;

var cache = new CacheService();

// Store a product object with a 5-minute expiration
var product = new Product { Id = 42, Name = "Widget" };
await cache.SetAsync("product:42", product, TimeSpan.FromMinutes(5));

// Retrieve it later
var cached = await cache.GetAsync<Product>("product:42");
if (cached != null)
{
    Console.WriteLine($"Cached product: {cached.Name}");
}

// Remove the entry
await cache.RemoveAsync("product:42");
```

### Monitoring Cache Statistics

```csharp
var cache = new CacheService();

// Populate cache with sample data
for (int i = 0; i < 100; i++)
{
    await cache.SetAsync($"key:{i}", i, TimeSpan.FromHours(1));
}

// Retrieve statistics
var stats = await cache.GetStatisticsAsync();
Console.WriteLine($"Total items: {stats.TotalItems}");
Console.WriteLine($"Memory used: {stats.TotalMemoryMb} MB");
Console.WriteLine($"Oldest item age: {stats.OldestItemAge}");

// Clear the cache
await cache.ClearAsync();
```

## Notes

- **Thread safety**: All public methods are thread-safe. Concurrent calls to `GetAsync`, `SetAsync`, `RemoveAsync`, and `ClearAsync` are supported without additional synchronization.
- **Expiration**: Entries are lazily evicted. An expired entry may still be returned by `GetAsync` until the next background cleanup or until it is accessed again. The `ExpiresAt` property reflects the expiration time of the most recently touched entry.
- **Null keys**: All methods that accept a `key` parameter throw `ArgumentNullException` if the key is `null`. Empty strings are allowed.
- **Memory estimation**: `TotalMemoryMb` is an approximation based on the size of stored objects and internal overhead. It does not account for memory fragmentation or the garbage collector’s state.
- **Statistics properties**: `TotalItems`, `TotalMemoryMb`, and `OldestItemAge` are updated asynchronously and may not reflect the very latest state if concurrent modifications occur. They are intended for monitoring and diagnostics, not for precise transactional decisions.
