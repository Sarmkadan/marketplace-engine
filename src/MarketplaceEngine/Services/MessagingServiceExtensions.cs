#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.Services;

/// <summary>
/// Extension methods for <see cref="MessagingService"/> providing additional messaging utilities.
/// </summary>
public static class MessagingServiceExtensions
{
    /// <summary>
    /// Sends a message with automatic subject prefix based on listing context.
    /// </summary>
    /// <param name="service">The messaging service instance.</param>
    /// <param name="senderId">Sender user ID.</param>
    /// <param name="recipientId">Recipient user ID.</param>
    /// <param name="body">Message body.</param>
    /// <param name="listingId">Optional listing ID for context.</param>
    /// <param name="attachments">Optional attachments.</param>
    /// <returns>The created message.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static async Task<Message> SendMessageWithContextAsync(
        this MessagingService service,
        Guid senderId,
        Guid recipientId,
        string body,
        Guid? listingId = null,
        List<string>? attachments = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        var subjectPrefix = listingId.HasValue ? "[Listing] " : string.Empty;
        var subject = listingId.HasValue
            ? "Message about your listing"
            : "General message";

        return await service.SendMessageAsync(
            senderId,
            recipientId,
            subjectPrefix + subject,
            body,
            listingId,
            attachments);
    }

    /// <summary>
    /// Gets all messages in a conversation including replies, sorted by creation date.
    /// </summary>
    /// <param name="service">The messaging service instance.</param>
    /// <param name="userId1">First user ID.</param>
    /// <param name="userId2">Second user ID.</param>
    /// <returns>List of messages in chronological order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static async Task<List<Message>> GetCompleteConversationAsync(
        this MessagingService service,
        Guid userId1,
        Guid userId2)
    {
        ArgumentNullException.ThrowIfNull(service);

        var messages = await service.GetConversationAsync(userId1, userId2);

        // Collect all messages including replies
        var allMessages = new List<Message>();
        foreach (var message in messages)
        {
            allMessages.Add(message);
            if (message.Replies.Count > 0)
            {
                allMessages.AddRange(GetAllReplies(message));
            }
        }

        // Sort by creation date
        return allMessages
            .OrderBy(m => m.CreatedAt)
            .ToList();
    }

    /// <summary>
    /// Gets unread message count for a user.
    /// </summary>
    /// <param name="service">The messaging service instance.</param>
    /// <param name="userId">User ID to check.</param>
    /// <returns>Count of unread messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static async Task<int> GetUnreadMessageCountAsync(
        this MessagingService service,
        Guid userId)
    {
        ArgumentNullException.ThrowIfNull(service);

        var messages = await service.GetUnreadMessagesAsync(userId);
        return messages.Count;
    }

    /// <summary>
    /// Gets messages filtered by a specific listing ID.
    /// </summary>
    /// <param name="service">The messaging service instance.</param>
    /// <param name="userId">User ID to filter by.</param>
    /// <param name="listingId">Listing ID to filter by.</param>
    /// <returns>List of messages related to the listing.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static async Task<List<Message>> GetMessagesByListingAsync(
        this MessagingService service,
        Guid userId,
        Guid listingId)
    {
        ArgumentNullException.ThrowIfNull(service);

        var allMessages = new List<Message>();

        // Get sent messages related to listing
        var sentMessages = await service.GetSentMessagesAsync(userId);
        allMessages.AddRange(sentMessages.Where(m => m.ListingId == listingId));

        // Get received messages related to listing
        var receivedMessages = await service.GetReceivedMessagesAsync(userId);
        allMessages.AddRange(receivedMessages.Where(m => m.ListingId == listingId));

        return allMessages
            .OrderByDescending(m => m.CreatedAt)
            .ToList();
    }

    /// <summary>
    /// Recursively gets all replies from a message including nested replies.
    /// </summary>
    /// <param name="message">The parent message to get replies for.</param>
    /// <returns>Flattened collection of all replies.</returns>
    private static IEnumerable<Message> GetAllReplies(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        foreach (var reply in message.Replies)
        {
            yield return reply;
            foreach (var nestedReply in GetAllReplies(reply))
            {
                yield return nestedReply;
            }
        }
    }
}