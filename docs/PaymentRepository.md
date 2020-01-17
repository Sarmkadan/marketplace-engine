# PaymentRepository

Central repository component for managing payment entities in the marketplace system. Provides CRUD operations and specialized queries for payment data, including filtering by buyer, seller, listing, status, and pagination support. All operations are asynchronous and return domain model instances or collections.

## API

### `PaymentRepository`

Initializes a new repository instance with required dependencies for data access.

### `async Task<Payment?> GetByIdAsync(Guid id)`

Retrieves a single payment by its unique identifier.

- **Parameters**
  - `id` – The unique identifier of the payment to retrieve.
- **Return value**
  - A `Payment` instance if found; otherwise `null`.
- **Exceptions**
  - Throws if the underlying data store fails during retrieval.

### `async Task<List<Payment>> GetAllAsync()`

Retrieves all payments stored in the system.

- **Return value**
  - A list of all `Payment` instances, possibly empty.
- **Exceptions**
  - Throws if the underlying data store fails during retrieval.

### `async Task<Payment> AddAsync(Payment payment)`

Adds a new payment to the system.

- **Parameters**
  - `payment` – The payment entity to insert.
- **Return value**
  - The inserted `Payment` instance, including any generated identifiers.
- **Exceptions**
  - Throws if the payment is invalid or the data store fails during insertion.

### `async Task<Payment> UpdateAsync(Payment payment)`

Updates an existing payment in the system.

- **Parameters**
  - `payment` – The payment entity with updated values.
- **Return value**
  - The updated `Payment` instance.
- **Exceptions**
  - Throws if the payment does not exist or the data store fails during update.

### `async Task DeleteAsync(Guid id)`

Removes a payment from the system by its identifier.

- **Parameters**
  - `id` – The unique identifier of the payment to remove.
- **Exceptions**
  - Throws if the payment does not exist or the data store fails during deletion.

### `async Task<bool> ExistsAsync(Guid id)`

Checks whether a payment with the given identifier exists.

- **Parameters**
  - `id` – The unique identifier of the payment to check.
- **Return value**
  - `true` if the payment exists; otherwise `false`.
- **Exceptions**
  - Throws if the underlying data store fails during the check.

### `async Task<int> CountAsync()`

Retrieves the total number of payments in the system.

- **Return value**
  - The count of all payments.
- **Exceptions**
  - Throws if the underlying data store fails during retrieval.

### `async Task<List<Payment>> GetByBuyerIdAsync(Guid buyerId)`

Retrieves all payments associated with a specific buyer.

- **Parameters**
  - `buyerId` – The unique identifier of the buyer.
- **Return value**
  - A list of `Payment` instances for the buyer, possibly empty.
- **Exceptions**
  - Throws if the underlying data store fails during retrieval.

### `async Task<List<Payment>> GetBySellerIdAsync(Guid sellerId)`

Retrieves all payments associated with a specific seller.

- **Parameters**
  - `sellerId` – The unique identifier of the seller.
- **Return value**
  - A list of `Payment` instances for the seller, possibly empty.
- **Exceptions**
  - Throws if the underlying data store fails during retrieval.

### `async Task<List<Payment>> GetByListingIdAsync(Guid listingId)`

Retrieves all payments associated with a specific listing.

- **Parameters**
  - `listingId` – The unique identifier of the listing.
- **Return value**
  - A list of `Payment` instances for the listing, possibly empty.
- **Exceptions**
  - Throws if the underlying data store fails during retrieval.

### `async Task<List<Payment>> GetByStatusAsync(PaymentStatus status)`

Retrieves all payments matching a specific status.

- **Parameters**
  - `status` – The payment status to filter by.
- **Return value**
  - A list of `Payment` instances with the given status, possibly empty.
- **Exceptions**
  - Throws if the underlying data store fails during retrieval.

### `async Task<(List<Payment> items, int total)> GetPagedAsync(int pageNumber, int pageSize)`

Retrieves a page of payments along with the total count of all payments.

- **Parameters**
  - `pageNumber` – The zero-based page index.
  - `pageSize` – The maximum number of items per page.
- **Return value**
  - A tuple containing the list of `Payment` instances for the page and the total count of all payments.
- **Exceptions**
  - Throws if the underlying data store fails during retrieval or if `pageNumber` or `pageSize` are invalid.

### `async Task<decimal> GetTotalRevenueAsync()`

Calculates the total revenue from all completed payments.

- **Return value**
  - The sum of amounts from all payments with a completed status.
- **Exceptions**
  - Throws if the underlying data store fails during aggregation.

## Usage
