# RecommendationDto

The `RecommendationDto` class serves as a data transfer object within the `marketplace-engine` project, encapsulating the structure for recommendation responses returned to clients. It supports both singular recommendation details and aggregated recommendation sets, distinguishing between personalized and generic strategies while providing metadata regarding the generation context, scoring, and underlying logic used to derive the suggestions.

## API

### Properties

#### `ListingId`
*   **Type:** `public Guid`
*   **Purpose:** Identifies the specific marketplace listing associated with this recommendation item.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The unique identifier of the listing.
*   **Throws:** None.

#### `Title`
*   **Type:** `public string`
*   **Purpose:** Holds the display title of the recommended listing.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The title string.
*   **Throws:** None.

#### `Price`
*   **Type:** `public decimal`
*   **Purpose:** Represents the current price of the recommended item.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The numeric price value.
*   **Throws:** None.

#### `Currency`
*   **Type:** `public string`
*   **Purpose:** Specifies the ISO currency code (e.g., "USD", "EUR") associated with the `Price`.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The currency code string.
*   **Throws:** None.

#### `ThumbnailUrl`
*   **Type:** `public string?`
*   **Purpose:** Provides an optional URL to the thumbnail image for the recommended listing.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The image URL or `null` if no image is available.
*   **Throws:** None.

#### `Score`
*   **Type:** `public double`
*   **Purpose:** Indicates the relevance score calculated by the recommendation engine for this specific item.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The numerical score.
*   **Throws:** None.

#### `Reason`
*   **Type:** `public string`
*   **Purpose:** Contains a human-readable explanation describing why this specific item was recommended.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The reason string.
*   **Throws:** None.

#### `CategoryId`
*   **Type:** `public Guid`
*   **Purpose:** Identifies the category to which the recommended listing belongs.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The unique identifier of the category.
*   **Throws:** None.

#### `SellerId`
*   **Type:** `public Guid`
*   **Purpose:** Identifies the seller offering the recommended listing.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The unique identifier of the seller.
*   **Throws:** None.

#### `UserId` (Nullable)
*   **Type:** `public Guid?`
*   **Purpose:** Optionally identifies the user context for which this specific item recommendation was generated, if applicable in a nested context.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The user ID or `null`.
*   **Throws:** None.

#### `BasedOnListingId`
*   **Type:** `public Guid?`
*   **Purpose:** Optionally references the source listing ID used as the basis for "similar item" recommendations.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The source listing ID or `null`.
*   **Throws:** None.

#### `Count`
*   **Type:** `public int`
*   **Purpose:** Represents the total number of items contained within the `Items` collection when this DTO acts as an aggregate container.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The integer count.
*   **Throws:** None.

#### `Strategy` (Nullable)
*   **Type:** `public RecommendationStrategy?`
*   **Purpose:** Specifies the enumeration value representing the algorithm or strategy used to generate the recommendation.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The strategy enum value or `null`.
*   **Throws:** None.

#### `Items`
*   **Type:** `public List<RecommendationDto>`
*   **Purpose:** Contains a list of nested `RecommendationDto` objects, allowing for hierarchical representation of recommendation sets.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The list of recommendations.
*   **Throws:** None. Accessing this property does not throw, but modifying the list may throw if the list is null (though typically initialized in constructor).

#### `IsPersonalised`
*   **Type:** `public bool`
*   **Purpose:** Indicates whether the recommendation set was generated using personalized user data.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** `true` if personalized, otherwise `false`.
*   **Throws:** None.

#### `Strategy` (String)
*   **Type:** `public string`
*   **Purpose:** Provides a string representation or name of the strategy used. Note: This member coexists with the nullable enum `Strategy` property; care must be taken to distinguish between the two based on context or serialization rules.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The strategy name string.
*   **Throws:** None.

#### `GeneratedAt`
*   **Type:** `public DateTime`
*   **Purpose:** Records the timestamp when the recommendation was generated.
*   **Parameters:** None (Property getter/setter).
*   **Return Value:** The date and time of generation.
*   **Throws:** None.

#### `UserId` (Required)
*   **Type:** `public required Guid`
*   **Purpose:** Mandates the presence of a user identifier for the root recommendation context, ensuring every top-level recommendation response is tied to a specific user request.
*   **Parameters:** None (Property getter/setter). Must be set during object initialization.
*   **Return Value:** The unique identifier of the user.
*   **Throws:** Throws a runtime exception if the object is instantiated without setting this property (due to the `required` modifier).

