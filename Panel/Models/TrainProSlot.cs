namespace Panel.Models;

/// <summary>Một slot trong lịch train theo giờ (TrainPro).</summary>
public class TrainProSlot
{
    /// <summary>Phút bắt đầu tính từ 00:00 GMT+7 (0-1439). -1 = chưa đặt.</summary>
    public int StartMinute { get; set; } = -1;

    /// <summary>Phút kết thúc tính từ 00:00 GMT+7 (0-1439). -1 = chưa đặt.</summary>
    public int EndMinute { get; set; } = -1;

    /// <summary>Map ID đích. -1 = chưa đặt.</summary>
    public int MapId { get; set; } = -1;

    /// <summary>Zone ID đích.</summary>
    public int ZoneId { get; set; } = 0;

    /// <summary>Danh sách mob ID cách nhau bởi dấu ';'. Rỗng = tất cả quái.</summary>
    public string MobIds { get; set; } = "";

    // ── Helpers ──────────────────────────────────────────────────────────

    public bool IsValid => StartMinute >= 0 && EndMinute >= 0 && MapId >= 0;

    /// <summary>Parse chuỗi HH:mm thành số phút (0-1439). Trả -1 nếu lỗi.</summary>
    public static int ParseHHmm(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return -1;
        var p = s.Trim().Split(':');
        if (p.Length != 2) return -1;
        if (!int.TryParse(p[0].Trim(), out int h) || !int.TryParse(p[1].Trim(), out int m)) return -1;
        if (h < 0 || h > 23 || m < 0 || m > 59) return -1;
        return h * 60 + m;
    }

    /// <summary>Định dạng số phút thành chuỗi HH:mm.</summary>
    public static string FormatMinute(int minute)
    {
        if (minute < 0) return "--:--";
        int h = minute / 60, m = minute % 60;
        return $"{h:D2}:{m:D2}";
    }

    public string StartText => FormatMinute(StartMinute);
    public string EndText => FormatMinute(EndMinute);
}
