# ModerationReportExtensions

Provides convenience extension methods for working with moderation reports in the marketplace engine. These methods encapsulate common checks and calculations—such as determining whether a report warrants action, extracting a human-readable description of its target, computing its age, and evaluating whether it has exceeded a processing deadline—without requiring direct access to the underlying report internals.

## API

### IsActionable

```csharp
public static bool IsActionable(this ModerationReport report)
```

**Purpose:** Determines whether the report meets the minimum criteria to be acted upon by a moderator or automated workflow.

**Parameters:**
- `report` — The `ModerationReport` instance to evaluate. Must not be `null`.

**Return Value:** `true` if the report is in a state that permits action (e.g., not already resolved, not a duplicate, and contains sufficient evidence); otherwise `false`.

**Throws:** `ArgumentNullException` when `report` is `null`.

---

### GetTargetDescription

```csharp
public static string? GetTargetDescription(this ModerationReport report)
```

**Purpose:** Produces a human-readable description of the entity that was reported (e.g., a listing title, user display name, or message preview). Useful for moderation dashboards and audit logs.

**Parameters:**
- `report` — The `ModerationReport` instance to inspect. Must not be `null`.

**Return Value:** A `string` describing the target, or `null` if the target information is unavailable or has been redacted.

**Throws:** `ArgumentNullException` when `report` is `null`.

---

### GetAgeInHours

```csharp
public static double GetAgeInHours(this ModerationReport report)
```

**Purpose:** Calculates the elapsed time since the report was created, expressed in fractional hours. Enables time-based prioritization and SLA monitoring.

**Parameters:**
- `report` — The `ModerationReport` instance to measure. Must not be `null`.

**Return Value:** A `double` representing the age of the report in hours. Returns `0.0` for a report created at the current instant; returns a negative value if the system clock has been adjusted backward.

**Throws:** `ArgumentNullException` when `report` is `null`.

---

### IsOverdue

```csharp
public static bool IsOverdue(this ModerationReport report)
```

**Purpose:** Indicates whether the report has exceeded its maximum allowed processing time without resolution. Typically used to escalate or flag reports in monitoring tools.

**Parameters:**
- `report` — The `ModerationReport` instance to evaluate. Must not be `null`.

**Return Value:** `true` if the report’s age exceeds a predefined threshold and it remains in an actionable state; otherwise `false`.

**Throws:** `ArgumentNullException` when `report` is `null`.

## Usage

### Example 1: Filtering and Prioritizing a Report Queue

```csharp
IEnumerable<ModerationReport> incomingReports = GetPendingReports();

var actionableOverdue = incomingReports
    .Where(r => r.IsActionable() && r.IsOverdue())
    .OrderByDescending(r => r.GetAgeInHours())
    .ToList();

foreach (var report in actionableOverdue)
{
    Console.WriteLine($"Escalation: {report.GetTargetDescription() ?? "Unknown target"} — {report.GetAgeInHours():F1} hours old");
}
```

### Example 2: Dashboard Summary Generation

```csharp
ModerationReport report = GetReportById(reportId);

string target = report.GetTargetDescription();
string status = report.IsActionable() ? "Pending Review" : "Closed";
string slaStatus = report.IsOverdue() ? "Overdue" : "Within SLA";
double age = report.GetAgeInHours();

Console.WriteLine($"Report #{reportId}");
Console.WriteLine($"  Target: {target ?? "N/A"}");
Console.WriteLine($"  Status: {status}");
Console.WriteLine($"  SLA:    {slaStatus} ({age:F2} hours)");
```

## Notes

- All methods throw `ArgumentNullException` when passed a `null` report. Callers should guard against `null` before invoking these extensions, especially when retrieving reports from collections that may contain default or uninitialized entries.
- `GetAgeInHours` relies on the system clock (`DateTime.UtcNow` or equivalent). Clock adjustments (e.g., NTP corrections, daylight saving transitions) can produce small negative values or sudden jumps. Downstream logic should treat negative ages as zero and avoid assuming strictly monotonic values.
- `IsOverdue` depends on a configurable threshold. If the threshold is changed at runtime, previously evaluated results may become stale. Re-evaluate immediately before making escalation decisions.
- `GetTargetDescription` may return `null` when the target entity has been deleted, anonymized, or when the report was created with minimal metadata. Always null-check the result before display or serialization.
- These methods are stateless and do not modify the report. They are safe to call concurrently from multiple threads, provided the underlying `ModerationReport` instance is not being mutated elsewhere during the call.
