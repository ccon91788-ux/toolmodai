namespace Panel.Models;

public class BossFeatureSettings
{
    public bool Enabled { get; set; }
    
    // Hành động chính
    public bool GoAttackBoss { get; set; }
    public bool GoTieBoss { get; set; }
    
    // Chức năng dò boss
    public bool AutoScoutContinuous { get; set; }
    public bool ScoutOnVipChat { get; set; }
    
    // Phụ trợ ăn item
    public bool EatCuongNo { get; set; }
    public bool EatBoHuyet { get; set; }
    public bool EatGiapXen { get; set; }
    public bool EatAnDanh { get; set; }
    public bool EatThucAn { get; set; }
    public bool AutoTdlt { get; set; }
    public bool EatCo4La { get; set; }
    
    public int BossCtId { get; set; } = -1;
    public int BossVpdlId { get; set; } = -1;
    public int BossPetId { get; set; } = -1;

    public bool UnequipTrainingArmor { get; set; }
    
    // Mục tiêu & Phạm vi săn
    public bool LimitMap { get; set; }
    public string MapRanges { get; set; } = string.Empty;

    public bool LimitZone { get; set; }
    public string ZoneRanges { get; set; } = string.Empty;

    public string BossNames { get; set; } = string.Empty;
    public bool EnableSyncCoordinator { get; set; }

    // Cài đặt Nâng cao
    public bool EnableTimeSchedule { get; set; }
    public string TimeSchedules { get; set; } = string.Empty;

    public bool LimitHpAbove { get; set; }
    public long HpAboveValue { get; set; }

    public bool LimitHpBelow { get; set; }
    public long HpBelowValue { get; set; }

    public bool EnableFinishingMove { get; set; }
    public long FinishingMoveHpValue { get; set; }

    // Anti-ban
    public bool EnableAntiBan { get; set; }
    public bool AntiBanAttackMobs { get; set; }
    public int AntiBanAttackMobsSeconds { get; set; }
    public bool AntiBanChat { get; set; }
    public string AntiBanChatContents { get; set; } = string.Empty;

    // ──────────────────────────────────────────────────────────────────
    // Hệ thống tùy chọn Skill (Mới)
    // ──────────────────────────────────────────────────────────────────
    
    // Skills Trái Đất (Dragon=0, Kame=1, TDHS=6, ThoiMien=22, DCTT=20, Khien=19, Kaioken=9)
    public bool SkillEarthDragon { get; set; } = true;
    public bool SkillEarthKame { get; set; } = false;
    public bool SkillEarthTdhs { get; set; } = false;
    public bool SkillEarthThoiMien { get; set; } = false;
    public bool SkillEarthDctt { get; set; } = true;
    public bool SkillEarthKhien { get; set; } = true;
    public bool SkillEarthKaioken { get; set; } = false;

    // Skills Namếc (LienHoan=17, Demon=2, Makan=11, DeTrung=12, Khien=19)
    public bool SkillNamekLienHoan { get; set; } = true;
    public bool SkillNamekDemon { get; set; } = false;
    public bool SkillNamekMakan { get; set; } = false;
    public bool SkillNamekDeTrung { get; set; } = true;
    public bool SkillNamekKhien { get; set; } = true;

    // Skills Xayda (Galick=4, Antomic=5, BienHinh=13, TtNl=8, Khien=19)
    public bool SkillSaiyanGalick { get; set; } = true;
    public bool SkillSaiyanAntomic { get; set; } = false;
    public bool SkillSaiyanBienHinh { get; set; } = true;
    public bool SkillSaiyanTtNl { get; set; } = false;
    public bool SkillSaiyanKhien { get; set; } = true;

    // Khiên năng lượng dưới % HP
    public bool UseShieldUnderHp { get; set; } = false;
    public int ShieldHpPercent { get; set; } = 30;
}
