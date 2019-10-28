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

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Handles messaging between buyers and sellers.
/// Messages are not cached individually to ensure real-time delivery notifications.
/// </summary>
[ApiController]
[Route("api/v1/messages")]
public class MessagesController : ControllerBase
{
    private readonly MessagingService _messagingService;
    private readonly CacheService _cacheService;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(
        MessagingService messagingService,
        CacheService cacheService,
        ILogger<MessagesController> logger)
    {
        _messagingService = messagingService;
        _cacheService = cacheService;
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

        var cacheKey = $"conversations:{userId}";
        var cached = await _cacheService.GetAsync<List<ConversationDto>>(cacheKey);

        if (cached is not null)
        {
            _logger.LogInformation("Cache hit for conversations");
            return Ok(cached);
        }

        var conversations = await _messagingService.GetUserConversationsAsync(userId);
        var dtos = conversations.Select(c => new ConversationDto(c)).ToList();

        // Cache conversation list for 3 minutes
        await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(3));

        return Ok(dtos);
    }

    /// <summary>
    /// Retrieves messages in a specific conversation with pagination.
    /// Uses message ID ranges instead of caching individual messages
    /// to handle real-time message delivery correctly.
    /// </summary>
    [HttpGet("conversations/{conversationId}/messages")]
    [ProducesResponseType(typeof(PaginatedResponse<MessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversationMessages(
        Guid conversationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        _logger.LogInformation("Fetching messages for conversation: {ConversationId}", conversationId);

        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest("Invalid pagination parameters");

        var messages = await _messagingService.GetConversationMessagesAsync(conversationId, page, pageSize);
        var response = new PaginatedResponse<MessageDto>
        {
            Items = messages.Select(m => new MessageDto(m)).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = messages.Count
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
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        var created = await _messagingService.SendMessageAsync(message);

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

        var message = await _messagingService.GetMessageAsync(id);
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

        var message = await _messagingService.GetMessageAsync(id);
        if (message is null)
        {
            return NotFound();
        }

        message.IsRead = true;
        await _messagingService.UpdateMessageAsync(message);

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

        var message = await _messagingService.GetMessageAsync(id);
        if (message is null)
        {
            return NotFound();
        }

        await _messagingService.DeleteMessageAsync(id);

        return NoContent();
    }
}
