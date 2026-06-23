namespace Panel.Models;

public class AttendanceFeatureSettings
{
    [System.Text.Json.Serialization.JsonIgnore]
    public bool Enabled { get; set; }
    public bool AutoStart { get; set; } = true;
    public bool ScheduleEnabled { get; set; } = false;
    public int ScheduleHour { get; set; } = 7;
    public int ScheduleMinute { get; set; }
    public bool ClaimMonthly { get; set; } = true;
    public bool ClaimContinuous { get; set; } = true;
    public bool ClaimOnline { get; set; } = true;
    public string MonthlyClaimedKey { get; set; } = string.Empty;
    public string ContinuousClaimDate { get; set; } = string.Empty;
    public string OnlineClaimDate { get; set; } = string.Empty;
    public int OnlineClaimedCount { get; set; }
    public int NextOnlineSeconds { get; set; } = -1;
    public bool CanClaimOnline { get; set; }
    public string StateText { get; set; } = "Đang tắt";
    public string LastCheckTime { get; set; } = string.Empty;
}
