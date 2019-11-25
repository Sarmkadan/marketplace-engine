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

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _sut = new UserService(_userRepoMock.Object);
    }

    // ── RegisterUserAsync ──────────────────────────────────────────────────

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

    [Fact]
    public async Task RegisterUserAsync_WithDuplicateEmail_ThrowsDuplicateResourceException()
    {
        var existing = new User { Id = Guid.NewGuid(), Email = "taken@example.com", FullName = "Bob Jones" };
        _userRepoMock.Setup(r => r.GetByEmailAsync("taken@example.com")).ReturnsAsync(existing);

        var act = async () => await _sut.RegisterUserAsync("taken@example.com", "Carol Green");

        await act.Should().ThrowAsync<DuplicateResourceException>();
    }

    [Fact]
    public async Task RegisterUserAsync_WithShortFullName_ThrowsArgumentException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var act = async () => await _sut.RegisterUserAsync("x@example.com", "A");

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Full name*");
    }

    // ── GetUserAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserAsync_WhenUserNotFound_ThrowsResourceNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(missingId)).ReturnsAsync((User?)null);

        var act = async () => await _sut.GetUserAsync(missingId);

        await act.Should().ThrowAsync<ResourceNotFoundException>().WithMessage("*User*not found*");
    }

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
