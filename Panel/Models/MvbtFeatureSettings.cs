namespace Panel.Models;

public class MvbtFeatureSettings : TrainFeatureSettings
{
    public int StartHour { get; set; } = 22;
    public int StartMin { get; set; } = 0;
    public int StopHour { get; set; } = 6;
    public int StopMin { get; set; } = 0;
    public int TargetCount { get; set; } = 99;
}
