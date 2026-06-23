using System.Collections.Generic;

namespace NRO_v247.Mods
{
    internal sealed class TrainRuntimeSettings
    {
        public bool Enabled { get; private set; }
        public int MapId { get; private set; } = -1;
        public bool RequireZone { get; private set; }
        public int ZoneId { get; private set; } = -1;
        public bool UseTDLT { get; private set; }
        public bool OnlyUsePunch { get; private set; }
        public bool UseKaiokenLienHoan { get; private set; }
        public bool AvoidSuperMob { get; private set; }
        public int MobTargetType { get; private set; }
        public bool ChangeLowPlayerZoneIfNoMob { get; private set; }
        public string MobIdsRaw { get; private set; } = string.Empty;

        // Các kĩ năng Trái Đất
        public bool SkillEarthDragon { get; private set; }
        public bool SkillEarthKame { get; private set; }
        public bool SkillEarthTdhs { get; private set; }
        public bool SkillEarthThoiMien { get; private set; }
        public bool SkillEarthDctt { get; private set; }
        public bool SkillEarthKhien { get; private set; }
        public bool SkillEarthKaioken { get; private set; }

        // Các kĩ năng Namếc
        public bool SkillNamekLienHoan { get; private set; }
        public bool SkillNamekDemon { get; private set; }
        public bool SkillNamekMakan { get; private set; }
        public bool SkillNamekDeTrung { get; private set; }
        public bool SkillNamekKhien { get; private set; }

        // Các kĩ năng Xayda
        public bool SkillSaiyanGalick { get; private set; }
        public bool SkillSaiyanAntomic { get; private set; }
        public bool SkillSaiyanBienHinh { get; private set; }
        public bool SkillSaiyanTtNl { get; private set; }
        public bool SkillSaiyanKhien { get; private set; }
        public bool UseShieldUnderHp { get; private set; }
        public int ShieldHpPercent { get; private set; }

        private readonly HashSet<int> _mobIds = new HashSet<int>();

        public void Apply(
            int mapId,
            bool requireZone,
            int zoneId,
            bool useTdlt,
            bool onlyUsePunch,
            bool avoidSuperMob,
            int mobTargetType,
            bool changeLowPlayerZoneIfNoMob,
            string mobIdsRaw,
            bool[] skills,
            bool useShield = false,
            int shieldHp = 30)
        {
            Enabled = true;
            MapId = mapId;
            RequireZone = requireZone;
            ZoneId = zoneId;
            UseTDLT = useTdlt;
            OnlyUsePunch = onlyUsePunch;
            AvoidSuperMob = avoidSuperMob;
            MobTargetType = mobTargetType;
            ChangeLowPlayerZoneIfNoMob = changeLowPlayerZoneIfNoMob;
            MobIdsRaw = mobIdsRaw ?? string.Empty;
            UseShieldUnderHp = useShield;
            ShieldHpPercent = shieldHp;

            if (skills != null && skills.Length >= 17)
            {
                SkillEarthDragon = skills[0];
                SkillEarthKame = skills[1];
                SkillEarthTdhs = skills[2];
                SkillEarthThoiMien = skills[3];
                SkillEarthDctt = skills[4];
                SkillEarthKhien = skills[5];
                SkillEarthKaioken = skills[6];
                
                SkillNamekLienHoan = skills[7];
                SkillNamekDemon = skills[8];
                SkillNamekMakan = skills[9];
                SkillNamekDeTrung = skills[10];
                SkillNamekKhien = skills[11];
                
                SkillSaiyanGalick = skills[12];
                SkillSaiyanAntomic = skills[13];
                SkillSaiyanBienHinh = skills[14];
                SkillSaiyanTtNl = skills[15];
                SkillSaiyanKhien = skills[16];
            }

            ParseMobIds(MobIdsRaw);
        }

        public void Disable()
        {
            Enabled = false;
            UseTDLT = false;
        }

        public bool IsMobAllowed(Mob mob)
        {
            if (mob == null) return false;
            if (mob.isMobMe || mob.hp <= 0 || mob.status == 0 || mob.status == 1) return false;
            if (AvoidSuperMob && (mob.isBoss || mob.levelBoss > 0)) return false;

            // 0: danh tat ca, 1: theo id quai (template id)
            if (MobTargetType == 1)
            {
                return _mobIds.Contains(mob.templateId) || _mobIds.Contains(mob.mobId);
            }

            return true;
        }

        private void ParseMobIds(string raw)
        {
            _mobIds.Clear();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return;
            }

            string[] parts = raw.Split(',', ';', '|');
            for (int i = 0; i < parts.Length; i++)
            {
                string token = parts[i]?.Trim();
                if (string.IsNullOrEmpty(token))
                {
                    continue;
                }

                if (int.TryParse(token, out int mobId))
                {
                    _mobIds.Add(mobId);
                }
            }
        }
    }
}
