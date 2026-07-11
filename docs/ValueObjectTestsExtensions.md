# ValueObjectTestsExtensions

A set of extension methods for testing value objects in the marketplace-engine project. These utilities simplify assertions for equality, equivalence, and creation of common value object types such as `Money`, `Rating`, and `Location`.

## API

### `public static Money CreateMoney(decimal amount, string currencyCode)`

Creates a `Money` value object with the specified amount and currency code.

- **Parameters**:
  - `amount` (decimal): The monetary amount.
  - `currencyCode` (string): The ISO currency code (e.g., "USD", "EUR").
- **Return value**: A new instance of `Money`.
- **Throws**: `ArgumentNullException` if `currencyCode` is null or empty.
- **Throws**: `ArgumentException` if `currencyCode` is not a valid ISO currency code.

---

### `public static Rating CreateRating(int value)`

Creates a `Rating` value object with the specified numeric value.

- **Parameters**:
  - `value` (int): The rating value, typically constrained between a minimum and maximum (e.g., 1 to 5).
- **Return value**: A new instance of `Rating`.
- **Throws**: `ArgumentOutOfRangeException` if `value` is outside the valid range for ratings.

---

### `public static Location CreateLocation(decimal latitude, decimal longitude)`

Creates a `Location` value object with the specified geographic coordinates.

- **Parameters**:
  - `latitude` (decimal): The geographic latitude.
  - `longitude` (decimal): The geographic longitude.
- **Return value**: A new instance of `Location`.
- **Throws**: `ArgumentOutOfRangeException` if `latitude` or `longitude` is outside valid geographic bounds.

---
### `public static void ShouldBeEquivalentTo(this object actual, object expected)`

Asserts that two value objects are equivalent by comparing their serialized or structural properties.

- **Parameters**:
  - `actual` (object): The actual value object under test.
  - `expected` (object): The expected value object to compare against.
- **Throws**: `XunitException` if the objects are not equivalent.
- **Behavior**: Performs a deep comparison of value object properties, ignoring reference equality.

## Usage
