# MarketplaceWorkflowTests

Integration test suite for end-to-end workflow validation in the Marketplace Engine. Exercises core marketplace operations including listing lifecycle, user management, messaging, search, and concurrency scenarios to ensure system integrity under realistic usage patterns.

## API

### `public MarketplaceWorkflowTests()`

Constructor for the test class. Initializes test dependencies and configuration required for marketplace workflow validation.

### `public void Dispose()`

Releases all resources used by the test class. Ensures cleanup of test context, services, and any disposable dependencies created during test execution.

### `public async Task FullListingLifecycle_CreateSearchDelistAndVerify()`

Validates the complete lifecycle of a marketplace listing. Creates a listing, searches for it using multiple criteria, verifies visibility, and confirms successful delisting. Ensures data consistency across create, read, and delete operations.

### `public async Task FullMessagingWorkflow_SendReadReplyAndDelete()`

Tests the full conversation flow between users in the marketplace. Sends a message, reads the conversation, replies, and deletes the thread. Verifies message persistence, read states, and thread cleanup.

### `public async Task UserRegistrationToPremiumPromotion_WhenEligible_Succeeds()`

Validates the promotion workflow from standard user registration to premium status. Confirms eligibility checks, status upgrade, and access to premium-only features. Ensures role transitions are applied atomically and consistently.

### `public async Task AdvancedSearch_WithPriceAndCategoryFilters_ReturnsCorrectListings()`

Tests complex search functionality using combined filters. Applies price range and category constraints, verifies result accuracy, and confirms no cross-contamination between unrelated listings. Ensures search index consistency and filter logic correctness.

### `public async Task ConcurrentListingCreation_AllListingsPersistedWithoutDataCorruption()`

Stress-tests listing creation under high concurrency. Spawns multiple threads creating listings simultaneously, then verifies all entries exist without duplication or corruption. Validates thread-safe persistence and idempotent write operations.

### `public async Task EmailVerification_WithCorrectToken_VerifiesUser()`

Tests user email verification flow. Simulates token-based verification, confirms user status update, and validates access to authenticated features. Ensures token handling is secure and state transitions are irreversible.

### `public async Task PaginatedListings_SecondPage_ReturnsDistinctItemsFromFirstPage()`

Validates pagination logic for marketplace listings. Retrieves first and second pages, confirms no overlap, and ensures total count matches expectations. Tests offset and limit calculations under realistic data volumes.

### `public async Task SearchByCategory_TotalMatchesBothPages()`

Tests category-based search with pagination. Searches a category, retrieves all pages, sums results, and confirms total matches align with expected count. Validates category index integrity and pagination boundary conditions.

### `public async Task DeactivatedUser_CannotCreateNewListings()`

Ensures system enforces user status restrictions. Attempts to create a listing as a deactivated user and verifies the operation is rejected. Confirms role-based access control is enforced at the service layer.

## Usage
