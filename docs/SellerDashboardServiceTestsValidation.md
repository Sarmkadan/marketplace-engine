# SellerDashboardServiceTestsValidation

`SellerDashboardServiceTestsValidation` is a static utility class within the `marketplace-engine` project that provides centralized validation logic for seller dashboard service tests. It exposes overloaded methods to check the validity of test-related data, return lists of validation error messages, and enforce correctness by throwing exceptions when validation fails. The class is designed to be used in testing and quality-assurance contexts where consistent, reusable validation rules are required.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate(/* parameters not shown */)
public static IReadOnlyList<string> Validate(/* parameters not shown */)
public static IReadOnlyList<string> Validate(/* parameters not shown */)
```

Performs validation on the provided input and returns a read-only list of error messages. Each overload accepts a different set of parameters relevant to seller dashboard service tests. An empty list indicates that the input is valid. The returned list is never `null`.

**Returns:**  
An `IReadOnlyList<string>` containing zero or more human-readable validation error messages.

**Throws:**  
No exceptions are thrown by these methods themselves; all validation failures are communicated through the returned list.

---

### IsValid

```csharp
public static bool IsValid(/* parameters not shown */)
public static bool IsValid(/* parameters not shown */)
public static bool IsValid(/* parameters not shown */)
```

Evaluates the given parameters against the validation rules and returns a boolean indicating whether the input is valid. Each overload corresponds to a different validation scenario within the seller dashboard service test domain.

**Returns:**  
`true` if the input passes all validation rules; otherwise `false`.

**Throws:**  
No exceptions are thrown. All outcomes are expressed through the boolean return value.

---

### EnsureValid

```csharp
public static void EnsureValid(/* parameters not shown */)
public static void EnsureValid(/* parameters not shown */)
public static void EnsureValid(/* parameters not shown */)
```

Validates the given parameters and throws an exception if any validation rule is violated. This is the strictest validation method, intended for scenarios where invalid state must halt execution immediately.

**Throws:**  
An exception (typically `ArgumentException`, `ValidationException`, or a domain-specific exception) when validation fails. The exact exception type and message depend on the specific overload and the nature of the validation failure.

## Usage

### Example 1: Collecting validation errors

```csharp
var errors = SellerDashboardServiceTestsValidation.Validate(sellerId, dashboardOptions);
if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
    return;
}

// Proceed with test execution
RunSellerDashboardTests(sellerId, dashboardOptions);
```

### Example 2: Fail-fast validation

```csharp
try
{
    SellerDashboardServiceTestsValidation.EnsureValid(sellerId, dateRange, testConfiguration);
    ExecuteDashboardTestSuite(sellerId, dateRange, testConfiguration);
}
catch (ValidationException ex)
{
    Logger.LogError(ex, "Seller dashboard test validation failed");
    throw;
}
```

## Notes

- All members are `static`; the class is not intended to be instantiated.
- The `Validate` methods never return `null` — an empty list is returned for valid input.
- The `EnsureValid` methods are the only members that throw exceptions; `Validate` and `IsValid` always complete without throwing.
- Thread safety: All methods are static and do not mutate shared state. They are safe to call concurrently from multiple threads as long as the arguments themselves are not mutated during the call.
- Edge cases: When parameters are `null` or contain default values, the behavior depends on the specific overload. `IsValid` will return `false`, `Validate` will include an appropriate error message in the returned list, and `EnsureValid` will throw an exception.
- The three overloads of each member correspond to distinct validation scenarios within the seller dashboard service test domain. Callers must select the appropriate overload based on the data available at the call site.
