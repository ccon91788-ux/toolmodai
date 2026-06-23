namespace Panel.Models;

public class DailyQuestFeatureSettings
{
    [System.Text.Json.Serialization.JsonIgnore]
    public bool Enabled { get; set; }
    public bool EnableSync { get; set; } = true;
    public string Difficulty { get; set; } = "siêu khó";
    public bool ScheduleEnabled { get; set; }
    public int StartHour { get; set; } = 4;
    public int StartMinute { get; set; }
    public bool CancelKillPlayerQuest { get; set; }
    public bool CancelTrainGoldQuest { get; set; }
    public bool CancelTrainMonsterQuest { get; set; }

    public bool TrainMonsterEnabled { get; set; } = true;
    public string TrainMonsterMobNames { get; set; } = string.Empty;
    public int TrainMonsterMapId { get; set; } = -1;
    public int TrainMonsterZoneId { get; set; } = -1;

    public bool TrainGoldEnabled { get; set; } = true;
    public int TrainGoldMapId { get; set; } = 80;
    public bool TrainGoldRequireZone { get; set; }
    public int TrainGoldZoneId { get; set; } = -1;
    public int TrainGoldSuicideMapId { get; set; } = 44;
    public int TrainGoldSuicideZoneId { get; set; } = -1;
    public bool UseGoldSuicideMode { get; set; }

    public bool KillPlayerEnabled { get; set; } = true;
    public int KillPlayerMapId { get; set; } = -1;
    public int KillPlayerZoneId { get; set; } = -1;
    public bool KillPlayerOnlyListedTargets { get; set; }
    public string KillPlayerTargetNames { get; set; } = string.Empty;

    public bool AutoFusion { get; set; }
    public int TrainingArmorMode { get; set; }
    public bool UseTdltWhenDoingDailyQuest { get; set; } = true;
    public bool TdltForTrainMonster { get; set; }
    public bool TdltForKillPlayer { get; set; }
    public bool TdltForTrainGold { get; set; }

    public bool CancelUnsupportedQuest { get; set; } = true;
    public bool RetryWhenNoSignal { get; set; } = true;
    public int RetryDelayMs { get; set; } = 1700;
    public int ActionDelayMs { get; set; } = 1000;
}
