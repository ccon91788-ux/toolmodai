using System;
using System.Windows.Forms;
using Panel.Models;
using Panel.Services;

namespace Panel;

public partial class GlobalSettingsForm : Form
{
    /// <summary>Raised khi người dùng lưu cấu hình Auto dọn RAM, để Form1 cập nhật timer ngay.</summary>
    public event EventHandler? AutoCleanRamSettingsChanged;
    public GlobalSettingsForm()
    {
        InitializeComponent();
        Panel.Helpers.UIThemeHelper.ApplyFlatTheme(this);
        LoadSettings();
        
        cboApiServer.SelectedIndexChanged += CboApiServer_SelectedIndexChanged;
    }

    private void CboApiServer_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cboApiServer.Text.Contains("103.252.93.32"))
        {
            txtApiKey.Text = "HDJEUTDYSJSIUEJ483455";
        }
    }

    private void LoadSettings()
    {
        var config = ConfigManager.Load();

        // Captcha
        cboApiServer.Text = config.CaptchaApiServer;
        if (cboApiServer.SelectedIndex == -1)
        {
            cboApiServer.Items.Add(config.CaptchaApiServer);
            cboApiServer.SelectedIndex = cboApiServer.Items.Count - 1;
        }
        txtApiKey.Text = config.CaptchaApiKey;

        // Window Size
        txtWidth.Text = config.WindowWidth.ToString();
        txtHeight.Text = config.WindowHeight.ToString();
        nudFPS.Value = Math.Max(15, Math.Min(60, config.TargetFPS));

        // Auto Clean RAM
        chkAutoCleanRam.Checked = config.AutoCleanRam;
        nudCleanRamInterval.Value = Math.Max(1, Math.Min(config.AutoCleanRamIntervalMinutes, 1440));
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        if (!int.TryParse(txtWidth.Text, out int width) || width < 250 || width > 1068)
        {
            MessageBox.Show("Chiều rộng cửa sổ phải từ 250 đến 1068.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!int.TryParse(txtHeight.Text, out int height) || height < 250 || height > 600)
        {
            MessageBox.Show("Chiều cao cửa sổ phải từ 250 đến 600.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var config = ConfigManager.Load();
        config.CaptchaApiServer = cboApiServer.Text;
        config.CaptchaApiKey = txtApiKey.Text;

        config.WindowWidth = width;
        config.WindowHeight = height;
        config.TargetFPS = (int)nudFPS.Value;

        // Auto Clean RAM
        config.AutoCleanRam = chkAutoCleanRam.Checked;
        config.AutoCleanRamIntervalMinutes = (int)nudCleanRamInterval.Value;

        ConfigManager.Save(config);

        AutoCleanRamSettingsChanged?.Invoke(this, EventArgs.Empty);

        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }
}
