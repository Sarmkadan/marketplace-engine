# MessageDtoExtensions

Provides a set of extension methods for `MessageDto` that simplify common checks and formatting operations related to message direction, participant identification, and display preparation in the marketplace engine.

## API

### IsSentBy
```csharp
public static bool IsSentBy(this MessageDto message, Guid userId)
```
**Purpose** – Determines whether the specified message was sent by the user identified by `userId`.  
**Parameters**  
- `message`: The `MessageDto` instance to evaluate.  
- `userId`: The identifier of the putative sender.  
**Return value** – `true` if `message.SenderId` equals `userId`; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `message` is `null`.

### IsReceivedBy
```csharp
public static bool IsReceivedBy(this MessageDto message, Guid userId)
```
**Purpose** – Determines whether the specified message was received by the user identified by `userId`.  
**Parameters**  
- `message`: The `MessageDto` instance to evaluate.  
- `userId`: The identifier of the putative recipient.  
**Return value** – `true` if `message.RecipientId` equals `userId`; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `message` is `null`.

### GetOtherParticipantId
```csharp
public static Guid GetOtherParticipantId(this MessageDto message, Guid currentUserId)
```
**Purpose** – Returns the identifier of the participant in the conversation that is not the current user.  
**Parameters**  
- `message`: The `MessageDto` instance representing the conversation message.  
- `currentUserId`: The identifier of the user for whom the opposite participant is sought.  
**Return value** – The `Guid` of the other participant; if `currentUserId` matches the sender, returns the recipient id, and vice‑versa.  
**Exceptions** –  
- `ArgumentNullException` if `message` is `null`.  
- `InvalidOperationException` if `currentUserId` does not match either `message.SenderId` or `message.RecipientId`.

### FormatContent
```csharp
public static string FormatContent(this MessageDto message)
```
**Purpose** – Produces a display‑ready string representation of the message’s raw content, applying any necessary escaping or transformation (e.g., HTML encoding, newline handling).  
**Parameters**  
- `message`: The `MessageDto` instance whose content should be formatted.  
**Return value** – A formatted string suitable for rendering in UI contexts.  
**Exceptions** – Throws `ArgumentNullException` if `message` is `null`. Returns an empty string if `message.Content` is `null`.

### IsBetweenUsers
```csharp
public static bool IsBetweenUsers(this MessageDto message, Guid userIdOne, Guid userIdTwo)
```
**Purpose** – Checks whether the message exchanges occur exclusively between the two specified users, regardless of direction.  
**Parameters**  
- `message`: The `MessageDto` instance to evaluate.  
- `userIdOne`: Identifier of the first user.  
- `userIdTwo`: Identifier of the second user.  
**Return value** – `true` if `{message.SenderId, message.RecipientId}` equals `{userIdOne, userIdTwo}`; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `message` is `null`.

### GetDisplayName
```csharp
public static string GetDisplayName(this MessageDto message)
```
**Purpose** – Retrieves the display name associated with the message’s sender (or, if the sender lacks a name, falls back to a generic placeholder).  
**Parameters**  
- `message`: The `MessageDto` instance from which to obtain the display name.  
**Return value** – The sender’s display name as a string; returns `"Unknown"` if the sender’s name cannot be determined.  
**Exceptions** – Throws `ArgumentNullException` if `message` is `null`.

## Usage

```csharp
var msg = await _messageRepository.GetAsync(messageId);

// Show a badge only when the current user sent the message
if (msg.IsSentBy(currentUser.Id))
{
    badge.Visible = true;
    badge.Text = "Sent";
}

// Determine who the other party is for displaying the conversation header
Guid otherId = msg.GetOtherParticipantId(currentUser.Id);
string otherName = _userService.GetDisplayName(otherId);
header.Text = $"Conversation with {otherName}";
```

```csharp
// Validate that a message belongs to a private thread between two users before allowing access
if (!msg.IsBetweenUsers(userA.Id, userB.Id))
{
    throw new UnauthorizedAccessException("Message does not belong to the requested conversation.");
}

// Safely render the message body in a Razor view
<div class="message-body">
    @Html.Raw(msg.FormatContent())
</div>
```

## Notes

- All extension methods are pure; they read only from the supplied `MessageDto` and do not modify any external state. Consequently, they are thread‑safe as long as the `MessageDto` instance itself is not mutated concurrently.  
- Passing `null` for the `message` argument results in an `ArgumentNullException` for every method; callers should ensure the DTO is instantiated before invoking these helpers.  
- `GetOtherParticipantId` assumes the message is part of a two‑person conversation; if the DTO ever represents a group message, the method will throw an `InvalidOperationException` when the supplied `currentUserId` does not match either participant.  
- `FormatContent` does not alter the original `Content` property; it returns a new string. If the original content is `null`, an empty string is returned to avoid propagating null references in UI rendering.  
- The display name logic in `GetDisplayName` relies on the underlying `MessageDto.Sender` (or similar) property; if that relation is not populated, the method returns `"Unknown"` rather than throwing.  
- Because these methods are implemented as static extension members, they are resolved at compile time and incur no allocation overhead beyond the parameters and any string allocations performed inside the methods.
