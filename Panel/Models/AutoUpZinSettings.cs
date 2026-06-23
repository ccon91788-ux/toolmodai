namespace Panel.Models;

public class AutoUpZinSettings
{
    public bool Enabled { get; set; }
    public string NamePrefix { get; set; } = string.Empty;
    public int TargetClass { get; set; } = -1;
}
