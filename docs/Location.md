# Location

Represents a geographical location with city, state, country, and optional postal code and coordinate data. This type is used throughout the marketplace engine to model addresses, calculate distances between points, and support location-based queries such as proximity searches and regional filtering.

## API

### Public Members

#### `string City`

Gets the city name of the location.

**Returns:** A non-null string containing the city name.

---

#### `string State`

Gets the state or province name of the location.

**Returns:** A non-null string containing the state or province.

---

#### `string CountryCode`

Gets the country code of the location.

**Returns:** A non-null string containing the country code (typically an ISO 3166 alpha-2 or alpha-3 code).

---

#### `string? PostalCode`

Gets the postal code of the location, if available.

**Returns:** A string containing the postal code, or `null` if not specified.

---

#### `double? Latitude`

Gets the latitude coordinate of the location, if available.

**Returns:** A `double` representing the latitude in decimal degrees, or `null` if not specified.

---

#### `double? Longitude`

Gets the longitude coordinate of the location, if available.

**Returns:** A `double` representing the longitude in decimal degrees, or `null` if not specified.

---

#### `Location()`

Initializes a new instance of the `Location` type.

**Remarks:** Default constructor. Properties must be set after construction or through an alternative initialization path.

---

#### `double? DistanceTo(Location other)`

Calculates the distance from this location to another location.

**Parameters:**
- `other` — The target `Location` to measure distance to. Must not be `null`.

**Returns:** A `double?` representing the distance (typically in kilometers or miles, depending on implementation context), or `null` if either location lacks sufficient coordinate data (`Latitude` and `Longitude` must both be non-null on both instances).

**Throws:** `ArgumentNullException` if `other` is `null`.

---

#### `bool Equals(Location other)`

Determines whether this instance is equal to another `Location` instance.

**Parameters:**
- `other` — The `Location` to compare with, or `null`.

**Returns:** `true` if the two instances are considered equal based on their property values; `false` otherwise.

---

#### `override bool Equals(object obj)`

Determines whether this instance is equal to a specified object.

**Parameters:**
- `obj` — The object to compare with, or `null`.

**Returns:** `true` if `obj` is a `Location` and equal to this instance; `false` otherwise.

---

#### `override int GetHashCode()`

Generates a hash code for this instance based on its property values.

**Returns:** An `int` hash code suitable for use in hash-based collections.

---

#### `override string ToString()`

Returns a string representation of the location.

**Returns:** A formatted string that typically includes city, state, and country components.

## Usage

### Example 1: Basic Location Creation and Comparison

```csharp
var warehouse = new Location
{
    City = "Austin",
    State = "Texas",
    CountryCode = "US",
    PostalCode = "78701",
    Latitude = 30.2672,
    Longitude = -97.7431
};

var customer = new Location
{
    City = "Austin",
    State = "Texas",
    CountryCode = "US",
    PostalCode = "78701",
    Latitude = 30.2672,
    Longitude = -97.7431
};

bool sameLocation = warehouse.Equals(customer);
Console.WriteLine($"Locations equal: {sameLocation}");
Console.WriteLine(warehouse.ToString());
```

### Example 2: Distance Calculation for Proximity Check

```csharp
var sellerLocation = new Location
{
    City = "Seattle",
    State = "Washington",
    CountryCode = "US",
    Latitude = 47.6062,
    Longitude = -122.3321
};

var buyerLocation = new Location
{
    City = "Portland",
    State = "Oregon",
    CountryCode = "US",
    Latitude = 45.5152,
    Longitude = -122.6784
};

double? distance = sellerLocation.DistanceTo(buyerLocation);

if (distance.HasValue && distance.Value < 300)
{
    Console.WriteLine($"Within delivery range: {distance.Value:F1} units away.");
}
else
{
    Console.WriteLine("Outside delivery range or coordinates unavailable.");
}
```

## Notes

- **Coordinate Dependency:** `DistanceTo` returns `null` when either location has a `null` `Latitude` or `Longitude`. Callers must guard against this by checking `HasValue` on the result before performing numeric comparisons.
- **Equality Semantics:** `Equals(Location)` and the overridden `Equals(object)` rely on property-level comparison. Two instances with identical city, state, country, postal code, and coordinate values are considered equal. `GetHashCode` is consistent with this equality definition, making `Location` safe for use as a dictionary key or in hash sets when all contributing properties are set.
- **Null Handling:** `Equals(Location)` accepts `null` and returns `false`. `DistanceTo` throws `ArgumentNullException` on a `null` argument. `ToString` does not throw and handles missing optional fields gracefully.
- **Thread Safety:** This type is not inherently thread-safe for mutation. Once all properties are assigned and the instance is treated as immutable, it is safe to read from multiple threads concurrently. Avoid modifying properties after publication to other threads unless external synchronization is applied.
