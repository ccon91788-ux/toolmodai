namespace Panel
{
    partial class UpSkhControl
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
            this.components = new System.ComponentModel.Container();
            this.lblCountdown = new System.Windows.Forms.Label();
            this.grpSkhTracker = new System.Windows.Forms.GroupBox();
            this.lblSet5 = new System.Windows.Forms.Label();
            this.lblSet4 = new System.Windows.Forms.Label();
            this.lblSet3 = new System.Windows.Forms.Label();
            this.lblSet2 = new System.Windows.Forms.Label();
            this.lblSet1 = new System.Windows.Forms.Label();
            this.lblTotalSkh = new System.Windows.Forms.Label();
            this.grpSettings = new System.Windows.Forms.GroupBox();
            this.chkUseCoMayMan = new System.Windows.Forms.CheckBox();
            this.chkUseTdlt = new System.Windows.Forms.CheckBox();
            this.chkUsePrivateTicket = new System.Windows.Forms.CheckBox();
            this.grpAutoBuy = new System.Windows.Forms.GroupBox();
            this.chkAutoBuyPrivateTicket = new System.Windows.Forms.CheckBox();
            this.chkAutoBuyCoMayMan = new System.Windows.Forms.CheckBox();
            this.chkAutoBuyTdlt = new System.Windows.Forms.CheckBox();
            this.tmrCountdown = new System.Windows.Forms.Timer(this.components);
            this.grpSkhTracker.SuspendLayout();
            this.grpSettings.SuspendLayout();
            this.grpAutoBuy.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCountdown
            // 
            this.lblCountdown.AutoSize = true;
            this.lblCountdown.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCountdown.ForeColor = System.Drawing.Color.DarkOrange;
            this.lblCountdown.Location = new System.Drawing.Point(14, 15);
            this.lblCountdown.Name = "lblCountdown";
            this.lblCountdown.Size = new System.Drawing.Size(325, 19);
            this.lblCountdown.TabIndex = 0;
            this.lblCountdown.Text = "Thời gian còn lại: 00 ngày 00 giờ 00 phút 00 giây";
            // 
            // grpSkhTracker
            // 
            this.grpSkhTracker.Controls.Add(this.lblSet5);
            this.grpSkhTracker.Controls.Add(this.lblSet4);
            this.grpSkhTracker.Controls.Add(this.lblSet3);
            this.grpSkhTracker.Controls.Add(this.lblSet2);
            this.grpSkhTracker.Controls.Add(this.lblSet1);
            this.grpSkhTracker.Controls.Add(this.lblTotalSkh);
            this.grpSkhTracker.Location = new System.Drawing.Point(14, 46);
            this.grpSkhTracker.Name = "grpSkhTracker";
            this.grpSkhTracker.Size = new System.Drawing.Size(380, 185);
            this.grpSkhTracker.TabIndex = 1;
            this.grpSkhTracker.TabStop = false;
            this.grpSkhTracker.Text = "List SKH";
            // 
            // lblSet5
            // 
            this.lblSet5.AutoSize = true;
            this.lblSet5.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSet5.Location = new System.Drawing.Point(16, 126);
            this.lblSet5.Name = "lblSet5";
            this.lblSet5.Size = new System.Drawing.Size(147, 15);
            this.lblSet5.TabIndex = 5;
            this.lblSet5.Text = "Set 5 [0-0-0-0-0]";
            // 
            // lblSet4
            // 
            this.lblSet4.AutoSize = true;
            this.lblSet4.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSet4.Location = new System.Drawing.Point(16, 101);
            this.lblSet4.Name = "lblSet4";
            this.lblSet4.Size = new System.Drawing.Size(147, 15);
            this.lblSet4.TabIndex = 4;
            this.lblSet4.Text = "Set 4 [0-0-0-0-0]";
            // 
            // lblSet3
            // 
            this.lblSet3.AutoSize = true;
            this.lblSet3.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSet3.Location = new System.Drawing.Point(16, 76);
            this.lblSet3.Name = "lblSet3";
            this.lblSet3.Size = new System.Drawing.Size(147, 15);
            this.lblSet3.TabIndex = 3;
            this.lblSet3.Text = "Set 3 [0-0-0-0-0]";
            // 
            // lblSet2
            // 
            this.lblSet2.AutoSize = true;
            this.lblSet2.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSet2.Location = new System.Drawing.Point(16, 51);
            this.lblSet2.Name = "lblSet2";
            this.lblSet2.Size = new System.Drawing.Size(147, 15);
            this.lblSet2.TabIndex = 2;
            this.lblSet2.Text = "Set 2 [0-0-0-0-0]";
            // 
            // lblSet1
            // 
            this.lblSet1.AutoSize = true;
            this.lblSet1.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSet1.Location = new System.Drawing.Point(16, 26);
            this.lblSet1.Name = "lblSet1";
            this.lblSet1.Size = new System.Drawing.Size(147, 15);
            this.lblSet1.TabIndex = 1;
            this.lblSet1.Text = "Set 1 [0-0-0-0-0]";
            // 
            // lblTotalSkh
            // 
            this.lblTotalSkh.AutoSize = true;
            this.lblTotalSkh.Font = new System.Drawing.Font("Segoe UI", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point);
            this.lblTotalSkh.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblTotalSkh.Location = new System.Drawing.Point(16, 156);
            this.lblTotalSkh.Name = "lblTotalSkh";
            this.lblTotalSkh.Size = new System.Drawing.Size(169, 15);
            this.lblTotalSkh.TabIndex = 0;
            this.lblTotalSkh.Text = "Tổng số món Kích Hoạt đang có: 0";
            // 
            // grpSettings
            // 
            this.grpSettings.Controls.Add(this.chkUseCoMayMan);
            this.grpSettings.Controls.Add(this.chkUseTdlt);
            this.grpSettings.Controls.Add(this.chkUsePrivateTicket);
            this.grpSettings.Location = new System.Drawing.Point(14, 237);
            this.grpSettings.Name = "grpSettings";
            this.grpSettings.Size = new System.Drawing.Size(185, 105);
            this.grpSettings.TabIndex = 2;
            this.grpSettings.TabStop = false;
            this.grpSettings.Text = "Cài đặt úp skh";
            // 
            // chkUseCoMayMan
            // 
            this.chkUseCoMayMan.AutoSize = true;
            this.chkUseCoMayMan.Location = new System.Drawing.Point(16, 75);
            this.chkUseCoMayMan.Name = "chkUseCoMayMan";
            this.chkUseCoMayMan.Size = new System.Drawing.Size(130, 19);
            this.chkUseCoMayMan.TabIndex = 2;
            this.chkUseCoMayMan.Text = "Dùng Cỏ may mắn";
            this.chkUseCoMayMan.UseVisualStyleBackColor = true;
            // 
            // chkUseTdlt
            // 
            this.chkUseTdlt.AutoSize = true;
            this.chkUseTdlt.Location = new System.Drawing.Point(16, 50);
            this.chkUseTdlt.Name = "chkUseTdlt";
            this.chkUseTdlt.Size = new System.Drawing.Size(84, 19);
            this.chkUseTdlt.TabIndex = 1;
            this.chkUseTdlt.Text = "Dùng TĐLT";
            this.chkUseTdlt.UseVisualStyleBackColor = true;
            // 
            // chkUsePrivateTicket
            // 
            this.chkUsePrivateTicket.AutoSize = true;
            this.chkUsePrivateTicket.Location = new System.Drawing.Point(16, 25);
            this.chkUsePrivateTicket.Name = "chkUsePrivateTicket";
            this.chkUsePrivateTicket.Size = new System.Drawing.Size(149, 19);
            this.chkUsePrivateTicket.TabIndex = 0;
            this.chkUsePrivateTicket.Text = "Map Riêng Tư (Dùng vé)";
            this.chkUsePrivateTicket.UseVisualStyleBackColor = true;
            // 
            // grpAutoBuy
            // 
            this.grpAutoBuy.Controls.Add(this.chkAutoBuyPrivateTicket);
            this.grpAutoBuy.Controls.Add(this.chkAutoBuyCoMayMan);
            this.grpAutoBuy.Controls.Add(this.chkAutoBuyTdlt);
            this.grpAutoBuy.Location = new System.Drawing.Point(209, 237);
            this.grpAutoBuy.Name = "grpAutoBuy";
            this.grpAutoBuy.Size = new System.Drawing.Size(185, 105);
            this.grpAutoBuy.TabIndex = 3;
            this.grpAutoBuy.TabStop = false;
            this.grpAutoBuy.Text = "Tự động mua";
            // 
            // chkAutoBuyPrivateTicket
            // 
            this.chkAutoBuyPrivateTicket.AutoSize = true;
            this.chkAutoBuyPrivateTicket.Location = new System.Drawing.Point(16, 75);
            this.chkAutoBuyPrivateTicket.Name = "chkAutoBuyPrivateTicket";
            this.chkAutoBuyPrivateTicket.Size = new System.Drawing.Size(107, 19);
            this.chkAutoBuyPrivateTicket.TabIndex = 2;
            this.chkAutoBuyPrivateTicket.Text = "Mua Vé riêng tư";
            this.chkAutoBuyPrivateTicket.UseVisualStyleBackColor = true;
            // 
            // chkAutoBuyCoMayMan
            // 
            this.chkAutoBuyCoMayMan.AutoSize = true;
            this.chkAutoBuyCoMayMan.Location = new System.Drawing.Point(16, 50);
            this.chkAutoBuyCoMayMan.Name = "chkAutoBuyCoMayMan";
            this.chkAutoBuyCoMayMan.Size = new System.Drawing.Size(126, 19);
            this.chkAutoBuyCoMayMan.TabIndex = 1;
            this.chkAutoBuyCoMayMan.Text = "Mua Cỏ may mắn";
            this.chkAutoBuyCoMayMan.UseVisualStyleBackColor = true;
            // 
            // chkAutoBuyTdlt
            // 
            this.chkAutoBuyTdlt.AutoSize = true;
            this.chkAutoBuyTdlt.Location = new System.Drawing.Point(16, 25);
            this.chkAutoBuyTdlt.Name = "chkAutoBuyTdlt";
            this.chkAutoBuyTdlt.Size = new System.Drawing.Size(80, 19);
            this.chkAutoBuyTdlt.TabIndex = 0;
            this.chkAutoBuyTdlt.Text = "Mua TĐLT";
            this.chkAutoBuyTdlt.UseVisualStyleBackColor = true;
            // 
            // tmrCountdown
            // 
            this.tmrCountdown.Interval = 1000;
            this.tmrCountdown.Tick += new System.EventHandler(this.TmrCountdown_Tick);
            // 
            // UpSkhControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.grpAutoBuy);
            this.Controls.Add(this.grpSettings);
            this.Controls.Add(this.grpSkhTracker);
            this.Controls.Add(this.lblCountdown);
            this.Name = "UpSkhControl";
            this.Size = new System.Drawing.Size(410, 355);
            this.grpSkhTracker.ResumeLayout(false);
            this.grpSkhTracker.PerformLayout();
            this.grpSettings.ResumeLayout(false);
            this.grpSettings.PerformLayout();
            this.grpAutoBuy.ResumeLayout(false);
            this.grpAutoBuy.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblCountdown;
        private System.Windows.Forms.GroupBox grpSkhTracker;
        private System.Windows.Forms.Label lblTotalSkh;
        private System.Windows.Forms.Label lblSet5;
        private System.Windows.Forms.Label lblSet4;
        private System.Windows.Forms.Label lblSet3;
        private System.Windows.Forms.Label lblSet2;
        private System.Windows.Forms.Label lblSet1;
        private System.Windows.Forms.GroupBox grpSettings;
        public System.Windows.Forms.CheckBox chkUseCoMayMan;
        public System.Windows.Forms.CheckBox chkUseTdlt;
        public System.Windows.Forms.CheckBox chkUsePrivateTicket;
        private System.Windows.Forms.GroupBox grpAutoBuy;
        public System.Windows.Forms.CheckBox chkAutoBuyPrivateTicket;
        public System.Windows.Forms.CheckBox chkAutoBuyCoMayMan;
        public System.Windows.Forms.CheckBox chkAutoBuyTdlt;
        private System.Windows.Forms.Timer tmrCountdown;
    }
}
