// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.ValueObjects;

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Represents a marketplace user (buyer/seller).
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public Location? Location { get; set; }
    public Rating? Rating { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public int TotalListings { get; set; }
    public int TotalSales { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public string? VerificationToken { get; set; }
    public DateTime? VerificationExpiry { get; set; }

    // Validates email format
    public void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            throw new ArgumentException("Invalid email format", nameof(Email));
    }

    // Validates user profile completeness
    public void ValidateProfile()
    {
        if (string.IsNullOrWhiteSpace(FullName) || FullName.Length < 2)
            throw new ArgumentException("Full name is required and must be at least 2 characters", nameof(FullName));

        ValidateEmail();

        if (FullName.Length > 100)
            throw new ArgumentException("Full name cannot exceed 100 characters", nameof(FullName));

        if (!string.IsNullOrEmpty(Bio) && Bio.Length > 500)
            throw new ArgumentException("Bio cannot exceed 500 characters", nameof(Bio));
    }

    // Updates user's last activity timestamp
    public void UpdateLastActivity()
    {
        LastActiveAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Promotes user to premium seller
    public void PromoteToPremiumSeller()
    {
        if (Role != UserRole.User)
            throw new InvalidOperationException("Only regular users can be promoted to premium seller");

        Role = UserRole.PremiumSeller;
        UpdatedAt = DateTime.UtcNow;
    }

    // Records a successful sale
    public void RecordSale()
    {
        TotalSales++;
        UpdatedAt = DateTime.UtcNow;
    }

    // Generates verification token for email confirmation
    public void GenerateVerificationToken(int expiryMinutes = 1440)
    {
        VerificationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        VerificationExpiry = DateTime.UtcNow.AddMinutes(expiryMinutes);
    }

    // Verifies email using the provided token
    public bool VerifyEmail(string token)
    {
        if (IsVerified)
            return true;

        if (string.IsNullOrEmpty(VerificationToken) || token != VerificationToken)
            return false;

        if (VerificationExpiry < DateTime.UtcNow)
            return false;

        IsVerified = true;
        VerificationToken = null;
        VerificationExpiry = null;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    // Deactivates the user account
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // Updates rating when new review is received
    public void UpdateRating(Rating newRating)
    {
        Rating = newRating;
        UpdatedAt = DateTime.UtcNow;
    }
}
