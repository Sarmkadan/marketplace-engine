# ListingCreatedEvent

Domain event published when a new listing is created in the marketplace. Carries the listing’s core attributes and the identity of the seller who created it. Used by consumers to trigger downstream processes such as indexing, notifications, analytics, and moderation.

## API

### `EventId`
Unique identifier for this event instance. Guaranteed to be different from every other event produced in the system.
- **Type:** `System.Guid`
- **Access:** Public read-only property

### `OccurredAt`
UTC timestamp indicating when the listing was created and the event was raised.
- **Type:** `System.DateTime`
- **Access:** Public read-only property

### `ListingId`
Identifier of the newly created listing.
- **Type:** `System.Guid`
- **Access:** Public read-only property

### `SellerId`
Identifier of the user who created the listing.
- **Type:** `System.Guid`
- **Access:** Public read-only property

### `Title`
Human-readable title of the listing.
- **Type:** `System.String`
- **Access:** Public read-only property

### `Category`
Classification under which the listing is grouped (e.g., “Electronics”, “Furniture”).
- **Type:** `System.String`
- **Access:** Public read-only property

### `PreviousStatus`
Optional status of the listing before this event (null for new listings).
- **Type:** `System.String?`
- **Access:** Public read-only property

### `NewStatus`
Status assigned to the listing at creation (e.g., “Draft”, “Published”).
- **Type:** `System.String?`
- **Access:** Public read-only property

### `MessageId`
Identifier of the message or command that triggered the creation (if applicable).
- **Type:** `System.Guid`
- **Access:** Public read-only property

### `SenderId`
Identifier of the actor who sent the triggering message or command.
- **Type:** `System.Guid`
- **Access:** Public read-only property

## Usage
