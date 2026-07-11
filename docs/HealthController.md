# HealthController

The `HealthController` provides endpoints and properties to monitor the operational state of the marketplace engine. It exposes health checks for liveness, readiness, and dependency status, along with metadata such as version, uptime, and timestamps.

## API

### `HealthController`
The controller class that groups health-related endpoints and exposes status properties.

### `public async Task<IActionResult> Health()`
Checks the overall health of the service by validating critical dependencies and system state.

- **Returns**: `Task<IActionResult>` – `200 OK` with a healthy status if all checks pass; otherwise, `503 Service Unavailable` with failure details.
- **Throws**: May throw if dependency resolution fails during the check.

### `public async Task<IActionResult> ReadinessCheck()`
Determines whether the service is ready to accept traffic by verifying readiness of all required dependencies (e.g., databases, caches, external services).

- **Returns**: `Task<IActionResult>` – `200 OK` if all dependencies are ready; otherwise, `503 Service Unavailable` with a list of unavailable dependencies.
- **Throws**: May throw if dependency resolution fails during the check.

### `public IActionResult LivenessCheck()`
Confirms that the service process is running and responding to requests. This check should not depend on external systems.

- **Returns**: `IActionResult` – `200 OK` if the service is responsive; otherwise, `503 Service Unavailable`.
- **Throws**: Never throws under normal operation.

### `public string Status`
Gets the current health status of the service.

- **Returns**: `string` – A human-readable status such as `"Healthy"`, `"Degraded"`, or `"Unhealthy"`.
- **Thread Safety**: Safe for concurrent reads.

### `public DateTime Timestamp`
Gets the UTC timestamp when the health status was last evaluated.

- **Returns**: `DateTime` – The timestamp of the most recent health evaluation.
- **Thread Safety**: Safe for concurrent reads.

### `public string Version`
Gets the semantic version of the running service.

- **Returns**: `string` – The version string (e.g., `"1.2.3"`).
- **Thread Safety**: Safe for concurrent reads.

### `public string Uptime`
Gets the duration for which the service has been running.

- **Returns**: `string` – A human-readable duration (e.g., `"2h 15m 30s"`).
- **Thread Safety**: Safe for concurrent reads.

### `public Dictionary<string, string> Dependencies`
Gets a dictionary of dependency names and their statuses.

- **Returns**: `Dictionary<string, string>` – A mapping of dependency names (e.g., `"Database"`, `"Cache"`) to their status strings (e.g., `"Available"`, `"Unavailable"`).
- **Thread Safety**: Safe for concurrent reads; modifications are not exposed.

## Usage
