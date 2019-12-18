# MarketplaceException

A custom exception type used in the **marketplace-engine** library to represent domain-specific failures. It extends the base `Exception` class and provides additional properties for error codes and validation details, enabling callers to programmatically inspect failure reasons.

## API

### Constructors

- `MarketplaceException()`  
  **Purpose:** Initializes a new instance of the `MarketplaceException` class with default values.  
  **Parameters:** None.  
  **Return value:** (constructor)  
  **Throws:** None.

- `MarketplaceException(string message)`  
  **Purpose:** Initializes a new instance with a specified error message.  
  **Parameters:**  
    - `message`: The error message that explains the reason for the exception.  
  **Return value:** (constructor)  
  **Throws:** None.

- `MarketplaceException(string message, Exception innerException)`  
  **Purpose:** Initializes a new instance with a specified error message and a reference to the inner exception that is the cause of this exception.  
  **Parameters:**  
    - `message`: The error message.  
    - `innerException`: The exception that is the cause of the current exception, or `null` if no inner exception is specified.  
  **Return value:** (constructor)  
  **Throws:** None.

### Properties

- `string? ErrorCode`  
  **Purpose:** Gets or sets the application‑specific error code associated with the exception.  
  **Parameters:** (property)  
  **Return value:** A string representing the error code, or `null` if not set.  
  **Throws:** None.

- `Dictionary<string, string[]>? ValidationErrors`  
  **Purpose:** Gets or sets a dictionary where the key is the field name and the value is an array of validation error messages for that field.  
  **Parameters:** (property)  
  **Return value:** A dictionary of validation errors, or `null` if no validation errors are present.  
  **Throws:** None.

### Static Methods

- `static MarketplaceException CreateWithContext(...)`  
  **Purpose:** Factory method that creates a `MarketplaceException` instance enriched with contextual information (e.g., operation name, request identifiers).  
  **Parameters:** The method accepts contextual parameters as defined by the implementation; see the source code for the exact signature.  
  **Return value:** A new `MarketplaceException` populated with the supplied context.  
  **Throws:** May throw `ArgumentNullException` if required context parameters are `null`.

## Usage

```csharp
// Example 1: Throwing a MarketplaceException with an error code and validation errors.
var ex = new MarketplaceException("Invalid payload supplied.")
{
    ErrorCode = "PAYLOAD_INVALID",
    ValidationErrors = new Dictionary<string, string[]>
    {
        { "email", new[] { "Email address is required.", "Email address is malformed." } },
        { "age",  new[] { "Age must be a positive integer." } }
    };
throw ex;
```

```csharp
// Example 2: Using the CreateWithContext factory to add diagnostic information.
try
{
    // Some operation that may fail
}
catch (InvalidOperationException inner)
{
    var contextualEx = MarketplaceException.CreateWithContext(
        operation: "ProcessOrder",
        correlationId: Guid.NewGuid().ToString(),
        innerException: inner);
    throw contextualEx;
}
```

## Notes

- The `ErrorCode` and `ValidationErrors` properties can be set to `null`; consumers should check for `null` before accessing them.
- After an exception instance is created, mutating its properties from multiple threads without external synchronization is not thread‑safe. However, constructing distinct `MarketplaceException` objects concurrently is safe.
- If `ValidationErrors` is non‑null but contains empty arrays, it indicates that a field was validated but no errors were found; this is permissible but may be redundant.
- The `CreateWithContext` method does not modify any existing exception instance; it always returns a new object. Passing `null` for a required contextual argument will result in an `ArgumentNullException`.
