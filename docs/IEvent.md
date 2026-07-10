# IEvent

The `IEvent` interface provides a standardized mechanism for decoupling components within the `marketplace-engine` by facilitating asynchronous, type-safe event-driven communication. It serves as an abstraction layer for an underlying event bus, allowing modules to subscribe to, publish, and manage event handlers without direct dependencies on one another or the specific implementation details of the message transport.

## API

### EventBus
Provides access to the underlying `EventBus` instance associated with this `IEvent` implementation.

### Subscribe&lt;TEvent&gt;
Registers an event handler to be invoked whenever an event of type `TEvent` is published. 
*   **Parameters:** `TEvent` (the event type), and a delegate representing the handler logic.
*   **Return Value:** `void`.

### PublishAsync&lt;TEvent&gt;
Publishes an event of type `TEvent` to all registered subscribers asynchronously.
*   **Parameters:** `TEvent` (the event instance).
*   **Return Value:** A `Task` representing the asynchronous publication operation.
*   **Exceptions:** May throw exceptions if the underlying message transport fails or if event validation fails.

### Unsubscribe&lt;TEvent&gt;
Removes a specific previously registered event handler for the specified `TEvent` type.
*   **Parameters:** `TEvent` (the event type), and the delegate instance to remove.
*   **Return Value:** `void`.

### UnsubscribeAll&lt;TEvent&gt;
Removes all registered event handlers for the specified `TEvent` type.
*   **Parameters:** `TEvent` (the event type).
*   **Return Value:** `void`.

## Usage

### Example 1: Subscribing to and Publishing an Event
```csharp
// Subscribe to an event
iEvent.Subscribe<OrderPlacedEvent>(HandleOrderPlaced);

// Define the handler
private void HandleOrderPlaced(OrderPlacedEvent eventData)
{
    Console.WriteLine($"Order {eventData.OrderId} was placed.");
}

// Publish an event
await iEvent.PublishAsync(new OrderPlacedEvent(12345));
```

### Example 2: Managing Subscriptions
```csharp
// Remove a specific handler
iEvent.Unsubscribe<OrderPlacedEvent>(HandleOrderPlaced);

// Remove all handlers for a specific event type
iEvent.UnsubscribeAll<OrderPlacedEvent>();
```

## Notes

*   **Thread Safety:** While `IEvent` provides a thread-safe interface for managing subscriptions and publishing, the event handlers themselves must be implemented with thread safety in mind if they are invoked concurrently by the underlying bus implementation.
*   **Asynchronous Execution:** The `PublishAsync` method is non-blocking. The order of execution for multiple handlers registered to the same event type is not guaranteed.
*   **Edge Cases:** If `Unsubscribe` is called with a handler that is not currently registered, the operation is typically treated as a no-op, though behavior may vary depending on the underlying implementation. Ensure proper lifecycle management of handlers to prevent memory leaks, especially in transient components.
