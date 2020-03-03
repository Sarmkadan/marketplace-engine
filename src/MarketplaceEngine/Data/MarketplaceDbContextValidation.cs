using System;
using System.Collections.Generic;
using System.Linq;
using MarketplaceEngine.Domain.Models;
using System.Diagnostics.CodeAnalysis;

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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this MarketplaceDbContext value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Collections must not be null
            problems.AddRange(ValidateCollection(value.Users, nameof(value.Users)));
            problems.AddRange(ValidateCollection(value.Categories, nameof(value.Categories)));
            problems.AddRange(ValidateCollection(value.Listings, nameof(value.Listings)));
            problems.AddRange(ValidateCollection(value.Messages, nameof(value.Messages)));
            problems.AddRange(ValidateCollection(value.ModerationReports, nameof(value.ModerationReports)));
            problems.AddRange(ValidateCollection(value.Payments, nameof(value.Payments)));
            problems.AddRange(ValidateCollection(value.Reviews, nameof(value.Reviews)));

            // Total entity count must be non‑negative and consistent
            int totalCount;
            try
            {
                totalCount = value.GetTotalEntityCount();
                if (totalCount < 0)
                {
                    problems.Add("Total entity count is negative.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"Calling GetTotalEntityCount threw an exception: {ex.Message}");
                totalCount = -1; // mark as invalid for further checks
            }

            // Verify that the total count matches the sum of the collections
            int sum = (value.Users?.Count ?? 0)
                + (value.Categories?.Count ?? 0)
                + (value.Listings?.Count ?? 0)
                + (value.Messages?.Count ?? 0)
                + (value.ModerationReports?.Count ?? 0)
                + (value.Payments?.Count ?? 0)
                + (value.Reviews?.Count ?? 0);

            if (totalCount >= 0 && totalCount != sum)
            {
                problems.Add(string.Format("Total entity count ({0}) does not match the sum of individual collections ({1}).", totalCount, sum));
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates a collection and returns validation messages if issues are found.
        /// </summary>
        /// <typeparam name="T">The collection element type.</typeparam>
        /// <param name="collection">The collection to validate.</param>
        /// <param name="collectionName">The name of the collection property.</param>
        /// <returns>List of validation error messages, or empty list if valid.</returns>
        private static IEnumerable<string> ValidateCollection<T>(IEnumerable<T>? collection, string collectionName)
        {
            if (collection is null)
            {
                yield return string.Format("Collection '{0}' is null.", collectionName);
                yield break;
            }

            if (collection.Any(item => item is null))
            {
                yield return string.Format("Collection '{0}' contains null entries.", collectionName);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the <paramref name="value"/> passes all validation checks.
        /// </summary>
        /// <param name="value">The <see cref="MarketplaceDbContext"/> to check.</param>
        /// <returns>True if valid; otherwise false.</returns>
        public static bool IsValid(this MarketplaceDbContext value) => !value.Validate().Any();

        /// <summary>
        /// Ensures the <paramref name="value"/> is valid, otherwise throws an <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="value">The <see cref="MarketplaceDbContext"/> to validate.</param>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        public static void EnsureValid(this MarketplaceDbContext value)
        {
            var problems = value.Validate();
            if (problems.Any())
            {
                throw new ArgumentException(
                    string.Format("MarketplaceDbContext validation failed: {0}", string.Join("; ", problems)),
                    nameof(value));
            }
        }
    }
}