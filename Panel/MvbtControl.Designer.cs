namespace Panel
{
    partial class MvbtControl
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
            // ── GroupBox 1: Vị trí ───────────────────────────────────────
            this.grpLocation = new System.Windows.Forms.GroupBox();
            this.chkMasterEnable = new System.Windows.Forms.CheckBox();
            this.lblMap = new System.Windows.Forms.Label();
            this.cboMap = new System.Windows.Forms.ComboBox();
            this.chkZoneRequire = new System.Windows.Forms.CheckBox();
            this.numZone = new System.Windows.Forms.NumericUpDown();
            this.lblTargetCount = new System.Windows.Forms.Label();
            this.numTargetCount = new System.Windows.Forms.NumericUpDown();
            this.btnResetCount = new System.Windows.Forms.Button();

            // ── GroupBox 2: Scheduling ───────────────────────────────────
            this.grpSchedule = new System.Windows.Forms.GroupBox();
            this.chkEnable = new System.Windows.Forms.CheckBox();
            this.lblStartTime = new System.Windows.Forms.Label();
            this.numStartHour = new System.Windows.Forms.NumericUpDown();
            this.numStartMin = new System.Windows.Forms.NumericUpDown();
            this.lblEndTime = new System.Windows.Forms.Label();
            this.numEndHour = new System.Windows.Forms.NumericUpDown();
            this.numEndMin = new System.Windows.Forms.NumericUpDown();
            this.lblTimeExample = new System.Windows.Forms.Label();

            // ── GroupBox 3: Hành vi đánh quái ───────────────────────────
            this.grpCombat = new System.Windows.Forms.GroupBox();
            this.chkUseTDLT = new System.Windows.Forms.CheckBox();
            this.chkAvoidSuperMob = new System.Windows.Forms.CheckBox();
            this.chkAutoZone = new System.Windows.Forms.CheckBox();
            this.lblMobTargetType = new System.Windows.Forms.Label();
            this.cboMobTargetType = new System.Windows.Forms.ComboBox();
            this.lblMobIds = new System.Windows.Forms.Label();
            this.txtMobIds = new System.Windows.Forms.TextBox();
            this.lblTrainingArmorMode = new System.Windows.Forms.Label();
            this.cboTrainingArmorMode = new System.Windows.Forms.ComboBox();

            // SuspendLayout
            this.grpLocation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numZone)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetCount)).BeginInit();
            this.grpSchedule.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numStartHour)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStartMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEndHour)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEndMin)).BeginInit();
            this.grpCombat.SuspendLayout();
            this.SuspendLayout();

            // ════════════════════════════════════════════════════════════
            // grpLocation  (Row 1, Max Width 550)
            // ════════════════════════════════════════════════════════════
            this.grpLocation.Controls.Add(this.chkMasterEnable);
            this.grpLocation.Controls.Add(this.lblMap);
            this.grpLocation.Controls.Add(this.cboMap);
            this.grpLocation.Controls.Add(this.chkZoneRequire);
            this.grpLocation.Controls.Add(this.numZone);
            this.grpLocation.Controls.Add(this.lblTargetCount);
            this.grpLocation.Controls.Add(this.numTargetCount);
            this.grpLocation.Controls.Add(this.btnResetCount);
            this.grpLocation.Location = new System.Drawing.Point(6, 6);
            this.grpLocation.Name = "grpLocation";
            this.grpLocation.Size = new System.Drawing.Size(390, 160);
            this.grpLocation.TabIndex = 0;
            this.grpLocation.TabStop = false;
            this.grpLocation.Text = "Cấu hình Vị trí & Mục tiêu";

            // chkMasterEnable
            this.chkMasterEnable.AutoSize = true;
            this.chkMasterEnable.Location = new System.Drawing.Point(12, 25);
            this.chkMasterEnable.Text = "✅ Bật Tự động tính năng này";
            this.chkMasterEnable.Name = "chkMasterEnable";
            this.chkMasterEnable.ForeColor = System.Drawing.Color.Red;
            this.chkMasterEnable.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // lblMap
            this.lblMap.AutoSize = true;
            this.lblMap.Location = new System.Drawing.Point(12, 60);
            this.lblMap.Text = "ID Map:";

            // cboMap
            this.cboMap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cboMap.FormattingEnabled = true;
            this.cboMap.Location = new System.Drawing.Point(85, 57);
            this.cboMap.Size = new System.Drawing.Size(180, 23);
            this.cboMap.Name = "cboMap";

            // chkZoneRequire
            this.chkZoneRequire.AutoSize = true;
            this.chkZoneRequire.Location = new System.Drawing.Point(268, 59);
            this.chkZoneRequire.Text = "Zone:";
            this.chkZoneRequire.Name = "chkZoneRequire";

            // numZone
            this.numZone.Location = new System.Drawing.Point(328, 57);
            this.numZone.Minimum = new decimal(new int[] { 1, 0, 0, -2147483648 });
            this.numZone.Maximum = new decimal(new int[] { 22, 0, 0, 0 });
            this.numZone.Name = "numZone";
            this.numZone.Size = new System.Drawing.Size(50, 23);
            this.numZone.Value = new decimal(new int[] { 1, 0, 0, -2147483648 });

            // lblTargetCount
            this.lblTargetCount.AutoSize = true;
            this.lblTargetCount.Location = new System.Drawing.Point(12, 95);
            this.lblTargetCount.Text = "Số mảnh cần úp:";
            this.lblTargetCount.Name = "lblTargetCount";

            // numTargetCount
            this.numTargetCount.Location = new System.Drawing.Point(125, 93);
            this.numTargetCount.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            this.numTargetCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numTargetCount.Name = "numTargetCount";
            this.numTargetCount.Size = new System.Drawing.Size(65, 23);
            this.numTargetCount.Value = new decimal(new int[] { 99, 0, 0, 0 });

            // lblCurrentCount
            this.lblCurrentCount = new System.Windows.Forms.Label();
            this.lblCurrentCount.AutoSize = true;
            this.lblCurrentCount.Location = new System.Drawing.Point(200, 95);
            this.lblCurrentCount.Text = "Đã nhặt: 0";
            this.lblCurrentCount.Name = "lblCurrentCount";
            this.lblCurrentCount.ForeColor = System.Drawing.Color.Blue;
            this.lblCurrentCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpLocation.Controls.Add(this.lblCurrentCount);

            // btnResetCount
            this.btnResetCount.Location = new System.Drawing.Point(115, 125);
            this.btnResetCount.Size = new System.Drawing.Size(180, 25);
            this.btnResetCount.Text = "Reset bộ đếm mảnh";
            this.btnResetCount.Name = "btnResetCount";
            this.btnResetCount.Click += new System.EventHandler(this.BtnResetCount_Click);

            // ════════════════════════════════════════════════════════════
            // grpSchedule  (Row 2, Multiple lines)
            // ════════════════════════════════════════════════════════════
            this.grpSchedule.Controls.Add(this.chkEnable);
            this.grpSchedule.Controls.Add(this.lblStartTime);
            this.grpSchedule.Controls.Add(this.numStartHour);
            this.grpSchedule.Controls.Add(this.numStartMin);
            this.grpSchedule.Controls.Add(this.lblEndTime);
            this.grpSchedule.Controls.Add(this.numEndHour);
            this.grpSchedule.Controls.Add(this.numEndMin);
            this.grpSchedule.Controls.Add(this.lblTimeExample);
            this.grpSchedule.Location = new System.Drawing.Point(6, 175);
            this.grpSchedule.Name = "grpSchedule";
            this.grpSchedule.Size = new System.Drawing.Size(410, 115);
            this.grpSchedule.TabIndex = 1;
            this.grpSchedule.TabStop = false;
            this.grpSchedule.Text = "Lập lịch (Treament Plan)";

            // chkEnable
            this.chkEnable.AutoSize = true;
            this.chkEnable.Location = new System.Drawing.Point(12, 30);
            this.chkEnable.Text = "Chạy theo khung giờ:";
            this.chkEnable.Name = "chkEnable";

            // lblStartTime
            this.lblStartTime.AutoSize = true;
            this.lblStartTime.Location = new System.Drawing.Point(180, 31);
            this.lblStartTime.Text = "Bắt đầu:";

            // numStartHour
            this.numStartHour.Location = new System.Drawing.Point(250, 29);
            this.numStartHour.Maximum = new decimal(new int[] { 23, 0, 0, 0 });
            this.numStartHour.Name = "numStartHour";
            this.numStartHour.Size = new System.Drawing.Size(55, 23);

            // numStartMin
            this.numStartMin.Location = new System.Drawing.Point(320, 29);
            this.numStartMin.Maximum = new decimal(new int[] { 59, 0, 0, 0 });
            this.numStartMin.Name = "numStartMin";
            this.numStartMin.Size = new System.Drawing.Size(55, 23);

            // lblEndTime
            this.lblEndTime.AutoSize = true;
            this.lblEndTime.Location = new System.Drawing.Point(180, 60);
            this.lblEndTime.Text = "Dừng lúc:";

            // numEndHour
            this.numEndHour.Location = new System.Drawing.Point(250, 58);
            this.numEndHour.Maximum = new decimal(new int[] { 23, 0, 0, 0 });
            this.numEndHour.Name = "numEndHour";
            this.numEndHour.Size = new System.Drawing.Size(55, 23);

            // numEndMin
            this.numEndMin.Location = new System.Drawing.Point(320, 58);
            this.numEndMin.Maximum = new decimal(new int[] { 59, 0, 0, 0 });
            this.numEndMin.Name = "numEndMin";
            this.numEndMin.Size = new System.Drawing.Size(55, 23);

            // lblTimeExample
            this.lblTimeExample.AutoSize = true;
            this.lblTimeExample.Location = new System.Drawing.Point(257, 85);
            this.lblTimeExample.ForeColor = System.Drawing.Color.Blue;
            this.lblTimeExample.Text = "Ví dụ, 07:30 => 21:30";
            this.lblTimeExample.Name = "lblTimeExample";

            // ════════════════════════════════════════════════════════════
            // grpCombat  (Row 3, Max Width 550)
            // ════════════════════════════════════════════════════════════
            this.grpCombat.Controls.Add(this.chkUseTDLT);
            this.grpCombat.Controls.Add(this.chkAvoidSuperMob);
            this.grpCombat.Controls.Add(this.chkAutoZone);
            this.grpCombat.Controls.Add(this.lblMobTargetType);
            this.grpCombat.Controls.Add(this.cboMobTargetType);
            this.grpCombat.Controls.Add(this.lblMobIds);
            this.grpCombat.Controls.Add(this.txtMobIds);
            this.grpCombat.Controls.Add(this.lblTrainingArmorMode);
            this.grpCombat.Controls.Add(this.cboTrainingArmorMode);
            this.grpCombat.Location = new System.Drawing.Point(6, 305);
            this.grpCombat.Name = "grpCombat";
            this.grpCombat.Size = new System.Drawing.Size(410, 115);
            this.grpCombat.TabIndex = 2;
            this.grpCombat.TabStop = false;
            this.grpCombat.Text = "Cài đặt Đánh Quái còn lại";

            // chkUseTDLT
            this.chkUseTDLT.AutoSize = true;
            this.chkUseTDLT.Location = new System.Drawing.Point(12, 25);
            this.chkUseTDLT.Text = "Tự động luyện tập";
            this.chkUseTDLT.Name = "chkUseTDLT";

            // chkAvoidSuperMob
            this.chkAvoidSuperMob.AutoSize = true;
            this.chkAvoidSuperMob.Location = new System.Drawing.Point(160, 25);
            this.chkAvoidSuperMob.Text = "Né siêu quái";
            this.chkAvoidSuperMob.Name = "chkAvoidSuperMob";
            
            // chkAutoZone
            this.chkAutoZone.AutoSize = true;
            this.chkAutoZone.Location = new System.Drawing.Point(280, 25);
            this.chkAutoZone.Text = "Auto zone";
            this.chkAutoZone.Name = "chkAutoZone";

            // lblMobTargetType
            this.lblMobTargetType.AutoSize = true;
            this.lblMobTargetType.Location = new System.Drawing.Point(12, 55);
            this.lblMobTargetType.Text = "Loại quái:";
            this.lblMobTargetType.Name = "lblMobTargetType";

            // cboMobTargetType
            this.cboMobTargetType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMobTargetType.FormattingEnabled = true;
            this.cboMobTargetType.Items.AddRange(new object[] { "Đánh tất cả", "Theo id" });
            this.cboMobTargetType.Location = new System.Drawing.Point(80, 52);
            this.cboMobTargetType.Size = new System.Drawing.Size(110, 23);
            this.cboMobTargetType.Name = "cboMobTargetType";
            
            // lblMobIds
            this.lblMobIds.AutoSize = true;
            this.lblMobIds.Location = new System.Drawing.Point(210, 55);
            this.lblMobIds.Text = "ID quái:";
            this.lblMobIds.Name = "lblMobIds";

            // txtMobIds
            this.txtMobIds.Location = new System.Drawing.Point(275, 52);
            this.txtMobIds.Size = new System.Drawing.Size(100, 23);
            this.txtMobIds.Name = "txtMobIds";
            this.txtMobIds.PlaceholderText = "VD: 1,2,3";

            // lblTrainingArmorMode
            this.lblTrainingArmorMode.AutoSize = true;
            this.lblTrainingArmorMode.Location = new System.Drawing.Point(12, 85);
            this.lblTrainingArmorMode.Text = "Giáp LT:";
            this.lblTrainingArmorMode.Name = "lblTrainingArmorMode";

            // cboTrainingArmorMode
            this.cboTrainingArmorMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTrainingArmorMode.FormattingEnabled = true;
            this.cboTrainingArmorMode.Items.AddRange(new object[] { "Không chạy", "Mặc giáp", "Tháo giáp" });
            this.cboTrainingArmorMode.Location = new System.Drawing.Point(80, 82);
            this.cboTrainingArmorMode.Size = new System.Drawing.Size(130, 23);
            this.cboTrainingArmorMode.Name = "cboTrainingArmorMode";

            // ════════════════════════════════════════════════════════════
            // MvbtControl  (UserControl root)
            // ════════════════════════════════════════════════════════════
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.grpLocation);
            this.Controls.Add(this.grpSchedule);
            this.Controls.Add(this.grpCombat);
            this.Name = "MvbtControl";
            this.Size = new System.Drawing.Size(430, 430);

            // ResumeLayout
            this.grpLocation.ResumeLayout(false);
            this.grpLocation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numZone)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetCount)).EndInit();
            this.grpSchedule.ResumeLayout(false);
            this.grpSchedule.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numStartHour)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStartMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEndHour)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEndMin)).EndInit();
            this.grpCombat.ResumeLayout(false);
            this.grpCombat.PerformLayout();
            this.ResumeLayout(false);
        }

        // ── Fields ───────────────────────────────────────────────────────
        private System.Windows.Forms.GroupBox grpLocation;
        private System.Windows.Forms.CheckBox chkMasterEnable;
        private System.Windows.Forms.Label lblMap;
        private System.Windows.Forms.ComboBox cboMap;
        private System.Windows.Forms.CheckBox chkZoneRequire;
        private System.Windows.Forms.NumericUpDown numZone;
        private System.Windows.Forms.Label lblTargetCount;
        private System.Windows.Forms.NumericUpDown numTargetCount;
        private System.Windows.Forms.Label lblCurrentCount;
        private System.Windows.Forms.Button btnResetCount;

        private System.Windows.Forms.GroupBox grpSchedule;
        private System.Windows.Forms.CheckBox chkEnable;
        private System.Windows.Forms.Label lblStartTime;
        private System.Windows.Forms.NumericUpDown numStartHour;
        private System.Windows.Forms.NumericUpDown numStartMin;
        private System.Windows.Forms.Label lblEndTime;
        private System.Windows.Forms.NumericUpDown numEndHour;
        private System.Windows.Forms.NumericUpDown numEndMin;
        private System.Windows.Forms.Label lblTimeExample;

        private System.Windows.Forms.GroupBox grpCombat;
        private System.Windows.Forms.CheckBox chkUseTDLT;
        private System.Windows.Forms.CheckBox chkAvoidSuperMob;
        private System.Windows.Forms.CheckBox chkAutoZone;
        private System.Windows.Forms.Label lblMobTargetType;
        private System.Windows.Forms.ComboBox cboMobTargetType;
        private System.Windows.Forms.Label lblMobIds;
        private System.Windows.Forms.TextBox txtMobIds;
        private System.Windows.Forms.Label lblTrainingArmorMode;
        private System.Windows.Forms.ComboBox cboTrainingArmorMode;
    }
}
