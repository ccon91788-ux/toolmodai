using System;
using System.ComponentModel;
using System.Windows.Forms;
using Panel.Models;

namespace Panel
{
    public partial class ScheduleControl : UserControl
    {
        public ScheduleControl()
        {
            InitializeComponent();
            HookEvents();
        }

        public event EventHandler? SettingsChanged;

        private bool _isBinding = false;

        private void HookEvents()
        {
            chkScheduleEnable.CheckedChanged += OnSettingsChanged;
            dtpStartTime.ValueChanged += OnSettingsChanged;
            dtpEndTime.ValueChanged += OnSettingsChanged;
        }

        private void OnSettingsChanged(object? sender, EventArgs e)

        {
            if (_isBinding) return;
            SettingsChanged?.Invoke(this, EventArgs.Empty);
            UpdateUiState();
        }

        private void UpdateUiState()
        {
            dtpStartTime.Enabled = chkScheduleEnable.Checked;
            dtpEndTime.Enabled = chkScheduleEnable.Checked;
        }

        public ScheduleSettings GetSettings()
        {
            var s = new ScheduleSettings();
            s.IsScheduleEnabled = chkScheduleEnable.Checked;
            s.StartTime = dtpStartTime.Value.ToString("HH:mm:ss");
            s.EndTime = dtpEndTime.Value.ToString("HH:mm:ss");
            
            return s;
        }

        public void ApplySettings(ScheduleSettings s)
        {
            if (s == null) s = new ScheduleSettings();
            
            _isBinding = true;
            try
            {
                chkScheduleEnable.Checked = s.IsScheduleEnabled;
                
                if (DateTime.TryParse(s.StartTime, out var st)) dtpStartTime.Value = st;
                if (DateTime.TryParse(s.EndTime, out var et)) dtpEndTime.Value = et;
                
                UpdateUiState();
            }
            finally
            {
                _isBinding = false;
            }
        }
    }
}
