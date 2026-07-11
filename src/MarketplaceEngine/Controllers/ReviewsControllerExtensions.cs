#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using MarketplaceEngine.DTOs;

namespace MarketplaceEngine.Controllers;

/// <summary>
/// Extension methods for <see cref="ReviewsController"/> providing additional convenience methods
/// for common review operations and batch processing scenarios.
/// </summary>
public static class ReviewsControllerExtensions
{
	/// <summary>
	/// Retrieves paginated reviews for a seller with filtering by minimum score.
	/// </summary>
	/// <param name="controller">The reviews controller instance.</param>
	/// <param name="sellerId">The seller identifier.</param>
	/// <param name="minScore">Minimum score threshold (inclusive).</param>
	/// <param name="page">Page number (1-based).</param>
	/// <param name="pageSize">Number of items per page.</param>
	/// <returns>Paginated response with filtered reviews.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
	public static async Task<IActionResult> GetSellerReviewsByMinScore(
		this ReviewsController controller,
		Guid sellerId,
		int minScore = 1,
		int page = 1,
		int pageSize = 20)
	{
		ArgumentNullException.ThrowIfNull(controller);

		if (minScore < 1 || minScore > 5)
			return controller.BadRequest("Minimum score must be between 1 and 5.");

		if (page < 1 || pageSize < 1 || pageSize > 100)
			return controller.BadRequest("Invalid pagination parameters.");

		var actionResult = await controller.GetSellerReviews(sellerId, page, pageSize);

		if (actionResult is not OkObjectResult okResult)
			return actionResult;

		if (okResult.Value is not PaginatedResponse<ReviewDto> paginatedResponse)
			return controller.BadRequest("Invalid response format.");

		var filteredReviews = paginatedResponse.Items.Where(r => r.Score >= minScore).ToList();

		var response = new PaginatedResponse<ReviewDto>
		{
			Items = filteredReviews,
			Page = paginatedResponse.Page,
			PageSize = paginatedResponse.PageSize,
			Total = filteredReviews.Count
		};

		return controller.Ok(response);
	}

	/// <summary>
	/// Retrieves reviews for multiple sellers in a single batch request.
	/// </summary>
	/// <param name="controller">The reviews controller instance.</param>
	/// <param name="sellerIds">Collection of seller identifiers.</param>
	/// <param name="page">Page number (1-based).</param>
	/// <param name="pageSize">Number of items per page.</param>
	/// <returns>Dictionary mapping seller IDs to their paginated reviews.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="sellerIds"/> is <see langword="null"/>.</exception>
	public static async Task<IActionResult> GetMultipleSellersReviews(
		this ReviewsController controller,
		IEnumerable<Guid> sellerIds,
		int page = 1,
		int pageSize = 20)
	{
		ArgumentNullException.ThrowIfNull(controller);
		ArgumentNullException.ThrowIfNull(sellerIds);

		if (page < 1 || pageSize < 1 || pageSize > 100)
			return controller.BadRequest("Invalid pagination parameters.");

		var sellerIdList = sellerIds.ToList();
		if (!sellerIdList.Any())
			return controller.BadRequest("At least one seller ID is required.");

		var tasks = sellerIdList.Select(async sellerId =>
		{
			var actionResult = await controller.GetSellerReviews(sellerId, page, pageSize);

			if (actionResult is OkObjectResult okResult && okResult.Value is PaginatedResponse<ReviewDto> paginatedResponse)
			{
				return new SellerReviewResult
				{
					SellerId = sellerId,
					Reviews = paginatedResponse.Items,
					Total = paginatedResponse.Total,
					Page = paginatedResponse.Page,
					PageSize = paginatedResponse.PageSize
				};
			}

			return new SellerReviewResult
			{
				SellerId = sellerId,
				Reviews = [],
				Total = 0,
				Page = page,
				PageSize = pageSize
			};
		}).ToList();

		var results = await Task.WhenAll(tasks);

		var response = results.ToDictionary(
			r => r.SellerId,
			r => new PaginatedResponse<ReviewDto>
			{
				Items = r.Reviews,
				Page = r.Page,
				PageSize = r.PageSize,
				Total = r.Total
			}
		);

		return controller.Ok(response);
	}

