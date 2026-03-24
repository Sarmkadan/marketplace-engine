// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace MarketplaceEngine.Infrastructure.Background;

/// <summary>
/// Job interface for background work items.
/// </summary>
public interface IBackgroundJob
{
    Guid JobId { get; }
    string JobName { get; }
    Task ExecuteAsync();
}

/// <summary>
/// Background job queue for processing long-running tasks asynchronously.
/// In production, use a proper background job library like Hangfire, Quartz, or Azure Functions.
/// This implementation uses an in-memory queue with worker threads.
/// </summary>
public class BackgroundJobQueue
{
    private readonly ConcurrentQueue<IBackgroundJob> _queue = new();
    private readonly ILogger<BackgroundJobQueue> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task? _workerTask;

    public BackgroundJobQueue(ILogger<BackgroundJobQueue> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Enqueues a background job for processing.
    /// </summary>
    public void Enqueue(IBackgroundJob job)
    {
        if (job == null)
            return;

        _queue.Enqueue(job);
        _logger.LogInformation("Job enqueued: {JobName} (Id: {JobId})", job.JobName, job.JobId);
    }

    /// <summary>
    /// Starts the background worker thread.
    /// Should be called during application startup.
    /// </summary>
    public void Start()
    {
        if (_workerTask != null)
            return;

        _workerTask = ProcessQueueAsync(_cancellationTokenSource.Token);
        _logger.LogInformation("Background job queue started");
    }

    /// <summary>
    /// Stops the background worker thread.
    /// Waits for currently processing job to complete.
    /// </summary>
    public async Task StopAsync()
    {
        _cancellationTokenSource.Cancel();

        if (_workerTask != null)
        {
            try
            {
                await _workerTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
        }

        _logger.LogInformation("Background job queue stopped");
    }

    /// <summary>
    /// Gets the current queue size.
    /// </summary>
    public int GetQueueSize()
    {
        return _queue.Count;
    }

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Try to dequeue a job with timeout
                if (_queue.TryDequeue(out var job))
                {
                    try
                    {
                        _logger.LogInformation("Processing job: {JobName} (Id: {JobId})", job.JobName, job.JobId);

                        await job.ExecuteAsync();

                        _logger.LogInformation("Job completed: {JobName} (Id: {JobId})", job.JobName, job.JobId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Job failed: {JobName} (Id: {JobId})", job.JobName, job.JobId);

                        // Could implement retry logic here if needed
                        // For now, job is simply discarded on failure
                    }
                }
                else
                {
                    // No job available, wait a bit before checking again
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}

/// <summary>
/// Concrete job for rebuilding search index.
/// Runs periodically to update search indexes with new listings.
/// </summary>
public class SearchIndexingJob : IBackgroundJob
{
    public Guid JobId { get; } = Guid.NewGuid();
    public string JobName => "Search Indexing";

    private readonly ILogger<SearchIndexingJob> _logger;

    public SearchIndexingJob(ILogger<SearchIndexingJob> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting search index rebuild");

        // In production, this would:
        // 1. Query all active listings from database
        // 2. Batch them for efficiency
        // 3. Index in Elasticsearch or similar
        // 4. Handle failures and retries

        await Task.Delay(500); // Simulate indexing work

        _logger.LogInformation("Search index rebuild completed");
    }
}

/// <summary>
/// Job for cleaning up old/deleted records from database.
/// </summary>
public class DataCleanupJob : IBackgroundJob
{
    public Guid JobId { get; } = Guid.NewGuid();
    public string JobName => "Data Cleanup";

    private readonly ILogger<DataCleanupJob> _logger;

    public DataCleanupJob(ILogger<DataCleanupJob> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting data cleanup");

        // In production, this would:
        // 1. Delete expired sessions/tokens
        // 2. Remove deleted listings (soft deleted -> hard deleted)
        // 3. Archive old messages/logs
        // 4. Compact database

        await Task.Delay(300); // Simulate cleanup work

        _logger.LogInformation("Data cleanup completed");
    }
}

/// <summary>
/// Job for sending batched notifications (emails, pushes, etc).
/// </summary>
public class NotificationDispatchJob : IBackgroundJob
{
    public Guid JobId { get; } = Guid.NewGuid();
    public string JobName => "Notification Dispatch";

    private readonly ILogger<NotificationDispatchJob> _logger;

    public NotificationDispatchJob(ILogger<NotificationDispatchJob> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting notification dispatch");

        // In production, this would:
        // 1. Query pending notifications
        // 2. Batch them (e.g., digest emails)
        // 3. Send via email provider API
        // 4. Track delivery status
        // 5. Retry failed sends

        await Task.Delay(200); // Simulate sending

        _logger.LogInformation("Notification dispatch completed");
    }
}
