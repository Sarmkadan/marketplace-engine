using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MarketplaceEngine.Domain.Models;

namespace MarketplaceEngine.Domain.Services
{
    public class ReportService
    {
        private readonly ConcurrentBag<ListingReport> _reports = new();

        public void Report(Guid listingId, Guid reporterUserId, string reason)
        {
            if (listingId == Guid.Empty) throw new ArgumentException("ListingId cannot be empty.", nameof(listingId));
            if (reporterUserId == Guid.Empty) throw new ArgumentException("ReporterUserId cannot be empty.", nameof(reporterUserId));
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Reason cannot be empty.", nameof(reason));

            var report = new ListingReport(listingId, reporterUserId, reason);
            _reports.Add(report);
        }

        public IEnumerable<ListingReport> GetReportsForListing(Guid listingId)
        {
            if (listingId == Guid.Empty) throw new ArgumentException("ListingId cannot be empty.", nameof(listingId));
            return _reports.Where(r => r.ListingId == listingId).ToList();
        }
    }
}
