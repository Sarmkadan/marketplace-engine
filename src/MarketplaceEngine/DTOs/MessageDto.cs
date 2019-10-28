#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Message data transfer object for API responses.
/// </summary>
public class MessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public MessageDto() { }

    public MessageDto(Message message)
    {
        Id = message.Id;
        SenderId = message.SenderId;
        RecipientId = message.RecipientId;
        Content = message.Content;
        IsRead = message.IsRead;
        CreatedAt = message.CreatedAt;
    }
}

/// <summary>
/// Conversation summary for listing user conversations.
/// </summary>
public class ConversationDto
{
    public Guid ConversationId { get; set; }
    public Guid OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastMessageAt { get; set; }
    public int UnreadCount { get; set; }

    public ConversationDto() { }

    public ConversationDto(Message conversation)
    {
        // In a real system, this would be mapped from a Conversation entity
        // For now, we use the last message
        ConversationId = conversation.Id;
        LastMessage = conversation.Content;
        LastMessageAt = conversation.CreatedAt;
    }
}

/// <summary>
/// Request to send a new message.
/// </summary>
public class SendMessageRequest
{
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public string Content { get; set; } = string.Empty;
}
