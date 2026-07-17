# MappingUtilityJsonExtensions

The `MappingUtilityJsonExtensions` class provides a centralized set of static extension methods for serializing and deserializing core domain entities within the `marketplace-engine` project. It facilitates the conversion between specific business objects—such as `Listing`, `User`, `Message`, `ModerationReport`, and their corresponding Data Transfer Objects (DTOs)—and their JSON string representations. By offering both direct conversion methods and safe "try" patterns, this utility ensures consistent JSON handling across the application while allowing callers to choose between exception-driven error handling and boolean-based flow control.

## API

### ToJson
Converts a supported object instance into its JSON string representation.
*   **Purpose**: Serializes the input object to a JSON string.
*   **Parameters**: Accepts a single generic or overloaded parameter representing the object to serialize (e.g., `Listing`, `User`, `Message`, `ModerationReport`, or their DTO equivalents).
*   **Return Value**: Returns a `string` containing the JSON representation of the object.
*   **Exceptions**: Throws a serialization exception if the object contains circular references, unsupported types, or if the underlying serializer fails.

### FromJsonToListing
Deserializes a JSON string into a `Listing` object.
*   **Purpose**: Converts a JSON string specifically into a `Listing` instance.
*   **Parameters**: `json` (string) – The JSON string to deserialize.
*   **Return Value**: Returns a `Listing?` object if successful, or `null` if the JSON is invalid or represents a null value.
*   **Exceptions**: May throw an exception if the JSON structure is malformed or does not match the `Listing` schema.

### TryFromJsonToListing
Attempts to deserialize a JSON string into a `Listing` object without throwing exceptions.
*   **Purpose**: Safely converts a JSON string to a `Listing` instance, indicating success via a boolean return.
*   **Parameters**: `json` (string) – The JSON string to deserialize.
*   **Return Value**: Returns `true` if deserialization succeeds and outputs the result via an `out` parameter; otherwise returns `false`.
*   **Exceptions**: Does not throw exceptions for invalid JSON; failures are indicated by the boolean return value.

### FromJsonToUser
Deserializes a JSON string into a `User` object.
*   **Purpose**: Converts a JSON string specifically into a `User` instance.
*   **Parameters**: `json` (string) – The JSON string to deserialize.
*   **Return Value**: Returns a `User?` object if successful, or `null` if the JSON is invalid.
*   **Exceptions**: May throw an exception if the JSON structure is malformed.

### TryFromJsonToUser
Attempts to deserialize a JSON string into a `User` object without throwing exceptions.
*   **Purpose**: Safely converts a JSON string to a `User` instance.
*   **Parameters**: `json` (string) – The JSON string to deserialize.
*   **Return Value**: Returns `true` on success with the result in an `out` parameter; `false` on failure.
*   **Exceptions**: Does not throw exceptions for parsing errors.

### FromJsonToMessage
Deserializes a JSON string into a `Message` object.
*   **Purpose**: Converts a JSON string specifically into a `Message` instance.
*   **Parameters**: `json` (string) – The JSON string to deserialize.
*   **Return Value**: Returns a `Message?` object if successful, or `null` if the JSON is invalid.
*   **Exceptions**: May throw an exception if the JSON structure is malformed.

### TryFromJsonToMessage
Attempts to deserialize a JSON string into a `Message` object without throwing exceptions.
*   **Purpose**: Safely converts a JSON string to a `Message` instance.
*   **Parameters**: `json` (string) – The JSON string to deserialize.
*   **Return Value**: Returns `true` on success with the result in an `out` parameter; `false` on failure.
*   **Exceptions**: Does not throw exceptions for parsing errors.

### FromJsonToModerationReport
Deserializes a JSON string into a `ModerationReport` object.
*   **Purpose**: Converts a JSON string specifically into a `ModerationReport` instance.
*   **Parameters**: `json` (string) – The JSON string to deserialize.
*   **Return Value**: Returns a `ModerationReport?` object if successful, or `null` if the JSON is invalid.
*   **Exceptions**: May throw an exception if the JSON structure is malformed.

### TryFromJsonToModerationReport
Attempts to deserialize a JSON string into a `ModerationReport` object without throwing exceptions.
*   **Purpose**: Safely converts a JSON string to a `ModerationReport` instance.
*   **Parameters**: `json` (string) – The JSON string to deserialize.
*   **Return Value**: Returns `true` on success with the result in an `out` parameter; `false` on failure.
*   **Exceptions**: Does not throw exceptions for parsing errors.

