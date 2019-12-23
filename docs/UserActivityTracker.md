# UserActivityTracker

Tracks user interactions with listings in the marketplace engine, enabling analysis of user behavior patterns, audience identification, and signal filtering within time windows.

## API

### `UserActivityTracker`

Initializes a new tracker instance. The tracker is designed to be thread-safe and supports concurrent recording and retrieval operations.

### `RecordAsync`

Records a user activity signal asynchronously.

- **Parameters**
  - `signal`: The `UserActivitySignal` to record, containing details such as user identifier, listing identifier, timestamp, and signal type.
- **Return Value**
  - A `Task` representing the asynchronous operation.
- **Exceptions**
  - Throws `ArgumentNullException` if `signal` is `null`.
  - Throws `InvalidOperationException` if the tracker is disposed or in an invalid state.

### `GetUserHistoryAsync`

Retrieves the full history of user activity signals for a given user asynchronously.

- **Return Value**
  - A `Task<IReadOnlyList<UserActivitySignal>>` containing the user's activity signals in chronological order.
- **Exceptions**
  - Throws `ArgumentNullException` if the user identifier is `null`.
  - Throws `InvalidOperationException` if the tracker is disposed or in an invalid state.

### `GetListingAudienceAsync`

Retrieves the list of unique user identifiers who have interacted with a specific listing asynchronously.

- **Parameters**
  - `listingId`: The identifier of the listing to analyze.
- **Return Value**
  - A `Task<IReadOnlyList<Guid>>` containing unique user identifiers who have interacted with the listing.
- **Exceptions**
  - Throws `InvalidOperationException` if the tracker is disposed or in an invalid state.

### `GetSignalsInWindowAsync`

Retrieves user activity signals that occurred within a specified time window asynchronously.

- **Parameters**
  - `startUtc`: The start of the time window (inclusive).
  - `endUtc`: The end of the time window (inclusive).
- **Return Value**
  - A `Task<IReadOnlyList<UserActivitySignal>>` containing signals within the window, ordered chronologically.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `startUtc` is after `endUtc`.
  - Throws `InvalidOperationException` if the tracker is disposed or in an invalid state.

## Usage

### Recording a user activity signal
