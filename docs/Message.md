# Message
The `Message` type in the `marketplace-engine` project represents a communication between two users, typically related to a listing. It encapsulates the message's metadata, content, and state, providing a structured way to manage and interact with messages within the marketplace.

## API
### Properties
* `Id`: A unique identifier for the message, represented as a `Guid`.
* `SenderId`: The identifier of the user who sent the message, represented as a `Guid`.
* `Sender`: The user who sent the message, represented as a `User` object, which may be null.
* `RecipientId`: The identifier of the user who received the message, represented as a `Guid`.
* `Recipient`: The user who received the message, represented as a `User` object, which may be null.
* `ListingId`: The identifier of the listing related to the message, represented as a `Guid?`, which may be null.
* `Listing`: The listing related to the message, represented as a `Listing` object, which may be null.
* `Subject`: The subject of the message, represented as a string.
* `Body`: The content of the message, represented as a string.
* `IsRead`: A boolean indicating whether the message has been read.
* `IsFlagged`: A boolean indicating whether the message has been flagged.
* `AttachmentUrls`: A list of URLs for attachments related to the message, represented as a `List<string>`.
* `CreatedAt`: The date and time when the message was created, represented as a `DateTime`.
* `ReadAt`: The date and time when the message was read, represented as a `DateTime?`, which may be null.
* `ParentMessageId`: The identifier of the parent message, represented as a `Guid?`, which may be null.
* `ParentMessage`: The parent message, represented as a `Message` object, which may be null.
* `Replies`: A list of replies to the message, represented as a `List<Message>`.

### Methods
* `ValidateBeforeSending()`: Validates the message before it is sent. This method does not return a value and does not throw any exceptions based on its signature, but its implementation details are not provided.
* `MarkAsRead()`: Marks the message as read. This method does not return a value and does not throw any exceptions based on its signature, but its implementation details are not provided.
* `MarkAsUnread()`: Marks the message as unread. This method does not return a value and does not throw any exceptions based on its signature, but its implementation details are not provided.

## Usage
The following examples demonstrate how to use the `Message` type:
```csharp
// Example 1: Creating a new message
var message = new Message
{
    SenderId = Guid.NewGuid(),
    RecipientId = Guid.NewGuid(),
    Subject = "Example Message",
    Body = "This is an example message.",
    AttachmentUrls = new List<string> { "https://example.com/attachment1", "https://example.com/attachment2" }
};
message.ValidateBeforeSending();

// Example 2: Marking a message as read
var existingMessage = GetMessageFromDatabase(); // Assume GetMessageFromDatabase() retrieves a message from the database
existingMessage.MarkAsRead();
```

## Notes
When working with the `Message` type, consider the following:
* The `ListingId` and `Listing` properties may be null if the message is not related to a listing.
* The `ReadAt` property may be null if the message has not been read.
* The `ParentMessageId` and `ParentMessage` properties may be null if the message does not have a parent message.
* The `Replies` list may be empty if the message does not have any replies.
* The `ValidateBeforeSending`, `MarkAsRead`, and `MarkAsUnread` methods do not throw exceptions based on their signatures, but their implementation details may include error handling.
* The `Message` type does not appear to be thread-safe based on its public members. If multiple threads access and modify the same `Message` instance, it may lead to inconsistent state or other concurrency issues.
