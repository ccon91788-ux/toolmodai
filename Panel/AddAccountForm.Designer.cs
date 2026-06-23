namespace Panel;

partial class AddAccountForm
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
        this.tabControlAdd = new System.Windows.Forms.TabControl();
        this.tabSingle = new System.Windows.Forms.TabPage();
        this.tabBulk = new System.Windows.Forms.TabPage();
        this.lblTypeAccount = new System.Windows.Forms.Label();
        this.nudTypeAccount = new System.Windows.Forms.NumericUpDown();
        this.lblBulkTypeAccount = new System.Windows.Forms.Label();
        this.nudBulkTypeAccount = new System.Windows.Forms.NumericUpDown();
        this.lblUsername = new System.Windows.Forms.Label();
        this.lblPassword = new System.Windows.Forms.Label();
        this.txtUsername = new System.Windows.Forms.TextBox();
        this.txtPassword = new System.Windows.Forms.TextBox();
        this.lblServer = new System.Windows.Forms.Label();
        this.cboServer = new System.Windows.Forms.ComboBox();
        this.btnSelectFile = new System.Windows.Forms.Button();
        this.lblSelectedFile = new System.Windows.Forms.Label();
        this.lblBulkInfo = new System.Windows.Forms.Label();
        this.lblDefaultServer = new System.Windows.Forms.Label();
        this.cboDefaultServer = new System.Windows.Forms.ComboBox();
        this.btnSave = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.tabControlAdd.SuspendLayout();
        this.tabSingle.SuspendLayout();
        this.tabBulk.SuspendLayout();
        this.SuspendLayout();
        
        // tabControlAdd
        this.tabControlAdd.Controls.Add(this.tabSingle);
        this.tabControlAdd.Controls.Add(this.tabBulk);
        this.tabControlAdd.Location = new System.Drawing.Point(10, 10);
        this.tabControlAdd.Size = new System.Drawing.Size(300, 230);
        
        // tabSingle
        this.tabSingle.Controls.Add(this.lblUsername);
        this.tabSingle.Controls.Add(this.txtUsername);
        this.tabSingle.Controls.Add(this.lblPassword);
        this.tabSingle.Controls.Add(this.txtPassword);
        this.tabSingle.Controls.Add(this.lblServer);
        this.tabSingle.Controls.Add(this.cboServer);
        this.tabSingle.Controls.Add(this.lblTypeAccount);
        this.tabSingle.Controls.Add(this.nudTypeAccount);
        this.tabSingle.Location = new System.Drawing.Point(4, 24);
        this.tabSingle.Size = new System.Drawing.Size(292, 202);
        this.tabSingle.Text = "Thêm Đơn";
        this.tabSingle.BackColor = System.Drawing.Color.White;
        
        // lblUsername
        this.lblUsername.AutoSize = true;
        this.lblUsername.Location = new System.Drawing.Point(20, 30);
        this.lblUsername.Text = "Tài khoản:";

        // txtUsername
        this.txtUsername.Location = new System.Drawing.Point(95, 27);
        this.txtUsername.Width = 150;

        // lblPassword
        this.lblPassword.AutoSize = true;
        this.lblPassword.Location = new System.Drawing.Point(20, 70);
        this.lblPassword.Text = "Mật khẩu:";

        // txtPassword
        this.txtPassword.Location = new System.Drawing.Point(95, 67);
        this.txtPassword.Width = 150;
        this.txtPassword.UseSystemPasswordChar = true;

        // lblServer
        this.lblServer.AutoSize = true;
        this.lblServer.Location = new System.Drawing.Point(20, 110);
        this.lblServer.Text = "Máy chủ:";

        // cboServer
        this.cboServer.Location = new System.Drawing.Point(95, 107);
        this.cboServer.Width = 150;
        this.cboServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

        // lblTypeAccount
        this.lblTypeAccount.AutoSize = true;
        this.lblTypeAccount.Location = new System.Drawing.Point(20, 150);
        this.lblTypeAccount.Text = "Loại TK:";

        // nudTypeAccount
        this.nudTypeAccount.Location = new System.Drawing.Point(95, 147);
        this.nudTypeAccount.Width = 60;
        this.nudTypeAccount.Maximum = 1000;

        // tabBulk
        this.tabBulk.Controls.Add(this.lblBulkInfo);
        this.tabBulk.Controls.Add(this.btnSelectFile);
        this.tabBulk.Controls.Add(this.lblSelectedFile);
        this.tabBulk.Controls.Add(this.lblDefaultServer);
        this.tabBulk.Controls.Add(this.cboDefaultServer);
        this.tabBulk.Controls.Add(this.lblBulkTypeAccount);
        this.tabBulk.Controls.Add(this.nudBulkTypeAccount);
        this.tabBulk.Location = new System.Drawing.Point(4, 24);
        this.tabBulk.Size = new System.Drawing.Size(292, 202);
        this.tabBulk.Text = "Thêm Lô";
        this.tabBulk.BackColor = System.Drawing.Color.White;

        // lblBulkInfo
        this.lblBulkInfo.AutoSize = true;
        this.lblBulkInfo.Location = new System.Drawing.Point(10, 10);
        this.lblBulkInfo.Text = "Chọn file danh sách (phân tách bởi dấu |)\nCú pháp: tk|mk|sv hoặc tk|mk";

        // btnSelectFile
        this.btnSelectFile.Location = new System.Drawing.Point(10, 48);
        this.btnSelectFile.Size = new System.Drawing.Size(120, 30);
        this.btnSelectFile.Text = "Chọn File (.txt)";
        this.btnSelectFile.Click += new System.EventHandler(this.BtnSelectFile_Click);

        // lblSelectedFile
        this.lblSelectedFile.AutoSize = true;
        this.lblSelectedFile.Location = new System.Drawing.Point(10, 85);
        this.lblSelectedFile.Text = "(Chưa chọn file) - 0 dòng";
        this.lblSelectedFile.ForeColor = System.Drawing.Color.DarkBlue;

        // lblDefaultServer
        this.lblDefaultServer.AutoSize = true;
        this.lblDefaultServer.Location = new System.Drawing.Point(10, 142);
        this.lblDefaultServer.Text = "Mặc định:";

        // cboDefaultServer
        this.cboDefaultServer.Location = new System.Drawing.Point(90, 139);
        this.cboDefaultServer.Width = 150;
        this.cboDefaultServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

        // lblBulkTypeAccount
        this.lblBulkTypeAccount.AutoSize = true;
        this.lblBulkTypeAccount.Location = new System.Drawing.Point(10, 172);
        this.lblBulkTypeAccount.Text = "Loại TK lô:";

        // nudBulkTypeAccount
        this.nudBulkTypeAccount.Location = new System.Drawing.Point(90, 169);
        this.nudBulkTypeAccount.Width = 60;
        this.nudBulkTypeAccount.Maximum = 1000;

        // btnSave
        this.btnSave.Location = new System.Drawing.Point(110, 250);
        this.btnSave.Size = new System.Drawing.Size(75, 30);
        this.btnSave.Text = "Thêm";
        this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);

        // btnCancel
        this.btnCancel.Location = new System.Drawing.Point(195, 250);
        this.btnCancel.Size = new System.Drawing.Size(75, 30);
        this.btnCancel.Text = "Hủy";
        this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);

        // AddAccountForm
        this.ClientSize = new System.Drawing.Size(320, 290);
        this.Controls.Add(this.tabControlAdd);
        this.Controls.Add(this.btnSave);
        this.Controls.Add(this.btnCancel);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Thêm Tài Khoản Mới";
        this.tabControlAdd.ResumeLayout(false);
        this.tabSingle.ResumeLayout(false);
        this.tabSingle.PerformLayout();
        this.tabBulk.ResumeLayout(false);
        this.tabBulk.PerformLayout();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.TabControl tabControlAdd;
    private System.Windows.Forms.TabPage tabSingle;
    private System.Windows.Forms.TabPage tabBulk;
    private System.Windows.Forms.Button btnSelectFile;
    private System.Windows.Forms.Label lblSelectedFile;
    private System.Windows.Forms.Label lblBulkInfo;
    private System.Windows.Forms.Label lblDefaultServer;
    private System.Windows.Forms.ComboBox cboDefaultServer;

    private System.Windows.Forms.Label lblUsername;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox txtUsername;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Label lblServer;
    private System.Windows.Forms.ComboBox cboServer;
    private System.Windows.Forms.Label lblTypeAccount;
    private System.Windows.Forms.NumericUpDown nudTypeAccount;
    private System.Windows.Forms.Label lblBulkTypeAccount;
    private System.Windows.Forms.NumericUpDown nudBulkTypeAccount;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Button btnCancel;
}

