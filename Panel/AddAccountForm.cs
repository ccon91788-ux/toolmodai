namespace Panel;

public partial class AddAccountForm : Form
{
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string Username { get; set; } = string.Empty;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string Password { get; set; } = string.Empty;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string ServerName { get; set; } = string.Empty;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int TypeAccountVal { get; set; } = 0;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public List<(string Username, string Password, string Server, int TypeAccount)> AddedAccounts { get; private set; } = new();

    private string[] _bulkLines = Array.Empty<string>();

    public AddAccountForm()
    {
        InitializeComponent();
        Panel.Helpers.UIThemeHelper.ApplyFlatTheme(this);
        foreach (var server in Panel.Models.ServerInfo.All)
        {
            cboServer.Items.Add(server.DisplayName);
            cboDefaultServer.Items.Add(server.DisplayName);
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        txtUsername.Text = Username;
        txtPassword.Text = Password;

        if (!string.IsNullOrEmpty(ServerName) && cboServer.Items.Contains(ServerName))
            cboServer.SelectedItem = ServerName;
        else if (cboServer.Items.Count > 0)
            cboServer.SelectedIndex = 0;

        nudTypeAccount.Value = TypeAccountVal;

        if (cboDefaultServer.Items.Count > 0)
            cboDefaultServer.SelectedIndex = 0;

        if (this.Text.Contains("Sửa"))
        {
            btnSave.Text = "Lưu";
            tabControlAdd.TabPages.Remove(tabBulk);
        }
        else
        {
            btnSave.Text = "Thêm";
        }
    }

    private void BtnSelectFile_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog();
        ofd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
        ofd.Title = "Chọn file danh sách tài khoản";

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var lines = System.IO.File.ReadAllLines(ofd.FileName, System.Text.Encoding.UTF8);
                var validLines = new List<string>();
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        validLines.Add(line);
                }

                _bulkLines = validLines.ToArray();
                lblSelectedFile.Text = $"{System.IO.Path.GetFileName(ofd.FileName)} - {_bulkLines.Length} dòng";
                if (_bulkLines.Length > 200)
                {
                    lblSelectedFile.Text += " (Sẽ chỉ lấy 200 dòng đầu)";
                    lblSelectedFile.ForeColor = Color.Red;
                }
                else
                {
                    lblSelectedFile.ForeColor = Color.DarkBlue;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đọc file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        AddedAccounts.Clear();

        if (tabControlAdd.SelectedTab == tabSingle || tabControlAdd.TabPages.Count == 1)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tài khoản và Mật khẩu!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Username = txtUsername.Text.Trim();
            Password = txtPassword.Text.Trim();
            ServerName = cboServer.SelectedItem?.ToString() ?? "Vũ trụ 1";
            TypeAccountVal = (int)nudTypeAccount.Value;
            AddedAccounts.Add((Username, Password, ServerName, TypeAccountVal));
        }
        else if (tabControlAdd.SelectedTab == tabBulk)
        {
            if (_bulkLines.Length == 0)
            {
                MessageBox.Show("Vui lòng chọn file chứa danh sách tài khoản cần thêm!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string defaultServer = cboDefaultServer.SelectedItem?.ToString() ?? "Vũ trụ 1";
            int bulkTypeAccount = (int)nudBulkTypeAccount.Value;
            
            int count = 0;
            foreach (var line in _bulkLines)
            {
                if (count >= 200) break; // Hard limit 200

                var parts = line.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    string u = parts[0].Trim();
                    string p = parts[1].Trim();
                    string s = defaultServer;
                    if (parts.Length >= 3)
                    {
                        if (int.TryParse(parts[2].Trim(), out int serverId))
                            s = Panel.Models.ServerInfo.GetDisplayName(serverId);
                        else
                            s = parts[2].Trim();
                    }
                    if (!string.IsNullOrEmpty(u) && !string.IsNullOrEmpty(p))
                    {
                        AddedAccounts.Add((u, p, s, bulkTypeAccount));
                        count++;
                    }
                }
            }

            if (AddedAccounts.Count == 0)
            {
                MessageBox.Show("Không tìm thấy dữ liệu hợp lệ trong file.\nVui lòng kiểm tra lại định dạng: tk|mk|sv hoặc tk|mk", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }
}

