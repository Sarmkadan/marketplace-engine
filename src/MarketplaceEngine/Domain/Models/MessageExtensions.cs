#nullable enable

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Extension methods for the Message class providing additional functionality.
/// </summary>
public static class MessageExtensions
{
    /// <summary>
    /// Determines if the message is part of a conversation thread (has replies or is a reply itself).
    /// </summary>
    /// <param name="message">The message to check</param>
    /// <returns>True if the message is part of a conversation thread; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    public static bool IsInThread(this Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return message.ParentMessageId.HasValue || message.Replies.Count > 0;
    }

    /// <summary>
    /// Gets the conversation depth level of the message (0 for root messages, 1 for direct replies, etc.).
    /// </summary>
    /// <remarks>
    /// This method follows the parent chain to determine depth. Be aware that circular references in the parent chain
    /// could cause infinite loops, though the domain model should prevent this through proper entity relationships.
    /// </remarks>
    /// <param name="message">The message to check</param>
    /// <returns>The depth level in the conversation thread.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    public static int GetConversationDepth(this Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        int depth = 0;
        var current = message;

        while (current.ParentMessage is not null)
        {
            depth++;
            current = current.ParentMessage;
        }

        return depth;
    }

    /// <summary>
    /// Gets the total number of messages in this conversation thread (including all replies).
    /// </summary>
    /// <param name="message">The root message of the conversation</param>
    /// <returns>The total count of messages in the conversation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    public static int GetConversationCount(this Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return message.GetConversation().Count;
    }

    /// <summary>
    /// Creates a reply message with the specified body and sender.
    /// </summary>
    /// <param name="message">The original message being replied to</param>
    /// <param name="replyBody">The body content of the reply</param>
    /// <param name="senderId">The ID of the sender creating the reply</param>
    /// <returns>A new Message instance configured as a reply.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="replyBody"/> is null or whitespace.</exception>
    public static Message CreateReply(this Message message, string replyBody, Guid senderId)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentException.ThrowIfNullOrEmpty(replyBody);

        var reply = new Message
        {
            SenderId = senderId,
            RecipientId = message.SenderId,
            ListingId = message.ListingId,
            Subject = message.Subject,
            Body = replyBody,
            CreatedAt = DateTime.UtcNow,
            ParentMessageId = message.Id,
            ParentMessage = message
        };

        return reply;
    }
}