### FromJsonToListingDto
Deserializes a JSON string into a `ListingDto` object.
*   **Purpose**: Converts a JSON string specifically into a `ListingDto` instance.
*   **Parameters**: `json` (string) – The JSON string to deserialize.
*   **Return Value**: Returns a `ListingDto?` object if successful, or `null` if the JSON is invalid.
*   **Exceptions**: May throw an exception if the JSON structure is malformed.

### TryFromJsonToListingDto
Attempts to deserialize a JSON string into a `ListingDto` object without throwing exceptions.
*   **Purpose**: Safely converts a JSON string to a `ListingDto` instance.
*   **Parameters**: `json` (string) – The JSON string to deserialize.
*   **Return Value**: Returns `true` on success with the result in an `out` parameter; `false` on failure.
*   **Exceptions**: Does not throw exceptions for parsing errors.

### FromJsonToUserDto
Deserializes a JSON string into a `UserDto` object.
*   **Purpose**: Converts a JSON string specifically into a `UserDto` instance.
*   **Parameters**: `json` (string) – The JSON string to deserialize.
*   **Return Value**: Returns a `UserDto?` object if successful, or `null` if the JSON is invalid.
*   **Exceptions**: May throw an exception if the JSON structure is malformed.

### TryFromJsonToUserDto
Attempts to deserialize a JSON string into a `UserDto` object without throwing exceptions.
*   **Purpose**: Safely converts a JSON string to a `UserDto` instance.
*   **Parameters**: `json` (string) – The JSON string to deserialize.
*   **Return Value**: Returns `true` on success with the result in an `out` parameter; `false` on failure.
*   **Exceptions**: Does not throw exceptions for parsing errors.

### FromJsonToMessageDto
Deserializes a JSON string into a `MessageDto` object.
*   **Purpose**: Converts a JSON string specifically into a `MessageDto` instance.
*   **Parameters**: `json` (string) – The JSON string to deserialize.
*   **Return Value**: Returns a `MessageDto?` object if successful, or `null` if the JSON is invalid.
*   **Exceptions**: May throw an exception if the JSON structure is malformed.

## Usage

### Example 1: Safe Deserialization with Fallback
This example demonstrates using the `TryFromJsonToListing` method to handle potentially malformed user input without crashing the application. If deserialization fails, a default empty listing is used.

```csharp
using MarketplaceEngine.Utilities;

public class ListingService
{
    public Listing ProcessListingInput(string jsonPayload)
    {
        if (string.IsNullOrWhiteSpace(jsonPayload))
        {
            return new Listing();
        }

        if (MappingUtilityJsonExtensions.TryFromJsonToListing(jsonPayload, out var listing))
        {
            return listing;
        }

        // Log warning or handle error appropriately
        Console.WriteLine("Failed to parse listing JSON, returning default.");
        return new Listing();
    }
}
```

### Example 2: Direct Serialization and Deserialization of DTOs
This example shows the round-trip conversion of a `UserDto` using the direct `ToJson` and `FromJsonToUserDto` methods, suitable for scenarios where JSON validity is guaranteed or exceptions are handled at a higher level.

```csharp
using MarketplaceEngine.DTOs;
using MarketplaceEngine.Utilities;

public class UserDataManager
{
    public UserDto CacheAndRetrieve(UserDto user)
    {
        // Serialize to JSON for storage
        string jsonCache = MappingUtilityJsonExtensions.ToJson(user);
        
        // Simulate retrieval from cache
        string retrievedJson = jsonCache; 
        
        // Deserialize back to object
        // Note: This will throw if retrievedJson is invalid
        var cachedUser = MappingUtilityJsonExtensions.FromJsonToUserDto(retrievedJson);
        
        if (cachedUser == null)
        {
            throw new InvalidOperationException("Cache corruption detected: UserDto is null after deserialization.");
        }

        return cachedUser;
    }
}
```

## Notes

*   **Null Handling**: The `FromJson...` methods return `null` for both invalid JSON strings and valid JSON representing a `null` value. Callers must distinguish between these states if necessary, potentially by validating the input string prior to calling or by using the `Try...` variants which may offer clearer failure semantics depending on implementation details.
*   **Thread Safety**: As the class consists entirely of static methods that operate on input parameters without maintaining internal mutable state, `MappingUtilityJsonExtensions` is inherently thread-safe. Multiple threads can safely call these methods concurrently.
*   **Exception Boundaries**: The `ToJson` and `FromJson...` (non-Try) methods will propagate exceptions thrown by the underlying JSON serializer. In high-throughput environments or when processing untrusted input, prefer the `TryFromJson...` variants to avoid the performance cost of exception throwing and catching.
*   **Type Specificity**: Each method is strongly typed to a specific domain model or DTO. Passing a JSON string intended for a `User` into `FromJsonToListing` will result in a failure (either an exception or a `false` return), as the schema mismatch prevents valid deserialization.
