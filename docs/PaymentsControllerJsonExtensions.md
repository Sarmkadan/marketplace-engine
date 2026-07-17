# PaymentsControllerJsonExtensions

Provides static extension methods for serializing and deserializing payment-related DTOs to and from JSON strings. These methods are used by the `PaymentsController` to convert between HTTP request/response payloads and strongly-typed domain objects, centralizing JSON handling for the payment workflow.

## API

### `ToJson`

```csharp
public static string ToJson(this object value)
```

Serializes the specified object to its JSON string representation.

- **Parameters**  
  `value` – The object to serialize. Must not be `null`.
- **Returns**  
  A JSON string representing the object.
- **Throws**  
  `ArgumentNullException` if `value` is `null`.  
  `JsonException` if serialization fails (e.g., circular references or unsupported types).

### `FromJsonToPaymentDto`

```csharp
public static PaymentDto? FromJsonToPaymentDto(this string json)
```

Deserializes a JSON string into a `PaymentDto` instance.

- **Parameters**  
  `json` – The JSON string to deserialize. May be `null` or empty.
- **Returns**  
  A `PaymentDto` if deserialization succeeds; otherwise `null`.
- **Throws**  
  `JsonException` if the JSON is malformed or cannot be mapped to `PaymentDto`.

### `FromJsonToInitiatePaymentRequest`

```csharp
public static InitiatePaymentRequest? FromJsonToInitiatePaymentRequest(this string json)
```

Deserializes a JSON string into an `InitiatePaymentRequest` instance.

- **Parameters**  
  `json` – The JSON string to deserialize. May be `null` or empty.
- **Returns**  
  An `InitiatePaymentRequest` if deserialization succeeds; otherwise `null`.
- **Throws**  
  `JsonException` if the JSON is malformed or cannot be mapped to `InitiatePaymentRequest`.

### `FromJsonToCompletePaymentRequest`

```csharp
public static CompletePaymentRequest? FromJsonToCompletePaymentRequest(this string json)
```

Deserializes a JSON string into a `CompletePaymentRequest` instance.

- **Parameters**  
  `json` – The JSON string to deserialize. May be `null` or empty.
- **Returns**  
  A `CompletePaymentRequest` if deserialization succeeds; otherwise `null`.
- **Throws**  
  `JsonException` if the JSON is malformed or cannot be mapped to `CompletePaymentRequest`.

### `FromJsonToRefundPaymentRequest`

```csharp
public static RefundPaymentRequest? FromJsonToRefundPaymentRequest(this string json)
```

Deserializes a JSON string into a `RefundPaymentRequest` instance.

- **Parameters**  
  `json` – The JSON string to deserialize. May be `null` or empty.
- **Returns**  
  A `RefundPaymentRequest` if deserialization succeeds; otherwise `null`.
- **Throws**  
  `JsonException` if the JSON is malformed or cannot be mapped to `RefundPaymentRequest`.

### `TryFromJson` (PaymentDto)

```csharp
public static bool TryFromJson(this string json, out PaymentDto? result)
```

Attempts to deserialize a JSON string into a `PaymentDto` without throwing exceptions.

- **Parameters**  
  `json` – The JSON string to deserialize.  
  `result` – When this method returns, contains the deserialized `PaymentDto` if successful, or `null` if deserialization failed.
- **Returns**  
  `true` if deserialization succeeded; `false` otherwise (including `null` or malformed JSON).

### `TryFromJson` (InitiatePaymentRequest)

```csharp
public static bool TryFromJson(this string json, out InitiatePaymentRequest? result)
```

Attempts to deserialize a JSON string into an `InitiatePaymentRequest` without throwing exceptions.

- **Parameters**  
  `json` – The JSON string to deserialize.  
  `result` – When this method returns, contains the deserialized `InitiatePaymentRequest` if successful, or `null` if deserialization failed.
- **Returns**  
  `true` if deserialization succeeded; `false` otherwise.

### `TryFromJson` (CompletePaymentRequest)

```csharp
public static bool TryFromJson(this string json, out CompletePaymentRequest? result)
```

Attempts to deserialize a JSON string into a `CompletePaymentRequest` without throwing exceptions.

- **Parameters**  
  `json` – The JSON string to deserialize.  
  `result` – When this method returns, contains the deserialized `CompletePaymentRequest` if successful, or `null` if deserialization failed.
- **Returns**  
  `true` if deserialization succeeded; `false` otherwise.

### `TryFromJson` (RefundPaymentRequest)

```csharp
public static bool TryFromJson(this string json, out RefundPaymentRequest? result)
```

Attempts to deserialize a JSON string into a `RefundPaymentRequest` without throwing exceptions.

- **Parameters**  
  `json` – The JSON string to deserialize.  
  `result` – When this method returns, contains the deserialized `RefundPaymentRequest` if successful, or `null` if deserialization failed.
- **Returns**  
  `true` if deserialization succeeded; `false` otherwise.

## Usage

### Example 1: Serializing a request and deserializing with error handling

```csharp
using MarketplaceEngine.Payments;

var initiateRequest = new InitiatePaymentRequest
{
    Amount = 49.99m,
    Currency = "USD",
    PaymentMethod = "card"
};

// Serialize to JSON
string json = initiateRequest.ToJson();

// Deserialize back using TryFromJson to avoid exceptions
if (json.TryFromJson(out InitiatePaymentRequest? restored))
{
    Console.WriteLine($"Deserialized amount: {restored.Amount}");
}
else
{
    Console.WriteLine("Failed to deserialize initiate payment request.");
}
```

### Example 2: Handling malformed JSON with the nullable FromJson methods

```csharp
string invalidJson = "{ \"invalid\": true }";

// FromJsonToPaymentDto returns null on failure (but may throw on malformed JSON)
PaymentDto? dto = invalidJson.FromJsonToPaymentDto();
if (dto == null)
{
    // Use TryFromJson for safe parsing
    if (invalidJson.TryFromJson(out PaymentDto? safeDto))
    {
        // Process safeDto
    }
    else
    {
        Console.WriteLine("JSON could not be parsed as PaymentDto.");
    }
}
```

## Notes

- All methods are **static** and operate only on their input parameters; they do not modify any shared state. Therefore, they are **thread-safe** and can be called concurrently from multiple threads.
- The `FromJsonTo*` methods throw a `JsonException` when the input is syntactically invalid JSON or when the JSON structure does not match the target type. For scenarios where exceptions are undesirable, use the corresponding `TryFromJson` overload.
- Passing `null` or an empty string to any `FromJsonTo*` method will return `null` (no exception). The `TryFromJson` methods will return `false` and set the `out` parameter to `null` for `null` or empty input.
- The serialization behavior (e.g., property naming policy, handling of missing fields) follows the default conventions of the underlying JSON serializer used by the `marketplace-engine` project. No custom converters or attributes are exposed through these extension methods.
- These methods are intended for use within the `PaymentsController` and related infrastructure; they are not designed for general-purpose JSON conversion outside the payment domain.
