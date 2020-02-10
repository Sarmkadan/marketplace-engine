# ReviewServiceTests

`ReviewServiceTests` is the unit test suite for the review subsystem within the `marketplace-engine` project. It validates the behavior of the review service under both successful and exceptional conditions, covering submission of buyer reviews, seller replies, retrieval of seller statistics, and moderator-driven removal. Each test method targets a specific business rule or authorization boundary, ensuring that the service layer enforces constraints such as reviewer eligibility, duplicate detection, role-based access, and rating aggregation logic.

## API

### public ReviewServiceTests

Constructor for the test fixture. Initializes any shared test infrastructure, mock dependencies, and the system under test. Does not accept parameters or return a value. Not expected to throw.

### public async Task SubmitReviewAsync_WhenReviewerNotFound_ThrowsResourceNotFoundException

**Purpose:** Verifies that submitting a review on behalf of a reviewer who does not exist in the system results in a `ResourceNotFoundException`.

**Parameters:** None (inputs are arranged within the test body).

**Return value:** A completed `Task` representing the asynchronous test operation.

**Throws:** The test asserts that the service call throws `ResourceNotFoundException`. The test itself fails if the exception is not thrown or a different exception type is raised.

### public async Task SubmitReviewAsync_WhenReviewerIsInactive_ThrowsUnauthorizedException

**Purpose:** Confirms that an inactive (e.g., suspended or deactivated) user attempting to submit a review receives an `UnauthorizedException`.

**Parameters:** None.

**Return value:** `Task`.

**Throws:** Asserts `UnauthorizedException` is thrown by the service method.

### public async Task SubmitReviewAsync_WhenReviewerIsSeller_ThrowsMarketplaceException

**Purpose:** Ensures that a seller attempting to review their own product or another product while acting as a seller triggers a `MarketplaceException`, enforcing the rule that sellers cannot submit reviews.

**Parameters:** None.

**Return value:** `Task`.

**Throws:** Asserts `MarketplaceException` is thrown.

### public async Task SubmitReviewAsync_WhenDuplicateReview_ThrowsDuplicateResourceException

**Purpose:** Validates that a second review submission by the same reviewer for the same order or product raises a `DuplicateResourceException`, preventing duplicate reviews.

**Parameters:** None.

**Return value:** `Task`.

**Throws:** Asserts `DuplicateResourceException` is thrown.

### public async Task SubmitReviewAsync_WithValidData_CreatesReviewAndUpdatesSellerRating

**Purpose:** The happy-path test confirming that a valid review from an eligible, active buyer successfully persists the review entity and triggers a recalculation of the seller’s aggregate rating.

**Parameters:** None.

**Return value:** `Task`.

**Throws:** The test expects no exceptions; it asserts the review was created and the seller rating was updated accordingly.

### public async Task AddSellerReplyAsync_WhenCallerIsNotSeller_ThrowsUnauthorizedException

**Purpose:** Verifies that a user who is not the seller associated with the review cannot add a seller reply, resulting in an `UnauthorizedException`.

**Parameters:** None.

**Return value:** `Task`.

**Throws:** Asserts `UnauthorizedException` is thrown.

### public async Task GetSellerStatsAsync_ReturnsCorrectAverageAndDistribution

**Purpose:** Tests that the seller statistics query returns the correctly computed average rating and the proper distribution of ratings (e.g., counts per star level) based on existing review data.

**Parameters:** None.

**Return value:** `Task`.

**Throws:** The test expects no exceptions; it asserts the returned stats object contains the expected average and distribution values.

### public async Task RemoveReviewAsync_WhenCallerIsNotModerator_ThrowsUnauthorizedException

**Purpose:** Ensures that review removal is restricted to moderators. A non-moderator caller receives an `UnauthorizedException`.

**Parameters:** None.

**Return value:** `Task`.

**Throws:** Asserts `UnauthorizedException` is thrown.

## Usage

### Example 1: Testing the happy path for review submission

```csharp
[Fact]
public async Task SubmitReviewAsync_WithValidData_CreatesReviewAndUpdatesSellerRating()
{
    // Arrange
    var buyer = new User { Id = Guid.NewGuid(), Status = UserStatus.Active, Role = UserRole.Buyer };
    var seller = new User { Id = Guid.NewGuid(), Role = UserRole.Seller };
    var order = new Order { Id = Guid.NewGuid(), BuyerId = buyer.Id, SellerId = seller.Id };

    _userRepositoryMock.Setup(r => r.GetByIdAsync(buyer.Id)).ReturnsAsync(buyer);
    _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
    _reviewRepositoryMock.Setup(r => r.ExistsAsync(buyer.Id, order.Id)).ReturnsAsync(false);

    var reviewService = new ReviewService(
        _reviewRepositoryMock.Object,
        _userRepositoryMock.Object,
        _orderRepositoryMock.Object,
        _ratingCalculatorMock.Object);

    // Act
    await reviewService.SubmitReviewAsync(buyer.Id, order.Id, rating: 5, comment: "Excellent product");

    // Assert
    _reviewRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Review>()), Times.Once);
    _ratingCalculatorMock.Verify(c => c.RecalculateAsync(seller.Id), Times.Once);
}
```

### Example 2: Testing authorization for seller reply

```csharp
[Fact]
public async Task AddSellerReplyAsync_WhenCallerIsNotSeller_ThrowsUnauthorizedException()
{
    // Arrange
    var review = new Review { Id = Guid.NewGuid(), SellerId = Guid.NewGuid() };
    var caller = new User { Id = Guid.NewGuid(), Role = UserRole.Buyer };

    _reviewRepositoryMock.Setup(r => r.GetByIdAsync(review.Id)).ReturnsAsync(review);
    _userRepositoryMock.Setup(r => r.GetByIdAsync(caller.Id)).ReturnsAsync(caller);

    var reviewService = new ReviewService(
        _reviewRepositoryMock.Object,
        _userRepositoryMock.Object,
        Mock.Of<IOrderRepository>(),
        Mock.Of<IRatingCalculator>());

    // Act & Assert
    await Assert.ThrowsAsync<UnauthorizedException>(
        () => reviewService.AddSellerReplyAsync(review.Id, caller.Id, "Thank you!"));
}
```

## Notes

- **Edge Cases:** Tests for `SubmitReviewAsync` cover missing reviewer, inactive reviewer, seller-as-reviewer, and duplicate submissions. The `GetSellerStatsAsync` test implicitly handles scenarios with zero reviews or uniform ratings by asserting exact average and distribution values. Moderator-only removal is enforced; any non-moderator role (buyer, seller, anonymous) should trigger `UnauthorizedException`.
- **Thread Safety:** These are unit tests and execute synchronously within a single test runner thread. They do not validate thread safety of the service itself. In production, concurrent duplicate-review detection and rating recalculation must rely on database-level uniqueness constraints and atomic aggregate updates to avoid race conditions.
- **Test Isolation:** Each test method arranges its own mock dependencies and does not share mutable state with other tests. The constructor may set up shared mocks, but individual tests override setups as needed to simulate specific conditions.
- **Exception Taxonomy:** The test suite distinguishes between `ResourceNotFoundException` (entity missing), `UnauthorizedException` (caller lacks permission or is inactive), `MarketplaceException` (domain rule violation, e.g., seller reviewing), and `DuplicateResourceException` (unique constraint violation). This taxonomy should be preserved when extending the service.
