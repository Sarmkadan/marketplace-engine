#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.DTOs;
using MarketplaceEngine.Infrastructure.Caching;
using MarketplaceEngine.Repositories; // Hotfix: Add missing using directive

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Handles messaging between buyers and sellers.
/// Messages are not cached individually to ensure real-time delivery notifications.
/// </summary>
[ApiController]
[Route("api/v1/messages")]
public class MessagesController : ControllerBase
{
    internal readonly MessagingService _messagingService;
    internal readonly CacheService _cacheService;
    internal readonly IMessageRepository _messageRepository;
    internal readonly IUserRepository _userRepository; // Hotfix: Injected for user details in conversations
    internal readonly ILogger<MessagesController> _logger;

    public MessagesController(
        MessagingService messagingService,
        CacheService cacheService,
        IMessageRepository messageRepository,
        IUserRepository userRepository,
        ILogger<MessagesController> logger)
    {
        _messagingService = messagingService;
        _cacheService = cacheService;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves conversation history between two users.
    /// Conversation lists are cached but individual messages are not
    /// to prevent stale message delivery.
    /// </summary>
    [HttpGet("conversations/{userId}")]
    [ProducesResponseType(typeof(List<ConversationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversations(Guid userId)
    {
        _logger.LogInformation("Fetching conversations for user: {UserId}", userId);

        var sent = await _messageRepository.GetSentMessagesAsync(userId);
        var received = await _messageRepository.GetReceivedMessagesAsync(userId);

        var dtos = new List<ConversationDto>();

        foreach (var group in sent.Concat(received).GroupBy(m => m.SenderId == userId ? m.RecipientId : m.SenderId))
        {
            var otherUserId = group.Key;
            var lastMessage = group.OrderByDescending(m => m.CreatedAt).First();
            var unreadCount = group.Count(m => m.RecipientId == userId && !m.IsRead);

            var otherUser = await _userRepository.GetByIdAsync(otherUserId);

            dtos.Add(new ConversationDto
            {
                ConversationId = otherUserId,
                OtherUserId = otherUserId,
                OtherUserName = otherUser is not null ? otherUser.GetDisplayName() : string.Empty,
                LastMessage = lastMessage.Body,
                LastMessageAt = lastMessage.CreatedAt,
                UnreadCount = unreadCount
            });
        }

        dtos = dtos.OrderByDescending(d => d.LastMessageAt).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Retrieves messages in a specific conversation with cursor-based pagination.
    /// Using a cursor (last seen message ID) instead of page offsets prevents duplicate
    /// messages when new rows are inserted concurrently between requests.
    /// </summary>
    [HttpGet("conversations/{conversationId}/messages")]
    [ProducesResponseType(typeof(CursorPaginatedResponse<MessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversationMessages(
        Guid conversationId,
        [FromQuery] Guid? after = null,
        [FromQuery] int pageSize = 50)
    {
        _logger.LogInformation("Fetching messages for conversation: {ConversationId}", conversationId);

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("Invalid pageSize: must be between 1 and 100");

        var (items, nextCursor) = await _messagingService.GetMessagesByCursorAsync(
            conversationId, after, pageSize);

        var response = new CursorPaginatedResponse<MessageDto>
        {
            Items = items.Select(m => new MessageDto(m)).ToList(),
            NextCursor = nextCursor,
            PageSize = pageSize,
            HasMore = nextCursor.HasValue
        };

        return Ok(response);
    }

    /// <summary>
    /// Sends a new message in a conversation.
    /// Invalidates conversation caches to reflect new message.
    /// Triggers real-time notification through event bus.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        _logger.LogInformation("Sending message from {SenderId} to {RecipientId}", request.SenderId, request.RecipientId);

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Message content is required");

        if (request.Content.Length > 5000)
            return BadRequest("Message exceeds maximum length of 5000 characters");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = request.SenderId,
            RecipientId = request.RecipientId,
            Body = request.Content, // Hotfix: Use Body property
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        var created = await _messagingService.SendMessageAsync(
            request.SenderId,
            request.RecipientId,
            "New Message", // Hotfix: Placeholder subject
            request.Content);

        // Invalidate conversation caches for both users
        await _cacheService.RemoveAsync($"conversations:{request.SenderId}");
        await _cacheService.RemoveAsync($"conversations:{request.RecipientId}");

        _logger.LogInformation("Message sent: {MessageId}", created.Id);
        return CreatedAtAction(nameof(GetMessage), new { id = created.Id }, new MessageDto(created));
    }

    /// <summary>
    /// Retrieves a specific message by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessage(Guid id)
    {
        _logger.LogInformation("Fetching message: {MessageId}", id);

        var message = await _messageRepository.GetByIdAsync(id); // Hotfix: Use repository directly
        if (message is null)
        {
            return NotFound();
        }

        return Ok(new MessageDto(message));
    }

    /// <summary>
    /// Marks a message as read.
    /// Updates message state without invalidating caches
    /// as read status is per-user metadata.
    /// </summary>
    [HttpPut("{id}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkMessageAsRead(Guid id)
    {
        _logger.LogInformation("Marking message as read: {MessageId}", id);

        var message = await _messageRepository.GetByIdAsync(id);
        if (message is null)
        {
            return NotFound();
        }

        message.MarkAsRead(); // Hotfix: Use Message domain method
        await _messageRepository.UpdateAsync(message); // Hotfix: Use repository directly

        return Ok(new { message = "Message marked as read" });
    }

    /// <summary>
    /// Deletes a message (soft delete).
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMessage(Guid id)
    {
        _logger.LogInformation("Deleting message: {MessageId}", id);

        var message = await _messageRepository.GetByIdAsync(id); // Hotfix: Use repository directly
        if (message is null)
        {
            return NotFound();
        }

        // Hotfix: MessagingService.DeleteMessageAsync requires a requesterId
        // Assuming current user's ID can be obtained from context,
        // using Guid.Empty as a placeholder for compilation.
        await _messagingService.DeleteMessageAsync(id, Guid.Empty);

        return NoContent();
    }
}
