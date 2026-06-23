using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Panel.Models;

namespace Panel
{
    public partial class BuffNamekControl : UserControl
    {
        public event EventHandler? SettingsChanged;
        public event EventHandler? RequestGetPosition;
        public bool IsReducePowerTabSelected => tabControlMain.SelectedTab == tabReducePower;

        private bool _binding;

        public BuffNamekControl()
        {
            InitializeComponent();
            AttachEvents();
            UpdateModeUi();
            lblStatus.Visible = false;
        }

        private void AttachEvents()
        {
            chkEnabled.CheckedChanged += OnChanged;
            cboMapId.SelectedIndexChanged += OnChanged;
            cboMapId.TextChanged += OnChanged;
            chkRequireZone.CheckedChanged += OnChanged;
            nudZoneId.ValueChanged += OnChanged;
            chkRequirePosition.CheckedChanged += OnChanged;
            nudPosX.ValueChanged += OnChanged;
            nudPosY.ValueChanged += OnChanged;

            cboBuffTargetMode.SelectedIndexChanged += CboBuffTargetMode_Changed;
            cboBuffCondition.SelectedIndexChanged += CboBuffCondition_Changed;
            nudHpThreshold.ValueChanged += OnChanged;
            cboBuffRangeMode.SelectedIndexChanged += CboBuffRangeMode_Changed;
            txtTargetNames.TextChanged += OnChanged;

            btnGetPos.Click += (s, e) => RequestGetPosition?.Invoke(this, EventArgs.Empty);

            chkRpEnabled.CheckedChanged += OnChanged;
            chkRpAutoPunchBlackFlag.CheckedChanged += OnChanged;
            chkRpUseHpPunch.CheckedChanged += OnChanged;
            nudRpHpPunch.ValueChanged += OnChanged;
            chkRpUseTdlt.CheckedChanged += OnChanged;
            cboRpMapId.SelectedIndexChanged += OnChanged;
            cboRpMapId.TextChanged += OnChanged;
            nudRpZoneId.ValueChanged += OnChanged;
            nudRpX.ValueChanged += OnChanged;
            nudRpY.ValueChanged += OnChanged;
            nudRpProvokeCount.ValueChanged += OnChanged;
            nudRpDeadDelayMs.ValueChanged += OnChanged;
            btnRpGetPos.Click += (s, e) => RequestGetPosition?.Invoke(this, EventArgs.Empty);
        }

        private void CboBuffTargetMode_Changed(object? s, EventArgs e)
        {
            UpdateModeUi();
            OnChanged(s, e);
        }

        private void CboBuffCondition_Changed(object? s, EventArgs e)
        {
            UpdateConditionUi();
            UpdateRangeWarning();
            OnChanged(s, e);
        }

        private void CboBuffRangeMode_Changed(object? s, EventArgs e)
        {
            UpdateRangeWarning();
            OnChanged(s, e);
        }

        private void UpdateModeUi()
        {
            int selectedMode = cboBuffTargetMode.SelectedIndex;
            bool isTargetMode = selectedMode == 1;
            grpTargetSettings.Visible = isTargetMode;
            grpTargetSettings.Enabled = isTargetMode;

            if (selectedMode == 2)
            {
                lblBuffTargetNote.Text = "Part 2: đứng im, tự buff bản thân khi có lệnh cứu hộ";
            }
            else
            {
                lblBuffTargetNote.Text = isTargetMode
                    ? "Part 2: buff theo danh sach ten (so khop cNameClear)"
                    : "Part 2: buff ban than theo CD";
            }

            UpdateConditionUi();
            UpdateRangeWarning();
        }

        private void UpdateConditionUi()
        {
            pnlHpThreshold.Visible = cboBuffCondition.SelectedIndex == 2;
        }

        private void UpdateRangeWarning()
        {
            bool isSpam = cboBuffCondition.SelectedIndex == 0;
            bool isNear = cboBuffRangeMode.SelectedIndex == 1;

            if (isSpam && isNear)
            {
                lblRangeWarning.Text = "Hoi CD + Tele toi+ve co the loop nhanh";
                lblRangeWarning.Visible = true;
            }
            else
            {
                lblRangeWarning.Text = string.Empty;
                lblRangeWarning.Visible = false;
            }
        }

