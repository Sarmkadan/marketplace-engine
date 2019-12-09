using System;
using System.Collections.Generic;
using System.Linq;
using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.Data
{
    /// <summary>
    /// Validation helpers for <see cref="MarketplaceDbContext"/>.
    /// </summary>
    public static class MarketplaceDbContextValidation
    {
        /// <summary>
        /// Validates the <paramref name="value"/> and returns a list of human‑readable problems.
        /// </summary>
        /// <param name="value">The <see cref="MarketplaceDbContext"/> instance to validate.</param>
        /// <returns>A read‑only list of validation error messages. Empty if the instance is valid.</returns>
        public static IReadOnlyList<string> Validate(this MarketplaceDbContext value)
        {
            var problems = new List<string>();

            if (value is null)
            {
                problems.Add("MarketplaceDbContext instance is null.");
                return problems;
            }

            // Collections must not be null
            if (value.Users is null)
                problems.Add("Users collection is null.");
            else if (value.Users.Any(u => u is null))
                problems.Add("Users collection contains null entries.");

            if (value.Categories is null)
                problems.Add("Categories collection is null.");
            else if (value.Categories.Any(c => c is null))
                problems.Add("Categories collection contains null entries.");

            if (value.Listings is null)
                problems.Add("Listings collection is null.");
            else if (value.Listings.Any(l => l is null))
                problems.Add("Listings collection contains null entries.");

            if (value.Messages is null)
                problems.Add("Messages collection is null.");
            else if (value.Messages.Any(m => m is null))
                problems.Add("Messages collection contains null entries.");

            if (value.ModerationReports is null)
                problems.Add("ModerationReports collection is null.");
            else if (value.ModerationReports.Any(r => r is null))
                problems.Add("ModerationReports collection contains null entries.");

            if (value.Payments is null)
                problems.Add("Payments collection is null.");
            else if (value.Payments.Any(p => p is null))
                problems.Add("Payments collection contains null entries.");

            if (value.Reviews is null)
                problems.Add("Reviews collection is null.");
            else if (value.Reviews.Any(r => r is null))
                problems.Add("Reviews collection contains null entries.");

            // Total entity count must be non‑negative and consistent
            int totalCount;
            try
            {
                totalCount = value.GetTotalEntityCount();
                if (totalCount < 0)
                    problems.Add("Total entity count is negative.");
            }
            catch (Exception ex)
            {
                problems.Add($"Calling GetTotalEntityCount threw an exception: {ex.Message}");
                totalCount = -1; // mark as invalid for further checks
            }

            // Verify that the total count matches the sum of the collections
            int sum = (value.Users?.Count ?? 0) +
                      (value.Categories?.Count ?? 0) +
                      (value.Listings?.Count ?? 0) +
                      (value.Messages?.Count ?? 0) +
                      (value.ModerationReports?.Count ?? 0) +
                      (value.Payments?.Count ?? 0) +
                      (value.Reviews?.Count ?? 0);

            if (totalCount >= 0 && totalCount != sum)
                problems.Add($"Total entity count ({totalCount}) does not match the sum of individual collections ({sum}).");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Returns <c>true</c> if the <paramref name="value"/> passes all validation checks.
        /// </summary>
        public static bool IsValid(this MarketplaceDbContext value) => !value.Validate().Any();

        /// <summary>
        /// Ensures the <paramref name="value"/> is valid, otherwise throws an <see cref="ArgumentException"/>.
        /// </summary>
        public static void EnsureValid(this MarketplaceDbContext value)
        {
            var problems = value.Validate();
            if (problems.Any())
            {
                throw new ArgumentException(
                    $"MarketplaceDbContext validation failed: {string.Join("; ", problems)}",
                    nameof(value));
            }
        }
    }
}
