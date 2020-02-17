using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MarketplaceEngine.Infrastructure.Events
{
    /// <summary>
    /// Extension methods for <see cref="ListingCreatedEventHandler"/>.
    /// These helpers use reflection to invoke the public <c>HandleAsync</c> method
    /// without needing to know its exact signature at compile time.
    /// </summary>
    public static class ListingCreatedEventHandlerExtensions
    {
        /// <summary>
        /// Invokes the handler's <c>HandleAsync</c> method with the supplied arguments.
        /// </summary>
        /// <param name="handler">The <see cref="ListingCreatedEventHandler"/> instance.</param>
        /// <param name="args">Arguments that match the handler's <c>HandleAsync</c> signature.</param>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The <c>HandleAsync</c> method cannot be found on the handler.</exception>
        /// <exception cref="TargetInvocationException">The handler threw an exception.</exception>
        public static async Task InvokeHandleAsync(this ListingCreatedEventHandler handler, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentNullException.ThrowIfNull(args);

            MethodInfo method = handler.GetType().GetMethod(
                "HandleAsync",
                BindingFlags.Instance | BindingFlags.Public);

            if (method == null)
            {
                throw new InvalidOperationException(
                    $"HandleAsync method not found on type {handler.GetType().FullName}.");
            }

            object? result = method.Invoke(handler, args);
            if (result is Task task)
            {
                await task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes the handler's <c>HandleAsync</c> method and retries on transient failures.
        /// </summary>
        /// <param name="handler">The handler to execute.</param>
        /// <param name="retryCount">Maximum number of retries (default 3).</param>
        /// <param name="delay">Delay between retries (default 500 ms).</param>
        /// <param name="args">Arguments for the underlying <c>HandleAsync</c> method.</param>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="retryCount"/> is negative.</exception>
        /// <exception cref="TimeoutException">The operation timed out after all retry attempts.</exception>
        public static async Task HandleWithRetryAsync(
            this ListingCreatedEventHandler handler,
            int retryCount = 3,
            TimeSpan? delay = null,
            params object[] args)
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentOutOfRangeException.ThrowIfNegative(retryCount);
            ArgumentNullException.ThrowIfNull(args);

            TimeSpan wait = delay ?? TimeSpan.FromMilliseconds(500);
            int attempt = 0;

            while (true)
            {
                try
                {
                    await handler.InvokeHandleAsync(args).ConfigureAwait(false);
                    return; // success
                }
                catch (TargetInvocationException ex) when (attempt < retryCount)
                {
                    attempt++;
                    // Unwrap TargetInvocationException to get the actual exception
                    Exception actualException = ex.InnerException ?? ex;

                    // Only retry on transient exceptions (network, transient failures, etc.)
                    // Don't retry on ArgumentNullException, InvalidOperationException, etc.
                    if (actualException is not OperationCanceledException &&
                        actualException is not ArgumentException &&
                        actualException is not InvalidOperationException)
                    {
                        await Task.Delay(wait).ConfigureAwait(false);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Executes the handler's <c>HandleAsync</c> method with a timeout.
        /// </summary>
        /// <param name="handler">The handler to execute.</param>
        /// <param name="timeout">Maximum time to wait before cancelling.</param>
        /// <param name="cancellationToken">Cancellation token for cooperative cancellation.</param>
        /// <param name="args">Arguments for the underlying <c>HandleAsync</c> method.</param>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is not positive.</exception>
        /// <exception cref="TimeoutException">Thrown when the operation exceeds the timeout.</exception>
        public static async Task HandleWithTimeoutAsync(
            this ListingCreatedEventHandler handler,
            TimeSpan timeout,
            CancellationToken cancellationToken = default,
            params object[] args)
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);
            ArgumentNullException.ThrowIfNull(args);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            Task handlingTask = handler.InvokeHandleAsync(args);
            Task completed = await Task.WhenAny(
                    handlingTask,
                    Task.Delay(Timeout.InfiniteTimeSpan, cts.Token))
                .ConfigureAwait(false);

            if (completed != handlingTask)
            {
                throw new TimeoutException(
                    $"ListingCreatedEventHandler.HandleAsync timed out after {timeout.TotalSeconds} seconds.");
            }

            await handlingTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Logs before and after invoking the handler's <c>HandleAsync</c> method.
        /// </summary>
        /// <param name="handler">The handler to execute.</param>
        /// <param name="args">Arguments for the underlying <c>HandleAsync</c> method.</param>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is <see langword="null"/>.</exception>
        public static async Task LogAndHandleAsync(this ListingCreatedEventHandler handler, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentNullException.ThrowIfNull(args);

            Console.WriteLine($"[ListingCreated] Handling started at {DateTime.UtcNow:O}");
            await handler.InvokeHandleAsync(args).ConfigureAwait(false);
            Console.WriteLine($"[ListingCreated] Handling completed at {DateTime.UtcNow:O}");
        }
    }
}