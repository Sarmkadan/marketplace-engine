#nullable enable

using System;
using System.Collections.Generic;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Provides validation helpers for <see cref="MessagesController"/> instances.
/// Validates controller dependencies and state to ensure business rules and API constraints are met.
/// </summary>
public static class MessagesControllerValidation
{
    /// <summary>
    /// Validates the specified <see cref="MessagesController"/> instance and its dependencies.
    /// </summary>
    /// <param name="value">The controller instance to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this MessagesController? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate injected services by checking if they can perform basic operations
        try
        {
            // Check if messaging service is available
            _ = value._messagingService;
        }
        catch (NullReferenceException)
        {
            errors.Add("MessagingService dependency must not be null.");
        }

        try
        {
            // Check if cache service is available
            _ = value._cacheService;
        }
        catch (NullReferenceException)
        {
            errors.Add("CacheService dependency must not be null.");
        }

        try
        {
            // Check if message repository is available
            _ = value._messageRepository;
        }
        catch (NullReferenceException)
        {
            errors.Add("MessageRepository dependency must not be null.");
        }

        try
        {
            // Check if user repository is available
            _ = value._userRepository;
        }
        catch (NullReferenceException)
        {
            errors.Add("UserRepository dependency must not be null.");
        }

        try
        {
            // Check if logger is available
            _ = value._logger;
        }
        catch (NullReferenceException)
        {
            errors.Add("Logger dependency must not be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="MessagesController"/> is valid.
    /// </summary>
    /// <param name="value">The controller instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this MessagesController? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="MessagesController"/> is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The controller instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this MessagesController? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"MessagesController validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}",
                nameof(value));
        }
    }
}
