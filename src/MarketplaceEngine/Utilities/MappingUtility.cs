#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.DTOs;

namespace MarketplaceEngine.Utilities;

/// <summary>
/// Centralized mapping utility for converting between domain models and DTOs.
/// Keeps mapping logic in one place for maintainability.
/// In production, use AutoMapper library for complex mappings.
/// </summary>
public static class MappingUtility
{
    /// <summary>
    /// Maps a Listing domain model to ListingDto.
    /// </summary>
    public static ListingDto ToDto(Listing listing)
    {
        return new ListingDto
        {
            Id = listing.Id,
            Title = listing.Title,
            Description = listing.Description,
            Price = listing.Price?.Amount ?? 0,
            SellerId = listing.SellerId,
            SellerName = string.Empty, // Hotfix: Listing model does not have SellerName
            CategoryId = listing.CategoryId,
            Status = listing.Status.ToString(),
            ViewCount = listing.ViewCount,
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt // Already nullable in DTO
        };
    }

    /// <summary>
    /// Maps a User domain model to UserDto.
    /// </summary>
    public static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.FullName, // Hotfix: Use FullName
            Bio = user.Bio,
            Role = user.Role.ToString(),
            AverageRating = user.Rating?.AverageRating ?? 0, // Hotfix: Use AverageRating
            ReviewCount = user.Rating?.TotalReviews ?? 0, // Hotfix: Use TotalReviews
            EmailVerified = user.IsVerified, // Hotfix: Use IsVerified
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    /// <summary>
    /// Maps a Message domain model to MessageDto.
    /// </summary>
    public static MessageDto ToDto(Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            RecipientId = message.RecipientId,
            Content = message.Body, // Hotfix: Use Body
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt
        };
    }

    /// <summary>
    /// Maps a ModerationReport domain model to ModerationReportDto.
    /// </summary>
    public static ModerationReportDto ToDto(ModerationReport report)
    {
        return new ModerationReportDto
        {
            Id = report.Id,
            ListingId = report.TargetListingId, // Hotfix: Use TargetListingId
            UserId = report.TargetUserId, // Hotfix: Use TargetUserId
            ReporterUserId = report.ReporterId, // Hotfix: Use ReporterId
            Reason = report.Reason,
            Status = report.Status.ToString(),
            CreatedAt = report.CreatedAt
        };
    }

    /// <summary>
    /// Maps multiple listings to DTOs.
    /// </summary>
    public static List<ListingDto> ToListingDtos(List<Listing> listings)
    {
        return listings.Select(ToDto).ToList();
    }

    /// <summary>
    /// Maps multiple users to DTOs.
    /// </summary>
    public static List<UserDto> ToUserDtos(List<User> users)
    {
        return users.Select(ToDto).ToList();
    }

    /// <summary>
    /// Maps multiple messages to DTOs.
    /// </summary>
    public static List<MessageDto> ToMessageDtos(List<Message> messages)
    {
        return messages.Select(ToDto).ToList();
    }
}
