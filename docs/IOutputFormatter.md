# IOutputFormatter

The `IOutputFormatter` interface defines a contract for formatting marketplace listings and listing collections into serialized output formats. Implementations are responsible for converting domain objects into structured strings suitable for API responses or other output channels.

## API

### `string FormatListings(IEnumerable<Listing> listings)`

Formats a collection of listings into a serialized string representation.

- **Parameters**
  - `listings`: An enumerable collection of `Listing` objects to be formatted.
- **Return Value**
  - A string containing the serialized representation of the listings.
- **Exceptions**
  - Throws `ArgumentNullException` if `listings` is `null`.

### `string FormatListing(Listing listing)`

Formats a single listing into a serialized string representation.

- **Parameters**
  - `listing`: The `Listing` object to be formatted.
- **Return Value**
  - A string containing the serialized representation of the listing.
- **Exceptions**
  - Throws `ArgumentNullException` if `listing` is `null`.

### `FormatterFactory`

A static factory method for creating formatter instances.

- **Return Value**
  - Returns an instance of a type implementing `IOutputFormatter`.
- **Exceptions**
  - May throw exceptions specific to the implementation if initialization fails.

### `IOutputFormatter GetFormatter(string format)`

Retrieves a formatter instance for the specified format identifier.

- **Parameters**
  - `format`: A string identifying the desired output format (e.g., "json", "xml").
- **Return Value**
  - An `IOutputFormatter` configured for the requested format.
- **Exceptions**
  - Throws `ArgumentNullException` if `format` is `null`.
  - Throws `FormatException` if no formatter is available for the specified format.

## Usage
