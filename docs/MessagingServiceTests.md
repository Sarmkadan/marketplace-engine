# MessagingServiceTests

The `MessagingServiceTests` class serves as the comprehensive test suite for the messaging functionality within the `marketplace-engine` project. It validates the core business logic of the messaging service by verifying correct behavior under valid conditions, ensuring appropriate exception handling for invalid inputs or missing resources, and enforcing security constraints regarding message access and modification. Each method within this class represents a specific test case designed to assert the reliability and integrity of message transmission, retrieval, status updates, and conversation management.

## API

### `MessagingServiceTests`
Initializes a new instance of the test class. This constructor sets up the necessary context, mocks, or dependencies required to execute the subsequent test methods against the messaging service.

### `SendMessageAsync_WhenSenderNotFound_ThrowsResourceNotFoundException`
Verifies that the service correctly rejects a message sending attempt when the specified sender identity does not exist in the system.
*   **Parameters**: None (test context is internal).
*   **Return Value**: `Task` representing the asynchronous operation.
*   **Throws**: Expects a `ResourceNotFoundException` if the sender cannot be resolved.

### `SendMessageAsync_WhenRecipientNotFound_ThrowsResourceNotFoundException`
Verifies that the service correctly rejects a message sending attempt when the specified recipient identity does not exist in the system.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Throws**: Expects a `ResourceNotFoundException` if the recipient cannot be resolved.

### `SendMessageAsync_WithValidData_ReturnsCreatedMessage`
Validates the successful creation and persistence of a message when both sender and recipient are valid and distinct.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Behavior**: Asserts that the returned object matches the expected created message structure.

### `SendMessageAsync_WithSenderEqualToRecipient_ThrowsArgumentException`
Ensures that the system prevents a user from sending a message to themselves, enforcing logical constraints on communication.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Throws**: Expects an `ArgumentException` when the sender ID matches the recipient ID.

### `GetReceivedMessagesAsync_WhenUserNotFound_ThrowsResourceNotFoundException`
Tests the retrieval of incoming messages for a user ID that does not exist in the database.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Throws**: Expects a `ResourceNotFoundException`.

### `GetReceivedMessagesAsync_WhenUserExists_ReturnsMessages`
Confirms that the service returns a collection of messages for a valid user, handling both empty and populated inboxes correctly.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Behavior**: Asserts the returned collection contains the expected messages associated with the user.

### `MarkAsReadAsync_WhenMessageNotFound_ThrowsResourceNotFoundException`
Verifies that attempting to mark a non-existent message as read results in a specific resource error.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Throws**: Expects a `ResourceNotFoundException`.

### `MarkAsReadAsync_WhenMessageExists_MarksMessageAsRead`
Validates the state transition of a message from unread to read when the message ID is valid.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Behavior**: Asserts that the `IsRead` property of the message is updated to `true`.

### `DeleteMessageAsync_WhenRequesterIsSender_DeletesSuccessfully`
Ensures that the original sender of a message has the authorization to delete their own sent message.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Behavior**: Asserts successful completion without exceptions and verifies message removal or soft-delete status.

### `DeleteMessageAsync_WhenRequesterIsRecipient_DeletesSuccessfully`
Ensures that the recipient of a message has the authorization to delete the message from their own inbox.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Behavior**: Asserts successful completion without exceptions.

### `DeleteMessageAsync_WhenRequesterIsUnrelated_ThrowsUnauthorizedException`
Validates security constraints by ensuring that a user who is neither the sender nor the recipient cannot delete the message.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Throws**: Expects an `UnauthorizedException`.

### `FlagMessageAsync_WhenMessageExists_SetsIsFlaggedTrue`
Tests the functionality of marking a message as flagged for review or attention.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Behavior**: Asserts that the `IsFlagged` property is set to `true`.

