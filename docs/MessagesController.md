# MessagesController

The `MessagesController` handles HTTP requests related to user messaging functionality within the marketplace platform. It provides endpoints for retrieving conversations, fetching and sending messages, marking messages as read, and deleting messages. This controller facilitates real-time and asynchronous communication between users, typically in the context of product inquiries, negotiations, or support.

## API

### `GetConversations`
Retrieves a list of conversations for the authenticated user.

**Purpose:**
Returns a paginated or filtered list of conversations associated with the current user, including metadata such as the last message, unread count, and participant details.

**Parameters:**
- None (relies on authentication via request context).

**Returns:**
- `IActionResult` containing a `200 OK` response with a collection of conversation summaries (e.g., `IEnumerable<ConversationSummaryDto>`) on success.
- `401 Unauthorized` if the user is not authenticated.
- `500 Internal Server Error` if an unexpected failure occurs during data retrieval.

**Throws:**
- None explicitly; exceptions are handled internally and returned as `500` responses.

---

### `GetConversationMessages`
Retrieves messages for a specific conversation.

**Purpose:**
Fetches a paginated list of messages within a given conversation, identified by its unique identifier. Supports optional filtering (e.g., by date or message ID).

**Parameters:**
- `conversationId` (`string` or `Guid`): The unique identifier of the conversation.
- `pageNumber` (`int`, optional): The page number for pagination (default: `1`).
- `pageSize` (`int`, optional): The number of messages per page (default: `20`).

**Returns:**
- `IActionResult` containing a `200 OK` response with a paginated list of messages (e.g., `PaginatedList<MessageDto>`) on success.
- `400 Bad Request` if `conversationId` is invalid or missing.
- `401 Unauthorized` if the user is not authenticated.
- `403 Forbidden` if the user lacks permission to access the conversation.
- `404 Not Found` if the conversation does not exist.
- `500 Internal Server Error` for unexpected failures.

**Throws:**
- `ArgumentException` if `conversationId` is malformed (handled internally).

---

### `SendMessage`
Sends a new message in a conversation.

**Purpose:**
Creates and persists a new message in the specified conversation, notifying the recipient(s) asynchronously (e.g., via SignalR or email).

**Parameters:**
- `conversationId` (`string` or `Guid`): The unique identifier of the target conversation.
- `messageRequest` (`SendMessageRequest`): A DTO containing:
  - `Content` (`string`): The message text.
  - `Attachments` (`IEnumerable<AttachmentDto>`, optional): File or media attachments.

**Returns:**
- `IActionResult` containing a `201 Created` response with the newly created message (e.g., `MessageDto`) on success.
- `400 Bad Request` if `conversationId` or `messageRequest` is invalid.
- `401 Unauthorized` if the user is not authenticated.
- `403 Forbidden` if the user lacks permission to send messages in the conversation.
- `404 Not Found` if the conversation does not exist.
- `500 Internal Server Error` for unexpected failures.

**Throws:**
- `ArgumentNullException` if `messageRequest` is `null` (handled internally).
- `ValidationException` if `Content` is empty or exceeds length limits (handled internally).

---

### `GetMessage`
Retrieves a single message by its identifier.

**Purpose:**
Fetches a specific message, including its content, metadata, and attachments, for display or processing.

**Parameters:**
- `messageId` (`string` or `Guid`): The unique identifier of the message.

**Returns:**
- `IActionResult` containing a `200 OK` response with the message (e.g., `MessageDto`) on success.
- `400 Bad Request` if `messageId` is invalid.
- `401 Unauthorized` if the user is not authenticated.
- `403 Forbidden` if the user lacks permission to view the message.
- `404 Not Found` if the message does not exist.
- `500 Internal Server Error` for unexpected failures.

**Throws:**
- `ArgumentException` if `messageId` is malformed (handled internally).

---

### `MarkMessageAsRead`
Marks a message as read by the authenticated user.

**Purpose:**
Updates the read status of a message to reflect that the recipient has viewed it, typically used to clear unread notifications.

**Parameters:**
- `messageId` (`string` or `Guid`): The unique identifier of the message.

**Returns:**
- `IActionResult` containing a `200 OK` response on success.
- `400 Bad Request` if `messageId` is invalid.
- `401 Unauthorized` if the user is not authenticated.
- `403 Forbidden` if the user is not the recipient of the message.
- `404 Not Found` if the message does not exist.
- `500 Internal Server Error` for unexpected failures.

**Throws:**
- `ArgumentException` if `messageId` is malformed (handled internally).

---

### `DeleteMessage`
Deletes a message from a conversation.

**Purpose:**
Removes a message permanently or marks it as deleted (soft delete), depending on business rules. Only the sender or an administrator may delete messages.

**Parameters:**
- `messageId` (`string` or `Guid`): The unique identifier of the message.

**Returns:**
- `IActionResult` containing a `204 No Content` response on success.
- `400 Bad Request` if `messageId` is invalid.
- `401 Unauthorized` if the user is not authenticated.
- `403 Forbidden` if the user lacks permission to delete the message.
- `404 Not Found` if the message does not exist.
- `500 Internal Server Error` for unexpected failures.

**Throws:**
- `ArgumentException` if `messageId` is malformed (handled internally).
- `InvalidOperationException` if the message is already deleted (handled internally).

## Usage

### Example 1: Fetching and Displaying Conversations
