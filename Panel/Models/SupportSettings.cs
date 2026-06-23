namespace Panel.Models;

public class SupportSettings
{
    /// <summary>
    /// 0 = Không chạy, 1 = Hợp thể bông tai, 2 = Tách hợp thể
    /// </summary>
    public int BongTaiState { get; set; } = 0;

    /// <summary>
    /// Hành động đệ tử sau khi tách hợp thể:
    /// 0 = Đi theo, 1 = Bảo vệ, 2 = Tấn công, 3 = Về nhà
    /// </summary>
    public int BongTaiPetAction { get; set; } = 3;

    /// <summary>Tự động bật cờ.</summary>
    public bool AutoCoDen { get; set; } = false;

    /// <summary>Loại cờ (1-8 hoặc hơn, mặc định 8 là cờ đen).</summary>
    public int FlagType { get; set; } = 8;

    /// <summary>Tắt cờ khi có người khác trong map bật cờ.</summary>
    public bool DisableCoDenIfOthers { get; set; } = false;
}
