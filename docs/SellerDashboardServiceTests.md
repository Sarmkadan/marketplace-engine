# SellerDashboardServiceTests

Unit test class for `SellerDashboardService`, verifying dashboard retrieval, revenue calculations, and listing statistics for sellers in the marketplace engine.

## API

### Constructor `SellerDashboardServiceTests`
Initializes a new instance of the test class with required dependencies for testing seller dashboard functionality.

### `Task GetDashboardAsync_WhenSellerNotFound_ThrowsResourceNotFoundException()`
Verifies that attempting to retrieve a dashboard for a non-existent seller throws a `ResourceNotFoundException`.

- **Parameters**: None
- **Return value**: `Task` (completes when assertion passes)
- **Throws**: `ResourceNotFoundException` if the seller does not exist

### `Task GetDashboardAsync_WithValidSeller_ReturnsCorrectMetrics()`
Ensures that a valid seller’s dashboard returns accurate metrics including active listings, total sales, and recent activity.

- **Parameters**: None
- **Return value**: `Task` (completes when assertion passes)
- **Throws**: None

### `Task GetRevenueAsync_WithCompletedPayments_ReturnsMonthlyBreakdown()`
Confirms that revenue data for a seller with completed payments is correctly aggregated into a monthly breakdown.

- **Parameters**: None
- **Return value**: `Task` (completes when assertion passes)
- **Throws**: None

### `Task GetListingStatsAsync_ReturnsTopListingsByViewCount()`
Validates that listing statistics return the top listings ranked by view count.

- **Parameters**: None
- **Return value**: `Task` (completes when assertion passes)
- **Throws**: None

## Usage
