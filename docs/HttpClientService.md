# HttpClientService

A lightweight wrapper around `System.Net.Http.HttpClient` that simplifies HTTP requests for RESTful API interactions, including automatic JSON serialization/deserialization and header management.

## API

### `public HttpClientService`

Initializes a new instance of the `HttpClientService` with a configured `HttpClient`. The underlying `HttpClient` is reused across requests for performance and connection pooling benefits.

### `public async Task<T?> GetAsync<T>(string url)`

Sends an HTTP GET request to the specified URL.

- **Parameters**:
  - `url` (string): The target endpoint URL.
- **Return value**: The deserialized response body as type `T`, or `null` if the response is empty or deserialization fails.
- **Exceptions**: Throws `HttpRequestException` if the request fails (e.g., network issues, non-2xx status codes).

### `public async Task<T?> PostAsync<T>(string url, object? body)`

Sends an HTTP POST request with a JSON-serialized body to the specified URL.

- **Parameters**:
  - `url` (string): The target endpoint URL.
  - `body` (object?): The payload to serialize as JSON. May be `null`.
- **Return value**: The deserialized response body as type `T`, or `null` if the response is empty or deserialization fails.
- **Exceptions**: Throws `HttpRequestException` if the request fails or serialization of `body` fails.

### `public async Task<T?> PutAsync<T>(string url, object? body)`

Sends an HTTP PUT request with a JSON-serialized body to the specified URL.

- **Parameters**:
  - `url` (string): The target endpoint URL.
  - `body` (object?): The payload to serialize as JSON. May be `null`.
- **Return value**: The deserialized response body as type `T`, or `null` if the response is empty or deserialization fails.
- **Exceptions**: Throws `HttpRequestException` if the request fails or serialization of `body` fails.

### `public async Task DeleteAsync(string url)`

Sends an HTTP DELETE request to the specified URL.

- **Parameters**:
  - `url` (string): The target endpoint URL.
- **Return value**: `Task` (void).
- **Exceptions**: Throws `HttpRequestException` if the request fails.

### `public void SetAuthorizationHeader(string token)`

Sets the `Authorization` header for subsequent requests using the Bearer scheme.

- **Parameters**:
  - `token` (string): The bearer token value.
- **Exceptions**: Throws `ArgumentNullException` if `token` is `null` or empty.

### `public void AddHeader(string name, string value)`

Adds a custom header to be included in all subsequent requests.

- **Parameters**:
  - `name` (string): The header name.
  - `value` (string): The header value.
- **Exceptions**: Throws `ArgumentNullException` if `name` or `value` is `null`.

## Usage

### Example: Fetching a product
