namespace Panel;

partial class Form1
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
        this.panelTop = new System.Windows.Forms.Panel();
        this.btnToggleGame = new System.Windows.Forms.Button();
        this.btnAdd = new System.Windows.Forms.Button();
        this.btnEdit = new System.Windows.Forms.Button();
        this.btnDelete = new System.Windows.Forms.Button();
        this.btnSelectAll = new System.Windows.Forms.Button();
        this.btnSettings = new System.Windows.Forms.Button();
        this.btnArrangeWindows = new System.Windows.Forms.Button();
        this.btnCloseAll = new System.Windows.Forms.Button();
        this.btnHideAll = new System.Windows.Forms.Button();
        this.chkHideAccount = new System.Windows.Forms.CheckBox();
        this.chkAutoLogin = new System.Windows.Forms.CheckBox();
        this.chkAutoHideClient = new System.Windows.Forms.CheckBox();
        this.numAutoLoginThread = new System.Windows.Forms.TextBox();
        this.btnDeleteSelected = new System.Windows.Forms.Button();
        this.lblSocketStatus = new System.Windows.Forms.Label();
        
        // System Info Controls
        this.grpSystemInfo = new System.Windows.Forms.GroupBox();
        this.lblAccountStats = new System.Windows.Forms.Label();
        this.lblSystemStats = new System.Windows.Forms.Label();
        this.btnMoveUp = new System.Windows.Forms.Button();
        this.btnMoveDown = new System.Windows.Forms.Button();
        this.btnCleanRam = new System.Windows.Forms.Button();
        this.lblSystemTime = new System.Windows.Forms.Label();

        this.panelMiddle = new System.Windows.Forms.Panel();
        this.dgvAccounts = new System.Windows.Forms.DataGridView();
        this.colSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
        this.colSTT = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colCharacter = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colDataInGame = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colServer = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colTypeAccount = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colAccount = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colShow = new System.Windows.Forms.DataGridViewButtonColumn();
        // Account Action Buttons Panel
        this.panelAccountActions = new System.Windows.Forms.Panel();
        this.btnBatAuto = new System.Windows.Forms.Button();
        this.btnDungAuto = new System.Windows.Forms.Button();
        this.btnBatAutoTatCaChon = new System.Windows.Forms.Button();
        this.btnDungAutoTatCaChon = new System.Windows.Forms.Button();
        this.btnGDVP = new System.Windows.Forms.Button();
        this.btnAutoBoMong = new System.Windows.Forms.Button();

        this.panelBottom = new System.Windows.Forms.Panel();
        this.tabControlFeatures = new System.Windows.Forms.TabControl();
        this.tabTrain = new System.Windows.Forms.TabPage();
        this.tabControlTrain = new System.Windows.Forms.TabControl();
        this.tabTrainBasic = new System.Windows.Forms.TabPage();
        this.tabTrainMvbt = new System.Windows.Forms.TabPage();
        this.tabTrainMhbt = new System.Windows.Forms.TabPage();
        this.tabTrainUpKilis = new System.Windows.Forms.TabPage();
        this.tabTrainBossVegetaCity = new System.Windows.Forms.TabPage();
        this.chkBossVegetaCityEnable = new System.Windows.Forms.CheckBox();
        this.chkBossVegetaCityAuto3h = new System.Windows.Forms.CheckBox();
        this.chkBossVegetaCityAuto2230 = new System.Windows.Forms.CheckBox();
        this.chkBossVegetaCityReviveByGem = new System.Windows.Forms.CheckBox();
        this.chkBossVegetaCityUseTdlt = new System.Windows.Forms.CheckBox();
        this.mvbtControl = new MvbtControl();
        this.mhbtControl = new MvbtControl();
        this.kilisControl = new KilisControl();
        this.upSkhControl = new UpSkhControl();
        this.tabTrainAdvanced = new System.Windows.Forms.TabPage();
        this.tabUpSkh = new System.Windows.Forms.TabPage();
        this.tabPet = new System.Windows.Forms.TabPage();
        this.petControl = new PetControl();
        this.tabDauThan = new System.Windows.Forms.TabPage();
        this.dauThanControl = new DauThanControl();
        this.tabBuffNamek = new System.Windows.Forms.TabPage();
        this.buffNamekControl = new BuffNamekControl();
        // Account Info sub-tabs
        this.tabControlAccountInfo = new System.Windows.Forms.TabControl();
        this.tabAccountSuPhu = new System.Windows.Forms.TabPage();
        this.tabAccountDeTu = new System.Windows.Forms.TabPage();
        this.tabAccountHanhTrang = new System.Windows.Forms.TabPage();
        this.tabAccountNhatKi = new System.Windows.Forms.TabPage();
        this.txtSuPhuInfo = new System.Windows.Forms.RichTextBox();
        this.txtDeTuInfo = new System.Windows.Forms.RichTextBox();
        this.txtNhatKiInfo = new System.Windows.Forms.RichTextBox();
        
        // Tab Hanh Trang Controls
        this.panelHanhTrangTop = new System.Windows.Forms.Panel();
        this.btnXemHanhTrang = new System.Windows.Forms.Button();
        this.btnXemRuongDo = new System.Windows.Forms.Button();
        this.grpHanhTrangSummary = new System.Windows.Forms.GroupBox();
        this.lblHanhTrangCoin = new System.Windows.Forms.Label();
        this.lblHanhTrangGem = new System.Windows.Forms.Label();
        this.lblHanhTrangRuby = new System.Windows.Forms.Label();
        this.lblHanhTrangSlots = new System.Windows.Forms.Label();
        this.lblRuongDoSlots = new System.Windows.Forms.Label();

        this.tabItemManagement = new System.Windows.Forms.TabPage();
        this.tabControlItem = new System.Windows.Forms.TabControl();
        this.tabItemDrop = new System.Windows.Forms.TabPage();
        this.tabItemStore = new System.Windows.Forms.TabPage();
        this.tabItemSell = new System.Windows.Forms.TabPage();
        this.tabItemBuy = new System.Windows.Forms.TabPage();
        this.tabItemPick = new System.Windows.Forms.TabPage();
        this.tabItemUse = new System.Windows.Forms.TabPage();
        // Auto Pick Instantiations
        this.grpAutoPick = new System.Windows.Forms.GroupBox();
        this.chkAutoPick = new System.Windows.Forms.CheckBox();
        this.lblPickMode = new System.Windows.Forms.Label();
        this.cboPickMode = new System.Windows.Forms.ComboBox();
        this.chkOnlyMyItems = new System.Windows.Forms.CheckBox();
        this.lblPickDesc = new System.Windows.Forms.Label();
        this.txtPickIdsList = new System.Windows.Forms.TextBox();
        this.lblPickBlackList = new System.Windows.Forms.Label();
        this.txtPickBlackList = new System.Windows.Forms.TextBox();
        
        this.chkAutoStoreWhenFull = new System.Windows.Forms.CheckBox();
        this.grpStoreFilter = new System.Windows.Forms.GroupBox();
        this.chkStoreKichHoat = new System.Windows.Forms.CheckBox();
        this.chkStoreThanLinh = new System.Windows.Forms.CheckBox();
        this.chkStorePhaLe = new System.Windows.Forms.CheckBox();
        this.lblStarCount = new System.Windows.Forms.Label();
        this.nudStoreStarCount = new System.Windows.Forms.NumericUpDown();
        this.grpAutoDrop = new System.Windows.Forms.GroupBox();
        this.chkAutoDrop = new System.Windows.Forms.CheckBox();
        this.lblDropDesc1 = new System.Windows.Forms.Label();
        this.chkDropByHsd = new System.Windows.Forms.CheckBox();
        this.lblDropDesc2 = new System.Windows.Forms.Label();
        this.txtDropIds = new System.Windows.Forms.TextBox();
        this.grpAccountInfo = new System.Windows.Forms.GroupBox();
        // Auto Buy Instantiations
        this.grpAutoBuy = new System.Windows.Forms.GroupBox();
        this.cbAutoBuyPrivateTicket = new System.Windows.Forms.CheckBox();
        this.cbAutoBuyTdlt = new System.Windows.Forms.CheckBox();
        this.cbAutoBuyKhauTrang = new System.Windows.Forms.CheckBox();
        this.numBuyKhauTrangQty = new System.Windows.Forms.NumericUpDown();
        this.cbAutoBuyCoBonLa = new System.Windows.Forms.CheckBox();
        this.numBuyCoBonLaQty = new System.Windows.Forms.NumericUpDown();
        this.cbAutoBuyBuaDe = new System.Windows.Forms.CheckBox();
        this.numBuyBuaDeQty = new System.Windows.Forms.NumericUpDown();
        // Auto Sell Instantiations
        this.grpSellOptions = new System.Windows.Forms.GroupBox();
        this.grpSellKeepFilter = new System.Windows.Forms.GroupBox();
        this.grpSellCustom = new System.Windows.Forms.GroupBox();
        this.chkAutoSellTrash = new System.Windows.Forms.CheckBox();
        this.lblSellEmptySlots = new System.Windows.Forms.Label();
        this.nudSellEmptySlots = new System.Windows.Forms.NumericUpDown();
        this.chkDropInsteadOfSell = new System.Windows.Forms.CheckBox();
        this.chkKeepStarItems = new System.Windows.Forms.CheckBox();
        this.chkKeepGodItems = new System.Windows.Forms.CheckBox();
        this.chkKeepSkhItems = new System.Windows.Forms.CheckBox();
        this.lblSellMaxLevel = new System.Windows.Forms.Label();
        this.nudSellMaxLevel = new System.Windows.Forms.NumericUpDown();
        this.lblSellKeepIds = new System.Windows.Forms.Label();
        this.txtSellKeepIds = new System.Windows.Forms.TextBox();
        this.chkSellCustomNoStarCheck = new System.Windows.Forms.CheckBox();
        this.lblSellCustomDesc = new System.Windows.Forms.Label();
        this.txtSellCustomIdsList = new System.Windows.Forms.TextBox();
        this.chkAutoBuyCustom = new System.Windows.Forms.CheckBox();
        this.btnBuyCustomHelp = new System.Windows.Forms.Button();
        this.txtBuyCustomList = new System.Windows.Forms.TextBox();
        this.tabGeneral = new System.Windows.Forms.TabPage();
        this.tabControlGeneral = new System.Windows.Forms.TabControl();
        this.tabBasic = new System.Windows.Forms.TabPage();
        this.tabConfigManagement = new System.Windows.Forms.TabPage();
        this.btnCopyConfig = new System.Windows.Forms.Button();
        this.btnPasteConfigCurrent = new System.Windows.Forms.Button();
        this.btnPasteConfigChecked = new System.Windows.Forms.Button();
        this.btnPasteConfigAll = new System.Windows.Forms.Button();
        this.btnPasteConfigByType = new System.Windows.Forms.Button();
        this.nudPasteConfigTypeAccount = new System.Windows.Forms.NumericUpDown();
        this.nudFilterTypeAccount = new System.Windows.Forms.NumericUpDown();
        this.lblConfigClipboard = new System.Windows.Forms.Label();
        this.lblTypeAccountFilter = new System.Windows.Forms.Label();
        this.chkEatChicken = new System.Windows.Forms.CheckBox();
        this.chkUseTdltXmap = new System.Windows.Forms.CheckBox();
        this.lblActionOnDeath = new System.Windows.Forms.Label();
        this.cboActionOnDeath = new System.Windows.Forms.ComboBox();
        // tabConnection (Kết nối - Proxy)
        this.tabConnection = new System.Windows.Forms.TabPage();
        this.chkUseProxy = new System.Windows.Forms.CheckBox();
        this.lblProxyType = new System.Windows.Forms.Label();
        this.cboProxyType = new System.Windows.Forms.ComboBox();
        this.lblProxyAddress = new System.Windows.Forms.Label();
        this.txtProxyAddress = new System.Windows.Forms.TextBox();
        this.btnTestProxy = new System.Windows.Forms.Button();
        this.lblProxyStatus = new System.Windows.Forms.Label();
        this.chkTrainEnable = new System.Windows.Forms.CheckBox();
        this.lblTrainMapId = new System.Windows.Forms.Label();
        this.cboTrainMapId = new System.Windows.Forms.ComboBox();
        this.chkTrainZoneRequire = new System.Windows.Forms.CheckBox();
        this.txtTrainZone = new System.Windows.Forms.TextBox();
        // tabTrainBasic instantiation
        this.chkUseTDLT = new System.Windows.Forms.CheckBox();
        this.chkCheckLagMob = new System.Windows.Forms.CheckBox();
        this.chkOnlyUsePunch = new System.Windows.Forms.CheckBox();
        this.chkFreezePunchSkillCd = new System.Windows.Forms.CheckBox();
        this.chkUseKaiokenLienHoan = new System.Windows.Forms.CheckBox();
        this.chkAvoidSuperMob = new System.Windows.Forms.CheckBox();
        this.chkChangeLowPlayerZoneIfNoMob = new System.Windows.Forms.CheckBox();
        this.lblMobTargetType = new System.Windows.Forms.Label();
        this.cboMobTargetType = new System.Windows.Forms.ComboBox();
        this.lblMobIds = new System.Windows.Forms.Label();
        this.txtMobIds = new System.Windows.Forms.TextBox();
        this.lblTrainingArmorMode = new System.Windows.Forms.Label();
        this.cboTrainingArmorMode = new System.Windows.Forms.ComboBox();

        this.tabItemBongTaiCoDen = new System.Windows.Forms.TabPage();
        this.grpAutoBongTai = new System.Windows.Forms.GroupBox();
        this.lblBongTaiState = new System.Windows.Forms.Label();
        this.cboBongTaiState = new System.Windows.Forms.ComboBox();
        this.lblBongTaiPetAction = new System.Windows.Forms.Label();
        this.cboBongTaiPetAction = new System.Windows.Forms.ComboBox();
        this.lblBongTaiWarning = new System.Windows.Forms.Label();
        this.grpAutoCoDen = new System.Windows.Forms.GroupBox();
        this.chkAutoCoDen = new System.Windows.Forms.CheckBox();
        this.cboFlagType = new System.Windows.Forms.ComboBox();
        this.chkDisableCoDenIfOthers = new System.Windows.Forms.CheckBox();
        // tabControlAccountInfo SuspendLayout
        this.tabControlAccountInfo.SuspendLayout();
        this.tabAccountSuPhu.SuspendLayout();
        this.tabAccountDeTu.SuspendLayout();
        this.tabAccountHanhTrang.SuspendLayout();
        this.panelHanhTrangTop.SuspendLayout();
        this.grpHanhTrangSummary.SuspendLayout();
        // Form layout initialization setup
        this.panelTop.SuspendLayout();
        this.grpSystemInfo.SuspendLayout();
        this.panelMiddle.SuspendLayout();
        this.panelAccountActions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvAccounts)).BeginInit();
        // numAutoLoginThread initialization does not need BeginInit for TextBox
        ((System.ComponentModel.ISupportInitialize)(this.nudStoreStarCount)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.nudSellEmptySlots)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.nudSellMaxLevel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numBuyKhauTrangQty)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numBuyCoBonLaQty)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numBuyBuaDeQty)).BeginInit();
        
        this.chkStoreCustom = new System.Windows.Forms.CheckBox();
        this.txtStoreCustomList = new System.Windows.Forms.TextBox();
        // Auto Use Item Checkboxes
        this.chkUseCuongNo = new System.Windows.Forms.CheckBox();
        this.chkUseBoHuyet = new System.Windows.Forms.CheckBox();
        this.chkUseBoKhi = new System.Windows.Forms.CheckBox();
        this.chkUseGiapXen = new System.Windows.Forms.CheckBox();
        this.chkUseMask = new System.Windows.Forms.CheckBox();
        this.chkUse4LeafClover = new System.Windows.Forms.CheckBox();
        this.chkUseFood = new System.Windows.Forms.CheckBox();
        this.chkUseDetector = new System.Windows.Forms.CheckBox();
        this.chkUseItemById = new System.Windows.Forms.CheckBox();
        this.lblItemByIds = new System.Windows.Forms.Label();
        this.txtItemByIds = new System.Windows.Forms.TextBox();
        this.grpStoreFilter.SuspendLayout();
        this.panelBottom.SuspendLayout();
        this.tabControlFeatures.SuspendLayout();
        this.tabTrain.SuspendLayout();
        this.tabControlTrain.SuspendLayout();
        this.tabItemManagement.SuspendLayout();
        this.tabControlItem.SuspendLayout();
        this.tabItemBuy.SuspendLayout();
        this.tabItemSell.SuspendLayout();
        this.grpAutoBuy.SuspendLayout();
        this.grpSellOptions.SuspendLayout();
        this.grpSellKeepFilter.SuspendLayout();
        this.grpSellCustom.SuspendLayout();
        this.tabItemBongTaiCoDen.SuspendLayout();
        this.grpAutoBongTai.SuspendLayout();
        this.grpAutoCoDen.SuspendLayout();
        this.tabGeneral.SuspendLayout();
        this.tabControlGeneral.SuspendLayout();
        this.tabBasic.SuspendLayout();
        this.tabConfigManagement.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudPasteConfigTypeAccount)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.nudFilterTypeAccount)).BeginInit();
        this.grpAccountInfo.SuspendLayout();
        this.SuspendLayout();

        // panelTop
        this.panelTop.Controls.Add(this.btnToggleGame);
        this.panelTop.Controls.Add(this.btnAdd);
        this.panelTop.Controls.Add(this.btnEdit);
        this.panelTop.Controls.Add(this.btnDelete);
        this.panelTop.Controls.Add(this.btnSelectAll);
        this.panelTop.Controls.Add(this.btnCloseAll);
        this.panelTop.Controls.Add(this.btnHideAll);
        this.panelTop.Controls.Add(this.btnSettings);
        this.panelTop.Controls.Add(this.btnArrangeWindows);
        this.panelTop.Controls.Add(this.btnDeleteSelected);
        this.panelTop.Controls.Add(this.chkHideAccount);
        this.panelTop.Controls.Add(this.chkAutoLogin);
        this.panelTop.Controls.Add(this.chkAutoHideClient);
        this.panelTop.Controls.Add(this.numAutoLoginThread);
        this.panelTop.Controls.Add(this.lblSocketStatus);
        this.panelTop.Controls.Add(this.grpSystemInfo);
        this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
        this.panelTop.Location = new System.Drawing.Point(0, 0);
        this.panelTop.Name = "panelTop";
        this.panelTop.Size = new System.Drawing.Size(790, 100);

        // btnToggleGame
        this.btnToggleGame.Location = new System.Drawing.Point(12, 12);
        this.btnToggleGame.Size = new System.Drawing.Size(85, 26);
        this.btnToggleGame.Text = "Bật Game";

        // btnAdd
        this.btnAdd.Location = new System.Drawing.Point(107, 12);
        this.btnAdd.Size = new System.Drawing.Size(85, 26);
        this.btnAdd.Text = "Thêm";

        // btnEdit
        this.btnEdit.Location = new System.Drawing.Point(202, 12);
        this.btnEdit.Size = new System.Drawing.Size(85, 26);
        this.btnEdit.Text = "Sửa";

        // btnDelete
        this.btnDelete.Location = new System.Drawing.Point(297, 12);
        this.btnDelete.Size = new System.Drawing.Size(85, 26);
        this.btnDelete.Text = "Xóa";

        // btnSelectAll
        this.btnSelectAll.Location = new System.Drawing.Point(392, 12);
        this.btnSelectAll.Size = new System.Drawing.Size(85, 26);
        this.btnSelectAll.Text = "Select All";
        this.btnSelectAll.Click += new System.EventHandler(this.BtnSelectAll_Click);

        // btnCloseAll
        this.btnCloseAll.Location = new System.Drawing.Point(12, 44);
        this.btnCloseAll.Size = new System.Drawing.Size(85, 26);
        this.btnCloseAll.Text = "Tắt tất cả";

        // btnHideAll
        this.btnHideAll.Location = new System.Drawing.Point(107, 44);
        this.btnHideAll.Size = new System.Drawing.Size(85, 26);
        this.btnHideAll.Text = "Ẩn tất cả";

        // btnSettings
        this.btnSettings.Location = new System.Drawing.Point(202, 44);
        this.btnSettings.Size = new System.Drawing.Size(85, 26);
        this.btnSettings.Text = "Cài Đặt";
        this.btnSettings.Click += new System.EventHandler(this.BtnSettings_Click);

        // btnArrangeWindows
        this.btnArrangeWindows.Location = new System.Drawing.Point(297, 44);
        this.btnArrangeWindows.Size = new System.Drawing.Size(85, 26);
        this.btnArrangeWindows.Text = "Sắp xếp";
        this.btnArrangeWindows.Click += new System.EventHandler(this.BtnArrangeWindows_Click);

        // btnDeleteSelected
        this.btnDeleteSelected.Location = new System.Drawing.Point(392, 44);
        this.btnDeleteSelected.Size = new System.Drawing.Size(85, 26);
        this.btnDeleteSelected.Text = "Xóa Chọn";
        this.btnDeleteSelected.Name = "btnDeleteSelected";

        // chkHideAccount
        this.chkHideAccount.AutoSize = true;
        this.chkHideAccount.Location = new System.Drawing.Point(680, 19);
        this.chkHideAccount.Name = "chkHideAccount";
        this.chkHideAccount.Size = new System.Drawing.Size(91, 19);
        this.chkHideAccount.TabIndex = 6;
        this.chkHideAccount.Text = "Ẩn tài khoản";

        // chkAutoLogin
        this.chkAutoLogin.AutoSize = true;
        this.chkAutoLogin.Location = new System.Drawing.Point(680, 45);
        this.chkAutoLogin.Name = "chkAutoLogin";
        this.chkAutoLogin.Size = new System.Drawing.Size(61, 19);
        this.chkAutoLogin.TabIndex = 7;
        this.chkAutoLogin.Text = "Tự mở";

        // numAutoLoginThread
        this.numAutoLoginThread.Location = new System.Drawing.Point(745, 43);
        this.numAutoLoginThread.Name = "numAutoLoginThread";
        this.numAutoLoginThread.Size = new System.Drawing.Size(35, 23);
        this.numAutoLoginThread.TabIndex = 8;
        this.numAutoLoginThread.Text = "2";
        this.numAutoLoginThread.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

        // chkAutoHideClient
        this.chkAutoHideClient.AutoSize = true;
        this.chkAutoHideClient.Location = new System.Drawing.Point(680, 70);
        this.chkAutoHideClient.Name = "chkAutoHideClient";
        this.chkAutoHideClient.Size = new System.Drawing.Size(91, 19);
        this.chkAutoHideClient.TabIndex = 8;
        this.chkAutoHideClient.Text = "Tự ẩn client";

        // lblSocketStatus
        this.lblSocketStatus.AutoSize = true;
        this.lblSocketStatus.Location = new System.Drawing.Point(5, 75);
        this.lblSocketStatus.Name = "lblSocketStatus";
        this.lblSocketStatus.Size = new System.Drawing.Size(0, 15);
        this.lblSocketStatus.TabIndex = 9;

        // grpSystemInfo
        this.grpSystemInfo.Controls.Add(this.lblSystemTime);
        this.grpSystemInfo.Controls.Add(this.lblAccountStats);
        this.grpSystemInfo.Controls.Add(this.lblSystemStats);
        this.grpSystemInfo.Controls.Add(this.btnMoveUp);
        this.grpSystemInfo.Controls.Add(this.btnMoveDown);
        this.grpSystemInfo.Controls.Add(this.btnCleanRam);
        this.grpSystemInfo.Location = new System.Drawing.Point(490, 5);
        this.grpSystemInfo.Name = "grpSystemInfo";
        this.grpSystemInfo.Size = new System.Drawing.Size(180, 90);
        this.grpSystemInfo.TabIndex = 10;
        this.grpSystemInfo.TabStop = false;
        this.lblAccountStats.Location = new System.Drawing.Point(8, 30);
        this.lblAccountStats.Size = new System.Drawing.Size(130, 15);
        this.lblAccountStats.Text = "On/Log/Total: 0 / 0 / 0";

        // lblSystemStats
        this.lblSystemStats.AutoSize = true;
        this.lblSystemStats.Location = new System.Drawing.Point(8, 45);
        this.lblSystemStats.Name = "lblSystemStats";
        this.lblSystemStats.Size = new System.Drawing.Size(125, 15);
        this.lblSystemStats.Text = "RAM: 0% - CPU: 0.0%";

        // lblSystemTime
        this.lblSystemTime.AutoSize = true;
        this.lblSystemTime.Location = new System.Drawing.Point(8, 15);
        this.lblSystemTime.Name = "lblSystemTime";
        this.lblSystemTime.Size = new System.Drawing.Size(125, 15);
        this.lblSystemTime.Text = "29/03/2026 22:00:00";

        // btnMoveUp
        this.btnMoveUp.Location = new System.Drawing.Point(8, 62);
        this.btnMoveUp.Name = "btnMoveUp";
        this.btnMoveUp.Size = new System.Drawing.Size(30, 22);
        this.btnMoveUp.Text = "↑";
        this.btnMoveUp.UseVisualStyleBackColor = true;
        this.btnMoveUp.Click += new System.EventHandler(this.BtnMoveUp_Click);

        // btnMoveDown
        this.btnMoveDown.Location = new System.Drawing.Point(42, 62);
        this.btnMoveDown.Name = "btnMoveDown";
        this.btnMoveDown.Size = new System.Drawing.Size(30, 22);
        this.btnMoveDown.Text = "↓";
        this.btnMoveDown.UseVisualStyleBackColor = true;
        this.btnMoveDown.Click += new System.EventHandler(this.BtnMoveDown_Click);

        // btnCleanRam
        this.btnCleanRam.Location = new System.Drawing.Point(78, 62);
        this.btnCleanRam.Name = "btnCleanRam";
        this.btnCleanRam.Size = new System.Drawing.Size(90, 22);
        this.btnCleanRam.Text = "Dọn RAM";
        this.btnCleanRam.UseVisualStyleBackColor = true;
        this.btnCleanRam.Click += new System.EventHandler(this.BtnCleanRam_Click);

        // panelMiddle
        this.panelMiddle.Controls.Add(this.dgvAccounts);
        this.panelMiddle.Dock = System.Windows.Forms.DockStyle.Fill;

        // panelAccountActions
        this.panelAccountActions.Controls.Add(this.btnBatAuto);
        this.panelAccountActions.Controls.Add(this.btnDungAuto);
        this.panelAccountActions.Controls.Add(this.btnBatAutoTatCaChon);
        this.panelAccountActions.Controls.Add(this.btnDungAutoTatCaChon);
        this.panelAccountActions.Controls.Add(this.btnGDVP);
        this.panelAccountActions.Controls.Add(this.btnAutoBoMong);
        this.panelAccountActions.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panelAccountActions.Height = 32;
        this.panelAccountActions.Name = "panelAccountActions";
        this.panelAccountActions.BackColor = System.Drawing.SystemColors.Control;

        // btnBatAuto
        this.btnBatAuto.Location = new System.Drawing.Point(8, 3);
        this.btnBatAuto.Size = new System.Drawing.Size(80, 26);
        this.btnBatAuto.Text = "Bật auto";
        this.btnBatAuto.Name = "btnBatAuto";
        this.btnBatAuto.Click += new System.EventHandler(this.BtnBatAuto_Click);

        // btnDungAuto
        this.btnDungAuto.Location = new System.Drawing.Point(93, 3);
        this.btnDungAuto.Size = new System.Drawing.Size(80, 26);
        this.btnDungAuto.Text = "Tắt auto";
        this.btnDungAuto.Name = "btnDungAuto";
        this.btnDungAuto.Click += new System.EventHandler(this.BtnDungAuto_Click);

        // btnBatAutoTatCaChon
        this.btnBatAutoTatCaChon.Location = new System.Drawing.Point(178, 3);
        this.btnBatAutoTatCaChon.Size = new System.Drawing.Size(110, 26);
        this.btnBatAutoTatCaChon.Text = "Bật Auto(Chọn)";
        this.btnBatAutoTatCaChon.Name = "btnBatAutoTatCaChon";
        this.btnBatAutoTatCaChon.Click += new System.EventHandler(this.BtnBatAutoTatCaChon_Click);

        // btnDungAutoTatCaChon
        this.btnDungAutoTatCaChon.Location = new System.Drawing.Point(293, 3);
        this.btnDungAutoTatCaChon.Size = new System.Drawing.Size(110, 26);
        this.btnDungAutoTatCaChon.Text = "Tắt Auto(Chọn)";
        this.btnDungAutoTatCaChon.Name = "btnDungAutoTatCaChon";
        this.btnDungAutoTatCaChon.Click += new System.EventHandler(this.BtnDungAutoTatCaChon_Click);

        // btnGDVP
        this.btnGDVP.Location = new System.Drawing.Point(408, 3);
        this.btnGDVP.Size = new System.Drawing.Size(60, 26);
        this.btnGDVP.Text = "GDVP";
        this.btnGDVP.Name = "btnGDVP";

        // btnAutoBoMong
        this.btnAutoBoMong.Location = new System.Drawing.Point(473, 3);
        this.btnAutoBoMong.Size = new System.Drawing.Size(100, 26);
        this.btnAutoBoMong.Text = "Auto NVHN";
        this.btnAutoBoMong.Name = "btnAutoBoMong";



        // dgvAccounts
        this.dgvAccounts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvAccounts.RowHeadersVisible = false;
        this.dgvAccounts.AllowUserToAddRows = false;
        this.dgvAccounts.AllowUserToResizeColumns = false;
        this.dgvAccounts.AllowUserToResizeRows = false;
        this.dgvAccounts.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
        this.dgvAccounts.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(0, 120, 215);
        this.dgvAccounts.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
        this.dgvAccounts.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
        this.dgvAccounts.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        this.dgvAccounts.EnableHeadersVisualStyles = false;
        this.dgvAccounts.GridColor = System.Drawing.Color.FromArgb(224, 224, 224);
        this.dgvAccounts.RowTemplate.Height = 28;
        this.dgvAccounts.BackgroundColor = System.Drawing.Color.White;
        this.dgvAccounts.BorderStyle = System.Windows.Forms.BorderStyle.None;
        // Không set ReadOnly ở cấp grid vì sẽ override cả colSelect/colServer
        this.dgvAccounts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSelect, this.colSTT, this.colCharacter, this.colDataInGame, 
            this.colStatus, this.colServer, this.colTypeAccount, this.colAccount, this.colShow
        });
        this.dgvAccounts.Dock = System.Windows.Forms.DockStyle.Fill;

        // Columns setup
        this.colSelect.HeaderText = "Chọn";
        this.colSelect.Width = 55;
        // colSelect.ReadOnly giữ mặc định false => cho phép tick
        this.colSTT.HeaderText = "STT";
        this.colSTT.Width = 45;
        this.colSTT.ReadOnly = true;
        this.colSTT.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
        this.colSTT.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
        this.colCharacter.HeaderText = "Nhân Vật";
        this.colCharacter.Width = 100;
        this.colCharacter.ReadOnly = true;  // Chỉ đọc
        this.colDataInGame.HeaderText = "Data In Game";
        this.colDataInGame.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        this.colDataInGame.FillWeight = 24F;
        this.colDataInGame.ReadOnly = true;  // Chỉ đọc
        this.colStatus.HeaderText = "Trạng Thái";
        this.colStatus.Width = 110;
        this.colStatus.ReadOnly = true;  // Chỉ đọc
        this.colServer.HeaderText = "Server";
        this.colServer.Width = 90;
        this.colServer.ReadOnly = true;
        this.colTypeAccount.HeaderText = "Type";
        this.colTypeAccount.Width = 60;
        this.colTypeAccount.ReadOnly = true;
        this.colTypeAccount.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
        this.colAccount.HeaderText = "Tài Khoản";
        this.colAccount.Width = 110;
        this.colAccount.ReadOnly = true;  // Chỉ đọc
        // colShow – nút SHOW
        this.colShow.HeaderText = "Show";
        this.colShow.Text = "SHOW";
        this.colShow.UseColumnTextForButtonValue = true;
        this.colShow.Width = 65;

        // panelBottom
        this.panelBottom.Controls.Add(this.tabControlFeatures);
        this.panelBottom.Controls.Add(this.grpAccountInfo);
        this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panelBottom.Height = 250;

        // tabControlFeatures
        this.tabControlFeatures.Controls.Add(this.tabTrain);
        this.tabControlFeatures.Controls.Add(this.tabItemManagement);
        this.tabControlFeatures.Controls.Add(this.tabPet);
        this.tabControlFeatures.Controls.Add(this.tabDauThan);
        this.tabControlFeatures.Controls.Add(this.tabBuffNamek);
        this.tabControlFeatures.Controls.Add(this.tabGeneral);
        this.tabControlFeatures.Controls.Add(this.tabConfigManagement);
        this.tabControlFeatures.Dock = System.Windows.Forms.DockStyle.Fill;

        // tabTrain
        this.tabTrain.Controls.Add(this.tabControlTrain);
        this.tabTrain.Text = "Đánh quái(Train)";
        this.tabTrain.Padding = new System.Windows.Forms.Padding(3);

        // tabControlTrain
        this.tabControlTrain.Controls.Add(this.tabTrainBasic);
        this.tabControlTrain.Controls.Add(this.tabTrainMvbt);
        this.tabControlTrain.Controls.Add(this.tabTrainMhbt);
        this.tabControlTrain.Controls.Add(this.tabTrainUpKilis);
        this.tabControlTrain.Controls.Add(this.tabTrainBossVegetaCity);
        this.tabControlTrain.Controls.Add(this.tabTrainAdvanced);
        this.tabControlTrain.Controls.Add(this.tabUpSkh);
        this.tabControlTrain.Dock = System.Windows.Forms.DockStyle.Fill;

        // tabTrainBasic
        this.tabTrainBasic.Controls.Add(this.chkTrainEnable);
        this.tabTrainBasic.Controls.Add(this.lblTrainMapId);
        this.tabTrainBasic.Controls.Add(this.cboTrainMapId);
        this.tabTrainBasic.Controls.Add(this.chkTrainZoneRequire);
        this.tabTrainBasic.Controls.Add(this.txtTrainZone);
        this.tabTrainBasic.Controls.Add(this.chkUseTDLT);
        this.tabTrainBasic.Controls.Add(this.chkCheckLagMob);
        this.tabTrainBasic.Controls.Add(this.chkOnlyUsePunch);
        this.tabTrainBasic.Controls.Add(this.chkFreezePunchSkillCd);
        this.tabTrainBasic.Controls.Add(this.chkUseKaiokenLienHoan);
        this.tabTrainBasic.Controls.Add(this.chkAvoidSuperMob);
        this.tabTrainBasic.Controls.Add(this.chkChangeLowPlayerZoneIfNoMob);
        this.tabTrainBasic.Controls.Add(this.lblMobTargetType);
        this.tabTrainBasic.Controls.Add(this.cboMobTargetType);
        this.tabTrainBasic.Controls.Add(this.lblMobIds);
        this.tabTrainBasic.Controls.Add(this.txtMobIds);
        this.tabTrainBasic.Controls.Add(this.lblTrainingArmorMode);
        this.tabTrainBasic.Controls.Add(this.cboTrainingArmorMode);
        this.tabTrainBasic.Text = "Cơ bản";
        this.tabTrainBasic.Padding = new System.Windows.Forms.Padding(3);
        this.tabTrainBasic.AutoScroll = true;

        // Row 1
        // chkTrainEnable
        this.chkTrainEnable.AutoSize = true;
        this.chkTrainEnable.Location = new System.Drawing.Point(15, 15);
        this.chkTrainEnable.Size = new System.Drawing.Size(81, 19);
        this.chkTrainEnable.Text = "Đánh quái";

        // Row 2
        // lblTrainMapId
        this.lblTrainMapId.AutoSize = true;
        this.lblTrainMapId.Location = new System.Drawing.Point(15, 45);
        this.lblTrainMapId.Size = new System.Drawing.Size(48, 15);
        this.lblTrainMapId.Text = "ID Map:";

        // cboTrainMapId
        this.cboTrainMapId.Location = new System.Drawing.Point(70, 42);
        this.cboTrainMapId.Size = new System.Drawing.Size(195, 23);
        this.cboTrainMapId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;

        // chkTrainZoneRequire
        this.chkTrainZoneRequire.AutoSize = true;
        this.chkTrainZoneRequire.Location = new System.Drawing.Point(275, 44);
        this.chkTrainZoneRequire.Size = new System.Drawing.Size(55, 19);
        this.chkTrainZoneRequire.Text = "Zone:";

        // txtTrainZone
        this.txtTrainZone.Location = new System.Drawing.Point(340, 42);
        this.txtTrainZone.Size = new System.Drawing.Size(45, 23);
        this.txtTrainZone.Text = "-1";

        // Row 3
        // chkChangeLowPlayerZoneIfNoMob
        this.chkChangeLowPlayerZoneIfNoMob.AutoSize = true;
        this.chkChangeLowPlayerZoneIfNoMob.Location = new System.Drawing.Point(15, 75);
        this.chkChangeLowPlayerZoneIfNoMob.Size = new System.Drawing.Size(80, 19);
        this.chkChangeLowPlayerZoneIfNoMob.Text = "Auto zone";

        // Row 4
        // chkUseTDLT
        this.chkUseTDLT.AutoSize = true;
        this.chkUseTDLT.Location = new System.Drawing.Point(15, 105);
        this.chkUseTDLT.Size = new System.Drawing.Size(81, 19);
        this.chkUseTDLT.Text = "Dùng TĐLT";

        // chkOnlyUsePunch
        this.chkOnlyUsePunch.AutoSize = true;
        this.chkOnlyUsePunch.Location = new System.Drawing.Point(210, 105);
        this.chkOnlyUsePunch.Size = new System.Drawing.Size(120, 19);
        this.chkOnlyUsePunch.Text = "Chỉ dùng chiêu đấm";

        // Row 5
        // chkFreezePunchSkillCd
        this.chkFreezePunchSkillCd.AutoSize = true;
        this.chkFreezePunchSkillCd.Location = new System.Drawing.Point(15, 135);
        this.chkFreezePunchSkillCd.Size = new System.Drawing.Size(100, 19);
        this.chkFreezePunchSkillCd.Name = "chkFreezePunchSkillCd";
        this.chkFreezePunchSkillCd.Text = "Đóng băng skill";

        // chkUseKaiokenLienHoan
        this.chkUseKaiokenLienHoan.AutoSize = true;
        this.chkUseKaiokenLienHoan.Location = new System.Drawing.Point(210, 135);
        this.chkUseKaiokenLienHoan.Size = new System.Drawing.Size(155, 19);
        this.chkUseKaiokenLienHoan.Text = "Ưu tiên Kaioken/Liên Hoàn";

        // Row 6
        // chkAvoidSuperMob
        this.chkAvoidSuperMob.AutoSize = true;
        this.chkAvoidSuperMob.Location = new System.Drawing.Point(15, 165);
        this.chkAvoidSuperMob.Size = new System.Drawing.Size(90, 19);
        this.chkAvoidSuperMob.Text = "Né siêu quái";

        // chkCheckLagMob
        this.chkCheckLagMob.AutoSize = true;
        this.chkCheckLagMob.Location = new System.Drawing.Point(210, 165);
        this.chkCheckLagMob.Size = new System.Drawing.Size(100, 19);
        this.chkCheckLagMob.Text = "Đổi MT khi lag";

        // Row 7
        // lblMobTargetType
        this.lblMobTargetType.AutoSize = true;
        this.lblMobTargetType.Location = new System.Drawing.Point(15, 195);
        this.lblMobTargetType.Size = new System.Drawing.Size(60, 15);
        this.lblMobTargetType.Text = "Loại quái:";

        // cboMobTargetType
        this.cboMobTargetType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboMobTargetType.FormattingEnabled = true;
        this.cboMobTargetType.Items.AddRange(new object[] { "Đánh tất cả", "Theo id" 
        });
        this.cboMobTargetType.Location = new System.Drawing.Point(80, 192);
        this.cboMobTargetType.Size = new System.Drawing.Size(110, 23);

        // lblMobIds
        this.lblMobIds.AutoSize = true;
        this.lblMobIds.Location = new System.Drawing.Point(210, 195);
        this.lblMobIds.Size = new System.Drawing.Size(50, 15);
        this.lblMobIds.Text = "ID quái:";

        // txtMobIds
        this.txtMobIds.Location = new System.Drawing.Point(275, 192);
        this.txtMobIds.Size = new System.Drawing.Size(100, 23);
        this.txtMobIds.PlaceholderText = "VD: 1,2,3";

        // Row 8
        // lblTrainingArmorMode
        this.lblTrainingArmorMode.AutoSize = true;
        this.lblTrainingArmorMode.Location = new System.Drawing.Point(15, 225);
        this.lblTrainingArmorMode.Size = new System.Drawing.Size(80, 15);
        this.lblTrainingArmorMode.Text = "Giáp LT:";

        // cboTrainingArmorMode
        this.cboTrainingArmorMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboTrainingArmorMode.FormattingEnabled = true;
        this.cboTrainingArmorMode.Items.AddRange(new object[] {
            "Không chạy", "Mặc giáp", "Tháo giáp"
        });
        this.cboTrainingArmorMode.Location = new System.Drawing.Point(80, 222);
        this.cboTrainingArmorMode.Size = new System.Drawing.Size(130, 23);
        this.cboTrainingArmorMode.Name = "cboTrainingArmorMode";

        // tabTrainMvbt
        this.mvbtControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.mvbtControl.Name = "mvbtControl";
        this.mvbtControl.Title = "Cấu hình Mảnh vỡ bông tai";
        this.mvbtControl.MasterCheckboxText = "Auto MVBT";
        this.tabTrainMvbt.Controls.Add(this.mvbtControl);
        this.tabTrainMvbt.Text = "MVBT";
        this.tabTrainMvbt.Padding = new System.Windows.Forms.Padding(3);

        // tabTrainMhbt
        this.mhbtControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.mhbtControl.Name = "mhbtControl";
        this.mhbtControl.Title = "Cấu hình Mảnh hồn bông tai";
        this.mhbtControl.MasterCheckboxText = "Auto MHBT";
        this.tabTrainMhbt.Controls.Add(this.mhbtControl);
        this.tabTrainMhbt.Text = "MHBT";
        this.tabTrainMhbt.Padding = new System.Windows.Forms.Padding(3);

        // tabTrainUpKilis
        this.kilisControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.kilisControl.Name = "kilisControl";
        this.tabTrainUpKilis.Controls.Add(this.kilisControl);
        this.tabTrainUpKilis.Text = "Úp kilis";
        this.tabTrainUpKilis.Padding = new System.Windows.Forms.Padding(3);

        // tabTrainBossVegetaCity
        this.tabTrainBossVegetaCity.Controls.Add(this.chkBossVegetaCityEnable);
        this.tabTrainBossVegetaCity.Controls.Add(this.chkBossVegetaCityAuto3h);
        this.tabTrainBossVegetaCity.Controls.Add(this.chkBossVegetaCityAuto2230);
        this.tabTrainBossVegetaCity.Controls.Add(this.chkBossVegetaCityReviveByGem);
        this.tabTrainBossVegetaCity.Controls.Add(this.chkBossVegetaCityUseTdlt);
        this.tabTrainBossVegetaCity.Text = "Boss 22h";
        this.tabTrainBossVegetaCity.Padding = new System.Windows.Forms.Padding(3);
        this.tabTrainBossVegetaCity.AutoScroll = true;

        // chkBossVegetaCityEnable
        this.chkBossVegetaCityEnable.AutoSize = true;
        this.chkBossVegetaCityEnable.Location = new System.Drawing.Point(15, 18);
        this.chkBossVegetaCityEnable.Name = "chkBossVegetaCityEnable";
        this.chkBossVegetaCityEnable.Size = new System.Drawing.Size(136, 20);
        this.chkBossVegetaCityEnable.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.chkBossVegetaCityEnable.ForeColor = System.Drawing.Color.Red;
        this.chkBossVegetaCityEnable.Text = "Bật auto Boss 22h";

        // chkBossVegetaCityAuto3h
        this.chkBossVegetaCityAuto3h.AutoSize = true;
        this.chkBossVegetaCityAuto3h.Location = new System.Drawing.Point(15, 48);
        this.chkBossVegetaCityAuto3h.Name = "chkBossVegetaCityAuto3h";
        this.chkBossVegetaCityAuto3h.Size = new System.Drawing.Size(111, 19);
        this.chkBossVegetaCityAuto3h.Text = "Auto khung giờ 3h";

        // chkBossVegetaCityAuto2230
        this.chkBossVegetaCityAuto2230.AutoSize = true;
        this.chkBossVegetaCityAuto2230.Location = new System.Drawing.Point(15, 78);
        this.chkBossVegetaCityAuto2230.Name = "chkBossVegetaCityAuto2230";
        this.chkBossVegetaCityAuto2230.Size = new System.Drawing.Size(129, 19);
        this.chkBossVegetaCityAuto2230.Text = "Auto khung giờ 22h30";

        // chkBossVegetaCityReviveByGem
        this.chkBossVegetaCityReviveByGem.AutoSize = true;
        this.chkBossVegetaCityReviveByGem.Location = new System.Drawing.Point(15, 108);
        this.chkBossVegetaCityReviveByGem.Name = "chkBossVegetaCityReviveByGem";
        this.chkBossVegetaCityReviveByGem.Size = new System.Drawing.Size(91, 19);
        this.chkBossVegetaCityReviveByGem.Text = "Hồi sinh ngọc";

        // chkBossVegetaCityUseTdlt
        this.chkBossVegetaCityUseTdlt.AutoSize = true;
        this.chkBossVegetaCityUseTdlt.Location = new System.Drawing.Point(15, 138);
        this.chkBossVegetaCityUseTdlt.Name = "chkBossVegetaCityUseTdlt";
        this.chkBossVegetaCityUseTdlt.Size = new System.Drawing.Size(80, 19);
        this.chkBossVegetaCityUseTdlt.Text = "Dùng TĐLT";

        // tabTrainAdvanced
        this.tabTrainAdvanced.Text = "Nâng cao";
        this.tabTrainAdvanced.Padding = new System.Windows.Forms.Padding(3);

        // tabUpSkh
        this.upSkhControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.upSkhControl.Name = "upSkhControl";
        this.tabUpSkh.Controls.Add(this.upSkhControl);
        this.tabUpSkh.Text = "Up SKH";
        this.tabUpSkh.Padding = new System.Windows.Forms.Padding(3);

        // tabPet
        this.petControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.petControl.Name = "petControl";
        this.tabPet.Controls.Add(this.petControl);
        this.tabPet.Text = "Úp đệ";
        this.tabPet.Padding = new System.Windows.Forms.Padding(3);

        // tabDauThan
        this.dauThanControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dauThanControl.Name = "dauThanControl";
        this.tabDauThan.Controls.Add(this.dauThanControl);
        this.tabDauThan.Text = "Auto đậu thần";
        this.tabDauThan.Padding = new System.Windows.Forms.Padding(3);

        // tabBuffNamek
        this.buffNamekControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.buffNamekControl.Name = "buffNamekControl";
        this.tabBuffNamek.Controls.Add(this.buffNamekControl);
        this.tabBuffNamek.Text = "Buff Namek";
        this.tabBuffNamek.Padding = new System.Windows.Forms.Padding(3);
        // tabItemManagement
        this.tabItemManagement.Controls.Add(this.tabControlItem);
        this.tabItemManagement.Text = "Quản lí item";
        this.tabItemManagement.Padding = new System.Windows.Forms.Padding(3);

        // tabControlItem
        // 
        this.tabControlItem.Controls.Add(this.tabItemUse);
        this.tabControlItem.Controls.Add(this.tabItemDrop);
        this.tabControlItem.Controls.Add(this.tabItemStore);
        this.tabControlItem.Controls.Add(this.tabItemSell);
        this.tabControlItem.Controls.Add(this.tabItemBuy);
        this.tabControlItem.Controls.Add(this.tabItemPick);
        this.tabControlItem.Controls.Add(this.tabItemBongTaiCoDen);
        this.tabControlItem.Dock = System.Windows.Forms.DockStyle.Fill;

        // tabItemUse
        this.tabItemUse.Controls.Add(this.chkUseCuongNo);
        this.tabItemUse.Controls.Add(this.chkUseBoHuyet);
        this.tabItemUse.Controls.Add(this.chkUseBoKhi);
        this.tabItemUse.Controls.Add(this.chkUseGiapXen);
        this.tabItemUse.Controls.Add(this.chkUseMask);
        this.tabItemUse.Controls.Add(this.chkUse4LeafClover);
        this.tabItemUse.Controls.Add(this.chkUseFood);
        this.tabItemUse.Controls.Add(this.chkUseDetector);
        this.tabItemUse.Controls.Add(this.chkUseItemById);
        this.tabItemUse.Controls.Add(this.lblItemByIds);
        this.tabItemUse.Controls.Add(this.txtItemByIds);
        this.tabItemUse.Text = "Dùng";
        this.tabItemUse.Padding = new System.Windows.Forms.Padding(3);

        // chkUseCuongNo
        this.chkUseCuongNo.AutoSize = true;
        this.chkUseCuongNo.Location = new System.Drawing.Point(13, 13);
        this.chkUseCuongNo.Text = "Cuồng Nộ";
        this.chkUseCuongNo.Name = "chkUseCuongNo";

        // chkUseBoHuyet
        this.chkUseBoHuyet.AutoSize = true;
        this.chkUseBoHuyet.Location = new System.Drawing.Point(165, 13);
        this.chkUseBoHuyet.Text = "Bổ Huyết";
        this.chkUseBoHuyet.Name = "chkUseBoHuyet";

        // chkUseBoKhi
        this.chkUseBoKhi.AutoSize = true;
        this.chkUseBoKhi.Location = new System.Drawing.Point(13, 40);
        this.chkUseBoKhi.Text = "Bổ Khí";
        this.chkUseBoKhi.Name = "chkUseBoKhi";

        // chkUseGiapXen
        this.chkUseGiapXen.AutoSize = true;
        this.chkUseGiapXen.Location = new System.Drawing.Point(165, 40);
        this.chkUseGiapXen.Text = "Giáp Xên";
        this.chkUseGiapXen.Name = "chkUseGiapXen";

        // chkUseMask
        this.chkUseMask.AutoSize = true;
        this.chkUseMask.Location = new System.Drawing.Point(13, 67);
        this.chkUseMask.Text = "Khẩu Trang";
        this.chkUseMask.Name = "chkUseMask";

        // chkUse4LeafClover
        this.chkUse4LeafClover.AutoSize = true;
        this.chkUse4LeafClover.Location = new System.Drawing.Point(165, 67);
        this.chkUse4LeafClover.Text = "Cỏ may mắn";
        this.chkUse4LeafClover.Name = "chkUse4LeafClover";

        // chkUseFood
        this.chkUseFood.AutoSize = true;
        this.chkUseFood.Location = new System.Drawing.Point(13, 100);
        this.chkUseFood.Text = "Thức ăn (Kem dâu/Mì ly/Xúc xích/Sushi/Pudding)";
        this.chkUseFood.Name = "chkUseFood";

        // chkUseDetector
        this.chkUseDetector.AutoSize = true;
        this.chkUseDetector.Location = new System.Drawing.Point(13, 127);
        this.chkUseDetector.Text = "Máy dò Capsule kì bí";
        this.chkUseDetector.Name = "chkUseDetector";

        // chkUseItemById
        this.chkUseItemById.AutoSize = true;
        this.chkUseItemById.Checked = false;
        this.chkUseItemById.Location = new System.Drawing.Point(13, 155);
        this.chkUseItemById.Text = "Dùng item tùy chỉnh";
        this.chkUseItemById.Name = "chkUseItemById";

        // lblItemByIds
        this.lblItemByIds.AutoSize = true;
        this.lblItemByIds.Location = new System.Drawing.Point(13, 180);
        this.lblItemByIds.Text = "Tùy chỉnh (mỗi dòng: itemId-intervalMs, VD: 379-60000):";
        this.lblItemByIds.Name = "lblItemByIds";

        // txtItemByIds
        this.txtItemByIds.Location = new System.Drawing.Point(13, 197);
        this.txtItemByIds.Size = new System.Drawing.Size(300, 70);
        this.txtItemByIds.Multiline = true;
        this.txtItemByIds.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtItemByIds.Name = "txtItemByIds";

        // tabItemDrop
        this.tabItemDrop.Controls.Add(this.grpAutoDrop);
        this.tabItemDrop.Text = "Vứt";
        // tabItemStore
        this.tabItemStore.Controls.Add(this.chkAutoStoreWhenFull);
        this.tabItemStore.Controls.Add(this.grpStoreFilter);
        this.tabItemStore.Text = "Cất";

        // chkAutoStoreWhenFull
        this.chkAutoStoreWhenFull.AutoSize = true;
        this.chkAutoStoreWhenFull.Location = new System.Drawing.Point(13, 13);
        this.chkAutoStoreWhenFull.Text = "Tự động Cất đồ VIP (Ngay lập tức)";

        // grpStoreFilter
        this.grpStoreFilter.Controls.Add(this.chkStoreKichHoat);
        this.grpStoreFilter.Controls.Add(this.chkStoreThanLinh);
        this.grpStoreFilter.Controls.Add(this.chkStorePhaLe);
        this.grpStoreFilter.Controls.Add(this.lblStarCount);
        this.grpStoreFilter.Controls.Add(this.nudStoreStarCount);
        this.grpStoreFilter.Location = new System.Drawing.Point(13, 40);
        this.grpStoreFilter.Size = new System.Drawing.Size(260, 150);
        this.grpStoreFilter.Text = "Bộ lọc Đồ Cất";

        // chkStoreCustom
        this.chkStoreCustom.AutoSize = true;
        this.chkStoreCustom.Location = new System.Drawing.Point(13, 195);
        this.chkStoreCustom.Text = "Cất theo config,Mỗi dòng: id vp-số lượng(đủ sẽ đi cất):";

        // txtStoreCustomList
        this.txtStoreCustomList.Location = new System.Drawing.Point(13, 220);
        this.txtStoreCustomList.Multiline = true;
        this.txtStoreCustomList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtStoreCustomList.Size = new System.Drawing.Size(260, 100);

        this.tabItemStore.Controls.Add(this.chkStoreCustom);
        this.tabItemStore.Controls.Add(this.txtStoreCustomList);

        // chkStoreKichHoat
        this.chkStoreKichHoat.AutoSize = true;
        this.chkStoreKichHoat.Location = new System.Drawing.Point(15, 25);
        this.chkStoreKichHoat.Text = "Cất đồ Kích hoạt";

        // chkStoreThanLinh
        this.chkStoreThanLinh.AutoSize = true;
        this.chkStoreThanLinh.Location = new System.Drawing.Point(15, 55);
        this.chkStoreThanLinh.Text = "Cất đồ Thần linh (TL)";

        // chkStorePhaLe
        this.chkStorePhaLe.AutoSize = true;
        this.chkStorePhaLe.Location = new System.Drawing.Point(15, 85);
        this.chkStorePhaLe.Text = "Cất đồ Sao pha lê";

        // lblStarCount
        this.lblStarCount.AutoSize = true;
        this.lblStarCount.Location = new System.Drawing.Point(15, 115);
        this.lblStarCount.Text = "Số sao tối thiểu cần cất:";

        // nudStoreStarCount
        this.nudStoreStarCount.Location = new System.Drawing.Point(160, 113);
        this.nudStoreStarCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this.nudStoreStarCount.Maximum = new decimal(new int[] { 9, 0, 0, 0 });
        this.nudStoreStarCount.Size = new System.Drawing.Size(50, 23);
        this.nudStoreStarCount.Value = new decimal(new int[] { 1, 0, 0, 0 });

        // ── tabItemSell ────────────────────────────────────────────────────
        //
        // Layout:
        //  [✓] Tự bán đồ rác   Còn trống ≤ [N]   [✓] Vứt tại chỗ
        //  ┌─ Đồ GIỮ (không bán) ───────────────────────────────────────┐
        //  │ [✓] Giữ đồ Sao   [✓] Giữ đồ Thần   [✓] Giữ đồ SKH       │
        //  │ Level bán tối đa: [N]                                       │
        //  │ ID không bán (whitelist) – mỗi dòng 1 ID:                  │
        //  │ [TextBox]                                                   │
        //  └─────────────────────────────────────────────────────────────┘
        //  ┌─ Bán theo ID tùy chọn (ưu tiên cao) ──────────────────────┐
        //  │ [✓] Ép bán dù đồ VIP (bỏ qua bộ lọc)                      │
        //  │ Mỗi dòng: ID  hoặc  ID|SốLượng                            │
        //  │ VD: 380|99 (có đủ 99 CSKB → bay về bán ngay)             │
        //  │ [TextBox]                                                   │
        //  └─────────────────────────────────────────────────────────────┘

        this.tabItemSell.Controls.Add(this.grpSellOptions);
        this.tabItemSell.Controls.Add(this.grpSellKeepFilter);
        this.tabItemSell.Controls.Add(this.grpSellCustom);
        this.tabItemSell.Text = "Bán rác";
        this.tabItemSell.Padding = new System.Windows.Forms.Padding(3);

        // grpSellOptions – dòng bật/tắt + trigger
        this.grpSellOptions.Controls.Add(this.chkAutoSellTrash);
        this.grpSellOptions.Controls.Add(this.lblSellEmptySlots);
        this.grpSellOptions.Controls.Add(this.nudSellEmptySlots);
        this.grpSellOptions.Controls.Add(this.chkDropInsteadOfSell);
        this.grpSellOptions.Location = new System.Drawing.Point(6, 6);
        this.grpSellOptions.Size = new System.Drawing.Size(400, 85);
        this.grpSellOptions.Text = "Kích hoạt";

        // chkAutoSellTrash
        this.chkAutoSellTrash.AutoSize = true;
        this.chkAutoSellTrash.Location = new System.Drawing.Point(10, 24);
        this.chkAutoSellTrash.Text = "Tự bán đồ rác khi ô trống ≤";

        // lblSellEmptySlots
        this.lblSellEmptySlots.AutoSize = true;
        this.lblSellEmptySlots.Location = new System.Drawing.Point(275, 26);
        this.lblSellEmptySlots.Text = "ô";

        // nudSellEmptySlots
        this.nudSellEmptySlots.Location = new System.Drawing.Point(210, 23);
        this.nudSellEmptySlots.Size = new System.Drawing.Size(60, 23);
        this.nudSellEmptySlots.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
        this.nudSellEmptySlots.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
        this.nudSellEmptySlots.Value = new decimal(new int[] { 0, 0, 0, 0 });

        // chkDropInsteadOfSell
        this.chkDropInsteadOfSell.AutoSize = true;
        this.chkDropInsteadOfSell.Location = new System.Drawing.Point(10, 53);
        this.chkDropInsteadOfSell.Text = "Vứt tại chỗ (không đi đến trạm)";

        // grpSellKeepFilter – bộ lọc đồ GIỮ
        this.grpSellKeepFilter.Controls.Add(this.chkKeepStarItems);
        this.grpSellKeepFilter.Controls.Add(this.chkKeepGodItems);
        this.grpSellKeepFilter.Controls.Add(this.chkKeepSkhItems);
        this.grpSellKeepFilter.Controls.Add(this.lblSellMaxLevel);
        this.grpSellKeepFilter.Controls.Add(this.nudSellMaxLevel);
        this.grpSellKeepFilter.Controls.Add(this.lblSellKeepIds);
        this.grpSellKeepFilter.Controls.Add(this.txtSellKeepIds);
        this.grpSellKeepFilter.Location = new System.Drawing.Point(6, 97);
        this.grpSellKeepFilter.Size = new System.Drawing.Size(400, 170);
        this.grpSellKeepFilter.Text = "Đồ GIỮ (không bán)";

        // chkKeepStarItems
        this.chkKeepStarItems.AutoSize = true;
        this.chkKeepStarItems.Checked = true;
        this.chkKeepStarItems.Location = new System.Drawing.Point(10, 25);
        this.chkKeepStarItems.Text = "Giữ đồ Sao";

        // chkKeepGodItems
        this.chkKeepGodItems.AutoSize = true;
        this.chkKeepGodItems.Checked = true;
        this.chkKeepGodItems.Location = new System.Drawing.Point(135, 25);
        this.chkKeepGodItems.Text = "Giữ đồ Thần/H.diệt";

        // chkKeepSkhItems
        this.chkKeepSkhItems.AutoSize = true;
        this.chkKeepSkhItems.Checked = true;
        this.chkKeepSkhItems.Location = new System.Drawing.Point(280, 25);
        this.chkKeepSkhItems.Text = "Giữ đồ SKH";

        // lblSellMaxLevel
        this.lblSellMaxLevel.AutoSize = true;
        this.lblSellMaxLevel.Location = new System.Drawing.Point(10, 57);
        this.lblSellMaxLevel.Text = "Bán đồ thường có level ≤";

        // nudSellMaxLevel
        this.nudSellMaxLevel.Location = new System.Drawing.Point(195, 55);
        this.nudSellMaxLevel.Size = new System.Drawing.Size(60, 23);
        this.nudSellMaxLevel.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this.nudSellMaxLevel.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
        this.nudSellMaxLevel.Value = new decimal(new int[] { 10, 0, 0, 0 });

        // lblSellKeepIds
        this.lblSellKeepIds.AutoSize = true;
        this.lblSellKeepIds.Location = new System.Drawing.Point(10, 88);
        this.lblSellKeepIds.ForeColor = System.Drawing.Color.DimGray;
        this.lblSellKeepIds.Text = "ID giữ (ID;ID;...):";

        // txtSellKeepIds
        this.txtSellKeepIds.Location = new System.Drawing.Point(10, 110);
        this.txtSellKeepIds.Multiline = true;
        this.txtSellKeepIds.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtSellKeepIds.Size = new System.Drawing.Size(380, 50);

        // grpSellCustom – bán theo ID tùy chọn
        this.grpSellCustom.Controls.Add(this.chkSellCustomNoStarCheck);
        this.grpSellCustom.Controls.Add(this.lblSellCustomDesc);
        this.grpSellCustom.Controls.Add(this.txtSellCustomIdsList);
        this.grpSellCustom.Location = new System.Drawing.Point(6, 273);
        this.grpSellCustom.Size = new System.Drawing.Size(400, 130);
        this.grpSellCustom.Text = "Bán theo ID tùy chọn (ưu tiên cao hơn bộ lọc)";

        // chkSellCustomNoStarCheck
        this.chkSellCustomNoStarCheck.AutoSize = true;
        this.chkSellCustomNoStarCheck.Location = new System.Drawing.Point(10, 24);
        this.chkSellCustomNoStarCheck.Text = "Bỏ qua bộ lọc (ép bán theo ID)";

        // lblSellCustomDesc
        this.lblSellCustomDesc.AutoSize = true;
        this.lblSellCustomDesc.Location = new System.Drawing.Point(10, 50);
        this.lblSellCustomDesc.ForeColor = System.Drawing.Color.DimGray;
        this.lblSellCustomDesc.Text = "Mỗi dòng ID hoặc ID|SL:";

        // txtSellCustomIdsList
        this.txtSellCustomIdsList.Location = new System.Drawing.Point(10, 75);
        this.txtSellCustomIdsList.Multiline = true;
        this.txtSellCustomIdsList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtSellCustomIdsList.Size = new System.Drawing.Size(380, 45);
        // tabItemBuy
        this.tabItemBuy.Controls.Add(this.grpAutoBuy);
        this.tabItemBuy.Text = "Mua";
        this.tabItemBuy.Padding = new System.Windows.Forms.Padding(3);
        this.tabItemBuy.AutoScroll = true;

        // grpAutoBuy
        this.grpAutoBuy.Controls.Add(this.cbAutoBuyPrivateTicket);
        this.grpAutoBuy.Controls.Add(this.cbAutoBuyTdlt);
        this.grpAutoBuy.Controls.Add(this.cbAutoBuyKhauTrang);
        this.grpAutoBuy.Controls.Add(this.numBuyKhauTrangQty);
        this.grpAutoBuy.Controls.Add(this.cbAutoBuyCoBonLa);
        this.grpAutoBuy.Controls.Add(this.numBuyCoBonLaQty);
        this.grpAutoBuy.Controls.Add(this.cbAutoBuyBuaDe);
        this.grpAutoBuy.Controls.Add(this.numBuyBuaDeQty);
        
        this.grpAutoBuy.Controls.Add(this.chkAutoBuyCustom);
        this.grpAutoBuy.Controls.Add(this.btnBuyCustomHelp);
        this.grpAutoBuy.Controls.Add(this.txtBuyCustomList);
        
        this.grpAutoBuy.Location = new System.Drawing.Point(13, 13);
        this.grpAutoBuy.Size = new System.Drawing.Size(400, 380);
        this.grpAutoBuy.Text = "Tự động mua Item";

        // cbAutoBuyTdlt
        this.cbAutoBuyTdlt.AutoSize = true;
        this.cbAutoBuyTdlt.Location = new System.Drawing.Point(15, 25);
        this.cbAutoBuyTdlt.Text = "Tự động mua TDLT khi hết";

        // cbAutoBuyPrivateTicket
        this.cbAutoBuyPrivateTicket.AutoSize = true;
        this.cbAutoBuyPrivateTicket.Location = new System.Drawing.Point(15, 145);
        this.cbAutoBuyPrivateTicket.Text = "Tự mua vé riêng tư khi hết";

        // cbAutoBuyKhauTrang
        this.cbAutoBuyKhauTrang.AutoSize = true;
        this.cbAutoBuyKhauTrang.Location = new System.Drawing.Point(15, 55);
        this.cbAutoBuyKhauTrang.Text = "Tự mua khẩu trang, số lượng:";

        // numBuyKhauTrangQty
        this.numBuyKhauTrangQty.Location = new System.Drawing.Point(240, 53);
        this.numBuyKhauTrangQty.Size = new System.Drawing.Size(60, 23);
        this.numBuyKhauTrangQty.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this.numBuyKhauTrangQty.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
        this.numBuyKhauTrangQty.Value = new decimal(new int[] { 1, 0, 0, 0 });

        // cbAutoBuyCoBonLa
        this.cbAutoBuyCoBonLa.AutoSize = true;
        this.cbAutoBuyCoBonLa.Location = new System.Drawing.Point(15, 85);
        this.cbAutoBuyCoBonLa.Text = "Tự mua cỏ bốn lá, số lượng:";

        // numBuyCoBonLaQty
        this.numBuyCoBonLaQty.Location = new System.Drawing.Point(240, 83);
        this.numBuyCoBonLaQty.Size = new System.Drawing.Size(60, 23);
        this.numBuyCoBonLaQty.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this.numBuyCoBonLaQty.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
        this.numBuyCoBonLaQty.Value = new decimal(new int[] { 1, 0, 0, 0 });

        // cbAutoBuyBuaDe
        this.cbAutoBuyBuaDe.AutoSize = true;
        this.cbAutoBuyBuaDe.Location = new System.Drawing.Point(15, 115);
        this.cbAutoBuyBuaDe.Text = "Tự mua bùa x2 đệ tử, số lượng:";

        // numBuyBuaDeQty
        this.numBuyBuaDeQty.Location = new System.Drawing.Point(240, 113);
        this.numBuyBuaDeQty.Size = new System.Drawing.Size(60, 23);
        this.numBuyBuaDeQty.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this.numBuyBuaDeQty.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
        this.numBuyBuaDeQty.Value = new decimal(new int[] { 1, 0, 0, 0 });

        // chkAutoBuyCustom
        this.chkAutoBuyCustom.AutoSize = true;
        this.chkAutoBuyCustom.Location = new System.Drawing.Point(15, 175);
        this.chkAutoBuyCustom.Text = "Kích hoạt tự mua theo list";

        // btnBuyCustomHelp
        this.btnBuyCustomHelp.Location = new System.Drawing.Point(200, 172);
        this.btnBuyCustomHelp.Name = "btnBuyCustomHelp";
        this.btnBuyCustomHelp.Size = new System.Drawing.Size(185, 23);
        this.btnBuyCustomHelp.Text = "[?] Hướng dẫn Mua Custom";
        this.btnBuyCustomHelp.UseVisualStyleBackColor = true;
        this.btnBuyCustomHelp.Click += new System.EventHandler(this.BtnBuyCustomHelp_Click);

        // txtBuyCustomList
        this.txtBuyCustomList.Location = new System.Drawing.Point(15, 203);
        this.txtBuyCustomList.Multiline = true;
        this.txtBuyCustomList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtBuyCustomList.Size = new System.Drawing.Size(370, 160);

        // tabItemPick
        this.tabItemPick.Controls.Add(this.grpAutoPick);
        this.tabItemPick.Text = "Nhặt";
        this.tabItemPick.Padding = new System.Windows.Forms.Padding(5);
        this.tabItemPick.AutoScroll = true;

        // grpAutoPick
        this.grpAutoPick.Controls.Add(this.chkAutoPick);
        this.grpAutoPick.Controls.Add(this.lblPickMode);
        this.grpAutoPick.Controls.Add(this.cboPickMode);
        this.grpAutoPick.Controls.Add(this.chkOnlyMyItems);
        this.grpAutoPick.Controls.Add(this.lblPickDesc);
        this.grpAutoPick.Controls.Add(this.txtPickIdsList);
        this.grpAutoPick.Controls.Add(this.lblPickBlackList);
        this.grpAutoPick.Controls.Add(this.txtPickBlackList);
        this.grpAutoPick.Location = new System.Drawing.Point(6, 6);
        this.grpAutoPick.Size = new System.Drawing.Size(400, 260);
        this.grpAutoPick.Text = "Tự động Nhặt Item";

        // ── Hàng 1: Bật/Tắt ──────────────────────────────────────────────────
        // chkAutoPick
        this.chkAutoPick.AutoSize = true;
        this.chkAutoPick.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.chkAutoPick.Location = new System.Drawing.Point(12, 22);
        this.chkAutoPick.Text = "Bật tự động nhặt đồ";

        // ── Hàng 2: Chế độ ────────────────────────────────────────────────────
        // lblPickMode
        this.lblPickMode.AutoSize = true;
        this.lblPickMode.Location = new System.Drawing.Point(12, 53);
        this.lblPickMode.Text = "Chế độ:";

        // cboPickMode
        this.cboPickMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboPickMode.FormattingEnabled = true;
        this.cboPickMode.Items.AddRange(new object[] { "Nhặt tất cả", "Theo whitelist" });
        this.cboPickMode.Location = new System.Drawing.Point(72, 50);
        this.cboPickMode.Size = new System.Drawing.Size(140, 23);

        // chkOnlyMyItems
        this.chkOnlyMyItems.AutoSize = true;
        this.chkOnlyMyItems.Location = new System.Drawing.Point(230, 53);
        this.chkOnlyMyItems.Text = "Chỉ nhặt đồ của mình";

        // ── Whitelist ────────────────────────────────────────────────────────
        // lblPickDesc
        this.lblPickDesc.AutoSize = true;
        this.lblPickDesc.Location = new System.Drawing.Point(12, 85);
        this.lblPickDesc.ForeColor = System.Drawing.Color.DimGray;
        this.lblPickDesc.Text = "ID muốn nhặt – Whitelist (ID;ID;...):";

        // txtPickIdsList
        this.txtPickIdsList.Location = new System.Drawing.Point(12, 103);
        this.txtPickIdsList.Multiline = true;
        this.txtPickIdsList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtPickIdsList.Size = new System.Drawing.Size(388, 55);

        // ── Blacklist ─────────────────────────────────────────────────────────
        // lblPickBlackList
        this.lblPickBlackList.AutoSize = true;
        this.lblPickBlackList.Location = new System.Drawing.Point(12, 169);
        this.lblPickBlackList.ForeColor = System.Drawing.Color.Crimson;
        this.lblPickBlackList.Text = "ID KHÔNG nhặt – Blacklist (ID;ID;...):";

        // txtPickBlackList
        this.txtPickBlackList.Location = new System.Drawing.Point(12, 187);
        this.txtPickBlackList.Multiline = true;
        this.txtPickBlackList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtPickBlackList.Size = new System.Drawing.Size(388, 55);

        // 
        // tabItemBongTaiCoDen
        // 
        this.tabItemBongTaiCoDen.Controls.Add(this.grpAutoBongTai);
        this.tabItemBongTaiCoDen.Controls.Add(this.grpAutoCoDen);
        this.tabItemBongTaiCoDen.Location = new System.Drawing.Point(4, 24);
        this.tabItemBongTaiCoDen.Name = "tabItemBongTaiCoDen";
        this.tabItemBongTaiCoDen.Padding = new System.Windows.Forms.Padding(3);
        this.tabItemBongTaiCoDen.Size = new System.Drawing.Size(418, 280);
        this.tabItemBongTaiCoDen.TabIndex = 6;
        this.tabItemBongTaiCoDen.Text = "Bông tai / Cờ đen";
        this.tabItemBongTaiCoDen.UseVisualStyleBackColor = true;
        // 
        // grpAutoBongTai
        // 
        this.grpAutoBongTai.Controls.Add(this.lblBongTaiState);
        this.grpAutoBongTai.Controls.Add(this.cboBongTaiState);
        this.grpAutoBongTai.Controls.Add(this.lblBongTaiPetAction);
        this.grpAutoBongTai.Controls.Add(this.cboBongTaiPetAction);
        this.grpAutoBongTai.Controls.Add(this.lblBongTaiWarning);
        this.grpAutoBongTai.Location = new System.Drawing.Point(6, 6);
        this.grpAutoBongTai.Name = "grpAutoBongTai";
        this.grpAutoBongTai.Size = new System.Drawing.Size(400, 135);
        this.grpAutoBongTai.TabIndex = 0;
        this.grpAutoBongTai.TabStop = false;
        this.grpAutoBongTai.Text = "Auto dùng bông tai";
        // 
        // lblBongTaiState
        // 
        this.lblBongTaiState.AutoSize = true;
        this.lblBongTaiState.Location = new System.Drawing.Point(10, 25);
        this.lblBongTaiState.Name = "lblBongTaiState";
        this.lblBongTaiState.Size = new System.Drawing.Size(102, 15);
        this.lblBongTaiState.TabIndex = 0;
        this.lblBongTaiState.Text = "Luôn ở trạng thái:";
        // cboBongTaiState
        // 
        this.cboBongTaiState.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboBongTaiState.FormattingEnabled = true;
        this.cboBongTaiState.Items.AddRange(new object[] {
            "Không chạy",
            "Hợp thể bông tai",
            "Tách hợp thể"});
        this.cboBongTaiState.Location = new System.Drawing.Point(150, 22);
        this.cboBongTaiState.Name = "cboBongTaiState";
        this.cboBongTaiState.Size = new System.Drawing.Size(150, 23);
        this.cboBongTaiState.TabIndex = 1;
        this.cboBongTaiState.Text = "Không chạy";
        // 
        // lblBongTaiPetAction
        // 
        this.lblBongTaiPetAction.AutoSize = true;
        this.lblBongTaiPetAction.Location = new System.Drawing.Point(10, 55);
        this.lblBongTaiPetAction.Name = "lblBongTaiPetAction";
        this.lblBongTaiPetAction.Size = new System.Drawing.Size(176, 15);
        this.lblBongTaiPetAction.TabIndex = 6;
        this.lblBongTaiPetAction.Text = "Sau khi tách hợp thể:";
        // 
        // cboBongTaiPetAction
        // 
        this.cboBongTaiPetAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboBongTaiPetAction.FormattingEnabled = true;
        this.cboBongTaiPetAction.Items.AddRange(new object[] {
            "Cho đệ tử đi theo",
            "Cho đệ tử bảo vệ",
            "Cho đệ tử tấn công",
            "Cho đệ tử về nhà"});
        this.cboBongTaiPetAction.Location = new System.Drawing.Point(150, 52);
        this.cboBongTaiPetAction.Name = "cboBongTaiPetAction";
        this.cboBongTaiPetAction.Size = new System.Drawing.Size(150, 23);
        this.cboBongTaiPetAction.TabIndex = 7;
        this.cboBongTaiPetAction.Text = "Cho đệ tử về nhà";
        // 
        // lblBongTaiWarning
        // 
        this.lblBongTaiWarning.AutoSize = false;
        this.lblBongTaiWarning.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(244)))), ((int)(((byte)(229)))));
        this.lblBongTaiWarning.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.lblBongTaiWarning.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(36)))), ((int)(((byte)(36)))));
        this.lblBongTaiWarning.Location = new System.Drawing.Point(10, 85);
        this.lblBongTaiWarning.Name = "lblBongTaiWarning";
        this.lblBongTaiWarning.Padding = new System.Windows.Forms.Padding(5);
        this.lblBongTaiWarning.Size = new System.Drawing.Size(380, 40);
        this.lblBongTaiWarning.TabIndex = 8;
        this.lblBongTaiWarning.Text = "Chức năng [Sau khi tách hợp thể] sẽ chỉ chạy khi tách hợp thể bằng auto";
        // 
        // grpAutoCoDen
        // 
        this.grpAutoCoDen.Controls.Add(this.chkAutoCoDen);
        this.grpAutoCoDen.Controls.Add(this.cboFlagType);
        this.grpAutoCoDen.Controls.Add(this.chkDisableCoDenIfOthers);
        this.grpAutoCoDen.Location = new System.Drawing.Point(6, 150);
        this.grpAutoCoDen.Name = "grpAutoCoDen";
        this.grpAutoCoDen.Size = new System.Drawing.Size(400, 75);
        this.grpAutoCoDen.TabIndex = 1;
        this.grpAutoCoDen.TabStop = false;
        this.grpAutoCoDen.Text = "Auto đổi cờ";
        // 
        // chkAutoCoDen
        // 
        this.chkAutoCoDen.AutoSize = true;
        this.chkAutoCoDen.Location = new System.Drawing.Point(13, 20);
        this.chkAutoCoDen.Name = "chkAutoCoDen";
        this.chkAutoCoDen.Size = new System.Drawing.Size(89, 19);
        this.chkAutoCoDen.TabIndex = 0;
        this.chkAutoCoDen.Text = "Auto bật cờ";
        this.chkAutoCoDen.UseVisualStyleBackColor = true;
        //
        // cboFlagType
        //
        this.cboFlagType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboFlagType.FormattingEnabled = true;
        this.cboFlagType.Location = new System.Drawing.Point(110, 18);
        this.cboFlagType.Name = "cboFlagType";
        this.cboFlagType.Size = new System.Drawing.Size(120, 23);
        this.cboFlagType.TabIndex = 2;
        this.cboFlagType.Items.AddRange(new object[] {
            "0: Tháo cờ",
            "1: Cờ xanh",
            "2: Cờ đỏ",
            "3: Cờ tím",
            "4: Cờ vàng",
            "5: Cờ lục",
            "6: Cờ hồng",
            "7: Cờ cam",
            "8: Cờ xám"});
        // 
        // chkDisableCoDenIfOthers
        // 
        this.chkDisableCoDenIfOthers.AutoSize = true;
        this.chkDisableCoDenIfOthers.Location = new System.Drawing.Point(13, 45);
        this.chkDisableCoDenIfOthers.Name = "chkDisableCoDenIfOthers";
        this.chkDisableCoDenIfOthers.Size = new System.Drawing.Size(248, 19);
        this.chkDisableCoDenIfOthers.TabIndex = 1;
        this.chkDisableCoDenIfOthers.Text = "Tắt cờ khi có người khác trong map bật cờ";
        this.chkDisableCoDenIfOthers.UseVisualStyleBackColor = true;




















        // grpAutoDrop
        this.grpAutoDrop.Controls.Add(this.chkAutoDrop);
        this.grpAutoDrop.Controls.Add(this.lblDropDesc1);
        this.grpAutoDrop.Controls.Add(this.chkDropByHsd);
        this.grpAutoDrop.Controls.Add(this.lblDropDesc2);
        this.grpAutoDrop.Controls.Add(this.txtDropIds);
        this.grpAutoDrop.Location = new System.Drawing.Point(6, 6);
        this.grpAutoDrop.Size = new System.Drawing.Size(262, 210);
        this.grpAutoDrop.Text = "Tự vứt đồ rác";

        // chkAutoDrop
        this.chkAutoDrop.AutoSize = true;
        this.chkAutoDrop.Location = new System.Drawing.Point(6, 22);
        this.chkAutoDrop.Text = "Tự vứt đồ rác";

        // lblDropDesc1
        this.lblDropDesc1.AutoSize = true;
        this.lblDropDesc1.Location = new System.Drawing.Point(23, 44);
        this.lblDropDesc1.Text = "Vứt theo ID, mỗi dòng 1 ID";

        // chkDropByHsd
        this.chkDropByHsd.AutoSize = true;
        this.chkDropByHsd.Location = new System.Drawing.Point(6, 62);
        this.chkDropByHsd.Text = "Vứt HSD thì mỗi dòng => ID|HSD";

        // lblDropDesc2
        this.lblDropDesc2.AutoSize = true;
        this.lblDropDesc2.Location = new System.Drawing.Point(23, 84);
        this.lblDropDesc2.Text = "- HSD (nếu có) dưới số nhập sẽ vứt";

        // txtDropIds
        this.txtDropIds.Location = new System.Drawing.Point(6, 107);
        this.txtDropIds.Multiline = true;
        this.txtDropIds.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtDropIds.Size = new System.Drawing.Size(250, 95);

        // tabGeneral
        this.tabGeneral.Controls.Add(this.tabControlGeneral);
        this.tabGeneral.Text = "Cài đặt chung";
        this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);

        // tabControlGeneral
        this.tabControlGeneral.Controls.Add(this.tabBasic);
        this.tabControlGeneral.Controls.Add(this.tabConnection);
        this.tabControlGeneral.Dock = System.Windows.Forms.DockStyle.Fill;

        // tabBasic
        this.tabBasic.Controls.Add(this.chkEatChicken);
        this.tabBasic.Controls.Add(this.chkUseTdltXmap);
        this.tabBasic.Controls.Add(this.lblActionOnDeath);
        this.tabBasic.Controls.Add(this.cboActionOnDeath);
        this.tabBasic.Text = "Cơ bản";
        this.tabBasic.Padding = new System.Windows.Forms.Padding(10);

        // chkEatChicken
        this.chkEatChicken.AutoSize = true;
        this.chkEatChicken.Location = new System.Drawing.Point(13, 13);
        this.chkEatChicken.Text = "Ăn đùi gà";

        // chkUseTdltXmap
        this.chkUseTdltXmap.AutoSize = true;
        this.chkUseTdltXmap.Location = new System.Drawing.Point(13, 75);
        this.chkUseTdltXmap.Text = "Dùng TDLT khi Xmap";

        // lblActionOnDeath
        this.lblActionOnDeath.AutoSize = true;
        this.lblActionOnDeath.Location = new System.Drawing.Point(13, 45);
        this.lblActionOnDeath.Text = "Khi chết:";
        this.lblActionOnDeath.Name = "lblActionOnDeath";

        // cboActionOnDeath
        this.cboActionOnDeath.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboActionOnDeath.FormattingEnabled = true;
        this.cboActionOnDeath.Items.AddRange(new object[] {
            "Về nhà",
            "Hồi sinh Ngọc",
            "Chờ (Đứng yên)"
        });
        this.cboActionOnDeath.Location = new System.Drawing.Point(70, 42);
        this.cboActionOnDeath.Size = new System.Drawing.Size(230, 23);
        this.cboActionOnDeath.Name = "cboActionOnDeath";



        // ── tabConfigManagement (Quản lý Config) ─────────────────────────────
        this.tabConfigManagement.Controls.Add(this.btnCopyConfig);
        this.tabConfigManagement.Controls.Add(this.btnPasteConfigCurrent);
        this.tabConfigManagement.Controls.Add(this.btnPasteConfigChecked);
        this.tabConfigManagement.Controls.Add(this.btnPasteConfigAll);
        this.tabConfigManagement.Controls.Add(this.btnPasteConfigByType);
        this.tabConfigManagement.Controls.Add(this.nudPasteConfigTypeAccount);
        this.tabConfigManagement.Controls.Add(this.lblTypeAccountFilter);
        this.tabConfigManagement.Controls.Add(this.nudFilterTypeAccount);
        this.tabConfigManagement.Controls.Add(this.lblConfigClipboard);
        this.tabConfigManagement.Text = "Quản lý Config";
        this.tabConfigManagement.Padding = new System.Windows.Forms.Padding(8);

        int cmBtnW = 310;
        int cmBtnH = 27;
        int cmX = 8;

        // btnCopyConfig – Row 1
        this.btnCopyConfig.Location = new System.Drawing.Point(cmX, 10);
        this.btnCopyConfig.Size = new System.Drawing.Size(cmBtnW, cmBtnH);
        this.btnCopyConfig.Text = "Copy config tài khoản đang chọn";
        this.btnCopyConfig.Name = "btnCopyConfig";
        this.btnCopyConfig.Click += new System.EventHandler(this.BtnCopyConfig_Click);

        // btnPasteConfigCurrent – Row 2
        this.btnPasteConfigCurrent.Location = new System.Drawing.Point(cmX, 43);
        this.btnPasteConfigCurrent.Size = new System.Drawing.Size(cmBtnW, cmBtnH);
        this.btnPasteConfigCurrent.Text = "Dán config đang copy cho tài khoản này";
        this.btnPasteConfigCurrent.Name = "btnPasteConfigCurrent";
        this.btnPasteConfigCurrent.Click += new System.EventHandler(this.BtnPasteConfigCurrent_Click);

        // btnPasteConfigChecked – Row 3
        this.btnPasteConfigChecked.Location = new System.Drawing.Point(cmX, 76);
        this.btnPasteConfigChecked.Size = new System.Drawing.Size(cmBtnW, cmBtnH);
        this.btnPasteConfigChecked.Text = "Dán config đang copy (tất cả tài khoản tích chọn)";
        this.btnPasteConfigChecked.Name = "btnPasteConfigChecked";
        this.btnPasteConfigChecked.Click += new System.EventHandler(this.BtnPasteConfigChecked_Click);

        // btnPasteConfigAll – Row 4
        this.btnPasteConfigAll.Location = new System.Drawing.Point(cmX, 109);
        this.btnPasteConfigAll.Size = new System.Drawing.Size(cmBtnW, cmBtnH);
        this.btnPasteConfigAll.Text = "Dán config đang copy (tất cả tài khoản)";
        this.btnPasteConfigAll.Name = "btnPasteConfigAll";
        this.btnPasteConfigAll.Click += new System.EventHandler(this.BtnPasteConfigAll_Click);

        // btnPasteConfigByType + NUD – Row 5
        this.btnPasteConfigByType.Location = new System.Drawing.Point(cmX, 142);
        this.btnPasteConfigByType.Size = new System.Drawing.Size(195, cmBtnH);
        this.btnPasteConfigByType.Text = "Dán config cho loại TK:";
        this.btnPasteConfigByType.Name = "btnPasteConfigByType";
        this.btnPasteConfigByType.Click += new System.EventHandler(this.BtnPasteConfigByType_Click);

        this.nudPasteConfigTypeAccount.Location = new System.Drawing.Point(210, 144);
        this.nudPasteConfigTypeAccount.Size = new System.Drawing.Size(50, 23);
        this.nudPasteConfigTypeAccount.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
        this.nudPasteConfigTypeAccount.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
        this.nudPasteConfigTypeAccount.Value = new decimal(new int[] { 0, 0, 0, 0 });
        this.nudPasteConfigTypeAccount.Name = "nudPasteConfigTypeAccount";

        // lblTypeAccountFilter + NUD – Row 6 (TypeAccount của tài khoản hiện tại)
        this.lblTypeAccountFilter.AutoSize = true;
        this.lblTypeAccountFilter.Location = new System.Drawing.Point(cmX + 120, 178);
        this.lblTypeAccountFilter.Text = "TypeAccount (TK này):";
        this.lblTypeAccountFilter.Name = "lblTypeAccountFilter";

        this.nudFilterTypeAccount.Location = new System.Drawing.Point(cmX + 275, 176);
        this.nudFilterTypeAccount.Size = new System.Drawing.Size(50, 23);
        this.nudFilterTypeAccount.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
        this.nudFilterTypeAccount.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
        this.nudFilterTypeAccount.Value = new decimal(new int[] { 0, 0, 0, 0 });
        this.nudFilterTypeAccount.Name = "nudFilterTypeAccount";

        // lblConfigClipboard – Row 7 (info config đang copy)
        this.lblConfigClipboard.AutoSize = false;
        this.lblConfigClipboard.Location = new System.Drawing.Point(cmX, 208);
        this.lblConfigClipboard.Size = new System.Drawing.Size(cmBtnW, 40);
        this.lblConfigClipboard.Text = "- Config đang copy là: (Chưa copy)";
        this.lblConfigClipboard.Name = "lblConfigClipboard";
        this.lblConfigClipboard.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);

        // tabConnection
        this.tabConnection.Controls.Add(this.chkUseProxy);
        this.tabConnection.Controls.Add(this.lblProxyType);
        this.tabConnection.Controls.Add(this.cboProxyType);
        this.tabConnection.Controls.Add(this.lblProxyAddress);
        this.tabConnection.Controls.Add(this.txtProxyAddress);
        this.tabConnection.Controls.Add(this.btnTestProxy);
        this.tabConnection.Controls.Add(this.lblProxyStatus);
        this.tabConnection.Text = "Đăng nhập";
        this.tabConnection.Padding = new System.Windows.Forms.Padding(10);

        // chkUseProxy
        this.chkUseProxy.AutoSize = true;
        this.chkUseProxy.Location = new System.Drawing.Point(13, 8);
        this.chkUseProxy.Name = "chkUseProxy";
        this.chkUseProxy.Size = new System.Drawing.Size(167, 19);
        this.chkUseProxy.TabIndex = 0;
        this.chkUseProxy.Text = "Bật proxy";

        // lblProxyType
        this.lblProxyType.AutoSize = true;
        this.lblProxyType.Location = new System.Drawing.Point(13, 35);
        this.lblProxyType.Text = "Loại proxy:";

        // cboProxyType
        this.cboProxyType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboProxyType.FormattingEnabled = true;
        this.cboProxyType.Items.AddRange(new object[] { "HTTP", "SOCKS5" });
        this.cboProxyType.Location = new System.Drawing.Point(90, 31);
        this.cboProxyType.Size = new System.Drawing.Size(100, 23);
        this.cboProxyType.Name = "cboProxyType";

        // lblProxyAddress
        this.lblProxyAddress.AutoSize = true;
        this.lblProxyAddress.Location = new System.Drawing.Point(13, 62);
        this.lblProxyAddress.Text = "Proxy (host:port):";

        // txtProxyAddress
        this.txtProxyAddress.Location = new System.Drawing.Point(120, 59);
        this.txtProxyAddress.Size = new System.Drawing.Size(200, 23);
        this.txtProxyAddress.Name = "txtProxyAddress";
        this.txtProxyAddress.PlaceholderText = "127.0.0.1:8080";

        // btnTestProxy
        this.btnTestProxy.Location = new System.Drawing.Point(330, 58);
        this.btnTestProxy.Size = new System.Drawing.Size(90, 26);
        this.btnTestProxy.Text = "Test Proxy";
        this.btnTestProxy.Name = "btnTestProxy";
        this.btnTestProxy.Click += new System.EventHandler(this.BtnTestProxy_Click);

        // lblProxyStatus
        this.lblProxyStatus.AutoSize = true;
        this.lblProxyStatus.Location = new System.Drawing.Point(13, 88);
        this.lblProxyStatus.Text = "";
        this.lblProxyStatus.Name = "lblProxyStatus";
        this.lblProxyStatus.ForeColor = System.Drawing.Color.Gray;

        // grpAccountInfo
        this.grpAccountInfo.Controls.Add(this.tabControlAccountInfo);
        this.grpAccountInfo.Dock = System.Windows.Forms.DockStyle.Right;
        this.grpAccountInfo.Width = 280;
        this.grpAccountInfo.Text = "Thông tin tài khoản";

        // tabControlAccountInfo
        this.tabControlAccountInfo.Controls.Add(this.tabAccountSuPhu);
        this.tabControlAccountInfo.Controls.Add(this.tabAccountDeTu);
        this.tabControlAccountInfo.Controls.Add(this.tabAccountHanhTrang);
        this.tabControlAccountInfo.Controls.Add(this.tabAccountNhatKi);
        this.tabControlAccountInfo.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tabControlAccountInfo.Name = "tabControlAccountInfo";

        // tabAccountSuPhu
        this.tabAccountSuPhu.Controls.Add(this.txtSuPhuInfo);
        this.tabAccountSuPhu.Text = "Sư phụ";
        this.tabAccountSuPhu.Name = "tabAccountSuPhu";

        // txtSuPhuInfo
        this.txtSuPhuInfo.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtSuPhuInfo.ReadOnly = true;
        this.txtSuPhuInfo.BackColor = System.Drawing.Color.FromArgb(245, 245, 248);
        this.txtSuPhuInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
        this.txtSuPhuInfo.ForeColor = System.Drawing.Color.FromArgb(30, 30, 40);
        this.txtSuPhuInfo.Name = "txtSuPhuInfo";
        this.txtSuPhuInfo.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
        this.txtSuPhuInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;

        // tabAccountDeTu
        this.tabAccountDeTu.Controls.Add(this.txtDeTuInfo);
        this.tabAccountDeTu.Text = "Đệ tử";
        this.tabAccountDeTu.Name = "tabAccountDeTu";

        // txtDeTuInfo
        this.txtDeTuInfo.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtDeTuInfo.ReadOnly = true;
        this.txtDeTuInfo.BackColor = System.Drawing.Color.FromArgb(245, 245, 248);
        this.txtDeTuInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
        this.txtDeTuInfo.ForeColor = System.Drawing.Color.FromArgb(30, 30, 40);
        this.txtDeTuInfo.Name = "txtDeTuInfo";
        this.txtDeTuInfo.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
        this.txtDeTuInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;

        // tabAccountHanhTrang
        this.tabAccountHanhTrang.Controls.Add(this.grpHanhTrangSummary);
        this.tabAccountHanhTrang.Controls.Add(this.panelHanhTrangTop);
        this.tabAccountHanhTrang.Text = "Hành trang";
        this.tabAccountHanhTrang.Name = "tabAccountHanhTrang";

        // panelHanhTrangTop
        this.panelHanhTrangTop.Dock = System.Windows.Forms.DockStyle.Top;
        this.panelHanhTrangTop.Height = 40;
        this.panelHanhTrangTop.Controls.Add(this.btnXemHanhTrang);
        this.panelHanhTrangTop.Controls.Add(this.btnXemRuongDo);

        // btnXemHanhTrang
        this.btnXemHanhTrang.Location = new System.Drawing.Point(17, 7);
        this.btnXemHanhTrang.Size = new System.Drawing.Size(120, 26);
        this.btnXemHanhTrang.Text = "Hành trang";
        this.btnXemHanhTrang.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
        this.btnXemHanhTrang.FlatStyle = System.Windows.Forms.FlatStyle.System;

        // btnXemRuongDo
        this.btnXemRuongDo.Location = new System.Drawing.Point(142, 7);
        this.btnXemRuongDo.Size = new System.Drawing.Size(120, 26);
        this.btnXemRuongDo.Text = "Rương đồ";
        this.btnXemRuongDo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
        this.btnXemRuongDo.FlatStyle = System.Windows.Forms.FlatStyle.System;

        // grpHanhTrangSummary
        this.grpHanhTrangSummary.Dock = System.Windows.Forms.DockStyle.Fill;
        this.grpHanhTrangSummary.Text = "Tóm tắt tài sản";
        this.grpHanhTrangSummary.Controls.Add(this.lblHanhTrangCoin);
        this.grpHanhTrangSummary.Controls.Add(this.lblHanhTrangGem);
        this.grpHanhTrangSummary.Controls.Add(this.lblHanhTrangRuby);
        this.grpHanhTrangSummary.Controls.Add(this.lblHanhTrangSlots);
        this.grpHanhTrangSummary.Controls.Add(this.lblRuongDoSlots);
        this.grpHanhTrangSummary.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
        this.grpHanhTrangSummary.Padding = new System.Windows.Forms.Padding(10);

        // lblHanhTrangCoin
        this.lblHanhTrangCoin.AutoSize = true;
        this.lblHanhTrangCoin.Location = new System.Drawing.Point(15, 25);
        this.lblHanhTrangCoin.Text = "- Vàng: ---";
        this.lblHanhTrangCoin.ForeColor = System.Drawing.Color.FromArgb(20, 20, 20);

        // lblHanhTrangGem
        this.lblHanhTrangGem.AutoSize = true;
        this.lblHanhTrangGem.Location = new System.Drawing.Point(15, 50);
        this.lblHanhTrangGem.Text = "- Ngọc xanh: ---";
        this.lblHanhTrangGem.ForeColor = System.Drawing.Color.FromArgb(20, 20, 20);

        // lblHanhTrangRuby
        this.lblHanhTrangRuby.AutoSize = true;
        this.lblHanhTrangRuby.Location = new System.Drawing.Point(15, 75);
        this.lblHanhTrangRuby.Text = "- Hồng ngọc: ---";
        this.lblHanhTrangRuby.ForeColor = System.Drawing.Color.FromArgb(20, 20, 20);

        // lblHanhTrangSlots
        this.lblHanhTrangSlots.AutoSize = true;
        this.lblHanhTrangSlots.Location = new System.Drawing.Point(15, 105);
        this.lblHanhTrangSlots.Text = "- Ô trống hành trang: ---/---";
        this.lblHanhTrangSlots.ForeColor = System.Drawing.Color.FromArgb(20, 20, 20);

        // lblRuongDoSlots
        this.lblRuongDoSlots.AutoSize = true;
        this.lblRuongDoSlots.Location = new System.Drawing.Point(15, 128);
        this.lblRuongDoSlots.Text = "- Ô trống rương: ---/---";
        this.lblRuongDoSlots.ForeColor = System.Drawing.Color.FromArgb(20, 20, 20);

        // tabAccountNhatKi
        this.tabAccountNhatKi.Controls.Add(this.txtNhatKiInfo);
        this.tabAccountNhatKi.Text = "Nhật kí";
        this.tabAccountNhatKi.Name = "tabAccountNhatKi";
        this.tabAccountNhatKi.Padding = new System.Windows.Forms.Padding(3);

        // txtNhatKiInfo
        this.txtNhatKiInfo.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtNhatKiInfo.ReadOnly = true;
        this.txtNhatKiInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(30)))));
        this.txtNhatKiInfo.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular);
        this.txtNhatKiInfo.ForeColor = System.Drawing.Color.MediumSpringGreen;
        this.txtNhatKiInfo.Name = "txtNhatKiInfo";
        this.txtNhatKiInfo.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
        this.txtNhatKiInfo.WordWrap = true;
        this.txtNhatKiInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

        // Form1
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(790, 550);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.Controls.Add(this.panelMiddle);
        this.Controls.Add(this.panelBottom);
        this.Controls.Add(this.panelAccountActions);
        this.Controls.Add(this.panelTop);
        this.Text = "Tool by Zfox";

        this.tabControlAccountInfo.ResumeLayout(false);
        this.tabAccountSuPhu.ResumeLayout(false);
        this.tabAccountDeTu.ResumeLayout(false);
        this.tabAccountHanhTrang.ResumeLayout(false);
        this.panelHanhTrangTop.ResumeLayout(false);
        this.grpHanhTrangSummary.ResumeLayout(false);
        this.grpHanhTrangSummary.PerformLayout();
        this.tabAccountNhatKi.ResumeLayout(false);
        this.panelTop.ResumeLayout(false);
        this.panelTop.PerformLayout();
        this.panelMiddle.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvAccounts)).EndInit();
        // numAutoLoginThread initialization does not need EndInit for TextBox
        ((System.ComponentModel.ISupportInitialize)(this.nudStoreStarCount)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.nudSellEmptySlots)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.nudSellMaxLevel)).EndInit();
        this.grpStoreFilter.ResumeLayout(false);
        this.grpStoreFilter.PerformLayout();
        this.panelBottom.ResumeLayout(false);
        this.tabControlFeatures.ResumeLayout(false);
        this.tabTrain.ResumeLayout(false);
        this.tabControlTrain.ResumeLayout(false);
        this.tabItemManagement.ResumeLayout(false);
        this.tabControlItem.ResumeLayout(false);
        this.tabItemBuy.ResumeLayout(false);
        this.grpAutoBuy.ResumeLayout(false);
        this.grpAutoBuy.PerformLayout();
        this.tabItemUse.ResumeLayout(false);
        this.tabItemUse.PerformLayout();
        this.tabItemSell.ResumeLayout(false);
        this.tabItemSell.PerformLayout();
        this.grpSellOptions.ResumeLayout(false);
        this.grpSellOptions.PerformLayout();
        this.grpSellKeepFilter.ResumeLayout(false);
        this.grpSellKeepFilter.PerformLayout();
        this.grpSellCustom.ResumeLayout(false);
        this.grpSellCustom.PerformLayout();
        this.tabItemBongTaiCoDen.ResumeLayout(false);
        this.grpAutoBongTai.ResumeLayout(false);
        this.grpAutoBongTai.PerformLayout();
        this.grpAutoCoDen.ResumeLayout(false);
        this.grpAutoCoDen.PerformLayout();
        this.tabGeneral.ResumeLayout(false);
        this.tabControlGeneral.ResumeLayout(false);
        this.tabBasic.ResumeLayout(false);
        this.tabBasic.PerformLayout();
        this.tabConnection.ResumeLayout(false);
        this.tabConnection.PerformLayout();
        this.tabConfigManagement.ResumeLayout(false);
        this.tabConfigManagement.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudPasteConfigTypeAccount)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.nudFilterTypeAccount)).EndInit();
        this.grpAccountInfo.ResumeLayout(false);
        this.panelAccountActions.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Panel panelTop;
    private System.Windows.Forms.Button btnToggleGame;
    private System.Windows.Forms.Button btnAdd;
    private System.Windows.Forms.Button btnEdit;
    private System.Windows.Forms.Button btnDelete;
    private System.Windows.Forms.Button btnSelectAll;
    private System.Windows.Forms.Button btnSettings;
    private System.Windows.Forms.Button btnArrangeWindows;
    private System.Windows.Forms.Button btnCloseAll;
    private System.Windows.Forms.Button btnHideAll;
    private System.Windows.Forms.CheckBox chkHideAccount;
    private System.Windows.Forms.CheckBox chkAutoLogin;
    private System.Windows.Forms.CheckBox chkAutoHideClient;
    private System.Windows.Forms.TextBox numAutoLoginThread;
    private System.Windows.Forms.Button btnDeleteSelected;
    private System.Windows.Forms.Label lblSocketStatus;
    
    // System Info Controls
    private System.Windows.Forms.GroupBox grpSystemInfo;
    private System.Windows.Forms.Label lblAccountStats;
    private System.Windows.Forms.Label lblSystemStats;
    private System.Windows.Forms.Button btnMoveUp;
    private System.Windows.Forms.Button btnMoveDown;
    private System.Windows.Forms.Button btnCleanRam;

    private System.Windows.Forms.Panel panelMiddle;
    private System.Windows.Forms.DataGridView dgvAccounts;
    private System.Windows.Forms.Panel panelAccountActions;
    private System.Windows.Forms.Button btnBatAuto;
    private System.Windows.Forms.Button btnDungAuto;
    private System.Windows.Forms.Button btnBatAutoTatCaChon;
    private System.Windows.Forms.Button btnDungAutoTatCaChon;
    private System.Windows.Forms.Button btnGDVP;
    private System.Windows.Forms.Button btnAutoBoMong;
    private System.Windows.Forms.Panel panelBottom;
    private System.Windows.Forms.TabControl tabControlFeatures;
    private System.Windows.Forms.TabPage tabTrain;
    private System.Windows.Forms.TabControl tabControlTrain;
    private System.Windows.Forms.TabPage tabTrainBasic;
    private System.Windows.Forms.TabPage tabTrainMvbt;
    private System.Windows.Forms.TabPage tabTrainMhbt;
    private System.Windows.Forms.TabPage tabTrainUpKilis;
    private System.Windows.Forms.TabPage tabTrainBossVegetaCity;
    private System.Windows.Forms.CheckBox chkBossVegetaCityEnable;
    private System.Windows.Forms.CheckBox chkBossVegetaCityAuto3h;
    private System.Windows.Forms.CheckBox chkBossVegetaCityAuto2230;
    private System.Windows.Forms.CheckBox chkBossVegetaCityReviveByGem;
    private System.Windows.Forms.CheckBox chkBossVegetaCityUseTdlt;
    private System.Windows.Forms.TabPage tabTrainAdvanced;
    private System.Windows.Forms.TabPage tabUpSkh;
    private System.Windows.Forms.TabPage tabPet;
    private System.Windows.Forms.TabPage tabDauThan;
    private DauThanControl dauThanControl;
    private System.Windows.Forms.TabPage tabBuffNamek;
    private BuffNamekControl buffNamekControl;
    private System.Windows.Forms.TabPage tabItemManagement;
    private System.Windows.Forms.TabControl tabControlItem;
    private System.Windows.Forms.TabPage tabItemDrop;
    private System.Windows.Forms.TabPage tabItemStore;
    private System.Windows.Forms.TabPage tabItemSell;
    private System.Windows.Forms.TabPage tabItemUse;
    private System.Windows.Forms.TabPage tabItemBuy;
    private System.Windows.Forms.TabPage tabItemPick;
    private System.Windows.Forms.CheckBox chkAutoStoreWhenFull;
    private System.Windows.Forms.GroupBox grpStoreFilter;
    private System.Windows.Forms.CheckBox chkStoreKichHoat;
    private System.Windows.Forms.CheckBox chkStoreThanLinh;
    private System.Windows.Forms.CheckBox chkStorePhaLe;
    private System.Windows.Forms.Label lblStarCount;
    private System.Windows.Forms.NumericUpDown nudStoreStarCount;
    private System.Windows.Forms.CheckBox chkTrainEnable;
    private System.Windows.Forms.Label lblTrainMapId;
    private System.Windows.Forms.ComboBox cboTrainMapId;
    private System.Windows.Forms.CheckBox chkTrainZoneRequire;
    private System.Windows.Forms.TextBox txtTrainZone;
    private System.Windows.Forms.CheckBox chkUseTDLT;
    private System.Windows.Forms.CheckBox chkCheckLagMob;
    private System.Windows.Forms.CheckBox chkOnlyUsePunch;
    private System.Windows.Forms.CheckBox chkFreezePunchSkillCd;
    private System.Windows.Forms.CheckBox chkUseKaiokenLienHoan;
    private System.Windows.Forms.CheckBox chkAvoidSuperMob;
    private System.Windows.Forms.CheckBox chkChangeLowPlayerZoneIfNoMob;
    private System.Windows.Forms.Label lblMobTargetType;
    private System.Windows.Forms.ComboBox cboMobTargetType;
    private System.Windows.Forms.Label lblMobIds;
    private System.Windows.Forms.TextBox txtMobIds;
    private System.Windows.Forms.Label lblTrainingArmorMode;
    private System.Windows.Forms.ComboBox cboTrainingArmorMode;
    private System.Windows.Forms.TabPage tabGeneral;
    private System.Windows.Forms.TabControl tabControlGeneral;
    private System.Windows.Forms.TabPage tabBasic;
    private System.Windows.Forms.CheckBox chkEatChicken;
    private System.Windows.Forms.CheckBox chkUseTdltXmap;
    private System.Windows.Forms.Label lblActionOnDeath;
    private System.Windows.Forms.ComboBox cboActionOnDeath;
    // Tab Kết nối
    private System.Windows.Forms.TabPage tabConnection;
    private System.Windows.Forms.CheckBox chkUseProxy;
    private System.Windows.Forms.Label lblProxyType;
    private System.Windows.Forms.ComboBox cboProxyType;
    private System.Windows.Forms.Label lblProxyAddress;
    private System.Windows.Forms.TextBox txtProxyAddress;
    private System.Windows.Forms.Button btnTestProxy;
    private System.Windows.Forms.Label lblProxyStatus;

    // Tab Quản lý Config
    private System.Windows.Forms.TabPage tabConfigManagement;
    private System.Windows.Forms.Button btnCopyConfig;
    private System.Windows.Forms.Button btnPasteConfigCurrent;
    private System.Windows.Forms.Button btnPasteConfigChecked;
    private System.Windows.Forms.Button btnPasteConfigAll;
    private System.Windows.Forms.Button btnPasteConfigByType;
    private System.Windows.Forms.NumericUpDown nudPasteConfigTypeAccount;
    private System.Windows.Forms.NumericUpDown nudFilterTypeAccount;
    private System.Windows.Forms.Label lblConfigClipboard;
    private System.Windows.Forms.Label lblTypeAccountFilter;
    private System.Windows.Forms.GroupBox grpAccountInfo;
    private System.Windows.Forms.TabControl tabControlAccountInfo;
    private System.Windows.Forms.TabPage tabAccountSuPhu;
    private System.Windows.Forms.TabPage tabAccountDeTu;
    private System.Windows.Forms.TabPage tabAccountHanhTrang;
    private System.Windows.Forms.TabPage tabAccountNhatKi;
    private System.Windows.Forms.RichTextBox txtSuPhuInfo;
    private System.Windows.Forms.RichTextBox txtDeTuInfo;
    private System.Windows.Forms.RichTextBox txtNhatKiInfo;
    
    // Tab Hanh Trang Fields
    private System.Windows.Forms.Panel panelHanhTrangTop;
    private System.Windows.Forms.Button btnXemHanhTrang;
    private System.Windows.Forms.Button btnXemRuongDo;
    private System.Windows.Forms.GroupBox grpHanhTrangSummary;
    private System.Windows.Forms.Label lblHanhTrangCoin;
    private System.Windows.Forms.Label lblHanhTrangGem;
    private System.Windows.Forms.Label lblHanhTrangRuby;
    private System.Windows.Forms.Label lblHanhTrangSlots;
    private System.Windows.Forms.Label lblRuongDoSlots;

    private System.Windows.Forms.DataGridViewCheckBoxColumn colSelect;
    private System.Windows.Forms.DataGridViewTextBoxColumn colSTT;
    private System.Windows.Forms.DataGridViewTextBoxColumn colCharacter;
    private System.Windows.Forms.DataGridViewTextBoxColumn colDataInGame;
    private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
    private System.Windows.Forms.DataGridViewTextBoxColumn colServer;
    private System.Windows.Forms.DataGridViewTextBoxColumn colTypeAccount;
    private System.Windows.Forms.DataGridViewTextBoxColumn colAccount;
    private System.Windows.Forms.Label lblSystemTime;
    private System.Windows.Forms.DataGridViewButtonColumn colShow;

    private System.Windows.Forms.CheckBox chkStoreCustom;
    private System.Windows.Forms.TextBox txtStoreCustomList;


    private System.Windows.Forms.GroupBox grpAutoDrop;
    private System.Windows.Forms.CheckBox chkAutoDrop;
    private System.Windows.Forms.Label lblDropDesc1;
    private System.Windows.Forms.CheckBox chkDropByHsd;
    private System.Windows.Forms.Label lblDropDesc2;
    private System.Windows.Forms.TextBox txtDropIds;

    // Auto Sell Controls
    private System.Windows.Forms.GroupBox grpSellOptions;
    private System.Windows.Forms.CheckBox chkAutoSellTrash;
    private System.Windows.Forms.Label lblSellEmptySlots;
    private System.Windows.Forms.NumericUpDown nudSellEmptySlots;
    private System.Windows.Forms.CheckBox chkDropInsteadOfSell;

    private System.Windows.Forms.GroupBox grpSellKeepFilter;
    private System.Windows.Forms.CheckBox chkKeepStarItems;
    private System.Windows.Forms.CheckBox chkKeepGodItems;
    private System.Windows.Forms.CheckBox chkKeepSkhItems;
    private System.Windows.Forms.Label lblSellMaxLevel;
    private System.Windows.Forms.NumericUpDown nudSellMaxLevel;
    private System.Windows.Forms.Label lblSellKeepIds;
    private System.Windows.Forms.TextBox txtSellKeepIds;

    private System.Windows.Forms.GroupBox grpSellCustom;
    private System.Windows.Forms.CheckBox chkSellCustomNoStarCheck;
    private System.Windows.Forms.Label lblSellCustomDesc;
    private System.Windows.Forms.TextBox txtSellCustomIdsList;

    // Auto Buy Controls
    private System.Windows.Forms.GroupBox grpAutoBuy;
    private System.Windows.Forms.CheckBox cbAutoBuyPrivateTicket;
    private System.Windows.Forms.CheckBox cbAutoBuyTdlt;
    private System.Windows.Forms.CheckBox cbAutoBuyKhauTrang;
    private System.Windows.Forms.NumericUpDown numBuyKhauTrangQty;
    private System.Windows.Forms.CheckBox cbAutoBuyCoBonLa;
    private System.Windows.Forms.NumericUpDown numBuyCoBonLaQty;
    private System.Windows.Forms.CheckBox cbAutoBuyBuaDe;
    private System.Windows.Forms.NumericUpDown numBuyBuaDeQty;

    private System.Windows.Forms.CheckBox chkAutoBuyCustom;
    private System.Windows.Forms.Button btnBuyCustomHelp;
    private System.Windows.Forms.TextBox txtBuyCustomList;

    // Auto Pick Controls
    private System.Windows.Forms.GroupBox grpAutoPick;
    private System.Windows.Forms.CheckBox chkAutoPick;
    private System.Windows.Forms.Label lblPickMode;
    private System.Windows.Forms.ComboBox cboPickMode;
    private System.Windows.Forms.CheckBox chkOnlyMyItems;
    private System.Windows.Forms.Label lblPickDesc;
    private System.Windows.Forms.TextBox txtPickIdsList;
    private System.Windows.Forms.Label lblPickBlackList;
    private System.Windows.Forms.TextBox txtPickBlackList;

    // Auto Use Item Controls
    private System.Windows.Forms.CheckBox chkUseCuongNo;
    private System.Windows.Forms.CheckBox chkUseBoHuyet;
    private System.Windows.Forms.CheckBox chkUseBoKhi;
    private System.Windows.Forms.CheckBox chkUseGiapXen;
    private System.Windows.Forms.CheckBox chkUseMask;
    private System.Windows.Forms.CheckBox chkUse4LeafClover;
    private System.Windows.Forms.CheckBox chkUseFood;
    private System.Windows.Forms.CheckBox chkUseDetector;
    private System.Windows.Forms.CheckBox chkUseItemById;
    private System.Windows.Forms.Label lblItemByIds;
    private System.Windows.Forms.TextBox txtItemByIds;

    // Bông tai & Cờ đen Controls
    private System.Windows.Forms.TabPage tabItemBongTaiCoDen;
    private System.Windows.Forms.GroupBox grpAutoBongTai;
    private System.Windows.Forms.Label lblBongTaiState;
    private System.Windows.Forms.ComboBox cboBongTaiState;
    private System.Windows.Forms.Label lblBongTaiPetAction;
    private System.Windows.Forms.ComboBox cboBongTaiPetAction;
    private System.Windows.Forms.Label lblBongTaiWarning;
    private System.Windows.Forms.GroupBox grpAutoCoDen;
    private System.Windows.Forms.CheckBox chkAutoCoDen;
    private System.Windows.Forms.ComboBox cboFlagType;
    private System.Windows.Forms.CheckBox chkDisableCoDenIfOthers;

    // Farm Feature UserControls
    private MvbtControl mvbtControl;
    private MvbtControl mhbtControl;
    private KilisControl kilisControl;
    private UpSkhControl upSkhControl;
    private PetControl petControl;
    private ScheduleControl scheduleControl;
}

