namespace Panel;

partial class GlobalSettingsForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.grpCaptcha = new System.Windows.Forms.GroupBox();
        this.lblApiServer = new System.Windows.Forms.Label();
        this.cboApiServer = new System.Windows.Forms.ComboBox();
        this.lblApiKey = new System.Windows.Forms.Label();
        this.txtApiKey = new System.Windows.Forms.TextBox();


        this.grpWindow = new System.Windows.Forms.GroupBox();
        this.lblWidth = new System.Windows.Forms.Label();
        this.txtWidth = new System.Windows.Forms.TextBox();
        this.lblHeight = new System.Windows.Forms.Label();
        this.txtHeight = new System.Windows.Forms.TextBox();
        this.lblFPS = new System.Windows.Forms.Label();
        this.nudFPS = new System.Windows.Forms.NumericUpDown();

        this.grpAutoCleanRam = new System.Windows.Forms.GroupBox();
        this.chkAutoCleanRam = new System.Windows.Forms.CheckBox();
        this.lblCleanRamInterval = new System.Windows.Forms.Label();
        this.nudCleanRamInterval = new System.Windows.Forms.NumericUpDown();
        this.lblCleanRamUnit = new System.Windows.Forms.Label();

        this.btnSave = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();

        this.grpCaptcha.SuspendLayout();
        this.grpWindow.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudFPS)).BeginInit();
        this.grpAutoCleanRam.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudCleanRamInterval)).BeginInit();
        this.SuspendLayout();

        // ===================== grpCaptcha =====================
        this.grpCaptcha.Controls.Add(this.lblApiServer);
        this.grpCaptcha.Controls.Add(this.cboApiServer);
        this.grpCaptcha.Controls.Add(this.lblApiKey);
        this.grpCaptcha.Controls.Add(this.txtApiKey);
        this.grpCaptcha.Location = new System.Drawing.Point(8, 8);
        this.grpCaptcha.Size = new System.Drawing.Size(348, 100);
        this.grpCaptcha.Text = "Cấu hình Captcha";

        this.lblApiServer.AutoSize = true;
        this.lblApiServer.Location = new System.Drawing.Point(10, 30);
        this.lblApiServer.Text = "Server API:";

        this.cboApiServer.FormattingEnabled = true;
        this.cboApiServer.Items.AddRange(new object[] {
            "http://103.252.93.32/shoptool247/SolveCaptcha?token=",
            "http://api.tooltvt.com/api?token="});
        this.cboApiServer.Location = new System.Drawing.Point(100, 27);
        this.cboApiServer.Size = new System.Drawing.Size(232, 23);

        this.lblApiKey.AutoSize = true;
        this.lblApiKey.Location = new System.Drawing.Point(10, 60);
        this.lblApiKey.Text = "API Key:";

        this.txtApiKey.Location = new System.Drawing.Point(100, 57);
        this.txtApiKey.Size = new System.Drawing.Size(232, 23);


        // ===================== grpWindow =====================
        this.grpWindow.Controls.Add(this.lblWidth);
        this.grpWindow.Controls.Add(this.txtWidth);
        this.grpWindow.Controls.Add(this.lblHeight);
        this.grpWindow.Controls.Add(this.txtHeight);
        this.grpWindow.Controls.Add(this.lblFPS);
        this.grpWindow.Controls.Add(this.nudFPS);
        this.grpWindow.Location = new System.Drawing.Point(8, 116);
        this.grpWindow.Size = new System.Drawing.Size(348, 110);
        this.grpWindow.Text = "Kích thước cửa sổ game";

        this.lblWidth.AutoSize = true;
        this.lblWidth.Location = new System.Drawing.Point(10, 25);
        this.lblWidth.Text = "Chiều rộng:";

        this.txtWidth.Location = new System.Drawing.Point(100, 22);
        this.txtWidth.Size = new System.Drawing.Size(232, 23);

        this.lblHeight.AutoSize = true;
        this.lblHeight.Location = new System.Drawing.Point(10, 52);
        this.lblHeight.Text = "Chiều cao:";

        this.txtHeight.Location = new System.Drawing.Point(100, 49);
        this.txtHeight.Size = new System.Drawing.Size(232, 23);

        this.lblFPS.AutoSize = true;
        this.lblFPS.Location = new System.Drawing.Point(10, 79);
        this.lblFPS.Text = "FPS:";

        this.nudFPS.Location = new System.Drawing.Point(100, 76);
        this.nudFPS.Size = new System.Drawing.Size(232, 23);
        this.nudFPS.Minimum = 15;
        this.nudFPS.Maximum = 60;
        this.nudFPS.Value = 60;

        // ===================== grpAutoCleanRam =====================
        this.grpAutoCleanRam.Controls.Add(this.chkAutoCleanRam);
        this.grpAutoCleanRam.Controls.Add(this.lblCleanRamInterval);
        this.grpAutoCleanRam.Controls.Add(this.nudCleanRamInterval);
        this.grpAutoCleanRam.Controls.Add(this.lblCleanRamUnit);
        this.grpAutoCleanRam.Location = new System.Drawing.Point(8, 234);
        this.grpAutoCleanRam.Size = new System.Drawing.Size(348, 80);
        this.grpAutoCleanRam.Text = "Tự động dọn RAM";

        this.chkAutoCleanRam.AutoSize = true;
        this.chkAutoCleanRam.Location = new System.Drawing.Point(10, 25);
        this.chkAutoCleanRam.Text = "Bật Auto dọn RAM";

        this.lblCleanRamInterval.AutoSize = true;
        this.lblCleanRamInterval.Location = new System.Drawing.Point(10, 52);
        this.lblCleanRamInterval.Text = "Chu kỳ:";

        this.nudCleanRamInterval.Location = new System.Drawing.Point(65, 49);
        this.nudCleanRamInterval.Size = new System.Drawing.Size(70, 23);
        this.nudCleanRamInterval.Minimum = 1;
        this.nudCleanRamInterval.Maximum = 1440;
        this.nudCleanRamInterval.Value = 30;

        this.lblCleanRamUnit.AutoSize = true;
        this.lblCleanRamUnit.Location = new System.Drawing.Point(141, 52);
        this.lblCleanRamUnit.Text = "phút/lần";

        // ===================== btnSave / btnCancel =====================
        this.btnSave.Location = new System.Drawing.Point(208, 328);
        this.btnSave.Size = new System.Drawing.Size(80, 30);
        this.btnSave.Text = "Lưu";
        this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);

        this.btnCancel.Location = new System.Drawing.Point(296, 328);
        this.btnCancel.Size = new System.Drawing.Size(80, 30);
        this.btnCancel.Text = "Hủy";
        this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);

        // ===================== GlobalSettingsForm =====================
        this.ClientSize = new System.Drawing.Size(384, 370);
        this.Controls.Add(this.grpCaptcha);
        this.Controls.Add(this.grpWindow);
        this.Controls.Add(this.grpAutoCleanRam);
        this.Controls.Add(this.btnSave);
        this.Controls.Add(this.btnCancel);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Cài Đặt Chung";

        this.grpCaptcha.ResumeLayout(false);
        this.grpCaptcha.PerformLayout();
        this.grpWindow.ResumeLayout(false);
        this.grpWindow.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudFPS)).EndInit();
        this.grpAutoCleanRam.ResumeLayout(false);
        this.grpAutoCleanRam.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudCleanRamInterval)).EndInit();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.GroupBox grpCaptcha;
    private System.Windows.Forms.Label lblApiServer;
    private System.Windows.Forms.ComboBox cboApiServer;
    private System.Windows.Forms.Label lblApiKey;
    private System.Windows.Forms.TextBox txtApiKey;

    private System.Windows.Forms.GroupBox grpWindow;
    private System.Windows.Forms.Label lblWidth;
    private System.Windows.Forms.TextBox txtWidth;
    private System.Windows.Forms.Label lblHeight;
    private System.Windows.Forms.TextBox txtHeight;
    private System.Windows.Forms.Label lblFPS;
    private System.Windows.Forms.NumericUpDown nudFPS;

    private System.Windows.Forms.GroupBox grpAutoCleanRam;
    private System.Windows.Forms.CheckBox chkAutoCleanRam;
    private System.Windows.Forms.Label lblCleanRamInterval;
    private System.Windows.Forms.NumericUpDown nudCleanRamInterval;
    private System.Windows.Forms.Label lblCleanRamUnit;

    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Button btnCancel;
}
