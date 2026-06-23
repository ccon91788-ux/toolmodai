namespace Panel.Models;

public class AutoUpZinTo700kSettings
{
    public bool Enabled { get; set; }
    public string NamePrefix { get; set; } = string.Empty;
    public int TargetClass { get; set; } = -1;
}