namespace Panel.Models;

public class ReducePowerFeatureSettings
{
    public bool Enabled { get; set; } = false;
    public int MapId { get; set; } = -1;
    public int ZoneId { get; set; } = -1;
    public int PosX { get; set; } = -1;
    public int PosY { get; set; } = -1;
    public int ProvokeMobCount { get; set; } = 1;
    public int DeadReportDelayMs { get; set; } = 1000;
    public bool AutoPunchBlackFlag { get; set; } = false;
    public bool UseHpPunch { get; set; } = false;
    public int PunchHpPercent { get; set; } = 10;
    public bool UseTdlt { get; set; } = false;
}
