using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MarketplaceEngine.DTOs;
using MarketplaceEngine.Domain.Services;

namespace MarketplaceEngine.Controllers
{
    /// <summary>
    /// Controller exposing moderation actions for reviews.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewModerationController : ControllerBase
    {
        private readonly ReviewReportService _reviewReportService;

        public ReviewModerationController(ReviewReportService reviewReportService)
        {
            _reviewReportService = reviewReportService;
        }

        /// <summary>
        /// Flags a review for moderation.
        /// </summary>
        /// <param name="reviewId">The identifier of the review to flag.</param>
        /// <param name="dto">Details of the report.</param>
        /// <returns>Result of the operation.</returns>
        [HttpPost("{reviewId:guid}/report")]
        public async Task<IActionResult> ReportReview(Guid reviewId, [FromBody] ReviewReportDto dto)
        {
            if (dto == null)
                return BadRequest("Report payload is required.");

            if (reviewId != dto.ReviewId)
                return BadRequest("Route reviewId does not match payload ReviewId.");

            await _reviewReportService.FlagReviewAsync(dto.ReviewId, dto.Reason, dto.Details);
            return Ok(new { Message = "Review reported successfully." });
        }
    }
}
