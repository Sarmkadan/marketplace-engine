# UnauthorizedException

This exception is thrown by the marketplace engine when an operation is attempted by a user who lacks the necessary permissions for the specified action. It carries the identity of the user and the action being attempted to provide context for authorization failures.

## API

### Properties

#### `public Guid UserId`
Gets the identifier of the user whose authorization failed. This value is set at construction and cannot be modified.

#### `public string Action`
Gets the name of the action that the user attempted to perform. This value is set at construction and cannot be modified.

### Constructors

#### `public UnauthorizedException()`
Initializes a new instance of the `UnauthorizedException` class with default values for `UserId` (an empty Guid) and `Action` (null).

#### `public UnauthorizedException(Guid userId, string action)`
Initializes a new instance of the `UnauthorizedException` class with the specified user identifier and action name.

- **userId**: The identifier of the user whose authorization failed.
- **action**: The name of the action that the user attempted to perform.

## Usage
