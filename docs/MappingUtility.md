# MappingUtility

`MappingUtility` is a static utility class responsible for converting domain entities into their corresponding Data Transfer Objects (DTOs). It centralises mapping logic for the core marketplace types—listings, users, messages, and moderation reports—ensuring consistent transformation from internal models to API-facing contracts. Both single-object and batch conversions are provided.

## API

### `public static ListingDto ToDto`

Converts a `Listing` entity to a `ListingDto`.

- **Parameters:**  
  `Listing listing` — the domain entity to map. Must not be `null`.

- **Returns:**  
  `ListingDto` — a fully populated DTO representing the listing.

- **Throws:**  
  `ArgumentNullException` when `listing` is `null`.

---

### `public static UserDto ToDto`

Converts a `User` entity to a `UserDto`.

- **Parameters:**  
  `User user` — the domain entity to map. Must not be `null`.

- **Returns:**  
  `UserDto` — a fully populated DTO representing the user.

- **Throws:**  
  `ArgumentNullException` when `user` is `null`.

---

### `public static MessageDto ToDto`

Converts a `Message` entity to a `MessageDto`.

- **Parameters:**  
  `Message message` — the domain entity to map. Must not be `null`.

- **Returns:**  
  `MessageDto` — a fully populated DTO representing the message.

- **Throws:**  
  `ArgumentNullException` when `message` is `null`.

---

### `public static ModerationReportDto ToDto`

Converts a `ModerationReport` entity to a `ModerationReportDto`.

- **Parameters:**  
  `ModerationReport report` — the domain entity to map. Must not be `null`.

- **Returns:**  
  `ModerationReportDto` — a fully populated DTO representing the moderation report.

- **Throws:**  
  `ArgumentNullException` when `report` is `null`.

---

### `public static List<ListingDto> ToListingDtos`

Converts a collection of `Listing` entities to a list of `ListingDto` objects.

- **Parameters:**  
  `IEnumerable<Listing> listings` — the source entities. May be empty but not `null`.

- **Returns:**  
  `List<ListingDto>` — a list where each element corresponds to an input entity, preserving enumeration order. An empty input yields an empty list.

- **Throws:**  
  `ArgumentNullException` when `listings` is `null`. Individual `null` elements within the collection cause an `ArgumentNullException` during iteration.

---

### `public static List<UserDto> ToUserDtos`

Converts a collection of `User` entities to a list of `UserDto` objects.

- **Parameters:**  
  `IEnumerable<User> users` — the source entities. May be empty but not `null`.

- **Returns:**  
  `List<UserDto>` — a list where each element corresponds to an input entity, preserving enumeration order. An empty input yields an empty list.

- **Throws:**  
  `ArgumentNullException` when `users` is `null`. Individual `null` elements within the collection cause an `ArgumentNullException` during iteration.

---

### `public static List<MessageDto> ToMessageDtos`

Converts a collection of `Message` entities to a list of `MessageDto` objects.

- **Parameters:**  
  `IEnumerable<Message> messages` — the source entities. May be empty but not `null`.

- **Returns:**  
  `List<MessageDto>` — a list where each element corresponds to an input entity, preserving enumeration order. An empty input yields an empty list.

- **Throws:**  
  `ArgumentNullException` when `messages` is `null`. Individual `null` elements within the collection cause an `ArgumentNullException` during iteration.

## Usage

### Example 1: Mapping a single entity for an API response

```csharp
public ListingDto GetListingById(Guid listingId)
{
    Listing listing = _listingRepository.GetById(listingId);

    if (listing == null)
        return null;

    return MappingUtility.ToDto(listing);
}
```

### Example 2: Batch-mapping search results

```csharp
public List<UserDto> SearchUsers(string query)
{
    IEnumerable<User> users = _userRepository.FindByQuery(query);

    // Safe to pass an empty enumerable — returns an empty list.
    return MappingUtility.ToUserDtos(users);
}
```

## Notes

- All methods are static and stateless; they are safe to call concurrently from multiple threads without external synchronisation.
- The batch methods (`ToListingDtos`, `ToUserDtos`, `ToMessageDtos`) iterate the source enumerable exactly once. If the source represents a deferred database query, mapping triggers execution of that query.
- `null` elements within a collection are not tolerated. The batch methods throw as soon as a `null` element is encountered, leaving any previously mapped items in the returned list. Callers should filter or guard against `null` entries beforehand if the source collection may contain them.
- There is no batch method for `ModerationReportDto`. Multiple reports must be mapped by calling the single-object overload in a loop or via a LINQ `Select` projection.
