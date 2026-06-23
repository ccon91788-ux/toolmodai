using System;
using System.Runtime.InteropServices;

using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Panel.Models;
using Panel.Repositories;
using Panel.Services;
using Panel.Sockets;
using Panel.Boss;


namespace Panel;

public partial class Form1 : Form
{
    private enum PositionRequestSource
    {
        None = 0,
        Pet = 1,
        BuffNamek = 2,
        ReducePower = 3
    }

    private readonly AccountRepository _accountRepo;
    private List<MapTemplate> _allMaps = new();
    private readonly AccountSettingsService _accountSettingsService;
    private readonly PanelSocketServer _socketServer;
    private readonly BossHuntCoordinator _bossCoordinator;


    private System.Windows.Forms.Timer _autoLoginTimer;
    private System.Windows.Forms.Timer _timeSyncTimer;
    private System.Windows.Forms.Timer _uiUpdateTimer;
    private System.Windows.Forms.Timer _autoCleanRamTimer;

    // Lưu mapping row index → accountId để cập nhật DataGridView nhanh
    private readonly Dictionary<int, int> _rowToAccountId = new();

    // Data setting đang chọn
    private int _currentSelectedAccountId = -1;
    private AccountSettingsRoot _currentSettings = new();
    private GeneralSettings _currentGeneralSettings = new();
    private TrainFeatureSettings _currentTrainSettings = new();
    private AutoUpZinSettings _currentAutoUpZinSettings = new();
    private AutoUpZinTo700kSettings _currentUpZin700kSettings = new();
    private MvbtFeatureSettings _currentMvbtSettings = new();
    private MvbtFeatureSettings _currentMhbtSettings = new();
    private KilisFeatureSettings _currentKilisSettings = new();
    private BossVegetaCityFeatureSettings _currentBossVegetaCitySettings = new();
    private DauThanSettings _currentDauThanSettings = new();
    private BuffNamekFeatureSettings _currentBuffNamekSettings = new();
    private ReducePowerFeatureSettings _currentReducePowerSettings = new();
    private BossFeatureSettings _currentBossSettings = new();
    private DailyQuestFeatureSettings _currentDailyQuestSettings = new();
    private AttendanceFeatureSettings _currentAttendanceSettings = new();
    private AutoAmuletSettings _currentAutoAmuletSettings = new();
    private AutoBossControl _autoBossControl = null!;
    private DailyQuestControl _dailyQuestControl = null!;
    private AttendanceControl _attendanceControl = null!;
    private TabPage _tabDailyQuest = null!;
    private TabPage _tabAttendance = null!;
    private TabPage _tabAutoUpZin = null!;
    private TabPage _tabUpZin700k = null!;
    private Button _btnMainTabVisibility = null!;
    private readonly List<TabPage> _mainTabOrder = new();
    private Form _mainTabPopup = null!;
    private CheckedListBox _mainTabCheckedList = null!;
    private bool _isUpdatingMainTabList = false;

    private ItemSettings _currentItemSettings = new();
    private ScheduleSettings _currentScheduleSettings = new();
    private SupportSettings _currentSupportSettings = new();
    private readonly HashSet<int> _settingsSyncedAfterStatus = new();
    private readonly object _settingsSyncLock = new();
    private readonly List<Action<int, AccountSettingsRoot>> _settingsReplayHandlers = new();

    private System.Windows.Forms.Timer _mapTypingTimer = null!;
    // FIX 4: Debounce timer cho TrainSettingsControl_Changed — tránh gửi liên tục khi user thay đổi nhiều field
    private System.Windows.Forms.Timer _trainDebounceTimer = null!;
    private MapTemplate? _lastValidMapTemplate;

    // ─── Quản lý Config (Copy / Paste) ──────────────────────────────────
    private AccountSettingsRoot? _clipboardSettings = null;
    private string _clipboardAccountInfo = "";

    // ─── Uptime & Stats tracking (Option B – Panel side) ─────────────────
    private readonly Dictionary<int, TimeSpan> _autoAccumulatedTime = new();
    private readonly Dictionary<int, DateTime> _lastCharStatsTime = new();
    private readonly Dictionary<int, (long gold, long power)> _autoBaseline = new();
    private readonly Dictionary<int, (long gold, long power)> _autoLatest = new();
    private readonly Dictionary<int, string> _latestCharInfoText = new();
    private readonly Dictionary<int, int> _latestMvbt = new();
    private readonly Dictionary<int, int> _latestMhbt = new();
    private readonly Dictionary<int, int> _latestKilis = new();
    private readonly Dictionary<int, int> _latestFarmedKilis = new();
    private readonly Dictionary<int, int> _latestFarmedMvbt = new();
    private readonly Dictionary<int, int> _latestFarmedMhbt = new();

    // ─── Uptime & Stats tracking Đệ tử ────────────────────────────────────
    private readonly Dictionary<int, TimeSpan> _autoAccumulatedTimePet = new();
    private readonly Dictionary<int, DateTime> _lastPetStatsTime = new();
    private readonly Dictionary<int, (long power, long tiemNang)> _autoBaselinePet = new();
    private readonly Dictionary<int, (long power, long tiemNang)> _autoLatestPet = new();
    private readonly Dictionary<int, string> _latestPetInfoText = new();
    private readonly Dictionary<int, DailyQuestRuntimeStatus> _dailyQuestRuntimeByAccount = new();
    private PositionRequestSource _pendingPositionRequestSource = PositionRequestSource.None;

    public Form1(string customerName = "", string licenseExpiresAt = "")
    {
        InitializeComponent();

        // Set title bar: tên khách + thời hạn key
        this.Text = BuildLicenseTitle(customerName, licenseExpiresAt);

        this.mvbtControl.MasterCheckboxText = "Auto MVBT";
        this.mhbtControl.MasterCheckboxText = "Auto MHBT";
        this.chkBossVegetaCityEnable.Text = "Bật auto Boss VegetaCity";
        this.chkBossVegetaCityEnable.Width = 180;
        this.chkBossVegetaCityAuto3h.Text = "Auto khung giờ 15h";

        ApplyUiTheme();

        InitializeTrainAdvancedTab();
        InitializeKsVangTab();
        InitializeSkillDropdownTrain();
        InitializeAutoUpZinUi();
        InitializeUpZin700kUi();
        InitializeScheduleTab();

        // Thêm Tab Săn Boss động
        _autoBossControl = new AutoBossControl();
        _autoBossControl.Dock = DockStyle.Fill;
        _autoBossControl.SettingsChanged += AutoBossSettingsControl_Changed;
        _autoBossControl.SyncParamsRequested += AutoBossSyncParams_Requested;
        var tabBoss = new TabPage("Săn Boss");
        // Màu Slate 300
        tabBoss.BackColor = Color.FromArgb(203, 213, 225);
        tabBoss.Controls.Add(_autoBossControl);
        
        int petIndex = this.tabControlFeatures.TabPages.IndexOf(this.tabPet);
        if (petIndex >= 0)
            this.tabControlFeatures.TabPages.Insert(petIndex + 1, tabBoss);
        else
            this.tabControlFeatures.TabPages.Add(tabBoss);

        _dailyQuestControl = new DailyQuestControl();
        _dailyQuestControl.Dock = DockStyle.Fill;
        _dailyQuestControl.SettingsChanged += DailyQuestSettingsControl_Changed;
        _dailyQuestControl.ToggleAutoRequested += DailyQuestControl_ToggleAutoRequested;
        _tabDailyQuest = new TabPage("Auto NVHN");
        _tabDailyQuest.BackColor = Color.FromArgb(241, 245, 249);
        _tabDailyQuest.Controls.Add(_dailyQuestControl);

        int bossIndex = this.tabControlFeatures.TabPages.IndexOf(tabBoss);
        if (bossIndex >= 0)
            this.tabControlFeatures.TabPages.Insert(bossIndex + 1, _tabDailyQuest);
        else
            this.tabControlFeatures.TabPages.Add(_tabDailyQuest);

        _attendanceControl = new AttendanceControl();
        _attendanceControl.Dock = DockStyle.Fill;
        _attendanceControl.SettingsChanged += AttendanceSettingsControl_Changed;
        _attendanceControl.ToggleAutoRequested += AttendanceControl_ToggleAutoRequested;
        _tabAttendance = new TabPage("Điểm danh");
        _tabAttendance.BackColor = Color.FromArgb(241, 245, 249);
        _tabAttendance.Controls.Add(_attendanceControl);

        int dailyQuestIndex = this.tabControlFeatures.TabPages.IndexOf(_tabDailyQuest);
        if (dailyQuestIndex >= 0)
            this.tabControlFeatures.TabPages.Insert(dailyQuestIndex + 1, _tabAttendance);
        else
            this.tabControlFeatures.TabPages.Add(_tabAttendance);

        InitializeMainTabVisibilityUi();

        _accountRepo = new AccountRepository();
        _accountSettingsService = new AccountSettingsService(_accountRepo);
        _socketServer = new PanelSocketServer();
        
        var reducePowerCoordinator = new Panel.Services.ReducePowerCoordinator(_socketServer, _accountRepo, _accountSettingsService);
        
        _bossCoordinator = new BossHuntCoordinator(_socketServer);
        _bossCoordinator.OnLog += LogToUi;
        
        _socketServer.OnBossFoundReceived += _bossCoordinator.OnBossFound;
        _socketServer.OnBossKilledReceived += _bossCoordinator.OnBossKilled;
        _socketServer.OnBossDeadReceived += _bossCoordinator.OnBossDead;
        _socketServer.OnBossScoutDoneReceived += _bossCoordinator.OnBossScoutDone;
        _socketServer.OnAntiAdminDoneReceived += _bossCoordinator.OnAntiAdminDone;
        _socketServer.OnClientConnectionChanged += (id, isConnected) => 
        {
            if (!isConnected) _bossCoordinator.OnClientDisconnected(id);
        };
        
        RegisterSettingsReplayHandlers();


        _mapTypingTimer = new System.Windows.Forms.Timer();
        _mapTypingTimer.Interval = 3000;
        _mapTypingTimer.Tick += MapTypingTimer_Tick;

        // FIX 4: Debounce 400ms — chỉ gửi command sau khi user dừng thay đổi settings Train
        _trainDebounceTimer = new System.Windows.Forms.Timer();
        _trainDebounceTimer.Interval = 400;
        _trainDebounceTimer.Tick += TrainDebounceTimer_Tick;

        _autoLoginTimer = new System.Windows.Forms.Timer();
        _autoLoginTimer.Interval = 3000;
        _autoLoginTimer.Tick += AutoLoginTimer_Tick;
        this.chkAutoLogin.CheckedChanged += ChkAutoLogin_CheckedChanged;
        _autoLoginTimer.Start();

        _uiUpdateTimer = new System.Windows.Forms.Timer();
        _uiUpdateTimer.Interval = 1000;
        _uiUpdateTimer.Tick += UiUpdateTimer_Tick;
        _uiUpdateTimer.Start();

        _autoCleanRamTimer = new System.Windows.Forms.Timer();
        _autoCleanRamTimer.Tick += (s, e) => Panel.Helpers.SystemResourceHelper.CleanOsMemory();
        ApplyAutoCleanRamConfig();

        // Khởi tạo menu Popup cho Server
        SetupServerContextMenu();

        // Tự động bỏ qua lỗi Format/Value không hợp lệ của DataGridViewComboBox
        this.dgvAccounts.DataError += (sender, e) => { e.Cancel = true; };

        // Gắn sự kiện
        this.btnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
        this.btnEdit.Click += new System.EventHandler(this.BtnEdit_Click);
        this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
        this.btnDeleteSelected.Click += new System.EventHandler(this.BtnDeleteSelected_Click);
        this.btnToggleGame.Click += new System.EventHandler(this.BtnToggleGame_Click);
        this.btnCloseAll.Click += new System.EventHandler(this.BtnCloseAll_Click);
        this.btnArrangeWindows.Click += new System.EventHandler(this.BtnArrangeWindows_Click);
        this.btnHideAll.Click += new System.EventHandler(this.BtnHideAll_Click);
        this.btnAutoBoMong.Click += BtnAutoBoMong_Click;
        this.dgvAccounts.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvAccounts_CellClick);
        this.dgvAccounts.SelectionChanged += new System.EventHandler(this.DgvAccounts_SelectionChanged);
        this.dgvAccounts.CellValueChanged += DgvAccounts_CellValueChanged;
        this.dgvAccounts.CurrentCellDirtyStateChanged += DgvAccounts_CurrentCellDirtyStateChanged;
        this.dgvAccounts.CellFormatting += DgvAccounts_CellFormatting;
        this.chkHideAccount.CheckedChanged += ChkHideAccount_CheckedChanged;
        this.btnXemHanhTrang.Click += BtnXemHanhTrang_Click;
        this.btnXemRuongDo.Click += BtnXemRuongDo_Click;
        
        // Settings changed events
        this.chkEatChicken.CheckedChanged += GeneralSettingsControl_Changed;
        this.chkUseTdltXmap.CheckedChanged += GeneralSettingsControl_Changed;
        this.cboActionOnDeath.SelectedIndexChanged += GeneralSettingsControl_Changed;
        this.chkUseProxy.CheckedChanged += GeneralSettingsControl_Changed;
        this.cboProxyType.SelectedIndexChanged += GeneralSettingsControl_Changed;
        this.txtProxyAddress.TextChanged += GeneralSettingsControl_Changed;

        this.chkTrainEnable.CheckedChanged += TrainSettingsControl_Changed;
        this.cboTrainMapId.SelectedIndexChanged += TrainSettingsControl_Changed;
        this.cboTrainMapId.TextUpdate += CboTrainMapId_TextUpdate;
        this.chkTrainZoneRequire.CheckedChanged += TrainSettingsControl_Changed;
        this.txtTrainZone.TextChanged += TrainSettingsControl_Changed;
        this.chkUseTDLT.CheckedChanged += TrainSettingsControl_Changed;
        this.chkOnlyUsePunch.CheckedChanged += TrainSettingsControl_Changed;
        this.chkFreezePunchSkillCd.CheckedChanged += TrainSettingsControl_Changed;
        
        this.mvbtControl.SettingsChanged += MvbtSettingsControl_Changed;
        this.mvbtControl.ResetCountRequested += MvbtControl_ResetCountRequested;
        this.mhbtControl.SettingsChanged += MhbtSettingsControl_Changed;
        this.mhbtControl.ResetCountRequested += MhbtControl_ResetCountRequested;
        this.kilisControl.SettingsChanged += KilisSettingsControl_Changed;
        this.chkBossVegetaCityEnable.CheckedChanged += BossVegetaCitySettingsControl_Changed;
        this.chkBossVegetaCityAuto3h.CheckedChanged += BossVegetaCitySettingsControl_Changed;
        this.chkBossVegetaCityAuto2230.CheckedChanged += BossVegetaCitySettingsControl_Changed;
        this.chkBossVegetaCityReviveByGem.CheckedChanged += BossVegetaCitySettingsControl_Changed;
        this.chkBossVegetaCityUseTdlt.CheckedChanged += BossVegetaCitySettingsControl_Changed;
        this.petControl.SettingsChanged += PetSettingsControl_Changed;
        this.dauThanControl.SettingsChanged += DauThanSettingsControl_Changed;
        this.buffNamekControl.SettingsChanged += BuffNamekSettingsControl_Changed;

        this.petControl.RequestGetLocation += PetControl_RequestGetLocation;
        this.buffNamekControl.RequestGetPosition += BuffNamekControl_RequestGetPosition;
        this.upSkhControl.SettingsChanged += UpSkhSettingsControl_Changed;
        this.chkUseKaiokenLienHoan.CheckedChanged += TrainSettingsControl_Changed;
        this.chkAvoidSuperMob.CheckedChanged += TrainSettingsControl_Changed;
        this.chkChangeLowPlayerZoneIfNoMob.CheckedChanged += TrainSettingsControl_Changed;
        this.cboMobTargetType.SelectedIndexChanged += TrainSettingsControl_Changed;
        this.txtMobIds.TextChanged += TrainSettingsControl_Changed;
        this.cboTrainingArmorMode.SelectedIndexChanged += TrainSettingsControl_Changed;
        this.chkAutoDrop.CheckedChanged += DropSettingsControl_Changed;
        this.chkDropByHsd.CheckedChanged += DropSettingsControl_Changed;
        this.txtDropIds.TextChanged += DropSettingsControl_Changed;

        this.chkAutoStoreWhenFull.CheckedChanged += StoreSettingsControl_Changed;
        this.chkStoreKichHoat.CheckedChanged += StoreSettingsControl_Changed;
        this.chkStoreThanLinh.CheckedChanged += StoreSettingsControl_Changed;
        this.chkStorePhaLe.CheckedChanged += StoreSettingsControl_Changed;
        this.nudStoreStarCount.ValueChanged += StoreSettingsControl_Changed;
        this.chkStoreCustom.CheckedChanged += StoreSettingsControl_Changed;
        this.txtStoreCustomList.TextChanged += StoreSettingsControl_Changed;

        this.chkAutoSellTrash.CheckedChanged += SellSettingsControl_Changed;
        this.nudSellEmptySlots.ValueChanged += SellSettingsControl_Changed;
        this.chkDropInsteadOfSell.CheckedChanged += SellSettingsControl_Changed;
        this.chkKeepStarItems.CheckedChanged += SellSettingsControl_Changed;
        this.chkKeepGodItems.CheckedChanged += SellSettingsControl_Changed;
        this.chkKeepSkhItems.CheckedChanged += SellSettingsControl_Changed;
        this.nudSellMaxLevel.ValueChanged += SellSettingsControl_Changed;
        this.txtSellKeepIds.TextChanged += SellSettingsControl_Changed;
        this.chkSellCustomNoStarCheck.CheckedChanged += SellSettingsControl_Changed;
        this.txtSellCustomIdsList.TextChanged += SellSettingsControl_Changed;

        this.cbAutoBuyTdlt.CheckedChanged += BuySettingsControl_Changed;
        this.cbAutoBuyPrivateTicket.CheckedChanged += BuySettingsControl_Changed;
        this.cbAutoBuyKhauTrang.CheckedChanged += BuySettingsControl_Changed;
        this.numBuyKhauTrangQty.ValueChanged += BuySettingsControl_Changed;
        this.cbAutoBuyCoBonLa.CheckedChanged += BuySettingsControl_Changed;
        this.numBuyCoBonLaQty.ValueChanged += BuySettingsControl_Changed;
        this.cbAutoBuyBuaDe.CheckedChanged += BuySettingsControl_Changed;
        this.numBuyBuaDeQty.ValueChanged += BuySettingsControl_Changed;
        this.chkAutoBuyCustom.CheckedChanged += BuySettingsControl_Changed;
        this.txtBuyCustomList.TextChanged += BuySettingsControl_Changed;

        this.chkAutoPick.CheckedChanged += PickSettingsControl_Changed;
        this.cboPickMode.SelectedIndexChanged += PickSettingsControl_Changed;
        this.chkOnlyMyItems.CheckedChanged += PickSettingsControl_Changed;
        this.txtPickIdsList.TextChanged += PickSettingsControl_Changed;
        this.txtPickBlackList.TextChanged += PickSettingsControl_Changed;

        // Use Item settings events
        this.chkUseCuongNo.CheckedChanged += UseItemSettingsControl_Changed;
        this.chkUseBoHuyet.CheckedChanged += UseItemSettingsControl_Changed;
        this.chkUseBoKhi.CheckedChanged += UseItemSettingsControl_Changed;
        this.chkUseGiapXen.CheckedChanged += UseItemSettingsControl_Changed;
        this.chkUseMask.CheckedChanged += UseItemSettingsControl_Changed;
        this.chkUse4LeafClover.CheckedChanged += UseItemSettingsControl_Changed;
        this.chkUseFood.CheckedChanged += UseItemSettingsControl_Changed;
        this.chkUseDetector.CheckedChanged += UseItemSettingsControl_Changed;
        this.chkUseItemById.CheckedChanged += UseItemSettingsControl_Changed;
        this.txtItemByIds.TextChanged += UseItemSettingsControl_Changed;

        // InitializeSupportTab removed
        // Bông tai & Cờ đen
        this.cboBongTaiState.SelectedIndexChanged    += SupportSettingsControl_Changed;
        this.cboBongTaiPetAction.SelectedIndexChanged += SupportSettingsControl_Changed;
        this.chkAutoCoDen.CheckedChanged             += SupportSettingsControl_Changed;
        this.chkDisableCoDenIfOthers.CheckedChanged  += SupportSettingsControl_Changed;
        this.cboFlagType.SelectedIndexChanged        += SupportSettingsControl_Changed;

        this.nudFilterTypeAccount.ValueChanged += GeneralSettingsControl_Changed;

        this.Load += Form1_Load;
        this.FormClosing += Form1_FormClosing;



