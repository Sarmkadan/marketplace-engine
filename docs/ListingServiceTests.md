# ListingServiceTests

Unit test class for verifying the behavior of `ListingService` operations, including listing creation, delisting, and featured marking. It validates authorization rules, error handling, and edge cases for seller and admin actions.

## API

### `public ListingServiceTests`

Constructor for the test class. Initializes dependencies and test context required to exercise `ListingService` functionality.

### `public async Task CreateListingAsync_WhenSellerNotFound_ThrowsResourceNotFoundException`

Verifies that attempting to create a listing for a non-existent seller results in a `ResourceNotFoundException`.

- **Parameters**: None.
- **Return value**: `Task` representing the asynchronous test execution.
- **Throws**: `ResourceNotFoundException` when the seller identifier does not correspond to an active seller.

### `public async Task CreateListingAsync_WhenSellerIsInactive_ThrowsUnauthorizedException`

Ensures that creating a listing for an inactive seller is rejected with an `UnauthorizedException`.

- **Parameters**: None.
- **Return value**: `Task` representing the asynchronous test execution.
- **Throws**: `UnauthorizedException` when the seller account is inactive.

### `public async Task DelistListingAsync_WhenCallerIsNotSeller_ThrowsUnauthorizedException`

Confirms that only the seller can delist their own listing; any other caller triggers an `UnauthorizedException`.

- **Parameters**: None.
- **Return value**: `Task` representing the asynchronous test execution.
- **Throws**: `UnauthorizedException` when the caller is not the listing’s seller.

### `public async Task MarkAsFeaturedAsync_WhenCallerIsNotAdmin_ThrowsUnauthorizedException`

Validates that only administrators can mark a listing as featured; non-admin callers receive an `UnauthorizedException`.

- **Parameters**: None.
- **Return value**: `Task` representing the asynchronous test execution.
- **Throws**: `UnauthorizedException` when the caller lacks admin privileges.

## Usage
