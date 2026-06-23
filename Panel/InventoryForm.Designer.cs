namespace Panel
{
    partial class InventoryForm
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelFilter = new System.Windows.Forms.Panel();
            this.grpFilter = new System.Windows.Forms.GroupBox();
            this.chkFilterSpl = new System.Windows.Forms.CheckBox();
            this.chkFilterThuong = new System.Windows.Forms.CheckBox();
            this.chkFilterVatPham = new System.Windows.Forms.CheckBox();
            this.chkFilterHuyDiet = new System.Windows.Forms.CheckBox();
            this.chkFilterThan = new System.Windows.Forms.CheckBox();
            this.dgvInventory = new System.Windows.Forms.DataGridView();
            this.colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colQty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelFilter.SuspendLayout();
            this.grpFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInventory)).BeginInit();
            this.SuspendLayout();
            // 
            // panelFilter
            // 
            this.panelFilter.Controls.Add(this.grpFilter);
            this.panelFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilter.Location = new System.Drawing.Point(0, 0);
            this.panelFilter.Name = "panelFilter";
            this.panelFilter.Padding = new System.Windows.Forms.Padding(10);
            this.panelFilter.Size = new System.Drawing.Size(400, 100);
            this.panelFilter.TabIndex = 1;
            this.panelFilter.BackColor = System.Drawing.SystemColors.Control;
            // 
            // grpFilter
            // 
            this.grpFilter.Controls.Add(this.chkFilterSpl);
            this.grpFilter.Controls.Add(this.chkFilterThuong);
            this.grpFilter.Controls.Add(this.chkFilterVatPham);
            this.grpFilter.Controls.Add(this.chkFilterHuyDiet);
            this.grpFilter.Controls.Add(this.chkFilterThan);
            this.grpFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpFilter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpFilter.Location = new System.Drawing.Point(10, 10);
            this.grpFilter.Name = "grpFilter";
            this.grpFilter.Size = new System.Drawing.Size(380, 80);
            this.grpFilter.TabIndex = 0;
            this.grpFilter.TabStop = false;
            this.grpFilter.Text = "Tùy chỉnh";
            // 
            // chkFilterSpl
            // 
            this.chkFilterSpl.AutoSize = true;
            this.chkFilterSpl.Checked = true;
            this.chkFilterSpl.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFilterSpl.Location = new System.Drawing.Point(15, 50);
            this.chkFilterSpl.Name = "chkFilterSpl";
            this.chkFilterSpl.Size = new System.Drawing.Size(124, 19);
            this.chkFilterSpl.TabIndex = 4;
            this.chkFilterSpl.Text = "Hiện trang bị có SPL";
            this.chkFilterSpl.UseVisualStyleBackColor = true;
            // 
            // chkFilterThuong
            // 
            this.chkFilterThuong.AutoSize = true;
            this.chkFilterThuong.Checked = true;
            this.chkFilterThuong.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFilterThuong.Location = new System.Drawing.Point(15, 25);
            this.chkFilterThuong.Name = "chkFilterThuong";
            this.chkFilterThuong.Size = new System.Drawing.Size(134, 19);
            this.chkFilterThuong.TabIndex = 0;
            this.chkFilterThuong.Text = "Hiện trang bị thường";
            this.chkFilterThuong.UseVisualStyleBackColor = true;
            // 
            // chkFilterVatPham
            // 
            this.chkFilterVatPham.AutoSize = true;
            this.chkFilterVatPham.Checked = true;
            this.chkFilterVatPham.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFilterVatPham.Location = new System.Drawing.Point(170, 25);
            this.chkFilterVatPham.Name = "chkFilterVatPham";
            this.chkFilterVatPham.Size = new System.Drawing.Size(133, 19);
            this.chkFilterVatPham.TabIndex = 1;
            this.chkFilterVatPham.Text = "Hiện vật phẩm thường";
            this.chkFilterVatPham.UseVisualStyleBackColor = true;
            // 
            // chkFilterHuyDiet
            // 
            this.chkFilterHuyDiet.AutoSize = true;
            this.chkFilterHuyDiet.Checked = true;
            this.chkFilterHuyDiet.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFilterHuyDiet.Location = new System.Drawing.Point(170, 50);
            this.chkFilterHuyDiet.Name = "chkFilterHuyDiet";
            this.chkFilterHuyDiet.Size = new System.Drawing.Size(146, 19);
            this.chkFilterHuyDiet.TabIndex = 3;
            this.chkFilterHuyDiet.Text = "Hiện trang bị cấp hủy diệt";
            this.chkFilterHuyDiet.UseVisualStyleBackColor = true;
            // 
            // chkFilterThan
            // 
            this.chkFilterThan.AutoSize = true;
            this.chkFilterThan.Checked = true;
            this.chkFilterThan.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFilterThan.Location = new System.Drawing.Point(170, 75);
            this.chkFilterThan.Name = "chkFilterThan";
            this.chkFilterThan.Size = new System.Drawing.Size(133, 19);
            this.chkFilterThan.TabIndex = 2;
            this.chkFilterThan.Text = "Hiện trang bị cấp thần";
            this.chkFilterThan.Visible = false; // Hide if out of bounds, or re-arrange
            this.chkFilterThan.UseVisualStyleBackColor = true;
            // 
            // dgvInventory
            // 
            this.dgvInventory.AllowUserToAddRows = false;
            this.dgvInventory.AllowUserToDeleteRows = false;
            this.dgvInventory.AllowUserToResizeRows = false;
            this.dgvInventory.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvInventory.EnableHeadersVisualStyles = true;
            this.dgvInventory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvInventory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvInventory.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colId,
            this.colName,
            this.colQty});
            this.dgvInventory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvInventory.Location = new System.Drawing.Point(0, 100);
            this.dgvInventory.Name = "dgvInventory";
            this.dgvInventory.ReadOnly = true;
            this.dgvInventory.RowHeadersVisible = false;
            this.dgvInventory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvInventory.Size = new System.Drawing.Size(400, 460);
            this.dgvInventory.TabIndex = 2;
            this.dgvInventory.RowTemplate.Height = 36;
            // 
            // colId
            // 
            this.colId.HeaderText = "Id";
            this.colId.Name = "colId";
            this.colId.ReadOnly = true;
            this.colId.Width = 50;
            // 
            // colName
            // 
            this.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colName.HeaderText = "Tên vật phẩm";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            // 
            // colQty
            // 
            this.colQty.HeaderText = "SL";
            this.colQty.Name = "colQty";
            this.colQty.ReadOnly = true;
            this.colQty.Width = 80;
            // 
            // InventoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 560);
            this.Controls.Add(this.dgvInventory);
            this.Controls.Add(this.panelFilter);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InventoryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "InventoryForm";
            this.BackColor = System.Drawing.SystemColors.Control;
            this.panelFilter.ResumeLayout(false);
            this.grpFilter.ResumeLayout(false);
            this.grpFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInventory)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelFilter;
        private System.Windows.Forms.GroupBox grpFilter;
        private System.Windows.Forms.CheckBox chkFilterSpl;
        private System.Windows.Forms.CheckBox chkFilterThuong;
        private System.Windows.Forms.CheckBox chkFilterVatPham;
        private System.Windows.Forms.CheckBox chkFilterHuyDiet;
        private System.Windows.Forms.CheckBox chkFilterThan;
        private System.Windows.Forms.DataGridView dgvInventory;
        private System.Windows.Forms.DataGridViewTextBoxColumn colId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colQty;
    }
}