        public BuffNamekFeatureSettings GetSettings()
        {
            return new BuffNamekFeatureSettings
            {
                Enabled = chkEnabled.Checked,
                MapId = ResolveSelectedMapId(cboMapId),
                RequireZone = chkRequireZone.Checked,
                ZoneId = (int)nudZoneId.Value,
                RequirePosition = chkRequirePosition.Checked,
                PosX = (int)nudPosX.Value,
                PosY = (int)nudPosY.Value,
                SkillId = 7,
                BuffTargetMode = Clamp(cboBuffTargetMode.SelectedIndex, 0, 2),
                BuffCondition = Clamp(cboBuffCondition.SelectedIndex, 0, 2),
                HpThreshold = Clamp((int)nudHpThreshold.Value, 1, 99),
                BuffRangeMode = Clamp(cboBuffRangeMode.SelectedIndex, 0, 1),
                TargetNames = NormalizeTargetNames(txtTargetNames.Text)
            };
        }

        public void ApplySettings(BuffNamekFeatureSettings? settings)
        {
            settings ??= new BuffNamekFeatureSettings();

            _binding = true;
            try
            {
                chkEnabled.Checked = settings.Enabled;
                SelectMap(cboMapId, settings.MapId);
                chkRequireZone.Checked = settings.RequireZone;
                nudZoneId.Value = Math.Clamp(settings.ZoneId, nudZoneId.Minimum, nudZoneId.Maximum);
                chkRequirePosition.Checked = settings.RequirePosition;
                nudPosX.Value = Math.Clamp(settings.PosX, nudPosX.Minimum, nudPosX.Maximum);
                nudPosY.Value = Math.Clamp(settings.PosY, nudPosY.Minimum, nudPosY.Maximum);

                cboBuffTargetMode.SelectedIndex = Clamp(settings.BuffTargetMode, 0, 2);
                cboBuffCondition.SelectedIndex = Clamp(settings.BuffCondition, 0, 2);
                nudHpThreshold.Value = Math.Clamp(settings.HpThreshold <= 0 ? 50 : settings.HpThreshold, nudHpThreshold.Minimum, nudHpThreshold.Maximum);
                cboBuffRangeMode.SelectedIndex = Clamp(settings.BuffRangeMode, 0, 1);
                txtTargetNames.Text = settings.TargetNames ?? string.Empty;
            }
            finally
            {
                _binding = false;
            }

            UpdateModeUi();
        }

        public ReducePowerFeatureSettings GetReducePowerSettings()
        {
            return new ReducePowerFeatureSettings
            {
                Enabled = chkRpEnabled.Checked,
                MapId = ResolveSelectedMapId(cboRpMapId),
                ZoneId = (int)nudRpZoneId.Value,
                PosX = (int)nudRpX.Value,
                PosY = (int)nudRpY.Value,
                ProvokeMobCount = (int)nudRpProvokeCount.Value,
                DeadReportDelayMs = (int)nudRpDeadDelayMs.Value,
                AutoPunchBlackFlag = chkRpAutoPunchBlackFlag.Checked,
                UseHpPunch = chkRpUseHpPunch.Checked,
                PunchHpPercent = (int)nudRpHpPunch.Value,
                UseTdlt = chkRpUseTdlt.Checked
            };
        }

        public void ApplyReducePowerSettings(ReducePowerFeatureSettings? settings)
        {
            settings ??= new ReducePowerFeatureSettings();

            _binding = true;
            try
            {
                chkRpEnabled.Checked = settings.Enabled;
                SelectMap(cboRpMapId, settings.MapId);
                nudRpZoneId.Value = Math.Clamp(settings.ZoneId, nudRpZoneId.Minimum, nudRpZoneId.Maximum);
                nudRpX.Value = Math.Clamp(settings.PosX, nudRpX.Minimum, nudRpX.Maximum);
                nudRpY.Value = Math.Clamp(settings.PosY, nudRpY.Minimum, nudRpY.Maximum);
                nudRpProvokeCount.Value = Math.Clamp(settings.ProvokeMobCount < 0 ? 0 : settings.ProvokeMobCount, nudRpProvokeCount.Minimum, nudRpProvokeCount.Maximum);
                nudRpDeadDelayMs.Value = Math.Clamp(settings.DeadReportDelayMs < 0 ? 0 : settings.DeadReportDelayMs, nudRpDeadDelayMs.Minimum, nudRpDeadDelayMs.Maximum);
                chkRpAutoPunchBlackFlag.Checked = settings.AutoPunchBlackFlag;
                chkRpUseHpPunch.Checked = settings.UseHpPunch;
                nudRpHpPunch.Value = Math.Clamp(settings.PunchHpPercent <= 0 ? 10 : settings.PunchHpPercent, nudRpHpPunch.Minimum, nudRpHpPunch.Maximum);
                chkRpUseTdlt.Checked = settings.UseTdlt;
            }
            finally
            {
                _binding = false;
            }
        }

