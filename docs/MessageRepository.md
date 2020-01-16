# MessageRepository

Central data access component for managing messages within the marketplace engine. Provides CRUD operations and specialized queries for message retrieval, conversation handling, and pagination. Designed to work with the `Message` entity and integrates with the underlying data store through asynchronous operations.

## API

### `public MessageRepository`

Constructor for the repository. Initializes the data access infrastructure required for message operations. The exact dependencies and configuration are handled by the dependency injection container.

### `public async Task<Message?> GetByIdAsync(Guid id)`

Retrieves a single message by its unique identifier.

- **Parameters**: `id` – The unique identifier of the message to retrieve.
- **Return value**: A `Message` instance if found; otherwise `null`.
- **Exceptions**: Throws if the identifier is invalid or if a data access error occurs.

### `public async Task<List<Message>> GetAllAsync()`

Retrieves all messages stored in the system.

- **Return value**: A list of all `Message` instances. May be empty.
- **Exceptions**: Throws if a data access error occurs.

### `public async Task<Message> AddAsync(Message message)`

Adds a new message to the data store.

- **Parameters**: `message` – The `Message` instance to add. Must not be `null`.
- **Return value**: The added `Message` instance, typically with generated identifiers populated.
- **Exceptions**: Throws if the message is `null`, invalid, or if a data access error occurs.

### `public async Task<Message> UpdateAsync(Message message)`

Updates an existing message in the data store.

- **Parameters**: `message` – The `Message` instance to update. Must not be `null` and must have a valid identifier.
- **Return value**: The updated `Message` instance.
- **Exceptions**: Throws if the message is `null`, invalid, not found, or if a data access error occurs.

### `public async Task DeleteAsync(Guid id)`

Removes a message from the data store by its unique identifier.

- **Parameters**: `id` – The unique identifier of the message to delete.
- **Exceptions**: Throws if the identifier is invalid or if a data access error occurs.

### `public async Task<bool> ExistsAsync(Guid id)`

Determines whether a message with the specified identifier exists.

- **Parameters**: `id` – The unique identifier to check.
- **Return value**: `true` if the message exists; otherwise `false`.
- **Exceptions**: Throws if the identifier is invalid or if a data access error occurs.

### `public async Task<int> CountAsync()`

Returns the total number of messages stored in the system.

- **Return value**: The count of messages as an integer.
- **Exceptions**: Throws if a data access error occurs.

### `public async Task<List<Message>> GetReceivedMessagesAsync(Guid recipientId)`

Retrieves all messages received by a specific user.

- **Parameters**: `recipientId` – The unique identifier of the recipient user.
- **Return value**: A list of `Message` instances received by the user. May be empty.
- **Exceptions**: Throws if the recipient identifier is invalid or if a data access error occurs.

### `public async Task<List<Message>> GetSentMessagesAsync(Guid senderId)`

Retrieves all messages sent by a specific user.

- **Parameters**: `senderId` – The unique identifier of the sender user.
- **Return value**: A list of `Message` instances sent by the user. May be empty.
- **Exceptions**: Throws if the sender identifier is invalid or if a data access error occurs.

### `public async Task<List<Message>> GetUnreadMessagesAsync(Guid userId)`

Retrieves all unread messages for a specific user.

- **Parameters**: `userId` – The unique identifier of the user.
- **Return value**: A list of unread `Message` instances. May be empty.
- **Exceptions**: Throws if the user identifier is invalid or if a data access error occurs.

### `public async Task<List<Message>> GetConversationAsync(Guid userId1, Guid userId2)`

Retrieves the complete message conversation between two users.

- **Parameters**:
  - `userId1` – The unique identifier of the first user.
  - `userId2` – The unique identifier of the second user.
- **Return value**: A list of `Message` instances representing the conversation. Order is not guaranteed unless specified by the caller.
- **Exceptions**: Throws if either identifier is invalid or if a data access error occurs.

### `public async Task<List<Message>> GetByListingIdAsync(Guid listingId)`

Retrieves all messages associated with a specific listing.

- **Parameters**: `listingId` – The unique identifier of the listing.
- **Return value**: A list of `Message` instances related to the listing. May be empty.
- **Exceptions**: Throws if the listing identifier is invalid or if a data access error occurs.

### `public async Task<List<Message>> GetConversationAboutListingAsync(Guid userId, Guid listingId)`

Retrieves messages sent by a specific user about a specific listing.

- **Parameters**:
  - `userId` – The unique identifier of the user.
  - `listingId` – The unique identifier of the listing.
- **Return value**: A list of `Message` instances. May be empty.
- **Exceptions**: Throws if either identifier is invalid or if a data access error occurs.

### `public async Task<List<Message>> GetFlaggedMessagesAsync()`

Retrieves all messages that have been flagged for review.

- **Return value**: A list of flagged `Message` instances. May be empty.
- **Exceptions**: Throws if a data access error occurs.

### `public async Task<int> GetConversationCountAsync(Guid userId1, Guid userId2)`

Returns the total number of messages exchanged between two users.

- **Parameters**:
  - `userId1` – The unique identifier of the first user.
  - `userId2` – The unique identifier of the second user.
- **Return value**: The count of messages as an integer.
- **Exceptions**: Throws if either identifier is invalid or if a data access error occurs.

### `public async Task<(List<Message> items, int total)> GetPagedAsync(int pageNumber, int pageSize)`

Retrieves a paged subset of messages along with the total count.

- **Parameters**:
  - `pageNumber` – The zero-based page number to retrieve.
  - `pageSize` – The maximum number of items per page.
- **Return value**: A tuple containing the list of `Message` instances for the page and the total count of messages.
- **Exceptions**: Throws if `pageNumber` or `pageSize` are invalid or if a data access error occurs.

### `public async Task<(List<Message> items, Guid? nextCursor)> GetPagedByCursorAsync(Guid? cursor, int pageSize)`

Retrieves a paged subset of messages using cursor-based pagination.

- **Parameters**:
  - `cursor` – The cursor identifier to start from (exclusive). Use `null` for the first page.
  - `pageSize` – The maximum number of items per page.
- **Return value**: A tuple containing the list of `Message` instances for the page and the next cursor (if available).
- **Exceptions**: Throws if `pageSize` is invalid or if a data access error occurs.

### `public async Task MarkAsReadAsync(Guid messageId)`

Marks a specific message as read by updating its read status.

- **Parameters**: `messageId` – The unique identifier of the message to mark as read.
- **Exceptions**: Throws if the identifier is invalid or if a data access error occurs.

### `public async Task<List<Message>> GetOldMessagesAsync(int daysOld)`

Retrieves messages older than a specified number of days.

- **Parameters**: `daysOld` – The number of days to consider messages as old.
- **Return value**: A list of `Message` instances older than the specified age. May be empty.
- **Exceptions**: Throws if `daysOld` is negative or if a data access error occurs.

## Usage
