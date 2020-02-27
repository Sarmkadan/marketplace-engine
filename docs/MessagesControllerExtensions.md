# MessagesControllerExtensions
The `MessagesControllerExtensions` class provides a set of extension methods for managing messages in the marketplace-engine project. These methods enable asynchronous operations for retrieving unread message counts, marking conversations as read, sending batches of messages, and searching messages. This class is designed to simplify the process of integrating messaging functionality into applications.

## API
The `MessagesControllerExtensions` class includes the following public members:
* `GetUnreadMessageCounts`: Retrieves the count of unread messages. This method returns an `IActionResult` and does not take any parameters. It throws an exception if an error occurs during the retrieval process.
* `MarkConversationAsRead`: Marks a conversation as read. This method returns an `IActionResult` and does not take any parameters. It throws an exception if an error occurs during the marking process.
* `SendMessageBatch`: Sends a batch of messages. This method returns an `IActionResult` and does not take any parameters. It throws an exception if an error occurs during the sending process.
* `SearchMessages`: Searches for messages based on specified criteria. This method returns an `IActionResult` and does not take any parameters. It throws an exception if an error occurs during the search process.

## Usage
Here are two examples of using the `MessagesControllerExtensions` class:
```csharp
// Example 1: Retrieving unread message counts
var unreadMessageCounts = await MessagesControllerExtensions.GetUnreadMessageCounts();
// Process the unread message counts

// Example 2: Sending a batch of messages
var sendMessageBatchResult = await MessagesControllerExtensions.SendMessageBatch();
// Handle the result of sending the batch of messages
```

## Notes
When using the `MessagesControllerExtensions` class, consider the following edge cases and thread-safety remarks:
* The `GetUnreadMessageCounts`, `MarkConversationAsRead`, `SendMessageBatch`, and `SearchMessages` methods are asynchronous, which means they do not block the calling thread. However, they may still throw exceptions if errors occur during their execution.
* Since these methods are static, they are thread-safe as long as the underlying messaging system is thread-safe. However, if the messaging system is not thread-safe, concurrent calls to these methods may result in unexpected behavior.
* The `IActionResult` return type of these methods indicates that they may return different types of results, such as `OkResult`, `NotFoundResult`, or `BadRequestResult`, depending on the outcome of the operation. It is essential to handle these different result types accordingly in the calling code.
