# IBackgroundJob

Interface representing a background job in the marketplace engine. Defines the contract for enqueuing, starting, stopping, and executing background tasks, along with querying job state and queue status.

## API

### `public BackgroundJobQueue`

Gets the queue associated with this background job. The queue determines the processing order and concurrency of job execution.

### `public void Enqueue()`

Enqueues the job for execution. The job will be processed according to the rules of its associated `BackgroundJobQueue`.

**Throws:**
- `InvalidOperationException` if the job is already enqueued or running.

### `public void Start()`

Starts the job execution immediately, bypassing the queue if applicable. This is typically used for jobs that should run without delay.

**Throws:**
- `InvalidOperationException` if the job is already running or completed.

### `public async Task StopAsync()`

Stops the job execution asynchronously. Any in-progress work may be interrupted depending on implementation.

**Returns:**
- `Task` representing the asynchronous stop operation.

### `public int GetQueueSize()`

Gets the current size of the job queue associated with this background job.

**Returns:**
- `int` indicating the number of jobs waiting to be processed in the queue.

### `public Guid JobId`

Gets the unique identifier for this job instance.

### `public SearchIndexingJob`

Gets the `SearchIndexingJob` instance associated with this background job, if applicable.

### `public async Task ExecuteAsync()`

Executes the background job asynchronously. This method contains the core logic of the job.

**Returns:**
- `Task` representing the asynchronous execution.

### `public Guid JobId`

Gets the unique identifier for the `DataCleanupJob` instance associated with this background job, if applicable.

### `public DataCleanupJob`

Gets the `DataCleanupJob` instance associated with this background job, if applicable.

### `public async Task ExecuteAsync()`

Executes the background job asynchronously. This method contains the core logic of the job.

**Returns:**
- `Task` representing the asynchronous execution.

### `public Guid JobId`

Gets the unique identifier for the `NotificationDispatchJob` instance associated with this background job, if applicable.

### `public NotificationDispatchJob`

Gets the `NotificationDispatchJob` instance associated with this background job, if applicable.

### `public async Task ExecuteAsync()`

Executes the background job asynchronously. This method contains the core logic of the job.

**Returns:**
- `Task` representing the asynchronous execution.

## Usage

### Enqueue and Execute a Job
