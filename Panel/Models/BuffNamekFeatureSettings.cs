namespace Panel.Models
{
    public class BuffNamekFeatureSettings
    {
        public bool Enabled { get; set; } = false;
        public int MapId { get; set; } = -1;
        public bool RequireZone { get; set; } = false;
        public int ZoneId { get; set; } = 0;
        public bool RequirePosition { get; set; } = false;
        public int PosX { get; set; } = 0;
        public int PosY { get; set; } = 0;
        public int SkillId { get; set; } = 7;
        public int BuffTargetMode { get; set; } = 0;
        public int BuffCondition { get; set; } = 0;
        public int HpThreshold { get; set; } = 50;
        public int BuffRangeMode { get; set; } = 0;
        public string TargetNames { get; set; } = string.Empty;
    }
}
