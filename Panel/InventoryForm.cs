using System;
using System.Drawing;
using System.Windows.Forms;

namespace Panel
{
    public partial class InventoryForm : Form
    {
        private int _accountId;
        private bool _isChest;
        private System.Windows.Forms.Timer _timer;

        public InventoryForm(int accountId, string accountName, string serverName, bool isChest)
        {
            InitializeComponent();
            
            _accountId = accountId;
            _isChest = isChest;

            string title = isChest ? $"Rương Đồ của: {accountName} - {serverName}" : $"Hành Trang của: {accountName} - {serverName}";
            
            this.Text = title; // Window title
            Panel.Helpers.UIThemeHelper.ApplyFlatTheme(this);
            
            Panel.Models.InventoryCacheManager.DataUpdated += CacheManager_DataUpdated;

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 2000; // Request update every 2s
            _timer.Tick += Timer_Tick;
            
            this.FormClosing += (s, e) => {
                _timer.Stop();
                Panel.Models.InventoryCacheManager.DataUpdated -= CacheManager_DataUpdated;
            };
            
            // Initial load
            LoadDataFromCache();
            RequestData();
            _timer.Start();

            // Bind filter events
            chkFilterThuong.CheckedChanged += (s, e) => LoadDataFromCache();
            chkFilterVatPham.CheckedChanged += (s, e) => LoadDataFromCache();
            chkFilterSpl.CheckedChanged += (s, e) => LoadDataFromCache();
            chkFilterHuyDiet.CheckedChanged += (s, e) => LoadDataFromCache();
            chkFilterThan.CheckedChanged += (s, e) => LoadDataFromCache();
        }

        private void RequestData()
        {
            // Panel.Program.MainForm...
            // Instead of depending on Form1, Form1 automatically fetches it?
            // Actually, we can just use static variable or Application.OpenForms["Form1"] if needed, 
            // but just getting from Cache is enough because Form1 doesn't fetch dynamically if tab is not focused.
            // Wait, InventoryForm needs to ask Server to fetch if we are viewing it.
            // Let's get the active Form1 instance to send socket command.
            if (Application.OpenForms["Form1"] is Form1 mainForm)
            {
                mainForm.SendSocketCommand(_accountId, $"GET_INVENTORY|{(_isChest ? 1 : 0)}");
            }
        }

        private void CacheManager_DataUpdated(int accId, int type)
        {
            if (accId == _accountId && type == (_isChest ? 1 : 0))
            {
                if (this.IsHandleCreated) this.BeginInvoke((Action)LoadDataFromCache);
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            RequestData();
        }


        private void LoadDataFromCache()
        {
            var data = Panel.Models.InventoryCacheManager.GetCache(_accountId, _isChest ? 1 : 0);
            if (data == null || data.Items == null) return;

            // Simple flicker-free update logic
            dgvInventory.SuspendLayout();
            
            int scrolledRow = dgvInventory.FirstDisplayedScrollingRowIndex;
            int selectedIndex = dgvInventory.CurrentRow?.Index ?? -1;

            try
            {
                dgvInventory.CurrentCell = null;
                dgvInventory.Rows.Clear();
            }
            catch (InvalidOperationException)
            {
                dgvInventory.ResumeLayout();
                return;
            }

            foreach(var item in data.Items)
            {
                if (!MatchesFilter(item.VipFlags)) continue;

                string qtyStr = item.Quantity > 0 ? $"x{item.Quantity}" : "";
                
                dgvInventory.Rows.Add(item.Id.ToString(), item.Name, qtyStr);
            }

            if (dgvInventory.Rows.Count > 0)
            {
                dgvInventory.ClearSelection();
                
                if (selectedIndex >= 0 && selectedIndex < dgvInventory.Rows.Count)
                {
                    // Đặt lại CurrentCell giúp các phím mũi tên ảo hoạt động đúng từ dòng đã Click
                    dgvInventory.CurrentCell = dgvInventory.Rows[selectedIndex].Cells[0];
                    dgvInventory.Rows[selectedIndex].Selected = true;
                }
                
                if (scrolledRow >= 0 && scrolledRow < dgvInventory.Rows.Count)
                {
                    dgvInventory.FirstDisplayedScrollingRowIndex = scrolledRow;
                }
            }

            dgvInventory.ResumeLayout();
        }

        private bool MatchesFilter(int flags)
        {
            // 1: Vật Phẩm Thường, 2: Trang Bị Thường, 4: SPL, 8: Hủy Diệt, 32: Thần Linh, 64: SKH
            if (flags == 1 && chkFilterVatPham.Checked) return true;
            if (flags == 2 && chkFilterThuong.Checked) return true;
            if ((flags & 4) == 4 && chkFilterSpl.Checked) return true;
            if ((flags & 8) == 8 && chkFilterHuyDiet.Checked) return true;
            if ((flags & 32) == 32 && chkFilterThan.Checked) return true;
            if ((flags & 64) == 64 && chkFilterThan.Checked) return true; // SKH + Thần Linh gộp chung hoặc cần checkbox khác? Checkbox Than is for ThanLinh + SKH wait.
            // Users want custom SKH? The UI only has 5. I will treat SKH/TL as Than/ThanLinh.
            return false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
