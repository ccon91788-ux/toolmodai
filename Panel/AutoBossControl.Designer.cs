namespace Panel
{
    partial class AutoBossControl
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
            this.tabControlBoss = new System.Windows.Forms.TabControl();
            this.tabBasic = new System.Windows.Forms.TabPage();
            this.tabAdvanced = new System.Windows.Forms.TabPage();
            this.grpAction = new System.Windows.Forms.GroupBox();
            this.chkEnableBoss = new System.Windows.Forms.CheckBox();
            this.chkAttackBoss = new System.Windows.Forms.CheckBox();
            this.chkTieBoss = new System.Windows.Forms.CheckBox();
            this.grpScout = new System.Windows.Forms.GroupBox();
            this.chkScoutContinuous = new System.Windows.Forms.CheckBox();
            this.chkScoutVip = new System.Windows.Forms.CheckBox();
            this.chkLimitZone = new System.Windows.Forms.CheckBox();
            this.txtZoneRanges = new System.Windows.Forms.TextBox();
            this.chkLimitMap = new System.Windows.Forms.CheckBox();
            this.txtMapRanges = new System.Windows.Forms.TextBox();
            this.lblBossNames = new System.Windows.Forms.Label();
            this.txtBossNames = new System.Windows.Forms.TextBox();
            this.chkEnableSync = new System.Windows.Forms.CheckBox();
            this.btnSyncParams = new System.Windows.Forms.Button();
            this.grpAuxiliary = new System.Windows.Forms.GroupBox();
            this.chkEatCn = new System.Windows.Forms.CheckBox();
            this.chkEatBh = new System.Windows.Forms.CheckBox();
            this.chkEatGx = new System.Windows.Forms.CheckBox();
            this.chkEatFood = new System.Windows.Forms.CheckBox();
            this.chkEatAd = new System.Windows.Forms.CheckBox();
            this.chkAutoTdlt = new System.Windows.Forms.CheckBox();
            this.chkEatCo4La = new System.Windows.Forms.CheckBox();
            this.chkAutoDisguise = new System.Windows.Forms.CheckBox();
            this.txtDisguiseInfo = new System.Windows.Forms.TextBox();
            this.chkAutoPet = new System.Windows.Forms.CheckBox();
            this.txtPetId = new System.Windows.Forms.TextBox();
            this.chkAutoVpdl = new System.Windows.Forms.CheckBox();
            this.txtVpdlId = new System.Windows.Forms.TextBox();
            this.chkUnequipTrainingArmor = new System.Windows.Forms.CheckBox();
            this.grpSchedule = new System.Windows.Forms.GroupBox();
            this.chkEnableSchedule = new System.Windows.Forms.CheckBox();
            this.txtTimeSchedules = new System.Windows.Forms.TextBox();
            this.grpHpFilter = new System.Windows.Forms.GroupBox();
            this.chkLimitHpAbove = new System.Windows.Forms.CheckBox();
            this.nudHpAbove = new System.Windows.Forms.NumericUpDown();
            this.chkLimitHpBelow = new System.Windows.Forms.CheckBox();
            this.nudHpBelow = new System.Windows.Forms.NumericUpDown();
            this.grpFinishingMove = new System.Windows.Forms.GroupBox();
            this.chkEnableFinishingMove = new System.Windows.Forms.CheckBox();
            this.nudFinishingMoveHp = new System.Windows.Forms.NumericUpDown();
            this.grpAntiBan = new System.Windows.Forms.GroupBox();
            this.chkEnableAntiBan = new System.Windows.Forms.CheckBox();
            this.chkAntiBanAttackMobs = new System.Windows.Forms.CheckBox();
            this.nudAntiBanAttackMobsSeconds = new System.Windows.Forms.NumericUpDown();
            this.lblAntiBanSeconds = new System.Windows.Forms.Label();
            this.chkAntiBanChat = new System.Windows.Forms.CheckBox();
            this.txtAntiBanChatContents = new System.Windows.Forms.TextBox();

            this.grpSkillSelection = new System.Windows.Forms.GroupBox();
            this.btnShowSkillPopup = new System.Windows.Forms.Button();
            this.chkUseShieldUnderHp = new System.Windows.Forms.CheckBox();
            this.nudShieldHpPercent = new System.Windows.Forms.NumericUpDown();
            this.lblShieldHpPercent = new System.Windows.Forms.Label();
            this.pnlSkillPopup = new System.Windows.Forms.Panel();
            this.tabControlSkills = new System.Windows.Forms.TabControl();
            this.tabEarth = new System.Windows.Forms.TabPage();
            this.chkEarthDragon = new System.Windows.Forms.CheckBox();
            this.chkEarthKaioken = new System.Windows.Forms.CheckBox();
            this.chkEarthKamejoko = new System.Windows.Forms.CheckBox();
            this.chkEarthTdhs = new System.Windows.Forms.CheckBox();
            this.chkEarthSleep = new System.Windows.Forms.CheckBox();
            this.chkEarthTeleport = new System.Windows.Forms.CheckBox();
            this.chkEarthShield = new System.Windows.Forms.CheckBox();
            this.tabNamek = new System.Windows.Forms.TabPage();
            this.chkNamekLienHoan = new System.Windows.Forms.CheckBox();
            this.chkNamekDemon = new System.Windows.Forms.CheckBox();
            this.chkNamekMakankosappo = new System.Windows.Forms.CheckBox();
            this.chkNamekDeTrung = new System.Windows.Forms.CheckBox();
            this.chkNamekShield = new System.Windows.Forms.CheckBox();
            this.tabSaiyan = new System.Windows.Forms.TabPage();
            this.chkXaydaGalick = new System.Windows.Forms.CheckBox();
            this.chkXaydaAtomic = new System.Windows.Forms.CheckBox();
            this.chkXaydaMonkey = new System.Windows.Forms.CheckBox();
            this.chkXaydaHeal = new System.Windows.Forms.CheckBox();
            this.chkXaydaShield = new System.Windows.Forms.CheckBox();

            this.tabControlBoss.SuspendLayout();
            this.tabBasic.SuspendLayout();
            this.tabAdvanced.SuspendLayout();
            this.grpAction.SuspendLayout();
            this.grpScout.SuspendLayout();
            this.grpAuxiliary.SuspendLayout();
            this.grpSchedule.SuspendLayout();
            this.grpHpFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudHpAbove)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudHpBelow)).BeginInit();
            this.grpFinishingMove.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudFinishingMoveHp)).BeginInit();
            this.grpAntiBan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAntiBanAttackMobsSeconds)).BeginInit();
            
            this.grpSkillSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudShieldHpPercent)).BeginInit();
            this.pnlSkillPopup.SuspendLayout();
            this.tabControlSkills.SuspendLayout();
            this.tabEarth.SuspendLayout();
            this.tabNamek.SuspendLayout();
            this.tabSaiyan.SuspendLayout();

            this.SuspendLayout();

            // ─── tabControlBoss ──────────────────────────────────────────────
            this.tabControlBoss.Controls.Add(this.tabBasic);
            this.tabControlBoss.Controls.Add(this.tabAdvanced);
            this.tabControlBoss.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlBoss.Location = new System.Drawing.Point(0, 0);
            this.tabControlBoss.Name = "tabControlBoss";
            this.tabControlBoss.SelectedIndex = 0;
            this.tabControlBoss.Size = new System.Drawing.Size(400, 460);

            // ─── tabBasic ────────────────────────────────────────────────────
            this.tabBasic.AutoScroll = true;
            this.tabBasic.BackColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.tabBasic.Controls.Add(this.chkEnableBoss);
            this.tabBasic.Controls.Add(this.grpAction);
            this.tabBasic.Controls.Add(this.grpScout);
            this.tabBasic.Controls.Add(this.grpAuxiliary);
            this.tabBasic.Location = new System.Drawing.Point(4, 24);
            this.tabBasic.Name = "tabBasic";
            this.tabBasic.Padding = new System.Windows.Forms.Padding(3);
            this.tabBasic.Size = new System.Drawing.Size(392, 432);
            this.tabBasic.Text = "Cơ bản";

            // ─── grpAction ───────────────────────────────────────────────────
            this.grpAction.Controls.Add(this.chkAttackBoss);
            this.grpAction.Controls.Add(this.chkTieBoss);
            this.grpAction.Location = new System.Drawing.Point(6, 36);
            this.grpAction.Name = "grpAction";
            this.grpAction.Size = new System.Drawing.Size(380, 55);
            this.grpAction.TabStop = false;
            this.grpAction.Text = "Cài đặt Hành Động";

            this.chkAttackBoss.AutoSize = true;
            this.chkAttackBoss.Location = new System.Drawing.Point(12, 25);
            this.chkAttackBoss.Name = "chkAttackBoss";
            this.chkAttackBoss.Text = "Đi Đánh Boss";

            this.chkEnableBoss.AutoSize = true;
            this.chkEnableBoss.Location = new System.Drawing.Point(12, 10);
            this.chkEnableBoss.Name = "chkEnableBoss";
            this.chkEnableBoss.Text = "BẬT AUTO SĂN BOSS";
            this.chkEnableBoss.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.chkEnableBoss.ForeColor = System.Drawing.Color.Red;

            this.chkTieBoss.AutoSize = true;
            this.chkTieBoss.Location = new System.Drawing.Point(140, 25);
            this.chkTieBoss.Name = "chkTieBoss";
            this.chkTieBoss.Text = "Đi Trói Boss";

            // ─── grpScout ────────────────────────────────────────────────────
            this.grpScout.Controls.Add(this.chkScoutContinuous);
            this.grpScout.Controls.Add(this.chkScoutVip);
            this.grpScout.Controls.Add(this.chkLimitZone);
            this.grpScout.Controls.Add(this.txtZoneRanges);
            this.grpScout.Controls.Add(this.chkLimitMap);
            this.grpScout.Controls.Add(this.txtMapRanges);
            this.grpScout.Controls.Add(this.lblBossNames);
            this.grpScout.Controls.Add(this.txtBossNames);
            this.grpScout.Controls.Add(this.chkEnableSync);
            this.grpScout.Controls.Add(this.btnSyncParams);
            this.grpScout.Location = new System.Drawing.Point(6, 97);
            this.grpScout.Name = "grpScout";
            this.grpScout.Size = new System.Drawing.Size(380, 260);
            this.grpScout.TabStop = false;
            this.grpScout.Text = "Cài đặt dò boss";

            this.chkScoutContinuous.AutoSize = true;
            this.chkScoutContinuous.Location = new System.Drawing.Point(12, 25);
            this.chkScoutContinuous.Name = "chkScoutContinuous";
            this.chkScoutContinuous.Text = "Tự đi dò liên tục";

            this.chkScoutVip.AutoSize = true;
            this.chkScoutVip.Location = new System.Drawing.Point(140, 25);
            this.chkScoutVip.Name = "chkScoutVip";
            this.chkScoutVip.Text = "Dò Boss khi có thông báo";

            this.chkLimitZone.AutoSize = true;
            this.chkLimitZone.Location = new System.Drawing.Point(12, 55);
            this.chkLimitZone.Name = "chkLimitZone";
            this.chkLimitZone.Text = "Giới hạn Khu vực";

            this.txtZoneRanges.Location = new System.Drawing.Point(140, 53);
            this.txtZoneRanges.Name = "txtZoneRanges";
            this.txtZoneRanges.Size = new System.Drawing.Size(220, 23);

            this.chkLimitMap.AutoSize = true;
            this.chkLimitMap.Location = new System.Drawing.Point(12, 85);
            this.chkLimitMap.Name = "chkLimitMap";
            this.chkLimitMap.Text = "Giới hạn ID Map";

            this.txtMapRanges.Location = new System.Drawing.Point(140, 83);
            this.txtMapRanges.Name = "txtMapRanges";
            this.txtMapRanges.Size = new System.Drawing.Size(220, 23);

            this.lblBossNames.AutoSize = true;
            this.lblBossNames.Location = new System.Drawing.Point(12, 115);
            this.lblBossNames.Name = "lblBossNames";
            this.lblBossNames.Text = "Danh sách Boss:";

            this.txtBossNames.Location = new System.Drawing.Point(140, 113);
            this.txtBossNames.Name = "txtBossNames";
            this.txtBossNames.Multiline = true;
            this.txtBossNames.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBossNames.Size = new System.Drawing.Size(220, 60);

            this.chkEnableSync.AutoSize = true;
            this.chkEnableSync.Location = new System.Drawing.Point(12, 195);
            this.chkEnableSync.Name = "chkEnableSync";
            this.chkEnableSync.Text = "Liên kết";

            this.btnSyncParams.Location = new System.Drawing.Point(90, 192);
            this.btnSyncParams.Name = "btnSyncParams";
            this.btnSyncParams.Size = new System.Drawing.Size(160, 24);
            this.btnSyncParams.Text = "Đồng bộ thông số";
            this.btnSyncParams.UseVisualStyleBackColor = true;
            this.btnSyncParams.Font = new System.Drawing.Font("Segoe UI", 8.5F);

            // ─── grpAuxiliary ────────────────────────────────────────────────
            this.grpAuxiliary.Controls.Add(this.chkEatCn);
            this.grpAuxiliary.Controls.Add(this.chkEatBh);
            this.grpAuxiliary.Controls.Add(this.chkEatGx);
            this.grpAuxiliary.Controls.Add(this.chkEatFood);
            this.grpAuxiliary.Controls.Add(this.chkEatAd);
            this.grpAuxiliary.Controls.Add(this.chkAutoTdlt);
            this.grpAuxiliary.Controls.Add(this.chkEatCo4La);
            this.grpAuxiliary.Controls.Add(this.chkAutoDisguise);
            this.grpAuxiliary.Controls.Add(this.txtDisguiseInfo);
            this.grpAuxiliary.Controls.Add(this.chkAutoPet);
            this.grpAuxiliary.Controls.Add(this.txtPetId);
            this.grpAuxiliary.Controls.Add(this.chkAutoVpdl);
            this.grpAuxiliary.Controls.Add(this.txtVpdlId);
            this.grpAuxiliary.Controls.Add(this.chkUnequipTrainingArmor);
            this.grpAuxiliary.Location = new System.Drawing.Point(6, 340);
            this.grpAuxiliary.Name = "grpAuxiliary";
            this.grpAuxiliary.Size = new System.Drawing.Size(380, 150);
            this.grpAuxiliary.TabStop = false;
            this.grpAuxiliary.Text = "Item sử dụng khi bem";

            this.chkEatCn.AutoSize = true;
            this.chkEatCn.Location = new System.Drawing.Point(12, 25);
            this.chkEatCn.Name = "chkEatCn";
            this.chkEatCn.Text = "Cuồng nộ";

            this.chkEatBh.AutoSize = true;
            this.chkEatBh.Location = new System.Drawing.Point(100, 25);
            this.chkEatBh.Name = "chkEatBh";
            this.chkEatBh.Text = "Bổ huyết";

            this.chkEatGx.AutoSize = true;
            this.chkEatGx.Location = new System.Drawing.Point(190, 25);
            this.chkEatGx.Name = "chkEatGx";
            this.chkEatGx.Text = "Giáp xên";

            this.chkEatFood.AutoSize = true;
            this.chkEatFood.Location = new System.Drawing.Point(280, 25);
            this.chkEatFood.Name = "chkEatFood";
            this.chkEatFood.Text = "Thức ăn";

            this.chkEatAd.AutoSize = true;
            this.chkEatAd.Location = new System.Drawing.Point(12, 55);
            this.chkEatAd.Name = "chkEatAd";
            this.chkEatAd.Text = "Ẩn danh";

            this.chkAutoTdlt.AutoSize = true;
            this.chkAutoTdlt.Location = new System.Drawing.Point(100, 55);
            this.chkAutoTdlt.Name = "chkAutoTdlt";
            this.chkAutoTdlt.Text = "TĐLT";

            this.chkEatCo4La.AutoSize = true;
            this.chkEatCo4La.Location = new System.Drawing.Point(190, 55);
            this.chkEatCo4La.Name = "chkEatCo4La";
            this.chkEatCo4La.Text = "Cỏ 4 lá";

            this.chkAutoDisguise.AutoSize = true;
            this.chkAutoDisguise.Location = new System.Drawing.Point(12, 85);
            this.chkAutoDisguise.Name = "chkAutoDisguise";
            this.chkAutoDisguise.Text = "Mặc cải trang";

            this.txtDisguiseInfo.Location = new System.Drawing.Point(125, 83);
            this.txtDisguiseInfo.Name = "txtDisguiseInfo";
            this.txtDisguiseInfo.Size = new System.Drawing.Size(60, 23);

            this.chkAutoPet.AutoSize = true;
            this.chkAutoPet.Location = new System.Drawing.Point(190, 85);
            this.chkAutoPet.Name = "chkAutoPet";
            this.chkAutoPet.Text = "Mặc Pet ID";

            this.txtPetId.Location = new System.Drawing.Point(280, 83);
            this.txtPetId.Name = "txtPetId";
            this.txtPetId.Size = new System.Drawing.Size(60, 23);

            this.chkAutoVpdl.AutoSize = true;
            this.chkAutoVpdl.Location = new System.Drawing.Point(12, 115);
            this.chkAutoVpdl.Name = "chkAutoVpdl";
            this.chkAutoVpdl.Text = "Mặc VPDL";

            this.txtVpdlId.Location = new System.Drawing.Point(125, 113);
            this.txtVpdlId.Name = "txtVpdlId";
            this.txtVpdlId.Size = new System.Drawing.Size(60, 23);

            this.chkUnequipTrainingArmor.AutoSize = true;
            this.chkUnequipTrainingArmor.Location = new System.Drawing.Point(190, 115);
            this.chkUnequipTrainingArmor.Name = "chkUnequipTrainingArmor";
            this.chkUnequipTrainingArmor.Text = "Tháo giáp luyện tập";

            // ─── tabAdvanced ─────────────────────────────────────────────────
            this.tabAdvanced.AutoScroll = true;
            this.tabAdvanced.BackColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.tabAdvanced.Controls.Add(this.grpSchedule);
            this.tabAdvanced.Controls.Add(this.grpSkillSelection);
            this.tabAdvanced.Controls.Add(this.grpHpFilter);
            this.tabAdvanced.Controls.Add(this.grpFinishingMove);
            this.tabAdvanced.Controls.Add(this.grpAntiBan);
            this.tabAdvanced.Location = new System.Drawing.Point(4, 24);
            this.tabAdvanced.Name = "tabAdvanced";
            this.tabAdvanced.Padding = new System.Windows.Forms.Padding(3);
            this.tabAdvanced.Size = new System.Drawing.Size(392, 432);
            this.tabAdvanced.Text = "Nâng cao";

            // ─── grpSchedule ─────────────────────────────────────────────────
            this.grpSchedule.Controls.Add(this.chkEnableSchedule);
            this.grpSchedule.Controls.Add(this.txtTimeSchedules);
            this.grpSchedule.Location = new System.Drawing.Point(6, 6);
            this.grpSchedule.Name = "grpSchedule";
            this.grpSchedule.Size = new System.Drawing.Size(380, 55);
            this.grpSchedule.TabStop = false;
            this.grpSchedule.Text = "Khung giờ săn boss";

            this.chkEnableSchedule.AutoSize = true;
            this.chkEnableSchedule.Location = new System.Drawing.Point(12, 25);
            this.chkEnableSchedule.Name = "chkEnableSchedule";
            this.chkEnableSchedule.Text = "Khung giờ";

            this.txtTimeSchedules.Location = new System.Drawing.Point(100, 23);
            this.txtTimeSchedules.Name = "txtTimeSchedules";
            this.txtTimeSchedules.Size = new System.Drawing.Size(270, 23);

            // ─── grpSkillSelection ───────────────────────────────────────────
            this.grpSkillSelection.Controls.Add(this.btnShowSkillPopup);
            this.grpSkillSelection.Controls.Add(this.chkUseShieldUnderHp);
            this.grpSkillSelection.Controls.Add(this.nudShieldHpPercent);
            this.grpSkillSelection.Controls.Add(this.lblShieldHpPercent);
            this.grpSkillSelection.Location = new System.Drawing.Point(6, 67);
            this.grpSkillSelection.Name = "grpSkillSelection";
            this.grpSkillSelection.Size = new System.Drawing.Size(380, 89);
            this.grpSkillSelection.TabStop = false;
            this.grpSkillSelection.Text = "Chọn skill tấn công";

            this.btnShowSkillPopup.Location = new System.Drawing.Point(12, 23);
            this.btnShowSkillPopup.Name = "btnShowSkillPopup";
            this.btnShowSkillPopup.Size = new System.Drawing.Size(180, 25);
            this.btnShowSkillPopup.Text = "Danh sách Skill sử dụng";
            this.btnShowSkillPopup.UseVisualStyleBackColor = true;

            this.chkUseShieldUnderHp.AutoSize = true;
            this.chkUseShieldUnderHp.Location = new System.Drawing.Point(12, 57);
            this.chkUseShieldUnderHp.Name = "chkUseShieldUnderHp";
            this.chkUseShieldUnderHp.Text = "Dùng Khiên khi HP dưới:";

            this.nudShieldHpPercent.Location = new System.Drawing.Point(235, 56);
            this.nudShieldHpPercent.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudShieldHpPercent.Name = "nudShieldHpPercent";
            this.nudShieldHpPercent.Size = new System.Drawing.Size(50, 23);

            this.lblShieldHpPercent.AutoSize = true;
            this.lblShieldHpPercent.Location = new System.Drawing.Point(290, 58);
            this.lblShieldHpPercent.Name = "lblShieldHpPercent";
            this.lblShieldHpPercent.Text = "%";

            // ─── grpHpFilter ─────────────────────────────────────────────────
            this.grpHpFilter.Controls.Add(this.chkLimitHpAbove);
            this.grpHpFilter.Controls.Add(this.nudHpAbove);
            this.grpHpFilter.Controls.Add(this.chkLimitHpBelow);
            this.grpHpFilter.Controls.Add(this.nudHpBelow);
            this.grpHpFilter.Location = new System.Drawing.Point(6, 162);
            this.grpHpFilter.Name = "grpHpFilter";
            this.grpHpFilter.Size = new System.Drawing.Size(380, 90);
            this.grpHpFilter.TabStop = false;
            this.grpHpFilter.Text = "Chỉ đánh theo HP";

            this.chkLimitHpAbove.AutoSize = true;
            this.chkLimitHpAbove.Location = new System.Drawing.Point(12, 25);
            this.chkLimitHpAbove.Name = "chkLimitHpAbove";
            this.chkLimitHpAbove.Text = "Chỉ đánh khi HP mục tiêu trên:";

            this.nudHpAbove.Location = new System.Drawing.Point(230, 24);
            this.nudHpAbove.Maximum = new decimal(new int[] { 2000000000, 0, 0, 0 });
            this.nudHpAbove.Name = "nudHpAbove";
            this.nudHpAbove.Size = new System.Drawing.Size(130, 23);

            this.chkLimitHpBelow.AutoSize = true;
            this.chkLimitHpBelow.Location = new System.Drawing.Point(12, 55);
            this.chkLimitHpBelow.Name = "chkLimitHpBelow";
            this.chkLimitHpBelow.Text = "Chỉ đánh khi HP mục tiêu dưới:";

            this.nudHpBelow.Location = new System.Drawing.Point(230, 54);
            this.nudHpBelow.Maximum = new decimal(new int[] { 2000000000, 0, 0, 0 });
            this.nudHpBelow.Name = "nudHpBelow";
            this.nudHpBelow.Size = new System.Drawing.Size(130, 23);

            // ─── grpFinishingMove ────────────────────────────────────────────
            this.grpFinishingMove.Controls.Add(this.chkEnableFinishingMove);
            this.grpFinishingMove.Controls.Add(this.nudFinishingMoveHp);
            this.grpFinishingMove.Location = new System.Drawing.Point(6, 258);
            this.grpFinishingMove.Name = "grpFinishingMove";
            this.grpFinishingMove.Size = new System.Drawing.Size(380, 60);
            this.grpFinishingMove.TabStop = false;
            this.grpFinishingMove.Text = "Chiêu kết liễu";

            this.chkEnableFinishingMove.AutoSize = true;
            this.chkEnableFinishingMove.Location = new System.Drawing.Point(12, 25);
            this.chkEnableFinishingMove.Name = "chkEnableFinishingMove";
            this.chkEnableFinishingMove.Text = "Dùng chiêu kết liễu khi boss còn:";

            this.nudFinishingMoveHp.Location = new System.Drawing.Point(230, 24);
            this.nudFinishingMoveHp.Maximum = new decimal(new int[] { 2000000000, 0, 0, 0 });
            this.nudFinishingMoveHp.Name = "nudFinishingMoveHp";
            this.nudFinishingMoveHp.Size = new System.Drawing.Size(130, 23);

            // ─── grpAntiBan ──────────────────────────────────────────────────
            this.grpAntiBan.Controls.Add(this.chkEnableAntiBan);
            this.grpAntiBan.Controls.Add(this.chkAntiBanAttackMobs);
            this.grpAntiBan.Controls.Add(this.nudAntiBanAttackMobsSeconds);
            this.grpAntiBan.Controls.Add(this.lblAntiBanSeconds);
            this.grpAntiBan.Controls.Add(this.chkAntiBanChat);
            this.grpAntiBan.Controls.Add(this.txtAntiBanChatContents);
            this.grpAntiBan.Location = new System.Drawing.Point(6, 324);
            this.grpAntiBan.Name = "grpAntiBan";
            this.grpAntiBan.Size = new System.Drawing.Size(380, 200);
            this.grpAntiBan.TabStop = false;
            this.grpAntiBan.Text = "Sau khi săn boss";

            this.chkEnableAntiBan.AutoSize = true;
            this.chkEnableAntiBan.Location = new System.Drawing.Point(12, 25);
            this.chkEnableAntiBan.Name = "chkEnableAntiBan";
            this.chkEnableAntiBan.Text = "Anti Lê Khắc Hậu";
            this.chkEnableAntiBan.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.chkAntiBanAttackMobs.AutoSize = true;
            this.chkAntiBanAttackMobs.Location = new System.Drawing.Point(30, 55);
            this.chkAntiBanAttackMobs.Name = "chkAntiBanAttackMobs";
            this.chkAntiBanAttackMobs.Text = "Train quái tại map sau khi boss mất";

            this.nudAntiBanAttackMobsSeconds.Location = new System.Drawing.Point(270, 54);
            this.nudAntiBanAttackMobsSeconds.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            this.nudAntiBanAttackMobsSeconds.Name = "nudAntiBanAttackMobsSeconds";
            this.nudAntiBanAttackMobsSeconds.Size = new System.Drawing.Size(50, 23);

            this.lblAntiBanSeconds.AutoSize = true;
            this.lblAntiBanSeconds.Location = new System.Drawing.Point(325, 56);
            this.lblAntiBanSeconds.Name = "lblAntiBanSeconds";
            this.lblAntiBanSeconds.Text = "giây";

            this.chkAntiBanChat.AutoSize = true;
            this.chkAntiBanChat.Location = new System.Drawing.Point(30, 85);
            this.chkAntiBanChat.Name = "chkAntiBanChat";
            this.chkAntiBanChat.Text = "Chat Custom (Danh sách, dòng 1 chữ)";

            this.txtAntiBanChatContents.Location = new System.Drawing.Point(30, 105);
            this.txtAntiBanChatContents.Multiline = true;
            this.txtAntiBanChatContents.AcceptsReturn = true;
            this.txtAntiBanChatContents.Name = "txtAntiBanChatContents";
            this.txtAntiBanChatContents.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtAntiBanChatContents.Size = new System.Drawing.Size(330, 80);

            // ─── AutoBossControl ──────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControlBoss);
            this.Name = "AutoBossControl";
            this.Size = new System.Drawing.Size(400, 460);

            this.tabControlBoss.ResumeLayout(false);
            this.tabBasic.ResumeLayout(false);
            this.grpAction.ResumeLayout(false);
            this.grpAction.PerformLayout();
            this.grpScout.ResumeLayout(false);
            this.grpScout.PerformLayout();
            this.grpAuxiliary.ResumeLayout(false);
            this.grpAuxiliary.PerformLayout();

            this.tabAdvanced.ResumeLayout(false);
            this.grpSchedule.ResumeLayout(false);
            this.grpSchedule.PerformLayout();
            this.grpHpFilter.ResumeLayout(false);
            this.grpHpFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudHpAbove)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudHpBelow)).EndInit();
            this.grpFinishingMove.ResumeLayout(false);
            this.grpFinishingMove.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudFinishingMoveHp)).EndInit();
            this.grpAntiBan.ResumeLayout(false);
            this.grpAntiBan.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAntiBanAttackMobsSeconds)).EndInit();

            // ─── pnlSkillPopup ───────────────────────────────────────────────
            this.pnlSkillPopup.BackColor = System.Drawing.SystemColors.Control;
            this.pnlSkillPopup.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlSkillPopup.Controls.Add(this.tabControlSkills);
            this.pnlSkillPopup.Size = new System.Drawing.Size(320, 160);
            this.pnlSkillPopup.Name = "pnlSkillPopup";

            // ─── tabControlSkills ────────────────────────────────────────────
            this.tabControlSkills.Controls.Add(this.tabEarth);
            this.tabControlSkills.Controls.Add(this.tabNamek);
            this.tabControlSkills.Controls.Add(this.tabSaiyan);
            this.tabControlSkills.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlSkills.Location = new System.Drawing.Point(0, 0);
            this.tabControlSkills.Name = "tabControlSkills";
            this.tabControlSkills.SelectedIndex = 0;
            this.tabControlSkills.Size = new System.Drawing.Size(318, 158);

            // ─── tabEarth ────────────────────────────────────────────────────
            this.tabEarth.Controls.Add(this.chkEarthDragon);
            this.tabEarth.Controls.Add(this.chkEarthKaioken);
            this.tabEarth.Controls.Add(this.chkEarthKamejoko);
            this.tabEarth.Controls.Add(this.chkEarthTdhs);
            this.tabEarth.Controls.Add(this.chkEarthSleep);
            this.tabEarth.Controls.Add(this.chkEarthTeleport);
            this.tabEarth.Controls.Add(this.chkEarthShield);
            this.tabEarth.Location = new System.Drawing.Point(4, 24);
            this.tabEarth.Name = "tabEarth";
            this.tabEarth.Padding = new System.Windows.Forms.Padding(3);
            this.tabEarth.Size = new System.Drawing.Size(310, 130);
            this.tabEarth.Text = "Trái Đất";
            this.tabEarth.UseVisualStyleBackColor = true;

            this.chkEarthDragon.AutoSize = true;
            this.chkEarthDragon.Location = new System.Drawing.Point(10, 10);
            this.chkEarthDragon.Name = "chkEarthDragon";
            this.chkEarthDragon.Text = "Đấm Dragon [0]";

            this.chkEarthKamejoko.AutoSize = true;
            this.chkEarthKamejoko.Location = new System.Drawing.Point(10, 40);
            this.chkEarthKamejoko.Name = "chkEarthKamejoko";
            this.chkEarthKamejoko.Text = "Kamejoko [1]";

            this.chkEarthSleep.AutoSize = true;
            this.chkEarthSleep.Location = new System.Drawing.Point(10, 70);
            this.chkEarthSleep.Name = "chkEarthSleep";
            this.chkEarthSleep.Text = "Thôi miên [22]";

            this.chkEarthShield.AutoSize = true;
            this.chkEarthShield.Location = new System.Drawing.Point(10, 100);
            this.chkEarthShield.Name = "chkEarthShield";
            this.chkEarthShield.Text = "Khiên năng lượng [19]";

            this.chkEarthKaioken.AutoSize = true;
            this.chkEarthKaioken.Location = new System.Drawing.Point(150, 10);
            this.chkEarthKaioken.Name = "chkEarthKaioken";
            this.chkEarthKaioken.Text = "Kaioken [9]";

            this.chkEarthTdhs.AutoSize = true;
            this.chkEarthTdhs.Location = new System.Drawing.Point(150, 40);
            this.chkEarthTdhs.Name = "chkEarthTdhs";
            this.chkEarthTdhs.Text = "Thái dương H.S [6]";

            this.chkEarthTeleport.AutoSize = true;
            this.chkEarthTeleport.Location = new System.Drawing.Point(150, 70);
            this.chkEarthTeleport.Name = "chkEarthTeleport";
            this.chkEarthTeleport.Text = "Dịch chuyển T.T [20]";

            // ─── tabNamek ────────────────────────────────────────────────────
            this.tabNamek.Controls.Add(this.chkNamekLienHoan);
            this.tabNamek.Controls.Add(this.chkNamekDemon);
            this.tabNamek.Controls.Add(this.chkNamekMakankosappo);
            this.tabNamek.Controls.Add(this.chkNamekDeTrung);
            this.tabNamek.Controls.Add(this.chkNamekShield);
            this.tabNamek.Location = new System.Drawing.Point(4, 24);
            this.tabNamek.Name = "tabNamek";
            this.tabNamek.Padding = new System.Windows.Forms.Padding(3);
            this.tabNamek.Size = new System.Drawing.Size(310, 130);
            this.tabNamek.Text = "Namếc";
            this.tabNamek.UseVisualStyleBackColor = true;

            this.chkNamekLienHoan.AutoSize = true;
            this.chkNamekLienHoan.Location = new System.Drawing.Point(10, 10);
            this.chkNamekLienHoan.Name = "chkNamekLienHoan";
            this.chkNamekLienHoan.Text = "Liên hoàn [17]";

            this.chkNamekMakankosappo.AutoSize = true;
            this.chkNamekMakankosappo.Location = new System.Drawing.Point(10, 40);
            this.chkNamekMakankosappo.Name = "chkNamekMakankosappo";
            this.chkNamekMakankosappo.Text = "Masenko [3]";

            this.chkNamekShield.AutoSize = true;
            this.chkNamekShield.Location = new System.Drawing.Point(10, 70);
            this.chkNamekShield.Name = "chkNamekShield";
            this.chkNamekShield.Text = "Khiên năng lượng [19]";

            this.chkNamekDemon.AutoSize = true;
            this.chkNamekDemon.Location = new System.Drawing.Point(150, 10);
            this.chkNamekDemon.Name = "chkNamekDemon";
            this.chkNamekDemon.Text = "Đấm Demon [2]";

            this.chkNamekDeTrung.AutoSize = true;
            this.chkNamekDeTrung.Location = new System.Drawing.Point(150, 40);
            this.chkNamekDeTrung.Name = "chkNamekDeTrung";
            this.chkNamekDeTrung.Text = "Đẻ trứng [12]";

            // ─── tabSaiyan ───────────────────────────────────────────────────
            this.tabSaiyan.Controls.Add(this.chkXaydaGalick);
            this.tabSaiyan.Controls.Add(this.chkXaydaAtomic);
            this.tabSaiyan.Controls.Add(this.chkXaydaMonkey);
            this.tabSaiyan.Controls.Add(this.chkXaydaHeal);
            this.tabSaiyan.Controls.Add(this.chkXaydaShield);
            this.tabSaiyan.Location = new System.Drawing.Point(4, 24);
            this.tabSaiyan.Name = "tabSaiyan";
            this.tabSaiyan.Padding = new System.Windows.Forms.Padding(3);
            this.tabSaiyan.Size = new System.Drawing.Size(310, 130);
            this.tabSaiyan.Text = "Xayda";
            this.tabSaiyan.UseVisualStyleBackColor = true;

            this.chkXaydaGalick.AutoSize = true;
            this.chkXaydaGalick.Location = new System.Drawing.Point(10, 10);
            this.chkXaydaGalick.Name = "chkXaydaGalick";
            this.chkXaydaGalick.Text = "Đấm Galick [4]";

            this.chkXaydaMonkey.AutoSize = true;
            this.chkXaydaMonkey.Location = new System.Drawing.Point(10, 40);
            this.chkXaydaMonkey.Name = "chkXaydaMonkey";
            this.chkXaydaMonkey.Text = "Biến hình [13]";

            this.chkXaydaShield.AutoSize = true;
            this.chkXaydaShield.Location = new System.Drawing.Point(10, 70);
            this.chkXaydaShield.Name = "chkXaydaShield";
            this.chkXaydaShield.Text = "Khiên năng lượng [19]";

            this.chkXaydaAtomic.AutoSize = true;
            this.chkXaydaAtomic.Location = new System.Drawing.Point(150, 10);
            this.chkXaydaAtomic.Name = "chkXaydaAtomic";
            this.chkXaydaAtomic.Text = "Antomic [5]";

            this.chkXaydaHeal.AutoSize = true;
            this.chkXaydaHeal.Location = new System.Drawing.Point(150, 40);
            this.chkXaydaHeal.Name = "chkXaydaHeal";
            this.chkXaydaHeal.Text = "Tái tạo N.Lượng [8]";

            this.grpSkillSelection.ResumeLayout(false);
            this.grpSkillSelection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudShieldHpPercent)).EndInit();
            this.pnlSkillPopup.ResumeLayout(false);
            this.tabControlSkills.ResumeLayout(false);
            this.tabEarth.ResumeLayout(false);
            this.tabEarth.PerformLayout();
            this.tabNamek.ResumeLayout(false);
            this.tabNamek.PerformLayout();
            this.tabSaiyan.ResumeLayout(false);
            this.tabSaiyan.PerformLayout();

            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl tabControlBoss;
        private System.Windows.Forms.TabPage tabBasic;
        private System.Windows.Forms.TabPage tabAdvanced;
        
        private System.Windows.Forms.CheckBox chkEnableBoss;
        private System.Windows.Forms.GroupBox grpAction;
        private System.Windows.Forms.CheckBox chkAttackBoss;
        private System.Windows.Forms.CheckBox chkTieBoss;

        private System.Windows.Forms.GroupBox grpScout;
        private System.Windows.Forms.CheckBox chkScoutContinuous;
        private System.Windows.Forms.CheckBox chkScoutVip;
        private System.Windows.Forms.CheckBox chkLimitZone;
        private System.Windows.Forms.TextBox txtZoneRanges;
        private System.Windows.Forms.CheckBox chkLimitMap;
        private System.Windows.Forms.TextBox txtMapRanges;
        private System.Windows.Forms.Label lblBossNames;
        private System.Windows.Forms.TextBox txtBossNames;
        private System.Windows.Forms.CheckBox chkEnableSync;
        private System.Windows.Forms.Button btnSyncParams;

        private System.Windows.Forms.GroupBox grpAuxiliary;
        private System.Windows.Forms.CheckBox chkEatCn;
        private System.Windows.Forms.CheckBox chkEatBh;
        private System.Windows.Forms.CheckBox chkEatGx;
        private System.Windows.Forms.CheckBox chkEatFood;
        private System.Windows.Forms.CheckBox chkEatAd;
        private System.Windows.Forms.CheckBox chkAutoTdlt;
        private System.Windows.Forms.CheckBox chkEatCo4La;
        private System.Windows.Forms.CheckBox chkAutoDisguise;
        private System.Windows.Forms.TextBox txtDisguiseInfo;
        private System.Windows.Forms.CheckBox chkAutoPet;
        private System.Windows.Forms.TextBox txtPetId;
        private System.Windows.Forms.CheckBox chkAutoVpdl;
        private System.Windows.Forms.TextBox txtVpdlId;
        private System.Windows.Forms.CheckBox chkUnequipTrainingArmor;

        private System.Windows.Forms.GroupBox grpSchedule;
        private System.Windows.Forms.CheckBox chkEnableSchedule;
        private System.Windows.Forms.TextBox txtTimeSchedules;

        private System.Windows.Forms.GroupBox grpHpFilter;
        private System.Windows.Forms.CheckBox chkLimitHpAbove;
        private System.Windows.Forms.NumericUpDown nudHpAbove;
        private System.Windows.Forms.CheckBox chkLimitHpBelow;
        private System.Windows.Forms.NumericUpDown nudHpBelow;

        private System.Windows.Forms.GroupBox grpFinishingMove;
        private System.Windows.Forms.CheckBox chkEnableFinishingMove;
        private System.Windows.Forms.NumericUpDown nudFinishingMoveHp;

        private System.Windows.Forms.GroupBox grpAntiBan;
        private System.Windows.Forms.CheckBox chkEnableAntiBan;
        private System.Windows.Forms.CheckBox chkAntiBanAttackMobs;
        private System.Windows.Forms.NumericUpDown nudAntiBanAttackMobsSeconds;
        private System.Windows.Forms.Label lblAntiBanSeconds;
        private System.Windows.Forms.CheckBox chkAntiBanChat;
        private System.Windows.Forms.TextBox txtAntiBanChatContents;

        private System.Windows.Forms.GroupBox grpSkillSelection;
        private System.Windows.Forms.Button btnShowSkillPopup;
        private System.Windows.Forms.CheckBox chkUseShieldUnderHp;
        private System.Windows.Forms.NumericUpDown nudShieldHpPercent;
        private System.Windows.Forms.Label lblShieldHpPercent;

        private System.Windows.Forms.Panel pnlSkillPopup;
        private System.Windows.Forms.TabControl tabControlSkills;
        private System.Windows.Forms.TabPage tabEarth;
        private System.Windows.Forms.TabPage tabNamek;
        private System.Windows.Forms.TabPage tabSaiyan;

        // Trái Đất
        private System.Windows.Forms.CheckBox chkEarthDragon;
        private System.Windows.Forms.CheckBox chkEarthKaioken;
        private System.Windows.Forms.CheckBox chkEarthKamejoko;
        private System.Windows.Forms.CheckBox chkEarthTdhs;
        private System.Windows.Forms.CheckBox chkEarthSleep;
        private System.Windows.Forms.CheckBox chkEarthTeleport;
        private System.Windows.Forms.CheckBox chkEarthShield;

        // Namếc
        private System.Windows.Forms.CheckBox chkNamekLienHoan;
        private System.Windows.Forms.CheckBox chkNamekDemon;
        private System.Windows.Forms.CheckBox chkNamekMakankosappo;
        private System.Windows.Forms.CheckBox chkNamekDeTrung;
        private System.Windows.Forms.CheckBox chkNamekShield;

        // Xayda
        private System.Windows.Forms.CheckBox chkXaydaGalick;
        private System.Windows.Forms.CheckBox chkXaydaAtomic;
        private System.Windows.Forms.CheckBox chkXaydaMonkey;
        private System.Windows.Forms.CheckBox chkXaydaHeal;
        private System.Windows.Forms.CheckBox chkXaydaShield;
    }
}
