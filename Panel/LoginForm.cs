using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Panel.Services;

namespace Panel;

public partial class LoginForm : Form
{
    // Thông tin khách hàng sau khi login/startup-check thành công
    public string CustomerName    { get; private set; } = "";
    public string LicenseExpiresAt { get; private set; } = "";

    // Kéo form Borderless
    public const int WM_NCLBUTTONDOWN = 0xA1;
    public const int HT_CAPTION       = 0x2;

    [DllImportAttribute("user32.dll")]
    public static extern int  SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    [DllImportAttribute("user32.dll")]
    public static extern bool ReleaseCapture();

    public LoginForm()
    {
        InitializeComponent();
        this.MouseDown      += LoginForm_MouseDown;
        this.lblTitle.MouseDown += LoginForm_MouseDown;
        this.lblInfo.MouseDown  += LoginForm_MouseDown;
    }

    private void LoginForm_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
    }

    private void BtnClose_Click(object sender, EventArgs e) => Application.Exit();

    // ── Khi form load: thử startup-check với saved key ────────────────────
    private async void LoginForm_Load(object sender, EventArgs e)
    {
        lblStatus.Text = "Đang kiểm tra phiên làm việc...";

        string savedKey = SecureDataStorage.LoadKey();
        if (!string.IsNullOrEmpty(savedKey))
        {
            var result = await LicenseAuthService.StartupCheckAsync(savedKey);
            if (result.Success)
            {
                // Máy khớp, key còn hạn → vào thẳng Form1
                CustomerName     = result.CustomerName;
                LicenseExpiresAt = result.LicenseExpiresAt;
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }
            else
            {
                // Key fail (IP đổi, hết hạn...) → hiển thị lý do, KHÔNG xóa key file
                // Fill sẵn key vào textbox để user thấy và bấm Login lại được ngay
                txtLicenseKey.Text = savedKey;
                lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
                lblStatus.Text = result.Message.Length > 0
                    ? result.Message
                    : "Phiên không hợp lệ. Vui lòng nhập Key.";
            }
        }
        else
        {
            lblStatus.Text = "";
        }
    }

    // ── Nhấn Login ────────────────────────────────────────────────────────
    private async void BtnLogin_Click(object sender, EventArgs e)
    {
        string key = txtLicenseKey.Text.Trim();
        if (string.IsNullOrEmpty(key))
        {
            lblStatus.Text = "Vui lòng nhập License Key!";
            return;
        }

        lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
        lblStatus.Text      = "Đang kết nối Server xác thực...";
        btnLogin.Enabled    = false;
        txtLicenseKey.Enabled = false;

        var result = await LicenseAuthService.LoginAsync(key);

        if (result.Success && !string.IsNullOrEmpty(result.Token))
        {
            // Lưu license_key (DPAPI) để dùng cho startup-check lần sau
            SecureDataStorage.SaveKey(key);
            // Lưu JWT in-memory cho heartbeat
            SecureDataStorage.SaveToken(result.Token);

            CustomerName      = result.CustomerName     ?? "";
            LicenseExpiresAt  = result.LicenseExpiresAt ?? "";
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        else if (result.Message == "LỖI_SSL_WIN_CŨ")
        {
            var mbr = MessageBox.Show(
                "VPS (Windows Server cũ) không hỗ trợ SSL bảo mật của API mới.\n\n" +
                "Bạn có muốn Tool TỰ ĐỘNG cấu hình nâng cấp TLS 1.2 cho VPS này không?\n" +
                "(Cần quyền Admin. Chọn YES máy sẽ tự cài đặt và Khởi Động Lại!)",
                "Hỗ Trợ Tự Động Vá Lỗi VPS Cũ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (mbr == DialogResult.Yes)
            {
                try
                {
                    string ps1 = @"
Write-Host 'Dang mo khoa TLS 1.2 cho VPS cu...'
$tls12Path = 'HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2'
If (!(Test-Path $tls12Path)) { New-Item -Path $tls12Path -Force | Out-Null }
If (!(Test-Path ""$tls12Path\Client"")) { New-Item -Path ""$tls12Path\Client"" -Force | Out-Null }
Set-ItemProperty -Path ""$tls12Path\Client"" -Name ""DisabledByDefault"" -Value 0 -Type DWord -Force
Set-ItemProperty -Path ""$tls12Path\Client"" -Name ""Enabled"" -Value 1 -Type DWord -Force
Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\.NetFramework\v4.0.30319' -Name 'SchUseStrongCrypto' -Value 1 -Type DWord -Force
Set-ItemProperty -Path 'HKLM:\SOFTWARE\WOW6432Node\Microsoft\.NetFramework\v4.0.30319' -Name 'SchUseStrongCrypto' -Value 1 -Type DWord -Force
Restart-Computer -Force
";
                    System.IO.File.WriteAllText("Fix_VPS_SSL.ps1", ps1);
                    var psi = new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName        = "powershell.exe",
                        Arguments       = "-ExecutionPolicy Bypass -File Fix_VPS_SSL.ps1",
                        Verb            = "runas",
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                    Application.Exit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể tự sửa lỗi. Chi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
            lblStatus.Text      = "VPS cũ lỗi SSL. Hãy bấm Login lại và chọn YES để fix tự động.";
            btnLogin.Enabled    = true;
            txtLicenseKey.Enabled = true;
        }
        else
        {
            lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
            lblStatus.Text      = result.Message ?? "Đăng nhập thất bại!";
            btnLogin.Enabled    = true;
            txtLicenseKey.Enabled = true;
        }
    }
}
