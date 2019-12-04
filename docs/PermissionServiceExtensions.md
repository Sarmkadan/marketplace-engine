# PermissionServiceExtensions

Provides static convenience properties for evaluating the current user's permissions within the marketplace engine. These properties encapsulate common authorization checks and return a Boolean indicating whether the associated permission is granted.

## API

### IsAdministrator
- **Purpose**: Determines whether the current user holds an administrator role.
- **Parameters**: None.
- **Return value**: `true` if the user is an administrator; otherwise `false`.
- **Exceptions**: May throw `InvalidOperationException` if the underlying permission service has not been initialized.

### CanEditAnyListing
- **Purpose**: Indicates whether the current user is allowed to edit any listing, regardless of ownership.
- **Parameters**: None.
- **Return value**: `true` if the user can edit all listings; otherwise `false`.
- **Exceptions**: May throw `InvalidOperationException` if the permission service is not configured.

### CanModerateListings
- **Purpose**: Checks if the current user has permission to moderate listings (e.g., approve, reject, or remove content).
- **Parameters**: None.
- **Return value**: `true` when moderation rights are granted; otherwise `false`.
- **Exceptions**: May throw `InvalidOperationException` when the service backing these checks is unavailable.

### CanManageOwnListings
- **Purpose**: Determines whether the current user can manage listings they own (e.g., update, delete, or close their own items).
- **Parameters**: None.
- **Return value**: `true` if the user can manage their own listings; otherwise `false`.
- **Exceptions**: May throw `InvalidOperationException` if the permission service has not been set up.

## Usage

```csharp
using MarketplaceEngine.Services;

// Example 1: Conditional UI rendering based on admin rights
if (PermissionServiceExtensions.IsAdministrator)
{
    RenderAdminDashboard();
}
else
{
    RenderStandardUserView();
}
```

```csharp
using MarketplaceEngine.Services;

// Example 2: Guarding a listing edit endpoint
public IActionResult EditListing(int listingId)
{
    if (!PermissionServiceExtensions.CanEditAnyListing &&
        !PermissionServiceExtensions.CanManageOwnListings)
    {
        return Forbid();
    }

    // Proceed with edit logic...
    return View(GetListing(listingId));
}
```

## Notes

- The properties are thread‑safe; they read shared state that is initialized once at application start and never mutated thereafter.
- If the permission service has not been initialized before any of these properties is accessed, an `InvalidOperationException` will be thrown to signal a configuration error.
- The return values reflect the effective permissions after role inheritance and policy evaluation; they do not change during the lifetime of a single request unless the underlying service is explicitly re‑configured.
- Consumers should treat a `false` result as a denial of the corresponding action and avoid attempting the operation, as downstream services will also enforce the same checks.