        EnableAutoScroll(this);
    }

    private static string BuildLicenseTitle(string customerName, string licenseExpiresAt)
    {
        string baseName = "zFox Tool";

        if (string.IsNullOrWhiteSpace(customerName) && string.IsNullOrWhiteSpace(licenseExpiresAt))
            return baseName;

        string title = baseName;

        if (!string.IsNullOrWhiteSpace(customerName))
            title += $" - {customerName.Trim()}";

        if (!string.IsNullOrWhiteSpace(licenseExpiresAt))
        {
            // Parse ngày hết hạn từ server (format: yyyy-MM-dd HH:mm:ss)
            if (DateTime.TryParse(licenseExpiresAt, out DateTime expiryDate))
            {
                int daysLeft = (int)(expiryDate.Date - DateTime.Now.Date).TotalDays;
                string expiryStr = expiryDate.ToString("dd/MM/yyyy");

                if (daysLeft > 0)
                    title += $" | Còn {daysLeft} ngày (hết hạn {expiryStr})";
                else if (daysLeft == 0)
                    title += $" | HẾT HẠN HÔM NAY ({expiryStr})";
                else
                    title += $" | ĐÃ HẾT HẠN ({expiryStr})";
            }
        }

        return title;
    }

    private void EnableAutoScroll(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            if (c is TabPage page)
            {
                page.AutoScroll = true;
            }
            if (c.HasChildren)
            {
                EnableAutoScroll(c);
            }
        }
    }

    // ─── UI Theme ─────────────────────────────────────────────────────────

    private void ApplyUiTheme()
    {
        // ── Borderless form + custom title bar ────────────────────────
        this.FormBorderStyle = FormBorderStyle.None;
        this.ClientSize = new Size(this.ClientSize.Width, this.ClientSize.Height + 34);
        this.Padding = new Padding(0, 34, 0, 0);
        BuildTitleBar();

        // ── Màu nền ───────────────────────────────────────────────────
        var bgForm  = Color.FromArgb(241, 245, 249);  // slate-100, nền form
        var bgPanel = Color.FromArgb(226, 232, 240);  // slate-200, nền panel phụ

        this.BackColor                      = bgForm;
        this.panelTop.BackColor             = bgForm;
        this.panelBottom.BackColor          = bgForm;
        this.panelAccountActions.BackColor  = bgPanel;
        this.lblSocketStatus.ForeColor      = Color.FromArgb(22, 163, 74); // xanh lá

        // Checkbox & NumericUpDown trong panelTop: giữ màu chữ tối (nền giờ sáng)
        foreach (Control c in panelTop.Controls)
        {
            if (c is CheckBox chk) { chk.BackColor = Color.Transparent; chk.ForeColor = Color.FromArgb(30, 41, 59); }
            if (c is NumericUpDown nud) { nud.BackColor = Color.White; nud.ForeColor = Color.FromArgb(30, 41, 59); }
        }

        // ── DataGridView header ────────────────────────────────────────
        this.dgvAccounts.EnableHeadersVisualStyles = false;
        
        // Header: Nền trắng/xám siêu nhạt, chữ đen đậm, Có viền chia ô
        this.dgvAccounts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249); 
        this.dgvAccounts.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42); // slate-900
        this.dgvAccounts.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
        this.dgvAccounts.ColumnHeadersBorderStyle                = DataGridViewHeaderBorderStyle.Single;
        this.dgvAccounts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        this.dgvAccounts.ColumnHeadersHeight         = 32;

        // Lưới: Bật vạch kẻ caro (kẻ ngang và kẻ dọc) để không bị rối rắm
        this.dgvAccounts.RowHeadersVisible           = false;
        this.dgvAccounts.CellBorderStyle             = DataGridViewCellBorderStyle.Single; // Bật vách ngăn dọc
        this.dgvAccounts.GridColor                   = Color.FromArgb(203, 213, 225); // Viền lưới xám rõ sắc nét (Slate-300)
        this.dgvAccounts.BackgroundColor             = Color.White;
        this.dgvAccounts.BorderStyle                 = BorderStyle.None;

        // Rows: Tăng chiều cao dòng cho thoáng, Highlight lúc chọn
        this.dgvAccounts.RowTemplate.Height          = 28;
        this.dgvAccounts.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215); 
        this.dgvAccounts.DefaultCellStyle.SelectionForeColor = Color.White; 

        
        // CHỐNG TRONG SUỐT: Ép màu nền của lưới về màu trắng để không bị hòa vào nền tối của Tab
        this.dgvAccounts.DefaultCellStyle.BackColor  = Color.White;
        this.dgvAccounts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252); // Trắng xen kẽ xanh siêu nhạt

        this.dgvAccounts.EditMode                    = DataGridViewEditMode.EditOnEnter;
        this.dgvAccounts.DefaultCellStyle.Padding    = new Padding(0, 1, 0, 1);

        // ── Đồng bộ quét tĩnh kiểu Flat Styling lên Vùng nội dung Cài đặt ──────
        // tabControlFeatures chứa mọi tab Đánh quái, Item, Nâng cao...
        SetupFlatControls(this.tabControlFeatures);
        // Quét luôn cả Tab Thông tin sư phụ/đệ tử
        SetupFlatControls(this.tabControlAccountInfo);
        SetupFlatControls(this.grpSystemInfo);

        // ── Flat button style – Dark Slate (Xám tối chuyên nghiệp) ────────────────────────
        // Phong cách Admin Panel / Server Tool: Xám tối trầm, không sặc sỡ, nhìn nghiêm túc.
        
        void SlateBtn(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.BackColor = Color.FromArgb(71, 85, 105);   // Slate-600 (Bớt tối hơn, tiệp màu nền sáng)
            b.ForeColor = Color.FromArgb(241, 245, 249); // Slate-100 (Chữ sáng)
            b.Font      = new Font("Segoe UI", 8.5f, FontStyle.Regular); 
            
            b.FlatAppearance.BorderSize          = 1; 
            b.FlatAppearance.BorderColor         = Color.FromArgb(100, 116, 139); // Slate-500 viền nhẹ
            b.FlatAppearance.MouseOverBackColor  = Color.FromArgb(100, 116, 139); // Slate-500 hover
            b.FlatAppearance.MouseDownBackColor  = Color.FromArgb(51, 65, 85);  // Slate-700 nhấn
            
            b.Cursor    = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
        }

        // panelTop (Quản lý)
        SlateBtn(btnToggleGame);  SlateBtn(btnAdd);         SlateBtn(btnEdit);
        SlateBtn(btnDelete);      SlateBtn(btnDeleteSelected); SlateBtn(btnSelectAll);
        SlateBtn(btnSettings);    SlateBtn(btnArrangeWindows); SlateBtn(btnCloseAll);    SlateBtn(btnHideAll);

        // panelAccountActions (Hành động kịch bản)
        SlateBtn(btnBatAuto);           SlateBtn(btnDungAuto);
        SlateBtn(btnBatAutoTatCaChon);  SlateBtn(btnDungAutoTatCaChon);
        SlateBtn(btnGDVP);              SlateBtn(btnAutoBoMong);

        // Nút rời
        SlateBtn(btnTestProxy);

        // Nút Quản lý Config
        SlateBtn(btnCopyConfig);
        SlateBtn(btnPasteConfigCurrent);
        SlateBtn(btnPasteConfigChecked);
        SlateBtn(btnPasteConfigAll);
        SlateBtn(btnPasteConfigByType);

        if (_btnMainTabVisibility != null)
        {
            SlateBtn(_btnMainTabVisibility);
        }
    }

    private void InitializeMainTabVisibilityUi()
    {
        _btnMainTabVisibility = new Button
        {
            Name = "btnMainTabVisibility",
            Text = "Hiển thị",
            Size = new Size(80, btnAutoBoMong.Height),
            Location = new Point(btnAutoBoMong.Right + 5, btnAutoBoMong.Top)
        };

        _btnMainTabVisibility.FlatStyle = FlatStyle.Flat;
        _btnMainTabVisibility.BackColor = Color.FromArgb(71, 85, 105);
        _btnMainTabVisibility.ForeColor = Color.FromArgb(241, 245, 249);
        _btnMainTabVisibility.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        _btnMainTabVisibility.FlatAppearance.BorderSize = 1;
        _btnMainTabVisibility.FlatAppearance.BorderColor = Color.FromArgb(100, 116, 139);
        _btnMainTabVisibility.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 116, 139);
        _btnMainTabVisibility.FlatAppearance.MouseDownBackColor = Color.FromArgb(51, 65, 85);
        _btnMainTabVisibility.Cursor = Cursors.Hand;
        _btnMainTabVisibility.UseVisualStyleBackColor = false;
        _btnMainTabVisibility.Click += BtnMainTabVisibility_Click;
        panelAccountActions.Controls.Add(_btnMainTabVisibility);
        _btnMainTabVisibility.BringToFront();

        _mainTabPopup = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            TopMost = true,
            MinimizeBox = false,
            MaximizeBox = false,
            Size = new Size(240, 260)
        };
        _mainTabPopup.Deactivate += (_, __) => _mainTabPopup.Hide();

        _mainTabCheckedList = new CheckedListBox
        {
            Dock = DockStyle.Fill,
            CheckOnClick = true,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular)
        };
        _mainTabCheckedList.ItemCheck += MainTabCheckedList_ItemCheck;
        _mainTabPopup.Controls.Add(_mainTabCheckedList);

        _mainTabOrder.Clear();
        for (int i = 0; i < tabControlFeatures.TabPages.Count; i++)
        {
            _mainTabOrder.Add(tabControlFeatures.TabPages[i]);
        }

        SyncMainTabCheckedList();

        // Load và áp dụng setting tab visibility từ AppConfig
        LoadMainTabVisibilityConfig();
    }

    private void BtnMainTabVisibility_Click(object? sender, EventArgs e)
    {
        if (_mainTabPopup.Visible)
        {
            _mainTabPopup.Hide();
            return;
        }

        SyncMainTabCheckedList();
        Point p = _btnMainTabVisibility.PointToScreen(new Point(0, -_mainTabPopup.Height));
        _mainTabPopup.Location = p;
        _mainTabPopup.Show(this);
        _mainTabPopup.BringToFront();
    }

    private void SyncMainTabCheckedList()
    {
        _isUpdatingMainTabList = true;
        _mainTabCheckedList.Items.Clear();
        for (int i = 0; i < _mainTabOrder.Count; i++)
        {
            TabPage page = _mainTabOrder[i];
            bool visible = tabControlFeatures.TabPages.Contains(page);
            _mainTabCheckedList.Items.Add(page.Text, visible);
        }
        _isUpdatingMainTabList = false;
    }

    private void SaveMainTabVisibilityConfig()
    {
        try
        {
            var config = ConfigManager.Load();
            config.VisibleMainTabs = tabControlFeatures.TabPages.Cast<TabPage>().Select(p => p.Text).ToList();
            ConfigManager.Save(config);
        }
        catch
        {
            // Bỏ qua lỗi lưu config
        }
    }

    private void LoadMainTabVisibilityConfig()
    {
        try
        {
            var config = ConfigManager.Load();
            if (config.VisibleMainTabs == null || config.VisibleMainTabs.Count == 0)
            {
                // Nếu chưa có config, giữ nguyên mặc định (tất cả tab visible)
                return;
            }

            // Ẩn tất cả tab trước
            var allTabs = tabControlFeatures.TabPages.Cast<TabPage>().ToList();
            tabControlFeatures.TabPages.Clear();

            // Chỉ hiện các tab có trong config, giữ đúng thứ tự trong config
            foreach (string tabName in config.VisibleMainTabs)
            {
                var tab = allTabs.FirstOrDefault(t => t.Text == tabName);
                if (tab != null)
                {
                    tabControlFeatures.TabPages.Add(tab);
                }
            }

            // Đồng bộ lại CheckedListBox
            SyncMainTabCheckedList();
        }
        catch
        {
            // Bỏ qua lỗi load config, giữ mặc định
        }
    }

    private void MainTabCheckedList_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        if (_isUpdatingMainTabList) return;
        if (e.Index < 0 || e.Index >= _mainTabOrder.Count) return;

        TabPage page = _mainTabOrder[e.Index];
        bool shouldShow = e.NewValue == CheckState.Checked;
        bool visible = tabControlFeatures.TabPages.Contains(page);

        if (visible == shouldShow) return;

        if (!shouldShow && tabControlFeatures.TabPages.Count <= 1)
        {
            e.NewValue = CheckState.Checked;
            return;
        }

        if (shouldShow)
        {
            int targetOrder = _mainTabOrder.IndexOf(page);
            int insertIndex = tabControlFeatures.TabPages.Count;
            for (int i = 0; i < tabControlFeatures.TabPages.Count; i++)
            {
                TabPage current = tabControlFeatures.TabPages[i];
                int currentOrder = _mainTabOrder.IndexOf(current);
                if (currentOrder > targetOrder)
                {
                    insertIndex = i;
                    break;
                }
            }
            tabControlFeatures.TabPages.Insert(insertIndex, page);
        }
        else
        {
            tabControlFeatures.TabPages.Remove(page);
        }

        // Lưu setting tab visibility vào AppConfig
        SaveMainTabVisibilityConfig();
    }

    private void BuildTitleBar()
    {
        int barH = 34;
        int btnSize = 36; // Kích cỡ nút tròn

        var barBg   = Color.FromArgb(71, 85, 105);   // Slate-600 — hài hòa với nút
        var barText = Color.FromArgb(241, 245, 249); // Slate-100 — chữ sáng rõ

        var titleBar = new System.Windows.Forms.Panel
        {
            Name      = "panelTitleBar",
            BackColor = barBg,
            // Thay vì Dock(bị Padding đè xuống), dùng Anchor và Bounds ép cứng lên mép trên (Y=0)
            Bounds    = new Rectangle(0, 0, this.ClientSize.Width, barH),
            Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        // ── Nút Thu nhỏ — gạch ngang TRẮNG ──
        var btnMin = new Button
        {
            Text      = "",
            Width     = 46,
            Dock      = DockStyle.Right,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor    = Cursors.Hand,
            TabStop   = false
        };
        btnMin.FlatAppearance.BorderSize = 0;
        btnMin.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 116, 139); // Slate-500 hover
        var _minHover = false;
        btnMin.MouseEnter += (_, __) => { _minHover = true;  btnMin.Invalidate(); };
        btnMin.MouseLeave += (_, __) => { _minHover = false; btnMin.Invalidate(); };
        btnMin.Paint += (s, pe) => {
            var g = pe.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            // Màu trắng bình thường, sáng hơn khi hover
            var c = _minHover ? Color.White : Color.FromArgb(210, 218, 235);
            using var pen = new Pen(c, 2f);
            int cx = btnMin.Width / 2;
            int cy = btnMin.Height / 2;
            int r = Math.Min(btnMin.Width, btnMin.Height) / 5;
            g.DrawLine(pen, cx - r, cy, cx + r, cy);
        };
        btnMin.Click += (_, __) => this.WindowState = FormWindowState.Minimized;

        // ── Nút Đóng — dấu X ĐỎ NHẠT luôn rõ ──
        var btnClose = new Button
        {
            Text      = "",
            Width     = 46,
            Dock      = DockStyle.Right,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor    = Cursors.Hand,
            TabStop   = false
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 38, 38); // Red-600 hover
        var _closeHover = false;
        btnClose.MouseEnter += (_, __) => { _closeHover = true;  btnClose.Invalidate(); };
        btnClose.MouseLeave += (_, __) => { _closeHover = false; btnClose.Invalidate(); };
        btnClose.Paint += (s, pe) => {
            var g = pe.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            // X luôn đỏ nhạt → hover trắng trên nền đỏ
            var c = _closeHover ? Color.White : Color.FromArgb(252, 129, 129); // Red-400
            using var pen = new Pen(c, 2f);
            int cx = btnClose.Width / 2;
            int cy = btnClose.Height / 2;
            int r = Math.Min(btnClose.Width, btnClose.Height) / 5;
            g.DrawLine(pen, cx - r, cy - r, cx + r, cy + r);
            g.DrawLine(pen, cx + r, cy - r, cx - r, cy + r);
        };
        btnClose.Click += (_, __) => Application.Exit();

        // Tên form
        var lblTitle = new Label
        {
            Text      = this.Text,
            ForeColor = barText,
            Font      = new Font("Segoe UI", 9f, FontStyle.Regular),
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent,
            Padding   = new Padding(14, 0, 0, 0)
        };

        // WinForms ưu tiên Docking từ theo thứ tự add (càng thêm sau càng áp sát mép màn hình).
        // Nút Đóng (btnClose) phải ép sát mép phải, nên thêm nó VÀO SAU nút Thu Nhỏ (btnMin)!
        titleBar.Controls.Add(btnMin);
        titleBar.Controls.Add(btnClose);
        titleBar.Controls.Add(lblTitle);

        // Kéo di chuyển form
        Point _dragOff = Point.Empty;
        bool  _isDrag  = false;
        MouseEventHandler onDown = (s, e) => { _isDrag = true;  _dragOff = e.Location; };
        MouseEventHandler onMove = (s, e) => { if (_isDrag) this.Location = new Point(this.Location.X + e.X - _dragOff.X, this.Location.Y + e.Y - _dragOff.Y); };
        MouseEventHandler onUp   = (s, e) => _isDrag = false;

        foreach (Control c in new Control[] { titleBar, lblTitle })
        {
            c.MouseDown += onDown;
            c.MouseMove += onMove;
            c.MouseUp   += onUp;
        }

        this.Controls.Add(titleBar);
        titleBar.BringToFront();
    }

    /// <summary>Bo tròn góc tất cả flat button trên form (làm mịn góc).</summary>
    private void ApplyRoundedCorners(int radius = 7)
    {
        ApplyRoundedRegionsTo(this, radius);
    }

    private static void ApplyRoundedRegionsTo(Control parent, int radius)
    {
        foreach (Control c in parent.Controls)
        {
            if (c is Button btn && btn.FlatStyle == FlatStyle.Flat && btn.Width > 0 && btn.Height > 0)
            {
                // Ngăn cản event gán nhiều lần bằng cách gỡ ra trước
                btn.Paint -= Btn_Paint_SmoothCorners;
                btn.Paint += Btn_Paint_SmoothCorners;
                // Lưu radius vào Tag hoặc giả thiết dùng số 7 mặc định
                btn.Tag = radius;
            }

            if (c.HasChildren)
                ApplyRoundedRegionsTo(c, radius);
        }
    }

    private static void Btn_Paint_SmoothCorners(object? sender, PaintEventArgs e)
    {
        if (sender is not Button btn || btn.Parent == null) return;
        
        int r = btn.Tag is int radius ? radius : 7;
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        
        int w = btn.Width, h = btn.Height;
        
        // Vẽ 4 góc đè bằng màu nền. Kéo rộng các biên bên ngoài ra xa (-2 hoặc +2)
        // để vùng răng cưa cạnh tranh không thể rò rỉ vào viền vuông của button.
        using var brush = new SolidBrush(btn.Parent.BackColor);
        
        // Góc trên trái
        var pathTL = new System.Drawing.Drawing2D.GraphicsPath();
        pathTL.AddArc(0, 0, r * 2, r * 2, 180, 90);
        pathTL.AddLine(r, 0, r, -2);
        pathTL.AddLine(r, -2, -2, -2);
        pathTL.AddLine(-2, -2, -2, r);
        pathTL.AddLine(-2, r, 0, r);
        pathTL.CloseFigure();
        e.Graphics.FillPath(brush, pathTL);

        // Góc trên phải
        var pathTR = new System.Drawing.Drawing2D.GraphicsPath();
        pathTR.AddArc(w - r * 2, 0, r * 2, r * 2, 270, 90);
        pathTR.AddLine(w, r, w + 2, r);
        pathTR.AddLine(w + 2, r, w + 2, -2);
        pathTR.AddLine(w + 2, -2, w - r, -2);
        pathTR.AddLine(w - r, -2, w - r, 0);
        pathTR.CloseFigure();
        e.Graphics.FillPath(brush, pathTR);

        // Góc dưới phải
        var pathBR = new System.Drawing.Drawing2D.GraphicsPath();
        pathBR.AddArc(w - r * 2, h - r * 2, r * 2, r * 2, 0, 90);
        pathBR.AddLine(w - r, h, w - r, h + 2);
        pathBR.AddLine(w - r, h + 2, w + 2, h + 2);
        pathBR.AddLine(w + 2, h + 2, w + 2, h - r);
        pathBR.AddLine(w + 2, h - r, w, h - r);
        pathBR.CloseFigure();
        e.Graphics.FillPath(brush, pathBR);

        // Góc dưới trái
        var pathBL = new System.Drawing.Drawing2D.GraphicsPath();
        pathBL.AddArc(0, h - r * 2, r * 2, r * 2, 90, 90);
        pathBL.AddLine(0, h - r, -2, h - r);
        pathBL.AddLine(-2, h - r, -2, h + 2);
        pathBL.AddLine(-2, h + 2, r, h + 2);
        pathBL.AddLine(r, h + 2, r, h);
        pathBL.CloseFigure();
        e.Graphics.FillPath(brush, pathBL);
    }

    // ─── Sweep Styling Control Cài Đặt (Đệ quy) ─────────────────────────
    private void SetupFlatControls(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            if (c is TabPage tp)
            {
                // Tăng độ đậm màu xám sẫm (Slate-300) để độ ngả màu nhìn rõ mồn một!
                tp.UseVisualStyleBackColor = false;
                tp.BackColor = Color.FromArgb(203, 213, 225); 
            }
            else if (c is CheckBox chk)
            {
                chk.FlatStyle = FlatStyle.Flat;
                chk.FlatAppearance.BorderSize = 0;
            }
            else if (c is RadioButton rdo)
            {
                rdo.FlatStyle = FlatStyle.Flat;
                rdo.FlatAppearance.BorderSize = 0;
            }
            else if (c is TextBox txt)
            {
                txt.BorderStyle = BorderStyle.FixedSingle;
                txt.BackColor = Color.White; // Phủ nền trắng tinh để nổi bật rõ ràng ranh giới
            }
            else if (c is ComboBox cbo)
            {
                cbo.FlatStyle = FlatStyle.Flat;
                cbo.BackColor = Color.White; // Nền trắng
            }
            else if (c is NumericUpDown nud)
            {
                nud.BorderStyle = BorderStyle.FixedSingle;
                nud.BackColor = Color.White;
            }
            else if (c is GroupBox grp)
            {
                grp.FlatStyle = FlatStyle.Flat;
            }
            
            if (c.HasChildren)
            {
                SetupFlatControls(c);
            }
        }
    }

    private static InventoryForm? _currentInventoryForm;

    private void BtnXemHanhTrang_Click(object? sender, EventArgs e)
    {
        if (dgvAccounts.CurrentRow == null || !(dgvAccounts.CurrentRow.Tag is int accountId)) return;
        
        string name = dgvAccounts.CurrentRow.Cells[colCharacter.Index].Value?.ToString() ?? $"ID {accountId}";
        string server = dgvAccounts.CurrentRow.Cells[colServer.Index].Value?.ToString() ?? "N/A";

        if (_currentInventoryForm != null && !_currentInventoryForm.IsDisposed)
        {
            _currentInventoryForm.Close();
        }

        _currentInventoryForm = new InventoryForm(accountId, name, server, isChest: false);
        _currentInventoryForm.Show();
    }

    private void BtnXemRuongDo_Click(object? sender, EventArgs e)
    {
        if (dgvAccounts.CurrentRow == null || !(dgvAccounts.CurrentRow.Tag is int accountId)) return;

        string name = dgvAccounts.CurrentRow.Cells[colCharacter.Index].Value?.ToString() ?? $"ID {accountId}";
        string server = dgvAccounts.CurrentRow.Cells[colServer.Index].Value?.ToString() ?? "N/A";

        if (_currentInventoryForm != null && !_currentInventoryForm.IsDisposed)
        {
            _currentInventoryForm.Close();
        }

        _currentInventoryForm = new InventoryForm(accountId, name, server, isChest: true);
        _currentInventoryForm.Show();
    }


    private void BtnArrangeWindows_Click(object? sender, EventArgs e)
    {
        try
        {
            var config = ConfigManager.Load();
            int w = config.WindowWidth;
            int h = config.WindowHeight;

            GameLauncher.ResetWindowCounter();

            foreach (DataGridViewRow row in dgvAccounts.Rows)
            {
                if (row.Tag is int accountId)
                {
                    int pid = GetProcessIdForAccount(accountId);
                    if (pid > 0 && GameLauncher.IsRunning(pid))
                    {
                        GameLauncher.ArrangeGameWindow(pid, w, h, onlyVisible: true);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi sắp xếp cửa sổ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

#pragma warning disable CA1416
    private ContextMenuStrip _serverMenu = new ContextMenuStrip();
#pragma warning restore CA1416

    private void SetupServerContextMenu()
    {
        _serverMenu.ShowImageMargin = false;
        _serverMenu.BackColor = Color.FromArgb(241, 245, 249);
        _serverMenu.ItemClicked += ServerMenu_ItemClicked;

        foreach (var sv in ServerInfo.All)
        {
            var item = new ToolStripMenuItem(sv.DisplayName);
            item.ForeColor = Color.FromArgb(15, 23, 42); 
            item.Font = new Font("Segoe UI", 9f);
            _serverMenu.Items.Add(item);
        }
    }

    private void ServerMenu_ItemClicked(object? sender, ToolStripItemClickedEventArgs e)
    {
        if (e.ClickedItem == null) return;
        string serverName = e.ClickedItem.Text;

        if (dgvAccounts.CurrentCell == null || dgvAccounts.CurrentCell.OwningRow.Tag is not int accountId) return;

        // Cập nhật giá trị hiển thị (sẽ tự động nhảy trigger DgvAccounts_CellValueChanged cập nhật DB)
        dgvAccounts.CurrentCell.Value = serverName;
    }

    private bool _isForceClosing = false;

    private void Form1_Load(object? sender, EventArgs e)
    {
        LoadMapData();

        var config = ConfigManager.Load();
        chkHideAccount.Checked = config.HideAccount;
        chkAutoLogin.Checked = config.AutoLogin;
        chkAutoHideClient.Checked = config.AutoHideClient;
        numAutoLoginThread.Text = (config.AutoLoginThread > 0 ? config.AutoLoginThread : 2).ToString();

        this.chkHideAccount.CheckedChanged += SaveAppConfig_Changed;
        this.chkAutoLogin.CheckedChanged += SaveAppConfig_Changed;
        this.chkAutoHideClient.CheckedChanged += SaveAppConfig_Changed;
        this.numAutoLoginThread.TextChanged += SaveAppConfig_Changed;

        LoadAccounts();
        SelectFirstAccountAndLoadSettings();
        StartSocketServer();
        ApplyRoundedCorners();

        // Khởi động Heartbeat Anti-Crack V2
        HeartbeatService.OnLockoutTriggered += (string reason) => {
            this.Invoke((Action)(() => {
                _isForceClosing = true; // Bỏ qua confirm exit
                foreach (DataGridViewRow row in dgvAccounts.Rows) {
                    if (row.Tag is int accountId)
                    {
                        // Graceful Lockout: Tắt auto mềm qua socket thay vì rập khuôn đóng game (KillAccountRow)
                        _socketServer.SendCommand(accountId, "TRAIN_OFF");
                        _socketServer.SendCommand(accountId, "GOBACK_OFF");

                        // Đổi trạng thái hiển thị
                        row.Cells[colStatus.Index].Value = "LOCKED (No License)";
                    }
                }
                // Tắt Panel — popup lý do sẽ hiện ở Program.cs SAU khi Application.Run() kết thúc
                Application.Exit();
            }));
        };
        HeartbeatService.Start();

        // ─── Đồng bộ thời gian thực (NTP Time Sync) cho MVBT/MHBT ───
        _timeSyncTimer = new System.Windows.Forms.Timer { Interval = 10000 };
        _timeSyncTimer.Tick += TimeSyncTimer_Tick;
        _timeSyncTimer.Start();
        
        _ = TimeHelper.SyncWithInternetTimeAsync(); // Chạy nền không block UI
    }

    private void LoadMapData()
    {
        try
        {
            var maps = new List<MapTemplate> { new MapTemplate { Id = -1, Name = "Chưa chọn map" } };
            string mapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Maps.json");
            
            // Hỗ trợ chế độ Dev (chạy từ bin\Debug\net9.0-windows\win-x86\)
            if (!File.Exists(mapPath))
            {
                string devPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\Data\Maps.json"));
                if (File.Exists(devPath)) mapPath = devPath;
            }

            if (File.Exists(mapPath))
            {
                string json = File.ReadAllText(mapPath);
                var parsedMaps = System.Text.Json.JsonSerializer.Deserialize(json, PanelJsonContext.Default.ListMapTemplate);
                if (parsedMaps != null) maps.AddRange(parsedMaps);
            }
            _allMaps = new List<MapTemplate>(maps);
            cboTrainMapId.Items.Clear();
            foreach (var m in maps) cboTrainMapId.Items.Add(m);
            if (cboTrainMapId.Items.Count > 0) cboTrainMapId.SelectedIndex = 0;
            buffNamekControl.LoadMapItems(_allMaps);


        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tải Maps.json: " + ex.Message);
        }
    }

    private void SaveAppConfig_Changed(object? sender, EventArgs e)
    {
        var config = new AppConfig
        {
            HideAccount = chkHideAccount.Checked,
            AutoLogin = chkAutoLogin.Checked,
            AutoHideClient = chkAutoHideClient.Checked,
            AutoLoginThread = int.TryParse(numAutoLoginThread.Text, out int v) ? (v > 0 ? v : 2) : 2
        };
        ConfigManager.Save(config);
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // Kiểm tra xem có tab game nào đang mở không
        bool hasOpenGame = false;
        foreach (DataGridViewRow row in dgvAccounts.Rows)
        {
            string status = row.Cells[colStatus.Index].Value?.ToString() ?? "";
            if (!status.Contains("OFFLINE"))
            {
                hasOpenGame = true;
                break;
            }
        }

        if (hasOpenGame)
        {
            if (!_isForceClosing) 
            {
                var result = MessageBox.Show(
                    "Bạn đang có tab game đang mở, nếu tắt Panel thì toàn bộ tab game sẽ bị tắt theo.\nBạn có chắc chắn muốn tắt không?", 
                    "Xác nhận thoát", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Nếu đồng ý tắt (hoặc force closing), tiến hành kill hết process game
            foreach (DataGridViewRow row in dgvAccounts.Rows)
            {
                string status = row.Cells[colStatus.Index].Value?.ToString() ?? "";
                if (!status.Contains("OFFLINE"))
                {
                    KillAccountRow(row);
                }
            }
        }
        _socketServer.StopServer();

        HeartbeatService.Stop();
    }

    // ─── Boss Hunt Coordinator ──────────────────────────────────────────────
    private void LogToUi(string msg)
    {
        // Có thể tái sử dụng txtLogs hoặc MessageBox, tuỳ thiết kế UI của bạn. 
        // Ở đây thêm log vào Console hoặc Log UI chung (nếu có)
        Debug.WriteLine(msg);
        
        // Nếu Panel có ListBox hoặc TextBox log, thêm vào (ví dụ giả định)
        // this.Invoke((Action)(() => lblSocketStatus.Text = msg.Substring(0, Math.Min(msg.Length, 80))));
    }

    // ─── Socket Server ────────────────────────────────────────────────────

    public void SendSocketCommand(int accountId, string command)
    {
        _socketServer.SendCommand(accountId, command);
    }

    private void StartSocketServer()
    {
        _socketServer.OnStatusReceived += OnClientStatusReceived;
        _socketServer.OnClientConnectionChanged += OnClientConnectionChanged;
        _socketServer.OnCharInfoReceived += OnClientCharInfoReceived;
        _socketServer.OnCharStatsReceived += OnClientCharStatsReceived;
        _socketServer.OnCharItemsReceived += OnClientCharItemsReceived;
        _socketServer.OnPetInfoReceived += OnClientPetInfoReceived;
        _socketServer.OnPetStatsReceived += OnClientPetStatsReceived;
        _socketServer.OnSysPosReceived += OnSysPosReceived;
        _socketServer.OnSkhDataReceived += OnClientSkhDataReceived;
        _socketServer.OnSkhTimeReceived += OnClientSkhTimeReceived;
        _socketServer.OnGameLogReceived += OnGameLogReceived;
        _socketServer.OnInventoryDataReceived += OnClientInventoryDataReceived;
        _socketServer.OnBuffNamekStateReceived += OnBuffNamekStateReceived;
        _socketServer.OnDailyQuestStatusReceived += OnClientDailyQuestStatusReceived;
        _socketServer.OnAttendanceStatusReceived += OnClientAttendanceStatusReceived;
        
        // --- BOSS HUNT EVENTS ---
        _socketServer.OnBossFoundReceived += _bossCoordinator.OnBossFound;
        _socketServer.OnBossKilledReceived += _bossCoordinator.OnBossKilled;
        _socketServer.OnBossDeadReceived += _bossCoordinator.OnBossDead;
        _socketServer.OnAntiAdminDoneReceived += _bossCoordinator.OnAntiAdminDone;

        // Login status: cập nhật cột Data In Game khi client đang ở màn hình login
        _socketServer.OnLoginStatusReceived += OnClientLoginStatusReceived;

        // Client báo bị disconnect khỏi game server (quay về login screen), socket Panel vẫn sống
        _socketServer.OnBackToLoginReceived += OnClientBackToLogin;

        _socketServer.StartServer();
    }

    /// <summary>
    /// Client báo đã bị disconnect khỏi game server, quay về màn hình login.
    /// Socket Panel vẫn sống — khác với OnClientConnectionChanged(false).
    /// Force update status "3. LOGIN" vào DB + UI để Panel phát hiện đúng trạng thái.
    /// </summary>
    private void OnClientBackToLogin(int accountId)
    {
        // Cập nhật DB ngay trên background thread (tránh race condition với BeginInvoke)
        _accountRepo.UpdateStatus(accountId, "3. LOGIN", "---");

        // Reset settings sync flag để khi client login lại sẽ được push settings
        lock (_settingsSyncLock)
        {
            _settingsSyncedAfterStatus.Remove(accountId);
        }

        this.BeginInvoke((Action)(() =>
        {
            int rowIndex = FindRowByAccountId(accountId);
            if (rowIndex < 0) return;

            var row = dgvAccounts.Rows[rowIndex];
            row.Cells[colStatus.Index].Value = "3. LOGIN";
            row.Cells[colDataInGame.Index].Value = "Mất kết nối game, đang login lại...";

            if (dgvAccounts.CurrentRow != null && dgvAccounts.CurrentRow.Index == rowIndex)
                UpdateButtonState(row);
        }));
    }

    /// <summary>
    /// Nhận trạng thái login từ client (ví dụ "3. LOGIN: Đang kết nối TCP...")
    /// và cập nhật cột Data In Game song song với overlay trên màn hình.
    /// </summary>
    private void OnClientLoginStatusReceived(int accountId, string statusText)
    {
        this.BeginInvoke((Action)(() =>
        {
            int rowIndex = FindRowByAccountId(accountId);
            if (rowIndex < 0) return;

            var row = dgvAccounts.Rows[rowIndex];

            string currentStatus = row.Cells[colStatus.Index].Value?.ToString() ?? "";

            // Nếu status vẫn kẹt ở "1. ONLINE" nhưng client đã gửi LOGIN message,
            // nghĩa là BACK_TO_LOGIN chưa xử lý kịp → force chuyển sang "3. LOGIN"
            if (currentStatus == "1. ONLINE")
            {
                row.Cells[colStatus.Index].Value = "3. LOGIN";
                _accountRepo.UpdateStatus(accountId, "3. LOGIN", row.Cells[colCharacter.Index].Value?.ToString() ?? "---");
            }

            // statusText có dạng "3. LOGIN: Đang kết nối TCP..."
            // Tách phần sau "3. LOGIN: " để hiển thị vào Data In Game
            string displayText = statusText;
            if (statusText.StartsWith("3. LOGIN: "))
                displayText = statusText.Substring("3. LOGIN: ".Length);

            row.Cells[colDataInGame.Index].Value = displayText;
        }));
    }


    private void OnClientInventoryDataReceived(int accountId, int type)
    {
        if (accountId != _currentSelectedAccountId) return;
        this.BeginInvoke((Action)(() => RenderHanhTrangTab(accountId)));
    }

    private void RenderHanhTrangTab(int accountId)
    {
        var bagData = Panel.Models.InventoryCacheManager.GetCache(accountId, 0);
        var boxData = Panel.Models.InventoryCacheManager.GetCache(accountId, 1);
        
        if (bagData != null)
        {
            lblHanhTrangCoin.Text = $"- Vàng: {bagData.Gold:#,0}";
            lblHanhTrangGem.Text  = $"- Ngọc xanh: {bagData.Gem:#,0}";
            lblHanhTrangRuby.Text = $"- Hồng ngọc: {bagData.Ruby:#,0}";
            lblHanhTrangSlots.Text = $"- Ô trống hành trang: {bagData.EmptySlots}/{bagData.BagMax}";
        }
        else if (boxData != null)
        {
            lblHanhTrangCoin.Text = $"- Vàng: {boxData.Gold:#,0}";
            lblHanhTrangGem.Text  = $"- Ngọc xanh: {boxData.Gem:#,0}";
            lblHanhTrangRuby.Text = $"- Hồng ngọc: {boxData.Ruby:#,0}";
        }
        
        if (boxData != null)
        {
            lblRuongDoSlots.Text = $"- Ô trống rương: {boxData.EmptySlots}/{boxData.BoxMax}";
        }
    }

    private void OnClientSkhDataReceived(int accountId, int total, string[] names, string[] vals)
    {
        if (accountId != _currentSelectedAccountId) return;
        this.BeginInvoke((Action)(() =>
        {
            upSkhControl.UpdateSkhData(names, vals, total);
        }));
    }

    private Dictionary<int, DateTime> _skhTimes = new Dictionary<int, DateTime>();

    private void OnClientSkhTimeReceived(int accountId, string dtString)
    {
        if (DateTime.TryParseExact(dtString, "yyyy/MM/dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out var dt))
        {
            _skhTimes[accountId] = dt;
            if (accountId == _currentSelectedAccountId)
            {
                this.BeginInvoke((Action)(() =>
                {
                    upSkhControl.StartCountdown(dt);
                }));
            }
        }
    }

    private Dictionary<int, List<(DateTime dt, string type, string msg)>> _gameLogs = new Dictionary<int, List<(DateTime, string, string)>>();

    private void OnGameLogReceived(int accountId, string type, string msg)
    {
        List<(DateTime, string, string)> list;
        lock (_gameLogs)
        {
            if (!_gameLogs.ContainsKey(accountId))
                _gameLogs[accountId] = new List<(DateTime, string, string)>();

            list = _gameLogs[accountId];
        }

        lock (list)
        {
            list.Add((DateTime.Now, type, msg));
            if (list.Count > 100)
                list.RemoveAt(0);
        }

        if (accountId == _currentSelectedAccountId)
        {
            this.BeginInvoke((Action)(() => RenderNhatKiTab(accountId)));
        }
    }

    /// <summary>Nhận thông tin nhân vật từ client mỗi 5s, cập nhật tab Sư phụ.</summary>
    private void OnClientCharInfoReceived(int accountId, string infoText)
    {
        _latestCharInfoText[accountId] = infoText;
        if (accountId != _currentSelectedAccountId) return;
        this.BeginInvoke((Action)(() => RenderSuPhuTab(accountId)));

        // Gửi lệnh lấy inventory nếu tab đang được chọn để chống load ẩn
        if (tabControlAccountInfo.SelectedTab == tabAccountHanhTrang)
        {
            _socketServer.SendCommand(accountId, "GET_INVENTORY|0");
            _socketServer.SendCommand(accountId, "GET_INVENTORY|1");
        }
    }

    private void OnClientCharStatsReceived(int accountId, long gold, long power, bool autoOn)
    {
        if (autoOn)
        {
            if (!_autoAccumulatedTime.ContainsKey(accountId))
            {
                _autoAccumulatedTime[accountId] = TimeSpan.Zero;
                _autoBaseline.Remove(accountId);
            }

            if (_lastCharStatsTime.TryGetValue(accountId, out DateTime lastTime))
            {
                var diff = DateTime.Now - lastTime;
                if (diff.TotalSeconds <= 20)
                {
                    _autoAccumulatedTime[accountId] += diff;
                }
            }
            _lastCharStatsTime[accountId] = DateTime.Now;

            // Đợi nhân vật load xong sức mạnh (> 0) mới ghi nhận mốc bắt đầu tính
            if (!_autoBaseline.ContainsKey(accountId) && power > 0)
                _autoBaseline[accountId] = (gold, power);

            _autoLatest[accountId] = (gold, power);
        }
        else
        {
            _autoAccumulatedTime.Remove(accountId);
            _lastCharStatsTime.Remove(accountId);
            _autoBaseline.Remove(accountId);
            _autoLatest.Remove(accountId);
        }

        if (accountId != _currentSelectedAccountId) return;
        this.BeginInvoke((Action)(() => RenderSuPhuTab(accountId)));
    }

    /// <summary>Nhận thông tin đệ tử từ client mỗi 5s, cập nhật tab Đệ tử.</summary>
    private void OnClientPetInfoReceived(int accountId, string infoText)
    {
        _latestPetInfoText[accountId] = infoText;
        if (accountId != _currentSelectedAccountId) return;
        this.BeginInvoke((Action)(() => RenderDeTuTab(accountId)));
    }

    private void OnClientPetStatsReceived(int accountId, long power, long tiemNang, bool autoOn)
    {
        if (autoOn)
        {
            if (!_autoAccumulatedTimePet.ContainsKey(accountId))
            {
                _autoAccumulatedTimePet[accountId] = TimeSpan.Zero;
                _autoBaselinePet.Remove(accountId);
            }

            if (_lastPetStatsTime.TryGetValue(accountId, out DateTime lastTime))
            {
                var diff = DateTime.Now - lastTime;
                if (diff.TotalSeconds <= 20)
                {
                    _autoAccumulatedTimePet[accountId] += diff;
                }
            }
            _lastPetStatsTime[accountId] = DateTime.Now;

            // Đợi đệ tử load xong sức mạnh (> 0) mới ghi nhận mốc bắt đầu tính
            if (!_autoBaselinePet.ContainsKey(accountId) && power > 0)
                _autoBaselinePet[accountId] = (power, tiemNang);

            _autoLatestPet[accountId] = (power, tiemNang);
        }
        else
        {
            _autoAccumulatedTimePet.Remove(accountId);
            _lastPetStatsTime.Remove(accountId);
            _autoBaselinePet.Remove(accountId);
            _autoLatestPet.Remove(accountId);
        }

        if (accountId != _currentSelectedAccountId) return;
        this.BeginInvoke((Action)(() => RenderDeTuTab(accountId)));
    }

    private void OnClientCharItemsReceived(int accountId, int kilis, int mvbt, int mhbt)
    {
        _latestKilis[accountId] = kilis;
        _latestMvbt[accountId] = mvbt;
        _latestMhbt[accountId] = mhbt;

        var settings = _accountSettingsService.Load(accountId);
        if (settings == null) return;

        bool wasReset = Panel.Helpers.DailyMetricsHelper.CheckAndResetDailyMetrics(
            settings, kilis, mvbt, mhbt, out int farmedKilis, out int farmedMvbt, out int farmedMhbt);
            
        _latestFarmedKilis[accountId] = farmedKilis;
        _latestFarmedMvbt[accountId] = farmedMvbt;
        _latestFarmedMhbt[accountId] = farmedMhbt;

        if (wasReset)
        {
            _accountSettingsService.Save(accountId, settings);
        }

        // update UI if selected
        if (accountId == _currentSelectedAccountId)
        {
            this.mvbtControl.UpdateProgress(farmedMvbt, settings.Mvbt?.TargetCount ?? 99);
            this.mhbtControl.UpdateProgress(farmedMhbt, settings.Mhbt?.TargetCount ?? 99);
        }

        // Send settings to update client status dynamically without unchecking the Master checkbox.
        if (settings.Mvbt != null && settings.Mvbt.Enabled)
        {
            SendMvbtSettingsCommand(accountId, settings.Mvbt);
        }
        if (settings.Mhbt != null && settings.Mhbt.Enabled)
        {
            SendMhbtSettingsCommand(accountId, settings.Mhbt);
        }
    }

    private void MvbtControl_ResetCountRequested(object? sender, EventArgs e)
    {
        if (_currentSelectedAccountId < 0) return;
        int accId = _currentSelectedAccountId;
        var settings = _accountSettingsService.Load(accId);
        int currentKilis = _latestKilis.TryGetValue(accId, out int k) ? k : 0;
        int currentMvbt = _latestMvbt.TryGetValue(accId, out int m) ? m : 0;
        int currentMhbt = _latestMhbt.TryGetValue(accId, out int h) ? h : 0;
        Panel.Helpers.DailyMetricsHelper.ForceReset(settings, currentKilis, currentMvbt, currentMhbt);
        _accountSettingsService.Save(accId, settings);
        
        _latestFarmedMvbt[accId] = 0;
        this.mvbtControl.UpdateProgress(0, settings.Mvbt.TargetCount);
        if (settings.Mvbt != null) SendMvbtSettingsCommand(accId, settings.Mvbt);
    }

    private void MhbtControl_ResetCountRequested(object? sender, EventArgs e)
    {
        if (_currentSelectedAccountId < 0) return;
        int accId = _currentSelectedAccountId;
        var settings = _accountSettingsService.Load(accId);
        int currentKilis = _latestKilis.TryGetValue(accId, out int k) ? k : 0;
        int currentMvbt = _latestMvbt.TryGetValue(accId, out int m) ? m : 0;
        int currentMhbt = _latestMhbt.TryGetValue(accId, out int h) ? h : 0;
        Panel.Helpers.DailyMetricsHelper.ForceReset(settings, currentKilis, currentMvbt, currentMhbt);
        _accountSettingsService.Save(accId, settings);
        
        _latestFarmedMhbt[accId] = 0;
        this.mhbtControl.UpdateProgress(0, settings.Mhbt.TargetCount);
        if (settings.Mhbt != null) SendMhbtSettingsCommand(accId, settings.Mhbt);
    }

    private void RenderSuPhuTab(int accountId)
    {
        string header = $"[Cập nhật lúc {DateTime.Now:dd/MM/yyyy HH:mm:ss}]";
        string infoText = _latestCharInfoText.TryGetValue(accountId, out var t) ? t : GetOfflineCharInfoTemplate();

        // ─── Stats section ────────────────────────────────────────
        int fk = _latestFarmedKilis.TryGetValue(accountId, out var fk_val) ? fk_val : 0;
        int fm = _latestFarmedMvbt.TryGetValue(accountId, out var fm_val) ? fm_val : 0;
        int fh = _latestFarmedMhbt.TryGetValue(accountId, out var fh_val) ? fh_val : 0;
        
        string statsSection = $"\n[Hôm nay]\n- Nhặt Kilis: {fk:#,0}\n- Nhặt MVBT: {fm:#,0}\n- Nhặt MHBT: {fh:#,0}";

        if (_autoAccumulatedTime.TryGetValue(accountId, out TimeSpan uptime))
        {
            string uptimeStr = FormatUptime(uptime);

            long goldEarned = 0, powerEarned = 0;
            double goldPerHour = 0, powerPerHour = 0;
            if (_autoBaseline.TryGetValue(accountId, out var baseline)
                && _autoLatest.TryGetValue(accountId, out var latest))
            {
                goldEarned = latest.gold - baseline.gold;
                powerEarned = latest.power - baseline.power;
                double totalH = uptime.TotalHours;
                if (totalH > 0)
                {
                    goldPerHour = goldEarned / totalH;
                    powerPerHour = powerEarned / totalH;
                }
            }

            statsSection +=
                $"\n[Thời gian úp {uptimeStr}]\n" +
                $"- Vàng: {goldEarned:#,0}\n" +
                $"- Vàng 1h: {(long)goldPerHour:#,0}\n" +
                $"- SM: {powerEarned:#,0}\n" +
                $"- SM 1h: {(long)powerPerHour:#,0}";
        }
        else
        {
            statsSection +=
                "\n[Thời gian úp 0s]\n" +
                "- Vàng: 0\n" +
                "- Vàng 1h: 0\n" +
                "- SM: 0\n" +
                "- SM 1h: 0";
        }

        string final = header + "\n" + infoText + statsSection;
        UpdateRichTextDashboard(txtSuPhuInfo, final);
    }

    private void RenderDeTuTab(int accountId)
    {
        string header = $"[Cập nhật lúc {DateTime.Now:dd/MM/yyyy HH:mm:ss}]";
        string infoText = _latestPetInfoText.TryGetValue(accountId, out var t) ? t : GetOfflinePetInfoTemplate();

        // ─── Stats section ────────────────────────────────────────
        string statsSection;
        if (_autoAccumulatedTimePet.TryGetValue(accountId, out TimeSpan uptime))
        {
            string uptimeStr = FormatUptime(uptime);

            long powerEarned = 0, tnEarned = 0;
            double powerPerHour = 0, tnPerHour = 0;
            if (_autoBaselinePet.TryGetValue(accountId, out var baseline)
                && _autoLatestPet.TryGetValue(accountId, out var latest))
            {
                powerEarned = Math.Max(0, latest.power - baseline.power);
                tnEarned = Math.Max(0, latest.tiemNang - baseline.tiemNang);
                double totalH = uptime.TotalHours;
                if (totalH > 0)
                {
                    powerPerHour = powerEarned / totalH;
                    tnPerHour = tnEarned / totalH;
                }
            }

            statsSection =
                $"\n[Thời gian úp {uptimeStr}]\n" +
                $"- SM lên: {powerEarned:#,0}\n" +
                $"- SM 1h: {(long)powerPerHour:#,0}\n" +
                $"- TN lên: {tnEarned:#,0}\n" +
                $"- TN 1h: {(long)tnPerHour:#,0}";
        }
        else
        {
            statsSection =
                "\n[Thời gian úp 0s]\n" +
                "- SM lên: 0\n" +
                "- SM 1h: 0\n" +
                "- TN lên: 0\n" +
                "- TN 1h: 0";
        }

        string final = header + "\n" + infoText + "\n" + statsSection;
        UpdateRichTextDashboard(txtDeTuInfo, final);
    }

    private void RenderNhatKiTab(int accountId)
    {
        SendMessage(txtNhatKiInfo.Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
        txtNhatKiInfo.SuspendLayout();
        txtNhatKiInfo.Clear();

        if (_gameLogs.TryGetValue(accountId, out var logs) && logs.Count > 0)
        {
            List<(DateTime dt, string type, string msg)> snapshot;
            lock (logs)
            {
                snapshot = logs.ToList();
            }

            foreach (var log in snapshot)
            {
                // In time
                AppendColorText(txtNhatKiInfo, $"[{log.dt:HH:mm:ss dd/MM/yyyy}]\n", Color.Cyan, FontStyle.Regular);

                // In nội dung có màu
                if (log.type == "SKH_ME")
                    AppendColorText(txtNhatKiInfo, log.msg + "\n", Color.Yellow, FontStyle.Bold); // Màu cam vàng
                else if (log.type == "SYSTEM")
                    AppendColorText(txtNhatKiInfo, log.msg + "\n", Color.Red, FontStyle.Bold);    // Bảo trì màu đỏ
                else
                    AppendColorText(txtNhatKiInfo, log.msg + "\n", Color.White, FontStyle.Regular);
            }
        }
        else
        {
            AppendColorText(txtNhatKiInfo, $"[Nhật ký hoạt động]\nChưa có dữ liệu nhật ký nào...", Color.Silver, FontStyle.Italic);
        }
        
        
        // Cuộn xuống cuối
        txtNhatKiInfo.SelectionStart = txtNhatKiInfo.TextLength;
        txtNhatKiInfo.ScrollToCaret();
        txtNhatKiInfo.ResumeLayout();
        
        SendMessage(txtNhatKiInfo.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
        txtNhatKiInfo.Invalidate();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref POINT lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    
    private const int EM_GETSCROLLPOS = 0x0400 + 221;
    private const int EM_SETSCROLLPOS = 0x0400 + 222;
    private const int WM_SETREDRAW = 0x000B;

    // ─── Formatting Dashboard (RichTextBox) ───────────────────────────
    private void UpdateRichTextDashboard(RichTextBox rtb, string fullText)
    {
        // Lưu vị trí cuộn hiện tại
        POINT scrollPos = new POINT();
        SendMessage(rtb.Handle, EM_GETSCROLLPOS, IntPtr.Zero, ref scrollPos);
        
        // Ngăn RichTextBox vẽ lại tạm thời (fix lỗi nháy UI mỗi 5s)
        SendMessage(rtb.Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
        
        rtb.SuspendLayout();
        rtb.Clear();
        
        string[] lines = fullText.Split(new[] { '\n' }, StringSplitOptions.None);
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                rtb.AppendText("\n");
                continue;
            }

            if (line.StartsWith("[")) // Header [Cập nhật lúc...]
            {
                AppendColorText(rtb, line + "\n", Color.FromArgb(100, 116, 139), FontStyle.Italic); // Gray Italic
            }
            else if (line.StartsWith("- ")) // Keyword line like "- HP - MP: 100 - 100"
            {
                int colonIndex = line.IndexOf(':');
                if (colonIndex > 0)
                {
                    string label = line.Substring(0, colonIndex + 1);
                    string val = line.Substring(colonIndex + 1);
                    
                    AppendColorText(rtb, label, Color.FromArgb(71, 85, 105), FontStyle.Bold); // Bold Slate
                    
                    // Định vị màu sắc cho Value
                    Color valColor = Color.FromArgb(15, 23, 42); // Black default
                    if (label.Contains("HP") || label.Contains("MP") || label.Contains("Sức mạnh") || label.Contains("Thể lực")) valColor = Color.FromArgb(2, 132, 199); // Blue
                    if (label.Contains("Vàng") || label.Contains("Ngọc")) valColor = Color.FromArgb(202, 138, 4); // Orange
                    if (label.Contains("Tên") || label.Contains("SM")) valColor = Color.FromArgb(22, 163, 74); // Green

                    AppendColorText(rtb, " " + val.TrimStart() + "\n", valColor, FontStyle.Bold);
                }
                else
                {
                    AppendColorText(rtb, line + "\n", Color.FromArgb(71, 85, 105), FontStyle.Bold);
                }
            }
            else // Indented value line, like "Map / 0 / 0,0"
            {
                AppendColorText(rtb, "  " + line.Trim() + "\n", Color.FromArgb(30, 41, 59), FontStyle.Regular);
            }
        }

        rtb.ResumeLayout();
        
        // Khôi phục vị trí cuộn
        SendMessage(rtb.Handle, EM_SETSCROLLPOS, IntPtr.Zero, ref scrollPos);
        
        // Cho phép vẽ lại UI
        SendMessage(rtb.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
        rtb.Invalidate();
    }

    private void AppendColorText(RichTextBox box, string text, Color color, FontStyle style)
    {
        box.SelectionStart = box.TextLength;
        box.SelectionLength = 0;
        box.SelectionColor = color;
        box.SelectionFont = new Font("Segoe UI", 9f, style);
        box.AppendText(text);
        box.SelectionColor = box.ForeColor;
    }

    /// <summary>Format thời gian: giây → phút → giờ → ngày.</summary>
    private static string FormatUptime(TimeSpan ts)
    {
        if (ts.TotalDays >= 1)
            return $"{(int)ts.TotalDays}d {ts.Hours}h{ts.Minutes}'";
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h{ts.Minutes}'";
        if (ts.TotalMinutes >= 1)
            return $"{(int)ts.TotalMinutes}'{ts.Seconds}s";
        return $"{ts.Seconds}s";
    }

    private static string GetOfflineCharInfoTemplate() =>
        "- Tên Nhân Vật: \n" +
        "- Map / Khu / Tọa Độ:\n" +
        "  Map / 0 / 0,0\n" +
        "- HP: 100 / 100\n" +
        "- MP: 100 / 100\n" +
        "- Sức mạnh: 2,000\n" +
        "- Vàng / Ngọc / Hồng Ngọc:\n" +
        "  0 / 0 / 0\n" +
        "- Hành Trang: 0/20\n" +
        "- Rương: 0/20\n" +
        "- Thể lực: 10,000/10,000, 100%\n" +
        "- Tổng Kilis: 0\n" +
        "- Tổng mảnh vỡ BT: 0\n" +
        "- Tổng mảnh hồn BT: 0";

    private static string GetOfflinePetInfoTemplate() =>
        "- Tên đệ tử: Chưa có đệ tử\n" +
        "- Chế độ úp: \n" +
        "---\n" +
        "- Sức mạnh: 0\n" +
        "- Tiềm năng: 0\n" +
        "- Thể lực: 0\n" +
        "---\n" +
        "- HP: 0 / 0\n" +
        "- MP: 0 / 0\n" +
        "- Sức đánh: 0\n" +
        "- Giáp: 0\n" +
        "---\n" +
        "- Đang chờ dữ liệu kỹ năng...";

    /// <summary>Nhận báo cáo STATUS từ client: cập nhật dòng tương ứng trong grid.</summary>
    private void OnClientStatusReceived(int accountId, string charName, string extra)
    {
        string calculatedStatus = "1. ONLINE";
        if (extra.Contains("Chờ ở") || extra.Contains("Đang kết nối") || extra.Contains("SplashScr") || extra.Contains("LoginScr") || extra.Contains("ServerListScreen") || extra.Contains("Tạo nhân vật"))
        {
            calculatedStatus = "2. ĐANG VÀO GAME";
        }

        // Lưu vào DB
        _accountRepo.UpdateStatus(accountId, calculatedStatus, charName);

        // Tự động tắt Render nếu đang bật "Tự ẩn client"
        if (chkAutoHideClient.Checked)
        {
            _socketServer.SendCommand(accountId, "SET_RENDER|0");
        }

        // Sau khi client vào game và gửi STATUS lần đầu, push lại toàn bộ settings đã lưu.
        // Việc này giúp tránh rơi lệnh nếu command được gửi quá sớm ở pha HELLO.
        bool shouldSync = false;
        lock (_settingsSyncLock)
        {
            if (!_settingsSyncedAfterStatus.Contains(accountId))
            {
                _settingsSyncedAfterStatus.Add(accountId);
                shouldSync = true;
            }
        }

        if (shouldSync)
        {
            SendAllSavedSettingsToClient(accountId);
        }

        // Cập nhật UI thread-safe
        this.BeginInvoke((Action)(() =>
        {
            int rowIndex = FindRowByAccountId(accountId);
            if (rowIndex < 0) return;

            var row = dgvAccounts.Rows[rowIndex];
            row.Cells[colCharacter.Index].Value = charName;
            row.Cells[colStatus.Index].Value = calculatedStatus;
            row.Cells[colDataInGame.Index].Value = extra; // mapId|zoneId hoặc thông tin khác

            // Cập nhật lại nút nếu dòng này đang được chọn
            if (dgvAccounts.CurrentRow != null && dgvAccounts.CurrentRow.Index == rowIndex)
                UpdateButtonState(row);
        }));
    }

    /// <summary>Client kết nối hoặc ngắt kết nối.</summary>
    private void OnClientConnectionChanged(int accountId, bool isConnected)
    {
        string status = isConnected ? "3. LOGIN" : "0. OFFLINE";
        if (!isConnected)
        {
            _accountRepo.UpdateStatus(accountId, status, "---");
            MarkAttendanceOffline(accountId);
            _autoAccumulatedTime.Remove(accountId);
            _lastCharStatsTime.Remove(accountId);
            _autoBaseline.Remove(accountId);
            _autoLatest.Remove(accountId);
            _autoAccumulatedTimePet.Remove(accountId);
            _autoLatestPet.Remove(accountId);
            _skhTimes.Remove(accountId);
            _gameLogs.Remove(accountId);
            if (_dailyQuestRuntimeByAccount.TryGetValue(accountId, out var runtime))
            {
                runtime.IsRunning = false;
                runtime.RunMode = string.Empty;
                runtime.StateText = runtime.FinishedToday ? "Đã xong hôm nay" : "Đang tắt";
                _dailyQuestRuntimeByAccount[accountId] = runtime;
            }
            if (accountId == _currentSelectedAccountId)
            {
                this.BeginInvoke((Action)(() => { 
                    upSkhControl.StopCountdown(); 
                    ApplyDailyQuestRuntimeToControl(accountId);
                    RenderNhatKiTab(accountId);
                }));
            }
        }

        if (isConnected)
        {
            lock (_settingsSyncLock)
            {
                _settingsSyncedAfterStatus.Remove(accountId);
            }

            // Gửi ngay một lượt ở pha HELLO để giảm thời gian chờ.
            // Nếu rơi lệnh sớm, luồng OnClientStatusReceived sẽ tự push lại sau.
            SendAllSavedSettingsToClient(accountId);
        }
        else
        {
            lock (_settingsSyncLock)
            {
                _settingsSyncedAfterStatus.Remove(accountId);
            }
        }

        this.BeginInvoke((Action)(() =>
        {
            int rowIndex = FindRowByAccountId(accountId);
            if (rowIndex < 0) return;

            var row = dgvAccounts.Rows[rowIndex];
            row.Cells[colStatus.Index].Value = status;
            
            string oldVal = row.Cells[colDataInGame.Index].Value?.ToString() ?? "";
            if (isConnected)
            {
                if (string.IsNullOrEmpty(oldVal) || oldVal == "Mất kết nối")
                    row.Cells[colDataInGame.Index].Value = "Đang tải dữ liệu...";
                else if (oldVal.StartsWith("[OFF] "))
                    row.Cells[colDataInGame.Index].Value = oldVal.Replace("[OFF] ", "[LOGIN] ");
                else if (!oldVal.StartsWith("[LOGIN] "))
                    row.Cells[colDataInGame.Index].Value = "[LOGIN] " + oldVal;
            }
            else
            {
                if (string.IsNullOrEmpty(oldVal) || oldVal == "Đang tải dữ liệu...")
                    row.Cells[colDataInGame.Index].Value = "Mất kết nối";
                else
                {
                    string baseVal = oldVal.Replace("[LOGIN] ", "").Replace("[OFF] ", "");
                    row.Cells[colDataInGame.Index].Value = "[OFF] " + baseVal;
                }
            }

            // Cập nhật lại nút nếu dòng này đang được chọn
            if (dgvAccounts.CurrentRow != null && dgvAccounts.CurrentRow.Index == rowIndex)
                UpdateButtonState(row);

            if (accountId == _currentSelectedAccountId)
            {
                RenderSuPhuTab(accountId);
                RenderDeTuTab(accountId);
                ApplyDailyQuestRuntimeToControl(accountId);
                _attendanceControl.ApplyRuntime(_currentAttendanceSettings);
            }
        }));
    }

    private DailyQuestRuntimeStatus GetDailyQuestRuntimeStatus(int accountId)
    {
        if (_dailyQuestRuntimeByAccount.TryGetValue(accountId, out var runtime))
        {
            return runtime;
        }

        var settings = _accountSettingsService.Load(accountId);
        runtime = new DailyQuestRuntimeStatus
        {
            IsRunning = false,
            RunMode = settings.Daily.DailyQuestLastRunMode ?? string.Empty,
            StateText = settings.Daily.DailyQuestFinishedToday ? "Đã xong hôm nay" : "Đang tắt",
            CompletedToday = settings.Daily.DailyQuestCompletedCount,
            CanceledToday = settings.Daily.DailyQuestCanceledCount,
            FinishedToday = settings.Daily.DailyQuestFinishedToday
        };
        _dailyQuestRuntimeByAccount[accountId] = runtime;
        return runtime;
    }

    private void ApplyDailyQuestRuntimeToControl(int accountId)
    {
        if (_dailyQuestControl == null)
        {
            return;
        }

        _dailyQuestControl.ApplyRuntime(GetDailyQuestRuntimeStatus(accountId));
    }

    private static string EncodeCommandText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
    }

    private void OnSysPosReceived(int accountId, int mapId, int zoneId, int x, int y)
    {
        this.BeginInvoke((Action)(() =>
        {
            if (_currentSelectedAccountId != accountId) return;

            PositionRequestSource source = _pendingPositionRequestSource;
            _pendingPositionRequestSource = PositionRequestSource.None;

            switch (source)
            {
                case PositionRequestSource.Pet:
                    petControl.numTargetMapId.Value = Math.Clamp(mapId, petControl.numTargetMapId.Minimum, petControl.numTargetMapId.Maximum);
                    petControl.numTargetZoneId.Value = Math.Clamp(zoneId, petControl.numTargetZoneId.Minimum, petControl.numTargetZoneId.Maximum);
                    petControl.numTargetX.Value = Math.Clamp(x, petControl.numTargetX.Minimum, petControl.numTargetX.Maximum);
                    petControl.numTargetY.Value = Math.Clamp(y, petControl.numTargetY.Minimum, petControl.numTargetY.Maximum);
                    break;
                case PositionRequestSource.ReducePower:
                    buffNamekControl.SetRpMapFromPos(mapId, zoneId);
                    buffNamekControl.SetRpPosition(x, y);
                    break;
                case PositionRequestSource.BuffNamek:
                case PositionRequestSource.None:
                default:
                    // fallback giu hanh vi mac dinh cu
                    buffNamekControl.SetMapFromPos(mapId, zoneId);
                    buffNamekControl.SetPosition(x, y);
                    break;
            }
        }));
    }

    private void OnBuffNamekStateReceived(
        int accountId,
        int mapId,
        int zoneId,
        int x,
        int y,
        int skillId,
        long cdTotalMs,
        long cdRemainMs,
        long lastCastMs,
        string state,
        string targetName)
    {
        // Khong con override Data In Game o panel.
        // Data in game se di theo STATUS/ActivityState nhu cac auto khac.
    }

    // ─── Load accounts ────────────────────────────────────────────────────

    private void LoadAccounts()
    {
        this.dgvAccounts.Rows.Clear();
        _rowToAccountId.Clear();
        var accounts = _accountRepo.GetAllAccounts();
        foreach (var acc in accounts)
        {
            AddAccountRow(acc);
        }
    }

    private void SelectFirstAccountAndLoadSettings()
    {
        if (dgvAccounts.Rows.Count == 0)
            return;

        int firstRowIndex = 0;
        if (dgvAccounts.Rows[firstRowIndex].Cells.Count == 0)
            return;

        dgvAccounts.CurrentCell = dgvAccounts.Rows[firstRowIndex].Cells[colAccount.Index];
        UpdateButtonState(dgvAccounts.Rows[firstRowIndex]);
        LoadAccountSettings(dgvAccounts.Rows[firstRowIndex]);
    }

    private void AddAccountRow(Account acc)
    {
        var settings = _accountSettingsService.Load(acc.Id);
        int rowIndex = this.dgvAccounts.Rows.Add(
            acc.IsSelected,     // colSelect
            "",                 // colSTT
            acc.CharacterName,  // colCharacter
            "",                 // colDataInGame
            acc.Status,         // colStatus
            acc.Server,         // colServer
            settings.General.TypeAccount, // colTypeAccount
            FormatAccountForDisplay(acc.Username) // colAccount
        );
        this.dgvAccounts.Rows[rowIndex].Tag = acc.Id;
        _rowToAccountId[rowIndex] = acc.Id;
    }

    // ─── Nút Thêm ─────────────────────────────────────────────────────────

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        using var addForm = new AddAccountForm();
        if (addForm.ShowDialog() == DialogResult.OK)
        {
            foreach (var acc in addForm.AddedAccounts)
            {
                var newAccount = new Account
                {
                    Username = acc.Username,
                    Password = acc.Password,
                    Server = acc.Server,
                    Status = "0. OFFLINE",
                    CharacterName = "---"
                };

                newAccount.Id = _accountRepo.AddAccount(newAccount);

                var acSet = _accountSettingsService.Load(newAccount.Id);
                acSet.General.TypeAccount = acc.TypeAccount;
                _accountSettingsService.Save(newAccount.Id, acSet);

                AddAccountRow(newAccount);
            }
        }
    }

    // ─── Nút Sửa ─────────────────────────────────────────────────────────

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dgvAccounts.CurrentRow == null) return;
        int rowIndex = dgvAccounts.CurrentRow.Index;
        int? accountId = dgvAccounts.Rows[rowIndex].Tag as int?;

        if (!accountId.HasValue) return;

        var acc = _accountRepo.GetAllAccounts().FirstOrDefault(a => a.Id == accountId.Value);
        if (acc == null) return;

        var edit_acSet = _accountSettingsService.Load(acc.Id);

        using var editForm = new AddAccountForm();
        editForm.Text = "Sửa Tài Khoản"; 
        editForm.Username = acc.Username;
        editForm.Password = acc.Password;
        editForm.ServerName = acc.Server;
        editForm.TypeAccountVal = edit_acSet.General.TypeAccount;
        
        if (editForm.ShowDialog() == DialogResult.OK)
        {
            acc.Username = editForm.Username;
            acc.Password = editForm.Password;
            acc.Server = editForm.ServerName;
            
            _accountRepo.UpdateAccount(acc);

            edit_acSet.General.TypeAccount = editForm.TypeAccountVal;
            _accountSettingsService.Save(acc.Id, edit_acSet);

            // Cập nhật UI
            var row = dgvAccounts.Rows[rowIndex];
            row.Cells[colAccount.Index].Value = FormatAccountForDisplay(acc.Username);
            row.Cells[colServer.Index].Value = acc.Server;
            row.Cells[colTypeAccount.Index].Value = editForm.TypeAccountVal;
        }
    }

    // ─── Nút Xóa ─────────────────────────────────────────────────────────

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dgvAccounts.CurrentRow == null) return;
        int rowIndex = dgvAccounts.CurrentRow.Index;

        string status = dgvAccounts.Rows[rowIndex].Cells[colStatus.Index].Value?.ToString() ?? "";
        if (!status.Contains("OFFLINE"))
        {
            MessageBox.Show("Tài khoản này đang chạy game. Vui lòng tắt game trước khi xóa", "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int? accountId = dgvAccounts.Rows[rowIndex].Tag as int?;

        if (accountId.HasValue)
            _accountRepo.DeleteAccount(accountId.Value);

        dgvAccounts.Rows.RemoveAt(rowIndex);
        RebuildRowMap();
        UpdateSelectAllButtonState();
    }

    private void BtnDeleteSelected_Click(object? sender, EventArgs e)
    {
        var rowsToDelete = new List<DataGridViewRow>();
        foreach (DataGridViewRow row in dgvAccounts.Rows)
        {
            if (row.Cells[colSelect.Index].Value is bool isSelected && isSelected)
            {
                rowsToDelete.Add(row);
            }
        }

        if (rowsToDelete.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn ít nhất một tài khoản để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa {rowsToDelete.Count} tài khoản đã chọn?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        int deletedCount = 0;
        int skipCount = 0;

        // Thu thập danh sách ID và Row để xóa (tránh lỗi khi remove trực tiếp trong lúc duyệt dgvAccounts.Rows)
        foreach (var row in rowsToDelete)
        {
            string status = row.Cells[colStatus.Index].Value?.ToString() ?? "";
            if (!status.Contains("OFFLINE"))
            {
                skipCount++;
                continue;
            }

            if (row.Tag is int accountId)
            {
                _accountRepo.DeleteAccount(accountId);
                dgvAccounts.Rows.Remove(row);
                deletedCount++;
            }
        }

        if (deletedCount > 0)
        {
            RebuildRowMap();
            UpdateSelectAllButtonState();
        }

        string msg = $"Đã xóa {deletedCount} tài khoản.";
        if (skipCount > 0) msg += $"\nBỏ qua {skipCount} tài khoản đang online.";
        MessageBox.Show(msg, "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ─── Nút Bật/Tắt Game ────────────────────────────────────────────────

    private void BtnToggleGame_Click(object? sender, EventArgs e)
    {
        if (dgvAccounts.CurrentRow == null) return;
        
        var row = dgvAccounts.CurrentRow;
        string currentStatus = row.Cells[colStatus.Index].Value?.ToString() ?? "";
        
        if (row.Tag is int accountId)
        {
            var acSet = _accountSettingsService.Load(accountId);
            var sc = acSet.Schedule ?? new ScheduleSettings();

            if (sc.IsScheduleEnabled)
            {
                // Nếu game đang bật => Vẫn cho tắt tay để gỡ lỗi (lag, kẹt game)
                if (!currentStatus.Contains("OFFLINE"))
                {
                    KillAccountRow(row);
                }
                else
                {
                    // Game đang tắt. Nút Bật sẽ bị vô hiệu hóa ngầm (không báo pop-up) nếu ngoài khung giờ.
                    // Nếu đang trong khung giờ, cho phép kích tay.
                    DateTime now = TimeHelper.GetRealTime();
                    if (IsTimeInSchedule(now, sc.GetStartTime(), sc.GetEndTime()))
                    {
                        LaunchAccountRow(row.Index);
                    }
                }
                return;
            }

            if (!currentStatus.Contains("OFFLINE"))
            {
                KillAccountRow(row);
            }
            else
            {
                LaunchAccountRow(row.Index);
            }
        }
    }

    private void BtnCloseAll_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show("Bạn có chắc chắn muốn tắt TẤT CẢ game đang chạy không?", "Xác nhận", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
        if (result == DialogResult.OK)
        {
            foreach (DataGridViewRow row in dgvAccounts.Rows)
            {
                string status = row.Cells[colStatus.Index].Value?.ToString() ?? "";
                if (!status.Contains("OFFLINE"))
                {
                    KillAccountRow(row);
                }
            }
        }
    }

    private void BtnHideAll_Click(object? sender, EventArgs e)
    {
        foreach (DataGridViewRow row in dgvAccounts.Rows)
        {
            string status = row.Cells[colStatus.Index].Value?.ToString() ?? "";
            if (!status.Contains("OFFLINE"))
            {
                if (row.Tag is int accountId)
                {
                    int pid = GetProcessIdForAccount(accountId);
                    if (pid > 0)
                    {
                        GameLauncher.HideGame(pid);
                        _socketServer.SendCommand(accountId, "SET_RENDER|0");
                    }
                }
            }
        }
    }

    // ─── Nút Bật Auto / Tắt Auto (tài khoản đang chọn) ──────────────────

    private void UiUpdateTimer_Tick(object? sender, EventArgs e)
    {
        int total = dgvAccounts.Rows.Count;
        int on = 0;
        int log = 0;
        
        foreach (System.Windows.Forms.DataGridViewRow row in dgvAccounts.Rows)
        {
            string status = row.Cells[colStatus.Index].Value?.ToString() ?? "";
            if (status.Contains("ONLINE")) on++;
            else if (status.Contains("KẾT NỐI") || status.Contains("LOGIN") || status.Contains("VÀO GAME")) log++;
        }

        lblAccountStats.Text = $"On/Log/Total: {on} / {log} / {total}";
        lblSystemTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

        var (totalRam, usedRam, ramPerc) = Panel.Helpers.SystemResourceHelper.GetRamUsage();
        double cpuPerc = Panel.Helpers.SystemResourceHelper.GetCpuUsage();

        lblSystemStats.Text = $"RAM: {ramPerc:0}% - CPU: {cpuPerc:0.0}%";
    }

    private void BtnMoveUp_Click(object? sender, EventArgs e)
    {
        if (dgvAccounts.CurrentRow == null) return;
        int index = dgvAccounts.CurrentRow.Index;
        if (index <= 0) return;

        SwapRows(index, index - 1);
    }

    private void BtnMoveDown_Click(object? sender, EventArgs e)
    {
        if (dgvAccounts.CurrentRow == null) return;
        int index = dgvAccounts.CurrentRow.Index;
        if (index >= dgvAccounts.Rows.Count - 1) return;

        SwapRows(index, index + 1);
    }

    private void SwapRows(int index1, int index2)
    {
        // 1. Swap data in repository
        if (dgvAccounts.Rows[index1].Tag is int id1 && dgvAccounts.Rows[index2].Tag is int id2)
        {
            var acc1 = _accountRepo.GetAllAccounts().FirstOrDefault(a => a.Id == id1);
            var acc2 = _accountRepo.GetAllAccounts().FirstOrDefault(a => a.Id == id2);
            if (acc1 != null && acc2 != null)
            {
                // Just swap them visually in UI for now (if you have an Order column in DB, update it here)
            }
        }

        // 2. Tắt Databinding/Event ngầm
        dgvAccounts.ClearSelection();

        // 3. Clone and remove
        System.Windows.Forms.DataGridViewRow row1 = dgvAccounts.Rows[index1];
        dgvAccounts.Rows.RemoveAt(index1);
        dgvAccounts.Rows.Insert(index2, row1);

        // 4. Update internal Map
        RebuildRowMap();

        // 5. Re-select
        dgvAccounts.Rows[index2].Selected = true;
        dgvAccounts.CurrentCell = dgvAccounts.Rows[index2].Cells[colAccount.Index];
        UpdateButtonState(dgvAccounts.Rows[index2]);
    }

    private void BtnCleanRam_Click(object? sender, EventArgs e)
    {
        Panel.Helpers.SystemResourceHelper.CleanOsMemory();
    }

    /// <summary>Đọc config và khởi động/dừng timer Auto Clean RAM theo thiết lập người dùng.</summary>
    private void ApplyAutoCleanRamConfig()
    {
        var config = ConfigManager.Load();
        _autoCleanRamTimer.Stop();
        if (config.AutoCleanRam && config.AutoCleanRamIntervalMinutes >= 1)
        {
            _autoCleanRamTimer.Interval = config.AutoCleanRamIntervalMinutes * 60 * 1000;
            _autoCleanRamTimer.Start();
        }
    }

    // ─── Nút Bật Auto / Tắt Auto (tài khoản đang chọn) ──────────────────

    private void BtnBatAuto_Click(object? sender, EventArgs e)
    {
        if (_currentSelectedAccountId <= 0) return;
        _socketServer.SendCommand(_currentSelectedAccountId, "GLOBAL_AUTO_ON");
        // Bắt đầu tracking uptime, xóa baseline cũ để ghi lại từ đầu
        _autoAccumulatedTime[_currentSelectedAccountId] = TimeSpan.Zero;
        _lastCharStatsTime.Remove(_currentSelectedAccountId);
        _autoBaseline.Remove(_currentSelectedAccountId);
        _autoLatest.Remove(_currentSelectedAccountId);
        _autoAccumulatedTimePet[_currentSelectedAccountId] = TimeSpan.Zero;
        _lastPetStatsTime.Remove(_currentSelectedAccountId);
        _autoBaselinePet.Remove(_currentSelectedAccountId);
        _autoLatestPet.Remove(_currentSelectedAccountId);
        RenderSuPhuTab(_currentSelectedAccountId);
        RenderDeTuTab(_currentSelectedAccountId);
    }

    private void BtnDungAuto_Click(object? sender, EventArgs e)
    {
        if (_currentSelectedAccountId <= 0) return;
        _socketServer.SendCommand(_currentSelectedAccountId, "GLOBAL_AUTO_OFF");
        // Dừng tracking
        _autoAccumulatedTime.Remove(_currentSelectedAccountId);
        _lastCharStatsTime.Remove(_currentSelectedAccountId);
        _autoBaseline.Remove(_currentSelectedAccountId);
        _autoLatest.Remove(_currentSelectedAccountId);
        RenderSuPhuTab(_currentSelectedAccountId);
    }

    private void BtnBatAutoTatCaChon_Click(object? sender, EventArgs e)
    {
        foreach (DataGridViewRow row in dgvAccounts.Rows)
        {
            bool isChecked = row.Cells[colSelect.Index].Value is bool b && b;
            if (!isChecked) continue;
            if (row.Tag is not int accountId) continue;
            
            _socketServer.SendCommand(accountId, "GLOBAL_AUTO_ON");
            
            _autoAccumulatedTime[accountId] = TimeSpan.Zero;
            _lastCharStatsTime.Remove(accountId);
            _autoBaseline.Remove(accountId);
            _autoLatest.Remove(accountId);
            
            if (accountId == _currentSelectedAccountId)
            {
                RenderSuPhuTab(accountId);
            }
        }
    }

    private void BtnDungAutoTatCaChon_Click(object? sender, EventArgs e)
    {
        foreach (DataGridViewRow row in dgvAccounts.Rows)
        {
            bool isChecked = row.Cells[colSelect.Index].Value is bool b && b;
            if (!isChecked) continue;
            if (row.Tag is not int accountId) continue;
            
            _socketServer.SendCommand(accountId, "GLOBAL_AUTO_OFF");
            
            _autoAccumulatedTime.Remove(accountId);
            _lastCharStatsTime.Remove(accountId);
            _autoBaseline.Remove(accountId);
            _autoLatest.Remove(accountId);
            
            if (accountId == _currentSelectedAccountId)
            {
                RenderSuPhuTab(accountId);
            }
        }
    }


    private void DgvAccounts_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvAccounts.CurrentRow != null)
        {
            UpdateButtonState(dgvAccounts.CurrentRow);
            LoadAccountSettings(dgvAccounts.CurrentRow);
        }
    }

    private void LoadAccountSettings(DataGridViewRow row)
    {
        if (row.Tag is not int accountId) return;

        _currentSelectedAccountId = accountId;
        RenderSuPhuTab(accountId);
        RenderDeTuTab(accountId);
        RenderNhatKiTab(accountId);

        // Bỏ qua event trigger khi đang set data UI
        _isBindingData = true;

        _currentSettings = _accountSettingsService.Load(accountId);
        _currentGeneralSettings = _currentSettings.General ?? new GeneralSettings();
        _currentTrainSettings = _currentSettings.Train ?? new TrainFeatureSettings();
        _currentAutoUpZinSettings = _currentSettings.AutoUpZin ?? new AutoUpZinSettings();
        _currentUpZin700kSettings = _currentSettings.UpZin700k ?? new AutoUpZinTo700kSettings();
        _currentMvbtSettings = _currentSettings.Mvbt ?? new MvbtFeatureSettings();
        _currentMhbtSettings = _currentSettings.Mhbt ?? new MvbtFeatureSettings();
        _currentKilisSettings = _currentSettings.Kilis ?? new KilisFeatureSettings();
        _currentBossVegetaCitySettings = _currentSettings.BossVegetaCity ?? new BossVegetaCityFeatureSettings();
        _currentItemSettings = _currentSettings.Item ?? new ItemSettings();
        _currentDauThanSettings = _currentSettings.DauThan ?? new DauThanSettings();
        _currentBuffNamekSettings = _currentSettings.BuffNamek ?? new BuffNamekFeatureSettings();
        _currentReducePowerSettings = _currentSettings.ReducePower ?? new ReducePowerFeatureSettings();
        _currentBossSettings = _currentSettings.Boss ?? new BossFeatureSettings();
        _currentDailyQuestSettings = _currentSettings.DailyQuest ?? new DailyQuestFeatureSettings();
        _currentAttendanceSettings = _currentSettings.Attendance ?? new AttendanceFeatureSettings();
        _currentAutoAmuletSettings = _currentSettings.AutoAmulet ?? new AutoAmuletSettings();
        _currentScheduleSettings = _currentSettings.Schedule ?? new ScheduleSettings();


        chkEatChicken.Checked = _currentGeneralSettings.EatChicken;
        chkUseTdltXmap.Checked = _currentGeneralSettings.UseTdltXmap;
        int actionOnDeathIdx = (_currentGeneralSettings.ActionOnDeath >= 0 && _currentGeneralSettings.ActionOnDeath < cboActionOnDeath.Items.Count)
            ? _currentGeneralSettings.ActionOnDeath : 0;
        cboActionOnDeath.SelectedIndex = actionOnDeathIdx;
        chkUseProxy.Checked = _currentGeneralSettings.UseProxy;
        cboProxyType.SelectedIndex = (_currentGeneralSettings.ProxyType >= 0 && _currentGeneralSettings.ProxyType < cboProxyType.Items.Count)
            ? _currentGeneralSettings.ProxyType : 0;
        txtProxyAddress.Text = _currentGeneralSettings.ProxyAddress ?? "";
        nudFilterTypeAccount.Value = Math.Max(0, Math.Min(_currentGeneralSettings.TypeAccount, (int)nudFilterTypeAccount.Maximum));
        
        scheduleControl.ApplySettings(_currentScheduleSettings);
        lblProxyStatus.Text = "";
        lblProxyStatus.ForeColor = System.Drawing.Color.Gray;
        ApplyProxyUiState();

        
        chkTrainEnable.Checked = _currentTrainSettings.Enabled;
        
        cboTrainMapId.SelectedItem = null;
        foreach (MapTemplate m in cboTrainMapId.Items)
        {
            if (m.Id == _currentTrainSettings.MapId)
            {
                cboTrainMapId.SelectedItem = m;
                _lastValidMapTemplate = m;
                break;
            }
        }

        chkTrainZoneRequire.Checked = _currentTrainSettings.RequireZone;
        txtTrainZone.Text = _currentTrainSettings.ZoneId.ToString();
        chkUseTDLT.Checked = _currentTrainSettings.UseTDLT;
        chkOnlyUsePunch.Checked = _currentTrainSettings.OnlyUsePunch;
        chkFreezePunchSkillCd.Checked = _currentTrainSettings.FreezePunchSkillCd;
        chkAvoidSuperMob.Checked = _currentTrainSettings.AvoidSuperMob;

        if (chkTrainEarthDragon != null) {
            chkTrainEarthDragon.Checked = _currentTrainSettings.SkillEarthDragon;
            chkTrainEarthKame.Checked = _currentTrainSettings.SkillEarthKame;
            chkTrainEarthTdhs.Checked = _currentTrainSettings.SkillEarthTdhs;
            chkTrainEarthThoiMien.Checked = _currentTrainSettings.SkillEarthThoiMien;
            chkTrainEarthDctt.Checked = _currentTrainSettings.SkillEarthDctt;
            chkTrainEarthKhien.Checked = _currentTrainSettings.SkillEarthKhien;
            chkTrainEarthKaioken.Checked = _currentTrainSettings.SkillEarthKaioken;

            chkTrainNamekLienHoan.Checked = _currentTrainSettings.SkillNamekLienHoan;
            chkTrainNamekDemon.Checked = _currentTrainSettings.SkillNamekDemon;
            chkTrainNamekMakan.Checked = _currentTrainSettings.SkillNamekMakan;
            chkTrainNamekDeTrung.Checked = _currentTrainSettings.SkillNamekDeTrung;
            chkTrainNamekKhien.Checked = _currentTrainSettings.SkillNamekKhien;

            chkTrainXaydaGalick.Checked = _currentTrainSettings.SkillSaiyanGalick;
            chkTrainXaydaAntomic.Checked = _currentTrainSettings.SkillSaiyanAntomic;
            chkTrainXaydaBienHinh.Checked = _currentTrainSettings.SkillSaiyanBienHinh;
            chkTrainXaydaTtNl.Checked = _currentTrainSettings.SkillSaiyanTtNl;
            chkTrainXaydaKhien.Checked = _currentTrainSettings.SkillSaiyanKhien;
        }

        if (chkUseShieldUnderHpTrain != null)
        {
            chkUseShieldUnderHpTrain.Checked = _currentTrainSettings.UseShieldUnderHp;
            nudShieldHpPercentTrain.Value = Math.Max(0, Math.Min(_currentTrainSettings.ShieldHpPercent, 100));
        }

        chkChangeLowPlayerZoneIfNoMob.Checked = _currentTrainSettings.ChangeLowPlayerZoneIfNoMob;
        cboMobTargetType.SelectedIndex = (_currentTrainSettings.MobTargetType >= 0 && _currentTrainSettings.MobTargetType < cboMobTargetType.Items.Count) ? _currentTrainSettings.MobTargetType : 0;
        txtMobIds.Text = _currentTrainSettings.MobIds ?? "";
        if (chkCheckLagMob != null) chkCheckLagMob.Checked = _currentTrainSettings.CheckLagMob;
        int armorIdx = _currentTrainSettings.TrainingArmorMode >= 0 && _currentTrainSettings.TrainingArmorMode < cboTrainingArmorMode.Items.Count
            ? _currentTrainSettings.TrainingArmorMode : 0;
        cboTrainingArmorMode.SelectedIndex = armorIdx;

        if (chkAutoUpZin != null)
        {
            chkAutoUpZin.Checked = _currentAutoUpZinSettings.Enabled;
            txtAutoUpZinPrefix.Text = _currentAutoUpZinSettings.NamePrefix ?? string.Empty;
            
            if (_currentAutoUpZinSettings.TargetClass >= 0 && _currentAutoUpZinSettings.TargetClass <= 2)
                cmbAutoUpZinClass.SelectedIndex = _currentAutoUpZinSettings.TargetClass;
            else
                cmbAutoUpZinClass.SelectedIndex = 3;

            UpdateAutoUpZinPrefixUi();
        }

        if (chkUpZin700k != null)
        {
            chkUpZin700k.Checked = _currentUpZin700kSettings.Enabled;
            txtUpZin700kPrefix.Text = _currentUpZin700kSettings.NamePrefix ?? string.Empty;

            if (_currentUpZin700kSettings.TargetClass >= 0 && _currentUpZin700kSettings.TargetClass <= 2)
                cmbUpZin700kClass.SelectedIndex = _currentUpZin700kSettings.TargetClass;
            else
                cmbUpZin700kClass.SelectedIndex = 3;

            UpdateUpZin700kPrefixUi();
        }

        if (nudAstarStep != null) nudAstarStep.Value = _currentTrainSettings.AstarStepSize >= 1 ? Math.Min(_currentTrainSettings.AstarStepSize, 3) : 3;
        if (nudAstarDelay != null) nudAstarDelay.Value = _currentTrainSettings.AstarDelay >= 25 ? Math.Min(_currentTrainSettings.AstarDelay, 100) : 30;

        if (chkAttackHpAbove != null)
        {
            chkAttackHpAbove.Checked = _currentTrainSettings.AttackHpAbove;
            nudAttackHpAboveValue.Value = _currentTrainSettings.AttackHpAboveValue;
            chkAttackHpBelow.Checked = _currentTrainSettings.AttackHpBelow;
            nudAttackHpBelowValue.Value = _currentTrainSettings.AttackHpBelowValue;
            chkRotateZone.Checked = _currentTrainSettings.RotateZone;
            txtRotateZoneList.Text = _currentTrainSettings.RotateZoneList ?? "";
            chkAutoBuyThoiVang.Checked = _currentTrainSettings.AutoBuyThoiVang;

            // KS Vang
            chkOptimizeKsVang.Checked = _currentTrainSettings.OptimizeKsVang;
            if (rdoKsVangZoneLeast != null) 
            {
                rdoKsVangZoneLeast.Checked = _currentTrainSettings.KsVangAutoZoneMode == 0;
                rdoKsVangZoneMost.Checked = _currentTrainSettings.KsVangAutoZoneMode == 1;
                rdoKsVangTriggerMob.Checked = _currentTrainSettings.KsVangAutoZoneTrigger == 0;
                rdoKsVangTriggerTime.Checked = _currentTrainSettings.KsVangAutoZoneTrigger == 1;
                nudKsVangTimeMin.Value = Math.Max(nudKsVangTimeMin.Minimum, Math.Min(_currentTrainSettings.KsVangAutoZoneTimeMin, nudKsVangTimeMin.Maximum));
                chkKsVangFilterPlayer.Checked = _currentTrainSettings.KsVangFilterPlayer;
                nudKsVangPlayerMin.Value = Math.Max(nudKsVangPlayerMin.Minimum, Math.Min(_currentTrainSettings.KsVangPlayerMin, nudKsVangPlayerMin.Maximum));
                nudKsVangPlayerMax.Value = Math.Max(nudKsVangPlayerMax.Minimum, Math.Min(_currentTrainSettings.KsVangPlayerMax, nudKsVangPlayerMax.Maximum));
                if (chkKsVangAvoidChars != null)
                {
                    chkKsVangAvoidChars.Checked = _currentTrainSettings.KsVangAvoidChars;
                    txtKsVangAvoidCharsList.Text = _currentTrainSettings.KsVangAvoidCharsList ?? "";
                }
            }
            nudBuyThoiVangMinGold.Value = Math.Max(100_000_000, Math.Min(_currentTrainSettings.BuyThoiVangMinGold, 10_000_000_000L));
        }

        var mvbtMaps = new System.Collections.Generic.List<MapTemplate>();
        var mhbtMaps = new System.Collections.Generic.List<MapTemplate>();
        foreach (MapTemplate m in this.cboTrainMapId.Items)
        {
            if (m.Id == 156 || m.Id == 157) mvbtMaps.Add(m);
            if (m.Id == 158 || m.Id == 159) mhbtMaps.Add(m);
        }

        this.mvbtControl.ApplySettings(_currentMvbtSettings, mvbtMaps);
        this.mhbtControl.ApplySettings(_currentMhbtSettings, mhbtMaps);
        this.kilisControl.ApplySettings(_currentKilisSettings);
        this.chkBossVegetaCityEnable.Checked = _currentBossVegetaCitySettings.Enabled;
        this.chkBossVegetaCityAuto3h.Checked = _currentBossVegetaCitySettings.Auto15h;
        this.chkBossVegetaCityAuto2230.Checked = _currentBossVegetaCitySettings.Auto2230;
        this.chkBossVegetaCityReviveByGem.Checked = _currentBossVegetaCitySettings.ReviveByGem;
        this.chkBossVegetaCityUseTdlt.Checked = _currentBossVegetaCitySettings.UseTdlt;
        this.dauThanControl.ApplySettings(_currentDauThanSettings);
        this.buffNamekControl.ApplySettings(_currentBuffNamekSettings);
        this.buffNamekControl.ApplyReducePowerSettings(_currentReducePowerSettings);
        this._autoBossControl.ApplySettings(_currentBossSettings);
        this._dailyQuestControl.ApplySettings(_currentDailyQuestSettings);
        this._attendanceControl.ApplySettings(_currentAttendanceSettings);
        ApplyAutoAmuletSettingsToUi(_currentAutoAmuletSettings);
        ApplyDailyQuestRuntimeToControl(accountId);

        this.petControl.ApplySettings(_currentSettings.Pet ?? new PetFeatureSettings());
        this.upSkhControl.ApplySettings(_currentTrainSettings, _currentItemSettings);
        if (_skhTimes.TryGetValue(_currentSelectedAccountId, out var dtSkh))
        {
            this.upSkhControl.StartCountdown(dtSkh);
        }

        chkAutoDrop.Checked = _currentItemSettings.AutoDrop;
        chkDropByHsd.Checked = _currentItemSettings.DropByHsd;
        txtDropIds.Text = _currentItemSettings.DropIds ?? "";

        chkAutoStoreWhenFull.Checked = _currentItemSettings.AutoStoreWhenFull;
        chkStoreKichHoat.Checked = _currentItemSettings.StoreKichHoat;
        chkStoreThanLinh.Checked = _currentItemSettings.StoreThanLinh;
        chkStorePhaLe.Checked = _currentItemSettings.StorePhaLe;
        nudStoreStarCount.Value = _currentItemSettings.StoreStarCount > 0 ? _currentItemSettings.StoreStarCount : 1;
        chkStoreCustom.Checked = _currentItemSettings.StoreCustom;
        txtStoreCustomList.Text = _currentItemSettings.StoreCustomList ?? "";

        chkAutoSellTrash.Checked = _currentItemSettings.AutoSellTrash;
        nudSellEmptySlots.Value = Math.Max(0, Math.Min(_currentItemSettings.SellWhenEmptySlots, (int)nudSellEmptySlots.Maximum));
        chkDropInsteadOfSell.Checked = _currentItemSettings.DropInsteadOfSell;
        chkKeepStarItems.Checked = _currentItemSettings.KeepStarItems;
        chkKeepGodItems.Checked = _currentItemSettings.KeepGodItems;
        chkKeepSkhItems.Checked = _currentItemSettings.KeepSkhItems;
        nudSellMaxLevel.Value = Math.Max((int)nudSellMaxLevel.Minimum, Math.Min(_currentItemSettings.SellMaxLevel, (int)nudSellMaxLevel.Maximum));
        txtSellKeepIds.Text = _currentItemSettings.SellKeepIds ?? "";
        chkSellCustomNoStarCheck.Checked = _currentItemSettings.SellCustomNoStarCheck;
        txtSellCustomIdsList.Text = _currentItemSettings.SellCustomIdsList ?? "";

        cbAutoBuyTdlt.Checked = _currentItemSettings.AutoBuyTdlt;
        cbAutoBuyPrivateTicket.Checked = _currentItemSettings.AutoBuyPrivateTicket;
        cbAutoBuyKhauTrang.Checked = _currentItemSettings.AutoBuyKhauTrang;
        numBuyKhauTrangQty.Value = _currentItemSettings.BuyKhauTrangQty > 0 ? _currentItemSettings.BuyKhauTrangQty : 1;
        cbAutoBuyCoBonLa.Checked = _currentItemSettings.AutoBuyCoBonLa;
        numBuyCoBonLaQty.Value = _currentItemSettings.BuyCoBonLaQty > 0 ? _currentItemSettings.BuyCoBonLaQty : 1;
        cbAutoBuyBuaDe.Checked = _currentItemSettings.AutoBuyBuaDe;
        numBuyBuaDeQty.Value = _currentItemSettings.BuyBuaDeQty > 0 ? _currentItemSettings.BuyBuaDeQty : 1;
        chkAutoBuyCustom.Checked = _currentItemSettings.AutoBuyCustom;
        txtBuyCustomList.Text = _currentItemSettings.BuyCustomList ?? "";

        chkAutoPick.Checked = _currentItemSettings.AutoPick;
        cboPickMode.SelectedIndex = (_currentItemSettings.PickMode >= 0 && _currentItemSettings.PickMode < cboPickMode.Items.Count) ? _currentItemSettings.PickMode : 0;
        chkOnlyMyItems.Checked = _currentItemSettings.OnlyMyItems;
        txtPickIdsList.Text = _currentItemSettings.PickIdsList ?? "";
        txtPickBlackList.Text = _currentItemSettings.PickBlackList ?? "";

        chkUseCuongNo.Checked = _currentItemSettings.UseCuongNo;
        chkUseBoHuyet.Checked = _currentItemSettings.UseBoHuyet;
        chkUseBoKhi.Checked = _currentItemSettings.UseBoKhi;
        chkUseGiapXen.Checked = _currentItemSettings.UseGiapXen;
        chkUseMask.Checked = _currentItemSettings.UseMask;
        chkUse4LeafClover.Checked = _currentItemSettings.Use4LeafClover;
        chkUseFood.Checked = _currentItemSettings.UseFood;
        chkUseDetector.Checked = _currentItemSettings.UseDetector;
        chkUseItemById.Checked = _currentItemSettings.UseItemById;
        txtItemByIds.Text = _currentItemSettings.ItemByIds ?? "";

        _currentSupportSettings = _currentSettings.Support ?? new SupportSettings();
        int bongTaiIdx  = (_currentSupportSettings.BongTaiState >= 0 && _currentSupportSettings.BongTaiState < cboBongTaiState.Items.Count)
                          ? _currentSupportSettings.BongTaiState : 0;
        int petActionIdx = (_currentSupportSettings.BongTaiPetAction >= 0 && _currentSupportSettings.BongTaiPetAction < cboBongTaiPetAction.Items.Count)
                          ? _currentSupportSettings.BongTaiPetAction : 3;
        cboBongTaiState.SelectedIndex    = bongTaiIdx;
        cboBongTaiPetAction.SelectedIndex = petActionIdx;
        chkAutoCoDen.Checked             = _currentSupportSettings.AutoCoDen;
        chkDisableCoDenIfOthers.Checked  = _currentSupportSettings.DisableCoDenIfOthers;
        int flagIdx = _currentSupportSettings.FlagType;
        if (flagIdx < 0 || flagIdx >= cboFlagType.Items.Count) flagIdx = 8;
        cboFlagType.SelectedIndex = flagIdx;

        _isBindingData = false;
        // Neu account dang online thi dong bo lai setting ngay khi load tu DB.
        SendXmapSettingsCommand(accountId, _currentGeneralSettings);
        SendProxySettingsCommand(accountId, _currentGeneralSettings);
        SendActionOnDeathCommand(accountId, _currentGeneralSettings);
        if (_currentTrainSettings.Enabled) SendTrainSettingsCommand(accountId, _currentTrainSettings);
        else SendTrainOffCommand(accountId);
        
        SendMvbtSettingsCommand(accountId, _currentMvbtSettings);
        SendMhbtSettingsCommand(accountId, _currentMhbtSettings);
        SendKilisSettingsCommand(accountId, _currentKilisSettings);
        SendBossVegetaCitySettingsCommand(accountId, _currentBossVegetaCitySettings);
        SendPetSettingsCommand(accountId, _currentSettings.Pet ?? new PetFeatureSettings());
        SendBossSettingsCommand(accountId, _currentBossSettings);
        SendDailyQuestSettingsCommand(accountId, _currentDailyQuestSettings);
        
        SendTrainAdvancedCommand(accountId, _currentTrainSettings);
        SendStoreSettingsCommand(accountId, _currentItemSettings);
        SendSellSettingsCommand(accountId, _currentItemSettings);
        SendBuySettingsCommand(accountId, _currentItemSettings);
        SendAutoAmuletSettingsCommand(accountId, _currentAutoAmuletSettings);
        SendPickSettingsCommand(accountId, _currentItemSettings);
        SendDropSettingsCommand(accountId, _currentItemSettings);
        SendUseItemSettingsCommand(accountId, _currentItemSettings);
        SendSupportSettingsCommand(accountId, _currentSupportSettings);

        if (_currentAutoUpZinSettings.Enabled)
            SendAutoUpZinCommand(accountId, _currentAutoUpZinSettings);
        else
            SendAutoUpZinOffCommand(accountId);

        if (_currentUpZin700kSettings.Enabled)
            SendUpZin700kCommand(accountId, _currentUpZin700kSettings);
        else
            SendUpZin700kOffCommand(accountId);

    }

    private bool _isBindingData = false;

    private void ApplyProxyUiState()
    {
        bool enabled = chkUseProxy.Checked;
        cboProxyType.Enabled = enabled;
        txtProxyAddress.Enabled = enabled;
        btnTestProxy.Enabled = enabled;

        if (!enabled)
        {
            lblProxyStatus.Text = "Proxy đang tắt (Direct)";
            lblProxyStatus.ForeColor = System.Drawing.Color.Gray;
        }
        else if (lblProxyStatus.Text == "Proxy đang tắt (Direct)")
        {
            lblProxyStatus.Text = "";
            lblProxyStatus.ForeColor = System.Drawing.Color.Gray;
        }
    }

    private void GeneralSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        // Cập nhật giá trị vào object current
        _currentGeneralSettings.EatChicken = chkEatChicken.Checked;
        _currentGeneralSettings.UseTdltXmap = chkUseTdltXmap.Checked;
        _currentGeneralSettings.ActionOnDeath = cboActionOnDeath.SelectedIndex >= 0 ? cboActionOnDeath.SelectedIndex : 0;
        _currentGeneralSettings.UseProxy = chkUseProxy.Checked;
        _currentGeneralSettings.ProxyType = cboProxyType.SelectedIndex >= 0 ? cboProxyType.SelectedIndex : 0;
        _currentGeneralSettings.ProxyAddress = txtProxyAddress.Text.Trim();
        _currentGeneralSettings.TypeAccount = (int)nudFilterTypeAccount.Value;
        ApplyProxyUiState();

        // Lưu xuống DB
        _currentSettings.General = _currentGeneralSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        // Gửi command xuống client nếu client đang online
        SendXmapSettingsCommand(_currentSelectedAccountId, _currentGeneralSettings);
        SendProxySettingsCommand(_currentSelectedAccountId, _currentGeneralSettings);
        SendActionOnDeathCommand(_currentSelectedAccountId, _currentGeneralSettings);
    }


    private async void BtnTestProxy_Click(object? sender, EventArgs e)
    {
        string address = txtProxyAddress.Text.Trim();
        if (string.IsNullOrWhiteSpace(address))
        {
            lblProxyStatus.Text = "⚠ Chưa nhập địa chỉ proxy!";
            lblProxyStatus.ForeColor = System.Drawing.Color.OrangeRed;
            return;
        }

        // Parse host:port, host:port:user:pass or user:pass@host:port
        string host = "";
        int port = 0;
        string user = "";
        string pass = "";

        if (address.Contains("@"))
        {
            string[] authAddr = address.Split('@');
            if (authAddr.Length == 2)
            {
                string[] auth = authAddr[0].Split(':');
                if (auth.Length == 2)
                {
                    user = auth[0];
                    pass = auth[1];
                }
                string[] addr = authAddr[1].Split(':');
                if (addr.Length == 2 && int.TryParse(addr[1], out port))
                {
                    host = addr[0];
                }
            }
        }
        else
        {
            string[] parts = address.Split(':');
            if (parts.Length == 4)
            {
                host = parts[0].Trim();
                if (int.TryParse(parts[1], out port))
                {
                    user = parts[2].Trim();
                    pass = parts[3].Trim();
                }
            }
            else if (parts.Length == 2 && int.TryParse(parts[1], out port))
            {
                host = parts[0].Trim();
            }
        }

        if (string.IsNullOrEmpty(host) || port <= 0)
        {
            lblProxyStatus.Text = "⚠ Địa chỉ proxy sai định dạng (host:port, host:port:u:p hoặc u:p@host:port)";
            lblProxyStatus.ForeColor = System.Drawing.Color.OrangeRed;
            return;
        }

        int proxyTypeIdx = cboProxyType.SelectedIndex >= 0 ? cboProxyType.SelectedIndex : 0;

        lblProxyStatus.Text = "⏳ Đang test proxy...";
        lblProxyStatus.ForeColor = System.Drawing.Color.Gray;
        btnTestProxy.Enabled = false;

        try
        {
            System.Net.WebProxy proxy;
            string proxyProtocol = (proxyTypeIdx == 0) ? "http" : "socks5";
            proxy = new System.Net.WebProxy($"{proxyProtocol}://{host}:{port}", false);
            
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
            {
                proxy.Credentials = new System.Net.NetworkCredential(user, pass);
            }

            var handler = new System.Net.Http.HttpClientHandler { Proxy = proxy, UseProxy = true };
            using var client = new System.Net.Http.HttpClient(handler) { Timeout = System.TimeSpan.FromSeconds(8) };

            // Test bằng cách kết nối tới google.com
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var resp = await client.GetAsync("https://www.google.com");
            sw.Stop();

            if (resp.IsSuccessStatusCode)
            {
                lblProxyStatus.Text = $"✅ Proxy sống! ({sw.ElapsedMilliseconds}ms)";
                lblProxyStatus.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                lblProxyStatus.Text = $"❌ HTTP {(int)resp.StatusCode}";
                lblProxyStatus.ForeColor = System.Drawing.Color.Red;
            }
        }
        catch (System.Net.Http.HttpRequestException)
        {
            lblProxyStatus.Text = "❌ Proxy chết hoặc không kết nối được";
            lblProxyStatus.ForeColor = System.Drawing.Color.Red;
        }
        catch (TaskCanceledException)
        {
            lblProxyStatus.Text = "❌ Timeout (>8s) - Proxy quá chậm hoặc chết";
            lblProxyStatus.ForeColor = System.Drawing.Color.OrangeRed;
        }
        catch (Exception ex)
        {
            lblProxyStatus.Text = $"❌ Lỗi: {ex.Message}";
            lblProxyStatus.ForeColor = System.Drawing.Color.Red;
        }
        finally
        {
            btnTestProxy.Enabled = true;
        }
    }

    private void SendXmapSettingsCommand(int accountId, GeneralSettings settings)
    {
        string cmd = $"XMAP_SETTING|{settings.EatChicken}|{settings.PostMapLoadDelay}|{settings.UseTdltXmap}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void SendProxySettingsCommand(int accountId, GeneralSettings settings)
    {
        if (!settings.UseProxy)
        {
            _socketServer.SendCommand(accountId, "PROXY_SETTING|0|");
            return;
        }

        string addr = settings.ProxyAddress?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(addr))
        {
            // Tắt proxy: gửi không có địa chỉ
            _socketServer.SendCommand(accountId, "PROXY_SETTING|0|");
        }
        else
        {
            string cmd = $"PROXY_SETTING|{settings.ProxyType}|{addr}";
            _socketServer.SendCommand(accountId, cmd);
        }
    }

    private void SendActionOnDeathCommand(int accountId, GeneralSettings settings)
    {
        _socketServer.SendCommand(accountId, $"ACTION_ON_DEATH|{settings.ActionOnDeath}");
    }

    private void BtnAutoBoMong_Click(object? sender, EventArgs e)
    {
        if (_tabDailyQuest != null)
        {
            tabControlFeatures.SelectedTab = _tabDailyQuest;
        }
    }

    private void OnClientDailyQuestStatusReceived(
        int accountId,
        bool isRunning,
        string runMode,
        string stateText,
        int completedToday,
        int canceledToday,
        bool finishedToday)
    {
        var settings = _accountSettingsService.Load(accountId);
        int currentKilis = _latestKilis.TryGetValue(accountId, out int k) ? k : 0;
        int currentMvbt = _latestMvbt.TryGetValue(accountId, out int m) ? m : 0;
        int currentMhbt = _latestMhbt.TryGetValue(accountId, out int h) ? h : 0;

        bool wasReset = Panel.Helpers.DailyMetricsHelper.CheckAndResetDailyMetrics(
            settings,
            currentKilis,
            currentMvbt,
            currentMhbt,
            out _,
            out _,
            out _);

        bool shouldSave = wasReset;
        if (settings.Daily.DailyQuestCompletedCount != completedToday)
        {
            settings.Daily.DailyQuestCompletedCount = completedToday;
            shouldSave = true;
        }

        if (settings.Daily.DailyQuestCanceledCount != canceledToday)
        {
            settings.Daily.DailyQuestCanceledCount = canceledToday;
            shouldSave = true;
        }

        if (settings.Daily.DailyQuestFinishedToday != finishedToday)
        {
            settings.Daily.DailyQuestFinishedToday = finishedToday;
            shouldSave = true;
        }

        if (!string.Equals(settings.Daily.DailyQuestLastRunMode, runMode, StringComparison.Ordinal))
        {
            settings.Daily.DailyQuestLastRunMode = runMode ?? string.Empty;
            shouldSave = true;
        }

        settings.DailyQuest ??= new DailyQuestFeatureSettings();
        if (settings.DailyQuest.Enabled != isRunning)
        {
            settings.DailyQuest.Enabled = isRunning;
            shouldSave = true;
        }

        if (shouldSave)
        {
            _accountSettingsService.Save(accountId, settings);
        }

        _dailyQuestRuntimeByAccount[accountId] = new DailyQuestRuntimeStatus
        {
            IsRunning = isRunning,
            RunMode = runMode ?? string.Empty,
            StateText = string.IsNullOrWhiteSpace(stateText)
                ? (finishedToday ? "Đã xong hôm nay" : (isRunning ? "Đang chạy" : "Đang tắt"))
                : stateText,
            CompletedToday = completedToday,
            CanceledToday = canceledToday,
            FinishedToday = finishedToday
        };

        if (accountId == _currentSelectedAccountId)
        {
            this.BeginInvoke((Action)(() => ApplyDailyQuestRuntimeToControl(accountId)));
        }
    }
    
    private void CboTrainMapId_TextUpdate(object? sender, EventArgs e)
    {
        if (_isBindingData) return;
        _mapTypingTimer.Stop();
        _mapTypingTimer.Start();
    }

    private void MapTypingTimer_Tick(object? sender, EventArgs e)
    {
        _mapTypingTimer.Stop();
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        string inputText = cboTrainMapId.Text.Trim();
        if (int.TryParse(inputText, out int inputId))
        {
            foreach (MapTemplate m in cboTrainMapId.Items)
            {
                if (m.Id == inputId)
                {
                    cboTrainMapId.SelectedItem = m;
                    return;
                }
            }
        }

        if (_lastValidMapTemplate != null)
        {
            cboTrainMapId.SelectedItem = _lastValidMapTemplate;
        }
    }

    private void TrainSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        // FIX 4: Đọc toàn bộ UI vào _currentTrainSettings ngay lập tức (để UI luôn đồng bộ)
        _currentTrainSettings.Enabled = chkTrainEnable.Checked;
        if (cboTrainMapId.SelectedItem is MapTemplate selectedMap)
        {
            _currentTrainSettings.MapId = selectedMap.Id;
            _lastValidMapTemplate = selectedMap;
        }
        _currentTrainSettings.RequireZone = chkTrainZoneRequire.Checked;
        if (int.TryParse(txtTrainZone.Text, out int zoneId)) _currentTrainSettings.ZoneId = zoneId;
        _currentTrainSettings.UseTDLT = chkUseTDLT.Checked;
        _currentTrainSettings.OnlyUsePunch = chkOnlyUsePunch.Checked;
        _currentTrainSettings.FreezePunchSkillCd = chkFreezePunchSkillCd.Checked;
        _currentTrainSettings.AvoidSuperMob = chkAvoidSuperMob.Checked;

        if (chkTrainEarthDragon != null)
        {
            _currentTrainSettings.SkillEarthDragon = chkTrainEarthDragon.Checked;
            _currentTrainSettings.SkillEarthKame = chkTrainEarthKame.Checked;
            _currentTrainSettings.SkillEarthTdhs = chkTrainEarthTdhs.Checked;
            _currentTrainSettings.SkillEarthThoiMien = chkTrainEarthThoiMien.Checked;
            _currentTrainSettings.SkillEarthDctt = chkTrainEarthDctt.Checked;
            _currentTrainSettings.SkillEarthKhien = chkTrainEarthKhien.Checked;
            _currentTrainSettings.SkillEarthKaioken = chkTrainEarthKaioken.Checked;

            _currentTrainSettings.SkillNamekLienHoan = chkTrainNamekLienHoan.Checked;
            _currentTrainSettings.SkillNamekDemon = chkTrainNamekDemon.Checked;
            _currentTrainSettings.SkillNamekMakan = chkTrainNamekMakan.Checked;
            _currentTrainSettings.SkillNamekDeTrung = chkTrainNamekDeTrung.Checked;
            _currentTrainSettings.SkillNamekKhien = chkTrainNamekKhien.Checked;

            _currentTrainSettings.SkillSaiyanGalick = chkTrainXaydaGalick.Checked;
            _currentTrainSettings.SkillSaiyanAntomic = chkTrainXaydaAntomic.Checked;
            _currentTrainSettings.SkillSaiyanBienHinh = chkTrainXaydaBienHinh.Checked;
            _currentTrainSettings.SkillSaiyanTtNl = chkTrainXaydaTtNl.Checked;
            _currentTrainSettings.SkillSaiyanKhien = chkTrainXaydaKhien.Checked;
        }

        if (chkUseShieldUnderHpTrain != null)
        {
            _currentTrainSettings.UseShieldUnderHp = chkUseShieldUnderHpTrain.Checked;
            _currentTrainSettings.ShieldHpPercent = (int)nudShieldHpPercentTrain.Value;
        }

        _currentTrainSettings.ChangeLowPlayerZoneIfNoMob = chkChangeLowPlayerZoneIfNoMob.Checked;
        _currentTrainSettings.CheckLagMob = chkCheckLagMob != null ? chkCheckLagMob.Checked : true;
        _currentTrainSettings.MobTargetType = cboMobTargetType.SelectedIndex;
        _currentTrainSettings.MobIds = txtMobIds.Text;
        _currentTrainSettings.TrainingArmorMode = cboTrainingArmorMode.SelectedIndex < 0 ? 0 : cboTrainingArmorMode.SelectedIndex;

        _currentSettings.Train = _currentTrainSettings;
        // Lưu DB ngay lập tức để không mất data nếu đóng panel
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        upSkhControl.ApplySettings(_currentTrainSettings, _currentItemSettings);

        // FIX 4: Debounce — khởi động lại timer 400ms, chỉ gửi command sau khi user dừng thay đổi
        // Tránh gửi command N lần khi user tick/untick nhiều checkbox liên tiếp
        _trainDebounceTimer.Stop();
        _trainDebounceTimer.Start();
    }

    private void TrainDebounceTimer_Tick(object? sender, EventArgs e)
    {
        _trainDebounceTimer.Stop();
        if (_currentSelectedAccountId <= 0) return;

        // Gửi command sau khi debounce: chỉ 1 lần dù user thay đổi bao nhiêu field
        if (_currentTrainSettings.Enabled)
            SendTrainSettingsCommand(_currentSelectedAccountId, _currentTrainSettings);
        else
            SendTrainOffCommand(_currentSelectedAccountId);

        SendBossVegetaCitySettingsCommand(_currentSelectedAccountId, _currentBossVegetaCitySettings);
    }

    private void AutoUpZinSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _isUpdatingAutoUpZinPrefix || _currentSelectedAccountId <= 0) return;

        string normalizedPrefix = NormalizeAutoUpZinPrefix(txtAutoUpZinPrefix.Text);
        if (!string.Equals(txtAutoUpZinPrefix.Text, normalizedPrefix, StringComparison.Ordinal))
        {
            int cursor = txtAutoUpZinPrefix.SelectionStart;
            _isUpdatingAutoUpZinPrefix = true;
            txtAutoUpZinPrefix.Text = normalizedPrefix;
            txtAutoUpZinPrefix.SelectionStart = Math.Min(cursor, normalizedPrefix.Length);
            _isUpdatingAutoUpZinPrefix = false;
        }

        _currentAutoUpZinSettings.Enabled = chkAutoUpZin.Checked;
        _currentAutoUpZinSettings.NamePrefix = normalizedPrefix;
        _currentAutoUpZinSettings.TargetClass = cmbAutoUpZinClass.SelectedIndex == 3 ? -1 : cmbAutoUpZinClass.SelectedIndex;

        _currentSettings.AutoUpZin = _currentAutoUpZinSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        if (_currentAutoUpZinSettings.Enabled)
            SendAutoUpZinCommand(_currentSelectedAccountId, _currentAutoUpZinSettings);
        else
            SendAutoUpZinOffCommand(_currentSelectedAccountId);

        UpdateAutoUpZinPrefixUi();
    }

    private void UpdateAutoUpZinPrefixUi()
    {
        if (chkAutoUpZin == null || txtAutoUpZinPrefix == null || lblAutoUpZinPrefixHint == null)
            return;

        txtAutoUpZinPrefix.Enabled = chkAutoUpZin.Checked;
        cmbAutoUpZinClass.Enabled = chkAutoUpZin.Checked;

        string prefix = txtAutoUpZinPrefix.Text.Trim();
        if (string.IsNullOrEmpty(prefix))
        {
            lblAutoUpZinPrefixHint.Text = "Nhập prefix 3-4 ký tự (chữ/số), client sẽ random phần còn lại.";
            lblAutoUpZinPrefixHint.ForeColor = Color.DimGray;
            return;
        }

        if (prefix.Length < 3)
        {
            lblAutoUpZinPrefixHint.Text = $"Prefix quá ngắn ({prefix.Length}/3). Cần tối thiểu 3 ký tự.";
            lblAutoUpZinPrefixHint.ForeColor = Color.FromArgb(217, 119, 6);
            return;
        }

        if (IsValidAutoUpZinPrefix(prefix))
        {
            lblAutoUpZinPrefixHint.Text = $"Hợp lệ: {prefix} + random phần còn lại";
            lblAutoUpZinPrefixHint.ForeColor = Color.FromArgb(22, 163, 74);
        }
        else
        {
            lblAutoUpZinPrefixHint.Text = "Prefix không hợp lệ. Chỉ nhận 3-4 ký tự chữ/số.";
            lblAutoUpZinPrefixHint.ForeColor = Color.FromArgb(220, 38, 38);
        }
    }

    private static string NormalizeAutoUpZinPrefix(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        char[] normalized = raw.Trim().Where(char.IsLetterOrDigit).Take(4).ToArray();
        return new string(normalized);
    }

    private void TxtAutoUpZinPrefix_KeyPress(object? sender, KeyPressEventArgs e)
    {
        if (char.IsControl(e.KeyChar))
            return;

        if (!char.IsLetterOrDigit(e.KeyChar))
            e.Handled = true;
    }

    private static bool IsValidAutoUpZinPrefix(string prefix)
    {
        if (prefix.Length < 3 || prefix.Length > 4)
            return false;

        foreach (char c in prefix)
        {
            if (!char.IsLetterOrDigit(c))
                return false;
        }

        return true;
    }

    private void SendAutoUpZinCommand(int accountId, AutoUpZinSettings settings)
    {
        string prefix = NormalizeAutoUpZinPrefix(settings?.NamePrefix ?? string.Empty);
        int targetClass = settings?.TargetClass ?? -1;
        string cmd = $"UPZIN|1|{prefix}|{targetClass}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void SendAutoUpZinOffCommand(int accountId)
    {
        _socketServer.SendCommand(accountId, "UPZIN_OFF");
    }

    private void SendUpZin700kCommand(int accountId, AutoUpZinTo700kSettings settings)
    {
        string prefix = NormalizeUpZin700kPrefix(settings?.NamePrefix ?? string.Empty);
        int targetClass = settings?.TargetClass ?? -1;
        string cmd = $"UPZIN700K|1|{prefix}|{targetClass}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void SendUpZin700kOffCommand(int accountId)
    {
        _socketServer.SendCommand(accountId, "UPZIN700K_OFF");
    }

    private static string NormalizeUpZin700kPrefix(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        char[] chars = raw.Trim().Where(char.IsLetterOrDigit).Take(8).ToArray();
        return new string(chars).ToLowerInvariant();
    }

    private void UpdateUpZin700kPrefixUi()
    {
        if (chkUpZin700k == null || txtUpZin700kPrefix == null || lblUpZin700kPrefixHint == null)
            return;

        txtUpZin700kPrefix.Enabled = chkUpZin700k.Checked;
        cmbUpZin700kClass.Enabled = chkUpZin700k.Checked;

        string prefix = txtUpZin700kPrefix.Text.Trim();
        if (string.IsNullOrEmpty(prefix))
        {
            lblUpZin700kPrefixHint.Text = "Nhập prefix 3-8 ký tự (chữ/số), client sẽ random phần còn lại.";
            lblUpZin700kPrefixHint.ForeColor = Color.DimGray;
            return;
        }

        if (prefix.Length < 3)
        {
            lblUpZin700kPrefixHint.Text = $"Prefix quá ngắn ({prefix.Length}/3). Cần tối thiểu 3 ký tự.";
            lblUpZin700kPrefixHint.ForeColor = Color.FromArgb(217, 119, 6);
            return;
        }

        if (IsValidUpZin700kPrefix(prefix))
        {
            lblUpZin700kPrefixHint.Text = $"Hợp lệ: {prefix} + random phần còn lại";
            lblUpZin700kPrefixHint.ForeColor = Color.FromArgb(22, 163, 74);
        }
        else
        {
            lblUpZin700kPrefixHint.Text = "Prefix không hợp lệ. Chỉ nhận 3-8 ký tự chữ/số.";
            lblUpZin700kPrefixHint.ForeColor = Color.FromArgb(220, 38, 38);
        }
    }

    private static bool IsValidUpZin700kPrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix) || prefix.Length < 3 || prefix.Length > 8)
            return false;
        foreach (char c in prefix)
        {
            if (!char.IsLetterOrDigit(c))
                return false;
        }
        return true;
    }

    private void MvbtSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentMvbtSettings = mvbtControl.GetSettings();
        _currentSettings.Mvbt = _currentMvbtSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        SendMvbtSettingsCommand(_currentSelectedAccountId, _currentMvbtSettings);
    }

    private void MhbtSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentMhbtSettings = mhbtControl.GetSettings();
        _currentSettings.Mhbt = _currentMhbtSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        SendMhbtSettingsCommand(_currentSelectedAccountId, _currentMhbtSettings);
    }

    private void KilisSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentKilisSettings = kilisControl.GetSettings();
        _currentSettings.Kilis = _currentKilisSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        SendKilisSettingsCommand(_currentSelectedAccountId, _currentKilisSettings);
    }

    private void BossVegetaCitySettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentBossVegetaCitySettings.Enabled = chkBossVegetaCityEnable.Checked;
        _currentBossVegetaCitySettings.Auto15h = chkBossVegetaCityAuto3h.Checked;
        _currentBossVegetaCitySettings.Auto2230 = chkBossVegetaCityAuto2230.Checked;
        _currentBossVegetaCitySettings.ReviveByGem = chkBossVegetaCityReviveByGem.Checked;
        _currentBossVegetaCitySettings.UseTdlt = chkBossVegetaCityUseTdlt.Checked;

        _currentSettings.BossVegetaCity = _currentBossVegetaCitySettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        SendBossVegetaCitySettingsCommand(_currentSelectedAccountId, _currentBossVegetaCitySettings);
    }

    private void UpSkhSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        upSkhControl.GetSettings(_currentTrainSettings, _currentItemSettings);

        _isBindingData = true;
        if (chkUseTDLT != null) chkUseTDLT.Checked = _currentTrainSettings.UseTDLT;
        if (chkUse4LeafClover != null) chkUse4LeafClover.Checked = _currentItemSettings.Use4LeafClover;
        if (cbAutoBuyTdlt != null) cbAutoBuyTdlt.Checked = _currentItemSettings.AutoBuyTdlt;
        if (cbAutoBuyPrivateTicket != null) cbAutoBuyPrivateTicket.Checked = _currentItemSettings.AutoBuyPrivateTicket;
        if (cbAutoBuyCoBonLa != null) cbAutoBuyCoBonLa.Checked = _currentItemSettings.AutoBuyCoBonLa;
        _isBindingData = false;

        _currentSettings.Train = _currentTrainSettings;
        _currentSettings.Item = _currentItemSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        // FIX 5: Chỉ gửi TRAIN command khi Train đang enabled, tránh reset runtime vô cớ
        if (_currentTrainSettings.Enabled)
            SendTrainSettingsCommand(_currentSelectedAccountId, _currentTrainSettings);

        SendTrainAdvancedCommand(_currentSelectedAccountId, _currentTrainSettings);
        SendBossVegetaCitySettingsCommand(_currentSelectedAccountId, _currentBossVegetaCitySettings);
        SendBuySettingsCommand(_currentSelectedAccountId, _currentItemSettings);
        SendUseItemSettingsCommand(_currentSelectedAccountId, _currentItemSettings);
    }


    private void DauThanSettingsControl_Changed(object? sender, EventArgs e)
    {
        // FIX 3: Thêm _isBindingData guard (giống tất cả handler khác) và sửa dấu <= 0
        if (_isBindingData || _currentSelectedAccountId <= 0) return;
        lock (_settingsSyncLock)
        {
            _currentSettings.DauThan = _currentDauThanSettings;
            _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);
            SendDauThanSettingsCommand(_currentSelectedAccountId, _currentDauThanSettings);
        }
    }

    private void AutoBossSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_currentSelectedAccountId < 0) return;
        lock (_settingsSyncLock)
        {
            // Lấy từ control (single source of truth) thay vì field riêng
            _currentBossSettings = _autoBossControl.CurrentSettings;
            _currentSettings.Boss = _currentBossSettings;
            _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);
            
            // Chỉ gửi setting cho acc hiện tại — mỗi acc có cấu hình riêng
            SendBossSettingsCommand(_currentSelectedAccountId, _currentBossSettings);
        }
    }

    private void DailyQuestSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        lock (_settingsSyncLock)
        {
            _currentDailyQuestSettings = _dailyQuestControl.CurrentSettings;
            _currentSettings.DailyQuest = _currentDailyQuestSettings;
            _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);
            SendDailyQuestSettingsCommand(_currentSelectedAccountId, _currentDailyQuestSettings);
        }
    }

    private void DailyQuestControl_ToggleAutoRequested(object? sender, EventArgs e)
    {
        if (_currentSelectedAccountId <= 0)
        {
            return;
        }

        DailyQuestRuntimeStatus runtime = GetDailyQuestRuntimeStatus(_currentSelectedAccountId);
        if (runtime.IsRunning)
        {
            _currentDailyQuestSettings = _dailyQuestControl.CurrentSettings;
            _currentDailyQuestSettings.Enabled = false;
            _currentSettings.DailyQuest = _currentDailyQuestSettings;
            _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);
            SendDailyQuestStopCommand(_currentSelectedAccountId);
            runtime.IsRunning = false;
            runtime.StateText = runtime.FinishedToday ? "Đã xong hôm nay" : "Đang tắt";
        }
        else
        {
            _currentDailyQuestSettings = _dailyQuestControl.CurrentSettings;
            _currentDailyQuestSettings.Enabled = true;
            _currentSettings.DailyQuest = _currentDailyQuestSettings;
            _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);
            SendDailyQuestSettingsCommand(_currentSelectedAccountId, _currentDailyQuestSettings);
            SendDailyQuestStartCommand(_currentSelectedAccountId);
            runtime.StateText = "Đang khởi động";
        }

        _dailyQuestRuntimeByAccount[_currentSelectedAccountId] = runtime;
        if (_currentSelectedAccountId > 0)
        {
            ApplyDailyQuestRuntimeToControl(_currentSelectedAccountId);
        }
    }

    private void OnClientAttendanceStatusReceived(
        int accountId,
        bool enabled,
        string stateText,
        string monthlyKey,
        string continuousDate,
        string onlineDate,
        int onlineCount,
        int nextOnlineSeconds,
        bool canClaimOnline,
        string lastCheckTime)
    {
        var settings = _accountSettingsService.Load(accountId);
        settings.Attendance ??= new AttendanceFeatureSettings();
        var attendance = settings.Attendance;
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        string monthKey = DateTime.Now.ToString("yyyyMM");

        attendance.Enabled = enabled;
        attendance.StateText = string.IsNullOrWhiteSpace(stateText) ? (enabled ? "Đang chạy" : "Đang tắt") : stateText;
        if (!string.IsNullOrWhiteSpace(monthlyKey)) attendance.MonthlyClaimedKey = monthlyKey;
        if (!string.IsNullOrWhiteSpace(continuousDate)) attendance.ContinuousClaimDate = continuousDate;

        // Không cho report rỗng/chưa đọc online ghi đè dữ liệu đã lưu trong DB.
        // Khi client đang kẹt/chờ popup online, onlineCount=0 và onlineDate rỗng làm Panel mở lại tưởng chưa có mốc nào.
        bool hasNewOnlineProgress = onlineCount > 0 || !string.IsNullOrWhiteSpace(onlineDate) || canClaimOnline || nextOnlineSeconds > 0;
        if (hasNewOnlineProgress)
        {
            if (!string.IsNullOrWhiteSpace(onlineDate)) attendance.OnlineClaimDate = onlineDate;
            if (onlineCount > attendance.OnlineClaimedCount || attendance.OnlineClaimDate != today)
            {
                attendance.OnlineClaimedCount = Math.Max(0, onlineCount);
            }
            attendance.NextOnlineSeconds = nextOnlineSeconds;
            attendance.CanClaimOnline = canClaimOnline;
        }
        else
        {
            attendance.NextOnlineSeconds = enabled ? attendance.NextOnlineSeconds : -1;
            attendance.CanClaimOnline = false;
        }

        if (attendance.MonthlyClaimedKey != monthKey && !string.IsNullOrWhiteSpace(monthlyKey)) attendance.MonthlyClaimedKey = monthlyKey;
        if (attendance.ContinuousClaimDate != today && !string.IsNullOrWhiteSpace(continuousDate)) attendance.ContinuousClaimDate = continuousDate;
        attendance.LastCheckTime = lastCheckTime ?? string.Empty;
        _accountSettingsService.Save(accountId, settings);

        if (accountId == _currentSelectedAccountId)
        {
            _currentAttendanceSettings = settings.Attendance;
            this.BeginInvoke((Action)(() => _attendanceControl.ApplyRuntime(_currentAttendanceSettings)));
        }
    }

    private void MarkAttendanceOffline(int accountId)
    {
        if (accountId <= 0) return;

        var settings = _accountSettingsService.Load(accountId);
        settings.Attendance ??= new AttendanceFeatureSettings();
        settings.Attendance.Enabled = false;
        settings.Attendance.StateText = "Game offline";
        settings.Attendance.NextOnlineSeconds = -1;
        settings.Attendance.CanClaimOnline = false;
        _accountSettingsService.Save(accountId, settings);

        if (accountId == _currentSelectedAccountId)
        {
            _currentAttendanceSettings = settings.Attendance;
        }
    }

    private void AttendanceSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentAttendanceSettings = _attendanceControl.GetSettings();
        _currentSettings.Attendance = _currentAttendanceSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);
        SendAttendanceSettingsCommand(_currentSelectedAccountId, _currentAttendanceSettings);
    }

    private void AttendanceControl_ToggleAutoRequested(object? sender, EventArgs e)
    {
        if (_currentSelectedAccountId <= 0) return;

        _currentAttendanceSettings = _attendanceControl.GetSettings();
        _currentAttendanceSettings.Enabled = !_currentAttendanceSettings.Enabled;
        _currentAttendanceSettings.StateText = _currentAttendanceSettings.Enabled ? "Đang khởi động" : "Đang tắt";
        _currentSettings.Attendance = _currentAttendanceSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        if (_currentAttendanceSettings.Enabled)
        {
            SendAttendanceSettingsCommand(_currentSelectedAccountId, _currentAttendanceSettings);
            _socketServer.SendCommand(_currentSelectedAccountId, "ATTENDANCE_START");
        }
        else
        {
            SendAttendanceOffCommand(_currentSelectedAccountId);
        }

        _attendanceControl.ApplyRuntime(_currentAttendanceSettings);
    }

    private void SendAttendanceSettingsCommand(int accountId, AttendanceFeatureSettings settings)
    {
        if (settings == null) return;
        string cmd = $"ATTENDANCE|{(settings.Enabled ? 1 : 0)}|{(settings.AutoStart ? 1 : 0)}|{(settings.ClaimMonthly ? 1 : 0)}|{(settings.ClaimContinuous ? 1 : 0)}|{(settings.ClaimOnline ? 1 : 0)}|{(settings.ScheduleEnabled ? 1 : 0)}|{settings.ScheduleHour}|{settings.ScheduleMinute}|{settings.OnlineClaimedCount}|{EncodeCommandText(settings.OnlineClaimDate ?? string.Empty)}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void SendAttendanceOffCommand(int accountId)
    {
        _socketServer.SendCommand(accountId, "ATTENDANCE_OFF");
    }

    private void SendDailyQuestSettingsCommand(int accountId, DailyQuestFeatureSettings settings)
    {
        if (settings == null) return;
        string cmd =
            $"DAILY_QUEST|{(settings.Enabled ? 1 : 0)}|{(settings.ScheduleEnabled ? 1 : 0)}|{settings.StartHour}|{settings.StartMinute}|{EncodeCommandText(settings.Difficulty)}" +
            $"|{(settings.CancelKillPlayerQuest ? 1 : 0)}|{(settings.CancelTrainGoldQuest ? 1 : 0)}|{(settings.CancelTrainMonsterQuest ? 1 : 0)}" +
            $"|{(settings.TrainMonsterEnabled ? 1 : 0)}|{settings.TrainMonsterMapId}|{settings.TrainMonsterZoneId}|{EncodeCommandText(settings.TrainMonsterMobNames)}" +
            $"|{settings.TrainGoldMapId}|{(settings.TrainGoldRequireZone ? 1 : 0)}|{settings.TrainGoldZoneId}" +
            $"|{(settings.UseGoldSuicideMode ? 1 : 0)}|{settings.TrainGoldSuicideMapId}|{settings.TrainGoldSuicideZoneId}" +
            $"|{settings.KillPlayerMapId}|{settings.KillPlayerZoneId}|{(settings.KillPlayerOnlyListedTargets ? 1 : 0)}|{EncodeCommandText(settings.KillPlayerTargetNames)}" +
            $"|{(settings.AutoFusion ? 1 : 0)}|{settings.TrainingArmorMode}|{(settings.UseTdltWhenDoingDailyQuest ? 1 : 0)}" +
            $"|{(settings.TdltForTrainMonster ? 1 : 0)}|{(settings.TdltForKillPlayer ? 1 : 0)}|{(settings.TdltForTrainGold ? 1 : 0)}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void SendDailyQuestStartCommand(int accountId)
    {
        _socketServer.SendCommand(accountId, "DAILY_QUEST_START");
    }

    private void SendDailyQuestStopCommand(int accountId)
    {
        _socketServer.SendCommand(accountId, "DAILY_QUEST_OFF");
    }

    /// <summary>
    /// Khi bấm "Đồng bộ thông số": copy MapRanges, ZoneRanges, BossNames, LimitMap, LimitZone, EnableSyncCoordinator
    /// sang tất cả acc cùng server + đã tích Liên kết.
    /// </summary>
    private void AutoBossSyncParams_Requested(object? sender, EventArgs e)
    {
        if (_currentSelectedAccountId < 0) return;

        // Lấy server của acc nguồn
        var srcAcc = _accountRepo.GetAllAccounts().FirstOrDefault(a => a.Id == _currentSelectedAccountId);
        if (srcAcc == null) return;
        string srcServer = srcAcc.Server;

        // Params cần đồng bộ từ acc hiện tại — luôn lấy từ control để đảm bảo mới nhất
        _currentBossSettings = _autoBossControl.CurrentSettings;
        var src = _currentBossSettings;

        // Tất cả acc cùng server + đã tích Liên kết (EnableSyncCoordinator)
        var allAccounts = _accountRepo.GetAllAccounts();
        foreach (var acc in allAccounts)
        {
            if (acc.Id == _currentSelectedAccountId) continue;
            if (acc.Server != srcServer) continue;

            var accSettings = _accountSettingsService.Load(acc.Id);
            var bossSettings = accSettings.Boss ?? new Panel.Models.BossFeatureSettings();

            // Chỉ đồng bộ nếu acc đó đã tích Liên kết
            if (!bossSettings.EnableSyncCoordinator) continue;

            // Copy 3 thông số + cờ LimitMap/LimitZone + EnableSyncCoordinator
            bossSettings.MapRanges = src.MapRanges;
            bossSettings.ZoneRanges = src.ZoneRanges;
            bossSettings.BossNames = src.BossNames;
            bossSettings.LimitMap = src.LimitMap;
            bossSettings.LimitZone = src.LimitZone;
            bossSettings.EnableSyncCoordinator = true;

            accSettings.Boss = bossSettings;
            _accountSettingsService.Save(acc.Id, accSettings);

            // Gửi command xuống client nếu đang online
            SendBossSettingsCommand(acc.Id, bossSettings);
        }

        // Đồng bộ cả acc nguồn nếu chưa tích
        if (!src.EnableSyncCoordinator)
        {
            src.EnableSyncCoordinator = true;
            _currentSettings.Boss = src;
            _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);
            SendBossSettingsCommand(_currentSelectedAccountId, src);
            // Cập nhật lại UI (không trigger loop)
            _autoBossControl.ApplySettings(src);
        }

        System.Diagnostics.Debug.WriteLine($"[Boss] Đồng bộ thông số từ Acc {_currentSelectedAccountId} sang các acc cùng server {srcServer} đã tích Liên kết.");
    }

    private void SendBossSettingsCommand(int accountId, BossFeatureSettings settings)
    {
        _bossCoordinator.SyncSettingsToClient(accountId, settings, settings.Enabled);
    }

    private void SendDauThanSettingsCommand(int accountId, DauThanSettings settings)
    {
        // Phân tách command
        _socketServer.SendCommand(accountId, "DAUTHAN_REQUEST|" + settings.AutoRequest + "|" + settings.RequestCondition + "|" + settings.RequestIfUnder);
        _socketServer.SendCommand(accountId, "DAUTHAN_DONATE|" + settings.AutoDonate + "|" + settings.DonateFilter + "|" + settings.DonateNames.Replace("\r\n", ","));
        _socketServer.SendCommand(accountId, "DAUTHAN_BUFF|" + settings.AutoBuffMaster + "|" + settings.MasterHpUnder + "|" + settings.MasterKiUnder + "|" + settings.AutoBuffPet + "|" + settings.PetHpUnder + "|" + settings.PetKiUnder);
    }



    private void PetSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        var petSettings = petControl.GetSettings();
        _currentSettings.Pet = petSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        SendPetSettingsCommand(_currentSelectedAccountId, petSettings);
    }

    private void SendPetSettingsCommand(int accountId, PetFeatureSettings settings)
    {
        string cmd = $"PET|{(settings.EnableAutoPet ? 1 : 0)}|{(settings.AutoPemWhenPetCall ? 1 : 0)}|{(settings.AutoKOK ? 1 : 0)}|" +
                     $"{(settings.AutoTTNL ? 1 : 0)}|{settings.TTNLPercent}|{(settings.AutoHealing ? 1 : 0)}|{(settings.AutoFocusPet ? 1 : 0)}|" +
                     $"{(settings.AutoGobackMap ? 1 : 0)}|{settings.TargetMapId}|{(settings.AutoGobackZone ? 1 : 0)}|{settings.TargetZoneId}|" +
                     $"{(settings.AutoGobackPosition ? 1 : 0)}|{settings.TargetX}|{settings.TargetY}|" +
                     $"{(settings.AutoStopAtPower ? 1 : 0)}|{settings.TargetPower}|" +
                     $"{(settings.AutoJump ? 1 : 0)}|{(settings.AutoUsePetBuff ? 1 : 0)}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void PetControl_RequestGetLocation(object? sender, EventArgs e)
    {
        if (_currentSelectedAccountId <= 0) return;
        _pendingPositionRequestSource = PositionRequestSource.Pet;
        _socketServer.SendCommand(_currentSelectedAccountId, "GET_POS");
    }

    private void BuffNamekControl_RequestGetPosition(object? sender, EventArgs e)
    {
        if (_currentSelectedAccountId <= 0) return;
        _pendingPositionRequestSource = buffNamekControl.IsReducePowerTabSelected
            ? PositionRequestSource.ReducePower
            : PositionRequestSource.BuffNamek;
        _socketServer.SendCommand(_currentSelectedAccountId, "GET_POS");
    }

    private void BuffNamekSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentBuffNamekSettings = buffNamekControl.GetSettings();
        _currentReducePowerSettings = buffNamekControl.GetReducePowerSettings();

        // Enforce cứng vai trò để tránh user cấu hình conflict.
        if (_currentReducePowerSettings.Enabled)
        {
            _currentBuffNamekSettings.Enabled = false;
        }
        else if (_currentBuffNamekSettings.Enabled)
        {
            _currentReducePowerSettings.Enabled = false;
            _currentBuffNamekSettings.BuffTargetMode = 2;
        }

        _currentSettings.BuffNamek = _currentBuffNamekSettings;
        _currentSettings.ReducePower = _currentReducePowerSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        // Đồng bộ lại UI sau khi enforce.
        _isBindingData = true;
        try
        {
            buffNamekControl.ApplySettings(_currentBuffNamekSettings);
            buffNamekControl.ApplyReducePowerSettings(_currentReducePowerSettings);
        }
        finally
        {
            _isBindingData = false;
        }

        SendBuffNamekSettingsCommand(_currentSelectedAccountId, _currentBuffNamekSettings);
        SendReducePowerSettingsCommand(_currentSelectedAccountId, _currentReducePowerSettings);
    }

    private void SendBuffNamekSettingsCommand(int accountId, BuffNamekFeatureSettings settings)
    {
        string targetNamesRaw = (settings.TargetNames ?? string.Empty)
            .Replace("\r\n", "\\n")
            .Replace("\n", "\\n")
            .Replace("|", " ");

        string cmd =
            $"BUFF_NAMEK|{(settings.Enabled ? 1 : 0)}|{settings.MapId}|{(settings.RequireZone ? 1 : 0)}|{settings.ZoneId}" +
            $"|{(settings.RequirePosition ? 1 : 0)}|{settings.PosX}|{settings.PosY}|{settings.SkillId}" +
            $"|{settings.BuffTargetMode}|{settings.BuffCondition}|{settings.HpThreshold}|{settings.BuffRangeMode}|{targetNamesRaw}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void SendReducePowerSettingsCommand(int accountId, ReducePowerFeatureSettings settings)
    {
        int provokeMobCount = settings.ProvokeMobCount < 0 ? 0 : settings.ProvokeMobCount;
        int deadReportDelayMs = settings.DeadReportDelayMs < 0 ? 0 : settings.DeadReportDelayMs;
        string cmd = $"REDUCE_POWER|{(settings.Enabled ? 1 : 0)}|{settings.MapId}|{settings.ZoneId}|{settings.PosX}|{settings.PosY}|{provokeMobCount}|{deadReportDelayMs}|{(settings.AutoPunchBlackFlag ? 1 : 0)}|{(settings.UseHpPunch ? 1 : 0)}|{settings.PunchHpPercent}|{(settings.UseTdlt ? 1 : 0)}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void TrainAdvancedSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentTrainSettings.AstarStepSize = (int)nudAstarStep.Value;
        _currentTrainSettings.AstarDelay = (int)nudAstarDelay.Value;

        if (chkAttackHpAbove != null)
        {
            _currentTrainSettings.AttackHpAbove = chkAttackHpAbove.Checked;
            _currentTrainSettings.AttackHpAboveValue = (int)nudAttackHpAboveValue.Value;
            _currentTrainSettings.AttackHpBelow = chkAttackHpBelow.Checked;
            _currentTrainSettings.AttackHpBelowValue = (int)nudAttackHpBelowValue.Value;
            _currentTrainSettings.RotateZone = chkRotateZone.Checked;
            _currentTrainSettings.RotateZoneList = txtRotateZoneList.Text;
            _currentTrainSettings.AutoBuyThoiVang = chkAutoBuyThoiVang.Checked;

            // KS Vang
            if (rdoKsVangZoneLeast != null)
            {
                _currentTrainSettings.OptimizeKsVang = chkOptimizeKsVang.Checked;
                _currentTrainSettings.KsVangAutoZoneMode = rdoKsVangZoneMost.Checked ? 1 : 0;
                _currentTrainSettings.KsVangAutoZoneTrigger = rdoKsVangTriggerTime.Checked ? 1 : 0;
                _currentTrainSettings.KsVangAutoZoneTimeMin = (int)nudKsVangTimeMin.Value;
                _currentTrainSettings.KsVangFilterPlayer = chkKsVangFilterPlayer.Checked;
                _currentTrainSettings.KsVangPlayerMin = (int)nudKsVangPlayerMin.Value;
                _currentTrainSettings.KsVangPlayerMax = (int)nudKsVangPlayerMax.Value;
                if (chkKsVangAvoidChars != null)
                {
                    _currentTrainSettings.KsVangAvoidChars = chkKsVangAvoidChars.Checked;
                    _currentTrainSettings.KsVangAvoidCharsList = txtKsVangAvoidCharsList.Text;
                }
            }
            _currentTrainSettings.BuyThoiVangMinGold = (long)nudBuyThoiVangMinGold.Value;
        }

        _currentSettings.Train = _currentTrainSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        SendTrainAdvancedCommand(_currentSelectedAccountId, _currentTrainSettings);
    }

    private void SendTrainSettingsCommand(int accountId, TrainFeatureSettings settings)
    {
        string cmd =
            $"TRAIN|{settings.MapId}|{settings.RequireZone}|{settings.ZoneId}|{settings.UseTDLT}" +
            $"|{settings.OnlyUsePunch}|{false}|{settings.AvoidSuperMob}" +
            $"|{settings.MobTargetType}|{settings.ChangeLowPlayerZoneIfNoMob}|{settings.CheckLagMob}" +
            $"|{settings.TrainingArmorMode}|{settings.FreezePunchSkillCd}|{settings.MobIds}" +
            $"|{settings.SkillEarthDragon}|{settings.SkillEarthKame}|{settings.SkillEarthTdhs}|{settings.SkillEarthThoiMien}|{settings.SkillEarthDctt}|{settings.SkillEarthKhien}|{settings.SkillEarthKaioken}" +
            $"|{settings.SkillNamekLienHoan}|{settings.SkillNamekDemon}|{settings.SkillNamekMakan}|{settings.SkillNamekDeTrung}|{settings.SkillNamekKhien}" +
            $"|{settings.SkillSaiyanGalick}|{settings.SkillSaiyanAntomic}|{settings.SkillSaiyanBienHinh}|{settings.SkillSaiyanTtNl}|{settings.SkillSaiyanKhien}" +
            $"|{settings.UseShieldUnderHp}|{settings.ShieldHpPercent}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private readonly Dictionary<int, bool> _lastMvbtSentState = new();
    private readonly Dictionary<int, bool> _lastMhbtSentState = new();
    private readonly Dictionary<int, bool> _lastKilisSentState = new();

    private bool IsTimeSlotActive(bool enabled, int startHour, int startMin, int stopHour, int stopMin)
    {
        if (!enabled) return false;
        var now = TimeHelper.GetRealTime();
        var start = new DateTime(now.Year, now.Month, now.Day, startHour, startMin, 0);
        var stop = new DateTime(now.Year, now.Month, now.Day, stopHour, stopMin, 0);

        if (stop <= start) 
            stop = stop.AddDays(1); // Xuyên đêm (VD: 22:00 -> 02:00)

        if (now < start && start.Subtract(now).TotalHours > 12)
            now = now.AddDays(1);

        return now >= start && now <= stop;
    }

    private bool IsTimeSlotActive(KilisFeatureSettings settings)
    {
        return IsTimeSlotActive(settings.Enabled, settings.StartHour, settings.StartMin, settings.StopHour, settings.StopMin);
    }

    private bool IsTimeSlotActive(MvbtFeatureSettings settings)
    {
        return IsTimeSlotActive(settings.Enabled, settings.StartHour, settings.StartMin, settings.StopHour, settings.StopMin);
    }

    private bool IsMvbtActive(int accountId, MvbtFeatureSettings settings)
    {
        if (settings == null) return false;
        int farmedMvbt = _latestFarmedMvbt.TryGetValue(accountId, out var f) ? f : 0;
        bool isGoalReached = farmedMvbt >= settings.TargetCount;
        return IsTimeSlotActive(settings) && !isGoalReached;
    }

    private bool IsMhbtActive(int accountId, MvbtFeatureSettings settings)
    {
        if (settings == null) return false;
        int farmedMhbt = _latestFarmedMhbt.TryGetValue(accountId, out var f) ? f : 0;
        bool isGoalReached = farmedMhbt >= settings.TargetCount;
        return IsTimeSlotActive(settings) && !isGoalReached;
    }

    private void TimeSyncTimer_Tick(object? sender, EventArgs e)
    {
        var connectedIds = _socketServer.GetConnectedAccountIds();
        if (connectedIds.Count == 0) return;

        foreach (int accountId in connectedIds)
        {
            var settings = _accountSettingsService.Load(accountId);
            if (settings == null) continue;

            if (settings.Mvbt != null && settings.Mvbt.Enabled)
            {
                bool actualActive = IsMvbtActive(accountId, settings.Mvbt);
                _lastMvbtSentState.TryGetValue(accountId, out bool lastState);
                if (actualActive != lastState)
                {
                    SendMvbtSettingsCommand(accountId, settings.Mvbt);
                }
            }

            if (settings.Mhbt != null && settings.Mhbt.Enabled)
            {
                bool actualActive = IsMhbtActive(accountId, settings.Mhbt);
                _lastMhbtSentState.TryGetValue(accountId, out bool lastState);
                if (actualActive != lastState)
                {
                    SendMhbtSettingsCommand(accountId, settings.Mhbt);
                }
            }

            if (settings.Kilis != null && settings.Kilis.Enabled)
            {
                bool actualActive = IsTimeSlotActive(settings.Kilis);
                _lastKilisSentState.TryGetValue(accountId, out bool lastState);
                if (actualActive != lastState)
                {
                    SendKilisSettingsCommand(accountId, settings.Kilis);
                }
            }
        }
    }

    private void SendMvbtSettingsCommand(int accountId, MvbtFeatureSettings settings)
    {
        if (settings == null) return;
        bool actualActive = IsMvbtActive(accountId, settings);
        _lastMvbtSentState[accountId] = actualActive;

        string cmd = $"MVBT_SETTING|{(actualActive?1:0)}|{settings.StartHour}|{settings.StartMin}|{settings.StopHour}|{settings.StopMin}" +
                     $"|{settings.MapId}|{(settings.RequireZone?1:0)}|{settings.ZoneId}|{(settings.UseTDLT?1:0)}" +
                     $"|{0}|{0}|{(settings.AvoidSuperMob?1:0)}|{settings.MobTargetType}|{(settings.ChangeLowPlayerZoneIfNoMob?1:0)}" +
                     $"|{(settings.CheckLagMob?1:0)}|{settings.TrainingArmorMode}|{0}|{settings.TargetCount}|{settings.MobIds}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void SendMhbtSettingsCommand(int accountId, MvbtFeatureSettings settings)
    {
        if (settings == null) return;
        bool actualActive = IsMhbtActive(accountId, settings);
        _lastMhbtSentState[accountId] = actualActive;

        string cmd = $"MHBT_SETTING|{(actualActive?1:0)}|{settings.StartHour}|{settings.StartMin}|{settings.StopHour}|{settings.StopMin}" +
                     $"|{settings.MapId}|{(settings.RequireZone?1:0)}|{settings.ZoneId}|{(settings.UseTDLT?1:0)}" +
                     $"|{0}|{0}|{(settings.AvoidSuperMob?1:0)}|{settings.MobTargetType}|{(settings.ChangeLowPlayerZoneIfNoMob?1:0)}" +
                     $"|{(settings.CheckLagMob?1:0)}|{settings.TrainingArmorMode}|{0}|{settings.TargetCount}|{settings.MobIds}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void SendKilisSettingsCommand(int accountId, KilisFeatureSettings settings)
    {
        if (settings == null) return;
        bool actualActive = IsTimeSlotActive(settings);
        _lastKilisSentState[accountId] = actualActive;

        string payload = string.Join("|",
            actualActive ? 1 : 0,
            settings.StartHour,
            settings.StartMin,
            settings.StopHour,
            settings.StopMin,
            settings.ZoneId,
            settings.AutoBuyAmulet ? 1 : 0,
            settings.AmuletType,
            settings.UseTDLT ? 1 : 0,
            settings.AutoZone ? 1 : 0,
            settings.TrainingArmorMode
        );
        _socketServer.SendCommand(accountId, $"KILIS_SETTING|{payload}");
    }

    private void SendBossVegetaCitySettingsCommand(int accountId, BossVegetaCityFeatureSettings settings)
    {
        settings ??= new BossVegetaCityFeatureSettings();
        var train = _currentSelectedAccountId == accountId
            ? _currentTrainSettings
            : (_accountSettingsService.Load(accountId).Train ?? new TrainFeatureSettings());

        string cmd = string.Join("|",
            "BOSS_VEGETA_CITY_SETTING",
            settings.Enabled ? 1 : 0,
            settings.Auto15h ? 1 : 0,
            settings.Auto2230 ? 1 : 0,
            settings.ReviveByGem ? 1 : 0,
            settings.UseTdlt ? 1 : 0,
            train.TrainingArmorMode,
            train.FreezePunchSkillCd ? 1 : 0,
            train.UseShieldUnderHp ? 1 : 0,
            train.ShieldHpPercent,
            train.SkillEarthDragon ? 1 : 0,
            train.SkillEarthKame ? 1 : 0,
            train.SkillEarthTdhs ? 1 : 0,
            train.SkillEarthThoiMien ? 1 : 0,
            train.SkillEarthDctt ? 1 : 0,
            train.SkillEarthKhien ? 1 : 0,
            train.SkillEarthKaioken ? 1 : 0,
            train.SkillNamekLienHoan ? 1 : 0,
            train.SkillNamekDemon ? 1 : 0,
            train.SkillNamekMakan ? 1 : 0,
            train.SkillNamekDeTrung ? 1 : 0,
            train.SkillNamekKhien ? 1 : 0,
            train.SkillSaiyanGalick ? 1 : 0,
            train.SkillSaiyanAntomic ? 1 : 0,
            train.SkillSaiyanBienHinh ? 1 : 0,
            train.SkillSaiyanTtNl ? 1 : 0,
            train.SkillSaiyanKhien ? 1 : 0
        );

        _socketServer.SendCommand(accountId, cmd);
    }

    private void SendTrainAdvancedCommand(int accountId, TrainFeatureSettings settings)
    {
        string rotateZoneListStr = (settings.RotateZoneList ?? "").Replace("\r\n", ";").Replace("\n", ";");
        string cmd = $"TRAIN_ADVANCED|{settings.AstarStepSize}|{settings.AstarDelay}" +
                     $"|{(settings.AttackHpAbove ? 1 : 0)}|{settings.AttackHpAboveValue}" +
                     $"|{(settings.AttackHpBelow ? 1 : 0)}|{settings.AttackHpBelowValue}" +
                     $"|{(settings.RotateZone ? 1 : 0)}|{rotateZoneListStr}" +
                     $"|{(settings.AutoBuyThoiVang ? 1 : 0)}|{settings.BuyThoiVangMinGold}" +
                     $"|{(settings.UsePrivateTicket ? 1 : 0)}|{(settings.OptimizeKsVang ? 1 : 0)}" +
                     $"|{settings.KsVangAutoZoneMode}|{settings.KsVangAutoZoneTrigger}|{settings.KsVangAutoZoneTimeMin}" +
                     $"|{(settings.KsVangFilterPlayer ? 1 : 0)}|{settings.KsVangPlayerMin}|{settings.KsVangPlayerMax}" +
                     $"|{(settings.KsVangAvoidChars ? 1 : 0)}|{(settings.KsVangAvoidCharsList ?? "").Replace("\r\n", ";").Replace("\n", ";")}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void SendTrainOffCommand(int accountId)
    {
        string cmd = $"TRAIN_OFF";
        _socketServer.SendCommand(accountId, cmd);
    }
    private void DropSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentItemSettings.AutoDrop = chkAutoDrop.Checked;
        _currentItemSettings.DropByHsd = chkDropByHsd.Checked;
        _currentItemSettings.DropIds = txtDropIds.Text;

        _currentSettings.Item = _currentItemSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        SendDropSettingsCommand(_currentSelectedAccountId, _currentItemSettings);
    }

    private void StoreSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentItemSettings.AutoStoreWhenFull = chkAutoStoreWhenFull.Checked;
        _currentItemSettings.StoreKichHoat = chkStoreKichHoat.Checked;
        _currentItemSettings.StoreThanLinh = chkStoreThanLinh.Checked;
        _currentItemSettings.StorePhaLe = chkStorePhaLe.Checked;
        _currentItemSettings.StoreStarCount = (int)nudStoreStarCount.Value;
        _currentItemSettings.StoreCustom = chkStoreCustom.Checked;
        _currentItemSettings.StoreCustomList = txtStoreCustomList.Text;

        _currentSettings.Item = _currentItemSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        SendStoreSettingsCommand(_currentSelectedAccountId, _currentItemSettings);
    }

    private void SendStoreSettingsCommand(int accountId, ItemSettings settings)
    {
        string customList = settings.StoreCustomList?.Replace("\r\n", ";")?.Replace("\n", ";") ?? "";
        string cmd = $"STORE|{(settings.AutoStoreWhenFull ? 1 : 0)}|{(settings.StoreKichHoat ? 1 : 0)}|{(settings.StoreThanLinh ? 1 : 0)}|{(settings.StorePhaLe ? 1 : 0)}|{settings.StoreStarCount}|{(settings.StoreCustom ? 1 : 0)}|{customList}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void SellSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentItemSettings.AutoSellTrash           = chkAutoSellTrash.Checked;
        _currentItemSettings.SellWhenEmptySlots      = (int)nudSellEmptySlots.Value;
        _currentItemSettings.DropInsteadOfSell       = chkDropInsteadOfSell.Checked;
        _currentItemSettings.KeepStarItems           = chkKeepStarItems.Checked;
        _currentItemSettings.KeepGodItems            = chkKeepGodItems.Checked;
        _currentItemSettings.KeepSkhItems            = chkKeepSkhItems.Checked;
        _currentItemSettings.SellMaxLevel            = (int)nudSellMaxLevel.Value;
        _currentItemSettings.SellKeepIds             = txtSellKeepIds.Text;
        _currentItemSettings.SellCustomNoStarCheck   = chkSellCustomNoStarCheck.Checked;
        _currentItemSettings.SellCustomIdsList       = txtSellCustomIdsList.Text;

        _currentSettings.Item = _currentItemSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        SendSellSettingsCommand(_currentSelectedAccountId, _currentItemSettings);
    }

    private void SendSellSettingsCommand(int accountId, ItemSettings settings)
    {
        if (settings == null) return;
        // Format: SELL_SETTING|enabled|emptySlots|keepStar|keepGod|keepSkh|sellMaxLevel|keepIds|forceSellIds|dropInsteadOfSell
        string keepIds      = (settings.SellKeepIds      ?? "").Replace("\r\n", ";").Replace("\n", ";");
        string forceSellIds = (settings.SellCustomIdsList ?? "").Replace("\r\n", ";").Replace("\n", ";");
        string cmd = $"SELL_SETTING" +
                     $"|{(settings.AutoSellTrash  ? 1 : 0)}" +
                     $"|{settings.SellWhenEmptySlots}" +
                     $"|{(settings.KeepStarItems  ? 1 : 0)}" +
                     $"|{(settings.KeepGodItems   ? 1 : 0)}" +
                     $"|{(settings.KeepSkhItems   ? 1 : 0)}" +
                     $"|{settings.SellMaxLevel}" +
                     $"|{keepIds}" +
                     $"|{forceSellIds}" +
                     $"|{(settings.DropInsteadOfSell ? 1 : 0)}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void BtnBuyCustomHelp_Click(object? sender, EventArgs e)
    {
        string helpMsg = @"Hướng dẫn cấu hình Mua Custom (Tối đa 7 tham số):
ID_Item | SL | MapID | NpcID | Loại_Tiền | Kiểu_Mua | Menu (Tùy chọn)

1. ID_Item: ID vật phẩm (VD: Cỏ 4 lá=1635, Khẩu trang=764).
2. SL: Mục tiêu số lượng cần mua. (Lưu ý: Bot chỉ mua thêm khi túi ĐÃ HẾT SẠCH, tức SL=0).
3. MapID: Chuyển tới Map chứa NPC.
4. NpcID: Id của NPC bán đồ.
5. Loại_Tiền: Món đồ này thanh toán bằng gì?
   - Nhập 0 = Mua bằng VÀNG .
   - Nhập 1 = Mua bằng NGỌC .
6. Kiểu_Mua: Cách bot tương tác với ô mua:
   - Nhập 0 = Bấm nút [Mua] lẻ. (Bot auto-spam click mua từng cái, cách nhau dãn cách an toàn 0.5s đến khi đủ SL). Dành cho Khẩu trang, Thỏi vàng...
   - Nhập 1 = Có ô [Mua Nhiều]. (Bot nhập 1 phát toàn bộ SL vào ô điền chữ). Dành cho Cỏ 4 lá, Bùa Santa...
7. Menu (Tùy chọn):
   - Mặc định: Ghi các thứ tự Menu chọn (bắt đầu từ 0), cách nhau bằng dấu phẩy.
   - Dành cho NPC như Uron (bấm thẳng ra shop, KHÔNG CÓ MENU): BỎ TRỐNG, KHÔNG GHI GÌ CẢ.

VÍ DỤ CỤ THỂ
VD 1 - NPC Phức Tạp (Mua 99 Cỏ 4 Lá (1635), bằng Ngọc (1), Mua Nhiều (1), Tab Cửa Hàng (0)): 
1635 | 99 | 5 | 39 | 1 | 1 | 0

VD 2 - Spam Mua lẻ (Mua 99 Khẩu trang (764), bằng Ngọc (1), Mua Lẻ (0), Tab Hỗ Trợ (1)):
764 | 99 | 5 | 39 | 1 | 0 | 1

VD 3 - NPC Không Menu như URON (Bỏ trống tham số 7):
1234 | 99 | 24 | 14 | 0 | 1";

        MessageBox.Show(helpMsg, "Hướng dẫn cấu hình Auto Mua Custom", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BuySettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentItemSettings.AutoBuyTdlt = cbAutoBuyTdlt.Checked;
        _currentItemSettings.AutoBuyPrivateTicket = cbAutoBuyPrivateTicket.Checked;
        _currentItemSettings.AutoBuyKhauTrang = cbAutoBuyKhauTrang.Checked;
        _currentItemSettings.BuyKhauTrangQty = (int)numBuyKhauTrangQty.Value;
        _currentItemSettings.AutoBuyCoBonLa = cbAutoBuyCoBonLa.Checked;
        _currentItemSettings.BuyCoBonLaQty = (int)numBuyCoBonLaQty.Value;
        _currentItemSettings.AutoBuyBuaDe = cbAutoBuyBuaDe.Checked;
        _currentItemSettings.BuyBuaDeQty = (int)numBuyBuaDeQty.Value;
        _currentItemSettings.AutoBuyCustom = chkAutoBuyCustom.Checked;
        _currentItemSettings.BuyCustomList = txtBuyCustomList.Text;

        _currentSettings.Item = _currentItemSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        SendBuySettingsCommand(_currentSelectedAccountId, _currentItemSettings);
        upSkhControl.ApplySettings(_currentTrainSettings, _currentItemSettings);
    }

    private void SendBuySettingsCommand(int accountId, ItemSettings settings)
    {
        if (settings == null) return;
        string cmd = $"BUY_SETTING|{(settings.AutoBuyTdlt ? 1 : 0)}|{(settings.AutoBuyKhauTrang ? 1 : 0)}|{settings.BuyKhauTrangQty}|{(settings.AutoBuyCoBonLa ? 1 : 0)}|{settings.BuyCoBonLaQty}|{(settings.AutoBuyBuaDe ? 1 : 0)}|{settings.BuyBuaDeQty}|{(settings.AutoBuyPrivateTicket ? 1 : 0)}";
        _socketServer.SendCommand(accountId, cmd);
        
        string safeList = settings.BuyCustomList?.Replace("\r\n", ";").Replace("\n", ";").Replace("|", ",") ?? "";
        string customCmd = $"BUY_CUSTOM_SETTING|{(settings.AutoBuyCustom ? 1 : 0)}|{safeList}";
        _socketServer.SendCommand(accountId, customCmd);
    }

    private void PickSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentItemSettings.AutoPick = chkAutoPick.Checked;
        _currentItemSettings.PickMode = cboPickMode.SelectedIndex < 0 ? 0 : cboPickMode.SelectedIndex;
        _currentItemSettings.OnlyMyItems = chkOnlyMyItems.Checked;
        _currentItemSettings.PickIdsList = txtPickIdsList.Text;
        _currentItemSettings.PickBlackList = txtPickBlackList.Text;

        _currentSettings.Item = _currentItemSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        SendPickSettingsCommand(_currentSelectedAccountId, _currentItemSettings);
    }

    private void SendPickSettingsCommand(int accountId, ItemSettings settings)
    {
        if (settings == null) return;
        string whiteList = (settings.PickIdsList ?? "").Replace("\r\n", ";").Replace("\n", ";");
        string blackList = (settings.PickBlackList ?? "").Replace("\r\n", ";").Replace("\n", ";");
        string cmd = $"PICK_SETTING|{(settings.AutoPick ? 1 : 0)}|{settings.PickMode}|{(settings.OnlyMyItems ? 1 : 0)}|{settings.PickDistance}|{whiteList}|{blackList}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void UseItemSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentItemSettings.UseCuongNo = chkUseCuongNo.Checked;
        _currentItemSettings.UseBoHuyet = chkUseBoHuyet.Checked;
        _currentItemSettings.UseBoKhi = chkUseBoKhi.Checked;
        _currentItemSettings.UseGiapXen = chkUseGiapXen.Checked;
        _currentItemSettings.UseMask = chkUseMask.Checked;
        _currentItemSettings.Use4LeafClover = chkUse4LeafClover.Checked;
        _currentItemSettings.UseFood = chkUseFood.Checked;
        _currentItemSettings.UseDetector = chkUseDetector.Checked;
        _currentItemSettings.UseItemById = chkUseItemById.Checked;
        _currentItemSettings.ItemByIds = txtItemByIds.Text.Trim();

        _currentSettings.Item = _currentItemSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        SendUseItemSettingsCommand(_currentSelectedAccountId, _currentItemSettings);
        upSkhControl.ApplySettings(_currentTrainSettings, _currentItemSettings);
    }

    private void SendUseItemSettingsCommand(int accountId, ItemSettings settings)
    {
        if (settings == null) return;
        string itemByIdsStr = settings.UseItemById
            ? (settings.ItemByIds ?? "").Replace("\r\n", ";").Replace("\n", ";")
            : "";
        string cmd = $"USE_ITEM_SETTING|{(settings.UseCuongNo ? 1 : 0)}|{(settings.UseBoHuyet ? 1 : 0)}" +
                     $"|{(settings.UseBoKhi ? 1 : 0)}|{(settings.UseGiapXen ? 1 : 0)}" +
                     $"|{(settings.UseMask ? 1 : 0)}|{(settings.Use4LeafClover ? 1 : 0)}" +
                     $"|{(settings.UseFood ? 1 : 0)}|{(settings.UseDetector ? 1 : 0)}" +
                     $"|{itemByIdsStr}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void SendDropSettingsCommand(int accountId, ItemSettings settings)
    {
        if (settings == null) return;
        // Encode newline thành \n để truyền qua socket dạng 1 dòng, client sẽ replace lại
        string dropIds = (settings.DropIds ?? "").Replace("\r\n", "\\n").Replace("\n", "\\n");
        string cmd = $"DROP_SETTING|{(settings.AutoDrop ? 1 : 0)}|{(settings.DropByHsd ? 1 : 0)}|{dropIds}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void SupportSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentSupportSettings.BongTaiState      = cboBongTaiState.SelectedIndex < 0 ? 0 : cboBongTaiState.SelectedIndex;
        _currentSupportSettings.BongTaiPetAction  = cboBongTaiPetAction.SelectedIndex < 0 ? 3 : cboBongTaiPetAction.SelectedIndex;
        _currentSupportSettings.AutoCoDen         = chkAutoCoDen.Checked;
        _currentSupportSettings.DisableCoDenIfOthers = chkDisableCoDenIfOthers.Checked;
        _currentSupportSettings.FlagType          = cboFlagType.SelectedIndex < 0 ? 8 : cboFlagType.SelectedIndex;

        _currentSettings.Support = _currentSupportSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        SendSupportSettingsCommand(_currentSelectedAccountId, _currentSupportSettings);
    }

    private void AutoPointSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        var settings = new Models.AutoPointFeatureSettings
        {
            AddHP = chkAutoPointHP.Checked,
            TargetHP = (int)numAutoPointHP.Value,
            AddMP = chkAutoPointMP.Checked,
            TargetMP = (int)numAutoPointMP.Value,
            AddDamage = chkAutoPointDamage.Checked,
            TargetDamage = (int)numAutoPointDamage.Value
        };

        _currentSettings.AutoPoint = settings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        SendAutoPointSettingsCommand(_currentSelectedAccountId, settings);
    }

    private void SendAutoPointSettingsCommand(int accountId, Models.AutoPointFeatureSettings settings)
    {
        if (settings == null) return;
        string cmd = $"AUTO_POINT_SETTING|{(settings.AddHP ? 1 : 0)}|{settings.TargetHP}|{(settings.AddMP ? 1 : 0)}|{settings.TargetMP}|{(settings.AddDamage ? 1 : 0)}|{settings.TargetDamage}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void AutoAmuletSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentAutoAmuletSettings = GetAutoAmuletSettingsFromUi();
        _currentSettings.AutoAmulet = _currentAutoAmuletSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        SendAutoAmuletSettingsCommand(_currentSelectedAccountId, _currentAutoAmuletSettings);
    }

    private AutoAmuletSettings GetAutoAmuletSettingsFromUi()
    {
        var settings = new AutoAmuletSettings
        {
            Enabled = chkAutoAmuletEnabled.Checked,
            DurationMode = cmbAutoAmuletDuration.SelectedIndex < 0 ? 0 : cmbAutoAmuletDuration.SelectedIndex,
            Wisdom = chkAmuletWisdom.Checked,
            Strong = chkAmuletStrong.Checked,
            BuffaloSkin = chkAmuletBuffaloSkin.Checked,
            Heroic = chkAmuletHeroic.Checked,
            Immortal = chkAmuletImmortal.Checked,
            Enduring = chkAmuletEnduring.Checked,
            Magnet = chkAmuletMagnet.Checked,
            Disciple = chkAmuletDisciple.Checked,
            WisdomX3 = chkAmuletWisdomX3.Checked,
            WisdomX4 = chkAmuletWisdomX4.Checked
        };
        return settings;
    }

    private void ApplyAutoAmuletSettingsToUi(AutoAmuletSettings settings)
    {
        if (settings == null || chkAutoAmuletEnabled == null) return;

        chkAutoAmuletEnabled.Checked = settings.Enabled;
        cmbAutoAmuletDuration.SelectedIndex = settings.DurationMode >= 0 && settings.DurationMode < cmbAutoAmuletDuration.Items.Count
            ? settings.DurationMode
            : 0;
        chkAmuletWisdom.Checked = settings.Wisdom;
        chkAmuletStrong.Checked = settings.Strong;
        chkAmuletBuffaloSkin.Checked = settings.BuffaloSkin;
        chkAmuletHeroic.Checked = settings.Heroic;
        chkAmuletImmortal.Checked = settings.Immortal;
        chkAmuletEnduring.Checked = settings.Enduring;
        chkAmuletMagnet.Checked = settings.Magnet;
        chkAmuletDisciple.Checked = settings.Disciple;
        chkAmuletWisdomX3.Checked = settings.WisdomX3;
        chkAmuletWisdomX4.Checked = settings.WisdomX4;
    }

    private void SendAutoAmuletSettingsCommand(int accountId, AutoAmuletSettings settings)
    {
        if (settings == null) return;

        string cmd = $"AUTO_AMULET_SETTING|{(settings.Enabled ? 1 : 0)}|{settings.DurationMode}" +
                     $"|{(settings.Wisdom ? 1 : 0)}|{(settings.Strong ? 1 : 0)}|{(settings.BuffaloSkin ? 1 : 0)}" +
                     $"|{(settings.Heroic ? 1 : 0)}|{(settings.Immortal ? 1 : 0)}|{(settings.Enduring ? 1 : 0)}" +
                     $"|{(settings.Magnet ? 1 : 0)}|{(settings.Disciple ? 1 : 0)}|{(settings.WisdomX3 ? 1 : 0)}|{(settings.WisdomX4 ? 1 : 0)}";
        _socketServer.SendCommand(accountId, cmd);
    }

    private void SendSupportSettingsCommand(int accountId, SupportSettings settings)
    {
        if (settings == null) return;
        string cmd = $"SUPPORT_SETTING|{settings.BongTaiState}|{settings.BongTaiPetAction}" +
                     $"|{(settings.AutoCoDen ? 1 : 0)}|{(settings.DisableCoDenIfOthers ? 1 : 0)}|{settings.FlagType}";
        _socketServer.SendCommand(accountId, cmd);
    }

    // Mọi feature cần replay sau khi client ONLINE phải đăng ký ở đây.
    // Không thêm lệnh sync rải rác ở SendAllSavedSettingsToClient để tránh sót feature mới.
    private void RegisterSettingsReplayHandlers()
    {
        _settingsReplayHandlers.Clear();

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var appConfig = ConfigManager.Load();
            _socketServer.SendCommand(accountId, $"CAPTCHA_SETTING|{appConfig.CaptchaApiServer}|{appConfig.CaptchaApiKey}");
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var general = settings.General ?? new GeneralSettings();
            SendXmapSettingsCommand(accountId, general);
            SendProxySettingsCommand(accountId, general);
            SendActionOnDeathCommand(accountId, general);
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var train = settings.Train ?? new TrainFeatureSettings();
            if (train.Enabled) SendTrainSettingsCommand(accountId, train);
            else SendTrainOffCommand(accountId);

            SendTrainAdvancedCommand(accountId, train);
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var mvbt = settings.Mvbt ?? new MvbtFeatureSettings();
            SendMvbtSettingsCommand(accountId, mvbt);
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var mhbt = settings.Mhbt ?? new MvbtFeatureSettings();
            SendMhbtSettingsCommand(accountId, mhbt);
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var kilis = settings.Kilis ?? new KilisFeatureSettings();
            SendKilisSettingsCommand(accountId, kilis);
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var bossVegetaCity = settings.BossVegetaCity ?? new BossVegetaCityFeatureSettings();
            SendBossVegetaCitySettingsCommand(accountId, bossVegetaCity);
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var pet = settings.Pet ?? new PetFeatureSettings();
            SendPetSettingsCommand(accountId, pet);
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var buffNamek = settings.BuffNamek ?? new BuffNamekFeatureSettings();
            SendBuffNamekSettingsCommand(accountId, buffNamek);
            var reducePower = settings.ReducePower ?? new ReducePowerFeatureSettings();
            SendReducePowerSettingsCommand(accountId, reducePower);
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var boss = settings.Boss ?? new BossFeatureSettings();
            SendBossSettingsCommand(accountId, boss);
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var dailyQuest = settings.DailyQuest ?? new DailyQuestFeatureSettings();
            SendDailyQuestSettingsCommand(accountId, dailyQuest);
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var attendance = settings.Attendance ?? new AttendanceFeatureSettings();
            SendAttendanceSettingsCommand(accountId, attendance);
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var item = settings.Item ?? new ItemSettings();
            SendDropSettingsCommand(accountId, item);
            SendStoreSettingsCommand(accountId, item);
            SendSellSettingsCommand(accountId, item);
            SendBuySettingsCommand(accountId, item);
            SendPickSettingsCommand(accountId, item);
            SendUseItemSettingsCommand(accountId, item);
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var support = settings.Support ?? new SupportSettings();
            SendSupportSettingsCommand(accountId, support);
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var autoPoint = settings.AutoPoint ?? new AutoPointFeatureSettings();
            SendAutoPointSettingsCommand(accountId, autoPoint);

            var autoAmulet = settings.AutoAmulet ?? new AutoAmuletSettings();
            SendAutoAmuletSettingsCommand(accountId, autoAmulet);
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var upZin = settings.AutoUpZin ?? new AutoUpZinSettings();
            if (upZin.Enabled) SendAutoUpZinCommand(accountId, upZin);
            else SendAutoUpZinOffCommand(accountId);
        });

        _settingsReplayHandlers.Add((accountId, settings) =>
        {
            var upZin700k = settings.UpZin700k ?? new AutoUpZinTo700kSettings();
            if (upZin700k.Enabled) SendUpZin700kCommand(accountId, upZin700k);
            else SendUpZin700kOffCommand(accountId);
        });
    }

    private void SendAllSavedSettingsToClient(int accountId)
    {
        if (accountId <= 0)
        {
            return;
        }

        var settings = _accountSettingsService.Load(accountId);
        foreach (var replayHandler in _settingsReplayHandlers)
        {
            replayHandler(accountId, settings);
        }
    }
    private void UpdateButtonState(DataGridViewRow row)
    {
        string status = row.Cells[colStatus.Index].Value?.ToString() ?? "";
        if (!status.Contains("OFFLINE"))
        {
            btnToggleGame.Text = "Tắt Game";
        }
        else
        {
            btnToggleGame.Text = "Bật Game";
        }
    }

    private void KillAccountRow(DataGridViewRow row)
    {
        int? accountId = row.Tag as int?;
        if (!accountId.HasValue) return;

        int pid = GetProcessIdForAccount(accountId.Value);
        GameLauncher.KillGame(pid);
        _accountRepo.UpdateStatus(accountId.Value, "0. OFFLINE", "---");
        MarkAttendanceOffline(accountId.Value);
        row.Cells[colStatus.Index].Value = "0. OFFLINE";

        if (dgvAccounts.CurrentRow != null && dgvAccounts.CurrentRow.Index == row.Index)
            UpdateButtonState(row);
    }

    // ─── Click vào cell ──────────────────────────────────────────────────

    private void DgvAccounts_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

        var clickedRow = dgvAccounts.Rows[e.RowIndex];
        dgvAccounts.CurrentCell = clickedRow.Cells[e.ColumnIndex];

        // Click lại dòng đang chọn không bắn SelectionChanged, nên phải chủ động reload settings từ DB.
        // Tránh reload ở các cell có handler riêng để không ghi đè thao tác checkbox/combobox/show.
        if (e.ColumnIndex != colSelect.Index
            && e.ColumnIndex != colServer.Index
            && dgvAccounts.Columns[e.ColumnIndex] != colShow
            && dgvAccounts.Columns[e.ColumnIndex] is not DataGridViewComboBoxColumn)
        {
            UpdateButtonState(clickedRow);
            LoadAccountSettings(clickedRow);
        }

        // Hiển thị menu chọn Server
        if (e.ColumnIndex == colServer.Index)
        {
            dgvAccounts.CurrentCell = dgvAccounts.Rows[e.RowIndex].Cells[e.ColumnIndex];
            var cellRectangle = dgvAccounts.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            _serverMenu.Show(dgvAccounts, cellRectangle.Left, cellRectangle.Bottom);
            return;
        }

        // Bật dropdown của combobox ngay lập tức khi click 1 lần
        if (dgvAccounts.Columns[e.ColumnIndex] is DataGridViewComboBoxColumn)
        {
            dgvAccounts.BeginEdit(true);
            if (dgvAccounts.EditingControl is ComboBox comboBox)
            {
                comboBox.DroppedDown = true;
            }
        }

        // Nút SHOW
        if (dgvAccounts.Columns[e.ColumnIndex] == colShow)
        {
            var row = clickedRow;

            // Chọn dòng này và load settings (gọi trực tiếp vì SelectionChanged không fire khi dòng đã được chọn)
            dgvAccounts.CurrentCell = row.Cells[colAccount.Index];
            UpdateButtonState(row);
            LoadAccountSettings(row);

            if (row.Tag is int accountId)
            {
                int pid = GetProcessIdForAccount(accountId);
                if (pid > 0)
                {
                    GameLauncher.ShowGame(pid);
                    _socketServer.SendCommand(accountId, "SET_RENDER|1");
                }
            }
        }
    }

    private void DgvAccounts_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex == colSTT.Index)
        {
            e.Value = (e.RowIndex + 1).ToString();
            e.FormattingApplied = true;
        }
    }

    private void DgvAccounts_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        if (dgvAccounts.IsCurrentCellDirty)
        {
            if (dgvAccounts.CurrentCell is DataGridViewComboBoxCell || dgvAccounts.CurrentCell is DataGridViewCheckBoxCell)
            {
                dgvAccounts.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }
    }

    private void DgvAccounts_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        if (e.ColumnIndex == colSelect.Index)
        {
            if (dgvAccounts.Rows[e.RowIndex].Tag is int accountId)
            {
                bool isSelected = Convert.ToBoolean(dgvAccounts.Rows[e.RowIndex].Cells[colSelect.Index].Value);
                _accountRepo.UpdateAccountSelection(accountId, isSelected);
            }
            UpdateSelectAllButtonState();
            return;
        }

        if (e.ColumnIndex == colServer.Index)
        {
            var row = dgvAccounts.Rows[e.RowIndex];
            if (row.Tag is not int accountId) return;

            string? serverName = row.Cells[colServer.Index].Value?.ToString();
            if (string.IsNullOrWhiteSpace(serverName)) return;

            _accountRepo.UpdateAccountServer(accountId, serverName);
        }
    }

    // ─── Component Update / Select All ───────────────────────────────────
    
    private void BtnSelectAll_Click(object? sender, EventArgs e)
    {
        if (dgvAccounts.Rows.Count == 0) return;

        bool isSelectAll = btnSelectAll.Text == "Select All";
        
        foreach (DataGridViewRow row in dgvAccounts.Rows)
        {
            if (row.Cells[colSelect.Index] is DataGridViewCheckBoxCell checkCell)
            {
                checkCell.Value = isSelectAll;
            }
        }
        btnSelectAll.Text = isSelectAll ? "Unselect All" : "Select All";
    }

    private void BtnSettings_Click(object? sender, EventArgs e)
    {
        using var settingsForm = new GlobalSettingsForm();
        settingsForm.AutoCleanRamSettingsChanged += (s, e2) => ApplyAutoCleanRamConfig();
        if (settingsForm.ShowDialog() == DialogResult.OK)
        {
            var config = ConfigManager.Load();
            _socketServer.Broadcast($"CAPTCHA_SETTING|{config.CaptchaApiServer}|{config.CaptchaApiKey}");
        }
    }

    private void UpdateSelectAllButtonState()
    {
        if (dgvAccounts.Rows.Count == 0)
        {
            btnSelectAll.Text = "Select All";
            return;
        }

        bool allChecked = true;
        foreach (DataGridViewRow row in dgvAccounts.Rows)
        {
            if (row.Cells[colSelect.Index] is DataGridViewCheckBoxCell checkCell)
            {
                bool isChecked = checkCell.Value != null && (bool)checkCell.Value;
                if (!isChecked)
                {
                    allChecked = false;
                    break;
                }
            }
        }

        btnSelectAll.Text = allChecked ? "Unselect All" : "Select All";
    }

    // ─── Launch game ─────────────────────────────────────────────────────

    private void LaunchAccountRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= dgvAccounts.Rows.Count) return;

        var row = dgvAccounts.Rows[rowIndex];
        int? accountId = row.Tag as int?;
        if (!accountId.HasValue) return;

        // Lấy account từ DB để có Password
        var accounts = _accountRepo.GetAllAccounts();
        var acc = accounts.FirstOrDefault(a => a.Id == accountId.Value);
        if (acc == null) return;

        // Lấy server từ combobox trên dòng đó (user có thể đổi)
        string? serverName = row.Cells[colServer.Index].Value?.ToString();
        if (!string.IsNullOrEmpty(serverName))
        {
            acc.Server = serverName;
            _accountRepo.UpdateAccountServer(acc.Id, serverName);
        }

        string customTitle = $"Tab - {rowIndex + 1}";
        int pid = GameLauncher.LaunchGame(acc, chkAutoHideClient.Checked, customTitle);
        if (pid > 0)
        {
            // Cache processId theo accountId
            SetProcessIdForAccount(acc.Id, pid);
            row.Cells[colStatus.Index].Value = "2. LAUNCHING...";

            // Luôn chạy vòng lặp bắt handle để Xếp cửa sổ (tránh đè lên nhau)
            // và Ẩn màn hình (nếu có tùy chọn)
            Task.Run(async () =>
            {
                try
                {
                    var proc = Process.GetProcessById(pid);
                    int retries = 0;
                    IntPtr hwnd = GameLauncher.GetAnyWindowHandle(pid);
                    while (hwnd == IntPtr.Zero && retries < 150) // Tối đa 30s cày handle
                    {
                        await Task.Delay(200);
                        hwnd = GameLauncher.GetAnyWindowHandle(pid);
                        retries++;
                    }
                    if (hwnd != IntPtr.Zero)
                    {
                        // Sắp xếp cửa sổ tràn đều ra màn hình thay vì kẹt ở giữa
                        var currentCfg = ConfigManager.Load();
                        GameLauncher.ArrangeGameWindow(pid, currentCfg.WindowWidth, currentCfg.WindowHeight);

                        // Nếu chọn ẩn thì ẩn ngay sau khi có handle và đã xếp vị trí
                        if (chkAutoHideClient.Checked)
                        {
                            GameLauncher.HideGame(pid);
                        }
                    }
                }
                catch { } // Bỏ qua ngoại lệ nếu tiến trình bị kill sớm
            });
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private bool IsTimeInSchedule(DateTime time, TimeSpan start, TimeSpan end)
    {
        TimeSpan timeOfDay = time.TimeOfDay;
        if (start <= end)
        {
            return timeOfDay >= start && timeOfDay <= end;
        }
        else // spans midnight e.g. 22:00 -> 06:00
        {
            return timeOfDay >= start || timeOfDay <= end;
        }
    }

    private void ChkAutoLogin_CheckedChanged(object? sender, EventArgs e)
    {
        // Timer now runs always. Checkbox only controls launching, not killing.
        // So we don't start/stop the timer here anymore.
    }

    private void AutoLoginTimer_Tick(object? sender, EventArgs e)
    {
        int maxToOpen = int.TryParse(numAutoLoginThread.Text, out int v) ? (v > 0 ? v : 2) : 2;
        int countChecked = 0;
        DateTime now = TimeHelper.GetRealTime();

        foreach (DataGridViewRow row in dgvAccounts.Rows)
        {
            if (row.Tag is int accountId)
            {
                var acSet = _accountSettingsService.Load(accountId);
                var sc = acSet.Schedule ?? new ScheduleSettings();
                
                bool isRowActionSelected = row.Cells[colSelect.Index] is DataGridViewCheckBoxCell checkCell && 
                                           checkCell.Value != null && (bool)checkCell.Value;

                // Bắt buộc phải tích chọn account thì auto mới tác động (kể cả khung giờ bật).
                if (!isRowActionSelected)
                    continue;

                bool allowRun = true;
                string vetoReason = "";

                if (sc.IsScheduleEnabled && !IsTimeInSchedule(now, sc.GetStartTime(), sc.GetEndTime()))
                {
                    allowRun = false;
                    vetoReason = "💤 Đang ngủ đông";
                }

                string status = row.Cells[colStatus.Index].Value?.ToString() ?? "";

                // 2. Chế ngự toàn cục (Thiết quân luật): Smart Stop Enforcement
                if (!allowRun)
                {
                    if (status.Contains("ONLINE") || status.Contains("LOGIN") || status.Contains("VÀO GAME") || status.Contains("KẾT NỐI"))
                    {
                        KillAccountRow(row);
                    }
                    
                    if (!string.IsNullOrEmpty(vetoReason))
                    {
                        row.Cells[colDataInGame.Index].Value = vetoReason;
                    }
                    continue; // Chặn đứng tại đây, KHÔNG CÓ CỬA cho AutoLogin "Tự mở" hoạt động
                }

                // Xóa nhãn "Đang ngủ đông" nếu game đang Offline và bắt đầu được trả quyền lại
                if (sc.IsScheduleEnabled && allowRun && status.Contains("OFFLINE"))
                {
                    if ((row.Cells[colDataInGame.Index].Value?.ToString() ?? "").Contains("Đang ngủ đông"))
                    {
                        row.Cells[colDataInGame.Index].Value = "";
                    }
                }

                // Global manual toggle OR Schedule window for launching (đều phải qua điều kiện row đã tick).
                bool shouldLaunch = false;
                if (sc.IsScheduleEnabled)
                {
                    shouldLaunch = true; // Trong giờ: Mở game lên
                }
                else if (chkAutoLogin.Checked && isRowActionSelected)
                {
                    shouldLaunch = true;
                }
                
                if (!shouldLaunch) continue;

                countChecked++;
                if (countChecked <= maxToOpen)
                {
                    if (status.Contains("OFFLINE"))
                    {
                        LaunchAccountRow(row.Index);
                        return; // Launch max 1 per cycle
                    }
                }
            }
        }
    }

    private readonly Dictionary<int, int> _accountProcessIds = new(); // accountId → processId

    private void SetProcessIdForAccount(int accountId, int pid)
    {
        _accountProcessIds[accountId] = pid;
    }

    private int GetProcessIdForAccount(int accountId)
    {
        return _accountProcessIds.TryGetValue(accountId, out int pid) ? pid : -1;
    }

    private int FindRowByAccountId(int accountId)
    {
        foreach (DataGridViewRow row in dgvAccounts.Rows)
        {
            if (row.Tag is int id && id == accountId)
                return row.Index;
        }
        return -1;
    }

    private void RebuildRowMap()
    {
        _rowToAccountId.Clear();
        foreach (DataGridViewRow row in dgvAccounts.Rows)
        {
            if (row.Tag is int id)
            {
                _rowToAccountId[row.Index] = id;
                
                // Tự động update lại tên tab game nếu đang chạy cho đúng STT mới
                int pid = GetProcessIdForAccount(id);
                if (pid > 0)
                {
                    string newTitle = $"Tab - {row.Index + 1}";
                    GameLauncher.UpdateWindowTitle(pid, newTitle);
                }
            }
        }
    }

    private void ChkHideAccount_CheckedChanged(object? sender, EventArgs e)
    {
        RefreshAccountDisplay();
    }

    private void RefreshAccountDisplay()
    {
        var accountById = _accountRepo.GetAllAccounts().ToDictionary(a => a.Id, a => a.Username);
        foreach (DataGridViewRow row in dgvAccounts.Rows)
        {
            if (row.Tag is int id && accountById.TryGetValue(id, out var username))
            {
                row.Cells[colAccount.Index].Value = FormatAccountForDisplay(username ?? string.Empty);
            }
        }
    }

    private string FormatAccountForDisplay(string username)
    {
        if (!chkHideAccount.Checked || string.IsNullOrEmpty(username))
            return username;

        if (username.Length <= 3)
            return "****";

        int suffixLength = Math.Min(3, username.Length);
        return $"{username.Substring(0, 3)}****{username.Substring(username.Length - suffixLength, suffixLength)}";
    }

    // Support feature removed

    // ──────────────────────────────────────────────────────────────────────
    // Train Nâng Cao – Controls (động, tạo lúc runtime)
    // ──────────────────────────────────────────────────────────────────────



    // A* controls
    private System.Windows.Forms.NumericUpDown nudAstarStep = null!;
    private System.Windows.Forms.NumericUpDown nudAstarDelay = null!;

    private System.Windows.Forms.CheckBox chkAttackHpAbove = null!;
    private System.Windows.Forms.NumericUpDown nudAttackHpAboveValue = null!;
    private System.Windows.Forms.CheckBox chkAttackHpBelow = null!;
    private System.Windows.Forms.NumericUpDown nudAttackHpBelowValue = null!;
    
    private System.Windows.Forms.CheckBox chkRotateZone = null!;
    private System.Windows.Forms.TextBox txtRotateZoneList = null!;

    private System.Windows.Forms.CheckBox chkOptimizeKsVang = null!;

    private System.Windows.Forms.CheckBox chkAutoBuyThoiVang = null!;
    private System.Windows.Forms.NumericUpDown nudBuyThoiVangMinGold = null!;

    private GroupBox grpAutoUpZin = null!;
    private CheckBox chkAutoUpZin = null!;
    private TextBox txtAutoUpZinPrefix = null!;
    private ComboBox cmbAutoUpZinClass = null!;
    private Label lblAutoUpZinPrefixHint = null!;
    private bool _isUpdatingAutoUpZinPrefix;

    private GroupBox grpUpZin700k = null!;
    private CheckBox chkUpZin700k = null!;
    private TextBox txtUpZin700kPrefix = null!;
    private ComboBox cmbUpZin700kClass = null!;
    private Label lblUpZin700kPrefixHint = null!;
    private bool _isUpdatingUpZin700kPrefix;

    // Auto chỉ số UI controls
    private GroupBox grpAutoPoint = null!;
    private CheckBox chkAutoPointHP = null!;
    private NumericUpDown numAutoPointHP = null!;
    private CheckBox chkAutoPointMP = null!;
    private NumericUpDown numAutoPointMP = null!;
    private CheckBox chkAutoPointDamage = null!;
    private NumericUpDown numAutoPointDamage = null!;
    private GroupBox grpAutoAmulet = null!;
    private CheckBox chkAutoAmuletEnabled = null!;
    private ComboBox cmbAutoAmuletDuration = null!;
    private CheckBox chkAmuletWisdom = null!;
    private CheckBox chkAmuletStrong = null!;
    private CheckBox chkAmuletBuffaloSkin = null!;
    private CheckBox chkAmuletHeroic = null!;
    private CheckBox chkAmuletImmortal = null!;
    private CheckBox chkAmuletEnduring = null!;
    private CheckBox chkAmuletMagnet = null!;
    private CheckBox chkAmuletDisciple = null!;
    private CheckBox chkAmuletWisdomX3 = null!;
    private CheckBox chkAmuletWisdomX4 = null!;

    private void InitializeUpZin700kUi()
    {
        _tabUpZin700k = new TabPage("Tự động")
        {
            BackColor = Color.FromArgb(241, 245, 249),
            Padding = new Padding(6),
            AutoScroll = true
        };

        // Tạo TabControl bên trong tab "Tự động"
        var innerTabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Appearance = TabAppearance.Normal
        };

        // Tab con 1: Up Zin
        var tabUpZin = new TabPage("Up Zin")
        {
            BackColor = Color.FromArgb(241, 245, 249),
            Padding = new Padding(6),
            AutoScroll = true
        };

        grpUpZin700k = new GroupBox
        {
            Text = "Auto làm nhiệm vụ 0-3 tân thủ",
            Location = new Point(8, 8),
            Size = new Size(460, 180),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        chkUpZin700k = new CheckBox
        {
            Text = "Bật Auto làm NV 0-3",
            AutoSize = true,
            Location = new Point(12, 24)
        };

        var lblPrefix = new Label
        {
            Text = "Ký tự đầu tên (3-8 ký tự):",
            AutoSize = true,
            Location = new Point(12, 52)
        };

        txtUpZin700kPrefix = new TextBox
        {
            Location = new Point(200, 48),
            Size = new Size(150, 23),
            MaxLength = 8,
            PlaceholderText = "VD: abc hoặc abcd1234"
        };

        var lblClass = new Label
        {
            Text = "Chọn hành tinh:",
            AutoSize = true,
            Location = new Point(12, 80)
        };

        cmbUpZin700kClass = new ComboBox
        {
            Location = new Point(200, 76),
            Size = new Size(150, 23),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbUpZin700kClass.Items.AddRange(new object[] { "Trái Đất", "Namek", "Xayda", "Ngẫu nhiên" });
        cmbUpZin700kClass.SelectedIndex = 3;

        lblUpZin700kPrefixHint = new Label
        {
            AutoSize = true,
            Location = new Point(12, 108),
            ForeColor = Color.DimGray
        };

        var lblDesc = new Label
        {
            Text = "Tự tạo char → Nhặt Sao Băng → Tạo char tiếp.",
            AutoSize = true,
            Location = new Point(12, 132),
            ForeColor = Color.DimGray
        };

        grpUpZin700k.Controls.Add(chkUpZin700k);
        grpUpZin700k.Controls.Add(lblPrefix);
        grpUpZin700k.Controls.Add(txtUpZin700kPrefix);
        grpUpZin700k.Controls.Add(lblClass);
        grpUpZin700k.Controls.Add(cmbUpZin700kClass);
        grpUpZin700k.Controls.Add(lblUpZin700kPrefixHint);
        grpUpZin700k.Controls.Add(lblDesc);

        tabUpZin.Controls.Add(grpUpZin700k);

        // Tab con 2: Auto chỉ sổ
        var tabAutoChiSo = new TabPage("Auto chỉ số")
        {
            BackColor = Color.FromArgb(241, 245, 249),
            Padding = new Padding(6),
            AutoScroll = true
        };

        grpAutoPoint = new GroupBox
        {
            Text = "Auto chỉ số",
            Location = new Point(8, 8),
            Size = new Size(460, 200),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        chkAutoPointHP = new CheckBox
        {
            Text = "Tăng HP đến mốc",
            AutoSize = true,
            Location = new Point(12, 24)
        };

        numAutoPointHP = new NumericUpDown
        {
            Location = new Point(200, 22),
            Size = new Size(100, 23),
            Minimum = 0,
            Maximum = 50000,
            Value = 1000
        };

        chkAutoPointMP = new CheckBox
        {
            Text = "Tăng MP đến mốc",
            AutoSize = true,
            Location = new Point(12, 52)
        };

        numAutoPointMP = new NumericUpDown
        {
            Location = new Point(200, 50),
            Size = new Size(100, 23),
            Minimum = 0,
            Maximum = 50000,
            Value = 500
        };

        chkAutoPointDamage = new CheckBox
        {
            Text = "Tăng Sức đánh đến mốc",
            AutoSize = true,
            Location = new Point(12, 80)
        };

        numAutoPointDamage = new NumericUpDown
        {
            Location = new Point(200, 78),
            Size = new Size(100, 23),
            Minimum = 0,
            Maximum = 50000,
            Value = 500
        };

        var lblAutoPointDesc = new Label
        {
            Text = "Tự động cộng chỉ số khi còn tiềm năng.",
            AutoSize = true,
            Location = new Point(12, 110),
            ForeColor = Color.DimGray
        };

        grpAutoPoint.Controls.Add(chkAutoPointHP);
        grpAutoPoint.Controls.Add(numAutoPointHP);
        grpAutoPoint.Controls.Add(chkAutoPointMP);
        grpAutoPoint.Controls.Add(numAutoPointMP);
        grpAutoPoint.Controls.Add(chkAutoPointDamage);
        grpAutoPoint.Controls.Add(numAutoPointDamage);
        grpAutoPoint.Controls.Add(lblAutoPointDesc);

        tabAutoChiSo.Controls.Add(grpAutoPoint);

        // Event handlers cho Auto chỉ số
        chkAutoPointHP.CheckedChanged += AutoPointSettingsControl_Changed;
        chkAutoPointMP.CheckedChanged += AutoPointSettingsControl_Changed;
        chkAutoPointDamage.CheckedChanged += AutoPointSettingsControl_Changed;
        numAutoPointHP.ValueChanged += AutoPointSettingsControl_Changed;
        numAutoPointMP.ValueChanged += AutoPointSettingsControl_Changed;
        numAutoPointDamage.ValueChanged += AutoPointSettingsControl_Changed;

        // Tab con 3: Auto bùa
        var tabAutoBua = new TabPage("Auto bùa")
        {
            BackColor = Color.FromArgb(241, 245, 249),
            Padding = new Padding(6),
            AutoScroll = true
        };

        grpAutoAmulet = new GroupBox
        {
            Text = "Auto mua bùa",
            Location = new Point(8, 8),
            Size = new Size(520, 390),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        chkAutoAmuletEnabled = new CheckBox
        {
            Text = "Bật auto mua bùa khi sắp hết",
            AutoSize = true,
            Location = new Point(12, 28)
        };

        var lblAmuletDuration = new Label
        {
            Text = "Thời lượng mua:",
            AutoSize = true,
            Location = new Point(12, 64)
        };

        cmbAutoAmuletDuration = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(140, 60),
            Size = new Size(160, 23)
        };
        cmbAutoAmuletDuration.Items.AddRange(new object[] { "1 giờ", "8 giờ", "1 tháng" });
        cmbAutoAmuletDuration.SelectedIndex = 0;

        var lblAmuletList = new Label
        {
            Text = "Chọn bùa cần duy trì:",
            AutoSize = true,
            Location = new Point(12, 100)
        };

        // Checkbox riêng lẻ layout 2 cột
        chkAmuletWisdom = new CheckBox
        {
            Text = "213 - Bùa Trí Tuệ",
            AutoSize = true,
            Location = new Point(15, 126)
        };

        chkAmuletStrong = new CheckBox
        {
            Text = "214 - Bùa Mạnh Mẽ",
            AutoSize = true,
            Location = new Point(240, 126)
        };

        chkAmuletBuffaloSkin = new CheckBox
        {
            Text = "215 - Bùa Da Trâu",
            AutoSize = true,
            Location = new Point(15, 150)
        };

        chkAmuletHeroic = new CheckBox
        {
            Text = "216 - Bùa Oai Hùng",
            AutoSize = true,
            Location = new Point(240, 150)
        };

        chkAmuletImmortal = new CheckBox
        {
            Text = "217 - Bùa Bất Tử",
            AutoSize = true,
            Location = new Point(15, 174)
        };

        chkAmuletEnduring = new CheckBox
        {
            Text = "218 - Bùa Dẻo Dai",
            AutoSize = true,
            Location = new Point(240, 174)
        };

        chkAmuletMagnet = new CheckBox
        {
            Text = "219 - Bùa Thu Hút",
            AutoSize = true,
            Location = new Point(15, 198)
        };

        chkAmuletDisciple = new CheckBox
        {
            Text = "522 - Bùa Đệ Tử",
            AutoSize = true,
            Location = new Point(240, 198)
        };

        chkAmuletWisdomX3 = new CheckBox
        {
            Text = "671 - Bùa Trí Tuệ x3",
            AutoSize = true,
            Location = new Point(15, 222)
        };

        chkAmuletWisdomX4 = new CheckBox
        {
            Text = "672 - Bùa Trí Tuệ x4",
            AutoSize = true,
            Location = new Point(240, 222)
        };

        var lblAmuletDesc = new Label
        {
            Text = "Bot sẽ về NPC 21 ở map bùa theo hành tinh, đọc thời gian còn lại và mua lại khi bùa sắp hết.",
            AutoSize = false,
            Location = new Point(12, 345),
            Size = new Size(480, 38),
            ForeColor = Color.DimGray
        };

        grpAutoAmulet.Controls.Add(chkAutoAmuletEnabled);
        grpAutoAmulet.Controls.Add(lblAmuletDuration);
        grpAutoAmulet.Controls.Add(cmbAutoAmuletDuration);
        grpAutoAmulet.Controls.Add(lblAmuletList);
        grpAutoAmulet.Controls.Add(chkAmuletWisdom);
        grpAutoAmulet.Controls.Add(chkAmuletStrong);
        grpAutoAmulet.Controls.Add(chkAmuletBuffaloSkin);
        grpAutoAmulet.Controls.Add(chkAmuletHeroic);
        grpAutoAmulet.Controls.Add(chkAmuletImmortal);
        grpAutoAmulet.Controls.Add(chkAmuletEnduring);
        grpAutoAmulet.Controls.Add(chkAmuletMagnet);
        grpAutoAmulet.Controls.Add(chkAmuletDisciple);
        grpAutoAmulet.Controls.Add(chkAmuletWisdomX3);
        grpAutoAmulet.Controls.Add(chkAmuletWisdomX4);
        grpAutoAmulet.Controls.Add(lblAmuletDesc);
        tabAutoBua.Controls.Add(grpAutoAmulet);

        // Thêm các tab con vào TabControl
        innerTabControl.TabPages.Add(tabUpZin);
        innerTabControl.TabPages.Add(tabAutoChiSo);
        innerTabControl.TabPages.Add(tabAutoBua);

        // Thêm TabControl vào tab "Tự động"
        _tabUpZin700k.Controls.Add(innerTabControl);

        // Chèn tab sau tab Auto Up Zin (nếu có), nếu không thì thêm vào tabControlFeatures
        int upZinIndex = this.tabControlFeatures.TabPages.IndexOf(_tabAutoUpZin);
        if (upZinIndex >= 0)
            this.tabControlFeatures.TabPages.Insert(upZinIndex + 1, _tabUpZin700k);
        else
        {
            int trainTabIndex = this.tabControlFeatures.TabPages.IndexOf(this.tabTrain);
            if (trainTabIndex >= 0)
                this.tabControlFeatures.TabPages.Insert(trainTabIndex + 1, _tabUpZin700k);
            else
                this.tabControlFeatures.TabPages.Add(_tabUpZin700k);
        }

        chkUpZin700k.CheckedChanged += UpZin700kSettingsControl_Changed;
        txtUpZin700kPrefix.TextChanged += UpZin700kSettingsControl_Changed;
        txtUpZin700kPrefix.KeyPress += TxtUpZin700kPrefix_KeyPress;
        cmbUpZin700kClass.SelectedIndexChanged += UpZin700kSettingsControl_Changed;
        chkAutoAmuletEnabled.CheckedChanged += AutoAmuletSettingsControl_Changed;
        cmbAutoAmuletDuration.SelectedIndexChanged += AutoAmuletSettingsControl_Changed;
        chkAmuletWisdom.CheckedChanged += AutoAmuletSettingsControl_Changed;
        chkAmuletStrong.CheckedChanged += AutoAmuletSettingsControl_Changed;
        chkAmuletBuffaloSkin.CheckedChanged += AutoAmuletSettingsControl_Changed;
        chkAmuletHeroic.CheckedChanged += AutoAmuletSettingsControl_Changed;
        chkAmuletImmortal.CheckedChanged += AutoAmuletSettingsControl_Changed;
        chkAmuletEnduring.CheckedChanged += AutoAmuletSettingsControl_Changed;
        chkAmuletMagnet.CheckedChanged += AutoAmuletSettingsControl_Changed;
        chkAmuletDisciple.CheckedChanged += AutoAmuletSettingsControl_Changed;
        chkAmuletWisdomX3.CheckedChanged += AutoAmuletSettingsControl_Changed;
        chkAmuletWisdomX4.CheckedChanged += AutoAmuletSettingsControl_Changed;

        UpdateUpZin700kPrefixUi();
    }

    private void TxtUpZin700kPrefix_KeyPress(object? sender, KeyPressEventArgs e)
    {
        if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            e.Handled = true;
    }

    private void UpZin700kSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _isUpdatingUpZin700kPrefix || _currentSelectedAccountId <= 0) return;

        string normalizedPrefix = NormalizeUpZin700kPrefix(txtUpZin700kPrefix.Text);
        if (!string.Equals(txtUpZin700kPrefix.Text, normalizedPrefix, StringComparison.Ordinal))
        {
            int cursor = txtUpZin700kPrefix.SelectionStart;
            _isUpdatingUpZin700kPrefix = true;
            txtUpZin700kPrefix.Text = normalizedPrefix;
            txtUpZin700kPrefix.SelectionStart = Math.Min(cursor, normalizedPrefix.Length);
            _isUpdatingUpZin700kPrefix = false;
        }

        _currentUpZin700kSettings.Enabled = chkUpZin700k.Checked;
        _currentUpZin700kSettings.NamePrefix = normalizedPrefix;
        _currentUpZin700kSettings.TargetClass = cmbUpZin700kClass.SelectedIndex == 3 ? -1 : cmbUpZin700kClass.SelectedIndex;

        _currentSettings.UpZin700k = _currentUpZin700kSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);

        if (_currentUpZin700kSettings.Enabled)
            SendUpZin700kCommand(_currentSelectedAccountId, _currentUpZin700kSettings);
        else
            SendUpZin700kOffCommand(_currentSelectedAccountId);

        UpdateUpZin700kPrefixUi();
    }

    private void InitializeAutoUpZinUi()
    {
        _tabAutoUpZin = new TabPage("Auto Up Zin")
        {
            BackColor = Color.FromArgb(241, 245, 249),
            Padding = new Padding(6),
            AutoScroll = true
        };

        grpAutoUpZin = new GroupBox
        {
            Text = "Cấu hình Auto Up Zin",
            Location = new Point(8, 8),
            Size = new Size(460, 150),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        chkAutoUpZin = new CheckBox
        {
            Text = "Bật Auto Up Zin",
            AutoSize = true,
            Location = new Point(12, 24)
        };

        var lblPrefix = new Label
        {
            Text = "Ký tự đầu tên (3-4 ký tự):",
            AutoSize = true,
            Location = new Point(12, 56)
        };

        txtAutoUpZinPrefix = new TextBox
        {
            Location = new Point(200, 52),
            Size = new Size(150, 23),
            MaxLength = 4,
            PlaceholderText = "VD: abc hoặc abcd"
        };
        
        var lblClass = new Label
        {
            Text = "Chọn hành tinh:",
            AutoSize = true,
            Location = new Point(12, 84)
        };

        cmbAutoUpZinClass = new ComboBox
        {
            Location = new Point(200, 80),
            Size = new Size(150, 23),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbAutoUpZinClass.Items.AddRange(new object[] { "Trái Đất", "Namek", "Xayda", "Ngẫu nhiên" });
        cmbAutoUpZinClass.SelectedIndex = 3;

        lblAutoUpZinPrefixHint = new Label
        {
            AutoSize = true,
            Location = new Point(12, 112),
            ForeColor = Color.DimGray
        };

        grpAutoUpZin.Controls.Add(chkAutoUpZin);
        grpAutoUpZin.Controls.Add(lblPrefix);
        grpAutoUpZin.Controls.Add(txtAutoUpZinPrefix);
        grpAutoUpZin.Controls.Add(lblClass);
        grpAutoUpZin.Controls.Add(cmbAutoUpZinClass);
        grpAutoUpZin.Controls.Add(lblAutoUpZinPrefixHint);

        _tabAutoUpZin.Controls.Add(grpAutoUpZin);

        // Tab "Auto Up Zin" bị ẩn - không thêm vào tabControlFeatures
        // int trainTabIndex = this.tabControlFeatures.TabPages.IndexOf(this.tabTrain);
        // if (trainTabIndex >= 0)
        //     this.tabControlFeatures.TabPages.Insert(trainTabIndex + 1, _tabAutoUpZin);
        // else
        //     this.tabControlFeatures.TabPages.Add(_tabAutoUpZin);

        chkAutoUpZin.CheckedChanged += AutoUpZinSettingsControl_Changed;
        txtAutoUpZinPrefix.TextChanged += AutoUpZinSettingsControl_Changed;
        txtAutoUpZinPrefix.KeyPress += TxtAutoUpZinPrefix_KeyPress;
        cmbAutoUpZinClass.SelectedIndexChanged += AutoUpZinSettingsControl_Changed;

        UpdateAutoUpZinPrefixUi();
    }

    private void InitializeScheduleTab()
    {
        scheduleControl = new ScheduleControl();
        scheduleControl.Location = new System.Drawing.Point(6, 150);
        this.tabConnection.Controls.Add(scheduleControl);
        
        scheduleControl.SettingsChanged += ScheduleSettingsControl_Changed;
    }

    private void ScheduleSettingsControl_Changed(object? sender, EventArgs e)
    {
        if (_isBindingData || _currentSelectedAccountId <= 0) return;

        _currentScheduleSettings = scheduleControl.GetSettings();
        _currentSettings.Schedule = _currentScheduleSettings;
        _accountSettingsService.Save(_currentSelectedAccountId, _currentSettings);
        
        // Push update to Watchdog/Schedule Manager by evaluating immediately if needed
    }

    /// <summary>
    /// Gọi trong Form_Load để khởi tạo tab "Train Nâng Cao" động.
    /// Phải được gọi sau InitializeComponent().
    /// </summary>
    private void InitializeTrainAdvancedTab()
    {
        // Dùng tabTrainAdvanced đã có sẵn trong Designer (tab "Nâng cao")
        var tab = this.tabTrainAdvanced;

        int y = 8;

        // ── Nhóm A* ───────────────────────────────────────────────────────
        var gbAstar = new System.Windows.Forms.GroupBox { Text = "Cài đặt A*", Location = new System.Drawing.Point(6, y), Size = new System.Drawing.Size(420, 56) };
        tab.Controls.Add(gbAstar);

        AddLabel(gbAstar, "Bước:", 8, 26);
        nudAstarStep = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(55, 22), Size = new System.Drawing.Size(42, 22), Minimum = 1, Maximum = 3, Value = 3 };
        AddLabel(gbAstar, "Delay:", 115, 26);
        nudAstarDelay = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(165, 22), Size = new System.Drawing.Size(65, 22), Minimum = 25, Maximum = 100, Value = 30 };
        gbAstar.Controls.AddRange(new System.Windows.Forms.Control[] { nudAstarStep, nudAstarDelay });

        // ── Nhóm Chỉ đánh theo HP ─────────────────────────────────────────
        y += 62;
        var gbHp = new System.Windows.Forms.GroupBox { Text = "Chỉ đánh theo HP", Location = new System.Drawing.Point(6, y), Size = new System.Drawing.Size(420, 72) };
        tab.Controls.Add(gbHp);

        chkAttackHpAbove = new System.Windows.Forms.CheckBox { Text = "Chỉ đánh khi HP mục tiêu trên:", Location = new System.Drawing.Point(8, 20), AutoSize = true };
        nudAttackHpAboveValue = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(240, 18), Size = new System.Drawing.Size(110, 22), Maximum = 999999999, Value = 0 };
        
        chkAttackHpBelow = new System.Windows.Forms.CheckBox { Text = "Chỉ đánh khi HP mục tiêu dưới:", Location = new System.Drawing.Point(8, 44), AutoSize = true };
        nudAttackHpBelowValue = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(240, 42), Size = new System.Drawing.Size(110, 22), Maximum = 999999999, Value = 0 };

        gbHp.Controls.AddRange(new System.Windows.Forms.Control[] { chkAttackHpAbove, nudAttackHpAboveValue, chkAttackHpBelow, nudAttackHpBelowValue });

        // ── Nhóm Đổi khu khi đánh quái ───────────────────────────────────
        y += 82;
        var gbZone = new System.Windows.Forms.GroupBox { Text = "Đổi khu khi đánh quái", Location = new System.Drawing.Point(6, y), Size = new System.Drawing.Size(420, 95) };
        tab.Controls.Add(gbZone);

        chkRotateZone = new System.Windows.Forms.CheckBox { Text = "Đổi theo danh sách khu (cách nhau bằng dấu cách)", Location = new System.Drawing.Point(8, 20), AutoSize = true };
        var lblRotateZoneExample = new System.Windows.Forms.Label { Text = "(Ví dụ 0 1 2 3 4)", Location = new System.Drawing.Point(26, 40), AutoSize = true, ForeColor = System.Drawing.Color.Gray };
        txtRotateZoneList = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(8, 62), Size = new System.Drawing.Size(400, 22) };

        gbZone.Controls.AddRange(new System.Windows.Forms.Control[] { chkRotateZone, lblRotateZoneExample, txtRotateZoneList });

        // ── Nhóm Tự mua thỏi vàng ───────────────────────────────────────
        y += 103;
        var gbThoiVang = new System.Windows.Forms.GroupBox { Text = "Tự mua thỏi vàng (ID 457)", Location = new System.Drawing.Point(6, y), Size = new System.Drawing.Size(420, 70) };
        tab.Controls.Add(gbThoiVang);

        chkAutoBuyThoiVang = new System.Windows.Forms.CheckBox { Text = "Tự mua thỏi vàng khi đủ X vàng", Location = new System.Drawing.Point(8, 20), AutoSize = true };
        var lblThoiVangMin = new System.Windows.Forms.Label { Text = "(mua đến khi hết vàng)", Location = new System.Drawing.Point(8, 44), AutoSize = true, ForeColor = System.Drawing.Color.Gray };
        nudBuyThoiVangMinGold = new System.Windows.Forms.NumericUpDown
        {
            Location = new System.Drawing.Point(290, 18),
            Size = new System.Drawing.Size(120, 22),
            Maximum = 10_000_000_000L,
            Minimum = 100_000_000,
            Value = 1_000_000_000,
            ThousandsSeparator = true
        };
        gbThoiVang.Controls.AddRange(new System.Windows.Forms.Control[] { chkAutoBuyThoiVang, lblThoiVangMin, nudBuyThoiVangMinGold });

        // ── Đăng ký events ────────────────────────────────────────────────
        nudAstarStep.ValueChanged += TrainAdvancedSettingsControl_Changed;
        nudAstarDelay.ValueChanged += TrainAdvancedSettingsControl_Changed;
        chkAttackHpAbove.CheckedChanged += TrainAdvancedSettingsControl_Changed;
        nudAttackHpAboveValue.ValueChanged += TrainAdvancedSettingsControl_Changed;
        chkAttackHpBelow.CheckedChanged += TrainAdvancedSettingsControl_Changed;
        nudAttackHpBelowValue.ValueChanged += TrainAdvancedSettingsControl_Changed;
        chkRotateZone.CheckedChanged += TrainAdvancedSettingsControl_Changed;
        txtRotateZoneList.TextChanged += TrainAdvancedSettingsControl_Changed;
        chkAutoBuyThoiVang.CheckedChanged += TrainAdvancedSettingsControl_Changed;
        nudBuyThoiVangMinGold.ValueChanged += TrainAdvancedSettingsControl_Changed;
    }

    private TabPage _tabKsVang = null!;
    private RadioButton rdoKsVangZoneLeast = null!;
    private RadioButton rdoKsVangZoneMost = null!;
    private RadioButton rdoKsVangTriggerMob = null!;
    private RadioButton rdoKsVangTriggerTime = null!;
    private NumericUpDown nudKsVangTimeMin = null!;
    private CheckBox chkKsVangFilterPlayer = null!;
    private NumericUpDown nudKsVangPlayerMin = null!;
    private NumericUpDown nudKsVangPlayerMax = null!;
    private CheckBox chkKsVangAvoidChars = null!;
    private TextBox txtKsVangAvoidCharsList = null!;

    private void InitializeKsVangTab()
    {
        _tabKsVang = new TabPage("KS Vàng")
        {
            BackColor = Color.FromArgb(203, 213, 225),
            Padding = new Padding(6),
            AutoScroll = true
        };

        var lblNote = new Label
        {
            Text = "Lưu ý: Chỉ có tác dụng khi dùng Train cơ bản.",
            Location = new Point(8, 12),
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Italic | FontStyle.Bold),
            ForeColor = Color.DarkRed
        };
        _tabKsVang.Controls.Add(lblNote);

        chkOptimizeKsVang = new CheckBox
        {
            Text = "Tối ưu KS Vàng (First-hit)",
            Location = new Point(12, 40),
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.Red
        };
        _tabKsVang.Controls.Add(chkOptimizeKsVang);

        // GroupBox Auto Zone
        var gbAutoZone = new GroupBox
        {
            Text = "Cấu hình Auto Zone KS Vàng",
            Location = new Point(6, 70),
            Size = new Size(390, 260)
        };
        _tabKsVang.Controls.Add(gbAutoZone);

        int y = 24;

        // 1. Chế độ
        AddLabel(gbAutoZone, "Ưu tiên khu:", 8, y + 2);
        System.Windows.Forms.Panel pnlMode = new System.Windows.Forms.Panel { Location = new Point(110, y - 4), Size = new Size(250, 28) };
        rdoKsVangZoneLeast = new RadioButton { Text = "Ít người nhất", Location = new Point(0, 4), AutoSize = true, Checked = true };
        rdoKsVangZoneMost = new RadioButton { Text = "Đông người nhất", Location = new Point(130, 4), AutoSize = true };
        pnlMode.Controls.AddRange(new Control[] { rdoKsVangZoneLeast, rdoKsVangZoneMost });
        gbAutoZone.Controls.Add(pnlMode);

        y += 35;
        // 2. Điều kiện
        AddLabel(gbAutoZone, "Đổi khu khi:", 8, y + 2);
        System.Windows.Forms.Panel pnlTrigger = new System.Windows.Forms.Panel { Location = new Point(110, y - 4), Size = new Size(210, 28) };
        rdoKsVangTriggerMob = new RadioButton { Text = "Hết quái", Location = new Point(0, 4), AutoSize = true, Checked = true };
        rdoKsVangTriggerTime = new RadioButton { Text = "Sau (phút):", Location = new Point(110, 4), AutoSize = true };
        pnlTrigger.Controls.AddRange(new Control[] { rdoKsVangTriggerMob, rdoKsVangTriggerTime });
        gbAutoZone.Controls.Add(pnlTrigger);
        
        nudKsVangTimeMin = new NumericUpDown { Location = new Point(330, y - 2), Size = new Size(50, 23), Minimum = 1, Maximum = 60, Value = 5 };
        gbAutoZone.Controls.Add(nudKsVangTimeMin);

        y += 35;
        // 3. Lọc số người
        chkKsVangFilterPlayer = new CheckBox { Text = "Giới hạn số người:", Location = new Point(8, y), AutoSize = true };
        AddLabel(gbAutoZone, "Từ:", 165, y + 2);
        nudKsVangPlayerMin = new NumericUpDown { Location = new Point(195, y - 2), Size = new Size(50, 23), Minimum = 0, Maximum = 15, Value = 3 };
        AddLabel(gbAutoZone, "đến:", 260, y + 2);
        nudKsVangPlayerMax = new NumericUpDown { Location = new Point(300, y - 2), Size = new Size(50, 23), Minimum = 1, Maximum = 15, Value = 5 };
        gbAutoZone.Controls.AddRange(new Control[] { chkKsVangFilterPlayer, nudKsVangPlayerMin, nudKsVangPlayerMax });

        y += 35;
        // 4. Né list char
        chkKsVangAvoidChars = new CheckBox { Text = "Đổi khu để né list char:", Location = new Point(8, y), AutoSize = true, ForeColor = Color.DarkOrange };
        gbAutoZone.Controls.Add(chkKsVangAvoidChars);

        y += 26;
        txtKsVangAvoidCharsList = new TextBox { Location = new Point(10, y), Size = new Size(370, 85), Multiline = true, ScrollBars = ScrollBars.Vertical, ForeColor = Color.DarkRed };
        gbAutoZone.Controls.Add(txtKsVangAvoidCharsList);

        // Chèn vào sau tabUpSkh
        int index = tabControlTrain.TabPages.IndexOf(tabUpSkh);
        if (index >= 0)
        {
            tabControlTrain.TabPages.Insert(index + 1, _tabKsVang);
        }
        else
        {
            tabControlTrain.TabPages.Add(_tabKsVang);
        }

        // Attach events
        chkOptimizeKsVang.CheckedChanged += TrainAdvancedSettingsControl_Changed;
        rdoKsVangZoneLeast.CheckedChanged += TrainAdvancedSettingsControl_Changed;
        rdoKsVangZoneMost.CheckedChanged += TrainAdvancedSettingsControl_Changed;
        rdoKsVangTriggerMob.CheckedChanged += TrainAdvancedSettingsControl_Changed;
        rdoKsVangTriggerTime.CheckedChanged += TrainAdvancedSettingsControl_Changed;
        nudKsVangTimeMin.ValueChanged += TrainAdvancedSettingsControl_Changed;
        chkKsVangFilterPlayer.CheckedChanged += TrainAdvancedSettingsControl_Changed;
        nudKsVangPlayerMax.ValueChanged += TrainAdvancedSettingsControl_Changed;
        chkKsVangAvoidChars.CheckedChanged += TrainAdvancedSettingsControl_Changed;
        txtKsVangAvoidCharsList.TextChanged += TrainAdvancedSettingsControl_Changed;

        // Xử lý bật/tắt (ẩn/hiện) textbox nhập thời gian dựa vào checkbox
        txtKsVangAvoidCharsList.Enabled = chkKsVangAvoidChars.Checked;
        chkKsVangAvoidChars.CheckedChanged += (s, e) => { txtKsVangAvoidCharsList.Enabled = chkKsVangAvoidChars.Checked; };
        rdoKsVangTriggerTime.CheckedChanged += (s, e) => { nudKsVangTimeMin.Enabled = rdoKsVangTriggerTime.Checked; };
        chkKsVangFilterPlayer.CheckedChanged += (s, e) => 
        {
            nudKsVangPlayerMin.Enabled = chkKsVangFilterPlayer.Checked;
            nudKsVangPlayerMax.Enabled = chkKsVangFilterPlayer.Checked;
        };
    }

    /// <summary>Tìm TabControl đầu tiên trong cây controls (đệ quy).</summary>
    private static System.Windows.Forms.TabControl? FindTabControl(System.Windows.Forms.Control root)
    {
        foreach (System.Windows.Forms.Control c in root.Controls)
        {
            if (c is System.Windows.Forms.TabControl tc) return tc;
            var found = FindTabControl(c);
            if (found != null) return found;
        }
        return null;
    }

    private static System.Windows.Forms.Label AddLabel(System.Windows.Forms.Control parent, string text, int x, int y)
    {
        var lbl = new System.Windows.Forms.Label { Text = text, Location = new System.Drawing.Point(x, y), AutoSize = true };
        parent.Controls.Add(lbl);
        return lbl;
    }

    // ─── Quản lý Config: Copy / Paste ─────────────────────────────────────

    /// <summary>
    /// Copy config của tài khoản đang chọn vào clipboard nội bộ.
    /// Deep-clone bằng JSON để tránh reference sharing.
    /// </summary>
    private void BtnCopyConfig_Click(object? sender, EventArgs e)
    {
        if (_currentSelectedAccountId <= 0)
        {
            MessageBox.Show("Chưa chọn tài khoản nào!", "Quản lý Config", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Deep-clone settings hiện tại qua JSON
        var json = System.Text.Json.JsonSerializer.Serialize(
            _currentSettings, PanelJsonContext.Default.AccountSettingsRoot);
        _clipboardSettings = System.Text.Json.JsonSerializer.Deserialize(
            json, PanelJsonContext.Default.AccountSettingsRoot);

        // Lấy thông tin tài khoản để hiển thị
        string accountDisplay = dgvAccounts.CurrentRow?.Cells[colAccount.Index].Value?.ToString() ?? $"ID {_currentSelectedAccountId}";
        string serverDisplay  = dgvAccounts.CurrentRow?.Cells[colServer.Index].Value?.ToString() ?? "";
        _clipboardAccountInfo = $"{accountDisplay}, Server: {serverDisplay}";

        lblConfigClipboard.Text = $"- Config đang copy là:\n=> [{_clipboardAccountInfo}]";
        lblConfigClipboard.ForeColor = System.Drawing.Color.FromArgb(22, 163, 74); // xanh lá = đã copy
    }

    /// <summary>
    /// Dán config clipboard vào TÀI KHOẢN ĐANG CHỌN, giữ nguyên proxy riêng.
    /// </summary>
    private void BtnPasteConfigCurrent_Click(object? sender, EventArgs e)
    {
        if (_clipboardSettings == null)
        {
            MessageBox.Show("Chưa có config nào được copy!", "Quản lý Config", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_currentSelectedAccountId <= 0)
        {
            MessageBox.Show("Chưa chọn tài khoản nào!", "Quản lý Config", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        PasteSettingsToAccount(_currentSelectedAccountId);

        // Reload UI cho tài khoản hiện tại
        if (dgvAccounts.CurrentRow != null)
            LoadAccountSettings(dgvAccounts.CurrentRow);
    }

    /// <summary>
    /// Dán config clipboard vào TẤT CẢ TÀI KHOẢN ĐANG TÍCH CHỌN (cột Chọn).
    /// </summary>
    private void BtnPasteConfigChecked_Click(object? sender, EventArgs e)
    {
        if (_clipboardSettings == null)
        {
            MessageBox.Show("Chưa có config nào được copy!", "Quản lý Config", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int count = 0;
        foreach (DataGridViewRow row in dgvAccounts.Rows)
        {
            bool isChecked = row.Cells[colSelect.Index].Value is bool b && b;
            if (!isChecked) continue;
            if (row.Tag is not int accountId) continue;

            PasteSettingsToAccount(accountId);
            count++;
        }

        if (count == 0)
        {
            MessageBox.Show("Không có tài khoản nào được tích chọn!", "Quản lý Config", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        MessageBox.Show($"Đã dán config cho {count} tài khoản tích chọn!", "Quản lý Config", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // Reload UI cho tài khoản đang chọn
        if (dgvAccounts.CurrentRow != null)
            LoadAccountSettings(dgvAccounts.CurrentRow);
    }

    /// <summary>
    /// Dán config clipboard vào TẤT CẢ TÀI KHOẢN trong danh sách.
    /// </summary>
    private void BtnPasteConfigAll_Click(object? sender, EventArgs e)
    {
        if (_clipboardSettings == null)
        {
            MessageBox.Show("Chưa có config nào được copy!", "Quản lý Config", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int total = dgvAccounts.Rows.Count;
        if (total == 0) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn dán config này cho TẤT CẢ {total} tài khoản không?",
            "Xác nhận",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        int count = 0;
        foreach (DataGridViewRow row in dgvAccounts.Rows)
        {
            if (row.Tag is not int accountId) continue;
            PasteSettingsToAccount(accountId);
            count++;
        }

        MessageBox.Show($"Đã dán config cho {count} tài khoản!", "Quản lý Config", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // Reload UI cho tài khoản đang chọn
        if (dgvAccounts.CurrentRow != null)
            LoadAccountSettings(dgvAccounts.CurrentRow);
    }

    /// <summary>
    /// Dán config clipboard vào tất cả tài khoản có TypeAccount == nudPasteConfigTypeAccount.Value.
    /// </summary>
    private void BtnPasteConfigByType_Click(object? sender, EventArgs e)
    {
        if (_clipboardSettings == null)
        {
            MessageBox.Show("Chưa có config nào được copy!", "Quản lý Config", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int targetType = (int)nudPasteConfigTypeAccount.Value;
        int count = 0;

        foreach (DataGridViewRow row in dgvAccounts.Rows)
        {
            if (row.Tag is not int accountId) continue;

            // Đọc TypeAccount của từng tài khoản
            var settings = _accountSettingsService.Load(accountId);
            int acctType = settings.General?.TypeAccount ?? 0;
            if (acctType != targetType) continue;

            PasteSettingsToAccount(accountId);
            count++;
        }

        if (count == 0)
        {
            MessageBox.Show($"Không tìm thấy tài khoản nào có TypeAccount = {targetType}!", "Quản lý Config", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        MessageBox.Show($"Đã dán config cho {count} tài khoản có TypeAccount = {targetType}!", "Quản lý Config", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // Reload UI cho tài khoản đang chọn
        if (dgvAccounts.CurrentRow != null)
            LoadAccountSettings(dgvAccounts.CurrentRow);
    }

    /// <summary>
    /// Helper: Dán clipboard vào một accountId cụ thể.
    /// Giữ nguyên proxy (ProxyType, ProxyAddress) và TypeAccount của tài khoản đích.
    /// Deep-clone để tránh shared reference.
    /// </summary>
    private void PasteSettingsToAccount(int targetAccountId)
    {
        if (_clipboardSettings == null) return;

        // Deep-clone clipboard
        var json = System.Text.Json.JsonSerializer.Serialize(
            _clipboardSettings, PanelJsonContext.Default.AccountSettingsRoot);
        var newSettings = System.Text.Json.JsonSerializer.Deserialize(
            json, PanelJsonContext.Default.AccountSettingsRoot) ?? new AccountSettingsRoot();

        // Bảo toàn proxy và TypeAccount riêng của tài khoản đích
        var existingSettings = _accountSettingsService.Load(targetAccountId);
        if (newSettings.General == null) newSettings.General = new GeneralSettings();
        newSettings.General.UseProxy     = existingSettings.General?.UseProxy ?? true;
        newSettings.General.ProxyType    = existingSettings.General?.ProxyType ?? 0;
        newSettings.General.ProxyAddress = existingSettings.General?.ProxyAddress ?? "";
        newSettings.General.TypeAccount  = existingSettings.General?.TypeAccount ?? 0;

        // Bảo toàn Daily Metrics (không nên copy sang tài khoản khác)
        newSettings.Daily = existingSettings.Daily ?? new DailyMetrics();

        _accountSettingsService.Save(targetAccountId, newSettings);

        // Nếu tài khoản đang online thì gửi commands luôn
        SendAllSavedSettingsToClient(targetAccountId);
    }

    // ---- BIẾN GIAO DIỆN POPUP SKILL TRAIN ----
    private ToolStripDropDown _skillDropdownTrain;
    private ToolStripControlHost _skillHostTrain;
    private System.Windows.Forms.Panel _pnlSkillPopupTrain;
    private TabControl _tabSkillTrain;
    private TabPage _tabEarthTrain, _tabNamekTrain, _tabXaydaTrain;
    private Button btnShowSkillPopupTrain;
    private CheckBox chkUseShieldUnderHpTrain;
    private NumericUpDown nudShieldHpPercentTrain;
    private Label lblShieldHpPercentTrain;
    private CheckBox chkTrainEarthDragon, chkTrainEarthKame, chkTrainEarthTdhs, chkTrainEarthThoiMien, chkTrainEarthDctt, chkTrainEarthKhien, chkTrainEarthKaioken;
    private CheckBox chkTrainNamekLienHoan, chkTrainNamekDemon, chkTrainNamekMakan, chkTrainNamekDeTrung, chkTrainNamekKhien;
    private CheckBox chkTrainXaydaGalick, chkTrainXaydaAntomic, chkTrainXaydaBienHinh, chkTrainXaydaTtNl, chkTrainXaydaKhien;

    private void InitializeSkillDropdownTrain()
    {
        // 1. Tạo Nút mới ngụy trang và ẩn nút cũ
        btnShowSkillPopupTrain = new Button
        {
            Text = "Danh sách Skill sử dụng",
            Size = new Size(180, 32),
            Location = new Point(chkUseKaiokenLienHoan.Location.X, chkUseKaiokenLienHoan.Location.Y - 3), // dời lên ít hơn để căn giữa với nút cao 26
            UseVisualStyleBackColor = true,
            Cursor = Cursors.Hand
        };
        chkUseKaiokenLienHoan.Visible = false; // Ẩn checkbox cũ
        if (chkUseKaiokenLienHoan.Parent != null)
        {
            chkUseKaiokenLienHoan.Parent.Controls.Add(btnShowSkillPopupTrain);
        }

        _pnlSkillPopupTrain = new System.Windows.Forms.Panel
        {
            Size = new Size(240, 265),
            Padding = new Padding(0),
            BackColor = SystemColors.Control,
            BorderStyle = BorderStyle.FixedSingle
        };
        _tabSkillTrain = new TabControl { Location = new Point(0, 0), Size = new Size(240, 220), ItemSize = new Size(60, 20), SizeMode = TabSizeMode.Fixed };
        
        _tabEarthTrain = new TabPage("Trái Đất") { AutoScroll = true, BackColor = Color.White };
        _tabNamekTrain = new TabPage("Namếc") { AutoScroll = true, BackColor = Color.White };
        _tabXaydaTrain = new TabPage("Xayda") { AutoScroll = true, BackColor = Color.White };

        // Earth
        chkTrainEarthDragon = CreateTrainCheckbox("Đấm Dragon [0]", 0);
        chkTrainEarthKame = CreateTrainCheckbox("Kamejoko [1]", 1);
        chkTrainEarthTdhs = CreateTrainCheckbox("Thái Dương [6]", 2);
        chkTrainEarthKaioken = CreateTrainCheckbox("Kaioken [9]", 3);
        chkTrainEarthKhien = CreateTrainCheckbox("Khiên Năng Lượng [19]", 4);
        chkTrainEarthDctt = CreateTrainCheckbox("Dịch Chuyển [20]", 5);
        chkTrainEarthThoiMien = CreateTrainCheckbox("Thôi Miên [22]", 6);
        _tabEarthTrain.Controls.AddRange(new Control[] { chkTrainEarthDragon, chkTrainEarthKame, chkTrainEarthTdhs, chkTrainEarthKaioken, chkTrainEarthKhien, chkTrainEarthDctt, chkTrainEarthThoiMien });

        // Namek
        chkTrainNamekDemon = CreateTrainCheckbox("Đấm Demon [2]", 0);
        chkTrainNamekMakan = CreateTrainCheckbox("Masenko [3]", 1);
        chkTrainNamekDeTrung = CreateTrainCheckbox("Đẻ Trứng [12]", 2);
        chkTrainNamekLienHoan = CreateTrainCheckbox("Liên Hoàn [17]", 3);
        chkTrainNamekKhien = CreateTrainCheckbox("Khiên Năng Lượng [19]", 4);
        _tabNamekTrain.Controls.AddRange(new Control[] { chkTrainNamekDemon, chkTrainNamekMakan, chkTrainNamekDeTrung, chkTrainNamekLienHoan, chkTrainNamekKhien });

        // Xayda
        chkTrainXaydaGalick = CreateTrainCheckbox("Đấm Galick [4]", 0);
        chkTrainXaydaAntomic = CreateTrainCheckbox("Antomic [5]", 1);
        chkTrainXaydaTtNl = CreateTrainCheckbox("Tái Tạo [8]", 2);
        chkTrainXaydaBienHinh = CreateTrainCheckbox("Biến Khỉ [13]", 3);
        chkTrainXaydaKhien = CreateTrainCheckbox("Khiên Năng Lượng [19]", 4);
        _tabXaydaTrain.Controls.AddRange(new Control[] { chkTrainXaydaGalick, chkTrainXaydaAntomic, chkTrainXaydaTtNl, chkTrainXaydaBienHinh, chkTrainXaydaKhien });

        _tabSkillTrain.TabPages.Add(_tabEarthTrain);
        _tabSkillTrain.TabPages.Add(_tabNamekTrain);
        _tabSkillTrain.TabPages.Add(_tabXaydaTrain);

        // 2. Thêm row "Khiên khi HP dưới" vào cuối popup, bên dưới TabControl skill (y=222)
        chkUseShieldUnderHpTrain = new CheckBox
        {
            Text = "Khiên khi HP dưới:",
            AutoSize = true,
            Location = new Point(6, 228),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        chkUseShieldUnderHpTrain.FlatAppearance.BorderSize = 0;
        chkUseShieldUnderHpTrain.CheckedChanged += TrainSettingsControl_Changed;

        nudShieldHpPercentTrain = new NumericUpDown
        {
            Location = new Point(160, 226),
            Size = new Size(45, 23),
            Minimum = 0,
            Maximum = 100,
            Value = 30,
            BorderStyle = BorderStyle.FixedSingle
        };
        nudShieldHpPercentTrain.ValueChanged += TrainSettingsControl_Changed;

        lblShieldHpPercentTrain = new Label
        {
            Text = "%",
            AutoSize = true,
            Location = new Point(208, 230)
        };

        _pnlSkillPopupTrain.Controls.Add(_tabSkillTrain);
        _pnlSkillPopupTrain.Controls.Add(chkUseShieldUnderHpTrain);
        _pnlSkillPopupTrain.Controls.Add(nudShieldHpPercentTrain);
        _pnlSkillPopupTrain.Controls.Add(lblShieldHpPercentTrain);

        _skillHostTrain = new ToolStripControlHost(_pnlSkillPopupTrain)
        {
            AutoSize = false,
            Size = _pnlSkillPopupTrain.Size,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        _skillDropdownTrain = new ToolStripDropDown
        {
            AutoSize = true,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };
        _skillDropdownTrain.Items.Add(_skillHostTrain);
        
        // Nút binding
        btnShowSkillPopupTrain.Click += (s, e) => _skillDropdownTrain.Show(btnShowSkillPopupTrain, new Point(0, btnShowSkillPopupTrain.Height));
    }

    private CheckBox CreateTrainCheckbox(string text, int index)
    {
        var chk = new CheckBox 
        { 
            Text = text, 
            AutoSize = true, 
            Location = new Point(10, 10 + index * 25), 
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        chk.FlatAppearance.BorderSize = 0;
        chk.CheckedChanged += TrainSettingsControl_Changed;
        return chk;
    }
}

