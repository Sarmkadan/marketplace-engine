#nullable enable

using System;
using System.Collections.Generic;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Provides validation helpers for <see cref="SellerDashboardDto"/> instances.
/// </summary>
public static class SellerDashboardDtoValidation
{
	/// <summary>
	/// Validates a <see cref="SellerDashboardDto"/> instance and returns a list of validation problems.
	/// </summary>
	/// <param name="value">The DTO to validate.</param>
	/// <returns>A read-only list of validation error messages. Empty if valid.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static IReadOnlyList<string> Validate(this SellerDashboardDto? value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var errors = new List<string>();

		// Validate SellerId
		if (value.SellerId == Guid.Empty)
		{
			errors.Add("SellerId must be a non-empty GUID.");
		}

		// Validate SellerName
		ArgumentException.ThrowIfNullOrEmpty(value.SellerName, nameof(value.SellerName));
		if (value.SellerName.Length > 200)
		{
			errors.Add("SellerName must not exceed 200 characters.");
		}

		// Validate ActiveListings
		if (value.ActiveListings < 0)
		{
			errors.Add("ActiveListings must be non-negative.");
		}

		// Validate TotalListings
		if (value.TotalListings < 0)
		{
			errors.Add("TotalListings must be non-negative.");
		}
		else if (value.TotalListings < value.ActiveListings)
		{
			errors.Add("TotalListings cannot be less than ActiveListings.");
		}

		// Validate TotalSales
		if (value.TotalSales < 0)
		{
			errors.Add("TotalSales must be non-negative.");
		}

		// Validate TotalRevenue
		if (value.TotalRevenue < 0)
		{
			errors.Add("TotalRevenue must be non-negative.");
		}

		// Validate PendingPayout
		if (value.PendingPayout < 0)
		{
			errors.Add("PendingPayout must be non-negative.");
		}

		// Validate AverageRating
		if (value.AverageRating < 0 || value.AverageRating > 5)
		{
			errors.Add("AverageRating must be between 0 and 5.");
		}

		// Validate TotalReviews
		if (value.TotalReviews < 0)
		{
			errors.Add("TotalReviews must be non-negative.");
		}

		// Validate UnreadMessages
		if (value.UnreadMessages < 0)
		{
			errors.Add("UnreadMessages must be non-negative.");
		}

		// Validate LastActivityAt
		if (value.LastActivityAt.HasValue)
		{
			var lastActivityAt = value.LastActivityAt.Value;
			if (lastActivityAt > DateTime.UtcNow)
			{
				errors.Add("LastActivityAt must not be in the future.");
			}
		}

		return errors.AsReadOnly();
	}

	/// <summary>
	/// Determines whether a <see cref="SellerDashboardDto"/> instance is valid.
	/// </summary>
	/// <param name="value">The DTO to check.</param>
	/// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static bool IsValid(this SellerDashboardDto? value)
	{
		ArgumentNullException.ThrowIfNull(value);
		return Validate(value).Count == 0;
	}

	/// <summary>
	/// Ensures that a <see cref="SellerDashboardDto"/> instance is valid, throwing an exception if not.
	/// </summary>
	/// <param name="value">The DTO to validate.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid.
	/// The exception message contains all validation errors.</exception>
	public static void EnsureValid(this SellerDashboardDto? value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var errors = Validate(value);
		if (errors.Count > 0)
		{
			throw new ArgumentException(
				$"SellerDashboardDto validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}",
				nameof(value));
		}
	}
}