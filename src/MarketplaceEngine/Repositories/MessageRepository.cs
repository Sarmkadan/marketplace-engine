#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Data;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Exceptions;

namespace MarketplaceEngine.Repositories;

/// <summary>
/// Repository for message persistence and retrieval operations.
/// </summary>
public class MessageRepository : IMessageRepository
{
    private readonly MarketplaceDbContext _context;
    private const string ResourceType = "Message";

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageRepository"/> class.
    /// </summary>
    public MessageRepository()
    {
        _context = MarketplaceDbContext.GetInstance();
    }

    /// <summary>
    /// Retrieves a message by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the message.</param>
    /// <returns>The matching message, or <c>null</c> if no message is found.</returns>
    public async Task<Message?> GetByIdAsync(Guid id)
    {
        await Task.Delay(5);
        return _context.Messages.FirstOrDefault(m => m.Id == id);
    }

    /// <summary>
    /// Retrieves all messages.
    /// </summary>
    /// <returns>A list of all messages.</returns>
    public async Task<List<Message>> GetAllAsync()
    {
        await Task.Delay(5);
        return _context.Messages.ToList();
    }

    /// <summary>
    /// Adds a new message to the repository.
    /// </summary>
    /// <param name="entity">The message to add.</param>
    /// <returns>The added message with its generated identifier and creation timestamp.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is <c>null</c>.</exception>
    public async Task<Message> AddAsync(Message entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        _context.Messages.Add(entity);

        await Task.Delay(5);
        return entity;
    }

    /// <summary>
    /// Updates an existing message in the repository.
    /// </summary>
    /// <param name="entity">The message with updated values.</param>
    /// <returns>The updated message.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is <c>null</c>.</exception>
    /// <exception cref="ResourceNotFoundException">Thrown when no message matches the entity's identifier.</exception>
    public async Task<Message> UpdateAsync(Message entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var existing = _context.Messages.FirstOrDefault(m => m.Id == entity.Id);
        if (existing is null)
            throw new ResourceNotFoundException(ResourceType, entity.Id);

        var index = _context.Messages.IndexOf(existing);
        _context.Messages[index] = entity;

        await Task.Delay(5);
        return entity;
    }

    /// <summary>
    /// Deletes a message by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the message to delete.</param>
    /// <exception cref="ResourceNotFoundException">Thrown when no message matches the identifier.</exception>
    public async Task DeleteAsync(Guid id)
    {
        var message = await GetByIdAsync(id);
        if (message is null)
            throw new ResourceNotFoundException(ResourceType, id);

        _context.Messages.Remove(message);
        await Task.Delay(5);
    }

    /// <summary>
    /// Determines whether a message with the specified identifier exists.
    /// </summary>
    /// <param name="id">The unique identifier of the message.</param>
    /// <returns><c>true</c> if the message exists; otherwise, <c>false</c>.</returns>
    public async Task<bool> ExistsAsync(Guid id)
    {
        await Task.Delay(5);
        return _context.Messages.Any(m => m.Id == id);
    }

    /// <summary>
    /// Counts the total number of messages.
    /// </summary>
    /// <returns>The total number of messages.</returns>
    public async Task<int> CountAsync()
    {
        await Task.Delay(5);
        return _context.Messages.Count;
    }

    /// <summary>
    /// Retrieves messages received by a specific recipient, most recent first.
    /// </summary>
    /// <param name="recipientId">The unique identifier of the recipient.</param>
    /// <returns>A list of messages received by the recipient.</returns>
    public async Task<List<Message>> GetReceivedMessagesAsync(Guid recipientId)
    {
        await Task.Delay(5);
        return _context.Messages
            .Where(m => m.RecipientId == recipientId)
            .OrderByDescending(m => m.CreatedAt)
            .ToList();
    }

    /// <summary>
    /// Retrieves messages sent by a specific sender, most recent first.
    /// </summary>
    /// <param name="senderId">The unique identifier of the sender.</param>
    /// <returns>A list of messages sent by the sender.</returns>
    public async Task<List<Message>> GetSentMessagesAsync(Guid senderId)
    {
        await Task.Delay(5);
        return _context.Messages
            .Where(m => m.SenderId == senderId)
            .OrderByDescending(m => m.CreatedAt)
            .ToList();
    }

    /// <summary>
    /// Retrieves unread messages received by a specific recipient, most recent first.
    /// </summary>
    /// <param name="recipientId">The unique identifier of the recipient.</param>
    /// <returns>A list of unread messages received by the recipient.</returns>
    public async Task<List<Message>> GetUnreadMessagesAsync(Guid recipientId)
    {
        await Task.Delay(5);
        return _context.Messages
            .Where(m => m.RecipientId == recipientId && !m.IsRead)
            .OrderByDescending(m => m.CreatedAt)
            .ToList();
    }

