namespace Panel
{
    partial class BuffNamekControl
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
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabBasic = new System.Windows.Forms.TabPage();
            this.tabReducePower = new System.Windows.Forms.TabPage();
            
            this.chkEnabled         = new System.Windows.Forms.CheckBox();
            this.lblStatus          = new System.Windows.Forms.Label();

            this.grpPosition        = new System.Windows.Forms.GroupBox();
            this.lblMap             = new System.Windows.Forms.Label();
            this.cboMapId           = new System.Windows.Forms.ComboBox();
            this.chkRequireZone     = new System.Windows.Forms.CheckBox();
            this.nudZoneId          = new System.Windows.Forms.NumericUpDown();
            this.lblZoneNote        = new System.Windows.Forms.Label();
            this.chkRequirePosition = new System.Windows.Forms.CheckBox();
            this.lblPosX            = new System.Windows.Forms.Label();
            this.nudPosX            = new System.Windows.Forms.NumericUpDown();
            this.lblPosY            = new System.Windows.Forms.Label();
            this.nudPosY            = new System.Windows.Forms.NumericUpDown();
            this.btnGetPos          = new System.Windows.Forms.Button();

            this.grpBuffTarget      = new System.Windows.Forms.GroupBox();
            this.cboBuffTargetMode  = new System.Windows.Forms.ComboBox();
            this.lblBuffTargetNote  = new System.Windows.Forms.Label();

            this.grpTargetSettings  = new System.Windows.Forms.GroupBox();
            this.lblCondition       = new System.Windows.Forms.Label();
            this.cboBuffCondition   = new System.Windows.Forms.ComboBox();
            this.pnlHpThreshold     = new System.Windows.Forms.Panel();
            this.lblHp              = new System.Windows.Forms.Label();
            this.nudHpThreshold     = new System.Windows.Forms.NumericUpDown();
            this.lblHpUnit          = new System.Windows.Forms.Label();
            this.lblRangeMode       = new System.Windows.Forms.Label();
            this.cboBuffRangeMode   = new System.Windows.Forms.ComboBox();
            this.lblRangeWarning    = new System.Windows.Forms.Label();
            this.lblTargetNames     = new System.Windows.Forms.Label();
            this.txtTargetNames     = new System.Windows.Forms.TextBox();
            this.lblTargetHint      = new System.Windows.Forms.Label();

            this.chkRpEnabled = new System.Windows.Forms.CheckBox();
            this.grpRpPosition = new System.Windows.Forms.GroupBox();
            this.lblRpMap = new System.Windows.Forms.Label();
            this.cboRpMapId = new System.Windows.Forms.ComboBox();
            this.lblRpZone = new System.Windows.Forms.Label();
            this.nudRpZoneId = new System.Windows.Forms.NumericUpDown();
            this.lblRpX = new System.Windows.Forms.Label();
            this.nudRpX = new System.Windows.Forms.NumericUpDown();
            this.lblRpY = new System.Windows.Forms.Label();
            this.nudRpY = new System.Windows.Forms.NumericUpDown();
            this.lblRpProvokeCount = new System.Windows.Forms.Label();
            this.nudRpProvokeCount = new System.Windows.Forms.NumericUpDown();
            this.lblRpDeadDelayMs = new System.Windows.Forms.Label();
            this.nudRpDeadDelayMs = new System.Windows.Forms.NumericUpDown();
            this.btnRpGetPos = new System.Windows.Forms.Button();
            this.chkRpAutoPunchBlackFlag = new System.Windows.Forms.CheckBox();
            this.chkRpUseHpPunch = new System.Windows.Forms.CheckBox();
            this.nudRpHpPunch = new System.Windows.Forms.NumericUpDown();
            this.lblRpHpPunch = new System.Windows.Forms.Label();
            this.chkRpUseTdlt = new System.Windows.Forms.CheckBox();

