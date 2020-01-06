# MessageDto

Data transfer object representing a message within a marketplace conversation, including metadata about the conversation state and participants.

## API

### Properties

- **`Id`** (Guid)
  Unique identifier for the message. Used to reference the message in other operations.

- **`SenderId`** (Guid)
  Identifier of the user who sent the message. Must correspond to an existing user in the system.

- **`RecipientId`** (Guid)
  Identifier of the user who is the intended recipient of the message. Must correspond to an existing user in the system.

- **`Content`** (string)
  The textual content of the message. May be empty, but not null. Length is validated at the service layer.

- **`IsRead`** (bool)
  Indicates whether the message has been read by the recipient. Defaults to `false` when created.

- **`CreatedAt`** (DateTime)
  Timestamp when the message was created. Set automatically by the system and immutable after creation.

- **`ConversationId`** (Guid)
  Unique identifier for the conversation this message belongs to. Used to group messages into threads.

- **`OtherUserId`** (Guid)
  Identifier of the other participant in the conversation (i.e., the user who is not the current viewer). Used to simplify UI logic.

- **`OtherUserName`** (string)
  Display name of the other participant in the conversation. Used for rendering message headers and avatars.

- **`LastMessage`** (string)
  The most recent message content in the conversation. May be truncated or summarized for display purposes.

- **`LastMessageAt`** (DateTime)
  Timestamp of when the last message in the conversation was created. Used to sort conversations.

- **`UnreadCount`** (int)
  Number of unread messages in the conversation from the perspective of the current viewer. Non-negative integer.

### Constructors

- **`MessageDto()`**
  Default constructor. Initializes all properties to default values (e.g., `false` for `IsRead`, `Guid.Empty` for IDs, `DateTime.MinValue` for timestamps).

- **`MessageDto(...)`**
  Parameterized constructor used to hydrate a DTO from a database record or API response. Accepts values for all public properties. No validation is performed in the constructor; validation occurs at the service layer.

## Usage
