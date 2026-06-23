namespace NRO_v247.Mods.Pet
{
    public class PetSettings
    {
        public bool EnableAutoPet { get; set; } = false;
        public bool AutoPemWhenPetCall { get; set; } = false;
        public bool AutoKOK { get; set; } = false;
        public bool AutoTTNL { get; set; } = false;
        public int TTNLPercent { get; set; } = 15;
        public bool AutoHealing { get; set; } = false;
        public bool AutoFocusPet { get; set; } = false;

        // --- Giữ vị trí (Goback) ---
        public bool AutoGobackMap { get; set; } = false;
        public int TargetMapId { get; set; } = -1;
        public bool AutoGobackZone { get; set; } = false;
        public int TargetZoneId { get; set; } = -1;
        public bool AutoGobackPosition { get; set; } = false;
        public int TargetX { get; set; } = -1;
        public int TargetY { get; set; } = -1;
        public bool AutoStopAtPower { get; set; } = false;
        public long TargetPower { get; set; } = 149000000;

        public bool AutoJump { get; set; } = false;
        public bool AutoUsePetBuff { get; set; } = false;
    }
}