### Constructors

#### `RecommendationDto()`
*   **Purpose:** Initializes a new instance of the `RecommendationDto` class.
*   **Parameters:** None.
*   **Return Value:** A new `RecommendationDto` instance.
*   **Throws:** Throws a runtime exception if the `required Guid UserId` property is not initialized via an object initializer or constructor chain.

#### `RecommendationDto` (Copy/Overload)
*   **Purpose:** The signature `public RecommendationDto` appears in the public surface. In C# context, this typically implies a constructor overload or a static factory method pattern, though strictly interpreted as a constructor signature here, it suggests an alternative initialization path potentially accepting parameters not fully detailed in the simple signature list, or a typo in the source referring to the class name itself. Assuming standard DTO patterns, this likely refers to a constructor used for mapping or cloning.
*   **Parameters:** Dependent on specific implementation overload (not fully specified in signature list beyond name).
*   **Return Value:** A new `RecommendationDto` instance.
*   **Throws:** Depends on implementation; likely throws if required members are not satisfied.

## Usage

### Example 1: Creating a Personalized Recommendation Set
This example demonstrates initializing a root `RecommendationDto` with a required user ID, setting the strategy, and populating the `Items` list with specific product recommendations.

```csharp
var userId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000");

var response = new RecommendationDto
{
    UserId = userId,
    Strategy = RecommendationStrategy.CollaborativeFiltering,
    IsPersonalised = true,
    GeneratedAt = DateTime.UtcNow,
    Items = new List<RecommendationDto>
    {
        new RecommendationDto
        {
            ListingId = Guid.Parse("999e4567-e89b-12d3-a456-426614174999"),
            Title = "Wireless Noise-Cancelling Headphones",
            Price = 299.99m,
            Currency = "USD",
            Score = 0.95,
            Reason = "Based on your recent purchase of audio equipment",
            CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            SellerId = Guid.Parse("22222222-2222-2222-2222-222222222222")
        }
    }
};

// Set count based on items
response.Count = response.Items.Count;
```

### Example 2: Handling a Single "Similar Item" Recommendation
This example shows creating a standalone DTO for a single recommendation derived from a specific source listing, utilizing the `BasedOnListingId` property.

```csharp
var contextUserId = Guid.NewGuid();

var singleRecommendation = new RecommendationDto
{
    UserId = contextUserId,
    ListingId = Guid.Parse("888e4567-e89b-12d3-a456-426614174888"),
    BasedOnListingId = Guid.Parse("777e4567-e89b-12d3-a456-426614174777"),
    Title = "Ergonomic Office Chair",
    Price = 150.00m,
    Currency = "EUR",
    ThumbnailUrl = "https://cdn.marketplace.example/images/chair-thumb.jpg",
    Score = 0.88,
    Reason = "Similar to the item you viewed",
    CategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
    SellerId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
    Strategy = RecommendationStrategy.ContentBased,
    GeneratedAt = DateTime.UtcNow
};

// Ensure string strategy is also populated if required by consumer
singleRecommendation.Strategy = "ContentBased"; 
```

## Notes

*   **Required Member Initialization:** The `UserId` property is marked as `required`. Instantiating `RecommendationDto` without explicitly setting `UserId` in an object initializer or constructor will result in a runtime exception. This constraint ensures that no recommendation data exists without an associated user context.
*   **Property Name Collision:** The class defines two members named `Strategy`: one of type `RecommendationStrategy?` (enum) and one of type `string`. Consumers must be aware of which property is being accessed, particularly during serialization. Depending on the serializer configuration (e.g., Newtonsoft.Json or System.Text.Json), one may take precedence over the other, or both may be serialized under different naming conventions if attributes are applied elsewhere.
*   **Recursive Structure:** The `Items` property is a `List<RecommendationDto>`, allowing for recursive nesting. While useful for grouping, developers should guard against infinite recursion when implementing traversal logic or deep cloning.
*   **Thread Safety:** As a standard Data Transfer Object (DTO) with mutable public properties and a mutable `List<T>` in the `Items` property, `RecommendationDto` is **not thread-safe**. If an instance is shared across multiple threads, external synchronization is required for write operations. It is recommended to treat instances as immutable after construction and population.
*   **Nullable Reference Types:** The `ThumbnailUrl`, `UserId` (nullable variant), `BasedOnListingId`, and `Strategy` (enum variant) properties are nullable. Consumers should perform null checks before accessing members of these properties to avoid `NullReferenceException`.
