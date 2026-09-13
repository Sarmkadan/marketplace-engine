#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace MarketplaceEngine.Infrastructure.Security;

/// <summary>
/// API token information.
/// </summary>
public class ApiToken
{
    /// <summary>
    /// Gets or sets the token value.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the user to whom the token was issued.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the token was issued.
    /// </summary>
    public DateTime IssuedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the scopes granted to the token.
    /// </summary>
    public List<string> Scopes { get; set; } = new();
}

/// <summary>
/// Service for generating and validating API tokens.
/// In production, use JWT (JSON Web Tokens) with proper signature verification.
/// </summary>
public class TokenService
{
    private readonly ILogger<TokenService> _logger;
    private readonly string _tokenSecret;
    private readonly ConcurrentDictionary<string, byte> _revokedTokens = new();
    private const int TokenLengthBytes = 32;
    private const int TokenExpirationDays = 30;
    private const byte RevokedTokenMarker = 0;
    private const string Base64UrlPlus = "+";
    private const string Base64UrlMinus = "-";
    private const string Base64UrlSlash = "/";
    private const string Base64UrlUnderscore = "_";

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenService"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record token operations.</param>
    /// <param name="tokenSecret">The secret appended to token values before hashing.</param>
    public TokenService(ILogger<TokenService> logger, string tokenSecret)
    {
        _logger = logger;
        _tokenSecret = tokenSecret;
    }

    /// <summary>
    /// Generates a new API token for a user.
    /// Tokens are stored as salted hashes in production.
    /// </summary>
    /// <param name="userId">The identifier of the user for whom to generate the token.</param>
    /// <param name="scopes">The optional scopes to grant to the token.</param>
    /// <returns>The generated API token.</returns>
    public ApiToken GenerateToken(Guid userId, List<string>? scopes = null)
    {
        var token = new ApiToken
        {
            Token = GenerateRandomToken(),
            UserId = userId,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(TokenExpirationDays),
            Scopes = scopes ?? new()
        };

        _logger.LogInformation("API token generated for user: {UserId}", userId);
        return token;
    }

    /// <summary>
    /// Validates an API token format, expiration, and revocation status.
    /// In production, verify signature against stored hash.
    /// </summary>
    /// <param name="token">The API token to validate.</param>
    /// <returns><see langword="true"/> if the token is nonempty, unexpired, and not revoked; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="token"/> is <see langword="null"/>.</exception>
    public bool IsTokenValid(ApiToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (string.IsNullOrWhiteSpace(token.Token))
        {
            _logger.LogWarning("Token validation failed: token is empty");
            return false;
        }

        if (DateTime.UtcNow > token.ExpiresAt)
        {
            _logger.LogWarning("Token validation failed: token expired at {ExpiresAt}", token.ExpiresAt);
            return false;
        }

        if (_revokedTokens.ContainsKey(token.Token))
        {
            _logger.LogWarning("Token validation failed: token has been revoked");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a token has a specific scope.
    /// Scopes control what operations a token can perform.
    /// </summary>
    /// <param name="token">The API token whose scopes to inspect.</param>
    /// <param name="scope">The scope to locate.</param>
    /// <returns><see langword="true"/> if the token contains the scope; otherwise, <see langword="false"/>.</returns>
    public bool HasScope(ApiToken token, string scope)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(scope);

        return token.Scopes.Contains(scope, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Revokes a token, adding it to the in-memory blacklist checked by <see cref="IsTokenValid"/>.
    /// In production, store revoked token hashes in a distributed blacklist cache.
    /// </summary>
    /// <param name="tokenValue">The token value to revoke.</param>
    /// <exception cref="ArgumentException"><paramref name="tokenValue"/> is null or whitespace.</exception>
    public void RevokeToken(string tokenValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenValue);

        _revokedTokens[tokenValue] = RevokedTokenMarker;
        _logger.LogInformation("Token revoked");
    }

    private string GenerateRandomToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenData = new byte[TokenLengthBytes];
        rng.GetBytes(tokenData);

        // Return URL-safe base64 encoded token
        var token = Convert.ToBase64String(tokenData)
            .Replace(Base64UrlPlus, Base64UrlMinus)
            .Replace(Base64UrlSlash, Base64UrlUnderscore)
            .TrimEnd('=');

        return token;
    }

    /// <summary>
    /// Hashes a token for secure storage in database.
    /// </summary>
    /// <param name="token">The token value to hash.</param>
    /// <returns>The Base64-encoded SHA-256 hash of the token and configured secret.</returns>
    public string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashedToken = sha256.ComputeHash(Encoding.UTF8.GetBytes(token + _tokenSecret));
        return Convert.ToBase64String(hashedToken);
    }
}

/// <summary>
/// Service for validating API keys passed in request headers.
/// </summary>
public class ApiKeyValidator
{
    private readonly ILogger<ApiKeyValidator> _logger;
    private readonly ConcurrentDictionary<string, Guid> _validApiKeys = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyValidator"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record API key operations.</param>
    public ApiKeyValidator(ILogger<ApiKeyValidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a valid API key for a user.
    /// In production, load from secure configuration or database.
    /// </summary>
    /// <param name="apiKey">The API key to register.</param>
    /// <param name="userId">The identifier of the user associated with the API key.</param>
    /// <exception cref="ArgumentException"><paramref name="apiKey"/> is null or whitespace.</exception>
    public void RegisterApiKey(string apiKey, Guid userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        _validApiKeys[apiKey] = userId;
        _logger.LogInformation("API key registered for user: {UserId}", userId);
    }

    /// <summary>
    /// Validates an API key and returns associated user ID.
    /// </summary>
    /// <param name="apiKey">The API key to validate.</param>
    /// <param name="userId">When this method returns, contains the associated user identifier if validation succeeds; otherwise, <see cref="Guid.Empty"/>.</param>
    /// <returns><see langword="true"/> if the API key is registered; otherwise, <see langword="false"/>.</returns>
    public bool TryValidateApiKey(string apiKey, out Guid userId)
    {
        userId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("API key validation failed: key is empty");
            return false;
        }

        if (!_validApiKeys.TryGetValue(apiKey, out userId))
        {
            _logger.LogWarning("API key validation failed: key not found");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Revokes an API key.
    /// </summary>
    /// <param name="apiKey">The API key to revoke.</param>
    public void RevokeApiKey(string apiKey)
    {
        _validApiKeys.TryRemove(apiKey, out _);
        _logger.LogInformation("API key revoked");
    }
}
