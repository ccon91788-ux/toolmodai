using System;
using System.ComponentModel;
using System.Windows.Forms;
using Panel.Models;

namespace Panel
{
    public partial class PetControl : UserControl
    {
        public PetControl()
        {
            InitializeComponent();
            HookEvents();
        }

        public event EventHandler SettingsChanged;
        public event EventHandler RequestGetLocation;

        private void HookEvents()
        {
            chkEnableAutoPet.CheckedChanged += OnSettingsChanged;
            chkAutoPemWhenPetCall.CheckedChanged += OnSettingsChanged;
            chkAutoKOK.CheckedChanged += OnSettingsChanged;
            chkAutoTTNL.CheckedChanged += OnSettingsChanged;
            numTTNLPercent.ValueChanged += OnSettingsChanged;
            chkAutoHealing.CheckedChanged += OnSettingsChanged;
            chkAutoFocusPet.CheckedChanged += OnSettingsChanged;
            chkAutoStopAtPower.CheckedChanged += OnSettingsChanged;
            numTargetPower.ValueChanged += OnSettingsChanged;
            chkAutoJump.CheckedChanged += OnSettingsChanged;
            chkAutoUsePetBuff.CheckedChanged += OnSettingsChanged;

            chkAutoGobackMap.CheckedChanged += OnSettingsChanged;
            numTargetMapId.ValueChanged += OnSettingsChanged;
            chkAutoGobackZone.CheckedChanged += OnSettingsChanged;
            numTargetZoneId.ValueChanged += OnSettingsChanged;
            chkAutoGobackPosition.CheckedChanged += OnSettingsChanged;
            numTargetX.ValueChanged += OnSettingsChanged;
            numTargetY.ValueChanged += OnSettingsChanged;

            btnGetLocation.Click += (s, e) => RequestGetLocation?.Invoke(this, EventArgs.Empty);
        }

        private bool _isBinding = false;

        private void OnSettingsChanged(object sender, EventArgs e)
        {
            if (_isBinding) return;
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public PetFeatureSettings GetSettings()
        {
            return new PetFeatureSettings
            {
                EnableAutoPet = chkEnableAutoPet.Checked,
                AutoPemWhenPetCall = chkAutoPemWhenPetCall.Checked,
                AutoKOK = chkAutoKOK.Checked,
                AutoTTNL = chkAutoTTNL.Checked,
                TTNLPercent = (int)numTTNLPercent.Value,
                AutoHealing = chkAutoHealing.Checked,
                AutoFocusPet = chkAutoFocusPet.Checked,
                AutoStopAtPower = chkAutoStopAtPower.Checked,
                TargetPower = (long)numTargetPower.Value,

                AutoGobackMap = chkAutoGobackMap.Checked,
                TargetMapId = (int)numTargetMapId.Value,
                AutoGobackZone = chkAutoGobackZone.Checked,
                TargetZoneId = (int)numTargetZoneId.Value,
                AutoGobackPosition = chkAutoGobackPosition.Checked,
                TargetX = (int)numTargetX.Value,
                TargetY = (int)numTargetY.Value,

                AutoJump = chkAutoJump.Checked,
                AutoUsePetBuff = chkAutoUsePetBuff.Checked
            };
        }

        public void ApplySettings(PetFeatureSettings s)
        {
            if (s == null) return;
            _isBinding = true;
            try
            {
                chkEnableAutoPet.Checked = s.EnableAutoPet;
                chkAutoPemWhenPetCall.Checked = s.AutoPemWhenPetCall;
                chkAutoKOK.Checked = s.AutoKOK;
                chkAutoTTNL.Checked = s.AutoTTNL;
                numTTNLPercent.Value = s.TTNLPercent >= 1 && s.TTNLPercent <= 100 ? s.TTNLPercent : 15;
                chkAutoHealing.Checked = s.AutoHealing;
                chkAutoFocusPet.Checked = s.AutoFocusPet;
                chkAutoStopAtPower.Checked = s.AutoStopAtPower;
                numTargetPower.Value = Math.Clamp(s.TargetPower, numTargetPower.Minimum, numTargetPower.Maximum);

                chkAutoGobackMap.Checked = s.AutoGobackMap;
                numTargetMapId.Value = Math.Clamp(s.TargetMapId, numTargetMapId.Minimum, numTargetMapId.Maximum);
                chkAutoGobackZone.Checked = s.AutoGobackZone;
                numTargetZoneId.Value = Math.Clamp(s.TargetZoneId, numTargetZoneId.Minimum, numTargetZoneId.Maximum);
                chkAutoGobackPosition.Checked = s.AutoGobackPosition;
                numTargetX.Value = Math.Clamp(s.TargetX, numTargetX.Minimum, numTargetX.Maximum);
                numTargetY.Value = Math.Clamp(s.TargetY, numTargetY.Minimum, numTargetY.Maximum);

                chkAutoJump.Checked = s.AutoJump;
                chkAutoUsePetBuff.Checked = s.AutoUsePetBuff;
            }
            finally
            {
                _isBinding = false;
            }
        }
    }
}
