# MoneyExtensions

The `MoneyExtensions` class provides a set of utility extension methods for the `Money` type, facilitating essential arithmetic, comparison, and formatting operations within the `marketplace-engine`. These methods simplify interactions with financial data by encapsulating complex logic into readable, fluent API calls, ensuring consistency across the application.

## API

### Round
Rounds the money amount based on the provided `MidpointRounding` mode.
*   **Parameters:** `MidpointRounding mode`
*   **Returns:** A new `Money` instance with the rounded amount.

### IsZero
Determines whether the money amount is exactly zero.
*   **Returns:** `true` if the amount is zero; otherwise, `false`.

### IsPositive
Determines whether the money amount is greater than zero.
*   **Returns:** `true` if the amount is greater than zero; otherwise, `false`.

### IsNegative
Determines whether the money amount is less than zero.
*   **Returns:** `true` if the amount is less than zero; otherwise, `false`.

### PercentageOf
Calculates what percentage the `part` amount represents of the `total` amount.
*   **Parameters:** `Money total`, `Money part`
*   **Returns:** A `decimal` representation of the percentage.

### Percentage
Calculates a specific percentage of the money amount.
*   **Parameters:** `decimal percentage`
*   **Returns:** A new `Money` instance representing the calculated percentage value.

### CompareTo
Compares this money instance with another instance.
*   **Parameters:** `Money other`
*   **Returns:** An `int` indicating whether this instance precedes, follows, or occurs in the same position in the sort order as the other.

### IsGreaterThan
Determines if this money amount is greater than the specified amount.
*   **Parameters:** `Money other`
*   **Returns:** `true` if greater; otherwise, `false`.

### IsGreaterThanOrEqual
Determines if this money amount is greater than or equal to the specified amount.
*   **Parameters:** `Money other`
*   **Returns:** `true` if greater or equal; otherwise, `false`.

### IsLessThan
Determines if this money amount is less than the specified amount.
*   **Parameters:** `Money other`
*   **Returns:** `true` if less; otherwise, `false`.

### IsLessThanOrEqual
Determines if this money amount is less than or equal to the specified amount.
*   **Parameters:** `Money other`
*   **Returns:** `true` if less or equal; otherwise, `false`.

### Abs
Returns the absolute value of the money instance.
*   **Returns:** A new `Money` instance with a non-negative amount.

### Negate
Returns the negated value of the money instance.
*   **Returns:** A new `Money` instance with the amount multiplied by -1.

### ToStringWithSymbol
Formats the money instance as a string, including the currency symbol.
*   **Returns:** A `string` representing the formatted money value.

## Usage

```csharp
// Example 1: Comparing and checking money values
var price = new Money(150.00m, "USD");
var threshold = new Money(100.00m, "USD");

if (price.IsGreaterThan(threshold))
{
    Console.WriteLine("Price exceeds threshold.");
}

if (price.IsPositive())
{
    // Proceed with processing
}
```

```csharp
// Example 2: Arithmetic and formatting
var originalPrice = new Money(200.00m, "USD");

// Apply a 15% discount
var discountedPrice = originalPrice.Percentage(85.0m);
var roundedPrice = discountedPrice.Round(MidpointRounding.AwayFromZero);

// Output: $170.00
Console.WriteLine(roundedPrice.ToStringWithSymbol());
```

## Notes

*   **Thread Safety:** These extension methods are stateless and do not modify the original `Money` instance. They are thread-safe, provided the underlying `Money` objects are utilized in a thread-safe manner as defined by the `Money` class implementation.
*   **Precision:** Operations involving decimals (such as `Percentage`) are subject to standard C# `decimal` precision limitations. Care should be taken when performing repeated arithmetic operations to avoid cumulative rounding errors.
*   **Currency Mismatch:** These methods generally assume compatibility between `Money` instances (e.g., matching currency codes). Comparisons or arithmetic operations between mismatched currencies may result in unexpected behavior, depending on the underlying `Money` class implementation.
