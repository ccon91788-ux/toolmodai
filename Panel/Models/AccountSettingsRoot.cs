namespace Panel.Models;

public class AccountSettingsRoot
{
    public int Version { get; set; } = 1;
    public GeneralSettings General { get; set; } = new();
    public TrainFeatureSettings Train { get; set; } = new();
    public AutoUpZinSettings AutoUpZin { get; set; } = new();
    public AutoUpZinTo700kSettings UpZin700k { get; set; } = new();
    public BossFeatureSettings Boss { get; set; } = new();
    public GobackFeatureSettings Goback { get; set; } = new();
    public ItemSettings Item { get; set; } = new();
    public SupportSettings Support { get; set; } = new();
    public MvbtFeatureSettings Mvbt { get; set; } = new() { TargetCount = 99 };
    public MvbtFeatureSettings Mhbt { get; set; } = new() { TargetCount = 10 };
    public KilisFeatureSettings Kilis { get; set; } = new();
    public BossVegetaCityFeatureSettings BossVegetaCity { get; set; } = new();
    public PetFeatureSettings Pet { get; set; } = new();
    public BuffNamekFeatureSettings BuffNamek { get; set; } = new();
    public DauThanSettings DauThan { get; set; } = new();
    public DailyMetrics Daily { get; set; } = new();
    public AttendanceFeatureSettings Attendance { get; set; } = new();
    public AutoPointFeatureSettings AutoPoint { get; set; } = new();
    public AutoAmuletSettings AutoAmulet { get; set; } = new();

    public ReducePowerFeatureSettings ReducePower { get; set; } = new();
    public ScheduleSettings Schedule { get; set; } = new();
    public DailyQuestFeatureSettings DailyQuest { get; set; } = new();
}
