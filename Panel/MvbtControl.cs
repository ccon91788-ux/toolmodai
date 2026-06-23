using System;
using System.ComponentModel;
using System.Windows.Forms;
using Panel.Models;

namespace Panel
{
    public partial class MvbtControl : UserControl
    {
        public MvbtControl()
        {
            InitializeComponent();
            
            // Default select index 0 for ComboBoxes
            if (cboMobTargetType.Items.Count > 0)
                cboMobTargetType.SelectedIndex = 0; // "Đánh tất cả"
                
                cboTrainingArmorMode.SelectedIndex = 0; // "Không chạy"

            HookEvents();
        }

        public event EventHandler SettingsChanged;
        public event EventHandler ResetCountRequested;

        public void UpdateProgress(int farmedCount, int targetCount)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)(() => UpdateProgress(farmedCount, targetCount)));
                return;
            }
            lblCurrentCount.Text = $"Đã nhặt: {farmedCount} / {targetCount}";
        }

        private void BtnResetCount_Click(object sender, EventArgs e)
        {
            ResetCountRequested?.Invoke(this, EventArgs.Empty);
        }

        private void HookEvents()
        {
            // Subscribe to all changes
            chkMasterEnable.CheckedChanged += OnSettingsChanged;
            numStartHour.ValueChanged += OnSettingsChanged;
            numStartMin.ValueChanged += OnSettingsChanged;
            numEndHour.ValueChanged += OnSettingsChanged;
            numEndMin.ValueChanged += OnSettingsChanged;
            
            cboMap.SelectedIndexChanged += OnSettingsChanged;
            chkZoneRequire.CheckedChanged += OnSettingsChanged;
            numZone.ValueChanged += OnSettingsChanged;
            chkUseTDLT.CheckedChanged += OnSettingsChanged;
            chkAvoidSuperMob.CheckedChanged += OnSettingsChanged;
            chkAutoZone.CheckedChanged += OnSettingsChanged;
            cboMobTargetType.SelectedIndexChanged += OnSettingsChanged;
            cboTrainingArmorMode.SelectedIndexChanged += OnSettingsChanged;
            txtMobIds.TextChanged += OnSettingsChanged;
            numTargetCount.ValueChanged += OnSettingsChanged;
        }

        private void OnSettingsChanged(object sender, EventArgs e)
        {
            if (_isBinding) return;
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool _isBinding = false;

        public MvbtFeatureSettings GetSettings()
        {
            var s = new MvbtFeatureSettings();
            s.Enabled = chkMasterEnable.Checked;
            s.StartHour = (int)numStartHour.Value;
            s.StartMin = (int)numStartMin.Value;
            s.StopHour = (int)numEndHour.Value;
            s.StopMin = (int)numEndMin.Value;
            
            if (cboMap.SelectedItem is MapTemplate m) s.MapId = m.Id;
            s.RequireZone = chkZoneRequire.Checked;
            s.ZoneId = (int)numZone.Value;
            s.UseTDLT = chkUseTDLT.Checked;
            s.AvoidSuperMob = chkAvoidSuperMob.Checked;
            s.MobTargetType = cboMobTargetType.SelectedIndex;
            s.TrainingArmorMode = cboTrainingArmorMode.SelectedIndex;
            s.MobIds = txtMobIds.Text;
            s.TargetCount = (int)numTargetCount.Value;
            
            return s;
        }

        public void ApplySettings(MvbtFeatureSettings s, System.Collections.IEnumerable allMaps)
        {
            _isBinding = true;
            try
            {
                // Sync maps combo box if needed
                if (cboMap.Items.Count == 0 && allMaps != null)
                {
                    foreach (var m in allMaps) cboMap.Items.Add(m);
                }

                chkMasterEnable.Checked = s.Enabled;
                numStartHour.Value = s.StartHour >= 0 && s.StartHour <= 23 ? s.StartHour : 0;
                numStartMin.Value = s.StartMin >= 0 && s.StartMin <= 59 ? s.StartMin : 0;
                numEndHour.Value = s.StopHour >= 0 && s.StopHour <= 23 ? s.StopHour : 0;
                numEndMin.Value = s.StopMin >= 0 && s.StopMin <= 59 ? s.StopMin : 0;
                
                // Select map
                for (int i = 0; i < cboMap.Items.Count; i++)
                {
                    if (cboMap.Items[i] is MapTemplate m && m.Id == s.MapId)
                    {
                        cboMap.SelectedIndex = i;
                        break;
                    }
                }

                chkZoneRequire.Checked = s.RequireZone;
                numZone.Value = s.ZoneId >= -1 && s.ZoneId <= 22 ? s.ZoneId : -1;
                chkUseTDLT.Checked = s.UseTDLT;
                chkAvoidSuperMob.Checked = s.AvoidSuperMob;
                
                if (s.MobTargetType >= 0 && s.MobTargetType < cboMobTargetType.Items.Count)
                    cboMobTargetType.SelectedIndex = s.MobTargetType;
                
                if (s.TrainingArmorMode >= 0 && s.TrainingArmorMode < cboTrainingArmorMode.Items.Count)
                    cboTrainingArmorMode.SelectedIndex = s.TrainingArmorMode;
                else
                    cboTrainingArmorMode.SelectedIndex = 0;

                txtMobIds.Text = s.MobIds ?? "";
                numTargetCount.Value = s.TargetCount >= 1 && s.TargetCount <= 9999 ? s.TargetCount : 99;
            }
            finally
            {
                _isBinding = false;
            }
        }

        [Category("Appearance")]
        [Description("Thiết lập tiêu đề cho nhóm Cấu hình vị trí theo tab")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Title
        {
            get => grpLocation.Text;
            set => grpLocation.Text = value;
        }

        [Category("Appearance")]
        [Description("Thiết lập tên cho nút Auto theo tab")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string MasterCheckboxText
        {
            get => chkMasterEnable.Text;
            set => chkMasterEnable.Text = value;
        }
    }
}
