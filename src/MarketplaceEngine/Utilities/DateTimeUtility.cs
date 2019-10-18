// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Utilities;

/// <summary>
/// DateTime utilities for consistent timestamp handling across the application.
/// Ensures all timestamps are in UTC to prevent timezone-related bugs.
/// </summary>
public static class DateTimeUtility
{
    /// <summary>
    /// Gets the current UTC timestamp.
    /// Always prefer this over DateTime.Now to maintain UTC consistency.
    /// </summary>
    public static DateTime GetCurrentUtcTime()
    {
        return DateTime.UtcNow;
    }

    /// <summary>
    /// Converts any DateTime to UTC, handling both UTC and local times.
    /// </summary>
    public static DateTime ToUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };
    }

    /// <summary>
    /// Calculates the time elapsed since a given timestamp.
    /// Useful for displaying "posted 2 hours ago" style messages.
    /// </summary>
    public static TimeSpan GetElapsedTime(DateTime dateTime)
    {
        return GetCurrentUtcTime() - ToUtc(dateTime);
    }

    /// <summary>
    /// Gets a human-readable string describing time elapsed.
    /// </summary>
    public static string GetElapsedTimeString(DateTime dateTime)
    {
        var elapsed = GetElapsedTime(dateTime);

        return elapsed.TotalSeconds < 60
            ? "just now"
            : elapsed.TotalMinutes < 60
            ? $"{(int)elapsed.TotalMinutes}m ago"
            : elapsed.TotalHours < 24
            ? $"{(int)elapsed.TotalHours}h ago"
            : elapsed.TotalDays < 30
            ? $"{(int)elapsed.TotalDays}d ago"
            : $"{dateTime:yyyy-MM-dd}";
    }

    /// <summary>
    /// Checks if a timestamp is within the last N days.
    /// </summary>
    public static bool IsWithinDays(DateTime dateTime, int days)
    {
        var cutoff = GetCurrentUtcTime().AddDays(-days);
        return ToUtc(dateTime) >= cutoff;
    }

    /// <summary>
    /// Checks if a timestamp is within the last N hours.
    /// </summary>
    public static bool IsWithinHours(DateTime dateTime, int hours)
    {
        var cutoff = GetCurrentUtcTime().AddHours(-hours);
        return ToUtc(dateTime) >= cutoff;
    }

    /// <summary>
    /// Gets the start of the day (00:00:00) for a given date.
    /// </summary>
    public static DateTime GetDayStart(DateTime dateTime)
    {
        return ToUtc(dateTime).Date;
    }

    /// <summary>
    /// Gets the end of the day (23:59:59) for a given date.
    /// </summary>
    public static DateTime GetDayEnd(DateTime dateTime)
    {
        return GetDayStart(dateTime).AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the week (Monday) for a given date.
    /// </summary>
    public static DateTime GetWeekStart(DateTime dateTime)
    {
        var utcDate = ToUtc(dateTime);
        var daysToMonday = ((int)utcDate.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return utcDate.AddDays(-daysToMonday).Date;
    }

    /// <summary>
    /// Gets the start of the month for a given date.
    /// </summary>
    public static DateTime GetMonthStart(DateTime dateTime)
    {
        var utcDate = ToUtc(dateTime);
        return new DateTime(utcDate.Year, utcDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Checks if two dates fall on the same calendar day.
    /// </summary>
    public static bool IsSameDay(DateTime dateTime1, DateTime dateTime2)
    {
        return ToUtc(dateTime1).Date == ToUtc(dateTime2).Date;
    }

    /// <summary>
    /// Formats a DateTime for ISO 8601 compliance (common in APIs).
    /// </summary>
    public static string ToIso8601String(DateTime dateTime)
    {
        return ToUtc(dateTime).ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    /// <summary>
    /// Gets a timestamp that is N minutes in the future.
    /// Useful for setting expiration times.
    /// </summary>
    public static DateTime GetFutureTime(int minutes)
    {
        return GetCurrentUtcTime().AddMinutes(minutes);
    }
}
