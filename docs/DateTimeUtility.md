# DateTimeUtility

Provides a centralized set of date and time operations for the marketplace engine, ensuring consistent UTC handling, formatting, and temporal comparisons across the codebase. All methods operate in UTC unless explicitly noted, avoiding local-timezone ambiguity in distributed scenarios.

## API

### GetCurrentUtcTime
```csharp
public static DateTime GetCurrentUtcTime { get; }
```
Returns the current UTC date and time. Wraps `DateTime.UtcNow` to allow test-time substitution via ambient context or shim.

**Returns**: Current UTC `DateTime` with `Kind == DateTimeKind.Utc`.

---

### ToUtc
```csharp
public static DateTime ToUtc(DateTime dateTime)
```
Converts the supplied `DateTime` to UTC.

**Parameters**:
- `dateTime`: The value to convert. If `Kind` is `Unspecified`, it is treated as UTC. If `Kind` is `Local`, it is converted to UTC. If `Kind` is `Utc`, it is returned unchanged.

**Returns**: A `DateTime` with `Kind == DateTimeKind.Utc` representing the same instant.

**Throws**: `ArgumentOutOfRangeException` if conversion overflows `DateTime` range.

---

### GetElapsedTime
```csharp
public static TimeSpan GetElapsedTime(DateTime start, DateTime end)
```
Calculates the elapsed duration between two UTC timestamps.

**Parameters**:
- `start`: Inclusive start instant (UTC).
- `end`: Exclusive end instant (UTC).

**Returns**: `end - start` as a `TimeSpan`. Negative if `end` precedes `start`.

**Throws**: `ArgumentException` if either argument is not UTC (`Kind != DateTimeKind.Utc`).

---

### GetElapsedTimeString
```csharp
public static string GetElapsedTimeString(TimeSpan elapsed)
```
Formats a `TimeSpan` into a human-readable string (e.g., "2d 3h 15m 7s"). Omits zero-value units.

**Parameters**:
- `elapsed`: Non-negative duration to format.

**Returns**: Compact string representation.

**Throws**: `ArgumentOutOfRangeException` if `elapsed < TimeSpan.Zero`.

---

### IsWithinDays
```csharp
public static bool IsWithinDays(DateTime reference, DateTime target, int days)
```
Determines whether `target` falls within `days` calendar days of `reference` (inclusive).

**Parameters**:
- `reference`: Baseline instant (UTC).
- `target`: Instant to test (UTC).
- `days`: Non-negative day threshold.

**Returns**: `true` if `|target.Date - reference.Date| <= days`; otherwise `false`.

**Throws**: `ArgumentException` if either `DateTime` is not UTC. `ArgumentOutOfRangeException` if `days < 0`.

---

### IsWithinHours
```csharp
public static bool IsWithinHours(DateTime reference, DateTime target, int hours)
```
Determines whether `target` falls within `hours` of `reference` (inclusive).

**Parameters**:
- `reference`: Baseline instant (UTC).
- `target`: Instant to test (UTC).
- `hours`: Non-negative hour threshold.

**Returns**: `true` if `|target - reference| <= TimeSpan.FromHours(hours)`; otherwise `false`.

**Throws**: `ArgumentException` if either `DateTime` is not UTC. `ArgumentOutOfRangeException` if `hours < 0`.

---

### GetDayStart
```csharp
public static DateTime GetDayStart(DateTime dateTime)
```
Returns the start of the day (00:00:00.000) for the given UTC date.

**Parameters**:
- `dateTime`: Any UTC `DateTime`.

**Returns**: UTC `DateTime` at midnight of the same calendar day, `Kind == Utc`.

**Throws**: `ArgumentException` if `dateTime.Kind != DateTimeKind.Utc`.

---

### GetDayEnd
```csharp
public static DateTime GetDayEnd(DateTime dateTime)
```
Returns the end of the day (23:59:59.999) for the given UTC date.

**Parameters**:
- `dateTime`: Any UTC `DateTime`.

**Returns**: UTC `DateTime` at the last millisecond of the same calendar day, `Kind == Utc`.

**Throws**: `ArgumentException` if `dateTime.Kind != DateTimeKind.Utc`.

---

### GetWeekStart
```csharp
public static DateTime GetWeekStart(DateTime dateTime)
```
Returns the start of the week (Monday 00:00:00.000 UTC) containing the given date, per ISO 8601.

