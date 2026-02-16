#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Infrastructure.Events;

/// <summary>
/// Event raised when a new listing is created.
/// Used to trigger notifications, indexing, recommendations, etc.
/// </summary>
public class ListingCreatedEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => "listing.created";

    public Guid ListingId { get; set; }
    public Guid SellerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// Event raised when a listing is updated.
/// </summary>
public class ListingUpdatedEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => "listing.updated";

    public Guid ListingId { get; set; }
    public Guid SellerId { get; set; }
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
}

/// <summary>
/// Event raised when a listing is deleted or deactivated.
/// </summary>
public class ListingDeletedEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => "listing.deleted";

    public Guid ListingId { get; set; }
    public Guid SellerId { get; set; }
}

/// <summary>
/// Event raised when a message is sent between users.
/// Used for notifications, logging, spam detection, etc.
/// </summary>
public class MessageSentEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => "message.sent";

    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Event raised when a moderation report is created.
/// Used to alert moderators and track abuse patterns.
/// </summary>
public class ReportCreatedEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => "report.created";

    public Guid ReportId { get; set; }
    public Guid? ListingId { get; set; }
    public Guid ReporterUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Event raised when a moderation report is resolved.
/// Used for audit logging and policy enforcement.
/// </summary>
public class ReportResolvedEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => "report.resolved";

    public Guid ReportId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ResolutionNotes { get; set; }
}

/// <summary>
/// Event raised when user account is created.
/// Used for welcome emails, onboarding, analytics, etc.
/// </summary>
public class UserCreatedEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => "user.created";

    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

/// <summary>
/// Event raised when user email is verified.
/// Used to unlock features that require verified email.
/// </summary>
public class UserEmailVerifiedEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => "user.email_verified";

    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Event raised when a user rating/review is submitted.
/// Used to update seller reputation and detect fraudulent reviews.
/// </summary>
public class RatingSubmittedEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => "rating.submitted";

    public Guid UserId { get; set; }
    public Guid ReviewerId { get; set; }
    public decimal Score { get; set; }
    public string Comment { get; set; } = string.Empty;
}

/// <summary>
/// Event raised when a user activity signal is recorded by the recommendation engine.
/// Consumed by analytics, personalisation pipelines, and audit logging.
/// </summary>
public class UserActivityRecordedEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => "recommendation.activity_recorded";

    /// <summary>Identifier of the user who performed the interaction.</summary>
    public Guid UserId { get; set; }

    /// <summary>Identifier of the listing that was interacted with.</summary>
    public Guid ListingId { get; set; }

    /// <summary>String representation of the <see cref="MarketplaceEngine.Recommendations.SignalType"/>.</summary>
    public string SignalType { get; set; } = string.Empty;
}
