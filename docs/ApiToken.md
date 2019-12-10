# ApiToken

The `ApiToken` class acts as the central data structure and management interface for handling authentication and authorization tokens within the `marketplace-engine`. It encapsulates user identity, temporal validity, and granted permission scopes, while providing essential methods for generating new tokens, validating current status, managing API keys, and performing lifecycle operations such as revocation.

## API

*   **`string Token`**: The unique string representation of the authentication token.
*   **`Guid UserId`**: The identifier of the user associated with this token.
*   **`DateTime IssuedAt`**: The timestamp indicating when the token was created.
*   **`DateTime ExpiresAt`**: The timestamp indicating when the token ceases to be valid.
*   **`List<string> Scopes`**: A collection of permission scopes granted to this token.
*   **`TokenService TokenService`**: The service instance responsible for token operations.
*   **`ApiToken GenerateToken(Guid userId, List<string> scopes)`**: Creates and returns a new `ApiToken` for the specified user with the provided scopes.
*   **`bool IsTokenValid()`**: Determines if the token is currently valid based on its expiration date and revocation status.
*   **`bool HasScope(string scope)`**: Verifies if the token possesses the specified permission scope.
*   **`void RevokeToken()`**: Invalidates the current token.
*   **`string HashToken(string token)`**: Generates a secure hash of the provided token string.
*   **`ApiKeyValidator ApiKeyValidator`**: The validator component used for API key checks.
*   **`void RegisterApiKey(string key, Guid userId)`**: Associates a new API key with a specific user.
*   **`bool TryValidateApiKey(string key, out Guid userId)`**: Attempts to validate an API key; returns true and outputs the associated `UserId` if successful.
*   **`void RevokeApiKey(string key)`**: Removes the association for the specified API key, effectively revoking it.

## Usage

### Generating and Using a Token
```csharp
var tokenManager = new ApiToken();
var scopes = new List<string> { "read:products", "write:orders" };

// Generate a new token
var newToken = tokenManager.GenerateToken(userId, scopes);

// Validate and check scope
if (newToken.IsTokenValid() && newToken.HasScope("read:products"))
{
    // Proceed with authorized operation
}
```

### Managing API Keys
```csharp
var validator = new ApiToken();

// Register a new API key
validator.RegisterApiKey("example-key-value", userId);

// Validate key for a request
if (validator.TryValidateApiKey("example-key-value", out Guid validatedUserId))
{
    // Key is valid, use validatedUserId
}
else
{
    // Handle invalid key
}
```

## Notes

*   **Thread Safety**: The `ApiToken` class and its associated services are generally designed to be thread-safe for reading operations. However, mutation methods like `RevokeToken` and `RegisterApiKey` should be managed with appropriate synchronization primitives if called from multiple threads simultaneously.
*   **Expiration Handling**: Always invoke `IsTokenValid` immediately before performing any operation requiring authorization, as temporal validity may change between the instantiation of the `ApiToken` object and the request execution.
*   **API Key Revocation**: Revocation is immediate. Once `RevokeApiKey` is called, subsequent calls to `TryValidateApiKey` with that key will return false.
*   **Error Handling**: Methods such as `TryValidateApiKey` utilize the `out` parameter pattern to indicate failure gracefully without throwing exceptions for common validation scenarios. Ensure caller logic checks the boolean return value before accessing the `out` parameter.
