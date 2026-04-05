#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;

namespace MarketplaceEngine.Services;

/// <summary>
/// Service for managing user accounts and profiles.
/// </summary>
public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    // Creates a new user account
    public async Task<User> RegisterUserAsync(string email, string fullName, string? phone = null)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser is not null)
            throw new DuplicateResourceException("User", "email", email);

        var user = new User
        {
            Email = email,
            FullName = fullName,
            Phone = phone,
            Role = UserRole.User,
            IsActive = true,
            IsVerified = false
        };

        user.ValidateProfile();
        user.GenerateVerificationToken();

        return await _userRepository.AddAsync(user);
    }

    // Gets user by ID
    public async Task<User> GetUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new ResourceNotFoundException("User", userId);

        return user;
    }

    // Gets user by email
    public async Task<User> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
            throw new ResourceNotFoundException("User", email);

        return user;
    }

    // Updates user profile
    public async Task<User> UpdateProfileAsync(Guid userId, string? fullName = null, string? phone = null,
        string? bio = null, Location? location = null)
    {
        var user = await GetUserAsync(userId);

        if (!string.IsNullOrEmpty(fullName))
            user.FullName = fullName;

        if (phone is not null)
            user.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone;

        if (bio is not null)
            user.Bio = string.IsNullOrWhiteSpace(bio) ? null : bio;

        if (location is not null)
            user.Location = location;

        user.ValidateProfile();
        return await _userRepository.UpdateAsync(user);
    }

    // Verifies user email
    public async Task<bool> VerifyEmailAsync(Guid userId, string verificationToken)
    {
        var user = await GetUserAsync(userId);

        if (user.VerifyEmail(verificationToken))
        {
            await _userRepository.UpdateAsync(user);
            return true;
        }

        return false;
    }

    // Resends verification email
    public async Task<User> ResendVerificationTokenAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
            throw new ResourceNotFoundException("User", email);

        if (user.IsVerified)
            throw new InvalidOperationException("User email is already verified");

        user.GenerateVerificationToken();
        return await _userRepository.UpdateAsync(user);
    }

    // Promotes user to premium seller
    public async Task<User> PromoteToPremiumAsync(Guid userId)
    {
        var user = await GetUserAsync(userId);

        if (user.TotalSales < 5)
            throw new InvalidOperationException("User must have at least 5 sales to be promoted to premium");

        if (user.Rating is null || user.Rating.Score < 4)
            throw new InvalidOperationException("User must have a rating of 4+ stars to be promoted to premium");

        user.PromoteToPremiumSeller();
        return await _userRepository.UpdateAsync(user);
    }

    // Deactivates user account
    public async Task<User> DeactivateAccountAsync(Guid userId)
    {
        var user = await GetUserAsync(userId);
        user.Deactivate();
        return await _userRepository.UpdateAsync(user);
    }

    // Reactivates user account
    public async Task<User> ReactivateAccountAsync(Guid userId)
    {
        var user = await GetUserAsync(userId);

        if (!user.IsActive)
        {
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            return await _userRepository.UpdateAsync(user);
        }

        return user;
    }

    // Records a sale for the user
    public async Task<User> RecordSaleAsync(Guid userId)
    {
        var user = await GetUserAsync(userId);
        user.RecordSale();
        return await _userRepository.UpdateAsync(user);
    }

    // Updates user rating
    public async Task<User> UpdateRatingAsync(Guid userId, Rating rating)
    {
        var user = await GetUserAsync(userId);
        user.UpdateRating(rating);
        return await _userRepository.UpdateAsync(user);
    }

    // Gets top sellers
    public async Task<List<User>> GetTopSellersAsync(int limit = 10)
    {
        if (limit < 1 || limit > 50)
            limit = 10;

        return await _userRepository.GetTopSellersAsync(limit);
    }

    // Gets paginated users
    public async Task<(List<User> items, int total)> GetPaginatedUsersAsync(int pageNumber, int pageSize)
    {
        return await _userRepository.GetPagedAsync(pageNumber, pageSize);
    }

    // Updates last activity timestamp
    public async Task UpdateLastActivityAsync(Guid userId)
    {
        await _userRepository.UpdateLastActivityAsync(userId);
    }

    // Gets verified users count
    public async Task<int> GetVerifiedUserCountAsync()
    {
        var verified = await _userRepository.GetVerifiedUsersAsync();
        return verified.Count;
    }

    // Gets active users count
    public async Task<int> GetActiveUserCountAsync()
    {
        var active = await _userRepository.GetActiveUsersAsync();
        return active.Count;
    }

    // Validates user can perform action
    public async Task ValidateUserAccessAsync(Guid userId)
    {
        var user = await GetUserAsync(userId);

        if (!user.IsActive)
            throw new UnauthorizedException(userId, "access the marketplace");

        if (!user.IsVerified)
            throw new UnauthorizedException(userId, "perform this action - email not verified");
    }

    // Gets user profile for public display
    public async Task<User> GetPublicProfileAsync(Guid userId)
    {
        var user = await GetUserAsync(userId);
        await _userRepository.UpdateLastActivityAsync(userId);
        return user;
    }
}
