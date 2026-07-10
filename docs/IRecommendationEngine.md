# IRecommendationEngine

The `IRecommendationEngine` interface defines a contract for generating personalized listing recommendations within the marketplace system. It processes user interactions, preferences, and contextual signals to score and rank listings, enabling tailored suggestions based on behavioral patterns and metadata.

## API

### `ScoredListing` Record

Represents a listing scored for recommendation purposes, associating a user with a listing and metadata about the interaction.

| Member       | Type          | Description                                                                                     |
|--------------|---------------|-------------------------------------------------------------------------------------------------|
| `UserId`     | `Guid`        | The unique identifier of the user for whom the recommendation is generated.                    |
| `ListingId`  | `Guid`        | The unique identifier of the listing being scored.                                             |
| `CategoryId` | `Guid?`       | Optional category identifier associated with the listing, if applicable.                       |
| `SignalType` | `SignalType`  | The type of signal that triggered the scoring (e.g., view, purchase, wishlist).                |
| `Weight`     | `double`      | The computed score or weight of the recommendation, indicating its relevance to the user.      |
| `OccurredAt` | `DateTime`    | The timestamp when the signal occurred, used for temporal relevance calculations.              |

## Usage

### Example 1: Generating Recommendations for a User
