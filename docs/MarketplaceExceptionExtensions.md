# MarketplaceExceptionExtensions

Provides a set of extension methods for examining and formatting validation error details carried by marketplace‑related exceptions. These helpers allow callers to determine whether an exception contains validation information, retrieve a structured dictionary of field‑level errors, and produce readable error strings for logging or user feedback.

## API

### `public static bool HasValidationErrors(this Exception ex)`

**Purpose**  
Indicates whether the supplied exception contains validation error data.

**Parameters**  
- `ex`: The exception to inspect.

**Return value**  
`true` if the exception carries validation errors; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` if `ex` is `null`.

### `public static string GetErrorMessage(this Exception ex)`

**Purpose**  
Retrieves a concise, single‑line message summarizing the validation errors contained in the exception.

**Parameters**  
- `ex`: The exception to query.

**Return value**  
A string describing the validation errors, or `null`/empty string when no validation errors are present.

**Exceptions**  
- `ArgumentNullException` if `ex` is `null`.

### `public static Dictionary<string, string[]> GetValidationErrors(this Exception ex)`

**Purpose**  
Returns a dictionary mapping each field (or property) that failed validation to an array of associated error messages.

**Parameters**  
- `ex`: The exception to query.

**Return value**  
A `Dictionary<string, string[]>` where the key is the field name and the value is an array of error messages for that field. Returns an empty dictionary when the exception contains no validation errors.

**Exceptions**  
- `ArgumentNullException` if `ex` is `null`.

### `public static string ToErrorString(this Exception ex)`

**Purpose**  
Produces a multi‑line, human‑readable representation of all validation errors held by the exception.

**Parameters**  
- `ex`: The exception to format.

**Return value**  
A formatted string listing each field and its error messages. Returns an empty string when no validation errors are present.

**Exceptions**  
- `ArgumentNullException` if `ex` is `null`.

## Usage

```csharp
try
{
    // Some operation that may throw a marketplace validation exception
    ProcessOrder(order);
}
catch (Exception ex) when (ex.HasValidationErrors())
{
    // Log the structured error details
    var errors = ex.GetValidationErrors();
    foreach (var kvp in errors)
    {
        _logger.LogWarning("Validation failed for {Field}: {Errors}",
            kvp.Key, string.Join(", ", kvp.Value));
    }

    // Optionally return a user‑friendly message
    return BadRequest(ex.GetErrorMessage());
}
```

```csharp
public string GetFailureDetails(Exception ex)
{
    // Safely obtain a ready‑to‑display error string; returns empty string if none.
    string errorDetails = ex.ToErrorString();
    return string.IsNullOrWhiteSpace(errorDetails)
        ? "Operation completed successfully."
        : $"Operation failed with the following validation issues:{Environment.NewLine}{errorDetails}";
}
```

## Notes

- The extension methods are designed to work with exceptions that expose validation information (e.g., a custom `MarketplaceValidationException`). If the exception type does not contain such data, the methods return their default “no‑error” values (`false`, `null`/empty string, empty dictionary) rather than throwing.
- All methods validate that the supplied `ex` argument is not `null` and will throw `ArgumentNullException` otherwise.
- Because the methods contain no static state and rely solely on the input instance, they are thread‑safe and can be invoked concurrently from multiple threads without additional synchronization.
