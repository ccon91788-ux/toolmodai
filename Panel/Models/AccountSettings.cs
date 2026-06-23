namespace Panel.Models;

public class AccountSettings
{
    public int AccountId { get; set; }
    public string SettingsJson { get; set; } = "{}";
    public string TrainSettings { get; set; } = "{}";
    public string BossSettings { get; set; } = "{}";
    public string GobackSettings { get; set; } = "{}";
    public string GeneralSettings { get; set; } = "{}";
    public string AutoPoint { get; set; } = "{}";
}
