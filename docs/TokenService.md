# TokenService

The `TokenService` class provides secure API token generation, validation, and management functionality within the `marketplace-engine`. It handles cryptographically secure token creation, expiration checking, revocation mechanisms, and scope-based authorization. The service is designed for production use with proper secret management and includes companion `ApiKeyValidator` functionality for alternative authentication mechanisms.

## API

*   **`ILogger<TokenService> _logger`**: Logger instance for recording token operations and security events.
*   **`string _tokenSecret`**: Secret value appended to tokens before hashing for additional security.
*   **`ConcurrentDictionary<string, byte> _revokedTokens`**: In-memory storage for tracking revoked tokens.
*   **`int TokenLengthBytes`**: Length of cryptographically secure random tokens in bytes (32).
*   **`int TokenExpirationDays`**: Default token validity period in days (30).
*   **`byte RevokedTokenMarker`**: Marker value used in the revoked tokens dictionary.
*   **`string Base64UrlPlus`**: Standard Base64 '+' character for URL-safe encoding replacement.
*   **`string Base64UrlMinus`**: URL-safe '-' character replacing '+' in tokens.
*   **`string Base64UrlSlash`**: Standard Base64 '/' character for URL-safe encoding replacement.
*   **`string Base64UrlUnderscore`**: URL-safe '_' character replacing '/' in tokens.
*   **`TokenService(ILogger<TokenService> logger, string tokenSecret)`**: Constructor injecting logger and token secret.
*   **`ApiToken GenerateToken(Guid userId, List<string>? scopes = null)`**: Creates a new cryptographically secure API token for the specified user with optional scopes.
*   **`bool IsTokenValid(ApiToken token)`**: Validates token format, expiration, and revocation status.
*   **`bool HasScope(ApiToken token, string scope)`**: Checks if a token contains a specific permission scope.
*   **`void RevokeToken(string tokenValue)`**: Adds a token to the revocation blacklist.
*   **`string GenerateRandomToken()`**: Private method generating cryptographically secure random tokens.
*   **`string HashToken(string token)`**: Creates a SHA-256 hash of a token combined with the secret for secure storage.

## ApiKeyValidator API

*   **`ILogger<ApiKeyValidator> _logger`**: Logger instance for recording API key operations.
*   **`ConcurrentDictionary<string, Guid> _validApiKeys`**: In-memory storage for registered API keys and their associated user IDs.
*   **`ApiKeyValidator(ILogger<ApiKeyValidator> logger)`**: Constructor injecting logger.
*   **`void RegisterApiKey(string apiKey, Guid userId)`**: Registers a valid API key for a user.
*   **`bool TryValidateApiKey(string apiKey, out Guid userId)`**: Validates an API key and returns the associated user ID.
*   **`void RevokeApiKey(string apiKey)`**: Removes an API key from the valid keys collection.

## Usage

### Generating and Validating Tokens
```csharp
var tokenService = new TokenService(logger, "your-secure-secret-here");

// Generate a new token for a user
var userToken = tokenService.GenerateToken(
    userId: Guid.NewGuid(),
    scopes: new List<string> { "read:products", "write:orders" }
);

// Validate the token before use
if (tokenService.IsTokenValid(userToken))
{
    // Check specific permissions
    if (tokenService.HasScope(userToken, "read:products"))
    {
        // Proceed with authorized operation
    }
    
    // For storage, hash the token
    string hashedToken = tokenService.HashToken(userToken.Token);
    // Store hashedToken in database
}

// Revoke a token when needed
tokenService.RevokeToken(userToken.Token);
```

### Managing API Keys
```csharp
var apiKeyValidator = new ApiKeyValidator(logger);

// Register an API key for a user
apiKeyValidator.RegisterApiKey(
    apiKey: "sk_live_abcdef123456",
    userId: Guid.NewGuid()
);

// Validate an API key from a request
if (apiKeyValidator.TryValidateApiKey(
        apiKey: Request.Headers["X-API-Key"],
        out Guid userId))
{
    // Key is valid, proceed with userId
}
else
{
    // Handle invalid API key
}

// Revoke an API key
apiKeyValidator.RevokeApiKey("sk_live_abcdef123456");
```

## Notes

*   **Security**: Tokens are generated using `RandomNumberGenerator.Create()` for cryptographic security. In production, always use environment variables or secure vaults for `_tokenSecret` rather than hardcoding values.
*   **Token Format**: Generated tokens are URL-safe Base64 strings (32 bytes → 43 characters) without padding, suitable for HTTP headers and URL parameters.
*   **Expiration**: Tokens default to 30-day expiration. Always check `IsTokenValid()` immediately before performing authorized operations.
*   **Revocation**: Token revocation is immediate and stored in-memory. For production deployments with multiple instances, replace `_revokedTokens` with a distributed cache like Redis.
*   **API Keys**: The `ApiKeyValidator` provides simple API key authentication suitable for service-to-service communication. In production, load keys from secure configuration or database.
*   **Thread Safety**: Both `TokenService` and `ApiKeyValidator` use `ConcurrentDictionary` for thread-safe operations on revoked tokens and API keys respectively.
*   **Hashing**: The `HashToken` method provides salted SHA-256 hashing for secure token storage in databases. Never store raw tokens.
*   **Logging**: All significant operations (generation, validation, revocation) are logged at appropriate levels for security monitoring.