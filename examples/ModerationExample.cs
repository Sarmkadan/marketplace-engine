#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Services;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.Enums;
using MarketplaceEngine.Domain.ValueObjects;
using MarketplaceEngine.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarketplaceEngine.Examples;

/// <summary>
/// Demonstrates content moderation and safety features.
/// This example shows how to:
/// - Create moderation reports
/// - Retrieve reports (admin/moderator view)
/// - Update report status
/// - Enforce moderation decisions
/// </summary>
public class ModerationExample
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddMarketplaceServices();
        var provider = services.BuildServiceProvider();

        var moderationService = provider.GetRequiredService<ModerationService>();
        var listingService = provider.GetRequiredService<ListingService>();

        Console.WriteLine("=== Marketplace Engine - Moderation Example ===\n");

        try
        {
            // Set up test data
            Console.WriteLine("Setting up test data...");
            var suspiciousListing = await listingService.CreateListingAsync(
                sellerId: 1,
                title: "Test Listing",
                description: "Test content",
                price: new Money(100m, "USD"),
                category: "Electronics",
                tags: new[] { "test" },
                location: new Location { City = "New York", Country = "USA" }
            );
            Console.WriteLine("✓ Test data created\n");

            // Example 1: Create a low-priority report
            Console.WriteLine("1. Creating a low-priority report...");
            var lowPriorityReport = await moderationService.CreateReportAsync(
                reporterId: 5,
                reason: "Listing title unclear",
                priority: ReportPriority.Low,
                targetListingId: suspiciousListing.Id,
                description: "The title doesn't clearly describe the item"
            );
            Console.WriteLine($"✓ Report created:");
            Console.WriteLine($"  ID: {lowPriorityReport.Id}");
            Console.WriteLine($"  Priority: {lowPriorityReport.Priority}");
            Console.WriteLine($"  Status: {lowPriorityReport.Status}");
            Console.WriteLine($"  Reason: {lowPriorityReport.Reason}\n");

            // Example 2: Create a high-priority report
            Console.WriteLine("2. Creating a high-priority report...");
            var highPriorityReport = await moderationService.CreateReportAsync(
                reporterId: 6,
                reason: "Potentially counterfeit item",
                priority: ReportPriority.High,
                targetListingId: suspiciousListing.Id,
                description: "This appears to be a counterfeit Apple product"
            );
            Console.WriteLine($"✓ High-priority report created:");
            Console.WriteLine($"  ID: {highPriorityReport.Id}");
            Console.WriteLine($"  Priority: {highPriorityReport.Priority}\n");

            // Example 3: Get pending reports (for moderator)
            Console.WriteLine("3. Retrieving pending reports for moderation...");
            var pendingReports = await moderationService.GetReportsByStatusAsync(ReportStatus.Pending).ConfigureAwait(false);
            Console.WriteLine($"✓ Found {pendingReports.Count} pending reports:");
            foreach (var report in pendingReports.OrderByDescending(r => r.Priority))
            {
                var priorityLabel = GetPriorityLabel(report.Priority);
                Console.WriteLine($"  - [{priorityLabel}] {report.Reason}");
                Console.WriteLine($"    Target: Listing {report.TargetListingId}");
                Console.WriteLine($"    Reporter: User {report.ReporterId}");
            }
            Console.WriteLine();

            // Example 4: Assign report to moderator
            Console.WriteLine("4. Assigning high-priority report to moderator 10...");
            await moderationService.AssignReportAsync(
                reportId: highPriorityReport.Id,
                moderatorId: 10
            );
            Console.WriteLine($"✓ Report assigned to moderator 10\n");

            // Example 5: Update report to "In Review"
            Console.WriteLine("5. Updating report status to 'In Review'...");
            await moderationService.UpdateReportStatusAsync(
                reportId: highPriorityReport.Id,
                newStatus: ReportStatus.InReview,
                notes: "Investigating the claim. Need to verify product authenticity."
            );
            Console.WriteLine($"✓ Report status updated\n");

            // Example 6: Approve report and remove listing
            Console.WriteLine("6. Approving report and taking action...");
            await moderationService.UpdateReportStatusAsync(
                reportId: highPriorityReport.Id,
                newStatus: ReportStatus.Approved,
                notes: "Confirmed counterfeit product. Listing removed."
            );
            Console.WriteLine($"✓ Report approved. Taking action...");
            Console.WriteLine($"  Action: Remove listing {suspiciousListing.Id}\n");

            // Example 7: Reject low-priority report
            Console.WriteLine("7. Rejecting low-priority report...");
            await moderationService.UpdateReportStatusAsync(
                reportId: lowPriorityReport.Id,
                newStatus: ReportStatus.Rejected,
                notes: "Title is sufficiently clear. No action needed."
            );
            Console.WriteLine($"✓ Report rejected\n");

            // Example 8: Get reports by priority
            Console.WriteLine("8. Getting high-priority reports...");
            var highPriorityReports = await moderationService.GetReportsByPriorityAsync(ReportPriority.High).ConfigureAwait(false);
            Console.WriteLine($"✓ Found {highPriorityReports.Count} high-priority reports\n");

            // Example 9: Get moderator's assigned reports
            Console.WriteLine("9. Getting reports assigned to moderator 10...");
            var moderatorReports = await moderationService.GetReportsAssignedToAsync(moderatorId: 10).ConfigureAwait(false);
            Console.WriteLine($"✓ Moderator has {moderatorReports.Count} assigned reports:");
            foreach (var report in moderatorReports)
            {
                Console.WriteLine($"  - {report.Reason} ({report.Status})");
            }
            Console.WriteLine();

            // Example 10: Create critical priority report
            Console.WriteLine("10. Creating a critical priority report...");
            var criticalReport = await moderationService.CreateReportAsync(
                reporterId: 7,
                reason: "Illegal item listing - firearms",
                priority: ReportPriority.Critical,
                targetListingId: suspiciousListing.Id,
                description: "This listing appears to be for an illegal firearm"
            );
            Console.WriteLine($"✓ Critical report created:");
            Console.WriteLine($"  Priority: {criticalReport.Priority}");
            Console.WriteLine($"  Status: {criticalReport.Status}\n");

            // Example 11: Moderation statistics
            Console.WriteLine("11. Moderation statistics...");
            var allReports = await moderationService.GetAllReportsAsync().ConfigureAwait(false);
            var byStatus = allReports.GroupBy(r => r.Status).ToDictionary(g => g.Key, g => g.Count());
            var byPriority = allReports.GroupBy(r => r.Priority).ToDictionary(g => g.Key, g => g.Count());

            Console.WriteLine($"✓ Total reports: {allReports.Count}");
            Console.WriteLine($"  By Status:");
            foreach (var kvp in byStatus)
            {
                Console.WriteLine($"    - {kvp.Key}: {kvp.Value}");
            }
            Console.WriteLine($"  By Priority:");
            foreach (var kvp in byPriority)
            {
                Console.WriteLine($"    - {kvp.Key}: {kvp.Value}");
            }
            Console.WriteLine();

            Console.WriteLine("=== Example completed successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    private static string GetPriorityLabel(ReportPriority priority) => priority switch
    {
        ReportPriority.Low => "LOW",
        ReportPriority.Medium => "MEDIUM",
        ReportPriority.High => "HIGH",
        ReportPriority.Critical => "CRITICAL",
        _ => "UNKNOWN"
    };
}
