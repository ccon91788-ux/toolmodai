namespace Panel.Models;

public class KilisFeatureSettings
{
    public bool Enabled { get; set; }
    public int StartHour { get; set; } = 0;
    public int StartMin { get; set; } = 0;
    public int StopHour { get; set; } = 0;
    public int StopMin { get; set; } = 0;
    public int ZoneId { get; set; } = 0;
    public bool AutoBuyAmulet { get; set; }
    public int AmuletType { get; set; }
    public bool UseTDLT { get; set; } = false;
    public bool AutoZone { get; set; } = false;
    // Giáp luyện tập: 0=Không chạy, 1=Mặc luyện tập, 2=Tháo luyện tập
    public int TrainingArmorMode { get; set; } = 0;
}
