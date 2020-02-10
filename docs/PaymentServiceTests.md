# PaymentServiceTests

`PaymentServiceTests` is a test class that verifies the behavior of the `PaymentService` component in the marketplace-engine project. Each test method focuses on a specific scenario, confirming that the service throws the expected exceptions or produces the correct state transitions when interacting with listings, payments, and users.

## API

### `public PaymentServiceTests`
- **Purpose**: Constructor for the test class. No parameters; initializes any test fixtures or mocks required by the test methods.
- **Return value**: None (instance constructor).
- **Throws**: Does not throw any exceptions under normal test setup.

### `public async Task InitiatePaymentAsync_WhenListingNotFound_ThrowsResourceNotFoundException`
- **Purpose**: Verifies that calling `InitiatePaymentAsync` with a non‑existent listing identifier results in a `ResourceNotFoundException`.
- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous test execution.
- **Throws**: The test fails if the method does not throw a `ResourceNotFoundException`.

### `public async Task InitiatePaymentAsync_WhenListingIsNotActive_ThrowsMarketplaceException`
- **Purpose**: Ensures that attempting to initiate payment for a listing that is not active throws a `MarketplaceException`.
- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous test execution.
- **Throws**: The test fails if the method does not throw a `MarketplaceException`.

### `public async Task InitiatePaymentAsync_WhenBuyerIsSeller_ThrowsMarketplaceException`
- **Purpose**: Confirms that when the buyer attempting to initiate payment is also the seller of the listing, a `MarketplaceException` is thrown.
- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous test execution.
- **Throws**: The test fails if the method does not throw a `MarketplaceException`.

### `public async Task InitiatePaymentAsync_WithValidData_CreatesPayment`
- **Purpose**: Checks that supplying valid listing, buyer, and payment details results in the creation of a payment record.
- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous test execution.
- **Throws**: The test fails if no payment is created or if an unexpected exception is thrown.

### `public async Task CancelPaymentAsync_WhenCallerIsNotBuyer_ThrowsUnauthorizedException`
- **Purpose**: Validates that a user who is not the buyer of a payment cannot cancel it, resulting in an `UnauthorizedException`.
- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous test execution.
- **Throws**: The test fails if the method does not throw an `UnauthorizedException`.

### `public async Task RefundPaymentAsync_WhenPaymentIsPending_ThrowsInvalidOperationException`
- **Purpose**: Ensures that attempting to refund a payment that is still in the `Pending` state throws an `InvalidOperationException`.
- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous test execution.
- **Throws**: The test fails if the method does not throw an `InvalidOperationException`.

### `public async Task CompletePaymentAsync_WithValidTransactionId_MarksListingAsDelisted`
- **Purpose**: Confirms that completing a payment with a valid transaction ID correctly updates the associated listing to a `Delisted` state.
- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous test execution.
- **Throws**: The test fails if the listing is not marked as delisted or if an unexpected exception occurs.

## Usage

### Example 1: Testing payment initiation failure for a missing listing
```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Data;
using Moq;
using Xunit;

public class PaymentServiceTests
{
    [Fact]
    public async Task InitiatePaymentAsync_WhenListingNotFound_ThrowsResourceNotFoundException()
    {
        // Arrange
        var listingRepoMock = new Mock<IListingRepository>();
        listingRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                       .ReturnsAsync((Listing)null); // simulate missing listing

        var paymentService = new PaymentService(
            listingRepoMock.Object,
            Mock.Of<IPaymentRepository>(),
            Mock.Of<IUserRepository>(),
            Mock.Of<INotificationService>());

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            paymentService.InitiatePaymentAsync(
                buyerId: Guid.NewGuid(),
                listingId: Guid.Empty,
                amount: 100m));
    }
}
```

### Example 2: Verifying that a completed payment delists the listing
```csharp
using MarketplaceEngine.Services;
using MarketplaceEngine.Data;
using Moq;
using Xunit;

public class PaymentServiceTests
{
    [Fact]
    public async Task CompletePaymentAsync_WithValidTransactionId_MarksListingAsDelisted()
    {
        // Arrange
        var listing = new Listing { Id = Guid.NewGuid(), IsActive = true };
        var listingRepoMock = new Mock<IListingRepository>();
        listingRepoMock.Setup(r => r.GetByIdAsync(listing.Id))
                       .ReturnsAsync(listing);
        listingRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Listing>()))
                       .Returns(Task.CompletedTask);

        var paymentRepoMock = new Mock<IPaymentRepository>();
        paymentRepoMock.Setup(r => r.GetByTransactionIdAsync(It.IsAny<string>()))
                       .ReturnsAsync(new Payment
                       {
                           Id = Guid.NewGuid(),
                           ListingId = listing.Id,
                           BuyerId = Guid.NewGuid(),
                           Amount = 50m,
                           Status = PaymentStatus.Processed
                       });

        var paymentService = new PaymentService(
            listingRepoMock.Object,
            paymentRepoMock.Object,
            Mock.Of<IUserRepository>(),
            Mock.Of<INotificationService>());

        // Act
        await paymentService.CompletePaymentAsync("txn_123");

        // Assert
        listingRepoMock.Verify(r => r.UpdateAsync(
            It.Is<Listing>(l => l.Id == listing.Id && !l.IsActive)), Times.Once);
    }
}
```

## Notes
- Each test method is stateless and relies solely on mocked dependencies; therefore, tests can be executed in parallel without risk of shared state interference.
- Proper setup of mocks is essential: omitting a setup for a repository method will cause the test to fail with a null reference rather than the expected exception, obscuring the intent of the test.
- The test class does not inherit from any base test class; all required behavior is encapsulated within the individual methods.
- No static fields or shared resources are used, so there are no thread‑safety concerns beyond those inherent to the mocking framework.
- If the production `PaymentService` implementation changes its exception types, the corresponding test methods must be updated to reflect the new contract.
