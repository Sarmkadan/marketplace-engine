# UsersControllerExtensions

Extension methods for `UsersController` that provide convenient access to current-user operations such as profile retrieval, metrics lookup, and profile updates.

## API

### `GetCurrentUserId`

Retrieves the unique identifier of the currently authenticated user.

- **Returns**
  `Guid?`: The user ID if the user is authenticated; otherwise, `null`.
- **Remarks**
  Returns `null` when no user is authenticated or when the authentication context is unavailable.

---

### `GetCurrentUserProfile`

Asynchronously retrieves the profile of the currently authenticated user.

- **Returns**
  `Task<IActionResult>`: An `IActionResult` representing the HTTP response. On success, returns the user profile with HTTP 200 OK; otherwise, returns an appropriate error status.
- **Remarks**
  Throws `InvalidOperationException` if the user context is missing or invalid.

---

### `GetCurrentUserSellerMetrics`

Asynchronously retrieves seller-specific metrics for the currently authenticated user.

- **Returns**
  `Task<IActionResult>`: An `IActionResult` representing the HTTP response. On success, returns the seller metrics with HTTP 200 OK; otherwise, returns an appropriate error status.
- **Remarks**
  Throws `InvalidOperationException` if the user is not a seller or if the seller context is missing.

---
### `UpdateCurrentUserProfile`

Asynchronously updates the profile of the currently authenticated user.

- **Parameters**
  `controller` (`UsersController`): The controller instance.
  `profileUpdate` (`UserProfileUpdateDto`): The updated profile data.
- **Returns**
  `Task<IActionResult>`: An `IActionResult` representing the HTTP response. On success, returns HTTP 204 No Content; otherwise, returns an appropriate error status.
- **Remarks**
  Throws `ArgumentNullException` if `profileUpdate` is `null`.
  Throws `InvalidOperationException` if the user context is missing or invalid.

## Usage
