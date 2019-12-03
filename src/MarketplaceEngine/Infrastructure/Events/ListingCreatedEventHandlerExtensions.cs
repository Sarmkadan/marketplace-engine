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
        public static async Task InvokeHandleAsync(this ListingCreatedEventHandler handler, params object[] args)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            MethodInfo method = handler.GetType().GetMethod(
                "HandleAsync",
                BindingFlags.Instance | BindingFlags.Public);

            if (method == null)
                throw new InvalidOperationException("HandleAsync method not found on the handler.");

            object? result = method.Invoke(handler, args);
            if (result is Task task)
                await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the handler's <c>HandleAsync</c> method and retries on transient failures.
        /// </summary>
        /// <param name="handler">The handler to execute.</param>
        /// <param name="retryCount">Maximum number of retries (default 3).</param>
        /// <param name="delay">Delay between retries (default 500 ms).</param>
        /// <param name="args">Arguments for the underlying <c>HandleAsync</c> method.</param>
        public static async Task HandleWithRetryAsync(
            this ListingCreatedEventHandler handler,
            int retryCount = 3,
            TimeSpan? delay = null,
            params object[] args)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount));

            TimeSpan wait = delay ?? TimeSpan.FromMilliseconds(500);
            int attempt = 0;

            while (true)
            {
                try
                {
                    await handler.InvokeHandleAsync(args).ConfigureAwait(false);
                    break; // success
                }
                catch when (attempt < retryCount)
                {
                    attempt++;
                    await Task.Delay(wait).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Executes the handler's <c>HandleAsync</c> method with a timeout.
        /// </summary>
        /// <param name="handler">The handler to execute.</param>
        /// <param name="timeout">Maximum time to wait before cancelling.</param>
        /// <param name="args">Arguments for the underlying <c>HandleAsync</c> method.</param>
        /// <exception cref="TimeoutException">Thrown when the operation exceeds the timeout.</exception>
        public static async Task HandleWithTimeoutAsync(
            this ListingCreatedEventHandler handler,
            TimeSpan timeout,
            params object[] args)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            using var cts = new CancellationTokenSource(timeout);
            Task handlingTask = handler.InvokeHandleAsync(args);

            Task completed = await Task.WhenAny(handlingTask, Task.Delay(Timeout.Infinite, cts.Token))
                                         .ConfigureAwait(false);

            if (completed != handlingTask)
                throw new TimeoutException("ListingCreatedEventHandler.HandleAsync timed out.");
        }

        /// <summary>
        /// Logs before and after invoking the handler's <c>HandleAsync</c> method.
        /// </summary>
        /// <param name="handler">The handler to execute.</param>
        /// <param name="args">Arguments for the underlying <c>HandleAsync</c> method.</param>
        public static async Task LogAndHandleAsync(this ListingCreatedEventHandler handler, params object[] args)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            Console.WriteLine($"[ListingCreated] Handling started at {DateTime.UtcNow:o}");
            await handler.InvokeHandleAsync(args).ConfigureAwait(false);
            Console.WriteLine($"[ListingCreated] Handling completed at {DateTime.UtcNow:o}");
        }
    }
}
