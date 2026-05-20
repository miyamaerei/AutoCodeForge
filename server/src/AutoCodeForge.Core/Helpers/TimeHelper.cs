namespace AutoCodeForge.Core.Helpers;

/// <summary>
/// Provides common UTC and local time conversion helpers.
/// </summary>
public static class TimeHelper
{
    /// <summary>
    /// Gets the current UTC timestamp.
    /// </summary>
    /// <returns>The current UTC time.</returns>
    public static DateTime UtcNow() => DateTime.UtcNow;

    /// <summary>
    /// Converts a time value to UTC.
    /// </summary>
    /// <param name="time">The time to normalize.</param>
    /// <returns>The UTC time.</returns>
    public static DateTime ToUtc(DateTime time)
    {
        return time.Kind == DateTimeKind.Utc ? time : time.ToUniversalTime();
    }

    /// <summary>
    /// Converts a UTC time value to local time.
    /// </summary>
    /// <param name="utcTime">The UTC time to convert.</param>
    /// <returns>The local time.</returns>
    public static DateTime ToLocal(DateTime utcTime)
    {
        var normalized = utcTime.Kind == DateTimeKind.Utc
            ? utcTime
            : DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);

        return normalized.ToLocalTime();
    }
}