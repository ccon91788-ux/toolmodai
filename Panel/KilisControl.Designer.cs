namespace Panel
{
    partial class KilisControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.grpKilis = new System.Windows.Forms.GroupBox();
            this.chkMasterEnable = new System.Windows.Forms.CheckBox();
            this.lblZone = new System.Windows.Forms.Label();
            this.numZone = new System.Windows.Forms.NumericUpDown();
            this.lblStartTime = new System.Windows.Forms.Label();
            this.numStartHour = new System.Windows.Forms.NumericUpDown();
            this.numStartMin = new System.Windows.Forms.NumericUpDown();
            this.lblEndTime = new System.Windows.Forms.Label();
            this.numEndHour = new System.Windows.Forms.NumericUpDown();
            this.numEndMin = new System.Windows.Forms.NumericUpDown();
            this.chkAutoBuyAmulet = new System.Windows.Forms.CheckBox();
            this.cboAmuletType = new System.Windows.Forms.ComboBox();
            this.chkUseTDLT = new System.Windows.Forms.CheckBox();
            this.chkAutoZone = new System.Windows.Forms.CheckBox();
            this.lblTrainingArmorMode = new System.Windows.Forms.Label();
            this.cboTrainingArmorMode = new System.Windows.Forms.ComboBox();

            this.grpKilis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numZone)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStartHour)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStartMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEndHour)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEndMin)).BeginInit();
            this.SuspendLayout();

            // grpKilis
            this.grpKilis.Controls.Add(this.chkMasterEnable);
            this.grpKilis.Controls.Add(this.lblZone);
            this.grpKilis.Controls.Add(this.numZone);
            this.grpKilis.Controls.Add(this.lblStartTime);
            this.grpKilis.Controls.Add(this.numStartHour);
            this.grpKilis.Controls.Add(this.numStartMin);
            this.grpKilis.Controls.Add(this.lblEndTime);
            this.grpKilis.Controls.Add(this.numEndHour);
            this.grpKilis.Controls.Add(this.numEndMin);
            this.grpKilis.Controls.Add(this.chkAutoBuyAmulet);
            this.grpKilis.Controls.Add(this.cboAmuletType);
            this.grpKilis.Controls.Add(this.chkUseTDLT);
            this.grpKilis.Controls.Add(this.chkAutoZone);
            this.grpKilis.Controls.Add(this.lblTrainingArmorMode);
            this.grpKilis.Controls.Add(this.cboTrainingArmorMode);
            this.grpKilis.Location = new System.Drawing.Point(6, 6);
            this.grpKilis.Name = "grpKilis";
            this.grpKilis.Size = new System.Drawing.Size(410, 240);
            this.grpKilis.TabIndex = 0;
            this.grpKilis.TabStop = false;
            this.grpKilis.Text = "Úp Kilis (Đệ mới)";

            // chkMasterEnable
            this.chkMasterEnable.AutoSize = true;
            this.chkMasterEnable.Location = new System.Drawing.Point(12, 25);
            this.chkMasterEnable.Text = "Úp kilis";
            this.chkMasterEnable.Name = "chkMasterEnable";
            this.chkMasterEnable.ForeColor = System.Drawing.Color.Red;
            this.chkMasterEnable.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // lblZone
            this.lblZone.AutoSize = true;
            this.lblZone.Location = new System.Drawing.Point(12, 55);
            this.lblZone.Text = "Khu vực:";

            // numZone
            this.numZone.Location = new System.Drawing.Point(164, 53);
            this.numZone.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            this.numZone.Maximum = new decimal(new int[] { 22, 0, 0, 0 });
            this.numZone.Name = "numZone";
            this.numZone.Size = new System.Drawing.Size(60, 23);
            this.numZone.Value = new decimal(new int[] { 5, 0, 0, 0 });

            // lblStartTime
            this.lblStartTime.AutoSize = true;
            this.lblStartTime.Location = new System.Drawing.Point(12, 85);
            this.lblStartTime.Text = "Giờ, phút bắt đầu úp:";

            // numStartHour
            this.numStartHour.Location = new System.Drawing.Point(164, 83);
            this.numStartHour.Maximum = new decimal(new int[] { 23, 0, 0, 0 });
            this.numStartHour.Name = "numStartHour";
            this.numStartHour.Size = new System.Drawing.Size(45, 23);

            // numStartMin
            this.numStartMin.Location = new System.Drawing.Point(215, 83);
            this.numStartMin.Maximum = new decimal(new int[] { 59, 0, 0, 0 });
            this.numStartMin.Name = "numStartMin";
            this.numStartMin.Size = new System.Drawing.Size(45, 23);

            // lblEndTime
            this.lblEndTime.AutoSize = true;
            this.lblEndTime.Location = new System.Drawing.Point(12, 115);
            this.lblEndTime.Text = "Giờ, phút dừng úp:";

            // numEndHour
            this.numEndHour.Location = new System.Drawing.Point(164, 113);
            this.numEndHour.Maximum = new decimal(new int[] { 23, 0, 0, 0 });
            this.numEndHour.Name = "numEndHour";
            this.numEndHour.Size = new System.Drawing.Size(45, 23);

            // numEndMin
            this.numEndMin.Location = new System.Drawing.Point(215, 113);
            this.numEndMin.Maximum = new decimal(new int[] { 59, 0, 0, 0 });
            this.numEndMin.Name = "numEndMin";
            this.numEndMin.Size = new System.Drawing.Size(45, 23);

            // chkAutoBuyAmulet
            this.chkAutoBuyAmulet.AutoSize = true;
            this.chkAutoBuyAmulet.Location = new System.Drawing.Point(12, 145);
            this.chkAutoBuyAmulet.Text = "Tự mua bùa kilis khi hết:";
            this.chkAutoBuyAmulet.Name = "chkAutoBuyAmulet";

            // cboAmuletType
            this.cboAmuletType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAmuletType.FormattingEnabled = true;
            this.cboAmuletType.Items.AddRange(new object[] { "10 phút (5 ngọc)", "60 phút (28 ngọc)", "530 phút (250 ngọc)" });
            this.cboAmuletType.Location = new System.Drawing.Point(185, 143);
            this.cboAmuletType.Size = new System.Drawing.Size(160, 23);
            this.cboAmuletType.Name = "cboAmuletType";

            // chkUseTDLT
            this.chkUseTDLT.AutoSize = true;
            this.chkUseTDLT.Location = new System.Drawing.Point(12, 175);
            this.chkUseTDLT.Text = "Dùng TĐLT khi úp kilis";
            this.chkUseTDLT.Name = "chkUseTDLT";

            // chkAutoZone
            this.chkAutoZone.AutoSize = true;
            this.chkAutoZone.Location = new System.Drawing.Point(12, 205);
            this.chkAutoZone.Text = "Auto zone";
            this.chkAutoZone.Name = "chkAutoZone";

            // lblTrainingArmorMode
            this.lblTrainingArmorMode.AutoSize = true;
            this.lblTrainingArmorMode.Location = new System.Drawing.Point(120, 206);
            this.lblTrainingArmorMode.Text = "Giáp LT:";
            this.lblTrainingArmorMode.Name = "lblTrainingArmorMode";

            // cboTrainingArmorMode
            this.cboTrainingArmorMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTrainingArmorMode.FormattingEnabled = true;
            this.cboTrainingArmorMode.Items.AddRange(new object[] { "Không chạy", "Mặc giáp", "Tháo giáp" });
            this.cboTrainingArmorMode.Location = new System.Drawing.Point(185, 203);
            this.cboTrainingArmorMode.Size = new System.Drawing.Size(130, 23);
            this.cboTrainingArmorMode.Name = "cboTrainingArmorMode";

            // KilisControl
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.grpKilis);
            this.Name = "KilisControl";
            this.Size = new System.Drawing.Size(430, 260);

            this.grpKilis.ResumeLayout(false);
            this.grpKilis.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numZone)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStartHour)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStartMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEndHour)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEndMin)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpKilis;
        private System.Windows.Forms.CheckBox chkMasterEnable;
        private System.Windows.Forms.Label lblZone;
        private System.Windows.Forms.NumericUpDown numZone;
        private System.Windows.Forms.Label lblStartTime;
        private System.Windows.Forms.NumericUpDown numStartHour;
        private System.Windows.Forms.NumericUpDown numStartMin;
        private System.Windows.Forms.Label lblEndTime;
        private System.Windows.Forms.NumericUpDown numEndHour;
        private System.Windows.Forms.NumericUpDown numEndMin;
        private System.Windows.Forms.CheckBox chkAutoBuyAmulet;
        private System.Windows.Forms.ComboBox cboAmuletType;
        private System.Windows.Forms.CheckBox chkUseTDLT;
        private System.Windows.Forms.CheckBox chkAutoZone;
        private System.Windows.Forms.Label lblTrainingArmorMode;
        private System.Windows.Forms.ComboBox cboTrainingArmorMode;
    }
}
