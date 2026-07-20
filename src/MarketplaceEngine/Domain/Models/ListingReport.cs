using System;

namespace MarketplaceEngine.Domain.Models
{
    public class ListingReport
    {
        public Guid ListingId { get; set; }
        public Guid ReporterUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }

        public ListingReport(Guid listingId, Guid reporterUserId, string reason)
        {
            ListingId = listingId;
            ReporterUserId = reporterUserId;
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            CreatedAtUtc = DateTime.UtcNow;
        }
    }
}
