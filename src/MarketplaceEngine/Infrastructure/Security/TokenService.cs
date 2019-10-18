// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace MarketplaceEngine.Infrastructure.Security;

/// <summary>
/// API token information.
/// </summary>
public class ApiToken
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
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
    private const int TokenLengthBytes = 32;
    private const int TokenExpirationDays = 30;

    public TokenService(ILogger<TokenService> logger, string tokenSecret)
    {
        _logger = logger;
        _tokenSecret = tokenSecret;
    }

    /// <summary>
    /// Generates a new API token for a user.
    /// Tokens are stored as salted hashes in production.
    /// </summary>
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
    /// Validates an API token format and expiration.
    /// In production, verify signature against stored hash.
    /// </summary>
    public bool IsTokenValid(ApiToken token)
    {
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

        return true;
    }

    /// <summary>
    /// Checks if a token has a specific scope.
    /// Scopes control what operations a token can perform.
    /// </summary>
    public bool HasScope(ApiToken token, string scope)
    {
        return token.Scopes.Contains(scope, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Revokes a token (removes it from valid list).
    /// In production, store revoked token hashes in a blacklist cache.
    /// </summary>
    public void RevokeToken(string tokenValue)
    {
        // In production, add to revocation list/blacklist
        _logger.LogInformation("Token revoked");
    }

    private string GenerateRandomToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenData = new byte[TokenLengthBytes];
        rng.GetBytes(tokenData);

        // Return URL-safe base64 encoded token
        var token = Convert.ToBase64String(tokenData)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        return token;
    }

    /// <summary>
    /// Hashes a token for secure storage in database.
    /// </summary>
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
    private readonly Dictionary<string, Guid> _validApiKeys = new();

    public ApiKeyValidator(ILogger<ApiKeyValidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a valid API key for a user.
    /// In production, load from secure configuration or database.
    /// </summary>
    public void RegisterApiKey(string apiKey, Guid userId)
    {
        _validApiKeys[apiKey] = userId;
        _logger.LogInformation("API key registered for user: {UserId}", userId);
    }

    /// <summary>
    /// Validates an API key and returns associated user ID.
    /// </summary>
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
    public void RevokeApiKey(string apiKey)
    {
        _validApiKeys.Remove(apiKey);
        _logger.LogInformation("API key revoked");
    }
}
