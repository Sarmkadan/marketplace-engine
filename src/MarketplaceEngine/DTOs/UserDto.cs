#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// User profile data transfer object.
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserDto() { }

    public UserDto(User user)
    {
        Id = user.Id;
        Email = user.Email;
        DisplayName = user.DisplayName;
        Bio = user.Bio;
        Role = user.Role.ToString();
        AverageRating = user.Rating?.Score ?? 0;
        ReviewCount = user.Rating?.ReviewCount ?? 0;
        EmailVerified = user.EmailVerified;
        CreatedAt = user.CreatedAt;
        UpdatedAt = user.UpdatedAt;
    }
}

/// <summary>
/// Seller metrics including reputation and sales data.
/// </summary>
public class SellerMetricsDto
{
    public Guid UserId { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalSales { get; set; }
    public string ResponseTime { get; set; } = string.Empty;
}

/// <summary>
/// Top seller ranking with position.
/// </summary>
public class SellerRankingDto
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
}

/// <summary>
/// Request to update user profile.
/// </summary>
public class UpdateUserRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}
