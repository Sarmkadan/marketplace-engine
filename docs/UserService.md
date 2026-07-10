# UserService
The `UserService` class is a crucial component of the `marketplace-engine` project, responsible for managing user-related operations. It provides a comprehensive set of methods for user registration, retrieval, updates, and other actions, enabling the development of robust user management systems.

## API
The `UserService` class exposes the following public members:
* `public UserService`: The constructor for the `UserService` class.
* `public async Task<User> RegisterUserAsync`: Registers a new user asynchronously. Returns the newly created `User` object.
* `public async Task<User> GetUserAsync`: Retrieves a user by their identifier asynchronously. Returns the `User` object associated with the identifier.
* `public async Task<User> GetUserByEmailAsync`: Retrieves a user by their email address asynchronously. Returns the `User` object associated with the email address.
* `public async Task<User> UpdateProfileAsync`: Updates a user's profile information asynchronously. Returns the updated `User` object.
* `public async Task<bool> VerifyEmailAsync`: Verifies a user's email address asynchronously. Returns `true` if the verification is successful, `false` otherwise.
* `public async Task<User> ResendVerificationTokenAsync`: Resends a verification token to a user asynchronously. Returns the updated `User` object.
* `public async Task<User> PromoteToPremiumAsync`: Promotes a user to a premium account asynchronously. Returns the updated `User` object.
* `public async Task<User> DeactivateAccountAsync`: Deactivates a user's account asynchronously. Returns the updated `User` object.
* `public async Task<User> ReactivateAccountAsync`: Reactivates a user's account asynchronously. Returns the updated `User` object.
* `public async Task<User> RecordSaleAsync`: Records a sale for a user asynchronously. Returns the updated `User` object.
* `public async Task<User> UpdateRatingAsync`: Updates a user's rating asynchronously. Returns the updated `User` object.
* `public async Task<List<User>> GetTopSellersAsync`: Retrieves a list of top-selling users asynchronously. Returns a list of `User` objects.
* `public async Task<(List<User> items, int total)> GetPaginatedUsersAsync`: Retrieves a paginated list of users asynchronously. Returns a tuple containing a list of `User` objects and the total count of users.
* `public async Task UpdateLastActivityAsync`: Updates a user's last activity timestamp asynchronously.
* `public async Task<int> GetVerifiedUserCountAsync`: Retrieves the count of verified users asynchronously. Returns the count of verified users.
* `public async Task<int> GetActiveUserCountAsync`: Retrieves the count of active users asynchronously. Returns the count of active users.
* `public async Task ValidateUserAccessAsync`: Validates a user's access asynchronously.
* `public async Task<User> GetPublicProfileAsync`: Retrieves a user's public profile information asynchronously. Returns the `User` object associated with the public profile.

## Usage
The following examples demonstrate how to use the `UserService` class:
```csharp
// Example 1: Registering a new user
var userService = new UserService();
var user = await userService.RegisterUserAsync(new User { Email = "example@example.com", Password = "password" });
Console.WriteLine($"User {user.Email} registered successfully");

// Example 2: Retrieving a user's public profile
var userService = new UserService();
var user = await userService.GetPublicProfileAsync("example@example.com");
Console.WriteLine($"User {user.Email} public profile: {user.PublicProfile}");
```

## Notes
When using the `UserService` class, consider the following edge cases and thread-safety remarks:
* The `RegisterUserAsync` method may throw an exception if the email address is already in use.
* The `GetUserAsync` and `GetUserByEmailAsync` methods may return `null` if the user is not found.
* The `UpdateProfileAsync` method may throw an exception if the user's profile information is invalid.
* The `VerifyEmailAsync` method may throw an exception if the email verification process fails.
* The `ResendVerificationTokenAsync` method may throw an exception if the user's email address is not verified.
* The `PromoteToPremiumAsync`, `DeactivateAccountAsync`, and `ReactivateAccountAsync` methods may throw exceptions if the user's account is not in a valid state.
* The `RecordSaleAsync` and `UpdateRatingAsync` methods may throw exceptions if the user's sale or rating information is invalid.
* The `GetTopSellersAsync` and `GetPaginatedUsersAsync` methods may return empty lists if no users are found.
* The `UpdateLastActivityAsync` method may throw an exception if the user's last activity timestamp is not updated successfully.
* The `GetVerifiedUserCountAsync` and `GetActiveUserCountAsync` methods may return incorrect counts if the user data is not up-to-date.
* The `ValidateUserAccessAsync` method may throw an exception if the user's access is not validated successfully.
* The `GetPublicProfileAsync` method may return `null` if the user's public profile is not found.
* The `UserService` class is designed to be thread-safe, but it is still important to ensure that the underlying data storage and retrieval mechanisms are also thread-safe to avoid data corruption or other concurrency issues.
