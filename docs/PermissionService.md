# PermissionService

The `PermissionService` provides a centralized mechanism for evaluating authorization rules across the marketplace engine. It encapsulates business logic to determine whether a user is authorized to perform specific actions—such as listing management, moderation, or messaging—based on the user's roles and the context of the requested resource.

## API

### PermissionService()
Initializes a new instance of the `PermissionService` class.

### bool HasRole(string role)
Checks if the current user is assigned a specific role.
- **Parameters:** `role` (string) - The name of the role to check.
- **Returns:** `true` if the user has the specified role; otherwise, `false`.
- **Throws:** `ArgumentNullException` if `role` is null or empty.

### bool CanEditListing(Listing listing)
Determines if the current user has permission to modify the given listing.
- **Parameters:** `listing` (Listing) - The listing intended for modification.
- **Returns:** `true` if modification is permitted; otherwise, `false`.
- **Throws:** `ArgumentNullException` if `listing` is null.

### bool CanDeleteListing(Listing listing)
Determines if the current user has permission to delete the given listing.
- **Parameters:** `listing` (Listing) - The listing intended for deletion.
- **Returns:** `true` if deletion is permitted; otherwise, `false`.
- **Throws:** `ArgumentNullException` if `listing` is null.

### bool CanModerate(Content content)
Checks if the user has sufficient privileges to perform moderation actions on content.
- **Parameters:** `content` (Content) - The content targeted for moderation.
- **Returns:** `true` if moderation is permitted; otherwise, `false`.
- **Throws:** `ArgumentNullException` if `content` is null.

### bool CanCreateListing()
Checks if the current user is authorized to create a new marketplace listing.
- **Returns:** `true` if listing creation is permitted; otherwise, `false`.

### bool CanMessage(User recipient)
Checks if the current user is authorized to send a message to the specified recipient.
- **Parameters:** `recipient` (User) - The user to whom the message would be sent.
- **Returns:** `true` if messaging is permitted; otherwise, `false`.
- **Throws:** `ArgumentNullException` if `recipient` is null.

### bool CanSubmitReport(Reportable item)
Checks if the current user can report a specific item or content.
- **Parameters:** `item` (Reportable) - The item or content to be reported.
- **Returns:** `true` if submission is permitted; otherwise, `false`.
- **Throws:** `ArgumentNullException` if `item` is null.

## Usage

```csharp
// Example 1: Checking permission before editing a listing
if (permissionService.CanEditListing(targetListing))
{
    // Proceed with editing
    UpdateListing(targetListing);
}
else
{
    throw new UnauthorizedAccessException("You do not have permission to edit this listing.");
}
```

```csharp
// Example 2: Administrative check for moderation
if (permissionService.HasRole("Moderator") && permissionService.CanModerate(reportedContent))
{
    // Perform moderation action
    PerformModerationAction(reportedContent);
}
```

## Notes

- **Thread-Safety:** The `PermissionService` implementation is thread-safe for read operations. Concurrent modifications to the underlying user context should be managed by the calling application.
- **Performance:** Permission checks are intended to be lightweight. While some calls may involve database lookups for complex rules, frequent calls should rely on caching layers configured within the service.
- **Edge Cases:** Methods accepting entities (e.g., `Listing`, `User`, `Content`) validate inputs strictly; providing `null` references will result in an `ArgumentNullException`. Authorization status should be re-evaluated if the user's role or the target resource state changes during the request lifecycle.
