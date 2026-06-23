using System;
using System.Windows.Forms;
using Panel.Models;

namespace Panel
{
    public partial class AutoBossControl : UserControl
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        public event EventHandler? SettingsChanged;

        private BossFeatureSettings _currentSettings = new BossFeatureSettings();

        /// <summary>Trả về object settings hiện tại sau khi người dùng thay đổi UI.</summary>
        public BossFeatureSettings CurrentSettings => _currentSettings;

        private ToolStripDropDown _skillDropdown;
        private ToolStripControlHost _skillHost;

        public AutoBossControl()
        {
            InitializeComponent();
            InitializeSkillDropdown();
            AttachEvents();

            SendMessage(txtZoneRanges.Handle, EM_SETCUEBANNER, 0, "Nhập khoảng (vd: 1-5,7-10)");
            SendMessage(txtMapRanges.Handle, EM_SETCUEBANNER, 0, "Nhập ID Map (vd: 20,25)");
            SendMessage(txtTimeSchedules.Handle, EM_SETCUEBANNER, 0, "vd: 12:30-13:30, 14:40-17:00");
        }

        private void InitializeSkillDropdown()
        {
            _skillHost = new ToolStripControlHost(pnlSkillPopup)
            {
                AutoSize = false,
                Size = pnlSkillPopup.Size,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            _skillDropdown = new ToolStripDropDown
            {
                AutoSize = true,
                Padding = Padding.Empty
            };
            _skillDropdown.Items.Add(_skillHost);
        }

        /// <summary>Khi bấm nút "Đồng bộ thông số" → Form1 xử lý copy params sang các acc cùng server + đã tích Liên kết.</summary>
        public event EventHandler? SyncParamsRequested;

        private void AttachEvents()
        {
            // Các event hook bị tạm vô hiệu hóa để code lại từ đầu
            // Nhưng thêm logic mutual exclusive cho UI theo yêu cầu
            chkAttackBoss.CheckedChanged += (s, e) =>
            {
                if (chkAttackBoss.Checked) chkTieBoss.Checked = false;
            };
            chkTieBoss.CheckedChanged += (s, e) =>
            {
                if (chkTieBoss.Checked) chkAttackBoss.Checked = false;
            };

            chkScoutContinuous.CheckedChanged += (s, e) =>
            {
                if (chkScoutContinuous.Checked) chkScoutVip.Checked = false;
            };
            chkScoutVip.CheckedChanged += (s, e) =>
            {
                if (chkScoutVip.Checked) chkScoutContinuous.Checked = false;
            };

            btnSyncParams.Click += (s, e) => SyncParamsRequested?.Invoke(this, EventArgs.Empty);

            chkEnableBoss.CheckedChanged += Control_Changed;
            chkEnableSync.CheckedChanged += Control_Changed;
            chkAttackBoss.CheckedChanged += Control_Changed;
            chkTieBoss.CheckedChanged += Control_Changed;
            chkScoutContinuous.CheckedChanged += Control_Changed;
            chkScoutVip.CheckedChanged += Control_Changed;
            chkLimitMap.CheckedChanged += Control_Changed;
            chkLimitZone.CheckedChanged += Control_Changed;
            chkEnableAntiBan.CheckedChanged += Control_Changed;
            chkAntiBanAttackMobs.CheckedChanged += Control_Changed;
            nudAntiBanAttackMobsSeconds.ValueChanged += Control_Changed;
            chkEnableSchedule.CheckedChanged += Control_Changed;
            txtTimeSchedules.TextChanged += Control_Changed;
            txtMapRanges.TextChanged += Control_Changed;
            txtZoneRanges.TextChanged += Control_Changed;
            txtBossNames.TextChanged += Control_Changed;
            txtAntiBanChatContents.TextChanged += Control_Changed;
            // Item checkboxes
            chkAutoTdlt.CheckedChanged  += Control_Changed;
            chkEatCn.CheckedChanged     += Control_Changed;
            chkEatBh.CheckedChanged     += Control_Changed;
            chkEatGx.CheckedChanged     += Control_Changed;
            chkEatAd.CheckedChanged     += Control_Changed;
            chkEatCo4La.CheckedChanged  += Control_Changed;
            chkEatFood.CheckedChanged   += Control_Changed;
            chkUnequipTrainingArmor.CheckedChanged += Control_Changed;

            txtDisguiseInfo.TextChanged += Control_Changed;
            txtVpdlId.TextChanged += Control_Changed;
            txtPetId.TextChanged += Control_Changed;

            btnShowSkillPopup.Click += (s, e) =>
            {
                _skillDropdown.Show(btnShowSkillPopup, new System.Drawing.Point(0, btnShowSkillPopup.Height));
            };

            // Gắn event cho các CheckBox Skill
            chkEarthDragon.CheckedChanged += Control_Changed;
            chkEarthKamejoko.CheckedChanged += Control_Changed;
            chkEarthTdhs.CheckedChanged += Control_Changed;
            chkEarthSleep.CheckedChanged += Control_Changed;
            chkEarthTeleport.CheckedChanged += Control_Changed;
            chkEarthShield.CheckedChanged += Control_Changed;
            chkEarthKaioken.CheckedChanged += Control_Changed;

            chkNamekLienHoan.CheckedChanged += Control_Changed;
            chkNamekDemon.CheckedChanged += Control_Changed;
            chkNamekMakankosappo.CheckedChanged += Control_Changed;
            chkNamekDeTrung.CheckedChanged += Control_Changed;
            chkNamekShield.CheckedChanged += Control_Changed;

            chkXaydaGalick.CheckedChanged += Control_Changed;
            chkXaydaAtomic.CheckedChanged += Control_Changed;
            chkXaydaMonkey.CheckedChanged += Control_Changed;
            chkXaydaHeal.CheckedChanged += Control_Changed;
            chkXaydaShield.CheckedChanged += Control_Changed;

            chkUseShieldUnderHp.CheckedChanged += Control_Changed;
            nudShieldHpPercent.ValueChanged += Control_Changed;

            chkLimitHpAbove.CheckedChanged += Control_Changed;
            nudHpAbove.ValueChanged += Control_Changed;
            chkLimitHpBelow.CheckedChanged += Control_Changed;
            nudHpBelow.ValueChanged += Control_Changed;

            chkEnableFinishingMove.CheckedChanged += Control_Changed;
            nudFinishingMoveHp.ValueChanged += Control_Changed;
        }

        private bool _isApplying = false;

        public void ApplySettings(BossFeatureSettings? settings)
        {
            if (settings == null) return;
            _currentSettings = settings;

            _isApplying = true;
            chkEnableBoss.Checked = settings.Enabled;
            chkEnableSync.Checked = settings.EnableSyncCoordinator;
            chkAttackBoss.Checked = settings.GoAttackBoss;
            chkTieBoss.Checked = settings.GoTieBoss;
            chkScoutContinuous.Checked = settings.AutoScoutContinuous;
            chkScoutVip.Checked = settings.ScoutOnVipChat;
            chkLimitMap.Checked = settings.LimitMap;
            chkLimitZone.Checked = settings.LimitZone;
            txtMapRanges.Text = settings.MapRanges ?? "";
            txtZoneRanges.Text = settings.ZoneRanges ?? "";
            txtBossNames.Text = settings.BossNames ?? "";
            chkEnableSchedule.Checked = settings.EnableTimeSchedule;
            txtTimeSchedules.Text = settings.TimeSchedules ?? "";
            chkEnableAntiBan.Checked = settings.EnableAntiBan;
            chkAntiBanAttackMobs.Checked = settings.AntiBanAttackMobs;
            nudAntiBanAttackMobsSeconds.Value = Math.Max(nudAntiBanAttackMobsSeconds.Minimum, Math.Min(nudAntiBanAttackMobsSeconds.Maximum, settings.AntiBanAttackMobsSeconds));
            txtAntiBanChatContents.Text = settings.AntiBanChatContents ?? "";
            // Item checkboxes
            chkAutoTdlt.Checked  = settings.AutoTdlt;
            chkEatCn.Checked     = settings.EatCuongNo;
            chkEatBh.Checked     = settings.EatBoHuyet;
            chkEatGx.Checked     = settings.EatGiapXen;
            chkEatAd.Checked     = settings.EatAnDanh;
            chkEatCo4La.Checked  = settings.EatCo4La;
            chkEatFood.Checked   = settings.EatThucAn;
            chkUnequipTrainingArmor.Checked = settings.UnequipTrainingArmor;

            txtDisguiseInfo.Text = settings.BossCtId >= 0 ? settings.BossCtId.ToString() : "";
            txtVpdlId.Text = settings.BossVpdlId >= 0 ? settings.BossVpdlId.ToString() : "";
            txtPetId.Text = settings.BossPetId >= 0 ? settings.BossPetId.ToString() : "";

            // Load cấu hình Skill
            chkEarthDragon.Checked = settings.SkillEarthDragon;
            chkEarthKamejoko.Checked = settings.SkillEarthKame;
            chkEarthTdhs.Checked = settings.SkillEarthTdhs;
            chkEarthSleep.Checked = settings.SkillEarthThoiMien;
            chkEarthTeleport.Checked = settings.SkillEarthDctt;
            chkEarthShield.Checked = settings.SkillEarthKhien;
            chkEarthKaioken.Checked = settings.SkillEarthKaioken;

            chkNamekLienHoan.Checked = settings.SkillNamekLienHoan;
            chkNamekDemon.Checked = settings.SkillNamekDemon;
            chkNamekMakankosappo.Checked = settings.SkillNamekMakan;
            chkNamekDeTrung.Checked = settings.SkillNamekDeTrung;
            chkNamekShield.Checked = settings.SkillNamekKhien;

            chkXaydaGalick.Checked = settings.SkillSaiyanGalick;
            chkXaydaAtomic.Checked = settings.SkillSaiyanAntomic;
            chkXaydaMonkey.Checked = settings.SkillSaiyanBienHinh;
            chkXaydaHeal.Checked = settings.SkillSaiyanTtNl;
            chkXaydaShield.Checked = settings.SkillSaiyanKhien;

            chkUseShieldUnderHp.Checked = settings.UseShieldUnderHp;
            nudShieldHpPercent.Value = Math.Max(nudShieldHpPercent.Minimum, Math.Min(nudShieldHpPercent.Maximum, settings.ShieldHpPercent));

            chkLimitHpAbove.Checked = settings.LimitHpAbove;
            nudHpAbove.Value = Math.Max(nudHpAbove.Minimum, Math.Min(nudHpAbove.Maximum, (decimal)settings.HpAboveValue));
            chkLimitHpBelow.Checked = settings.LimitHpBelow;
            nudHpBelow.Value = Math.Max(nudHpBelow.Minimum, Math.Min(nudHpBelow.Maximum, (decimal)settings.HpBelowValue));

            chkEnableFinishingMove.Checked = settings.EnableFinishingMove;
            nudFinishingMoveHp.Value = Math.Max(nudFinishingMoveHp.Minimum, Math.Min(nudFinishingMoveHp.Maximum, (decimal)settings.FinishingMoveHpValue));

            _isApplying = false;
        }

        private void Control_Changed(object? sender, EventArgs e)
        {
            if (_isApplying) return;

            _currentSettings.Enabled = chkEnableBoss.Checked;
            _currentSettings.EnableSyncCoordinator = chkEnableSync.Checked;
            _currentSettings.GoAttackBoss = chkAttackBoss.Checked;
            _currentSettings.GoTieBoss = chkTieBoss.Checked;
            _currentSettings.AutoScoutContinuous = chkScoutContinuous.Checked;
            _currentSettings.ScoutOnVipChat = chkScoutVip.Checked;
            _currentSettings.LimitMap = chkLimitMap.Checked;
            _currentSettings.LimitZone = chkLimitZone.Checked;
            _currentSettings.MapRanges = txtMapRanges.Text;
            _currentSettings.ZoneRanges = txtZoneRanges.Text;
            _currentSettings.BossNames = txtBossNames.Text;
            _currentSettings.EnableTimeSchedule = chkEnableSchedule.Checked;
            _currentSettings.TimeSchedules = txtTimeSchedules.Text;
            _currentSettings.EnableAntiBan = chkEnableAntiBan.Checked;
            _currentSettings.AntiBanAttackMobs = chkAntiBanAttackMobs.Checked;
            _currentSettings.AntiBanAttackMobsSeconds = (int)nudAntiBanAttackMobsSeconds.Value;
            _currentSettings.AntiBanChatContents = txtAntiBanChatContents.Text;
            // Item checkboxes
            _currentSettings.AutoTdlt    = chkAutoTdlt.Checked;
            _currentSettings.EatCuongNo  = chkEatCn.Checked;
            _currentSettings.EatBoHuyet  = chkEatBh.Checked;
            _currentSettings.EatGiapXen  = chkEatGx.Checked;
            _currentSettings.EatAnDanh   = chkEatAd.Checked;
            _currentSettings.EatCo4La    = chkEatCo4La.Checked;
            _currentSettings.EatThucAn   = chkEatFood.Checked;
            _currentSettings.UnequipTrainingArmor = chkUnequipTrainingArmor.Checked;

            _currentSettings.BossCtId = int.TryParse(txtDisguiseInfo.Text.Trim(), out int ct) ? ct : -1;
            _currentSettings.BossVpdlId = int.TryParse(txtVpdlId.Text.Trim(), out int vpdl) ? vpdl : -1;
            _currentSettings.BossPetId = int.TryParse(txtPetId.Text.Trim(), out int pet) ? pet : -1;

            // Save cấu hình Skill
            _currentSettings.SkillEarthDragon = chkEarthDragon.Checked;
            _currentSettings.SkillEarthKame = chkEarthKamejoko.Checked;
            _currentSettings.SkillEarthTdhs = chkEarthTdhs.Checked;
            _currentSettings.SkillEarthThoiMien = chkEarthSleep.Checked;
            _currentSettings.SkillEarthDctt = chkEarthTeleport.Checked;
            _currentSettings.SkillEarthKhien = chkEarthShield.Checked;
            _currentSettings.SkillEarthKaioken = chkEarthKaioken.Checked;

            _currentSettings.SkillNamekLienHoan = chkNamekLienHoan.Checked;
            _currentSettings.SkillNamekDemon = chkNamekDemon.Checked;
            _currentSettings.SkillNamekMakan = chkNamekMakankosappo.Checked;
            _currentSettings.SkillNamekDeTrung = chkNamekDeTrung.Checked;
            _currentSettings.SkillNamekKhien = chkNamekShield.Checked;

            _currentSettings.SkillSaiyanGalick = chkXaydaGalick.Checked;
            _currentSettings.SkillSaiyanAntomic = chkXaydaAtomic.Checked;
            _currentSettings.SkillSaiyanBienHinh = chkXaydaMonkey.Checked;
            _currentSettings.SkillSaiyanTtNl = chkXaydaHeal.Checked;
            _currentSettings.SkillSaiyanKhien = chkXaydaShield.Checked;

            _currentSettings.UseShieldUnderHp = chkUseShieldUnderHp.Checked;
            _currentSettings.ShieldHpPercent = (int)nudShieldHpPercent.Value;

            _currentSettings.LimitHpAbove = chkLimitHpAbove.Checked;
            _currentSettings.HpAboveValue = (long)nudHpAbove.Value;
            _currentSettings.LimitHpBelow = chkLimitHpBelow.Checked;
            _currentSettings.HpBelowValue = (long)nudHpBelow.Value;

            _currentSettings.EnableFinishingMove = chkEnableFinishingMove.Checked;
            _currentSettings.FinishingMoveHpValue = (long)nudFinishingMoveHp.Value;

            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
