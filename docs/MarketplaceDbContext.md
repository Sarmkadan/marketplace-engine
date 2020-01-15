# MarketplaceDbContext

Central in-memory database context for the marketplace-engine project. Provides access to core domain collections and utilities for testing, seeding, and validation. All collections are exposed as public `List<T>` members for direct manipulation during tests or initialization.

## API

### `List<User> Users`
Holds all `User` entities in the system. May be modified directly to add, update, or remove users. No automatic validation or change tracking is performed.

### `List<Category> Categories`
Contains all `Category` entities. Direct mutations are allowed; no constraints are enforced by the context.

### `List<Listing> Listings`
Stores all `Listing` entities representing items or services offered in the marketplace. Modifications are permitted without validation.

### `List<Message> Messages`
Collection of all `Message` entities exchanged between users. Can be altered freely.

### `List<ModerationReport> ModerationReports`
Holds all `ModerationReport` instances submitted by users. Direct edits are supported.

### `List<Payment> Payments`
Contains all `Payment` records processed within the system. May be modified directly.

### `List<Review> Reviews`
Stores all `Review` entities submitted by users. Direct mutations are allowed.

### `public static MarketplaceDbContext GetInstance()`
Returns the singleton instance of `MarketplaceDbContext`. No parameters. Always returns the same instance; no lazy initialization or thread synchronization is performed by this method.

### `public void Clear()`
Removes all entities from every collection (`Users`, `Categories`, `Listings`, `Messages`, `ModerationReports`, `Payments`, `Reviews`). Does not reset internal state beyond clearing the lists. No parameters. No return value. Never throws.

### `public void Reset()`
Reinitializes all collections to empty lists and clears any internal state. Equivalent to calling `Clear()` followed by re-creating the collections. No parameters. No return value. Never throws.

### `public int GetTotalEntityCount()`
Returns the total number of entities across all collections (`Users`, `Categories`, `Listings`, `Messages`, `ModerationReports`, `Payments`, `Reviews`). No parameters. Return value is the sum of the counts of all lists. Never throws.

## Usage
