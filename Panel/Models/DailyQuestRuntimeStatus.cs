namespace Panel.Models;

public class DailyQuestRuntimeStatus
{
    public bool IsRunning { get; set; }
    public string RunMode { get; set; } = string.Empty;
    public string StateText { get; set; } = "Đang tắt";
    public int CompletedToday { get; set; }
    public int CanceledToday { get; set; }
    public bool FinishedToday { get; set; }
}
