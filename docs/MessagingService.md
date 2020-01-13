# MessagingService

The `MessagingService` class provides the core messaging functionality for the marketplace engine. It handles sending, receiving, flagging, and managing messages between users, as well as retrieving conversations, paginated message lists, and performing cleanup of old messages. All operations are asynchronous and designed to work with a persistent message store.

## API

### `public MessagingService()`

Initializes a new instance of the `MessagingService` class. In a production environment, this constructor would typically accept dependencies such as a message repository or a database context, but the default constructor is provided for basic instantiation.

### `public async Task<Message> SendMessageAsync(…)`

Sends a new message from one user to another. The method accepts parameters that identify the sender, recipient, and message content (subject, body, and optional parent message ID for replies).  
**Returns:** The created `Message` object with a unique identifier and timestamp.  
**Throws:** `ArgumentNullException` if required parameters are null or empty; `InvalidOperationException` if the sender or recipient does not exist.

### `public async Task<List<Message>> GetReceivedMessagesAsync(…)`

Retrieves all messages received by a specified user.  
**Parameters:** User identifier.  
**Returns:** A list of `Message` objects representing received messages, ordered by most recent first.  
**Throws:** `ArgumentNullException` if the user identifier is invalid.

### `public async Task<List<Message>> GetSentMessagesAsync(…)`

Retrieves all messages sent by a specified user.  
**Parameters:** User identifier.  
**Returns:** A list of `Message` objects representing sent messages, ordered by most recent first.  
**Throws:** `ArgumentNullException` if the user identifier is invalid.

### `public async Task<List<Message>> GetUnreadMessagesAsync(…)`

Retrieves all unread messages for a specified user.  
**Parameters:** User identifier.  
**Returns:** A list of `Message` objects that have not been marked as read.  
**Throws:** `ArgumentNullException` if the user identifier is invalid.

### `public async Task<List<Message>> GetConversationAsync(…)`

Retrieves the full conversation thread between two users.  
**Parameters:** Identifiers for both participants.  
**Returns:** A list of `Message` objects in chronological order, including all replies.  
**Throws:** `ArgumentNullException` if either user identifier is missing.

### `public async Task<Message> MarkAsReadAsync(…)`

Marks a single message as read by the recipient.  
**Parameters:** Message identifier.  
**Returns:** The updated `Message` object with the read status set to `true`.  
**Throws:** `KeyNotFoundException` if the message does not exist.

### `public async Task MarkMultipleAsReadAsync(…)`

Marks multiple messages as read in a single operation.  
**Parameters:** A collection of message identifiers.  
**Returns:** No return value.  
**Throws:** `ArgumentNullException` if the collection is null; `KeyNotFoundException` if any message does not exist.

### `public async Task<Message> MarkAsUnreadAsync(…)`

Marks a previously read message as unread.  
**Parameters:** Message identifier.  
**Returns:** The updated `Message` object with the read status set to `false`.  
**Throws:** `KeyNotFoundException` if the message does not exist.

### `public async Task<Message> FlagMessageAsync(…)`

Flags a message for follow-up or review.  
**Parameters:** Message identifier.  
**Returns:** The updated `Message` object with the flag status set to `true`.  
**Throws:** `KeyNotFoundException` if the message does not exist.

### `public async Task<Message> RemoveFlagAsync(…)`

Removes the flag from a previously flagged message.  
**Parameters:** Message identifier.  
**Returns:** The updated `Message` object with the flag status set to `false`.  
**Throws:** `KeyNotFoundException` if the message does not exist.

### `public async Task<Message> AddReplyAsync(…)**

Adds a reply to an existing message, creating a threaded conversation.  
**Parameters:** The parent message identifier and the reply content (sender, body).  
**Returns:** The newly created reply `Message` object.  
**Throws:** `ArgumentNullException` if reply content is invalid; `KeyNotFoundException` if the parent message does not exist.

### `public async Task<List<Message>> GetListingMessagesAsync(…)`

Retrieves all messages associated with a specific marketplace listing.  
**Parameters:** Listing identifier.  
**Returns:** A list of `Message` objects related to the listing.  
**Throws:** `ArgumentNullException` if the listing identifier is invalid.

### `public async Task<List<Message>> GetListingConversationAsync(…)`

Retrieves the full conversation thread for a specific listing, including all participants.  
**Parameters:** Listing identifier.  
**Returns:** A list of `Message` objects in chronological order.  
**Throws:** `ArgumentNullException` if the listing identifier is invalid.

