using System;
using System.Windows.Forms;
using Panel.Models;

namespace Panel
{
    public partial class DauThanControl : UserControl
    {
        public event EventHandler? SettingsChanged;

        private bool _isDataBinding = false;
        private DauThanSettings _currentSettings = new DauThanSettings();

        public DauThanControl()
        {
            InitializeComponent();
            AttachEvents();
        }

        private void AttachEvents()
        {
            // Xin Đậu
            chkAutoRequest.CheckedChanged += Control_Changed;
            chkRequestCondition.CheckedChanged += Control_Changed;
            numRequestIfUnder.ValueChanged += Control_Changed;

            // Cho Đậu
            chkAutoDonate.CheckedChanged += Control_Changed;
            chkDonateFilter.CheckedChanged += Control_Changed;
            txtDonateNames.TextChanged += Control_Changed;

            // Dùng Đậu
            chkAutoBuffPet.CheckedChanged += Control_Changed;
            numPetHpUnder.ValueChanged += Control_Changed;
            numPetKiUnder.ValueChanged += Control_Changed;
            chkAutoBuffMaster.CheckedChanged += Control_Changed;
            numMasterHpUnder.ValueChanged += Control_Changed;
            numMasterKiUnder.ValueChanged += Control_Changed;
        }

        public void ApplySettings(DauThanSettings? settings)
        {
            if (settings == null) return;
            
            _isDataBinding = true;
            _currentSettings = settings;

            // Xin đậu
            chkAutoRequest.Checked = settings.AutoRequest;
            chkRequestCondition.Checked = settings.RequestCondition;
            numRequestIfUnder.Value = settings.RequestIfUnder;

            // Cho đậu
            chkAutoDonate.Checked = settings.AutoDonate;
            chkDonateFilter.Checked = settings.DonateFilter;
            txtDonateNames.Text = settings.DonateNames;

            // Búp đậu
            chkAutoBuffPet.Checked = settings.AutoBuffPet;
            numPetHpUnder.Value = settings.PetHpUnder;
            numPetKiUnder.Value = settings.PetKiUnder;
            
            chkAutoBuffMaster.Checked = settings.AutoBuffMaster;
            numMasterHpUnder.Value = settings.MasterHpUnder;
            numMasterKiUnder.Value = settings.MasterKiUnder;

            _isDataBinding = false;
        }

        private void Control_Changed(object? sender, EventArgs e)
        {
            if (_isDataBinding) return;

            // Xin đậu
            _currentSettings.AutoRequest = chkAutoRequest.Checked;
            _currentSettings.RequestCondition = chkRequestCondition.Checked;
            _currentSettings.RequestIfUnder = (int)numRequestIfUnder.Value;

            // Cho đậu
            _currentSettings.AutoDonate = chkAutoDonate.Checked;
            _currentSettings.DonateFilter = chkDonateFilter.Checked;
            _currentSettings.DonateNames = txtDonateNames.Text;

            // Búp đậu
            _currentSettings.AutoBuffPet = chkAutoBuffPet.Checked;
            _currentSettings.PetHpUnder = (int)numPetHpUnder.Value;
            _currentSettings.PetKiUnder = (int)numPetKiUnder.Value;

            _currentSettings.AutoBuffMaster = chkAutoBuffMaster.Checked;
            _currentSettings.MasterHpUnder = (int)numMasterHpUnder.Value;
            _currentSettings.MasterKiUnder = (int)numMasterKiUnder.Value;

            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
