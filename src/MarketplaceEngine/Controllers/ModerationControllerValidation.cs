#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Provides validation helpers for <see cref="ModerationController"/> to ensure controller
/// dependencies and method parameters are valid before execution.
/// </summary>
public static class ModerationControllerValidation
{
    /// <summary>
    /// Validates the <see cref="ModerationController"/> instance and ensures it's ready for use.
    /// </summary>
    /// <param name="value">The controller instance to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this ModerationController value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // The controller is valid if it's not null - the DI container ensures dependencies are injected
        // This method provides a consistent validation API for all controllers

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="ModerationController"/> instance is valid.
    /// </summary>
    /// <param name="value">The controller instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this ModerationController value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures the <see cref="ModerationController"/> instance is valid.
    /// </summary>
    /// <param name="value">The controller instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing a list of problems.</exception>
    public static void EnsureValid(this ModerationController value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "ModerationController validation failed. " +
                string.Join(" ", errors),
                nameof(value));
        }
    }

    /// <summary>
    /// Validates pagination parameters for report retrieval methods.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page (1-100).</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    public static IReadOnlyList<string> ValidatePagination(int page, int pageSize)
    {
        var errors = new List<string>();

        if (page < 1)
        {
            errors.Add("Page number must be at least 1.");
        }

        if (pageSize < 1)
        {
            errors.Add("Page size must be at least 1.");
        }
        else if (pageSize > 100)
        {
            errors.Add("Page size cannot exceed 100.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates pagination parameters for report retrieval methods.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page (1-100).</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValidPagination(int page, int pageSize)
    {
        return ValidatePagination(page, pageSize).Count == 0;
    }

    /// <summary>
    /// Ensures pagination parameters are valid for report retrieval methods.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page (1-100).</param>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing a list of problems.</exception>
    public static void EnsureValidPagination(int page, int pageSize)
    {
        var errors = ValidatePagination(page, pageSize);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "Pagination parameters are invalid: " +
                string.Join(" ", errors),
                nameof(page));
        }
    }

    /// <summary>
    /// Validates a report ID parameter.
    /// </summary>
    /// <param name="id">The report identifier.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateReportId(Guid id)
    {
        var errors = new List<string>();

        if (id == Guid.Empty)
        {
            errors.Add("Report ID cannot be empty.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a report ID parameter is valid.
    /// </summary>
    /// <param name="id">The report identifier.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValidReportId(Guid id)
    {
        return ValidateReportId(id).Count == 0;
    }

    /// <summary>
    /// Ensures a report ID parameter is valid.
    /// </summary>
    /// <param name="id">The report identifier.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing a list of problems.</exception>
    public static void EnsureValidReportId(Guid id)
    {
        var errors = ValidateReportId(id);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "Report ID is invalid: " +
                string.Join(" ", errors),
                nameof(id));
        }
    }

    /// <summary>
    /// Validates action notes for moderation actions.
    /// </summary>
    /// <param name="actionNotes">The action notes to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateActionNotes(string? actionNotes)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(actionNotes))
        {
            errors.Add("Action notes are required and cannot be empty or whitespace.");
        }
        else if (actionNotes.Length < 5)
        {
            errors.Add("Action notes must be at least 5 characters long.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether action notes are valid.
    /// </summary>
    /// <param name="actionNotes">The action notes to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValidActionNotes(string? actionNotes)
    {
        return ValidateActionNotes(actionNotes).Count == 0;
    }

    /// <summary>
    /// Ensures action notes are valid.
    /// </summary>
    /// <param name="actionNotes">The action notes to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing a list of problems.</exception>
    public static void EnsureValidActionNotes(string? actionNotes)
    {
        var errors = ValidateActionNotes(actionNotes);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "Action notes are invalid: " +
                string.Join(" ", errors),
                nameof(actionNotes));
        }
    }

    /// <summary>
    /// Validates rejection reasons for moderation actions.
    /// </summary>
    /// <param name="rejectionReason">The rejection reason to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateRejectionReason(string? rejectionReason)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(rejectionReason))
        {
            errors.Add("Rejection reason is required and cannot be empty or whitespace.");
        }
        else if (rejectionReason.Length < 10)
        {
            errors.Add("Rejection reason must be at least 10 characters long.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether rejection reasons are valid.
    /// </summary>
    /// <param name="rejectionReason">The rejection reason to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValidRejectionReason(string? rejectionReason)
    {
        return ValidateRejectionReason(rejectionReason).Count == 0;
    }

    /// <summary>
    /// Ensures rejection reasons are valid.
    /// </summary>
    /// <param name="rejectionReason">The rejection reason to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing a list of problems.</exception>
    public static void EnsureValidRejectionReason(string? rejectionReason)
    {
        var errors = ValidateRejectionReason(rejectionReason);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "Rejection reason is invalid: " +
                string.Join(" ", errors),
                nameof(rejectionReason));
        }
    }

    /// <summary>
    /// Validates report reasons for creating new reports.
    /// </summary>
    /// <param name="reason">The report reason to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateReportReason(string? reason)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(reason))
        {
            errors.Add("Report reason is required and cannot be empty or whitespace.");
        }
        else if (reason.Length < 10)
        {
            errors.Add("Report reason must be at least 10 characters long.");
        }
        else if (reason.Length > 1000)
        {
            errors.Add("Report reason cannot exceed 1000 characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether report reasons are valid.
    /// </summary>
    /// <param name="reason">The report reason to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValidReportReason(string? reason)
    {
        return ValidateReportReason(reason).Count == 0;
    }

    /// <summary>
    /// Ensures report reasons are valid.
    /// </summary>
    /// <param name="reason">The report reason to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing a list of problems.</exception>
    public static void EnsureValidReportReason(string? reason)
    {
        var errors = ValidateReportReason(reason);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "Report reason is invalid: " +
                string.Join(" ", errors),
                nameof(reason));
        }
    }

    /// <summary>
    /// Validates bulk moderation requests.
    /// </summary>
    /// <param name="listingIds">The list of listing IDs to validate.</param>
    /// <param name="action">The moderation action to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateBulkModeration(IReadOnlyList<Guid>? listingIds, string? action)
    {
        var errors = new List<string>();

        if (listingIds is null || listingIds.Count == 0)
        {
            errors.Add("At least one listing ID is required for bulk moderation.");
        }
        else
        {
            for (int i = 0; i < listingIds.Count; i++)
            {
                if (listingIds[i] == Guid.Empty)
                {
                    errors.Add($"Listing ID at index {i} cannot be empty.");
                }
            }
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            errors.Add("Action is required for bulk moderation (approve, remove, escalate).");
        }
        else if (action.Length < 3 || action.Length > 20)
        {
            errors.Add("Action must be between 3 and 20 characters long.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether bulk moderation requests are valid.
    /// </summary>
    /// <param name="listingIds">The list of listing IDs to check.</param>
    /// <param name="action">The moderation action to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValidBulkModeration(IReadOnlyList<Guid>? listingIds, string? action)
    {
        return ValidateBulkModeration(listingIds, action).Count == 0;
    }

    /// <summary>
    /// Ensures bulk moderation requests are valid.
    /// </summary>
    /// <param name="listingIds">The list of listing IDs to validate.</param>
    /// <param name="action">The moderation action to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing a list of problems.</exception>
    public static void EnsureValidBulkModeration(IReadOnlyList<Guid>? listingIds, string? action)
    {
        var errors = ValidateBulkModeration(listingIds, action);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "Bulk moderation request is invalid: " +
                string.Join(" ", errors),
                nameof(listingIds));
        }
    }
}