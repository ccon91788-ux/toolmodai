using System;

namespace Panel.Models;

public class ScheduleSettings
{
    public bool IsScheduleEnabled { get; set; } = false;
    
    // TimeSpan stored as string for easier JSON serialization without external converters 
    // Format: "hh:mm:ss"
    public string StartTime { get; set; } = "05:00:00";
    public string EndTime { get; set; } = "08:00:00";

    // Helper properties to convert string to TimeSpan safely
    public TimeSpan GetStartTime() => TimeSpan.TryParse(StartTime, out var ts) ? ts : new TimeSpan(5, 0, 0);
    public TimeSpan GetEndTime() => TimeSpan.TryParse(EndTime, out var ts) ? ts : new TimeSpan(8, 0, 0);
}