### `FlagMessageAsync_WhenFlaggerNotFound_ThrowsResourceNotFoundException`
Verifies that the flagging operation fails appropriately if the user attempting to flag the message does not exist.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Throws**: Expects a `ResourceNotFoundException`.

### `AddReplyAsync_WithValidData_CreatesReplyWithPrefixedSubject`
Validates the creation of a reply message, specifically ensuring that the subject line is automatically prefixed (e.g., "Re: ") according to business rules.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Behavior**: Asserts the new message is linked to the parent and the subject format is correct.

### `AddReplyAsync_WhenParentNotFound_ThrowsResourceNotFoundException`
Ensures that a reply cannot be created if the original parent message it references does not exist.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Throws**: Expects a `ResourceNotFoundException`.

### `GetConversationAsync_WhenFirstUserNotFound_ThrowsResourceNotFoundException`
Tests the retrieval of a conversation thread, verifying that the operation fails if the first user specified in the query does not exist.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Throws**: Expects a `ResourceNotFoundException`.

### `MarkAsUnreadAsync_WhenMessageWasRead_SetsIsReadFalse`
Validates the reverse state transition, ensuring a previously read message can be marked back as unread.
*   **Parameters**: None.
*   **Return Value**: `Task`.
*   **Behavior**: Asserts that the `IsRead` property is updated to `false`.

## Usage

The following examples demonstrate how the test methods are typically invoked within a test runner framework like xUnit or NUnit. These tests rely on the internal setup of the `MessagingServiceTests` class to initialize mocks and dependencies.

```csharp
// Example 1: Running a validation test for successful message creation
[TestClass]
public class MessagingIntegration
{
    [TestMethod]
    public async Task Validate_Message_Creation_Flow()
    {
        var testSuite = new MessagingServiceTests();
        
        // Executes the test logic verifying valid data returns a created message
        await testSuite.SendMessageAsync_WithValidData_ReturnsCreatedMessage();
        
        // If no exception is thrown, the assertion within the test method passed
        Console.WriteLine("Message creation validation passed.");
    }
}
```

```csharp
// Example 2: Running a security constraint test for unauthorized deletion
[TestClass]
public class SecurityValidation
{
    [TestMethod]
    [ExpectedException(typeof(UnauthorizedException))]
    public async Task Validate_Unauthorized_Delete_Protection()
    {
        var testSuite = new MessagingServiceTests();
        
        // Executes the test logic ensuring unrelated users cannot delete messages
        // The test method internally asserts that UnauthorizedException is thrown
        await testSuite.DeleteMessageAsync_WhenRequesterIsUnrelated_ThrowsUnauthorizedException();
    }
}
```

## Notes

*   **Exception Specificity**: The test suite strictly differentiates between `ResourceNotFoundException` (for missing entities like users or messages) and `ArgumentException` (for logical inconsistencies like self-messaging). Implementations relying on these tests must ensure the correct exception type is thrown to avoid test failures.
*   **Authorization Boundaries**: The `DeleteMessageAsync` tests highlight a dual-ownership model where both the sender and recipient have deletion rights, while explicitly excluding third parties. Any changes to the underlying service must maintain this specific access control list.
*   **Thread Safety**: As all test methods are asynchronous (`async Task`), the underlying `MessagingService` implementation is expected to handle concurrent requests safely. While the tests themselves usually run sequentially in a standard test runner, the patterns verified (such as state changes in `MarkAsReadAsync` or `FlagMessageAsync`) imply that the service should utilize appropriate locking or atomic operations when deployed in a multi-threaded environment.
*   **Data Integrity**: The `AddReplyAsync` test enforces a specific business rule regarding subject line formatting (prefixing). This is a critical UI/UX contract that must be preserved in the service logic.
*   **State Dependencies**: Tests involving state transitions (e.g., `MarkAsUnreadAsync_WhenMessageWasRead_SetsIsReadFalse`) assume the message is in a specific prior state. The test setup must correctly initialize the message as "read" before invoking the method to ensure the assertion is valid.
