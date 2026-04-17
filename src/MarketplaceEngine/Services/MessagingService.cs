#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Exceptions;
using MarketplaceEngine.Repositories;

namespace MarketplaceEngine.Services;

/// <summary>
/// Service for managing user-to-user messaging.
/// </summary>
public class MessagingService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;

    public MessagingService(IMessageRepository messageRepository, IUserRepository userRepository)
    {
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    // Sends a message between users
    public async Task<Message> SendMessageAsync(Guid senderId, Guid recipientId, string subject, string body,
        Guid? listingId = null, List<string>? attachments = null)
    {
        var sender = await _userRepository.GetByIdAsync(senderId);
        if (sender is null)
            throw new ResourceNotFoundException("User", senderId);

        var recipient = await _userRepository.GetByIdAsync(recipientId);
        if (recipient is null)
            throw new ResourceNotFoundException("User", recipientId);

        var message = new Message
        {
            SenderId = senderId,
            RecipientId = recipientId,
            Subject = subject,
            Body = body,
            ListingId = listingId,
            AttachmentUrls = attachments ?? new List<string>()
        };

        message.ValidateBeforeSending();
        return await _messageRepository.AddAsync(message);
    }

    // Retrieves received messages
    public async Task<List<Message>> GetReceivedMessagesAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new ResourceNotFoundException("User", userId);

        return await _messageRepository.GetReceivedMessagesAsync(userId);
    }

    // Retrieves sent messages
    public async Task<List<Message>> GetSentMessagesAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new ResourceNotFoundException("User", userId);

        return await _messageRepository.GetSentMessagesAsync(userId);
    }

    // Retrieves unread messages
    public async Task<List<Message>> GetUnreadMessagesAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new ResourceNotFoundException("User", userId);

        return await _messageRepository.GetUnreadMessagesAsync(userId);
    }

    // Gets conversation between two users
    public async Task<List<Message>> GetConversationAsync(Guid userId1, Guid userId2)
    {
        var user1 = await _userRepository.GetByIdAsync(userId1);
        if (user1 is null)
            throw new ResourceNotFoundException("User", userId1);

        var user2 = await _userRepository.GetByIdAsync(userId2);
        if (user2 is null)
            throw new ResourceNotFoundException("User", userId2);

        return await _messageRepository.GetConversationAsync(userId1, userId2);
    }

    // Marks message as read
    public async Task<Message> MarkAsReadAsync(Guid messageId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message is null)
            throw new ResourceNotFoundException("Message", messageId);

        message.MarkAsRead();
        return await _messageRepository.UpdateAsync(message);
    }

    // Marks multiple messages as read
    public async Task MarkMultipleAsReadAsync(List<Guid> messageIds)
    {
        await _messageRepository.MarkAsReadAsync(messageIds);
    }

    // Marks message as unread
    public async Task<Message> MarkAsUnreadAsync(Guid messageId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message is null)
            throw new ResourceNotFoundException("Message", messageId);

        message.MarkAsUnread();
        return await _messageRepository.UpdateAsync(message);
    }

    // Flags a message
    public async Task<Message> FlagMessageAsync(Guid messageId, Guid flaggerId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message is null)
            throw new ResourceNotFoundException("Message", messageId);

        var flagger = await _userRepository.GetByIdAsync(flaggerId);
        if (flagger is null)
            throw new ResourceNotFoundException("User", flaggerId);

        message.Flag();
        return await _messageRepository.UpdateAsync(message);
    }

    // Removes flag from message
    public async Task<Message> RemoveFlagAsync(Guid messageId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message is null)
            throw new ResourceNotFoundException("Message", messageId);

        message.RemoveFlag();
        return await _messageRepository.UpdateAsync(message);
    }

    // Adds reply to message
    public async Task<Message> AddReplyAsync(Guid parentMessageId, Guid senderId, string body,
        List<string>? attachments = null)
    {
        var parentMessage = await _messageRepository.GetByIdAsync(parentMessageId);
        if (parentMessage is null)
            throw new ResourceNotFoundException("Message", parentMessageId);

        var sender = await _userRepository.GetByIdAsync(senderId);
        if (sender is null)
            throw new ResourceNotFoundException("User", senderId);

        var reply = new Message
        {
            SenderId = senderId,
            RecipientId = parentMessage.SenderId,
            Subject = $"Re: {parentMessage.Subject}",
            Body = body,
            ListingId = parentMessage.ListingId,
            ParentMessageId = parentMessageId,
            AttachmentUrls = attachments ?? new List<string>()
        };

        reply.ValidateBeforeSending();
        var created = await _messageRepository.AddAsync(reply);
        parentMessage.AddReply(created);
        await _messageRepository.UpdateAsync(parentMessage);

        return created;
    }

    // Gets messages about a specific listing
    public async Task<List<Message>> GetListingMessagesAsync(Guid listingId)
    {
        return await _messageRepository.GetByListingIdAsync(listingId);
    }

    // Gets conversation about a listing
    public async Task<List<Message>> GetListingConversationAsync(Guid userId1, Guid userId2, Guid listingId)
    {
        return await _messageRepository.GetConversationAboutListingAsync(userId1, userId2, listingId);
    }

    // Gets paginated messages
    public async Task<(List<Message> items, int total)> GetPaginatedMessagesAsync(Guid userId, int pageNumber, int pageSize)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new ResourceNotFoundException("User", userId);

        return await _messageRepository.GetPagedAsync(userId, pageNumber, pageSize);
    }

    // Gets paginated messages using cursor-based pagination to avoid duplicates on concurrent writes
    public async Task<(List<Message> items, Guid? nextCursor)> GetMessagesByCursorAsync(
        Guid userId, Guid? afterId, int pageSize)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new ResourceNotFoundException("User", userId);

        return await _messageRepository.GetPagedByCursorAsync(userId, afterId, pageSize);
    }

    // Gets conversation count
    public async Task<int> GetConversationCountAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new ResourceNotFoundException("User", userId);

        return await _messageRepository.GetConversationCountAsync(userId);
    }

    // Deletes a message
    public async Task DeleteMessageAsync(Guid messageId, Guid requesterId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message is null)
            throw new ResourceNotFoundException("Message", messageId);

        if (message.SenderId != requesterId && message.RecipientId != requesterId)
            throw new UnauthorizedException(requesterId, "delete this message");

        await _messageRepository.DeleteAsync(messageId);
    }

    // Gets flagged messages (admin only)
    public async Task<List<Message>> GetFlaggedMessagesAsync()
    {
        return await _messageRepository.GetFlaggedMessagesAsync();
    }

    // Cleans up old messages (admin only)
    public async Task CleanupOldMessagesAsync(int retentionDays)
    {
        var oldMessages = await _messageRepository.GetOldMessagesAsync(retentionDays);
        foreach (var msg in oldMessages)
        {
            await _messageRepository.DeleteAsync(msg.Id);
        }
    }
}
