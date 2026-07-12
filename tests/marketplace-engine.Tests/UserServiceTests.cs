#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;
using MarketplaceEngine.Services;
using Moq;
using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Contains unit tests for the <see cref="UserService"/> class.
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly UserService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserServiceTests"/> class.
    /// </summary>
    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _sut = new UserService(_userRepoMock.Object);
    }

    // ── RegisterUserAsync ──────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="UserService.RegisterUserAsync"/> returns a created user when provided with a unique email.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task RegisterUserAsync_WithUniqueEmail_ReturnsCreatedUser()
    {
        var createdUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "new@example.com",
            FullName = "Alice Smith",
            IsActive = true,
            IsVerified = false
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("new@example.com")).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(createdUser);

        var result = await _sut.RegisterUserAsync("new@example.com", "Alice Smith");

        result.Should().NotBeNull();
        result.Email.Should().Be("new@example.com");
    }

    /// <summary>
    /// Tests that <see cref="UserService.RegisterUserAsync"/> throws <see cref="DuplicateResourceException"/> when provided with a duplicate email.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task RegisterUserAsync_WithDuplicateEmail_ThrowsDuplicateResourceException()
    {
        var existing = new User { Id = Guid.NewGuid(), Email = "taken@example.com", FullName = "Bob Jones" };
        _userRepoMock.Setup(r => r.GetByEmailAsync("taken@example.com")).ReturnsAsync(existing);

        var act = async () => await _sut.RegisterUserAsync("taken@example.com", "Carol Green");

        await act.Should().ThrowAsync<DuplicateResourceException>();
    }

    /// <summary>
    /// Tests that <see cref="UserService.RegisterUserAsync"/> throws <see cref="ArgumentException"/> when provided with a short full name.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task RegisterUserAsync_WithShortFullName_ThrowsArgumentException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var act = async () => await _sut.RegisterUserAsync("x@example.com", "A");

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Full name*");
    }

    // ── GetUserAsync ───────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="UserService.GetUserAsync"/> throws <see cref="ResourceNotFoundException"/> when the user is not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUserAsync_WhenUserNotFound_ThrowsResourceNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(missingId)).ReturnsAsync((User?)null);

        var act = async () => await _sut.GetUserAsync(missingId);

        await act.Should().ThrowAsync<ResourceNotFoundException>().WithMessage("*User*not found*");
    }

    /// <summary>
    /// Tests that <see cref="UserService.GetUserAsync"/> returns the user when the user exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUserAsync_WhenUserExists_ReturnsUser()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "u@example.com", FullName = "Test User" };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await _sut.GetUserAsync(userId);

        result.Id.Should().Be(userId);
    }

    // ── VerifyEmailAsync ───────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="UserService.VerifyEmailAsync"/> returns true when provided with a valid token.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task VerifyEmailAsync_WithValidToken_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "u@example.com", FullName = "Test User" };
        user.GenerateVerificationToken();

        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateAsync(user)).ReturnsAsync(user);

        var result = await _sut.VerifyEmailAsync(userId, user.VerificationToken!);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="UserService.VerifyEmailAsync"/> returns false when provided with the wrong token.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task VerifyEmailAsync_WithWrongToken_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "u@example.com", FullName = "Test User" };
        user.GenerateVerificationToken();

        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await _sut.VerifyEmailAsync(userId, "wrong-token");

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserService.VerifyEmailAsync"/> returns false when provided with an expired token.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task VerifyEmailAsync_WithExpiredToken_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "u@example.com",
            FullName = "Test User",
            VerificationToken = "some-token",
            VerificationExpiry = DateTime.UtcNow.AddMinutes(-1) // already expired
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await _sut.VerifyEmailAsync(userId, "some-token");

        result.Should().BeFalse();
    }

    // ── PromoteToPremiumAsync ──────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="UserService.PromoteToPremiumAsync"/> throws <see cref="InvalidOperationException"/> when the user has insufficient sales.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task PromoteToPremiumAsync_WhenInsufficientSales_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "u@example.com",
            FullName = "Low Sales User",
            TotalSales = 3,
            Rating = new Rating(5, 10)
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var act = async () => await _sut.PromoteToPremiumAsync(userId);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*5 sales*");
    }

    /// <summary>
    /// Tests that <see cref="UserService.PromoteToPremiumAsync"/> throws <see cref="InvalidOperationException"/> when the user's rating is below the threshold.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task PromoteToPremiumAsync_WhenRatingBelowThreshold_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "u@example.com",
            FullName = "Low Rating User",
            TotalSales = 10,
            Rating = new Rating(3, 10)
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var act = async () => await _sut.PromoteToPremiumAsync(userId);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*4+*stars*");
    }

    /// <summary>
    /// Tests that <see cref="UserService.PromoteToPremiumAsync"/> throws <see cref="InvalidOperationException"/> when the user has no rating.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task PromoteToPremiumAsync_WhenNoRating_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "u@example.com",
            FullName = "No Rating User",
            TotalSales = 10,
            Rating = null
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var act = async () => await _sut.PromoteToPremiumAsync(userId);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that <see cref="UserService.PromoteToPremiumAsync"/> promotes the user when they are eligible.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task PromoteToPremiumAsync_WhenEligible_PromotesUser()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "u@example.com",
            FullName = "Good Seller",
            TotalSales = 10,
            Rating = new Rating(5, 20),
            Role = UserRole.User
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);

        var result = await _sut.PromoteToPremiumAsync(userId);

        result.Role.Should().Be(UserRole.PremiumSeller);
    }

    // ── DeactivateAccountAsync ─────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="UserService.DeactivateAccountAsync"/> sets the user's active status to false when the user exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeactivateAccountAsync_WhenUserExists_SetsIsActiveFalse()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "u@example.com", FullName = "Active User", IsActive = true };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var result = await _sut.DeactivateAccountAsync(userId);

        result.IsActive.Should().BeFalse();
    }

    // ── ValidateUserAccessAsync ────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="UserService.ValidateUserAccessAsync"/> throws <see cref="UnauthorizedException"/> when the user is inactive.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task ValidateUserAccessAsync_WhenUserInactive_ThrowsUnauthorizedException()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "u@example.com",
            FullName = "Inactive User",
            IsActive = false,
            IsVerified = true
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var act = async () => await _sut.ValidateUserAccessAsync(userId);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    /// <summary>
    /// Tests that <see cref="UserService.ValidateUserAccessAsync"/> throws <see cref="UnauthorizedException"/> when the user is not verified.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task ValidateUserAccessAsync_WhenUserNotVerified_ThrowsUnauthorizedException()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "u@example.com",
            FullName = "Unverified User",
            IsActive = true,
            IsVerified = false
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var act = async () => await _sut.ValidateUserAccessAsync(userId);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("*email not verified*");
    }

    /// <summary>
    /// Tests that <see cref="UserService.ValidateUserAccessAsync"/> does not throw when the user is active and verified.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task ValidateUserAccessAsync_WhenUserIsActiveAndVerified_DoesNotThrow()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "u@example.com",
            FullName = "Good User",
            IsActive = true,
            IsVerified = true
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var act = async () => await _sut.ValidateUserAccessAsync(userId);

        await act.Should().NotThrowAsync();
    }

    // ── UpdateProfileAsync ─────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="UserService.UpdateProfileAsync"/> updates the user's name when provided with a new full name.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateProfileAsync_WithNewFullName_UpdatesName()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "u@example.com", FullName = "Old Name" };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var result = await _sut.UpdateProfileAsync(userId, fullName: "New Name");

        result.FullName.Should().Be("New Name");
    }

    /// <summary>
    /// Tests that <see cref="UserService.UpdateProfileAsync"/> clears the user's phone when provided with blank input.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateProfileAsync_WithBlankPhone_ClearsPhone()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "u@example.com",
            FullName = "Test User",
            Phone = "+1234567890"
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var result = await _sut.UpdateProfileAsync(userId, phone: "   ");

        result.Phone.Should().BeNull();
    }

    // ── RecordSaleAsync ────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="UserService.RecordSaleAsync"/> increments the user's total sales count.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task RecordSaleAsync_WhenCalled_IncrementsTotalSales()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "u@example.com",
            FullName = "Seller User",
            TotalSales = 4
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var result = await _sut.RecordSaleAsync(userId);

        result.TotalSales.Should().Be(5);
    }
}