**Parameters**:
- `dateTime`: Any UTC `DateTime`.

**Returns**: UTC `DateTime` representing the Monday of that week, `Kind == Utc`.

**Throws**: `ArgumentException` if `dateTime.Kind != DateTimeKind.Utc`.

---

### GetMonthStart
```csharp
public static DateTime GetMonthStart(DateTime dateTime)
```
Returns the first day of the month (00:00:00.000 UTC) for the given date.

**Parameters**:
- `dateTime`: Any UTC `DateTime`.

**Returns**: UTC `DateTime` at midnight on day 1 of the same month, `Kind == Utc`.

**Throws**: `ArgumentException` if `dateTime.Kind != DateTimeKind.Utc`.

---

### IsSameDay
```csharp
public static bool IsSameDay(DateTime first, DateTime second)
```
Checks whether two UTC timestamps fall on the same calendar day.

**Parameters**:
- `first`: First UTC `DateTime`.
- `second`: Second UTC `DateTime`.

**Returns**: `true` if `first.Date == second.Date`; otherwise `false`.

**Throws**: `ArgumentException` if either `DateTime` is not UTC.

---

### ToIso8601String
```csharp
public static string ToIso8601String(DateTime dateTime)
```
Formats a UTC `DateTime` as an ISO 8601 string with UTC designator (`"yyyy-MM-ddTHH:mm:ss.fffZ"`).

**Parameters**:
- `dateTime`: UTC `DateTime` to format.

**Returns**: ISO 8601 string ending in `Z`.

**Throws**: `ArgumentException` if `dateTime.Kind != DateTimeKind.Utc`.

---

### GetFutureTime
```csharp
public static DateTime GetFutureTime(TimeSpan offset)
```
Returns a UTC timestamp offset from the current UTC time.

**Parameters**:
- `offset`: Non-negative `TimeSpan` to add to `GetCurrentUtcTime`.

**Returns**: `GetCurrentUtcTime + offset` as UTC `DateTime`.

**Throws**: `ArgumentOutOfRangeException` if `offset < TimeSpan.Zero` or result exceeds `DateTime.MaxValue`.

## Usage

```csharp
// Example 1: Check if an order was placed within the last 30 days
var orderPlacedAt = DateTimeUtility.ToUtc(order.CreatedAt); // ensure UTC
var now = DateTimeUtility.GetCurrentUtcTime;
if (DateTimeUtility.IsWithinDays(now, orderPlacedAt, 30))
{
    var elapsed = DateTimeUtility.GetElapsedTime(orderPlacedAt, now);
    logger.LogInformation("Order {OrderId} placed {Elapsed} ago", order.Id,
        DateTimeUtility.GetElapsedTimeString(elapsed));
}
```

```csharp
// Example 2: Generate a weekly report window and schedule a future reminder
var weekStart = DateTimeUtility.GetWeekStart(DateTimeUtility.GetCurrentUtcTime);
var weekEnd = weekStart.AddDays(7).AddTicks(-1);
var reportPeriod = $"{DateTimeUtility.ToIso8601String(weekStart)} / {DateTimeUtility.ToIso8601String(weekEnd)}";

var reminderAt = DateTimeUtility.GetFutureTime(TimeSpan.FromDays(1));
scheduler.Enqueue(() => SendWeeklyReport(reportPeriod), reminderAt);
```

## Notes

- **UTC enforcement**: Every method that accepts `DateTime` parameters validates `Kind == Utc` and throws `ArgumentException` otherwise. Callers must normalize inputs via `ToUtc` or ensure upstream sources provide UTC.
- **Thread safety**: All members are pure static functions with no shared mutable state; they are inherently thread-safe.
- **Precision**: `GetDayEnd` uses millisecond resolution (23:59:59.999). For inclusive range queries against data stores with higher precision, consider using `GetDayStart(nextDay)` as an exclusive upper bound.
- **Week definition**: `GetWeekStart` follows ISO 8601 (Monday-based). Adjust if business weeks differ.
- **Time provider**: `GetCurrentUtcTime` reads the system clock directly. In tests, wrap or abstract via an `IDateTimeProvider` to control time.
- **Overflow**: `GetFutureTime` and `ToUtc` propagate `ArgumentOutOfRangeException` on overflow; callers near `DateTime.MaxValue` should guard accordingly.
