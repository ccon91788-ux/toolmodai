namespace Panel
{
    partial class ScheduleControl
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
            this.grpSchedule = new System.Windows.Forms.GroupBox();
            this.chkScheduleEnable = new System.Windows.Forms.CheckBox();
            this.lblStartTime = new System.Windows.Forms.Label();
            this.dtpStartTime = new System.Windows.Forms.DateTimePicker();
            this.lblEndTime = new System.Windows.Forms.Label();
            this.dtpEndTime = new System.Windows.Forms.DateTimePicker();

            this.grpSchedule.SuspendLayout();
            this.SuspendLayout();

            // 
            // grpSchedule
            // 
            this.grpSchedule.Controls.Add(this.chkScheduleEnable);
            this.grpSchedule.Controls.Add(this.lblStartTime);
            this.grpSchedule.Controls.Add(this.dtpStartTime);
            this.grpSchedule.Controls.Add(this.lblEndTime);
            this.grpSchedule.Controls.Add(this.dtpEndTime);
            this.grpSchedule.Location = new System.Drawing.Point(6, 6);
            this.grpSchedule.Name = "grpSchedule";
            this.grpSchedule.Size = new System.Drawing.Size(320, 85);
            this.grpSchedule.TabIndex = 0;
            this.grpSchedule.TabStop = false;
            this.grpSchedule.Text = "Khung Giờ Hoạt Động (Lịch Trình)";

            // chkScheduleEnable
            this.chkScheduleEnable.AutoSize = true;
            this.chkScheduleEnable.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.chkScheduleEnable.ForeColor = System.Drawing.Color.MediumBlue;
            this.chkScheduleEnable.Location = new System.Drawing.Point(15, 25);
            this.chkScheduleEnable.Name = "chkScheduleEnable";
            this.chkScheduleEnable.Size = new System.Drawing.Size(193, 19);
            this.chkScheduleEnable.TabIndex = 0;
            this.chkScheduleEnable.Text = "Kích hoạt Khung giờ hoạt động";
            this.chkScheduleEnable.UseVisualStyleBackColor = true;

            // lblStartTime
            this.lblStartTime.AutoSize = true;
            this.lblStartTime.Location = new System.Drawing.Point(15, 55);
            this.lblStartTime.Name = "lblStartTime";
            this.lblStartTime.Size = new System.Drawing.Size(23, 15);
            this.lblStartTime.TabIndex = 1;
            this.lblStartTime.Text = "Từ:";

            // dtpStartTime
            this.dtpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpStartTime.Location = new System.Drawing.Point(45, 52);
            this.dtpStartTime.Name = "dtpStartTime";
            this.dtpStartTime.ShowUpDown = true;
            this.dtpStartTime.Size = new System.Drawing.Size(100, 23);
            this.dtpStartTime.TabIndex = 2;

            // lblEndTime
            this.lblEndTime.AutoSize = true;
            this.lblEndTime.Location = new System.Drawing.Point(155, 55);
            this.lblEndTime.Name = "lblEndTime";
            this.lblEndTime.Size = new System.Drawing.Size(31, 15);
            this.lblEndTime.TabIndex = 3;
            this.lblEndTime.Text = "Đến:";

            // dtpEndTime
            this.dtpEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpEndTime.Location = new System.Drawing.Point(195, 52);
            this.dtpEndTime.Name = "dtpEndTime";
            this.dtpEndTime.ShowUpDown = true;
            this.dtpEndTime.Size = new System.Drawing.Size(100, 23);
            this.dtpEndTime.TabIndex = 4;

            // 
            // ScheduleControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.grpSchedule);
            this.Name = "ScheduleControl";
            this.Size = new System.Drawing.Size(340, 100);
            this.grpSchedule.ResumeLayout(false);
            this.grpSchedule.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpSchedule;
        private System.Windows.Forms.CheckBox chkScheduleEnable;
        private System.Windows.Forms.Label lblStartTime;
        private System.Windows.Forms.DateTimePicker dtpStartTime;
        private System.Windows.Forms.Label lblEndTime;
        private System.Windows.Forms.DateTimePicker dtpEndTime;
    }
}