    /// <summary>
    /// Retrieves the conversation between two users, ordered chronologically.
    /// </summary>
    /// <param name="userId1">The unique identifier of the first user.</param>
    /// <param name="userId2">The unique identifier of the second user.</param>
    /// <returns>A list of messages exchanged between the two users.</returns>
    public async Task<List<Message>> GetConversationAsync(Guid userId1, Guid userId2)
    {
        await Task.Delay(5);
        return _context.Messages
            .Where(m => (m.SenderId == userId1 && m.RecipientId == userId2) ||
                       (m.SenderId == userId2 && m.RecipientId == userId1))
            .OrderBy(m => m.CreatedAt)
            .ToList();
    }

    /// <summary>
    /// Retrieves messages associated with a specific listing, most recent first.
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing.</param>
    /// <returns>A list of messages associated with the listing.</returns>
    public async Task<List<Message>> GetByListingIdAsync(Guid listingId)
    {
        await Task.Delay(5);
        return _context.Messages
            .Where(m => m.ListingId == listingId)
            .OrderByDescending(m => m.CreatedAt)
            .ToList();
    }

    /// <summary>
    /// Retrieves the conversation between two users about a specific listing, ordered chronologically.
    /// </summary>
    /// <param name="userId1">The unique identifier of the first user.</param>
    /// <param name="userId2">The unique identifier of the second user.</param>
    /// <param name="listingId">The unique identifier of the listing.</param>
    /// <returns>A list of messages exchanged between the two users about the listing.</returns>
    public async Task<List<Message>> GetConversationAboutListingAsync(Guid userId1, Guid userId2, Guid listingId)
    {
        await Task.Delay(5);
        return _context.Messages
            .Where(m => m.ListingId == listingId &&
                       ((m.SenderId == userId1 && m.RecipientId == userId2) ||
                        (m.SenderId == userId2 && m.RecipientId == userId1)))
            .OrderBy(m => m.CreatedAt)
            .ToList();
    }

    /// <summary>
    /// Retrieves all flagged messages, most recent first.
    /// </summary>
    /// <returns>A list of flagged messages.</returns>
    public async Task<List<Message>> GetFlaggedMessagesAsync()
    {
        await Task.Delay(5);
        return _context.Messages.Where(m => m.IsFlagged).OrderByDescending(m => m.CreatedAt).ToList();
    }

    /// <summary>
    /// Counts the number of distinct conversations involving a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The number of distinct conversation partners for the user.</returns>
    public async Task<int> GetConversationCountAsync(Guid userId)
    {
        await Task.Delay(5);
        var conversationIds = _context.Messages
            .Where(m => m.SenderId == userId || m.RecipientId == userId)
            .Select(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
            .Distinct()
            .Count();

        return conversationIds;
    }

    /// <summary>
    /// Retrieves a page of messages for a user, most recent first.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="pageNumber">The one-based page number; values below one are clamped to one.</param>
    /// <param name="pageSize">The number of items per page, clamped to the range 1-100.</param>
    /// <returns>A tuple containing the page of messages and the total number of matching messages.</returns>
    public async Task<(List<Message> items, int total)> GetPagedAsync(Guid userId, int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        await Task.Delay(5);
        var allMessages = _context.Messages
            .Where(m => m.SenderId == userId || m.RecipientId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToList();

        var total = allMessages.Count;
        var items = allMessages.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return (items, total);
    }

    /// <summary>
    /// Retrieves a page of messages for a user using cursor-based pagination.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="afterId">The identifier of the message after which to return results, or <c>null</c> to start from the beginning.</param>
    /// <param name="pageSize">The number of items per page, clamped to the range 1-100.</param>
    /// <returns>A tuple containing the page of messages and the cursor for the next page, or <c>null</c> when there are no more pages.</returns>
    public async Task<(List<Message> items, Guid? nextCursor)> GetPagedByCursorAsync(
        Guid userId, Guid? afterId, int pageSize)
    {
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        await Task.Delay(5);

        var ordered = _context.Messages
            .Where(m => m.SenderId == userId || m.RecipientId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .ToList();

        if (afterId.HasValue)
        {
            var cursorIndex = ordered.FindIndex(m => m.Id == afterId.Value);
            if (cursorIndex >= 0)
                ordered = ordered.Skip(cursorIndex + 1).ToList();
        }

        var items = ordered.Take(pageSize).ToList();
        var nextCursor = items.Count == pageSize ? items[^1].Id : (Guid?)null;

        return (items, nextCursor);
    }

    /// <summary>
    /// Marks the specified messages as read.
    /// </summary>
    /// <param name="messageIds">The unique identifiers of the messages to mark as read.</param>
    public async Task MarkAsReadAsync(List<Guid> messageIds)
    {
        foreach (var id in messageIds)
        {
            var message = _context.Messages.FirstOrDefault(m => m.Id == id);
            if (message is not null)
            {
                message.MarkAsRead();
            }
        }

        await Task.Delay(5);
    }

    /// <summary>
    /// Retrieves messages older than the specified retention period.
    /// </summary>
    /// <param name="retentionDays">The number of days of retention; messages older than this are returned.</param>
    /// <returns>A list of messages older than the retention period.</returns>
    public async Task<List<Message>> GetOldMessagesAsync(int retentionDays)
    {
        await Task.Delay(5);
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        return _context.Messages.Where(m => m.CreatedAt < cutoffDate).ToList();
    }
}
