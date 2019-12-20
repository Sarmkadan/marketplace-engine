# ResourceNotFoundException

A custom exception type used within the `marketplace-engine` project to indicate that a requested resource could not be found. This exception carries metadata about the type and identifier of the missing resource, enabling more precise error handling and logging.

## API

### `public string ResourceType`

A read-only property that returns the type of the resource that was not found. This value is set during construction and cannot be modified afterward.

### `public string ResourceId`

A read-only property that returns the unique identifier of the resource that was not found. This value is set during construction and cannot be modified afterward.

### `public ResourceNotFoundException(string resourceType, string resourceId)`

Constructs a new `ResourceNotFoundException` with the specified resource type and identifier.

**Parameters:**
- `resourceType` (string): The type of the resource that could not be found (e.g., "Product", "User").
- `resourceId` (string): The unique identifier of the missing resource.

**Throws:**
- `ArgumentNullException`: If `resourceType` or `resourceId` is `null`.

### `public ResourceNotFoundException(string resourceType, string resourceId, Exception innerException)`

Constructs a new `ResourceNotFoundException` with the specified resource type, identifier, and an inner exception.

**Parameters:**
- `resourceType` (string): The type of the resource that could not be found.
- `resourceId` (string): The unique identifier of the missing resource.
- `innerException` (Exception): The exception that is the cause of the current exception.

**Throws:**
- `ArgumentNullException`: If `resourceType` or `resourceId` is `null`.

## Usage