### `public async Task<(List<Message> items, int total)> GetPaginatedMessagesAsync(…)`

Retrieves a paginated subset of messages for a user, along with the total count.  
**Parameters:** User identifier, page number, page size, and optional filters (e.g., unread only).  
**Returns:** A tuple containing the list of `Message` objects for the requested page and the total number of matching messages.  
**Throws:** `ArgumentOutOfRangeException` if page number or page size is less than 1.

### `public async Task<(List<Message> items, Guid? nextCursor)> GetMessagesByCursorAsync(…)`

Retrieves messages using cursor-based pagination for efficient large-scale queries.  
**Parameters:** User identifier, cursor (optional), page size, and optional filters.  
**Returns:** A tuple containing the list of `Message` objects and a `nextCursor` value (or null if no more pages).  
**Throws:** `ArgumentOutOfRangeException` if page size is invalid.

### `public async Task<int> GetConversationCountAsync(…)`

Returns the total number of distinct conversations (threads) for a user.  
**Parameters:** User identifier.  
**Returns:** An integer count of conversations.  
**Throws:** `ArgumentNullException` if the user identifier is invalid.

### `public async Task DeleteMessageAsync(…)**

Permanently deletes a message from the store.  
**Parameters:** Message identifier.  
**Returns:** No return value.  
**Throws:** `KeyNotFoundException` if the message does not exist.

### `public async Task<List<Message>> GetFlaggedMessagesAsync(…)**

Retrieves all messages that have been flagged by a user.  
**Parameters:** User identifier.  
**Returns:** A list of `Message` objects with the flag status set to `true`.  
**Throws:** `ArgumentNullException` if the user identifier is invalid.

### `public async Task CleanupOldMessagesAsync(…)**

Removes messages older than a specified retention period.  
**Parameters:** Retention threshold (e.g., a `DateTime` or `TimeSpan`).  
**Returns:** No return value.  
**Throws:** `ArgumentOutOfRangeException` if the retention period is negative.

## Usage

### Example 1: Sending a message and marking it as read

```csharp
var messagingService = new MessagingService();

// Send a message from user A to user B
var sentMessage = await messagingService.SendMessageAsync(
    senderId: "user-a",
    recipientId: "user-b",
    subject: "Question about your listing",
    body: "Is the item still available?"
);

// Later, user B marks the message as read
var readMessage = await messagingService.MarkAsReadAsync(sentMessage.Id);
Console.WriteLine($"Message {readMessage.Id} marked as read: {readMessage.IsRead}");
```

### Example 2: Retrieving paginated messages with cursor-based pagination

```csharp
var messagingService = new MessagingService();
Guid? cursor = null;
const int pageSize = 20;

do
{
    var (messages, nextCursor) = await messagingService.GetMessagesByCursorAsync(
        userId: "user-a",
        cursor: cursor,
        pageSize: pageSize
    );

    foreach (var msg in messages)
    {
        Console.WriteLine($"{msg.Timestamp}: {msg.Subject}");
    }

    cursor = nextCursor;
} while (cursor.HasValue);
```

## Notes

- **Thread safety:** `MessagingService` is not inherently thread-safe. Concurrent calls to methods that modify the same message (e.g., `MarkAsReadAsync` and `DeleteMessageAsync`) may result in race conditions or inconsistent state. Use external synchronization (e.g., a lock or database transaction) when performing such operations in parallel.
- **Edge cases:**
  - Sending a message to a non-existent user may throw an `InvalidOperationException`. Always validate user identifiers before calling `SendMessageAsync`.
  - `GetConversationAsync` returns an empty list if no messages exist between the two users.
  - `DeleteMessageAsync` permanently removes the message; there is no undo.
  - `CleanupOldMessagesAsync` should be called periodically to prevent unbounded storage growth. The retention threshold must be a positive duration.
- **Performance:** Methods that return large result sets (e.g., `GetReceivedMessagesAsync` without pagination) may be slow for users with thousands of messages. Prefer `GetPaginatedMessagesAsync` or `GetMessagesByCursorAsync` for production use.
- **Dependency injection:** In a typical application, the `MessagingService` is registered as a scoped service and injected into controllers or other services. The default constructor is provided for testing or simple scenarios; for production use, ensure the service is configured with a concrete message store.
