# RecommendationOptions

Configuration class that controls the behavior of the recommendation engine in the marketplace-engine project. It defines thresholds, weights, cache durations, and feature toggles that determine how recommendations are generated, scored, and diversified.

## API

### `public int MinOverlapForNeighbour`
Minimum number of common items required between two users for them to be considered neighbors. Used during neighborhood-based recommendation generation to filter weak candidate pairs. Must be a positive integer; values less than 1 are treated as 1.

### `public int MaxNeighbours`
Maximum number of neighbors to consider when generating recommendations for a user. Affects both performance and recommendation diversity. Must be a positive integer; values less than 1 are treated as 1.

### `public double MinSimilarityThreshold`
Minimum similarity score (cosine or Jaccard) required between two items for them to be considered similar. Used during item similarity computation and candidate generation. Must be between 0.0 and 1.0 inclusive; values outside this range are clamped.

### `public int TrendingWindowHours`
Time window, in hours, used to compute trending items. Only interactions (views, saves, etc.) within this window contribute to trending scores. Must be a non-negative integer; negative values are treated as 0.

### `public double ViewWeight`
Weight assigned to item views when computing user affinity or recommendation scores. Higher values increase the influence of viewed items on recommendations. Must be a finite number; NaN or infinity are treated as 0.0.

### `public double SaveWeight`
Weight assigned to item saves when computing user affinity or recommendation scores. Higher values increase the influence of saved items on recommendations. Must be a finite number; NaN or infinity are treated as 0.0.

### `public double EnquiryWeight`
Weight assigned to item enquiries (e.g., detailed views or inquiries) when computing user affinity or recommendation scores. Higher values increase the influence of enquired items on recommendations. Must be a finite number; NaN or infinity are treated as 0.0.

### `public double PurchaseWeight`
Weight assigned to item purchases when computing user affinity or recommendation scores. Higher values increase the influence of purchased items on recommendations. Must be a finite number; NaN or infinity are treated as 0.0.

### `public int UserFeedCacheTtlMinutes`
Time-to-live, in minutes, for cached user recommendation feeds. Controls how often personalized feeds are regenerated for individual users. Must be a non-negative integer; negative values are treated as 0.

### `public int TrendingFeedCacheTtlMinutes`
Time-to-live, in minutes, for cached trending recommendation feeds. Controls how often the global trending feed is regenerated. Must be a non-negative integer; negative values are treated as 0.

### `public int ItemSimilarityCacheTtlMinutes`
Time-to-live, in minutes, for cached item similarity matrices. Controls how often item similarity scores are recomputed. Must be a non-negative integer; negative values are treated as 0.

### `public int MaxSignalsPerUser`
Maximum number of user activity signals (e.g., views, saves) to consider when building a user profile for recommendations. Older or excess signals are discarded. Must be a positive integer; values less than 1 are treated as 1.

### `public int ActivityHistoryDays`
Maximum age, in days, of user activity signals to consider for recommendations. Signals older than this are discarded. Must be a non-negative integer; negative values are treated as 0.

### `public int MinAffinitySignals`
Minimum number of non-zero affinity signals required for a user to receive personalized recommendations. Users with fewer signals fall back to non-personalized feeds. Must be a non-negative integer; negative values are treated as 0.

### `public bool EnablePersonalisation`
Flag indicating whether personalized recommendations are enabled. When false, all users receive non-personalized feeds (e.g., trending or category-based). No effect if underlying data is insufficient.

### `public bool EnableDiversification`
Flag indicating whether recommendation diversification is enabled. When true, the engine may suppress overly similar items in a user’s feed to improve variety. No effect if `MaxCategoryConcentration` is not enforced.

### `public double MaxCategoryConcentration`
Maximum allowed concentration of items from a single category in a user’s recommendation feed. Values between 0.0 and 1.0 control diversity; 0.0 allows any distribution, 1.0 allows all items from one category. Must be between 0.0 and 1.0 inclusive; values outside this range are clamped.

### `public static RecommendationOptions CreateDefault()`
Creates a new instance of `RecommendationOptions` with conservative, production-ready defaults suitable for most marketplace scenarios. Returns a fully initialized instance with no external dependencies.

### `public static RecommendationOptions FromConfiguration()`
Creates a new instance of `RecommendationOptions` by reading configuration from the application’s settings (e.g., appsettings.json, environment variables, or configuration provider). Throws `InvalidOperationException` if required configuration keys are missing or invalid. Returns a configured instance or throws on failure.

## Usage
