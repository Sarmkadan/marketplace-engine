#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.Repositories;

/// <summary>
/// Repository interface for message-specific operations.
/// </summary>
public interface IMessageRepository : IRepository<Message>
{
    // Retrieves messages received by user
    Task<List<Message>> GetReceivedMessagesAsync(Guid recipientId);

    // Retrieves messages sent by user
    Task<List<Message>> GetSentMessagesAsync(Guid senderId);

    // Retrieves unread messages for user
    Task<List<Message>> GetUnreadMessagesAsync(Guid recipientId);

    // Retrieves conversation between two users
    Task<List<Message>> GetConversationAsync(Guid userId1, Guid userId2);

    // Retrieves messages related to a listing
    Task<List<Message>> GetByListingIdAsync(Guid listingId);

    // Retrieves messages about specific listing between users
    Task<List<Message>> GetConversationAboutListingAsync(Guid userId1, Guid userId2, Guid listingId);

    // Retrieves flagged messages
    Task<List<Message>> GetFlaggedMessagesAsync();

    // Gets conversation count for user
    Task<int> GetConversationCountAsync(Guid userId);

    // Retrieves paginated messages
    Task<(List<Message> items, int total)> GetPagedAsync(Guid userId, int pageNumber, int pageSize);

    // Marks messages as read
    Task MarkAsReadAsync(List<Guid> messageIds);

    // Retrieves old messages for cleanup
    Task<List<Message>> GetOldMessagesAsync(int retentionDays);
}
