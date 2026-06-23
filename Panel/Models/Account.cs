namespace Panel.Models;

public class Account
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Server { get; set; } = "Vũ trụ 1";
    public string Status { get; set; } = "0. OFFLINE";
    public string CharacterName { get; set; } = "---";
    public bool IsSelected { get; set; } = false;

    /// <summary>Process ID của game đang chạy, -1 nếu chưa launch. (Không lưu DB)</summary>
    public int ProcessId { get; set; } = -1;

    // Cấu hình lưu dưới dạng JSon (có thể null nếu chưa khởi tạo)
    public string TrainSettings { get; set; } = "{}";
    public string BossSettings { get; set; } = "{}";
    public string GobackSettings { get; set; } = "{}";
    public string GeneralSettings { get; set; } = "{}";
}

