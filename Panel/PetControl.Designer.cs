namespace Panel
{
    partial class PetControl
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.grpAutoFeatures = new System.Windows.Forms.GroupBox();
            this.grpStopConditions = new System.Windows.Forms.GroupBox();
            this.chkEnableAutoPet = new System.Windows.Forms.CheckBox();
            this.chkAutoPemWhenPetCall = new System.Windows.Forms.CheckBox();
            this.chkAutoKOK = new System.Windows.Forms.CheckBox();
            this.chkAutoTTNL = new System.Windows.Forms.CheckBox();
            this.lblTTNLPercent = new System.Windows.Forms.Label();
            this.numTTNLPercent = new System.Windows.Forms.NumericUpDown();
            this.chkAutoHealing = new System.Windows.Forms.CheckBox();
            this.chkAutoFocusPet = new System.Windows.Forms.CheckBox();
            this.chkAutoStopAtPower = new System.Windows.Forms.CheckBox();
            this.numTargetPower = new System.Windows.Forms.NumericUpDown();
            this.chkAutoJump = new System.Windows.Forms.CheckBox();
            this.chkAutoUsePetBuff = new System.Windows.Forms.CheckBox();

            this.grpGoback = new System.Windows.Forms.GroupBox();
            this.chkAutoGobackMap = new System.Windows.Forms.CheckBox();
            this.numTargetMapId = new System.Windows.Forms.NumericUpDown();
            this.chkAutoGobackZone = new System.Windows.Forms.CheckBox();
            this.numTargetZoneId = new System.Windows.Forms.NumericUpDown();
            this.chkAutoGobackPosition = new System.Windows.Forms.CheckBox();
            this.numTargetX = new System.Windows.Forms.NumericUpDown();
            this.numTargetY = new System.Windows.Forms.NumericUpDown();
            this.btnGetLocation = new System.Windows.Forms.Button();

            this.grpAutoFeatures.SuspendLayout();
            this.grpStopConditions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTTNLPercent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetPower)).BeginInit();
            this.grpGoback.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetMapId)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetZoneId)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetY)).BeginInit();
            this.SuspendLayout();
            // 
            // grpAutoFeatures
            // 
            this.grpAutoFeatures.Controls.Add(this.chkAutoPemWhenPetCall);
            this.grpAutoFeatures.Controls.Add(this.chkAutoKOK);
            this.grpAutoFeatures.Controls.Add(this.chkAutoTTNL);
            this.grpAutoFeatures.Controls.Add(this.lblTTNLPercent);
            this.grpAutoFeatures.Controls.Add(this.numTTNLPercent);
            this.grpAutoFeatures.Controls.Add(this.chkAutoHealing);
            this.grpAutoFeatures.Controls.Add(this.chkAutoFocusPet);
            this.grpAutoFeatures.Controls.Add(this.chkAutoJump);
            this.grpAutoFeatures.Controls.Add(this.chkAutoUsePetBuff);
            this.grpAutoFeatures.Location = new System.Drawing.Point(10, 200);
            this.grpAutoFeatures.Name = "grpAutoFeatures";
            this.grpAutoFeatures.Size = new System.Drawing.Size(400, 170);
            this.grpAutoFeatures.TabIndex = 2;
            this.grpAutoFeatures.TabStop = false;
            this.grpAutoFeatures.Text = "Cách úp đệ";
            // 
            // grpStopConditions
            // 
            this.grpStopConditions.Controls.Add(this.chkAutoStopAtPower);
            this.grpStopConditions.Controls.Add(this.numTargetPower);
            this.grpStopConditions.Location = new System.Drawing.Point(10, 380);
            this.grpStopConditions.Name = "grpStopConditions";
            this.grpStopConditions.Size = new System.Drawing.Size(400, 70);
            this.grpStopConditions.TabIndex = 3;
            this.grpStopConditions.TabStop = false;
            this.grpStopConditions.Text = "Điều kiện dừng";
            // 
            // chkEnableAutoPet
            // 
            this.chkEnableAutoPet.AutoSize = true;
            this.chkEnableAutoPet.Location = new System.Drawing.Point(15, 10);
            this.chkEnableAutoPet.Name = "chkEnableAutoPet";
            this.chkEnableAutoPet.Size = new System.Drawing.Size(125, 19);
            this.chkEnableAutoPet.TabIndex = 0;
            this.chkEnableAutoPet.Text = "Bật Auto Úp Đệ";
            this.chkEnableAutoPet.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.chkEnableAutoPet.UseVisualStyleBackColor = true;
            // 
            // chkAutoPemWhenPetCall
            // 
            this.chkAutoPemWhenPetCall.AutoSize = true;
            this.chkAutoPemWhenPetCall.Location = new System.Drawing.Point(20, 30);
            this.chkAutoPemWhenPetCall.Name = "chkAutoPemWhenPetCall";
            this.chkAutoPemWhenPetCall.Size = new System.Drawing.Size(161, 19);
            this.chkAutoPemWhenPetCall.TabIndex = 1;
            this.chkAutoPemWhenPetCall.Text = "Tự động pem khi đệ sủa";
            this.chkAutoPemWhenPetCall.UseVisualStyleBackColor = true;
            // 
            // chkAutoKOK
            // 
            this.chkAutoKOK.AutoSize = true;
            this.chkAutoKOK.Location = new System.Drawing.Point(220, 30);
            this.chkAutoKOK.Name = "chkAutoKOK";
            this.chkAutoKOK.Size = new System.Drawing.Size(117, 19);
            this.chkAutoKOK.TabIndex = 2;
            this.chkAutoKOK.Text = "Auto up đệ KOK";
            this.chkAutoKOK.UseVisualStyleBackColor = true;
            // 
            // chkAutoTTNL
            // 
            this.chkAutoTTNL.AutoSize = true;
            this.chkAutoTTNL.Location = new System.Drawing.Point(220, 60);
            this.chkAutoTTNL.Name = "chkAutoTTNL";
            this.chkAutoTTNL.Size = new System.Drawing.Size(89, 19);
            this.chkAutoTTNL.TabIndex = 3;
            this.chkAutoTTNL.Text = "Auto TTNL";
            this.chkAutoTTNL.UseVisualStyleBackColor = true;
            // 
            // lblTTNLPercent
            // 
            this.lblTTNLPercent.AutoSize = true;
            this.lblTTNLPercent.Location = new System.Drawing.Point(220, 92);
            this.lblTTNLPercent.Name = "lblTTNLPercent";
            this.lblTTNLPercent.Size = new System.Drawing.Size(89, 15);
            this.lblTTNLPercent.TabIndex = 4;
            this.lblTTNLPercent.Text = "% HP/KI TTNL:";
            // 
            // numTTNLPercent
            // 
            this.numTTNLPercent.Location = new System.Drawing.Point(315, 89);
            this.numTTNLPercent.Minimum = new decimal(new int[]{ 1, 0, 0, 0 });
            this.numTTNLPercent.Maximum = new decimal(new int[]{ 100, 0, 0, 0 });
            this.numTTNLPercent.Name = "numTTNLPercent";
            this.numTTNLPercent.Size = new System.Drawing.Size(43, 23);
            this.numTTNLPercent.TabIndex = 5;
            this.numTTNLPercent.Value = new decimal(new int[]{ 15, 0, 0, 0 });
            // 
            // chkAutoHealing
            // 
            this.chkAutoHealing.AutoSize = true;
            this.chkAutoHealing.Location = new System.Drawing.Point(20, 60);
            this.chkAutoHealing.Name = "chkAutoHealing";
            this.chkAutoHealing.Size = new System.Drawing.Size(149, 19);
            this.chkAutoHealing.TabIndex = 6;
            this.chkAutoHealing.Text = "Auto trị thương (Namek)";
            this.chkAutoHealing.UseVisualStyleBackColor = true;
            // 
            // chkAutoFocusPet
            // 
            this.chkAutoFocusPet.AutoSize = true;
            this.chkAutoFocusPet.Location = new System.Drawing.Point(20, 90);
            this.chkAutoFocusPet.Name = "chkAutoFocusPet";
            this.chkAutoFocusPet.Size = new System.Drawing.Size(102, 19);
            this.chkAutoFocusPet.TabIndex = 7;
            this.chkAutoFocusPet.Text = "Auto Gim Đệ";
            this.chkAutoFocusPet.UseVisualStyleBackColor = true;
            // 
            // chkAutoStopAtPower
            // 
            this.chkAutoStopAtPower.AutoSize = true;
            this.chkAutoStopAtPower.Location = new System.Drawing.Point(20, 30);
            this.chkAutoStopAtPower.Name = "chkAutoStopAtPower";
            this.chkAutoStopAtPower.Size = new System.Drawing.Size(120, 19);
            this.chkAutoStopAtPower.TabIndex = 8;
            this.chkAutoStopAtPower.Text = "Dừng khi đệ đạt SM";
            this.chkAutoStopAtPower.UseVisualStyleBackColor = true;
            // 
            // numTargetPower
            // 
            this.numTargetPower.Location = new System.Drawing.Point(170, 28);
            this.numTargetPower.Maximum = new decimal(new int[]{ 999000000, 0, 0, 0 });
            this.numTargetPower.Minimum = new decimal(new int[]{ 100000, 0, 0, 0 });
            this.numTargetPower.Name = "numTargetPower";
            this.numTargetPower.Size = new System.Drawing.Size(120, 23);
            this.numTargetPower.TabIndex = 9;
            this.numTargetPower.Value = new decimal(new int[]{ 149000000, 0, 0, 0 });
            // 
            // chkAutoJump
            // 
            this.chkAutoJump.AutoSize = true;
            this.chkAutoJump.Location = new System.Drawing.Point(220, 120);
            this.chkAutoJump.Name = "chkAutoJump";
            this.chkAutoJump.Size = new System.Drawing.Size(102, 19);
            this.chkAutoJump.TabIndex = 10;
            this.chkAutoJump.Text = "Auto Nhảy";
            this.chkAutoJump.UseVisualStyleBackColor = true;
            // 
            // chkAutoUsePetBuff
            // 
            this.chkAutoUsePetBuff.AutoSize = true;
            this.chkAutoUsePetBuff.Location = new System.Drawing.Point(20, 120);
            this.chkAutoUsePetBuff.Name = "chkAutoUsePetBuff";
            this.chkAutoUsePetBuff.Size = new System.Drawing.Size(150, 19);
            this.chkAutoUsePetBuff.TabIndex = 11;
            this.chkAutoUsePetBuff.Text = "Auto bùa santa";
            this.chkAutoUsePetBuff.UseVisualStyleBackColor = true;
            // 
            // grpGoback
            // 
            this.grpGoback.Controls.Add(this.chkAutoGobackMap);
            this.grpGoback.Controls.Add(this.numTargetMapId);
            this.grpGoback.Controls.Add(this.chkAutoGobackZone);
            this.grpGoback.Controls.Add(this.numTargetZoneId);
            this.grpGoback.Controls.Add(this.chkAutoGobackPosition);
            this.grpGoback.Controls.Add(this.numTargetX);
            this.grpGoback.Controls.Add(this.numTargetY);
            this.grpGoback.Controls.Add(this.btnGetLocation);
            this.grpGoback.Location = new System.Drawing.Point(10, 40);
            this.grpGoback.Name = "grpGoback";
            this.grpGoback.Size = new System.Drawing.Size(400, 150);
            this.grpGoback.TabIndex = 1;
            this.grpGoback.TabStop = false;
            this.grpGoback.Text = "Vị trí";
            // 
            // chkAutoGobackMap
            // 
            this.chkAutoGobackMap.AutoSize = true;
            this.chkAutoGobackMap.Location = new System.Drawing.Point(20, 30);
            this.chkAutoGobackMap.Name = "chkAutoGobackMap";
            this.chkAutoGobackMap.Size = new System.Drawing.Size(95, 19);
            this.chkAutoGobackMap.Text = "Map";
            this.chkAutoGobackMap.UseVisualStyleBackColor = true;
            // 
            // numTargetMapId
            // 
            this.numTargetMapId.Location = new System.Drawing.Point(140, 28);
            this.numTargetMapId.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            this.numTargetMapId.Minimum = new decimal(new int[] { 1, 0, 0, -2147483648 });
            this.numTargetMapId.Name = "numTargetMapId";
            this.numTargetMapId.Size = new System.Drawing.Size(80, 23);
            // 
            // chkAutoGobackZone
            // 
            this.chkAutoGobackZone.AutoSize = true;
            this.chkAutoGobackZone.Location = new System.Drawing.Point(20, 70);
            this.chkAutoGobackZone.Name = "chkAutoGobackZone";
            this.chkAutoGobackZone.Size = new System.Drawing.Size(95, 19);
            this.chkAutoGobackZone.Text = "Khu";
            this.chkAutoGobackZone.UseVisualStyleBackColor = true;
            // 
            // numTargetZoneId
            // 
            this.numTargetZoneId.Location = new System.Drawing.Point(140, 68);
            this.numTargetZoneId.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            this.numTargetZoneId.Minimum = new decimal(new int[] { 1, 0, 0, -2147483648 });
            this.numTargetZoneId.Name = "numTargetZoneId";
            this.numTargetZoneId.Size = new System.Drawing.Size(80, 23);
            // 
            // chkAutoGobackPosition
            // 
            this.chkAutoGobackPosition.AutoSize = true;
            this.chkAutoGobackPosition.Location = new System.Drawing.Point(20, 110);
            this.chkAutoGobackPosition.Name = "chkAutoGobackPosition";
            this.chkAutoGobackPosition.Size = new System.Drawing.Size(107, 19);
            this.chkAutoGobackPosition.Text = "Tọa độ";
            this.chkAutoGobackPosition.UseVisualStyleBackColor = true;
            // 
            // numTargetX
            // 
            this.numTargetX.Location = new System.Drawing.Point(140, 108);
            this.numTargetX.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            this.numTargetX.Minimum = new decimal(new int[] { 1, 0, 0, -2147483648 });
            this.numTargetX.Name = "numTargetX";
            this.numTargetX.Size = new System.Drawing.Size(50, 23);
            // 
            // numTargetY
            // 
            this.numTargetY.Location = new System.Drawing.Point(200, 108);
            this.numTargetY.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            this.numTargetY.Minimum = new decimal(new int[] { 1, 0, 0, -2147483648 });
            this.numTargetY.Name = "numTargetY";
            this.numTargetY.Size = new System.Drawing.Size(50, 23);
            // 
            // btnGetLocation
            // 
            this.btnGetLocation.Location = new System.Drawing.Point(260, 28);
            this.btnGetLocation.Name = "btnGetLocation";
            this.btnGetLocation.Size = new System.Drawing.Size(120, 40);
            this.btnGetLocation.Text = "Lấy tọa độ hiện tại";
            this.btnGetLocation.UseVisualStyleBackColor = true;

            // 
            // 
            // PetControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.chkEnableAutoPet);
            this.Controls.Add(this.grpGoback);
            this.Controls.Add(this.grpAutoFeatures);
            this.Controls.Add(this.grpStopConditions);
            this.Name = "PetControl";
            this.Size = new System.Drawing.Size(420, 460);
            this.grpAutoFeatures.ResumeLayout(false);
            this.grpAutoFeatures.PerformLayout();
            this.grpStopConditions.ResumeLayout(false);
            this.grpStopConditions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTTNLPercent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetPower)).EndInit();
            this.grpGoback.ResumeLayout(false);
            this.grpGoback.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetMapId)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetZoneId)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetY)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox grpAutoFeatures;
        private System.Windows.Forms.GroupBox grpStopConditions;
        private System.Windows.Forms.CheckBox chkEnableAutoPet;
        private System.Windows.Forms.CheckBox chkAutoPemWhenPetCall;
        private System.Windows.Forms.CheckBox chkAutoKOK;
        private System.Windows.Forms.CheckBox chkAutoTTNL;
        private System.Windows.Forms.Label lblTTNLPercent;
        private System.Windows.Forms.NumericUpDown numTTNLPercent;
        private System.Windows.Forms.CheckBox chkAutoHealing;
        private System.Windows.Forms.CheckBox chkAutoFocusPet;
        private System.Windows.Forms.CheckBox chkAutoStopAtPower;
        private System.Windows.Forms.NumericUpDown numTargetPower;
        private System.Windows.Forms.CheckBox chkAutoJump;
        private System.Windows.Forms.CheckBox chkAutoUsePetBuff;

        private System.Windows.Forms.GroupBox grpGoback;
        private System.Windows.Forms.CheckBox chkAutoGobackMap;
        public System.Windows.Forms.NumericUpDown numTargetMapId;
        private System.Windows.Forms.CheckBox chkAutoGobackZone;
        public System.Windows.Forms.NumericUpDown numTargetZoneId;
        private System.Windows.Forms.CheckBox chkAutoGobackPosition;
        public System.Windows.Forms.NumericUpDown numTargetX;
        public System.Windows.Forms.NumericUpDown numTargetY;
        public System.Windows.Forms.Button btnGetLocation;
    }
}
