# User

Represents a registered participant in the marketplace, holding identity data, role-based permissions, verification state, activity metrics, and optional profile details. Instances are persisted and used throughout the engine for authentication, authorization, listing ownership, and transaction tracking.

## API

### Properties

- **`Guid Id`**  
  Unique immutable identifier assigned at creation. Used as the primary key in persistence and for cross-referencing listings, sales, and ratings.

- **`string Email`**  
  The user’s email address. Must be non‑null, non‑empty, and in a valid format as enforced by `ValidateEmail`. Serves as a login credential and contact channel.

- **`string FullName`**  
  Display name for the user. Required for profile completeness; validated by `ValidateProfile` to ensure it is non‑null, non‑empty, and within allowed length limits.

- **`string? Phone`**  
  Optional contact phone number. Null when not provided. Format is not enforced at the type level but may be validated externally before persistence.

- **`string? ProfileImageUrl`**  
  Optional URL pointing to the user’s avatar or profile image. Null when no image is set. No format validation is performed by this type.

- **`string? Bio`**  
  Optional free‑text biography. Null when empty. Length constraints, if any, are applied by `ValidateProfile`.

- **`UserRole Role`**  
  Enum value defining the user’s permissions (e.g., Buyer, Seller, Admin). Drives authorization decisions across the engine.

- **`Location? Location`**  
  Optional geographic location of the user. Null when not specified. Used for proximity‑based searches and shipping calculations.

- **`Rating? Rating`**  
  Optional aggregate rating computed from completed transactions. Null when the user has not yet received any ratings. Contains average score and count.

- **`bool IsVerified`**  
  Indicates whether the user has completed the verification flow (e.g., email confirmation). Set to `true` after successful token validation.

- **`bool IsActive`**  
  Controls whether the account is allowed to perform actions. Inactive users are typically prevented from logging in or creating listings. Set administratively or via self‑deactivation.

- **`int TotalListings`**  
  Running count of listings the user has ever published. Incremented atomically by the engine when a listing is created; never decremented on removal.

- **`int TotalSales`**  
  Running count of completed sales where this user acted as the seller. Updated by the engine upon successful transaction finalization.

- **`DateTime CreatedAt`**  
  UTC timestamp of initial record creation. Immutable after insertion.

- **`DateTime? UpdatedAt`**  
  UTC timestamp of the last modification to any mutable field. Null if the record has never been updated since creation.

- **`DateTime? LastActiveAt`**  
  UTC timestamp of the user’s most recent authenticated activity. Null if the user has never logged in. Updated on each login or token refresh.

- **`string? VerificationToken`**  
  Cryptographic token generated for email verification. Null when verification is complete or was never initiated. Cleared upon successful verification.

- **`DateTime? VerificationExpiry`**  
  UTC deadline by which the `VerificationToken` must be consumed. Null when no verification is pending. Expired tokens are rejected during validation.

### Methods

- **`void ValidateEmail()`**  
  Validates the `Email` property against required format rules (non‑null, non‑empty, conforming to a valid email pattern).  
  **Throws:** `ValidationException` (or a domain‑specific exception) when the email is missing or malformed.  
  **Returns:** Nothing. Mutates no state.

- **`void ValidateProfile()`**  
  Validates profile completeness by checking `FullName` (required, length constraints) and optionally `Bio` and `ProfileImageUrl` if business rules mandate constraints on those fields.  
  **Throws:** `ValidationException` when required fields are missing or exceed allowed lengths.  
  **Returns:** Nothing. Mutates no state.

## Usage

### Example 1: Creating and validating a new user

```csharp
var user = new User
{
    Id = Guid.NewGuid(),
    Email = "alice@example.com",
    FullName = "Alice Johnson",
    Role = UserRole.Buyer,
    IsActive = true,
    CreatedAt = DateTime.UtcNow
};

// Enforce data integrity before persistence
user.ValidateEmail();
user.ValidateProfile();

// Proceed to save via repository
await userRepository.AddAsync(user);
```

### Example 2: Initiating email verification

```csharp
// Assume user is already persisted and not yet verified
if (!user.IsVerified && user.VerificationToken == null)
{
    user.VerificationToken = Guid.NewGuid().ToString("N");
    user.VerificationExpiry = DateTime.UtcNow.AddHours(24);
    user.UpdatedAt = DateTime.UtcNow;

    await userRepository.UpdateAsync(user);

    // Send token via email service (out of scope)
    await emailService.SendVerificationAsync(user.Email, user.VerificationToken);
}
```

## Notes

- **Nullability:** Reference‑type fields marked `?` can be null and must be null‑checked by consumers before dereferencing. `Location` and `Rating` are value types wrapped in `Nullable<T>`; accessing their members without a null guard will throw at runtime.
- **Validation ordering:** Call `ValidateEmail` and `ValidateProfile` before persisting a new record or updating critical fields. These methods do not validate `Phone`, `ProfileImageUrl`, or `Bio` format beyond what `ValidateProfile` explicitly covers.
- **Token lifecycle:** `VerificationToken` and `VerificationExpiry` must be cleared together upon successful verification. Failing to clear both leaves the user in an inconsistent state.
- **Count integrity:** `TotalListings` and `TotalSales` are maintained by the engine, not by this type’s methods. Manual modification will cause metric drift.
- **Thread safety:** This type is not inherently thread‑safe. Concurrent mutations to mutable fields (e.g., `IsActive`, `LastActiveAt`, counters) must be synchronized externally, typically through database‑level concurrency controls or optimistic locking on the repository.
- **Immutability of identity:** `Id`, `Email`, and `CreatedAt` should not be changed after initial persistence. Changing `Email` would orphan verification state and break audit trails; prefer a dedicated email‑change flow that re‑verifies the new address.
