# ValueObjectTests

The `ValueObjectTests` class serves as the comprehensive test suite for validating the behavior, immutability, and business rule enforcement of value objects within the `marketplace-engine` domain. It specifically targets core entities such as `Money`, `Rating`, and `Location`, ensuring that constructor constraints, arithmetic operations, and state transitions adhere strictly to defined specifications while correctly handling invalid inputs through expected exceptions.

## API

### `Money_Constructor_WithNegativeAmount_ThrowsArgumentException`
Validates that the `Money` value object rejects negative values during instantiation. This method executes a test case where a negative amount is passed to the `Money` constructor. It does not return a value; instead, it asserts that an `ArgumentException` is thrown, ensuring financial integrity by preventing the creation of invalid monetary states.

### `Money_Add_WithSameCurrency_ReturnsCorrectSum`
Verifies the addition logic for two `Money` instances sharing the same currency code. The test invokes the `Add` method on a `Money` instance with another instance of the same currency. It asserts that the resulting object contains an amount equal to the sum of the operands, confirming correct arithmetic handling without currency conversion errors.

### `Money_Add_WithDifferentCurrencies_ThrowsInvalidOperationException`
Ensures type safety and business logic consistency when adding monetary values of different currencies. This test attempts to add two `Money` objects with differing currency codes. It expects an `InvalidOperationException` to be thrown, preventing accidental mixing of currencies without explicit conversion logic.

### `Money_Multiply_ByZero_ReturnsZeroAmount`
Tests the scalar multiplication behavior of the `Money` type when the multiplier is zero. The test calls the multiplication operator or method with a factor of `0`. It asserts that the resulting `Money` object has an amount of zero while preserving the original currency, verifying correct handling of nullifying factors.

### `Rating_Constructor_WithScoreAboveFive_ThrowsArgumentException`
Validates the upper bound constraint for the `Rating` value object. This test attempts to instantiate a `Rating` with a score greater than the maximum allowed value (5). It asserts that an `ArgumentException` is thrown, enforcing the standard 1-to-5 rating scale used throughout the marketplace.

### `Rating_AddReview_IncrementsTotalReviews`
Verifies the state transition logic when a new review is associated with a `Rating`. The test calls the `AddReview` method on an existing `Rating` instance. It asserts that the `TotalReviews` count increments by one while the aggregate score updates correctly, ensuring accurate statistical tracking.

### `Location_Constructor_WithThreeLetterCountryCode_ThrowsArgumentException`
Enforces the ISO country code format standard within the `Location` value object. This test provides a three-letter string to the `Location` constructor where a two-letter ISO 3166-1 alpha-2 code is expected. It asserts that an `ArgumentException` is thrown, maintaining data consistency for geographic filtering and calculations.

### `Location_DistanceTo_WithoutCoordinates_ReturnsNull`
Handles edge cases where geographic coordinates are missing. The test invokes the `DistanceTo` method on a `Location` instance that lacks defined latitude and longitude. It asserts that the method returns `null` instead of throwing an exception or calculating an invalid distance, allowing calling code to gracefully handle incomplete location data.

## Usage

The following examples demonstrate how the behaviors verified by `ValueObjectTests` manifest in actual domain logic.

### Example 1: Enforcing Monetary Constraints
This example illustrates the validation logic tested by `Money_Constructor_WithNegativeAmount_ThrowsArgumentException` and `Money_Add_WithDifferentCurrencies_ThrowsInvalidOperationException`.

```csharp
try 
{
    // This will throw ArgumentException as verified by Money_Constructor_WithNegativeAmount_ThrowsArgumentException
    var invalidMoney = new Money(-50.00m, "USD");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Construction failed: {ex.Message}");
}

var priceUsd = new Money(100.00m, "USD");
var priceEur = new Money(100.00m, "EUR");

try 
{
    // This will throw InvalidOperationException as verified by Money_Add_WithDifferentCurrencies_ThrowsInvalidOperationException
    var total = priceUsd.Add(priceEur);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Addition failed: {ex.Message}");
}
```

### Example 2: Handling Geographic and Rating Logic
This example demonstrates the behavior verified by `Location_DistanceTo_WithoutCoordinates_ReturnsNull` and `Rating_Constructor_WithScoreAboveFive_ThrowsArgumentException`.

```csharp
// Attempting to create an invalid rating throws ArgumentException
try 
{
    var invalidRating = new Rating(6); 
}
catch (ArgumentException)
{
    // Handle invalid user input
}

var warehouseLocation = new Location("US", null, null); // Coordinates missing
var customerLocation = new Location("US", 40.7128m, -74.0060m);

// Returns null as verified by Location_DistanceTo_WithoutCoordinates_ReturnsNull
decimal? distance = warehouseLocation.DistanceTo(customerLocation);

if (distance == null)
{
    Console.WriteLine("Distance calculation unavailable due to missing coordinates.");
}
```

## Notes

*   **Exception Specificity**: The test suite distinguishes between argument validation errors (`ArgumentException`) and logical state errors (`InvalidOperationException`). Consumers of these value objects should catch specific exception types rather than generic `Exception` to handle domain rule violations appropriately.
*   **Null Handling in Calculations**: The `Location_DistanceTo` method explicitly returns `null` for incomplete data rather than throwing. Callers must perform null checks on the return value before performing further mathematical operations to avoid `NullReferenceException`.
*   **Immutability**: As value objects, instances of `Money`, `Rating`, and `Location` are expected to be immutable. Methods like `Add` or `AddReview` return new instances rather than modifying the existing state.
*   **Thread Safety**: Since these value objects are immutable and stateless regarding external dependencies, they are inherently thread-safe for read operations. However, the creation of new instances via methods like `AddReview` must be handled by the calling context if aggregate root consistency is required across threads.
