using System;
using MarketplaceEngine.Domain.Enums;

namespace MarketplaceEngine.DTOs
{
    /// <summary>
    /// DTO used to submit a report for a specific review.
    /// </summary>
    public class ReviewReportDto
    {
        /// <summary>
        /// Identifier of the review being reported.
        /// </summary>
        public Guid ReviewId { get; set; }

        /// <summary>
        /// Reason for reporting the review.
        /// </summary>
        public ReviewReportReason Reason { get; set; }

        /// <summary>
        /// Optional free‑form details supplied by the reporter.
        /// </summary>
        public string? Details { get; set; }
    }
}