            this.tabControlMain.SuspendLayout();
            this.tabBasic.SuspendLayout();
            this.tabReducePower.SuspendLayout();
            this.grpPosition.SuspendLayout();
            this.grpBuffTarget.SuspendLayout();
            this.grpTargetSettings.SuspendLayout();
            this.pnlHpThreshold.SuspendLayout();
            this.grpRpPosition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudZoneId)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPosX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPosY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudHpThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRpZoneId)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRpX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRpY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRpProvokeCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRpDeadDelayMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRpHpPunch)).BeginInit();
            this.SuspendLayout();

            // tabControlMain
            this.tabControlMain.Controls.Add(this.tabBasic);
            this.tabControlMain.Controls.Add(this.tabReducePower);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(465, 310);
            this.tabControlMain.TabIndex = 0;

            // tabBasic
            this.tabBasic.Controls.Add(this.chkEnabled);
            this.tabBasic.Controls.Add(this.lblStatus);
            this.tabBasic.Controls.Add(this.grpPosition);
            this.tabBasic.Controls.Add(this.grpBuffTarget);
            this.tabBasic.Controls.Add(this.grpTargetSettings);
            this.tabBasic.Location = new System.Drawing.Point(4, 24);
            this.tabBasic.Name = "tabBasic";
            this.tabBasic.Padding = new System.Windows.Forms.Padding(3);
            this.tabBasic.Size = new System.Drawing.Size(457, 282);
            this.tabBasic.TabIndex = 0;
            this.tabBasic.Text = "Hồi Sinh (Cơ bản)";
            this.tabBasic.UseVisualStyleBackColor = true;

            // tabReducePower
            this.tabReducePower.Controls.Add(this.chkRpEnabled);
            this.tabReducePower.Controls.Add(this.grpRpPosition);
            this.tabReducePower.Location = new System.Drawing.Point(4, 24);
            this.tabReducePower.Name = "tabReducePower";
            this.tabReducePower.Padding = new System.Windows.Forms.Padding(3);
            this.tabReducePower.Size = new System.Drawing.Size(457, 330);
            this.tabReducePower.TabIndex = 1;
            this.tabReducePower.Text = "Acc Chết (Giảm SM)";
            this.tabReducePower.UseVisualStyleBackColor = true;

            // ─── Row 0: Enable + Status ───────────────────────────────────
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.chkEnabled.Location = new System.Drawing.Point(8, 6);
            this.chkEnabled.Name     = "chkEnabled";
            this.chkEnabled.Text     = "Bật Auto Namek Hồi Sinh";
            this.chkEnabled.TabIndex = 0;

            this.lblStatus.AutoSize  = false;
            this.lblStatus.Location  = new System.Drawing.Point(185, 7);
            this.lblStatus.Size      = new System.Drawing.Size(260, 16);
            this.lblStatus.Name      = "lblStatus";
            this.lblStatus.Text      = "";
            this.lblStatus.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblStatus.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic);

            // ─── COT TRAI: grpPosition  width=240, height=162 ─────────────
            this.grpPosition.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                this.lblMap, this.cboMapId,
                this.chkRequireZone, this.nudZoneId, this.lblZoneNote,
                this.chkRequirePosition,
                this.lblPosX, this.nudPosX, this.lblPosY, this.nudPosY,
                this.btnGetPos
            });
            this.grpPosition.Location = new System.Drawing.Point(6, 28);
            this.grpPosition.Name     = "grpPosition";
            this.grpPosition.Size     = new System.Drawing.Size(230, 162);
            this.grpPosition.TabStop  = false;
            this.grpPosition.Text     = "Vị trí đứng của Namek";

            // Map combo
            this.lblMap.AutoSize = true;
            this.lblMap.Location = new System.Drawing.Point(8, 22);
            this.lblMap.Text     = "Map:";

            this.cboMapId.Location      = new System.Drawing.Point(45, 19);
            this.cboMapId.Size          = new System.Drawing.Size(180, 23);
            this.cboMapId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cboMapId.Name          = "cboMapId";
            this.cboMapId.TabIndex      = 1;

            // Zone
            this.chkRequireZone.AutoSize = true;
            this.chkRequireZone.Location = new System.Drawing.Point(8, 51);
            this.chkRequireZone.Name     = "chkRequireZone";
            this.chkRequireZone.Text     = "Khu:";
            this.chkRequireZone.TabIndex = 2;

            this.nudZoneId.Location = new System.Drawing.Point(55, 49);
            this.nudZoneId.Size     = new System.Drawing.Size(45, 23);
            this.nudZoneId.Maximum  = 99;
            this.nudZoneId.Minimum  = 0;
            this.nudZoneId.Name     = "nudZoneId";
            this.nudZoneId.TabIndex = 3;

            this.lblZoneNote.AutoSize  = true;
            this.lblZoneNote.Location  = new System.Drawing.Point(105, 53);
            this.lblZoneNote.ForeColor = System.Drawing.Color.Gray;
            this.lblZoneNote.Font      = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lblZoneNote.Text      = "";

            // Toa do
            this.chkRequirePosition.AutoSize = true;
            this.chkRequirePosition.Location = new System.Drawing.Point(8, 81);
            this.chkRequirePosition.Name     = "chkRequirePosition";
            this.chkRequirePosition.Text     = "Toạ độ đứng:";
            this.chkRequirePosition.TabIndex = 4;

            this.lblPosX.AutoSize = true;
            this.lblPosX.Location = new System.Drawing.Point(8, 109);
            this.lblPosX.Text     = "X:";

            this.nudPosX.Location = new System.Drawing.Point(30, 106);
            this.nudPosX.Size     = new System.Drawing.Size(65, 23);
            this.nudPosX.Maximum  = 9999;
            this.nudPosX.Name     = "nudPosX";
            this.nudPosX.TabIndex = 5;

            this.lblPosY.AutoSize = true;
            this.lblPosY.Location = new System.Drawing.Point(100, 109);
            this.lblPosY.Text     = "Y:";

            this.nudPosY.Location = new System.Drawing.Point(120, 106);
            this.nudPosY.Size     = new System.Drawing.Size(65, 23);
            this.nudPosY.Maximum  = 9999;
            this.nudPosY.Name     = "nudPosY";
            this.nudPosY.TabIndex = 6;

            // Nut GET
            this.btnGetPos.Location = new System.Drawing.Point(8, 134);
            this.btnGetPos.Size     = new System.Drawing.Size(217, 22);
            this.btnGetPos.Name     = "btnGetPos";
            this.btnGetPos.Text     = "Lấy vị trí hiện tại đang đứng";
            this.btnGetPos.TabIndex = 7;

            // ─── COT TRAI: grpBuffTarget  width=240, height=80 ────────────
            this.grpBuffTarget.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                this.cboBuffTargetMode, this.lblBuffTargetNote
            });
            this.grpBuffTarget.Location = new System.Drawing.Point(6, 196);
            this.grpBuffTarget.Name     = "grpBuffTarget";
            this.grpBuffTarget.Size     = new System.Drawing.Size(230, 80);
            this.grpBuffTarget.TabStop  = false;
            this.grpBuffTarget.Text     = "Chế độ buff";

            this.cboBuffTargetMode.Location      = new System.Drawing.Point(8, 22);
            this.cboBuffTargetMode.Size          = new System.Drawing.Size(217, 23);
            this.cboBuffTargetMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBuffTargetMode.Name          = "cboBuffTargetMode";
            this.cboBuffTargetMode.Items.AddRange(new object[] {
            "Buff bản thân (+ xung quanh)",
            "Buff theo danh sách",
            "Buff giảm sức mạnh"});
            this.cboBuffTargetMode.Location = new System.Drawing.Point(6, 22);
            this.cboBuffTargetMode.TabIndex      = 8;

            this.lblBuffTargetNote.AutoSize  = false;
            this.lblBuffTargetNote.Location  = new System.Drawing.Point(8, 50);
            this.lblBuffTargetNote.Size      = new System.Drawing.Size(217, 24);
            this.lblBuffTargetNote.Name      = "lblBuffTargetNote";
            this.lblBuffTargetNote.ForeColor = System.Drawing.Color.DimGray;
            this.lblBuffTargetNote.Font      = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Italic);
            this.lblBuffTargetNote.Text      = "Đứng tại toạ độ trên, spam buff vào bản thân";

            // ─── COT PHAI: grpTargetSettings  x=242, width=205 ───────────
            this.grpTargetSettings.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                this.lblCondition, this.cboBuffCondition,
                this.pnlHpThreshold,
                this.lblRangeMode, this.cboBuffRangeMode,
                this.lblRangeWarning,
                this.lblTargetNames, this.txtTargetNames,
                this.lblTargetHint
            });
            this.grpTargetSettings.Location = new System.Drawing.Point(242, 28);
            this.grpTargetSettings.Name     = "grpTargetSettings";
            this.grpTargetSettings.Size     = new System.Drawing.Size(205, 248);
            this.grpTargetSettings.TabStop  = false;
            this.grpTargetSettings.Text     = "Cài đặt buff mục tiêu";

            // Dieu kien
            this.lblCondition.AutoSize = true;
            this.lblCondition.Location = new System.Drawing.Point(8, 24);
            this.lblCondition.Text     = "Điều kiện:";

            this.cboBuffCondition.Location      = new System.Drawing.Point(72, 21);
            this.cboBuffCondition.Size          = new System.Drawing.Size(125, 23);
            this.cboBuffCondition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBuffCondition.Name          = "cboBuffCondition";
            this.cboBuffCondition.Items.AddRange(new object[]
            {
                "Hồi CD là buff",
                "Khi target chết",
                "Khi HP% thấp hơn"
            });
            this.cboBuffCondition.SelectedIndex = 1;
            this.cboBuffCondition.TabIndex      = 9;

            // Nguong HP (an khi khong can)
            this.pnlHpThreshold.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                this.lblHp, this.nudHpThreshold, this.lblHpUnit
            });
            this.pnlHpThreshold.Location = new System.Drawing.Point(8, 50);
            this.pnlHpThreshold.Size     = new System.Drawing.Size(188, 26);
            this.pnlHpThreshold.Name     = "pnlHpThreshold";
            this.pnlHpThreshold.Visible  = false;

            this.lblHp.AutoSize = true;
            this.lblHp.Location = new System.Drawing.Point(0, 5);
            this.lblHp.Text     = "Target HP dưới:";

            this.nudHpThreshold.Location = new System.Drawing.Point(104, 2);
            this.nudHpThreshold.Size     = new System.Drawing.Size(52, 23);
            this.nudHpThreshold.Maximum  = 99;
            this.nudHpThreshold.Minimum  = 1;
            this.nudHpThreshold.Value    = 50;
            this.nudHpThreshold.Name     = "nudHpThreshold";
            this.nudHpThreshold.TabIndex = 10;

            this.lblHpUnit.AutoSize = true;
            this.lblHpUnit.Location = new System.Drawing.Point(160, 5);
            this.lblHpUnit.Text     = "%";

            // Di chuyển
            this.lblRangeMode.AutoSize = true;
            this.lblRangeMode.Location = new System.Drawing.Point(8, 108);
            this.lblRangeMode.Text     = "Kiểu buff:";

            this.cboBuffRangeMode.Location      = new System.Drawing.Point(72, 105);
            this.cboBuffRangeMode.Size          = new System.Drawing.Size(125, 23);
            this.cboBuffRangeMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBuffRangeMode.Name          = "cboBuffRangeMode";
            this.cboBuffRangeMode.Items.AddRange(new object[]
            {
                "Buff tại chỗ",
                "Tele tới + về"
            });
            this.cboBuffRangeMode.SelectedIndex = 0;
            this.cboBuffRangeMode.TabIndex      = 11;

            // Cảnh báo
            this.lblRangeWarning.AutoSize  = false;
            this.lblRangeWarning.Location  = new System.Drawing.Point(8, 134);
            this.lblRangeWarning.Size      = new System.Drawing.Size(188, 14);
            this.lblRangeWarning.Name      = "lblRangeWarning";
            this.lblRangeWarning.ForeColor = System.Drawing.Color.OrangeRed;
            this.lblRangeWarning.Font      = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Italic);
            this.lblRangeWarning.Text      = "";
            this.lblRangeWarning.Visible   = false;

            // Danh sách tên
            this.lblTargetNames.AutoSize = true;
            this.lblTargetNames.Location = new System.Drawing.Point(8, 153);
            this.lblTargetNames.Text     = "Tên InGame (mỗi dòng 1 tên):";

            this.txtTargetNames.Location   = new System.Drawing.Point(8, 170);
            this.txtTargetNames.Multiline  = true;
            this.txtTargetNames.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTargetNames.Size       = new System.Drawing.Size(188, 55);
            this.txtTargetNames.Name       = "txtTargetNames";
            this.txtTargetNames.TabIndex   = 12;

            this.lblTargetHint.AutoSize  = false;
            this.lblTargetHint.Location  = new System.Drawing.Point(8, 227);
            this.lblTargetHint.Size      = new System.Drawing.Size(188, 18);
            this.lblTargetHint.Name      = "lblTargetHint";
            this.lblTargetHint.ForeColor = System.Drawing.Color.Gray;
            this.lblTargetHint.Font      = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Italic);
            this.lblTargetHint.Text      = "Để trống = không buff ai.";

            // ─── TAB REDUCE POWER (ACC MOI) ──────────────────────────────────
            this.chkRpEnabled.AutoSize = true;
            this.chkRpEnabled.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.chkRpEnabled.Location = new System.Drawing.Point(12, 12);
            this.chkRpEnabled.Name     = "chkRpEnabled";
            this.chkRpEnabled.Text     = "Bật chế độ Giảm Sức Mạnh (Chết sẽ tự báo cứu)";
            this.chkRpEnabled.TabIndex = 0;

            this.chkRpAutoPunchBlackFlag.Location = new System.Drawing.Point(15, 215);
            this.chkRpAutoPunchBlackFlag.Size     = new System.Drawing.Size(200, 19);
            this.chkRpAutoPunchBlackFlag.Text     = "Auto đấm char cờ đen";
            this.chkRpAutoPunchBlackFlag.TabIndex = 13;

            // chkRpUseHpPunch
            this.chkRpUseHpPunch.AutoSize = true;
            this.chkRpUseHpPunch.Location = new System.Drawing.Point(15, 243);
            this.chkRpUseHpPunch.Name = "chkRpUseHpPunch";
            this.chkRpUseHpPunch.Size = new System.Drawing.Size(130, 20);
            this.chkRpUseHpPunch.Text = "Đấm khi HP trên:";
            this.chkRpUseHpPunch.TabIndex = 14;

            // nudRpHpPunch
            this.nudRpHpPunch.Location = new System.Drawing.Point(145, 241);
            this.nudRpHpPunch.Size = new System.Drawing.Size(50, 23);
            this.nudRpHpPunch.Minimum = 1;
            this.nudRpHpPunch.Maximum = 100;
            this.nudRpHpPunch.Value = 10;
            this.nudRpHpPunch.Name = "nudRpHpPunch";
            this.nudRpHpPunch.TabIndex = 15;

            // lblRpHpPunch
            this.lblRpHpPunch.AutoSize = true;
            this.lblRpHpPunch.Location = new System.Drawing.Point(197, 244);
            this.lblRpHpPunch.Text = "%";

            // chkRpUseTdlt
            this.chkRpUseTdlt.AutoSize = true;
            this.chkRpUseTdlt.Location = new System.Drawing.Point(15, 272);
            this.chkRpUseTdlt.Name = "chkRpUseTdlt";
            this.chkRpUseTdlt.Size = new System.Drawing.Size(150, 20);
            this.chkRpUseTdlt.Text = "Dùng TDLT";
            this.chkRpUseTdlt.TabIndex = 16;

            this.grpRpPosition.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                this.lblRpMap, this.cboRpMapId,
                this.lblRpZone, this.nudRpZoneId,
                this.lblRpX, this.nudRpX,
                this.lblRpY, this.nudRpY,
                this.lblRpProvokeCount, this.nudRpProvokeCount,
                this.lblRpDeadDelayMs, this.nudRpDeadDelayMs,
                this.btnRpGetPos,
                this.chkRpAutoPunchBlackFlag,
                this.chkRpUseHpPunch,
                this.nudRpHpPunch,
                this.lblRpHpPunch,
                this.chkRpUseTdlt
            });
            this.grpRpPosition.Location = new System.Drawing.Point(12, 45);
            this.grpRpPosition.Name     = "grpRpPosition";
            this.grpRpPosition.Size     = new System.Drawing.Size(350, 310);
            this.grpRpPosition.TabStop  = false;
            this.grpRpPosition.Text     = "Cài đặt vị trí chết (nơi đứng úp)";

            this.lblRpMap.AutoSize = true;
            this.lblRpMap.Location = new System.Drawing.Point(12, 28);
            this.lblRpMap.Text     = "Map:";
            
            this.cboRpMapId.Location = new System.Drawing.Point(60, 25);
            this.cboRpMapId.Size     = new System.Drawing.Size(180, 23);
            this.cboRpMapId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cboRpMapId.Name     = "cboRpMapId";

            this.lblRpZone.AutoSize = true;
            this.lblRpZone.Location = new System.Drawing.Point(12, 59);
            this.lblRpZone.Text     = "Khu:";

            this.nudRpZoneId.Location = new System.Drawing.Point(60, 56);
            this.nudRpZoneId.Size     = new System.Drawing.Size(50, 23);
            this.nudRpZoneId.Maximum  = 99;
            this.nudRpZoneId.Name     = "nudRpZoneId";

            this.lblRpX.AutoSize = true;
            this.lblRpX.Location = new System.Drawing.Point(12, 90);
            this.lblRpX.Text     = "Pos X:";

            this.nudRpX.Location = new System.Drawing.Point(70, 87);
            this.nudRpX.Size     = new System.Drawing.Size(60, 23);
            this.nudRpX.Maximum  = 9999;
            this.nudRpX.Name     = "nudRpX";

            this.lblRpY.AutoSize = true;
            this.lblRpY.Location = new System.Drawing.Point(135, 90);
            this.lblRpY.Text     = "Y:";

            this.nudRpY.Location = new System.Drawing.Point(155, 87);
            this.nudRpY.Size     = new System.Drawing.Size(60, 23);
            this.nudRpY.Maximum  = 9999;
            this.nudRpY.Name     = "nudRpY";

            this.lblRpProvokeCount.AutoSize = true;
            this.lblRpProvokeCount.Location = new System.Drawing.Point(12, 122);
            this.lblRpProvokeCount.Text     = "S\u1ed1 mob \u0111\u1ea5m/turn:";

            this.nudRpProvokeCount.Location = new System.Drawing.Point(155, 119);
            this.nudRpProvokeCount.Size     = new System.Drawing.Size(60, 23);
            this.nudRpProvokeCount.Minimum  = 0;
            this.nudRpProvokeCount.Maximum  = 20;
            this.nudRpProvokeCount.Value    = 1;
            this.nudRpProvokeCount.Name     = "nudRpProvokeCount";

            this.lblRpDeadDelayMs.AutoSize = true;
            this.lblRpDeadDelayMs.Location = new System.Drawing.Point(12, 153);
            this.lblRpDeadDelayMs.Text     = "\u0110\u1ed9 tr\u1ec5 g\u1ecdi c\u1ee9u (ms):";

            this.nudRpDeadDelayMs.Location = new System.Drawing.Point(155, 150);
            this.nudRpDeadDelayMs.Size     = new System.Drawing.Size(90, 23);
            this.nudRpDeadDelayMs.Minimum  = 0;
            this.nudRpDeadDelayMs.Maximum  = 10000;
            this.nudRpDeadDelayMs.Increment = 100;
            this.nudRpDeadDelayMs.Value    = 1000;
            this.nudRpDeadDelayMs.Name     = "nudRpDeadDelayMs";

            this.btnRpGetPos.Location = new System.Drawing.Point(15, 182);
            this.btnRpGetPos.Size     = new System.Drawing.Size(270, 25);
            this.btnRpGetPos.Name     = "btnRpGetPos";
            this.btnRpGetPos.Text     = "Lấy vị trí hiện tại";

            // ─── UserControl ──────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll          = true;
            this.Controls.Add(this.tabControlMain);

            this.Name = "BuffNamekControl";
            this.Size = new System.Drawing.Size(465, 360);

            this.tabControlMain.ResumeLayout(false);
            this.tabBasic.ResumeLayout(false);
            this.tabBasic.PerformLayout();
            this.tabReducePower.ResumeLayout(false);
            this.tabReducePower.PerformLayout();
            this.grpPosition.ResumeLayout(false);
            this.grpPosition.PerformLayout();
            this.grpBuffTarget.ResumeLayout(false);
            this.grpBuffTarget.PerformLayout();
            this.grpTargetSettings.ResumeLayout(false);
            this.grpTargetSettings.PerformLayout();
            this.pnlHpThreshold.ResumeLayout(false);
            this.pnlHpThreshold.PerformLayout();
            this.grpRpPosition.ResumeLayout(false);
            this.grpRpPosition.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudZoneId)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPosX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPosY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudHpThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRpZoneId)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRpX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRpY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRpProvokeCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRpDeadDelayMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRpHpPunch)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabBasic;
        private System.Windows.Forms.TabPage tabReducePower;

        private System.Windows.Forms.CheckBox      chkEnabled;
        private System.Windows.Forms.Label         lblStatus;
        private System.Windows.Forms.GroupBox      grpPosition;
        private System.Windows.Forms.Label         lblMap;
        private System.Windows.Forms.ComboBox      cboMapId;
        private System.Windows.Forms.CheckBox      chkRequireZone;
        private System.Windows.Forms.NumericUpDown nudZoneId;
        private System.Windows.Forms.Label         lblZoneNote;
        private System.Windows.Forms.CheckBox      chkRequirePosition;
        private System.Windows.Forms.Label         lblPosX;
        private System.Windows.Forms.NumericUpDown nudPosX;
        private System.Windows.Forms.Label         lblPosY;
        private System.Windows.Forms.NumericUpDown nudPosY;
        private System.Windows.Forms.Button        btnGetPos;
        private System.Windows.Forms.GroupBox      grpBuffTarget;
        private System.Windows.Forms.ComboBox      cboBuffTargetMode;
        private System.Windows.Forms.Label         lblBuffTargetNote;
        private System.Windows.Forms.GroupBox      grpTargetSettings;
        private System.Windows.Forms.Label         lblCondition;
        private System.Windows.Forms.ComboBox      cboBuffCondition;
        private System.Windows.Forms.Panel         pnlHpThreshold;
        private System.Windows.Forms.Label         lblHp;
        private System.Windows.Forms.NumericUpDown nudHpThreshold;
        private System.Windows.Forms.Label         lblHpUnit;
        private System.Windows.Forms.Label         lblRangeMode;
        private System.Windows.Forms.ComboBox      cboBuffRangeMode;
        private System.Windows.Forms.Label         lblRangeWarning;
        private System.Windows.Forms.Label         lblTargetNames;
        private System.Windows.Forms.TextBox       txtTargetNames;
        private System.Windows.Forms.Label         lblTargetHint;

        private System.Windows.Forms.CheckBox chkRpEnabled;
        private System.Windows.Forms.GroupBox grpRpPosition;
        private System.Windows.Forms.Label lblRpMap;
        private System.Windows.Forms.ComboBox cboRpMapId;
        private System.Windows.Forms.Label lblRpZone;
        private System.Windows.Forms.NumericUpDown nudRpZoneId;
        private System.Windows.Forms.Label lblRpX;
        private System.Windows.Forms.NumericUpDown nudRpX;
        private System.Windows.Forms.Label lblRpY;
        private System.Windows.Forms.NumericUpDown nudRpY;
        private System.Windows.Forms.Label lblRpProvokeCount;
        private System.Windows.Forms.NumericUpDown nudRpProvokeCount;
        private System.Windows.Forms.Label lblRpDeadDelayMs;
        private System.Windows.Forms.NumericUpDown nudRpDeadDelayMs;
        private System.Windows.Forms.Button btnRpGetPos;
        private System.Windows.Forms.CheckBox chkRpAutoPunchBlackFlag;
        private System.Windows.Forms.CheckBox chkRpUseHpPunch;
        private System.Windows.Forms.NumericUpDown nudRpHpPunch;
        private System.Windows.Forms.Label lblRpHpPunch;
        private System.Windows.Forms.CheckBox chkRpUseTdlt;
    }
}
