# Category

Represents a hierarchical grouping of listings within the marketplace engine, providing identity, metadata, and basic tree‑navigation capabilities.

## API

### Id
- **Purpose:** Unique identifier for the category.
- **Type:** `Guid`
- **Remarks:** Should be set to a non‑empty value before the category is persisted.

### Name
- **Purpose:** Human‑readable name of the category.
- **Type:** `string`
- **Remarks:** Required; used in UI and path generation.

### Slug
- **Purpose:** URL‑friendly identifier derived from the name.
- **Type:** `string`
- **Remarks:** Must be unique within the same parent hierarchy.

### Description
- **Purpose:** Optional detailed description of the category.
- **Type:** `string?`
- **Remarks:** May be `null` or empty.

### IconUrl
- **Purpose:** Optional URL pointing to an icon representing the category.
- **Type:** `string?`
- **Remarks:** May be `null`; if provided should be a valid absolute or relative URL.

### ParentCategoryId
- **Purpose:** Foreign key to the parent category, if any.
- **Type:** `Guid?`
- **Remarks:** `null` indicates a top‑level category.

### ParentCategory
- **Purpose:** Navigation property to the parent category instance.
- **Type:** `Category?`
- **Remarks:** Set by the ORM or manually; may be `null` when `ParentCategoryId` is `null`.

### SubCategories
- **Purpose:** Collection of direct child categories.
- **Type:** `List<Category>`
- **Remarks:** Should be initialized (e.g., via `ValidateAndInitialize`) before use.

### Listings
- **Purpose:** Collection of listings assigned to this category.
- **Type:** `List<Listing>`
- **Remarks:** Should be initialized before use.

### ListingCount
- **Purpose:** Cached count of listings in this category (including those in sub‑categories, depending on implementation).
- **Type:** `int`
- **Remarks:** Updated via `IncrementListingCount` or similar logic.

### IsActive
- **Purpose:** Indicates whether the category is visible and usable in the marketplace.
- **Type:** `bool`
- **Remarks:** Inactive categories are typically hidden from UI but may still exist in data store.

### DisplayOrder
- **Purpose:** Determines the order in which categories are sorted when listed alongside siblings.
- **Type:** `int`
- **Remarks:** Lower values appear first.

### CreatedAt
- **Purpose:** Timestamp when the category was first created.
- **Type:** `DateTime`
- **Remarks:** Set automatically on insertion.

### UpdatedAt
- **Purpose:** Timestamp when the category was last modified.
- **Type:** `DateTime?`
- **Remarks:** `null` if the category has never been updated after creation.

### ValidateAndInitialize
- **Purpose:** Ensures the category is in a consistent state; assigns default values to collections and validates required fields.
- **Signature:** `public void ValidateAndInitialize()`
- **Parameters:** None
- **Return:** None
- **Exceptions:** 
  - `ArgumentNullException` if `Name` is `null` or empty.
  - `InvalidOperationException` if `Id` equals `Guid.Empty`.

### AddSubCategory
- **Purpose:** Adds a child category to the `SubCategories` collection and sets its parent relationship.
- **Signature:** `public void AddSubCategory(Category subCategory)`
- **Parameters:** 
  - `subCategory`: The category to add as a child.
- **Return:** None
- **Exceptions:** 
  - `ArgumentNullException` if `subCategory` is `null`.
  - `InvalidOperationException` if `subCategory` already has a parent or if adding would create a circular reference.

### RemoveSubCategory
- **Purpose:** Attempts to remove this category from its parent’s `SubCategories` collection.
- **Signature:** `public bool RemoveSubCategory()`
- **Parameters:** None
- **Return:** `true` if the category was successfully removed from its parent; `false` if it has no parent or was not found in the parent’s collection.
- **Exceptions:** 
  - `InvalidOperationException` if the parent category’s `SubCategories` list is `null`.

### HasActiveListings
- **Purpose:** Checks whether any listing directly assigned to this category is active.
- **Signature:** `public bool HasActiveListings()`
- **Parameters:** None
- **Return:** `true` if at least one listing in `Listings` is active; otherwise `false`.
- **Exceptions:** None.

### GetFullPath
- **Purpose:** Constructs a hierarchical path string from the root category down to this category, using `Slug` values.
- **Signature:** `public string GetFullPath()`
- **Parameters:** None
- **Return:** A string like `"parent/child/this"`; for a top‑level category returns just its `Slug`.
- **Exceptions:** 
  - `InvalidOperationException` if a circular reference is detected while traversing parents.

### IncrementListingCount
- **Purpose:** Increments the `ListingCount` field by one.
- **Signature:** `public void IncrementListingCount()`
- **Parameters:** None
- **Return:** None
- **Exceptions:** 
  - `OverflowException` if the increment would exceed `Int32.MaxValue`.

## Usage

```csharp
// Example 1: Creating a top‑level category and adding a sub‑category.
var electronics = new Category
{
    Id = Guid.NewGuid(),
    Name = "Electronics",
    Slug = "electronics",
    IsActive = true,
    DisplayOrder = 0
};
electronics.ValidateAndInitialize(); // ensures SubCategories and Listings are initialized

var laptops = new Category
{
    Id = Guid.NewGuid(),
    Name = "Laptops",
    Slug = "laptops",
    IsActive = true,
    DisplayOrder = 0,
    ParentCategoryId = electronics.Id,
    ParentCategory = electronics
};
electronics.AddSubCategory(laptops);

// Verify the hierarchy.
Console.WriteLine(electronics.GetFullPath()); // Output: electronics
Console.WriteLine(laptops.GetFullPath());     // Output: electronics/laptops
```

```csharp
// Example 2: Checking for active listings and safely removing a category.
if (laptops.HasActiveListings())
{
    // Cannot remove while there are active listings.
    throw new InvalidOperationException("Cannot remove category with active listings.");
}

// Remove the laptops category from its parent.
bool removed = laptops.RemoveSubCategory();
Console.WriteLine(removed ? "Removed" : "Removal failed");

// After removal, the parent no longer references this category.
Console.WriteLine(electronics.SubCategories.Count); // 0
```

## Notes

- The `SubCategories` and `Listings` collections are **not** initialized by the default constructor; calling `ValidateAndInitialize` (or manually assigning new `List<Category>()` / `new List<Listing>()`) is required before adding items to avoid `NullReferenceException`.
- `ValidateAndInitialize` does **not** automatically set `ParentCategory` or `ParentCategoryId`; those must be supplied by the caller.
- The class is **not thread‑safe**. Concurrent calls to `AddSubCategory`, `RemoveSubCategory`, `IncrementListingCount`, or direct modification of the collections can lead to inconsistent state. External synchronization (e.g., locking) is necessary when accessed from multiple threads.
- Circular parent checks in `AddSubCategory` and `GetFullPath` rely on reference equality; manually breaking the parent link without updating `ParentCategoryId` may cause inconsistencies.
- `ListingCount` is a cached value; the implementation assumes that callers keep it in sync via `IncrementListingCount` (or similar decrement logic). Relying solely on this field without proper updates may yield inaccurate counts.
