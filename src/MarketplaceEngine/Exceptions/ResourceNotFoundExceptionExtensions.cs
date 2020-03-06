using System;

namespace MarketplaceEngine.Exceptions
{
    /// <summary>
    /// Provides extension methods for <see cref="ResourceNotFoundException"/> to facilitate error handling and resource identification.
    /// </summary>
    /// <remarks>
    /// The methods in this class generate user‑friendly messages and allow quick identification of a missing resource,
    /// while also providing a guard clause that throws <see cref="ArgumentNullException"/> when a null exception instance is supplied.
    /// </remarks>
    public static class ResourceNotFoundExceptionExtensions
    {
        /// <summary>
        /// Gets a formatted error message describing the missing resource.
        /// </summary>
        /// <param name="exception">The exception containing resource information.</param>
        /// <returns>A formatted error message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
        /// <remarks>
        /// Returns a string in the form <c>"Resource {ResourceType} with ID {ResourceId} not found."</c>.
        /// </remarks>
        public static string GetErrorMessage(this ResourceNotFoundException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return $"Resource {exception.ResourceType} with ID {exception.ResourceId} not found.";
        }

        /// <summary>
        /// Gets a string representation of the resource type and ID for logging or identification purposes.
        /// </summary>
        /// <param name="exception">The exception containing resource information.</param>
        /// <returns>A formatted string in the format "ResourceType - ResourceId".</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
        /// <remarks>
        /// Returns a string that concatenates <see cref="ResourceNotFoundException.ResourceType"/> and <see cref="ResourceNotFoundException.ResourceId"/>
        /// separated by a hyphen and spaces, e.g., <c>"Product - 12345"</c>.
        /// </remarks>
        public static string GetResourceDetails(this ResourceNotFoundException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return $"{exception.ResourceType} - {exception.ResourceId}";
        }

        /// <summary>
        /// Determines whether the specified exception is a <see cref="ResourceNotFoundException"/>.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns>True if the exception is a <see cref="ResourceNotFoundException"/>; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
        /// <remarks>
        /// Performs a simple type check using the <c>is</c> operator after validating that <paramref name="exception"/> is not null.
        /// </remarks>
        public static bool IsResourceNotFound(this Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception is ResourceNotFoundException;
        }
    }
}