        public void SetPosition(int x, int y)
        {
            _binding = true;
            try
            {
                nudPosX.Value = Math.Clamp(x, (int)nudPosX.Minimum, (int)nudPosX.Maximum);
                nudPosY.Value = Math.Clamp(y, (int)nudPosY.Minimum, (int)nudPosY.Maximum);
                chkRequirePosition.Checked = true;
            }
            finally
            {
                _binding = false;
            }

            OnChanged(this, EventArgs.Empty);
        }

        public void SetRpPosition(int x, int y)
        {
            _binding = true;
            try
            {
                nudRpX.Value = Math.Clamp(x, (int)nudRpX.Minimum, (int)nudRpX.Maximum);
                nudRpY.Value = Math.Clamp(y, (int)nudRpY.Minimum, nudRpY.Maximum);
            }
            finally
            {
                _binding = false;
            }

            OnChanged(this, EventArgs.Empty);
        }

        public void SetRpMapFromPos(int mapId, int zoneId)
        {
            _binding = true;
            try
            {
                SelectMap(cboRpMapId, mapId);
                nudRpZoneId.Value = Math.Clamp(zoneId, nudRpZoneId.Minimum, nudRpZoneId.Maximum);
            }
            finally
            {
                _binding = false;
            }

            OnChanged(this, EventArgs.Empty);
        }

        public void SetMapFromPos(int mapId, int zoneId)
        {
            _binding = true;
            try
            {
                SelectMap(cboMapId, mapId);
                nudZoneId.Value = Math.Clamp(zoneId, nudZoneId.Minimum, nudZoneId.Maximum);
                chkRequireZone.Checked = true;
            }
            finally
            {
                _binding = false;
            }

            OnChanged(this, EventArgs.Empty);
        }

        public void LoadMapItems(System.Collections.IEnumerable maps)
        {
            int selectedMapId = ResolveSelectedMapId(cboMapId);
            int selectedRpMapId = ResolveSelectedMapId(cboRpMapId);

            _binding = true;
            try
            {
                cboMapId.Items.Clear();
                cboRpMapId.Items.Clear();
                foreach (var m in maps)
                {
                    cboMapId.Items.Add(m);
                    cboRpMapId.Items.Add(m);
                }

                if (cboMapId.Items.Count > 0)
                {
                    SelectMap(cboMapId, selectedMapId);
                    if (cboMapId.SelectedIndex < 0)
                    {
                        cboMapId.SelectedIndex = 0;
                    }

                    SelectMap(cboRpMapId, selectedRpMapId);
                    if (cboRpMapId.SelectedIndex < 0)
                    {
                        cboRpMapId.SelectedIndex = 0;
                    }
                }
            }
            finally
            {
                _binding = false;
            }
        }

        public void SetStatus(string text, bool isOk = true)
        {
            // Intentionally hidden in current UX.
        }

        private int ResolveSelectedMapId(ComboBox combo = null)
        {
            var target = combo ?? cboMapId;
            if (target.SelectedItem is MapTemplate selected)
            {
                return selected.Id;
            }

            string text = target.Text?.Trim() ?? string.Empty;
            if (int.TryParse(text, out int mapId))
            {
                return mapId;
            }

            if (!string.IsNullOrEmpty(text))
            {
                int idxEnd = text.IndexOf(']');
                if (text.StartsWith("[") && idxEnd > 1)
                {
                    string candidate = text.Substring(1, idxEnd - 1);
                    if (int.TryParse(candidate, out mapId))
                    {
                        return mapId;
                    }
                }
            }

            return -1;
        }

        private void SelectMap(ComboBox combo, int mapId)
        {
            if (mapId < 0)
            {
                if (combo.Items.Count > 0)
                {
                    combo.SelectedIndex = 0;
                }
                return;
            }

            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is MapTemplate m && m.Id == mapId)
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }

            combo.Text = mapId.ToString();
        }

        private static string NormalizeTargetNames(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            var lines = raw
                .Replace("\r", "")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var uniq = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<string>();
            foreach (string line in lines)
            {
                string name = line.Trim();
                if (name.Length == 0) continue;
                if (uniq.Add(name)) result.Add(name);
            }

            return string.Join("\n", result);
        }

        private void OnChanged(object? sender, EventArgs e)
        {
            if (_binding) return;
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private static int Clamp(int val, int min, int max)
        {
            if (val < min) return min;
            return val > max ? max : val;
        }
    }
}
