# ListingCreatedEventExtensions

The `ListingCreatedEventExtensions` class provides a set of static extension methods designed to simplify the interaction with `ListingCreatedEvent` instances. These methods provide common functionality such as formatting identifiers, generating human-readable summaries, and performing basic validation checks on event properties, promoting cleaner and more maintainable code within the event-driven architecture.

## API

### GetListingId
Retrieves the `ListingId` from a `ListingCreatedEvent` formatted as a string using the 'D' (hyphenated) format specifier.
- **Parameters:** `this ListingCreatedEvent @event` (The event instance to inspect).
- **Returns:** `string` (The formatted listing identifier).
- **Throws:** `ArgumentNullException` if the provided `@event` is null.

### GetSellerId
Retrieves the `SellerId` from a `ListingCreatedEvent` formatted as a string using the 'D' (hyphenated) format specifier.
- **Parameters:** `this ListingCreatedEvent @event` (The event instance to inspect).
- **Returns:** `string` (The formatted seller identifier).
- **Throws:** `ArgumentNullException` if the provided `@event` is null.

### ToEventSummary
Generates a structured, human-readable summary string of the `ListingCreatedEvent`, useful for logging, debugging, or display purposes.
- **Parameters:** `this ListingCreatedEvent @event` (The event instance to summarize).
- **Returns:** `string` (A summary string containing listing, seller, title, category, and timestamp information).
- **Throws:** `ArgumentNullException` if the provided `@event` is null.

### HasValidTitle
Validates that the `Title` property of the `ListingCreatedEvent` is not null, empty, or composed entirely of whitespace.
- **Parameters:** `this ListingCreatedEvent @event` (The event instance to validate).
- **Returns:** `bool` (`true` if the title is valid; otherwise `false`).
- **Throws:** `ArgumentNullException` if the provided `@event` is null.

### HasValidCategory
Validates that the `Category` property of the `ListingCreatedEvent` is not null, empty, or composed entirely of whitespace.
- **Parameters:** `this ListingCreatedEvent @event` (The event instance to validate).
- **Returns:** `bool` (`true` if the category is valid; otherwise `false`).
- **Throws:** `ArgumentNullException` if the provided `@event` is null.

## Usage

```csharp
// Example 1: Logging an event summary
public void HandleEvent(ListingCreatedEvent e)
{
    if (e.HasValidTitle())
    {
        _logger.LogInformation("Processing event: {Summary}", e.ToEventSummary());
    }
}

// Example 2: Accessing formatted identifiers
public void UpdateDatabase(ListingCreatedEvent e)
{
    var listingId = e.GetListingId();
    var sellerId = e.GetSellerId();
    // Use IDs for database operations
}
```

## Notes

- **Edge Cases:** All methods explicitly check for a null event and will throw an `ArgumentNullException`. The validation methods (`HasValidTitle`, `HasValidCategory`) return `false` if the respective string property is null, empty, or whitespace, adhering to standard `string.IsNullOrWhiteSpace` behavior.
- **Thread Safety:** These extension methods are thread-safe as they do not modify the state of the `ListingCreatedEvent` object; they strictly read properties and return new values. However, the `ListingCreatedEvent` instance itself must be accessed in a thread-safe manner if it is being modified concurrently elsewhere in the application.
