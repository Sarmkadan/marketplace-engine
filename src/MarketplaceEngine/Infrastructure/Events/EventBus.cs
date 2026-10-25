#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Infrastructure.Events;

/// <summary>
/// Event bus for pub-sub pattern implementation.
/// Allows loose coupling between different parts of the application
/// by using events instead of direct method calls.
/// Handlers can react to events asynchronously.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Gets the unique identifier of this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the UTC timestamp at which the event occurred.
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Gets the name of the event type, used for logging.
    /// </summary>
    string EventType { get; }
}

/// <summary>
/// Generic event handler interface.
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    /// <summary>
    /// Handles a published event.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <returns>A task that completes when handling is finished.</returns>
    Task HandleAsync(TEvent @event);
}

/// <summary>
/// Event bus implementation for publishing and subscribing to events.
/// In production, replace with a message bus like RabbitMQ or Azure Service Bus.
/// </summary>
public class EventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();
    private readonly ILogger<EventBus> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventBus"/> class.
    /// </summary>
    /// <param name="logger">Logger used to report subscriptions, publishing and handler errors.</param>
    public EventBus(ILogger<EventBus> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Subscribes a handler to an event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
    /// <param name="handler">The asynchronous delegate invoked when an event of this type is published.</param>
    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(TEvent);

        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Delegate>();
        }

        _handlers[eventType].Add(handler);
        _logger.LogInformation("Handler subscribed to event type: {EventType}", eventType.Name);
    }

    /// <summary>
    /// Publishes an event to all registered handlers.
    /// Handlers are invoked asynchronously.
    /// </summary>
    /// <typeparam name="TEvent">The event type being published.</typeparam>
    /// <param name="event">The event to publish.</param>
    /// <returns>A task that completes when all handlers have finished.</returns>
    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        var eventType = typeof(TEvent);

        _logger.LogInformation(
            "Publishing event: {EventType}, EventId: {EventId}",
            @event.EventType,
            @event.EventId);

        if (!_handlers.ContainsKey(eventType))
        {
            _logger.LogDebug("No handlers registered for event type: {EventType}", eventType.Name);
            return;
        }

        var handlerList = _handlers[eventType];

        // Invoke all handlers asynchronously
        var tasks = handlerList
            .Cast<Func<TEvent, Task>>()
            .Select(handler => InvokeHandlerAsync(handler, @event))
            .ToList();

        try
        {
            await Task.WhenAll(tasks);
            _logger.LogInformation(
                "Event published successfully: {EventType}, handled by {Count} handlers",
                @event.EventType,
                handlerList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event: {EventType}", @event.EventType);
            throw;
        }
    }

    /// <summary>
    /// Unsubscribes a handler from an event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to unsubscribe from.</typeparam>
    /// <param name="handler">The handler delegate previously passed to <see cref="Subscribe{TEvent}"/>.</param>
    public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(TEvent);

        if (_handlers.ContainsKey(eventType))
        {
            _handlers[eventType].Remove(handler);
            _logger.LogInformation("Handler unsubscribed from event type: {EventType}", eventType.Name);
        }
    }

    /// <summary>
    /// Unsubscribes all handlers from a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type whose handlers should be removed.</typeparam>
    public void UnsubscribeAll<TEvent>() where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        if (_handlers.ContainsKey(eventType))
        {
            _handlers.Remove(eventType);
            _logger.LogInformation("All handlers unsubscribed from event type: {EventType}", eventType.Name);
        }
    }

    private async Task InvokeHandlerAsync<TEvent>(Func<TEvent, Task> handler, TEvent @event)
        where TEvent : IEvent
    {
        try
        {
            await handler(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in event handler for {EventType}", @event.EventType);
            // Continue processing other handlers even if one fails
        }
    }
}
