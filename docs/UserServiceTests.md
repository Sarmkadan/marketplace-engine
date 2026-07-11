# UserServiceTests

Unit test class for the `UserService` component in the marketplace-engine project. It validates the behavior of user‑related operations such as registration, email verification, profile updates, sales recording, and access validation under various success and failure conditions.

## API

- **RegisterUserAsync_WithUniqueEmail_ReturnsCreatedUser**  
  *Purpose*: Verifies that calling `UserService.RegisterUserAsync` with a unique email results in a created user entity.  
  *Parameters*: None.  
  *Return value*: `Task` representing the asynchronous test execution.  
  *Throws*: The test fails if the method does not return a user or if an unexpected exception is thrown.

- **RegisterUserAsync_WithDuplicateEmail_ThrowsDuplicateResourceException**  
  *Purpose*: Ensures that attempting to register a user with an email already in use throws a `DuplicateResourceException`.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test passes only when a `DuplicateResourceException` is observed; any other outcome causes the test to fail.

- **RegisterUserAsync_WithShortFullName_ThrowsArgumentException**  
  *Purpose*: Confirms that supplying a full name shorter than the allowed length triggers an `ArgumentException`.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test expects an `ArgumentException`; failure to throw or throwing a different exception results in a test failure.

- **GetUserAsync_WhenUserNotFound_ThrowsResourceNotFoundException**  
  *Purpose*: Checks that requesting a non‑existent user throws a `ResourceNotFoundException`.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test passes only when the specified exception is thrown.

- **GetUserAsync_WhenUserExists_ReturnsUser**  
  *Purpose*: Validates that retrieving an existing user returns the correct user object.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test fails if the returned user is null or does not match the expected data, or if an exception is thrown.

- **VerifyEmailAsync_WithValidToken_ReturnsTrue**  
  *Purpose*: Asserts that supplying a valid verification token yields a `true` result.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test fails if the method returns `false` or throws an exception.

- **VerifyEmailAsync_WithWrongToken_ReturnsFalse**  
  *Purpose*: Ensures that an incorrect verification token produces a `false` result.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test fails on a `true` return or any thrown exception.

- **VerifyEmailAsync_WithExpiredToken_ReturnsFalse**  
  *Purpose*: Confirms that an expired verification token results in `false`.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test fails if the method returns `true` or throws an exception.

- **PromoteToPremiumAsync_WhenInsufficientSales_ThrowsInvalidOperationException**  
  *Purpose*: Verifies that promoting a user with insufficient sales throws an `InvalidOperationException`.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test passes only when this specific exception is observed.

- **PromoteToPremiumAsync_WhenRatingBelowThreshold_ThrowsInvalidOperationException**  
  *Purpose*: Ensures that a user whose rating is below the required threshold cannot be promoted and throws `InvalidOperationException`.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test expects the exception; any other outcome fails the test.

- **PromoteToPremiumAsync_WhenNoRating_ThrowsInvalidOperationException**  
  *Purpose*: Confirms that attempting promotion when the user has no rating throws `InvalidOperationException`.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test passes only when the exception is thrown.

- **PromoteToPremiumAsync_WhenEligible_PromotesUser**  
  *Purpose*: Validates that a user meeting sales and rating criteria is successfully promoted to premium.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test fails if promotion does not occur or if an exception is thrown.

- **DeactivateAccountAsync_WhenUserExists_SetsIsActiveFalse**  
  *Purpose*: Checks that deactivating an existing user correctly sets the `IsActive` flag to `false`.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test fails if the flag is not updated correctly or if an exception occurs.

- **ValidateUserAccessAsync_WhenUserInactive_ThrowsUnauthorizedException**  
  *Purpose*: Ensures that accessing services with an inactive user throws `UnauthorizedException`.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test passes only when this exception is thrown.

- **ValidateUserAccessAsync_WhenUserNotVerified_ThrowsUnauthorizedException**  
  *Purpose*: Confirms that an unverified user cannot access protected resources and results in `UnauthorizedException`.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test expects the exception; any other result fails.

- **ValidateUserAccessAsync_WhenUserIsActiveAndVerified_DoesNotThrow**  
  *Purpose*: Verifies that an active, verified user can access services without triggering an exception.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test fails if any exception is thrown.

- **UpdateProfileAsync_WithNewFullName_UpdatesName**  
  *Purpose*: Asserts that updating a user’s full name persists the new value.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test fails if the name is not updated or if an exception occurs.

- **UpdateProfileAsync_WithBlankPhone_ClearsPhone**  
  *Purpose*: Ensures that setting the phone number to a blank string clears the stored phone value.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test fails if the phone number is not cleared or if an exception is thrown.

- **RecordSaleAsync_WhenCalled_IncrementsTotalSales**  
  *Purpose*: Validates that invoking `RecordSaleAsync` increments the user’s total sales count.  
  *Parameters*: None.  
  *Return value*: `Task`.  
  *Throws*: The test fails if the sales count does not increase or if an exception is thrown.

## Usage

```csharp
// Example 1: Running a specific test with xUnit
using Xunit;
using MarketplaceEngine.Tests;

public class UserServiceTestsRunner
{
    [Fact]
    public async Task RegisterUserAsync_WithUniqueEmail_ReturnsCreatedUser_Test()
    {
        var testClass = new UserServiceTests();
        await testClass.RegisterUserAsync_WithUniqueEmail_ReturnsCreatedUser();
    }
}
```

```csharp
// Example 2: Executing all tests in the class via a test runner (dotnet test)
// Assuming the project is built and the test assembly is available:
// dotnet test MarketplaceEngine.Tests.dll --filter "FullyQualifiedName~UserServiceTests"
```
The above commands illustrate how to invoke the tests programmatically or through the CLI test harness.

## Notes

- Each test method is independent; they do not rely on shared state, so tests can be executed in any order or in parallel without interference.  
- The class contains only asynchronous test methods; therefore, the test runner must support async `Task` test methods (xUnit, NUnit, or MSTest with async support).  
- Expected exceptions are part of the test contract; if the implementation changes such that a different exception is thrown, the corresponding test will fail, signaling a contract violation.  
- No static fields or shared resources are used, making the class thread‑safe for concurrent test execution.  
- Tests assume that any required mocks or test doubles are set up within the test class (not shown in the signatures); consequently, the behavior described is contingent on those internal arrangements.
