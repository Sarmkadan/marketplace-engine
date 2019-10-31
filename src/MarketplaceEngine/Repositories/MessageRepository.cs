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

    public MessageRepository()
    {
        _context = MarketplaceDbContext.GetInstance();
    }

    public async Task<Message?> GetByIdAsync(Guid id)
    {
        await Task.Delay(5);
        return _context.Messages.FirstOrDefault(m => m.Id == id);
    }

    public async Task<List<Message>> GetAllAsync()
    {
        await Task.Delay(5);
        return _context.Messages.ToList();
    }

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

    public async Task DeleteAsync(Guid id)
    {
        var message = await GetByIdAsync(id);
        if (message is null)
            throw new ResourceNotFoundException(ResourceType, id);

        _context.Messages.Remove(message);
        await Task.Delay(5);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        await Task.Delay(5);
        return _context.Messages.Any(m => m.Id == id);
    }

    public async Task<int> CountAsync()
    {
        await Task.Delay(5);
        return _context.Messages.Count;
    }

    public async Task<List<Message>> GetReceivedMessagesAsync(Guid recipientId)
    {
        await Task.Delay(5);
        return _context.Messages
            .Where(m => m.RecipientId == recipientId)
            .OrderByDescending(m => m.CreatedAt)
            .ToList();
    }

    public async Task<List<Message>> GetSentMessagesAsync(Guid senderId)
    {
        await Task.Delay(5);
        return _context.Messages
            .Where(m => m.SenderId == senderId)
            .OrderByDescending(m => m.CreatedAt)
            .ToList();
    }

    public async Task<List<Message>> GetUnreadMessagesAsync(Guid recipientId)
    {
        await Task.Delay(5);
        return _context.Messages
            .Where(m => m.RecipientId == recipientId && !m.IsRead)
            .OrderByDescending(m => m.CreatedAt)
            .ToList();
    }

    public async Task<List<Message>> GetConversationAsync(Guid userId1, Guid userId2)
    {
        await Task.Delay(5);
        return _context.Messages
            .Where(m => (m.SenderId == userId1 && m.RecipientId == userId2) ||
                       (m.SenderId == userId2 && m.RecipientId == userId1))
            .OrderBy(m => m.CreatedAt)
            .ToList();
    }

    public async Task<List<Message>> GetByListingIdAsync(Guid listingId)
    {
        await Task.Delay(5);
        return _context.Messages
            .Where(m => m.ListingId == listingId)
            .OrderByDescending(m => m.CreatedAt)
            .ToList();
    }

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

    public async Task<List<Message>> GetFlaggedMessagesAsync()
    {
        await Task.Delay(5);
        return _context.Messages.Where(m => m.IsFlagged).OrderByDescending(m => m.CreatedAt).ToList();
    }

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

    public async Task<List<Message>> GetOldMessagesAsync(int retentionDays)
    {
        await Task.Delay(5);
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        return _context.Messages.Where(m => m.CreatedAt < cutoffDate).ToList();
    }
}
