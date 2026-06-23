using System;
using System.ComponentModel;
using System.Windows.Forms;
using Panel.Models;

namespace Panel
{
    public partial class KilisControl : UserControl
    {
        public KilisControl()
        {
            InitializeComponent();
            
            if (cboAmuletType.Items.Count > 0)
                cboAmuletType.SelectedIndex = 0; // Default: 10 phút

            if (cboTrainingArmorMode.Items.Count > 0)
                cboTrainingArmorMode.SelectedIndex = 0; // "Không chạy"

            HookEvents();
        }

        public event EventHandler SettingsChanged;

        private void HookEvents()
        {
            chkMasterEnable.CheckedChanged += OnSettingsChanged;
            numZone.ValueChanged += OnSettingsChanged;
            numStartHour.ValueChanged += OnSettingsChanged;
            numStartMin.ValueChanged += OnSettingsChanged;
            numEndHour.ValueChanged += OnSettingsChanged;
            numEndMin.ValueChanged += OnSettingsChanged;
            chkAutoBuyAmulet.CheckedChanged += OnSettingsChanged;
            cboAmuletType.SelectedIndexChanged += OnSettingsChanged;
            chkUseTDLT.CheckedChanged += OnSettingsChanged;
            chkAutoZone.CheckedChanged += OnSettingsChanged;
            cboTrainingArmorMode.SelectedIndexChanged += OnSettingsChanged;
        }

        private void OnSettingsChanged(object sender, EventArgs e)
        {
            if (_isBinding) return;
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool _isBinding = false;

        public KilisFeatureSettings GetSettings()
        {
            var s = new KilisFeatureSettings();
            s.Enabled = chkMasterEnable.Checked;
            s.ZoneId = (int)numZone.Value;
            s.StartHour = (int)numStartHour.Value;
            s.StartMin = (int)numStartMin.Value;
            s.StopHour = (int)numEndHour.Value;
            s.StopMin = (int)numEndMin.Value;
            s.AutoBuyAmulet = chkAutoBuyAmulet.Checked;
            s.AmuletType = cboAmuletType.SelectedIndex;
            s.UseTDLT = chkUseTDLT.Checked;
            s.AutoZone = chkAutoZone.Checked;
            s.TrainingArmorMode = cboTrainingArmorMode.SelectedIndex < 0 ? 0 : cboTrainingArmorMode.SelectedIndex;
            
            return s;
        }

        public void ApplySettings(KilisFeatureSettings s)
        {
            _isBinding = true;
            try
            {
                chkMasterEnable.Checked = s.Enabled;
                numZone.Value = s.ZoneId >= 0 && s.ZoneId <= 22 ? s.ZoneId : 5;
                numStartHour.Value = s.StartHour >= 0 && s.StartHour <= 23 ? s.StartHour : 0;
                numStartMin.Value = s.StartMin >= 0 && s.StartMin <= 59 ? s.StartMin : 0;
                numEndHour.Value = s.StopHour >= 0 && s.StopHour <= 23 ? s.StopHour : 0;
                numEndMin.Value = s.StopMin >= 0 && s.StopMin <= 59 ? s.StopMin : 0;
                chkAutoBuyAmulet.Checked = s.AutoBuyAmulet;
                if (s.AmuletType >= 0 && s.AmuletType < cboAmuletType.Items.Count)
                    cboAmuletType.SelectedIndex = s.AmuletType;
                else
                    cboAmuletType.SelectedIndex = 0;

                chkUseTDLT.Checked = s.UseTDLT;
                chkAutoZone.Checked = s.AutoZone;

                if (s.TrainingArmorMode >= 0 && s.TrainingArmorMode < cboTrainingArmorMode.Items.Count)
                    cboTrainingArmorMode.SelectedIndex = s.TrainingArmorMode;
                else
                    cboTrainingArmorMode.SelectedIndex = 0;
            }
            finally
            {
                _isBinding = false;
            }
        }
    }
}
