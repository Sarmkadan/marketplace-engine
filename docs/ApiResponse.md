# ApiResponse

A generic response container used throughout the marketplace-engine to standardize API responses. It carries operation results, payload data, status messages, and error details in a structured format suitable for both success and failure scenarios.

## API

### Properties

- **`Success`** (bool)
  Indicates whether the operation completed successfully.
  This flag is `true` for successful responses and `false` for errors.

- **`Data`** (T?)
  The payload returned on success. Contains the requested resource or result set when `Success` is `true`. May be `null` when no data is available.

- **`Message`** (string?)
  A human-readable status or informational message. Typically used to convey success confirmation or high-level error context.

- **`ErrorCode`** (string?)
  A machine-readable error identifier. Present only when the operation failed, enabling clients to programmatically handle specific error conditions.

- **`Errors`** (Dictionary<string, string>?)
  A collection of field-specific validation or processing errors. Each key-value pair maps a field name to its corresponding error message. Populated only when detailed error breakdown is available (e.g., validation failures).

- **`Timestamp`** (DateTime)
  The UTC date and time when the response was generated. Provides traceability and auditability for all responses.

- **`RequestId`** (string?)
  An optional identifier correlating this response with the originating request. Useful for tracing requests across distributed systems.

### Static Methods

- **`SuccessResponse<T>(T? data, string? message = null, string? requestId = null)`** → `ApiResponse<T>`
  Constructs a successful response containing the provided data and optional message.
  Parameters:
    - `data`: The payload to return.
    - `message`: Optional success message.
    - `requestId`: Optional correlation identifier.
  Returns a new `ApiResponse<T>` with `Success` set to `true`.

- **`ErrorResponse<T>(string? message = null, string? errorCode = null, Dictionary<string, string>? errors = null, string? requestId = null)`** → `ApiResponse<T>`
  Constructs an error response without throwing an exception.
  Parameters:
    - `message`: Optional error description.
    - `errorCode`: Optional machine-readable error identifier.
    - `errors`: Optional collection of field-specific errors.
    - `requestId`: Optional correlation identifier.
  Returns a new `ApiResponse<T>` with `Success` set to `false`.

- **`ValidationErrorResponse<T>(Dictionary<string, string> errors, string? message = null, string? requestId = null)`** → `ApiResponse<T>`
  Constructs a validation error response, typically used when input validation fails.
  Parameters:
    - `errors`: Dictionary mapping invalid fields to their error messages.
    - `message`: Optional high-level validation message.
    - `requestId`: Optional correlation identifier.
  Returns a new `ApiResponse<T>` with `Success` set to `false` and `Errors` populated.

### Non-Generic ApiResponse (for non-generic success/error signaling)

- **`Success`** (bool)
  Indicates whether the operation completed successfully.
  This flag is `true` for successful responses and `false` for errors.

- **`Message`** (string?)
  A human-readable status or informational message.

- **`ErrorCode`** (string?)
  A machine-readable error identifier.

- **`Timestamp`** (DateTime)
  The UTC date and time when the response was generated.

- **`RequestId`** (string?)
  An optional identifier correlating this response with the originating request.

- **`SuccessResponse(string? message = null, string? requestId = null)`** → `ApiResponse`
  Constructs a successful non-generic response.
  Parameters:
    - `message`: Optional success message.
    - `requestId`: Optional correlation identifier.
  Returns a new `ApiResponse` with `Success` set to `true`.

- **`ErrorResponse(string? message = null, string? errorCode = null, string? requestId = null)`** → `ApiResponse`
  Constructs an error response without throwing an exception.
  Parameters:
    - `message`: Optional error description.
    - `errorCode`: Optional machine-readable error identifier.
    - `requestId`: Optional correlation identifier.
  Returns a new `ApiResponse` with `Success` set to `false`.

### Paginated Response (extends ApiResponse)

- **`Items`** (List<T>)
  The list of items returned in the current page. Empty if no items match the query.

- **`Page`** (int)
  The current page number, starting at 1. Reflects the requested page offset.

- **`PageSize`** (int)
  The maximum number of items per page as requested. Determines the size of the `Items` collection.
