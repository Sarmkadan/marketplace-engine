# MessagingServiceExtensions

The `MessagingServiceExtensions` class provides a set of static asynchronous extension methods designed to simplify common messaging operations within the `marketplace-engine` ecosystem. By encapsulating complex query logic and context resolution, these methods offer a streamlined interface for sending messages with full context, retrieving complete conversation histories, counting unread items, and filtering messages by specific marketplace listings, thereby reducing boilerplate code in consumer services.

## API

### SendMessageWithContextAsync
Asynchronously sends a new message while automatically resolving and attaching the necessary contextual metadata (such as listing details or user roles) required by the marketplace logic.
*   **Parameters**: Accepts the target service instance, the message content payload, and identifiers for the sender, recipient, and associated listing.
*   **Return Value**: Returns a `Task<Message>` containing the persisted message entity, including generated IDs and timestamps.
*   **Exceptions**: Throws an exception if the specified listing does not exist, if the sender or recipient IDs are invalid, or if the underlying database transaction fails.

### GetCompleteConversationAsync
Retrieves the entire chronological history of messages exchanged between two specific parties regarding a particular context or listing.
*   **Parameters**: Requires the service instance, the two participant user IDs, and an optional listing identifier to scope the conversation.
*   **Return Value**: Returns a `Task<List<Message>>` containing an ordered list of message objects from oldest to newest.
*   **Exceptions**: Throws an exception if one or both user IDs are not found or if the data store is inaccessible.

### GetUnreadMessageCountAsync
Calculates the total number of messages received by a specific user that have not yet been marked as read.
*   **Parameters**: Takes the service instance and the target user ID.
*   **Return Value**: Returns a `Task<int>` representing the count of unread messages.
*   **Exceptions**: Throws an exception if the provided user ID is invalid or null.

### GetMessagesByListingAsync
Fetches all messages associated with a specific marketplace listing, regardless of the participants involved, useful for moderation or administrative oversight.
*   **Parameters**: Requires the service instance and the unique identifier of the listing.
*   **Return Value**: Returns a `Task<List<Message>>` containing all messages linked to the specified listing.
*   **Exceptions**: Throws an exception if the listing ID does not correspond to an existing record.

## Usage

### Sending a Message with Context
The following example demonstrates how to send a message between a buyer and a seller regarding a specific item, ensuring all contextual links are established automatically.

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Models;

public async Task NotifySellerAsync(IMessagingService service, Guid listingId, Guid buyerId, Guid sellerId)
{
    var payload = new MessageContent 
    { 
        Body = "Is this item still available?", 
        Timestamp = DateTime.UtcNow 
    };

    // The extension method resolves listing context and validates participants
    Message sentMessage = await service.SendMessageWithContextAsync(
        payload, 
        buyerId, 
        sellerId, 
        listingId
    );

    Console.WriteLine($"Message sent with ID: {sentMessage.Id}");
}
```

### Retrieving Conversation History and Unread Counts
This example illustrates fetching a full conversation thread for a UI display and simultaneously checking for new unread notifications.

```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Models;

public async Task LoadChatDataAsync(IMessagingService service, Guid currentUserId, Guid otherUserId, Guid listingId)
{
    // Retrieve the full history for the chat window
    List<Message> conversation = await service.GetCompleteConversationAsync(
        currentUserId, 
        otherUserId, 
        listingId
    );

    // Check for unread count to update the notification badge
    int unreadCount = await service.GetUnreadMessageCountAsync(currentUserId);

    // Process data for view model
    var viewModel = new ChatViewModel 
    {
        Messages = conversation,
        UnreadIndicator = unreadCount > 0
    };
}
```

## Notes

*   **Thread Safety**: As all exposed members are static extension methods operating on stateless logic and relying on the thread safety of the underlying `IMessagingService` implementation and database context, these methods are safe for concurrent invocation from multiple threads.
*   **Data Consistency**: `GetCompleteConversationAsync` and `GetMessagesByListingAsync` return snapshots of data at the time of execution. In high-concurrency scenarios, the returned list may not reflect messages inserted by other transactions immediately after the query completes.
*   **Null Handling**: While the methods handle internal validation, passing `null` for required GUID parameters (such as User IDs or Listing IDs) will typically result in an immediate argument exception before any database call is made.
*   **Performance**: `GetUnreadMessageCountAsync` is optimized for aggregation and should be preferred over retrieving the full list of messages and counting them client-side, especially for users with large message histories.
