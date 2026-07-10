# UserRepository

Central data access component for managing user entities within the marketplace-engine system. Provides asynchronous CRUD operations and specialized queries for user data retrieval, filtering, and aggregation.

## API

### `UserRepository`
Initializes a new instance of the repository with required dependencies for database interaction.

### `async Task<User?> GetByIdAsync`
Fetches a single user by unique identifier.
- **Parameters**: `id` (string) – The user identifier.
- **Returns**: The user if found; otherwise `null`.
- **Exceptions**: Throws if the identifier is invalid or the database operation fails.

### `async Task<List<User>> GetAllAsync`
Retrieves all users in the system.
- **Returns**: A list of all users.
- **Exceptions**: Throws if the database operation fails.

### `async Task<User> AddAsync`
Creates a new user in the system.
- **Parameters**: `user` (User) – The user to add.
- **Returns**: The added user with generated values (e.g., ID).
- **Exceptions**: Throws if the user is invalid or the operation conflicts with existing data.

### `async Task<User> UpdateAsync`
Modifies an existing user.
- **Parameters**: `user` (User) – The user with updated values.
- **Returns**: The updated user.
- **Exceptions**: Throws if the user does not exist or the operation fails.

### `async Task DeleteAsync`
Removes a user from the system.
- **Parameters**: `id` (string) – The user identifier.
- **Exceptions**: Throws if the user does not exist or the operation fails.

### `async Task<bool> ExistsAsync`
Checks whether a user with the given identifier exists.
- **Parameters**: `id` (string) – The user identifier.
- **Returns**: `true` if the user exists; otherwise `false`.
- **Exceptions**: Throws if the identifier is invalid or the database operation fails.

### `async Task<int> CountAsync`
Returns the total number of users.
- **Returns**: The count of users.
- **Exceptions**: Throws if the database operation fails.

### `async Task<User?> GetByEmailAsync`
Finds a user by email address.
- **Parameters**: `email` (string) – The email address.
- **Returns**: The user if found; otherwise `null`.
- **Exceptions**: Throws if the email is invalid or the database operation fails.

### `async Task<List<User>> GetActiveUsersAsync`
Retrieves all users marked as active.
- **Returns**: A list of active users.
- **Exceptions**: Throws if the database operation fails.

### `async Task<List<User>> GetVerifiedUsersAsync`
Retrieves all users with verified status.
- **Returns**: A list of verified users.
- **Exceptions**: Throws if the database operation fails.

### `async Task<List<User>> GetByRoleAsync`
Fetches users matching a specific role.
- **Parameters**: `role` (string) – The role identifier.
- **Returns**: A list of users with the specified role.
- **Exceptions**: Throws if the role is invalid or the database operation fails.

### `async Task<List<User>> SearchAsync`
Performs a free-text search across user fields.
- **Parameters**: `query` (string) – The search term.
- **Returns**: A list of users matching the query.
- **Exceptions**: Throws if the query is invalid or the database operation fails.

### `async Task<List<User>> GetTopSellersAsync`
Retrieves users ranked by sales volume or activity.
- **Parameters**: `limit` (int) – Maximum number of users to return.
- **Returns**: A list of top sellers.
- **Exceptions**: Throws if the limit is invalid or the database operation fails.

### `async Task<List<User>> GetByLocationAsync`
Fetches users located within a specified geographic area.
- **Parameters**:
  - `latitude` (double) – Geographic latitude.
  - `longitude` (double) – Geographic longitude.
  - `radiusKm` (double) – Search radius in kilometers.
- **Returns**: A list of users within the specified area.
- **Exceptions**: Throws if the coordinates or radius are invalid or the database operation fails.

### `async Task<bool> EmailExistsAsync`
Checks whether an email address is already registered.
- **Parameters**: `email` (string) – The email address.
- **Returns**: `true` if the email exists; otherwise `false`.
- **Exceptions**: Throws if the email is invalid or the database operation fails.

### `async Task<User?> GetByVerificationTokenAsync`
Finds a user by account verification token.
- **Parameters**: `token` (string) – The verification token.
- **Returns**: The user if found; otherwise `null`.
- **Exceptions**: Throws if the token is invalid or the database operation fails.

### `async Task<(List<User> items, int total)> GetPagedAsync`
Retrieves a page of users with total count.
- **Parameters**:
  - `pageIndex` (int) – Zero-based page index.
  - `pageSize` (int) – Number of items per page.
- **Returns**: A tuple containing the page items and the total count.
- **Exceptions**: Throws if the pagination parameters are invalid or the database operation fails.

### `async Task UpdateLastActivityAsync`
Updates the last activity timestamp for a user.
- **Parameters**: `id` (string) – The user identifier.
- **Exceptions**: Throws if the user does not exist or the operation fails.

## Usage
