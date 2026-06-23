using System;

namespace Panel.Models;

public class DailyMetrics
{
    public DateTime LastResetDate { get; set; } = DateTime.MinValue;
    public int InitialMvbtCount { get; set; }
    public int InitialMhbtCount { get; set; }
    public int InitialKilisCount { get; set; }

    public int DailyQuestCompletedCount { get; set; }
    public int DailyQuestCanceledCount { get; set; }
    public bool DailyQuestFinishedToday { get; set; }
    public string DailyQuestLastRunMode { get; set; } = string.Empty;
}
