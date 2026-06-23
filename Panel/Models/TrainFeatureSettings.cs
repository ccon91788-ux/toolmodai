namespace Panel.Models;

public class TrainFeatureSettings
{
    public bool Enabled { get; set; }
    public int MapId { get; set; } = -1;
    public bool RequireZone { get; set; }
    public int ZoneId { get; set; } = -1;
    public bool UseTDLT { get; set; }
    public bool OnlyUsePunch { get; set; }
    public bool AvoidSuperMob { get; set; } = true;
    public bool ChangeLowPlayerZoneIfNoMob { get; set; }
    public bool CheckLagMob { get; set; } = true;
    public int MobTargetType { get; set; }
    public string MobIds { get; set; } = "";

    // Cài đặt A*
    public int AstarStepSize { get; set; } = 3;
    public int AstarDelay { get; set; } = 50;

    // Giáp luyện tập: 0=Không chạy, 1=Mặc luyện tập, 2=Tháo luyện tập
    public int TrainingArmorMode { get; set; } = 0;

    // Chỉ đánh theo HP
    public bool AttackHpAbove { get; set; }
    public int AttackHpAboveValue { get; set; }
    public bool AttackHpBelow { get; set; }
    public int AttackHpBelowValue { get; set; }

    // Đổi khu khi đánh quái
    public bool RotateZone { get; set; }
    public string RotateZoneList { get; set; } = "";

    // Tự mua thỏi vàng
    public bool AutoBuyThoiVang { get; set; }
    public long BuyThoiVangMinGold { get; set; } = 1_000_000_000;

    // Đóng băng CD và Mana skill đấm (cd=0, mana=0 để đánh nhanh hơn)
    public bool FreezePunchSkillCd { get; set; }

    // Sử dụng vé riêng tư (item id 1825) để train trong map riêng
    public bool UsePrivateTicket { get; set; }

    // Tối ưu KS vàng (First-hit)
    public bool OptimizeKsVang { get; set; } = true;

    // KS Vàng Auto Zone
    public int KsVangAutoZoneMode { get; set; } = 0; // 0=Ít nhất, 1=Đông nhất
    public int KsVangAutoZoneTrigger { get; set; } = 0; // 0=Hết quái, 1=Thời gian
    public int KsVangAutoZoneTimeMin { get; set; } = 5;
    public bool KsVangFilterPlayer { get; set; } = false;
    public int KsVangPlayerMin { get; set; } = 3;
    public int KsVangPlayerMax { get; set; } = 5;
    public bool KsVangAvoidChars { get; set; } = false;
    public string KsVangAvoidCharsList { get; set; } = "";

    // ── Danh sách skill sử dụng khi train ────────────────────────────────
    // Trái Đất
    public bool SkillEarthDragon { get; set; } = true;
    public bool SkillEarthKame { get; set; }
    public bool SkillEarthTdhs { get; set; }
    public bool SkillEarthThoiMien { get; set; }
    public bool SkillEarthDctt { get; set; }
    public bool SkillEarthKhien { get; set; }
    public bool SkillEarthKaioken { get; set; }

    // Namếc
    public bool SkillNamekLienHoan { get; set; }
    public bool SkillNamekDemon { get; set; } = true;
    public bool SkillNamekMakan { get; set; }
    public bool SkillNamekDeTrung { get; set; }
    public bool SkillNamekKhien { get; set; }

    // Xayda
    public bool SkillSaiyanGalick { get; set; } = true;
    public bool SkillSaiyanAntomic { get; set; }
    public bool SkillSaiyanBienHinh { get; set; }
    public bool SkillSaiyanTtNl { get; set; }
    public bool SkillSaiyanKhien { get; set; }
    
    // Khiên năng lượng dưới % HP
    public bool UseShieldUnderHp { get; set; } = false;
    public int ShieldHpPercent { get; set; } = 30;
}
