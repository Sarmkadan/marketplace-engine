# Money

Represents an immutable monetary value composed of a decimal amount and an ISO 4217 currency code. This type provides arithmetic operations that enforce same-currency constraints and value equality semantics, making it suitable for financial calculations where precision and currency integrity are required.

## API

### `public decimal Amount`

Gets the numeric value of the monetary amount. This value is always expressed in the base unit of the currency (e.g., dollars, euros, yen) and carries the full precision of the `decimal` type.

### `public string CurrencyCode`

Gets the ISO 4217 three-letter currency code (e.g., "USD", "EUR", "GBP") identifying the currency in which the amount is denominated. The code is case-sensitive and stored in uppercase.

### `public Money(decimal amount, string currencyCode)`

Initializes a new instance of the `Money` type with the specified amount and currency code.

**Parameters:**
- `amount` — The decimal value representing the monetary amount.
- `currencyCode` — A non-null, non-empty ISO 4217 currency code.

**Exceptions:**
- `ArgumentNullException` — Thrown when `currencyCode` is null.
- `ArgumentException` — Thrown when `currencyCode` is empty or consists only of whitespace.

### `public Money Add(Money other)`

Adds the specified monetary value to this instance and returns a new `Money` object representing the sum.

**Parameters:**
- `other` — The `Money` value to add. Must not be null.

**Returns:**
A new `Money` instance with the sum of the two amounts and the same currency code.

**Exceptions:**
- `ArgumentNullException` — Thrown when `other` is null.
- `InvalidOperationException` — Thrown when `other.CurrencyCode` does not match this instance's `CurrencyCode`.

### `public Money Subtract(Money other)`

Subtracts the specified monetary value from this instance and returns a new `Money` object representing the difference.

**Parameters:**
- `other` — The `Money` value to subtract. Must not be null.

**Returns:**
A new `Money` instance with the difference of the two amounts and the same currency code.

**Exceptions:**
- `ArgumentNullException` — Thrown when `other` is null.
- `InvalidOperationException` — Thrown when `other.CurrencyCode` does not match this instance's `CurrencyCode`.

### `public Money Multiply(decimal multiplier)`

Multiplies the amount by the specified decimal factor and returns a new `Money` object with the scaled amount and the same currency code.

**Parameters:**
- `multiplier` — The decimal factor by which to multiply the amount.

**Returns:**
A new `Money` instance with the multiplied amount.

**Remarks:**
Negative multipliers are permitted, which may result in a negative amount. No rounding is applied beyond standard `decimal` multiplication semantics.

### `public bool Equals(Money other)`

Indicates whether this instance is equal to another `Money` instance by comparing both the amount and the currency code for exact equality.

**Parameters:**
- `other` — The `Money` instance to compare with this instance. Can be null.

**Returns:**
`true` if `other` is not null and has exactly the same amount and currency code; otherwise `false`.

### `public override bool Equals(object obj)`

Indicates whether this instance is equal to a specified object. Returns `true` if the object is a `Money` instance and passes the typed `Equals(Money)` comparison.

**Parameters:**
- `obj` — The object to compare with this instance.

**Returns:**
`true` if `obj` is a `Money` with the same amount and currency code; otherwise `false`.

### `public override int GetHashCode()`

Returns a hash code for this instance computed from both the amount and the currency code. Two `Money` instances that are equal according to `Equals` will produce the same hash code.

### `public override string ToString()`

Returns a string representation of this monetary value, typically combining the amount and currency code in a human-readable format.

## Usage

```csharp
// Example 1: Basic arithmetic with same-currency values
Money price = new Money(29.99m, "USD");
Money tax = new Money(2.40m, "USD");
Money total = price.Add(tax);

Console.WriteLine(total.ToString()); // e.g., "32.39 USD"

Money discount = total.Multiply(0.10m);
Money final = total.Subtract(discount);

Console.WriteLine(final.ToString()); // e.g., "29.15 USD"
```

```csharp
// Example 2: Cross-currency validation and equality checks
Money usdAmount = new Money(100.00m, "USD");
Money eurAmount = new Money(100.00m, "EUR");

// This throws InvalidOperationException due to currency mismatch
// Money result = usdAmount.Add(eurAmount);

bool sameCurrency = usdAmount.Equals(new Money(100.00m, "USD")); // true
bool differentCurrency = usdAmount.Equals(eurAmount);            // false

// Using the typed Equals for null-safe comparison
Money? reference = null;
bool isNullEqual = usdAmount.Equals(reference); // false
```

## Notes

- **Immutability:** All arithmetic methods (`Add`, `Subtract`, `Multiply`) return new `Money` instances. The original instance is never modified, making the type safe for sharing across threads without synchronization.
- **Thread safety:** Since instances are immutable and the type contains no mutable static state, all public members are inherently thread-safe. Multiple threads may safely read and use `Money` instances concurrently.
- **Currency mismatch:** `Add` and `Subtract` enforce same-currency operations at runtime. Attempting to combine values with different currency codes throws `InvalidOperationException`. Callers must perform currency conversion explicitly before performing arithmetic.
- **Equality precision:** `Equals` performs exact decimal equality. For financial scenarios involving calculated values, consider whether rounding or tolerance-based comparison is needed before relying on equality checks.
- **Negative amounts:** The type does not restrict amounts to non-negative values. Negative amounts are valid and can arise from subtraction or multiplication by negative multipliers.
- **Currency code casing:** The constructor enforces non-null and non-empty input but does not normalize casing. Callers should supply uppercase ISO 4217 codes to maintain consistency across instances.
