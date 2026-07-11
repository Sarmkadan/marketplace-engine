# WebhookEventExtensions

Provides typed access to the payload data of a webhook event. The static methods in this class attempt to extract and convert a named data field from the event’s data container, returning `null` when the field is absent or cannot be converted to the requested type. These methods are designed to be used as extension methods on a webhook event instance.

## API

### `GetData<T>`

```csharp
public static T? GetData<T>(this WebhookEvent event)
```

Attempts to retrieve the data payload of the webhook event and deserialize it to the specified type `T`.

- **Purpose**: Obtain the strongly‑typed data object embedded in the event.
- **Parameters**:
  - `event` – The webhook event instance. Must not be `null`.
- **Returns**: The deserialized data as `T`, or `null` if the event contains no data or the data cannot be deserialized to `T`.
- **Throws**:
  - `ArgumentNullException` if `event` is `null`.
  - `InvalidOperationException` if the underlying data source is in an inconsistent state (e.g., malformed JSON).

### `GetDataString`

```csharp
public static string? GetDataString(this WebhookEvent event)
```

Retrieves the data payload as a raw string.

- **Purpose**: Obtain the unprocessed string representation of the event data.
- **Parameters**:
  - `event` – The webhook event instance. Must not be `null`.
- **Returns**: The data as a `string`, or `null` if no data is present.
- **Throws**:
  - `ArgumentNullException` if `event` is `null`.

### `GetDataDecimal`

```csharp
public static decimal? GetDataDecimal(this WebhookEvent event)
```

Attempts to parse the data payload as a `decimal` value.

- **Purpose**: Extract a numeric decimal value from the event data.
- **Parameters**:
  - `event` – The webhook event instance. Must not be `null`.
- **Returns**: The parsed `decimal` value, or `null` if the data is missing or cannot be parsed as a decimal.
- **Throws**:
  - `ArgumentNullException` if `event` is `null`.
  - `FormatException` if the data string is present but is not a valid decimal representation.

### `GetDataBoolean`

```csharp
public static bool? GetDataBoolean(this WebhookEvent event)
```

Attempts to parse the data payload as a `bool` value.

- **Purpose**: Extract a boolean value from the event data.
- **Parameters**:
  - `event` – The webhook event instance. Must not be `null`.
- **Returns**: The parsed `bool` value, or `null` if the data is missing or cannot be parsed as a boolean.
- **Throws**:
  - `ArgumentNullException` if `event` is `null`.
  - `FormatException` if the data string is present but is not a valid boolean representation (e.g., not "true" or "false", case‑insensitive).

## Usage

### Example 1: Retrieving a typed object

```csharp
using MarketplaceEngine.Webhooks;

public class OrderCreatedData
{
    public string OrderId { get; set; }
    public decimal Total { get; set; }
}

public void HandleOrderCreated(WebhookEvent webhookEvent)
{
    var data = webhookEvent.GetData<OrderCreatedData>();
    if (data != null)
    {
        Console.WriteLine($"Order {data.OrderId} created with total {data.Total:C}");
    }
    else
    {
        Console.WriteLine("No order data found in event.");
    }
}
```

### Example 2: Extracting primitive values

```csharp
public void ProcessPaymentEvent(WebhookEvent webhookEvent)
{
    decimal? amount = webhookEvent.GetDataDecimal();
    bool? isRefund = webhookEvent.GetDataBoolean();

    if (amount.HasValue)
    {
        string action = isRefund == true ? "refunded" : "charged";
        Console.WriteLine($"Payment {action}: {amount.Value:F2}");
    }
    else
    {
        Console.WriteLine("Payment amount missing.");
    }
}
```

## Notes

- All methods throw `ArgumentNullException` when the `event` parameter is `null`. Always validate the event instance before calling these methods.
- `GetData<T>` relies on the underlying serialization mechanism (typically JSON). If the data payload is not valid JSON or does not match the structure of `T`, the method returns `null` rather than throwing a serialization exception. However, if the data source itself is corrupted, an `InvalidOperationException` may be thrown.
- `GetDataDecimal` and `GetDataBoolean` use the invariant culture for parsing. Decimal separators and boolean strings must follow standard .NET conventions (e.g., `"123.45"` for decimal, `"true"`/`"false"` for boolean). Unexpected formats cause a `FormatException`.
- All methods are static and do not maintain any internal state. They are thread‑safe and can be called concurrently on different `WebhookEvent` instances without synchronization.
