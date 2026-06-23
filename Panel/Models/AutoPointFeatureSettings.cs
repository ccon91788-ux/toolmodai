namespace Panel.Models;

public class AutoPointFeatureSettings
{
    public bool AddHP { get; set; } = false;
    public int TargetHP { get; set; } = 1000;
    public bool AddMP { get; set; } = false;
    public int TargetMP { get; set; } = 500;
    public bool AddDamage { get; set; } = false;
    public int TargetDamage { get; set; } = 500;
}
