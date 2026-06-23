namespace Panel;

static class Program
{
    [STAThread]
    static void Main()
    {
        bool createNew;
        using (System.Threading.Mutex mutex = new System.Threading.Mutex(true, Panel.Helpers.StringShield.GetMutexName(), out createNew))
        {
            if (createNew)
            {
                Panel.Repositories.DatabaseHelper.InitializeDatabase();
                ApplicationConfiguration.Initialize();
                Panel.Helpers.AntiCrack.Initialize();
                
                using (var loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        Application.Run(new Form1(loginForm.CustomerName, loginForm.LicenseExpiresAt));

                        // ── Sau khi Panel đã đóng hoàn toàn ──────────────
                        // Nếu bị lockout (heartbeat fail) → hiện popup lý do
                        string? reason = Panel.Services.HeartbeatService.LastLockoutReason;
                        if (!string.IsNullOrEmpty(reason))
                        {
                            MessageBox.Show(
                                reason,
                                "⚠ ZFOX Panel - Đã Đóng",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Panel đang được mở rồi! Không thể mở thêm.", "ZFox Panel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
