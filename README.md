# Marketplace Engine

...

## MessageDtoExtensions

Provides extension methods for `MessageDto` to simplify common operations and validations. Includes methods to verify message participants, get display names, and format message content.

### Usage Example

```csharp
using MarketplaceEngine.DTOs;

// Assuming you have a MessageDto instance
var message = new MessageDto
{
    SenderId = Guid.NewGuid(),
    RecipientId = Guid.NewGuid(),
    Content = "Is this item still available?"
};

// Verify message sender
bool isSentByUser = MessageDtoExtensions.IsSentBy(message, userId: Guid.NewGuid());

// Get display name for other participant
string displayName = MessageDtoExtensions.GetDisplayName(message, currentUserId: Guid.NewGuid());

// Format message content for display
string formattedContent = MessageDtoExtensions.FormatContent(message);

// Verify if message is between two specific users
bool isBetweenUsers = MessageDtoExtensions.IsBetweenUsers(message, userId1: Guid.NewGuid(), userId2: Guid.NewGuid());

Console.WriteLine($"Is sent by user: {isSentByUser}");
Console.WriteLine($"Display name: {displayName}");
Console.WriteLine($"Formatted content: {formattedContent}");
Console.WriteLine($"Is between users: {isBetweenUsers}");
```

## ListingCreatedEventExtensions

...
