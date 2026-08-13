using System;
using MarketplaceEngine.Domain.Enums;

namespace MarketplaceEngine.Domain.Models
{
    /// <summary>
    /// Domain model representing a report made against a review.
    /// </summary>
    public class ReviewReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ReviewId { get; set; }

        public ReviewReportReason Reason { get; set; }

        public string? Details { get; set; }

        /// <summary>
        /// Indicates whether the report has been resolved by moderation staff.
        /// </summary>
        public bool Resolved { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
