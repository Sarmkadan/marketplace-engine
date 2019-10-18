// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Domain.Models;

/// <summary>
/// Represents a private message between marketplace users.
/// </summary>
public class Message
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public User? Sender { get; set; }
    public Guid RecipientId { get; set; }
    public User? Recipient { get; set; }
    public Guid? ListingId { get; set; }
    public Listing? Listing { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool IsFlagged { get; set; }
    public List<string> AttachmentUrls { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public Guid? ParentMessageId { get; set; }
    public Message? ParentMessage { get; set; }
    public List<Message> Replies { get; set; } = [];

    // Validates message content before sending
    public void ValidateBeforeSending()
    {
        if (SenderId == RecipientId)
            throw new ArgumentException("Cannot send message to yourself", nameof(RecipientId));

        if (string.IsNullOrWhiteSpace(Subject) || Subject.Length < 3)
            throw new ArgumentException("Subject must be at least 3 characters", nameof(Subject));

        if (Subject.Length > 100)
            throw new ArgumentException("Subject cannot exceed 100 characters", nameof(Subject));

        if (string.IsNullOrWhiteSpace(Body) || Body.Length < 5)
            throw new ArgumentException("Message body must be at least 5 characters", nameof(Body));

        if (Body.Length > 5000)
            throw new ArgumentException("Message body cannot exceed 5000 characters", nameof(Body));

        if (AttachmentUrls.Count > 5)
            throw new ArgumentException("Cannot exceed 5 attachments per message", nameof(AttachmentUrls));
    }

    // Marks message as read
    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }

    // Marks message as unread
    public void MarkAsUnread()
    {
        IsRead = false;
        ReadAt = null;
    }

    // Flags message as spam/inappropriate
    public void Flag()
    {
        IsFlagged = true;
    }

    // Removes flag from message
    public void RemoveFlag()
    {
        IsFlagged = false;
    }

    // Adds attachment to the message
    public void AddAttachment(string attachmentUrl)
    {
        if (string.IsNullOrWhiteSpace(attachmentUrl))
            return;

        if (AttachmentUrls.Count >= 5)
            throw new InvalidOperationException("Cannot exceed 5 attachments per message");

        if (!AttachmentUrls.Contains(attachmentUrl))
            AttachmentUrls.Add(attachmentUrl);
    }

    // Removes attachment from message
    public bool RemoveAttachment(string attachmentUrl)
    {
        return AttachmentUrls.Remove(attachmentUrl);
    }

    // Adds a reply to this message
    public void AddReply(Message reply)
    {
        if (reply == null)
            throw new ArgumentNullException(nameof(reply));

        reply.ParentMessageId = Id;
        reply.ParentMessage = this;
        Replies.Add(reply);
    }

    // Gets all conversation messages (including replies)
    public List<Message> GetConversation()
    {
        var messages = new List<Message> { this };
        messages.AddRange(Replies);

        foreach (var reply in Replies)
            messages.AddRange(reply.GetConversation().Skip(1));

        return messages;
    }

    // Checks if message is related to a specific listing
    public bool IsAboutListing(Guid listingId) => ListingId == listingId;

    // Gets read status for display
    public string GetReadStatus() => IsRead ? "Read" : "Unread";

    // Checks if message is old (older than 30 days)
    public bool IsOld() => (DateTime.UtcNow - CreatedAt).TotalDays > 30;
}
