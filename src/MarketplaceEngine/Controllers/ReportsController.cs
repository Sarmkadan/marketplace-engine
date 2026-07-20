using System;
using Microsoft.AspNetCore.Mvc;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.Services;

namespace MarketplaceEngine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportsController(ReportService reportService)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        }

        public class ReportRequest
        {
            public Guid ListingId { get; set; }
            public Guid ReporterUserId { get; set; }
            public string Reason { get; set; } = string.Empty;
        }

        [HttpPost]
        public IActionResult Report([FromBody] ReportRequest request)
        {
            if (request == null) return BadRequest("Request body is required.");

            _reportService.Report(request.ListingId, request.ReporterUserId, request.Reason);
            return Ok(new { Message = "Report submitted successfully." });
        }

        [HttpGet("{listingId:guid}")]
        public IActionResult GetReports(Guid listingId)
        {
            var reports = _reportService.GetReportsForListing(listingId);
            return Ok(reports);
        }
    }
}
