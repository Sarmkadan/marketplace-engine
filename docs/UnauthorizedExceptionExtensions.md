# UnauthorizedExceptionExtensions

The `UnauthorizedExceptionExtensions` class provides a set of static utility methods designed to extend the functionality of `UnauthorizedException` instances within the `marketplace-engine` project. These helpers facilitate consistent logging, user context validation, action description retrieval, and conditional exception throwing, ensuring standardized error handling and security checks across the application.

## API

### `ToLogMessage`
Generates a formatted string suitable for logging purposes based on the exception details.
*   **Parameters**: `UnauthorizedException exception` – The exception instance to process.
*   **Return Value**: `string` – A descriptive message containing relevant context from the exception.
*   **Throws**: `ArgumentNullException` if the provided `exception` is null.

### `IsForUser`
Determines whether the unauthorized exception specifically pertains to a user context mismatch or validation failure.
*   **Parameters**: `UnauthorizedException exception` – The exception instance to evaluate.
*   **Return Value**: `bool` – `true` if the exception indicates a user-specific authorization failure; otherwise, `false`.
*   **Throws**: `ArgumentNullException` if the provided `exception` is null.

### `GetActionDescription`
Retrieves a human-readable description of the action that was attempted when the authorization failure occurred.
*   **Parameters**: `UnauthorizedException exception` – The exception instance containing action metadata.
*   **Return Value**: `string` – The description of the blocked action. Returns an empty string if no description is available.
*   **Throws**: `ArgumentNullException` if the provided `exception` is null.

### `ThrowIfUserMismatch`
Validates that the current user context matches the expected context defined within the exception data; if a mismatch is detected, it re-throws or throws a new exception.
*   **Parameters**: 
    *   `UnauthorizedException exception` – The exception containing the expected user context.
    *   `string currentUserId` – The identifier of the currently authenticated user.
*   **Return Value**: `void`
*   **Throws**: `UnauthorizedException` if the `currentUserId` does not match the user context associated with the `exception`. Throws `ArgumentNullException` if `exception` or `currentUserId` is null.

## Usage

### Example 1: Logging and Action Inspection
This example demonstrates how to extract diagnostic information from a caught exception for logging and audit trails.

```csharp
try
{
    await marketplaceService.TransferOwnershipAsync(itemId, targetUserId);
}
catch (UnauthorizedException ex)
{
    // Log the standardized message
    logger.LogWarning(UnauthorizedExceptionExtensions.ToLogMessage(ex));

    // Check if this was a user-specific issue
    if (UnauthorizedExceptionExtensions.IsForUser(ex))
    {
        var action = UnauthorizedExceptionExtensions.GetActionDescription(ex);
        auditService.RecordFailure(currentUser.Id, $"Blocked action: {action}");
    }
    
    throw;
}
```

### Example 2: Enforcing User Context Consistency
This example shows how to enforce strict user matching before proceeding with sensitive operations using the helper method.

```csharp
public async Task ProcessRefundAsync(string refundId, UnauthorizedException validationEx, string currentUserId)
{
    // Ensure the exception context aligns with the current user before proceeding or failing
    UnauthorizedExceptionExtensions.ThrowIfUserMismatch(validationEx, currentUserId);

    // If no exception was thrown, the user context is valid for this operation
    await refundProcessor.ExecuteAsync(refundId);
}
```

## Notes

*   **Null Safety**: All methods in this class expect non-null arguments for the `UnauthorizedException` parameter. Passing `null` will result in an `ArgumentNullException`. Callers should ensure exceptions are caught and validated before passing them to these extensions.
*   **Thread Safety**: As this class consists entirely of static methods that operate solely on provided input parameters without maintaining internal mutable state, it is thread-safe and can be used concurrently across multiple threads.
*   **Exception Flow**: The `ThrowIfUserMismatch` method is designed to interrupt execution flow immediately upon detecting a discrepancy. It should be called early in a method body where user context validation is a prerequisite for subsequent logic.
*   **Return Values**: `GetActionDescription` may return an empty string if the underlying exception does not contain specific action metadata; callers should handle this possibility gracefully rather than assuming a populated string.
