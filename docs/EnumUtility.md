# EnumUtility

The `EnumUtility` class provides a set of static helper methods for performing common reflection-based operations on enumeration types within the `marketplace-engine` project. It simplifies tasks such as retrieving descriptive metadata, parsing string representations, extracting value lists, and evaluating flag combinations, offering a type-safe interface that reduces boilerplate code associated with standard `System.Enum` manipulation.

## API

### `GetDescription<T>`
Retrieves the human-readable description associated with a specific enum value, typically sourced from a `System.ComponentModel.DescriptionAttribute` applied to the field.
- **Parameters**: Takes a single parameter `value` of type `T`, where `T` is constrained to an enum type.
- **Return Value**: Returns a `string` containing the description if the attribute exists; otherwise, it returns the string representation of the enum member name.
- **Exceptions**: Throws an `ArgumentException` if `T` is not an enum type.

### `TryParseEnum<T>`
Attempts to convert a string representation of an enum member name or its underlying numeric value into the corresponding enum instance.
- **Parameters**: Accepts a `string` input representing the name or value to parse. Case sensitivity behavior depends on the underlying `Enum.Parse` implementation used (typically case-insensitive by default in utility wrappers unless specified).
- **Return Value**: Returns a nullable `T?`. If the parse is successful, it returns the enum value; if the input is invalid or null, it returns `null`.
- **Exceptions**: This method is designed not to throw exceptions for invalid input; it returns `null` instead. However, it may throw an `ArgumentException` if `T` is not an enum type.

### `GetEnumValues<T>`
Retrieves a list containing all defined values of the specified enumeration type.
- **Parameters**: No parameters required; the type `T` is inferred or specified generically.
- **Return Value**: Returns a `List<T>` containing all enum values in the order they are declared.
- **Exceptions**: Throws an `ArgumentException` if `T` is not an enum type.

### `GetEnumNames<T>`
Retrieves a list containing the names of all members in the specified enumeration type.
- **Parameters**: No parameters required; the type `T` is inferred or specified generically.
- **Return Value**: Returns a `List<string>` containing all enum member names in the order they are declared.
- **Exceptions**: Throws an `ArgumentException` if `T` is not an enum type.

### `GetEnumDictionary<T>`
Generates a dictionary mapping enum member names to their underlying integer values.
- **Parameters**: No parameters required; the type `T` is inferred or specified generically.
- **Return Value**: Returns a `Dictionary<string, int>` where the key is the member name and the value is the corresponding integral value.
- **Exceptions**: Throws an `ArgumentException` if `T` is not an enum type.

### `HasFlag<T>`
Determines whether one or more bit fields are set in the specified enum instance.
- **Parameters**: Accepts two parameters: `value` (the enum instance to check) and `flag` (the specific flag or combination of flags to test for), both of type `T`.
- **Return Value**: Returns a `bool` indicating `true` if the flag(s) are set, otherwise `false`.
- **Exceptions**: Throws an `ArgumentException` if `T` is not an enum type, or if `value` and `flag` are not of the same enum type (enforced by the generic constraint).

## Usage

### Example 1: Retrieving Descriptions and Values for UI Population
This example demonstrates how to populate a dropdown menu with enum names and their associated descriptions, falling back to the member name if no description attribute is present.

```csharp
public enum OrderStatus
{
    [Description("Pending Payment")]
    Pending,
    [Description("Shipped to Customer")]
    Shipped,
    Returned
}

// Populate a selection list
var statusNames = EnumUtility.GetEnumNames<OrderStatus>();
var statusValues = EnumUtility.GetEnumValues<OrderStatus>();

foreach (var status in statusValues)
{
    string displayName = EnumUtility.GetDescription(status);
    Console.WriteLine($"Value: {(int)status}, Name: {status}, Display: {displayName}");
}
```

### Example 2: Safe Parsing and Flag Evaluation
This example illustrates safely parsing a user-provided string into an enum and checking for specific permission flags without risking runtime exceptions on invalid input.

```csharp
[Flags]
public enum UserPermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    Admin = Read | Write | Delete
}

public void ProcessPermissionRequest(string input)
{
    // Safely parse input; returns null if "SuperUser" is not defined
    var permission = EnumUtility.TryParseEnum<UserPermissions>(input);

    if (permission.HasValue)
    {
        // Check if the Write flag is explicitly set
        if (EnumUtility.HasFlag(permission.Value, UserPermissions.Write))
        {
            Console.WriteLine("Write access granted.");
        }
        
        // Retrieve mapping for logging
        var permMap = EnumUtility.GetEnumDictionary<UserPermissions>();
        Console.WriteLine($"Current value integer: {permMap[permission.Value.ToString()]}");
    }
    else
    {
        Console.WriteLine("Invalid permission type provided.");
    }
}
```

## Notes

- **Generic Constraint**: All methods rely on the generic type `T` being an enumeration. Passing a non-enum type (e.g., `int`, `string`, or a class) will result in an `ArgumentException` at runtime.
- **Nullable Return**: The `TryParseEnum<T>` method returns a nullable type (`T?`). Callers must check `HasValue` or compare against `null` before accessing the result to avoid `InvalidOperationException`.
- **Thread Safety**: As the class consists entirely of stateless static methods that do not maintain internal mutable state, all members are thread-safe and can be called concurrently from multiple threads without external synchronization.
- **Attribute Dependency**: `GetDescription<T>` relies on the presence of `DescriptionAttribute`. If this attribute is missing from the enum definition, the method gracefully degrades to returning the enum member's name rather than throwing an error.
- **Flags Behavior**: When using `HasFlag<T>` with enums marked with the `[Flags]` attribute, ensure the `flag` parameter represents a valid combination of bits. While the method works on non-flags enums, the logical interpretation of the result differs for bitwise operations.
