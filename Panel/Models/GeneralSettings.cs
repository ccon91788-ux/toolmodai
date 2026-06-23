namespace Panel.Models;

public class GeneralSettings
{
    public bool EatChicken { get; set; } = true;
    public bool UseTdltXmap { get; set; } = false;
    // Delay chờ server thật gửi mob data sau khi load map xong (ms)
    public int PostMapLoadDelay { get; set; } = 300;

    // Proxy settings (per-account)
    // 0 = HTTP, 1 = SOCKS5
    public bool UseProxy { get; set; } = true;
    public int ProxyType { get; set; } = 0;
    public string ProxyAddress { get; set; } = "";

    // Phân loại tài khoản (dùng cho Quản lý Config - dán theo nhóm)
    public int TypeAccount { get; set; } = 0;

    // Hành động khi nhân vật bị chết:
    // 0 = Về nhà, 1 = Hồi sinh Ngọc, 2 = HS Ngọc (Về nhà nếu hết ngọc), 3 = Chờ (Đứng yên)
    public int ActionOnDeath { get; set; } = 0;
}
