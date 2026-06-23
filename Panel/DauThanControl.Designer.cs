namespace Panel
{
    partial class DauThanControl
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
            this.grpRequest = new System.Windows.Forms.GroupBox();
            this.chkAutoRequest = new System.Windows.Forms.CheckBox();
            this.chkRequestCondition = new System.Windows.Forms.CheckBox();
            this.numRequestIfUnder = new System.Windows.Forms.NumericUpDown();

            this.grpDonate = new System.Windows.Forms.GroupBox();
            this.chkAutoDonate = new System.Windows.Forms.CheckBox();
            this.chkDonateFilter = new System.Windows.Forms.CheckBox();
            this.txtDonateNames = new System.Windows.Forms.TextBox();

            this.grpBuff = new System.Windows.Forms.GroupBox();
            this.chkAutoBuffMaster = new System.Windows.Forms.CheckBox();
            this.lblMasterHp = new System.Windows.Forms.Label();
            this.numMasterHpUnder = new System.Windows.Forms.NumericUpDown();
            this.lblMasterKi = new System.Windows.Forms.Label();
            this.numMasterKiUnder = new System.Windows.Forms.NumericUpDown();

            this.chkAutoBuffPet = new System.Windows.Forms.CheckBox();
            this.lblPetHp = new System.Windows.Forms.Label();
            this.numPetHpUnder = new System.Windows.Forms.NumericUpDown();
            this.lblPetKi = new System.Windows.Forms.Label();
            this.numPetKiUnder = new System.Windows.Forms.NumericUpDown();

            this.grpRequest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRequestIfUnder)).BeginInit();
            
            this.grpDonate.SuspendLayout();

            this.grpBuff.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMasterHpUnder)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMasterKiUnder)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPetHpUnder)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPetKiUnder)).BeginInit();
            
            this.SuspendLayout();

            // ─── grpRequest ───────────────────────────────────────────────────
            this.grpRequest.Controls.Add(this.chkAutoRequest);
            this.grpRequest.Controls.Add(this.chkRequestCondition);
            this.grpRequest.Controls.Add(this.numRequestIfUnder);
            this.grpRequest.Location = new System.Drawing.Point(6, 6);
            this.grpRequest.Name = "grpRequest";
            this.grpRequest.Size = new System.Drawing.Size(380, 90);
            this.grpRequest.TabIndex = 0;
            this.grpRequest.TabStop = false;
            this.grpRequest.Text = "Xin đậu";

            this.chkAutoRequest.AutoSize = true;
            this.chkAutoRequest.Location = new System.Drawing.Point(12, 25);
            this.chkAutoRequest.Name = "chkAutoRequest";
            this.chkAutoRequest.Text = "Auto xin đậu (tự xin mỗi 310 giây)";

            this.chkRequestCondition.AutoSize = true;
            this.chkRequestCondition.Location = new System.Drawing.Point(12, 55);
            this.chkRequestCondition.Name = "chkRequestCondition";
            this.chkRequestCondition.Text = "Xin khi đậu trong hành trang dưới X viên:";
            
            this.numRequestIfUnder.Location = new System.Drawing.Point(290, 53);
            this.numRequestIfUnder.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            this.numRequestIfUnder.Name = "numRequestIfUnder";
            this.numRequestIfUnder.Size = new System.Drawing.Size(60, 23);

            // ─── grpDonate ────────────────────────────────────────────────────
            this.grpDonate.Controls.Add(this.chkAutoDonate);
            this.grpDonate.Controls.Add(this.chkDonateFilter);
            this.grpDonate.Controls.Add(this.txtDonateNames);
            this.grpDonate.Location = new System.Drawing.Point(6, 102);
            this.grpDonate.Name = "grpDonate";
            this.grpDonate.Size = new System.Drawing.Size(380, 140);
            this.grpDonate.TabIndex = 1;
            this.grpDonate.TabStop = false;
            this.grpDonate.Text = "Cho đậu";

            this.chkAutoDonate.AutoSize = true;
            this.chkAutoDonate.Location = new System.Drawing.Point(12, 25);
            this.chkAutoDonate.Name = "chkAutoDonate";
            this.chkAutoDonate.Text = "Tự động cho đậu";

            this.chkDonateFilter.AutoSize = true;
            this.chkDonateFilter.Location = new System.Drawing.Point(12, 55);
            this.chkDonateFilter.Name = "chkDonateFilter";
            this.chkDonateFilter.Text = "Chỉ cho những tên thành viên sau (mỗi tên m\u1ED9t dòng):";

            this.txtDonateNames.Location = new System.Drawing.Point(32, 80);
            this.txtDonateNames.Multiline = true;
            this.txtDonateNames.Name = "txtDonateNames";
            this.txtDonateNames.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDonateNames.Size = new System.Drawing.Size(330, 50);

            // ─── grpBuff ──────────────────────────────────────────────────────
            this.grpBuff.Controls.Add(this.chkAutoBuffMaster);
            this.grpBuff.Controls.Add(this.lblMasterHp);
            this.grpBuff.Controls.Add(this.numMasterHpUnder);
            this.grpBuff.Controls.Add(this.lblMasterKi);
            this.grpBuff.Controls.Add(this.numMasterKiUnder);
            this.grpBuff.Controls.Add(this.chkAutoBuffPet);
            this.grpBuff.Controls.Add(this.lblPetHp);
            this.grpBuff.Controls.Add(this.numPetHpUnder);
            this.grpBuff.Controls.Add(this.lblPetKi);
            this.grpBuff.Controls.Add(this.numPetKiUnder);
            this.grpBuff.Location = new System.Drawing.Point(6, 248);
            this.grpBuff.Name = "grpBuff";
            this.grpBuff.Size = new System.Drawing.Size(380, 150);
            this.grpBuff.TabIndex = 2;
            this.grpBuff.TabStop = false;
            this.grpBuff.Text = "Auto buff đậu";

            // Master Buff
            this.chkAutoBuffMaster.AutoSize = true;
            this.chkAutoBuffMaster.Location = new System.Drawing.Point(12, 25);
            this.chkAutoBuffMaster.Name = "chkAutoBuffMaster";
            this.chkAutoBuffMaster.Text = "Tự dùng buff đậu cho sư phụ";

            this.lblMasterHp.AutoSize = true;
            this.lblMasterHp.Location = new System.Drawing.Point(35, 55);
            this.lblMasterHp.Name = "lblMasterHp";
            this.lblMasterHp.Text = "Dưới (%) HP:";

            this.numMasterHpUnder.Location = new System.Drawing.Point(140, 53);
            this.numMasterHpUnder.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numMasterHpUnder.Name = "numMasterHpUnder";
            this.numMasterHpUnder.Size = new System.Drawing.Size(50, 23);

            this.lblMasterKi.AutoSize = true;
            this.lblMasterKi.Location = new System.Drawing.Point(210, 55);
            this.lblMasterKi.Name = "lblMasterKi";
            this.lblMasterKi.Text = "Dưới (%) KI:";

            this.numMasterKiUnder.Location = new System.Drawing.Point(310, 53);
            this.numMasterKiUnder.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numMasterKiUnder.Name = "numMasterKiUnder";
            this.numMasterKiUnder.Size = new System.Drawing.Size(50, 23);

            // Pet Buff
            this.chkAutoBuffPet.AutoSize = true;
            this.chkAutoBuffPet.Location = new System.Drawing.Point(12, 85);
            this.chkAutoBuffPet.Name = "chkAutoBuffPet";
            this.chkAutoBuffPet.Text = "Tự dùng buff đậu cho đệ tử";

            this.lblPetHp.AutoSize = true;
            this.lblPetHp.Location = new System.Drawing.Point(35, 115);
            this.lblPetHp.Name = "lblPetHp";
            this.lblPetHp.Text = "Dưới (%) HP:";

            this.numPetHpUnder.Location = new System.Drawing.Point(140, 113);
            this.numPetHpUnder.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numPetHpUnder.Name = "numPetHpUnder";
            this.numPetHpUnder.Size = new System.Drawing.Size(50, 23);

            this.lblPetKi.AutoSize = true;
            this.lblPetKi.Location = new System.Drawing.Point(210, 115);
            this.lblPetKi.Name = "lblPetKi";
            this.lblPetKi.Text = "Dưới (%) KI:";

            this.numPetKiUnder.Location = new System.Drawing.Point(310, 113);
            this.numPetKiUnder.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numPetKiUnder.Name = "numPetKiUnder";
            this.numPetKiUnder.Size = new System.Drawing.Size(50, 23);

            // ─── DauThanControl ───────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.grpRequest);
            this.Controls.Add(this.grpDonate);
            this.Controls.Add(this.grpBuff);
            this.Name = "DauThanControl";
            this.Size = new System.Drawing.Size(400, 420);

            this.grpRequest.ResumeLayout(false);
            this.grpRequest.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRequestIfUnder)).EndInit();

            this.grpDonate.ResumeLayout(false);
            this.grpDonate.PerformLayout();

            this.grpBuff.ResumeLayout(false);
            this.grpBuff.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMasterHpUnder)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMasterKiUnder)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPetHpUnder)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPetKiUnder)).EndInit();

            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpRequest;
        private System.Windows.Forms.CheckBox chkAutoRequest;
        private System.Windows.Forms.CheckBox chkRequestCondition;
        private System.Windows.Forms.NumericUpDown numRequestIfUnder;

        private System.Windows.Forms.GroupBox grpDonate;
        private System.Windows.Forms.CheckBox chkAutoDonate;
        private System.Windows.Forms.CheckBox chkDonateFilter;
        private System.Windows.Forms.TextBox txtDonateNames;

        private System.Windows.Forms.GroupBox grpBuff;
        private System.Windows.Forms.CheckBox chkAutoBuffMaster;
        private System.Windows.Forms.Label lblMasterHp;
        private System.Windows.Forms.NumericUpDown numMasterHpUnder;
        private System.Windows.Forms.Label lblMasterKi;
        private System.Windows.Forms.NumericUpDown numMasterKiUnder;

        private System.Windows.Forms.CheckBox chkAutoBuffPet;
        private System.Windows.Forms.Label lblPetHp;
        private System.Windows.Forms.NumericUpDown numPetHpUnder;
        private System.Windows.Forms.Label lblPetKi;
        private System.Windows.Forms.NumericUpDown numPetKiUnder;
    }
}
