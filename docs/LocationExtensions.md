# LocationExtensions

Provides extension methods for `Location` objects to facilitate common address formatting, country checks, and coordinate validation.

## API

### `ToFullAddressString`

Formats the location's address components into a single, human-readable string.

- **Parameters**: None
- **Return value**: `string` – A concatenated address string including street, city, region, postal code, and country.
- **Exceptions**: Throws `ArgumentNullException` if the location is `null`.

### `IsInCountry`

Determines whether the location resides within a specified country.

- **Parameters**:
  - `countryCode` (`string`) – The ISO 3166-1 alpha-2 country code to check against (e.g., `"US"`).
- **Return value**: `bool` – `true` if the location's country matches the provided code; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if the location is `null` or if `countryCode` is `null`.

### `ToSimpleName`

Extracts a concise display name for the location, typically the city or region.

- **Parameters**: None
- **Return value**: `string` – A simplified name derived from the location's city or region, or `null` if unavailable.
- **Exceptions**: Throws `ArgumentNullException` if the location is `null`.

### `HasCoordinates`

Checks whether the location has valid geographic coordinates.

- **Parameters**: None
- **Return value**: `bool` – `true` if both latitude and longitude are non-null and within valid ranges; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if the location is `null`.

## Usage