	private sealed class SellerReviewResult
	{
		public Guid SellerId { get; set; }
		public List<ReviewDto> Reviews { get; set; } = [];
		public int Total { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
	}

	/// <summary>
	/// Retrieves reviews for a listing with optional score range filtering.
	/// </summary>
	/// <param name="controller">The reviews controller instance.</param>
	/// <param name="listingId">The listing identifier.</param>
	/// <param name="minScore">Minimum score threshold (inclusive).</param>
	/// <param name="maxScore">Maximum score threshold (inclusive).</param>
	/// <returns>Filtered list of reviews for the listing.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
	public static async Task<IActionResult> GetListingReviewsByScoreRange(
		this ReviewsController controller,
		Guid listingId,
		int? minScore = null,
		int? maxScore = null)
	{
		ArgumentNullException.ThrowIfNull(controller);

		var actionResult = await controller.GetListingReviews(listingId);

		if (actionResult is not OkObjectResult okResult)
			return actionResult;

		if (okResult.Value is not List<ReviewDto> reviews)
			return controller.BadRequest("Invalid response format.");

		var filteredReviews = reviews.AsEnumerable();

		if (minScore.HasValue)
		{
			if (minScore < 1 || minScore > 5)
				return controller.BadRequest("Minimum score must be between 1 and 5.");

			filteredReviews = filteredReviews.Where(r => r.Score >= minScore.Value);
		}

		if (maxScore.HasValue)
		{
			if (maxScore < 1 || maxScore > 5)
				return controller.BadRequest("Maximum score must be between 1 and 5.");

			filteredReviews = filteredReviews.Where(r => r.Score <= maxScore.Value);
		}

		return controller.Ok(filteredReviews.ToList());
	}

	/// <summary>
	/// Retrieves summary statistics for multiple sellers in a single batch request.
	/// </summary>
	/// <param name="controller">The reviews controller instance.</param>
	/// <param name="sellerIds">Collection of seller identifiers.</param>
	/// <returns>Dictionary mapping seller IDs to their review summaries.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="sellerIds"/> is <see langword="null"/>.</exception>
	public static async Task<IActionResult> GetMultipleSellersSummaries(
		this ReviewsController controller,
		IEnumerable<Guid> sellerIds)
	{
		ArgumentNullException.ThrowIfNull(controller);
		ArgumentNullException.ThrowIfNull(sellerIds);

		var sellerIdList = sellerIds.ToList();
		if (!sellerIdList.Any())
			return controller.BadRequest("At least one seller ID is required.");

		var tasks = sellerIdList.Select(async sellerId =>
		{
			var summaryActionResult = await controller.GetSellerSummary(sellerId);

			if (summaryActionResult is OkObjectResult summaryOkResult && summaryOkResult.Value is ReviewSummaryDto summaryDto)
			{
				return new SellerSummaryResult
				{
					SellerId = sellerId,
					Summary = summaryDto
				};
			}

			return new SellerSummaryResult
			{
				SellerId = sellerId,
				Summary = new ReviewSummaryDto
				{
					SellerId = sellerId,
					AverageScore = 0,
					TotalReviews = 0,
					ScoreDistribution = new Dictionary<int, int>()
				}
			};
		}).ToList();

		var results = await Task.WhenAll(tasks);

		var response = results.ToDictionary(
			r => r.SellerId,
			r => r.Summary
		);

		return controller.Ok(response);
	}

	private sealed class SellerSummaryResult
	{
		public Guid SellerId { get; set; }
		public ReviewSummaryDto Summary { get; set; } = null!;
	}
}