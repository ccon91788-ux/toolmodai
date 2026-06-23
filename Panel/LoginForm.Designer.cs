namespace Panel;

partial class LoginForm
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
        this.txtLicenseKey = new System.Windows.Forms.TextBox();
        this.btnLogin = new System.Windows.Forms.Button();
        this.lblTitle = new System.Windows.Forms.Label();
        this.lblInfo = new System.Windows.Forms.Label();
        this.lblStatus = new System.Windows.Forms.Label();
        this.btnClose = new System.Windows.Forms.Button();
        this.SuspendLayout();
        
        // txtLicenseKey
        this.txtLicenseKey.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.txtLicenseKey.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtLicenseKey.Location = new System.Drawing.Point(35, 120);
        this.txtLicenseKey.Name = "txtLicenseKey";
        this.txtLicenseKey.PlaceholderText = "Nhập License Key...";
        this.txtLicenseKey.Size = new System.Drawing.Size(320, 20);
        this.txtLicenseKey.TabIndex = 0;
        this.txtLicenseKey.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        
        // btnLogin
        this.btnLogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(102)))), ((int)(((byte)(0)))));
        this.btnLogin.FlatAppearance.BorderSize = 0;
        this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnLogin.ForeColor = System.Drawing.Color.White;
        this.btnLogin.Location = new System.Drawing.Point(35, 175);
        this.btnLogin.Name = "btnLogin";
        this.btnLogin.Size = new System.Drawing.Size(320, 42);
        this.btnLogin.TabIndex = 1;
        this.btnLogin.Text = "Xác Thực Bản Quyền";
        this.btnLogin.UseVisualStyleBackColor = false;
        this.btnLogin.Click += new System.EventHandler(this.BtnLogin_Click);
        
        // lblTitle
        this.lblTitle.AutoSize = true;
        this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
        this.lblTitle.Location = new System.Drawing.Point(100, 30);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(183, 32);
        this.lblTitle.TabIndex = 2;
        this.lblTitle.Text = "ZFOX V2.4.7 PRO";
        
        // lblInfo
        this.lblInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
        this.lblInfo.Location = new System.Drawing.Point(35, 75);
        this.lblInfo.Name = "lblInfo";
        this.lblInfo.Size = new System.Drawing.Size(320, 35);
        this.lblInfo.TabIndex = 3;
        this.lblInfo.Text = "Vui lòng nhập mã Key của bạn hoặc dán vào đây để đăng nhập vào Panel điều khiển.";
        this.lblInfo.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        
        // lblStatus
        this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
        this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
        this.lblStatus.Location = new System.Drawing.Point(35, 148);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new System.Drawing.Size(320, 20);
        this.lblStatus.TabIndex = 4;
        this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        
        // btnClose
        this.btnClose.BackColor = System.Drawing.Color.Transparent;
        this.btnClose.FlatAppearance.BorderSize = 0;
        this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
        this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
        this.btnClose.Location = new System.Drawing.Point(357, 5);
        this.btnClose.Name = "btnClose";
        this.btnClose.Size = new System.Drawing.Size(30, 30);
        this.btnClose.TabIndex = 5;
        this.btnClose.Text = "X";
        this.btnClose.UseVisualStyleBackColor = false;
        this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
        
        // LoginForm
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
        this.ClientSize = new System.Drawing.Size(390, 250);
        this.Controls.Add(this.btnClose);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.lblInfo);
        this.Controls.Add(this.lblTitle);
        this.Controls.Add(this.btnLogin);
        this.Controls.Add(this.txtLicenseKey);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "LoginForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Login";
        this.Load += new System.EventHandler(this.LoginForm_Load);
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.TextBox txtLicenseKey;
    private System.Windows.Forms.Button btnLogin;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblInfo;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.Button btnClose;
}
