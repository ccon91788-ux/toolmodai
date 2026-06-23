using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Assets.src.g;
using NRO_v247.Mods.Xmap;

namespace NRO_v247.Mods.UpZin;

/// <summary>
/// Auto làm nhiệm vụ 0-3 tân thủ.
/// Luồng: Tạo char (prefix + random) → ĐK thông tin → Làm xong nhiệm vụ nhặt Sao Băng → Tạo char tiếp.
/// </summary>
public class AutoUpZinTo700kFeature : IAutoFeature
{
    private readonly Random _random = new();

    // ── Tạo char (giống AutoUpZinFeature) ──────────────────────────────
    private string _prefix = string.Empty;
    private int _targetClass = -1;
    private long _nextCreateAttemptAtMs;
    private int _retryCount;
    private string _lastRequestedName = string.Empty;
    private long _nextInfoSubmitAtMs;

    private const int MaxRetry = 50;
    private const long RetryDelayMs = 1400L;
    private const long InfoSubmitDelayMs = 1800L;

    // ── Làm NV (kế thừa logic từ AutoNewbieTaskFeature) ────────────────
    private bool _enabled;
    private bool _isTanSat;
    private bool _isPicking;
    private long _lastTimePickedItem;
    private bool _isHarvestingPean;
    private long _lastTimeEatPean;
    private long _lastTimeAutoPoint;
    private long _lastTimeCheckTN;
    private long _lastTN;
    protected long _lastTimeRevive;
    private long _lastTimeCheckNV;
    private long _lastTimeHarvestMenu;

    protected int _myMinMP = 15;
    protected int _myMinHP = 15;
    protected int _minHPMob = 0;
    protected int _maxHPMob = int.MaxValue;
    protected int _minPeans = 0;

    protected bool _isNhapCodeTanThu;
    protected bool _isPKKarinSama;
    protected bool _isPKT77;
    protected bool _isTeleT77 = true;
    protected long _nv0MapReadyTime = 0L;

    private int _charCompletedCount;

    // ── IAutoFeature ───────────────────────────────────────────────────
    public bool IsActive => _enabled;
    public string CurrentState
    {
        get
        {
            if (!_enabled) return "Tắt";
            string scrName = GameCanvas.currentScreen?.GetType().Name ?? "null";
            if (GameCanvas.currentScreen is CreateCharScr)
                return $"Tạo NV ({_retryCount + 1}) | Hoàn thành: {_charCompletedCount}";
            if (GameCanvas.currentScreen is RegisterScreen)
                return "Đang ĐK thông tin...";
            var task = Char.myCharz()?.taskMaint;
            if (task != null)
            {
                string progress = $"{task.taskId} - {task.index}";
                if (task.counts != null && task.index < task.counts.Length)
                    progress += $" - {task.count}/{task.counts[task.index]}";
                return $"NV: {progress} | Xong: {_charCompletedCount}";
            }
            return $"Chờ ở {scrName} | Xong: {_charCompletedCount}";
        }
    }
    public bool IsUtilityTask => false;
    public int Priority => 1000;
    public bool IsRequested => _enabled
        && !(GameCanvas.currentScreen is LoginScr)
        && !(GameCanvas.currentScreen is ServerListScreen);

    // ── Settings từ Panel ──────────────────────────────────────────────
    public void ApplySettingsFromPanel(bool enabled, string prefix, int targetClass)
    {
        string normalizedPrefix = NormalizePrefix(prefix);
        bool unchanged = _enabled == enabled
            && string.Equals(_prefix, normalizedPrefix, StringComparison.Ordinal)
            && _targetClass == targetClass;

        if (unchanged)
            return;

        _enabled = enabled;
        _prefix = normalizedPrefix;
        _targetClass = targetClass;
        ResetRuntime();

        if (_enabled)
        {
            string classStr = _targetClass switch { 0 => "Trái Đất", 1 => "Namek", 2 => "Xayda", _ => "Ngẫu nhiên" };
            LogToPanel($"Auto làm NV 0-3 ON - prefix: {(string.IsNullOrEmpty(_prefix) ? "(fallback)" : _prefix)} - Phái: {classStr}");
        }
        else
            LogToPanel("Auto làm NV 0-3 OFF");
    }

    public void DisableFromPanel()
    {
        _enabled = false;
        ResetRuntime();
        LogToPanel("Auto làm NV 0-3 OFF");
    }

    // ── Main Update ─────────────────────────────────────────────────────
    public void Update()
    {
        if (!_enabled) return;

        // Phase 1: Đang ở màn hình tạo char
        if (GameCanvas.currentScreen is CreateCharScr)
        {
            HandleCreateCharScreen();
            return;
        }

        // Phase 2: Đang ở màn hình đăng ký thông tin
        if (GameCanvas.currentScreen is RegisterScreen registerScreen)
        {
            HandleRegisterScreen(registerScreen);
            return;
        }

        // Phase 3: Đang ở màn hình nhập gifcode tân thủ
        if (_isNhapCodeTanThu && GameCanvas.currentScreen is ClientInput)
        {
            ClientInput ci = (ClientInput)GameCanvas.currentScreen;
            if (ci.tf != null && ci.tf.Length > 0)
                ci.tf[0].setText("tan thu nro");
            TField tField = new();
            tField.setText("tan thu nro");
            Service.gI().sendClientInput(new TField[1] { tField });
            GameScr.gI().switchToMe();
            ClientInput.instance = null;
            Char.chatPopup = null;
            _isNhapCodeTanThu = false;
            return;
        }

        // Phase 4: Đang ở GameScr → làm NV
        if (GameCanvas.currentScreen is GameScr)
        {
            var myChar = Char.myCharz();
            if (myChar == null || myChar.taskMaint == null) return;

            // Detect menu "Nhận quà" / "Từ chối" → set flag chờ ClientInput
            if (!_isNhapCodeTanThu
                && GameCanvas.menu.showMenu && GameCanvas.menu.menuItems != null
                && GameCanvas.menu.menuItems.size() == 2)
            {
                Command command1 = (Command)GameCanvas.menu.menuItems.elementAt(0);
                Command command2 = (Command)GameCanvas.menu.menuItems.elementAt(1);
                if (command1?.caption != null && command2?.caption != null
                    && command1.caption.ToLower().Contains("nhận quà") && command2.caption.ToLower().Contains("từ chối"))
                {
                    GameCanvas.menu.menuSelectedItem = 0;
                    command1.performAction();
                    _isNhapCodeTanThu = true;
                }
            }

            UpdateGameplay(myChar);
        }
    }

    // ── Xử lý server message (tên trùng) ───────────────────────────────
    public void OnServerMessage(string fullText)
    {
        if (!_enabled || string.IsNullOrWhiteSpace(fullText)) return;

        string msg = fullText.ToLowerInvariant();
        if (!IsDuplicateNameMessage(msg)) return;

        if (GameCanvas.currentScreen is not CreateCharScr) return;

        _nextCreateAttemptAtMs = mSystem.currentTimeMillis() + RetryDelayMs;
        LogToPanel("Auto làm NV 0-3: Tên đã tồn tại, random lại...");
    }

    // ── Tạo char (giống AutoUpZinFeature) ──────────────────────────────
    private void HandleCreateCharScreen()
    {
        long now = mSystem.currentTimeMillis();
        if (now < _nextCreateAttemptAtMs) return;

        if (_retryCount >= MaxRetry)
        {
            LogToPanel("Auto làm NV 0-3: Dừng tạo char (vượt quá số lần thử).");
            _enabled = false;
            return;
        }

        string nextName = BuildCharacterName();
        if (string.IsNullOrEmpty(nextName))
        {
            _nextCreateAttemptAtMs = now + RetryDelayMs;
            return;
        }

        _lastRequestedName = nextName;
        _retryCount++;
        _nextCreateAttemptAtMs = now + RetryDelayMs;

        try
        {
            if (CreateCharScr.tAddName != null)
                CreateCharScr.tAddName.setText(nextName);

            int gender = _targetClass;
            if (gender < 0 || gender > 2)
                gender = _random.Next(3);
            int hairIndex = _random.Next(CreateCharScr.hairID[gender].Length);
            short hairId = (short)CreateCharScr.hairID[gender][hairIndex];

            Service.gI().createChar(nextName, (sbyte)gender, hairId);

            string classStr = gender switch { 0 => "Trái Đất", 1 => "Namek", 2 => "Xayda", _ => "?" };
            LogToPanel($"Auto làm NV 0-3: Tạo char lần {_retryCount} - {nextName} ({classStr})");
        }
        catch (Exception ex)
        {
            LogToPanel("Auto làm NV 0-3: Lỗi tạo char - " + ex.Message);
        }
    }

    private void HandleRegisterScreen(RegisterScreen registerScreen)
    {
        long now = mSystem.currentTimeMillis();
        if (now < _nextInfoSubmitAtMs) return;

        try
        {
            registerScreen.tfUser?.setText("Phạm Văn A");
            registerScreen.tfSodt?.setText("0312345678");
            registerScreen.tfNgay?.setText("1");
            registerScreen.tfThang?.setText("1");
            registerScreen.tfNam?.setText("2000");

            registerScreen.perform(2008, null);
            _nextInfoSubmitAtMs = now + InfoSubmitDelayMs;
            LogToPanel("Auto làm NV 0-3: Đã điền thông tin và bấm OK.");

            // Reset state cho char mới
            _isTanSat = false;
            _isHarvestingPean = false;
            _isNhapCodeTanThu = false;
            _isPKKarinSama = false;
            _isPKT77 = false;
            _isTeleT77 = true;
            _nv0MapReadyTime = 0L;
            _myMinHP = 15;
            _myMinMP = 15;
            _minHPMob = 0;
            _maxHPMob = int.MaxValue;
            _minPeans = 0;
        }
        catch (Exception ex)
        {
            _nextInfoSubmitAtMs = now + InfoSubmitDelayMs;
            LogToPanel("Auto làm NV 0-3: Lỗi điền form - " + ex.Message);
        }
    }

    // ── Gameplay: nhặt Sao Băng ────────────────────────────────────────
    private void UpdateGameplay(Char myChar)
    {
        // Tự ăn đậu
        if (GameScr.hpPotion <= 0 && (myChar.cMP < 15 || myChar.cHP < 15))
        {
            if (!_isHarvestingPean)
                _isHarvestingPean = true;
            _isTanSat = false;
            _isPKKarinSama = false;
            _isPKT77 = false;
            if (TileMap.mapID != myChar.cgender + 21 && !IsXmapActing())
                StartXmap(myChar.cgender + 21);
        }

        if ((myChar.cMP < _myMinMP || myChar.cHP < _myMinHP) && !_isHarvestingPean
            && (TileMap.mapID != myChar.cgender + 21 || myChar.taskMaint.taskId < 3)
            && mSystem.currentTimeMillis() - _lastTimeEatPean > 2000
            && (_minPeans <= 0 || GameScr.hpPotion >= _minPeans))
        {
            _lastTimeEatPean = mSystem.currentTimeMillis();
            GameScr.gI().doUseHP();
        }

        // Hồi sinh
        if ((myChar.isDie || myChar.cHP <= 0) && mSystem.currentTimeMillis() - _lastTimeRevive > 1000)
        {
            _lastTimeRevive = mSystem.currentTimeMillis();
            Service.gI().returnTownFromDead();
        }

        // Xử lý thu hoạch / nhận đậu ở nhà.
        // Chỉ dùng menu index 0 để thu hoạch đậu thần,
        // không nâng cấp cây / không dùng index khác để tránh lệch menu.
        if (TileMap.mapID == myChar.cgender + 21)
        {
            if (!_isHarvestingPean && _minPeans > 0 && GameScr.hpPotion < _minPeans)
                _isHarvestingPean = true;

            if (GameScr.vItemMap.size() > 0)
            {
                ItemMap itemMap = (ItemMap)GameScr.vItemMap.elementAt(0);
                if (mSystem.currentTimeMillis() - _lastTimePickedItem >= 550)
                {
                    _lastTimePickedItem = mSystem.currentTimeMillis();
                    Service.gI().pickItem(itemMap.itemMapID);
                }
            }

            if (_minPeans <= 0)
            {
                if (GameScr.gI().magicTree.currPeas == 0
                    || myChar.cgender == 1 && GameScr.hpPotion >= 30
                    || myChar.cgender != 1 && GameScr.hpPotion >= 20)
                    _isHarvestingPean = false;
            }
            else if (GameScr.hpPotion >= _minPeans)
                _isHarvestingPean = false;

            if (myChar.taskMaint.taskId >= 2 && GameScr.gI().magicTree.currPeas > 0
                && (myChar.cgender == 1 && GameScr.hpPotion < 30 || myChar.cgender != 1 && GameScr.hpPotion < 20)
                && mSystem.currentTimeMillis() - _lastTimeHarvestMenu > 500)
            {
                _lastTimeHarvestMenu = mSystem.currentTimeMillis();
                Service.gI().openMenu(4);
                Service.gI().confirmMenu(4, 0);
            }

            if (GameCanvas.menu.showMenu)
                GameCanvas.menu.doCloseMenu();
        }

        // Tự mặc đồ tốt nhất
        if (!_isHarvestingPean && !IsXmapActing())
        {
            long nowEquip = mSystem.currentTimeMillis();
            if (nowEquip - _lastAutoEquipMs >= AutoEquipIntervalMs)
            {
                _lastAutoEquipMs = nowEquip;
                TryAutoEquipBestItems(myChar);
            }
        }

        // Tan sat đánh quái cho task 1 và task 2
        if (myChar.taskMaint.taskId == 1 || myChar.taskMaint.taskId == 2)
        {
            if (!IsXmapActing() && !_isNhapCodeTanThu && !_isHarvestingPean
                && myChar.cHP > 1 && !GameScr.gI().isBagFull()
                && (_minPeans <= 0 || GameScr.hpPotion >= _minPeans))
            {
                ModBootstrap.TrainFeature.IsUpZinOverride = _isTanSat;
                if (_isTanSat)
                {
                    ModBootstrap.TrainFeature.UpZinMinHp = _minHPMob;
                    ModBootstrap.TrainFeature.UpZinMaxHp = _maxHPMob;
                    ModBootstrap.TrainFeature.Update();
                }
                ModBootstrap.AutoPickFeature.IsUpZinOverride = true;
            }
            else
            {
                ModBootstrap.TrainFeature.IsUpZinOverride = false;
                ModBootstrap.AutoPickFeature.IsUpZinOverride = false;
            }
        }

        // Xử lý NV
        if (!_isNhapCodeTanThu && !_isHarvestingPean
            && mSystem.currentTimeMillis() - _lastTimeCheckNV > 500
            && myChar.cHP > 1 && !GameScr.gI().isBagFull())
        {
            _lastTimeCheckNV = mSystem.currentTimeMillis();
            AutoNV();
        }
    }

    // ── Auto NV: làm xong nhiệm vụ nhặt Sao Băng thì tạo char tiếp ──
    private void AutoNV()
    {
        var task = Char.myCharz()?.taskMaint;
        if (task == null) return;

        // Sau khi làm xong task 3, tắt feature
        if (task.taskId > 3)
        {
            _charCompletedCount++;
            LogToPanel($"✓ Char hoàn thành task 0-3, tổng hoàn thành: {_charCompletedCount}");
            _enabled = false;
            CleanupTrainOverride();
            LogToPanel("Auto làm NV 0-3: Đã hoàn thành và tự tắt");
            return;
        }

        if (task.taskId == 0)
            AutoNV0();
        else if (task.taskId == 1)
            AutoNV1();
        else if (task.taskId == 2)
            AutoNV2();
        else if (task.taskId == 3)
            AutoNV3();
    }

    // ── NV 0→6 (copy từ AutoNewbieTaskFeature) ─────────────────────────
    private void AutoNV0()
    {
        if (TileMap.mapID >= 39 && TileMap.mapID <= 41)
        {
            long now = mSystem.currentTimeMillis();
            if (_nv0MapReadyTime == 0L) { _nv0MapReadyTime = now + 2000L; return; }
            if (now < _nv0MapReadyTime) return;

            var myChar = Char.myCharz();
            Waypoint waypoint = (Waypoint)TileMap.vGo.elementAt(0);
            myChar.cy -= 30;
            Service.gI().charMove();
            TeleportMyChar(waypoint.maxX - 20, waypoint.maxY);
        }
        else
        {
            if (_nv0MapReadyTime != 0L) _nv0MapReadyTime = 0L;

            if (TileMap.mapID >= 21 && TileMap.mapID <= 23)
            {
                var myChar = Char.myCharz();
                if (myChar.taskMaint.index == 2)
                {
                    if (myChar.cgender == 0) Service.gI().openMenu(0);
                    if (myChar.cgender == 1) Service.gI().openMenu(2);
                    if (myChar.cgender == 2) Service.gI().openMenu(1);
                }
                else if (myChar.taskMaint.index == 3)
                {
                    if (myChar.cgender == 0)
                    {
                        if (Math.Abs(myChar.cx - 85) <= 10 && Math.Abs(myChar.cy - 336) <= 10) Service.gI().getItem(0, 0);
                        else TeleportMyChar(85, 336);
                    }
                    if (myChar.cgender == 2)
                    {
                        if (Math.Abs(myChar.cx - 94) <= 10 && Math.Abs(myChar.cy - 336) <= 10) Service.gI().getItem(0, 0);
                        else TeleportMyChar(94, 336);
                    }
                    if (myChar.cgender == 1)
                    {
                        if (Math.Abs(myChar.cx - 638) <= 10 && Math.Abs(myChar.cy - 336) <= 10) Service.gI().getItem(0, 0);
                        else TeleportMyChar(638, 336);
                    }
                }
                else if (myChar.taskMaint.index == 4)
                {
                    Service.gI().openMenu(4);
                    Service.gI().confirmMenu(4, 0);
                }
                else if (myChar.taskMaint.index == 5)
                {
                    if (GameCanvas.menu.showMenu) GameCanvas.menu.doCloseMenu();
                    if (myChar.cgender == 0) Service.gI().openMenu(0);
                    if (myChar.cgender == 1) Service.gI().openMenu(2);
                    if (myChar.cgender == 2) Service.gI().openMenu(1);
                }
            }
        }
    }

    private void AutoNV1()
    {
        var myChar = Char.myCharz();
        if (myChar.taskMaint.index == 0)
        {
            myChar.npcFocus = null;
            if (TileMap.mapID >= 21 && TileMap.mapID <= 23)
            {
                if (!IsXmapActing()) StartXmap(myChar.cgender * 7);
            }
            else if (TileMap.mapID == myChar.cgender * 7) _isTanSat = true;
        }
        else if (myChar.taskMaint.index == 1)
        {
            _isTanSat = false;
            myChar.mobFocus = null; myChar.itemFocus = null; myChar.charFocus = null;
            if (!IsXmapActing() && TileMap.mapID != myChar.cgender + 21) StartXmap(myChar.cgender + 21);
            else if (TileMap.mapID == myChar.cgender + 21)
            {
                if (myChar.cgender == 0) Service.gI().openMenu(0);
                if (myChar.cgender == 1) Service.gI().openMenu(2);
                if (myChar.cgender == 2) Service.gI().openMenu(1);
            }
        }
    }

    private void AutoNV2()
    {
        var myChar = Char.myCharz();
        if (myChar.taskMaint.index == 0)
        {
            if (TileMap.mapID != myChar.cgender * 7 + 1)
            {
                if (!IsXmapActing()) { _isTanSat = false; StartXmap(myChar.cgender * 7 + 1); }
            }
            else if (!_isTanSat) _isTanSat = true;
        }
        else if (myChar.taskMaint.index == 1)
        {
            myChar.mobFocus = null; _isTanSat = false;
            if (TileMap.mapID != myChar.cgender + 21)
            {
                if (!IsXmapActing()) StartXmap(myChar.cgender + 21);
            }
            else
            {
                if (myChar.cgender == 0) Service.gI().openMenu(0);
                if (myChar.cgender == 1) Service.gI().openMenu(2);
                if (myChar.cgender == 2) Service.gI().openMenu(1);
            }
        }
    }

    private void AutoNV3()
    {
        var myChar = Char.myCharz();
        if (myChar.taskMaint.index == 0) Service.gI().upPotential(2, 1);
        if (myChar.taskMaint.index == 1)
        {
            if (TileMap.mapID == myChar.cgender + 42)
            {
                if (myChar.cgender == 0) TeleportMyChar(149, 288);
                if (myChar.cgender == 1) TeleportMyChar(126, 264);
                if (myChar.cgender == 2) TeleportMyChar(156, 288);
            }
            else if (!IsXmapActing()) StartXmap(myChar.cgender + 42);
        }
        else if (myChar.taskMaint.index == 2)
        {
            if (TileMap.mapID != myChar.cgender + 21)
            {
                if (!IsXmapActing()) StartXmap(myChar.cgender + 21);
            }
            else
            {
                if (myChar.cgender == 0) Service.gI().openMenu(0);
                if (myChar.cgender == 1) Service.gI().openMenu(2);
                if (myChar.cgender == 2) Service.gI().openMenu(1);
            }
        }
    }

    private void AutoNV4to6()
    {
        var myChar = Char.myCharz();
        if (_myMinHP != 30) _myMinHP = 30;
        if (_myMinMP != 15) _myMinMP = 15;
        if (_maxHPMob != 500) _maxHPMob = 500;
        if (_minHPMob != 499) _minHPMob = 499;

        if (myChar.taskMaint.index < 3)
        {
            int mapID = 2 + myChar.cgender * 7;
            if (myChar.taskMaint.index == 1) { if (myChar.cgender == 0) mapID = 9; else mapID = 2; }
            if (myChar.taskMaint.index == 2) { if (myChar.cgender == 2) mapID = 9; else mapID = 16; }
            if (TileMap.mapID == mapID) _isTanSat = true;
            else { _isTanSat = false; if (!IsXmapActing()) StartXmap(mapID); }
        }
        else
        {
            _isTanSat = false;
            _maxHPMob = int.MaxValue; _minHPMob = 0;
            if (TileMap.mapID != myChar.cgender + 21)
            {
                if (!IsXmapActing()) StartXmap(myChar.cgender + 21);
            }
            else
            {
                if (myChar.cgender == 0) Service.gI().openMenu(0);
                if (myChar.cgender == 1) Service.gI().openMenu(2);
                if (myChar.cgender == 2) Service.gI().openMenu(1);
            }
        }
    }

    private bool IsXmapActing() => ModBootstrap.XmapFeature != null && ModBootstrap.XmapFeature.IsXmaping();
    private void StartXmap(int mapId) => ModBootstrap.XmapFeature?.StartGoToMapFromPanel(mapId);

    private void TeleportMyChar(int x, int y)
    {
        var myChar = Char.myCharz();
        if (myChar == null) return;
        myChar.currentMovePoint = null;
        myChar.cx = x; myChar.cy = y;
        Service.gI().charMove();
        myChar.cy = y + 1; Service.gI().charMove();
        myChar.cy = y; Service.gI().charMove();
        myChar.cxSend = x; myChar.cySend = y;
    }

    private void TeleportMyChar(Char myChar) { if (myChar != null) TeleportMyChar(myChar.cx, myChar.cy); }
    private void TeleportMyChar(Mob mob) { if (mob != null) TeleportMyChar(mob.x, mob.y); }

    private void CleanupTrainOverride()
    {
        ModBootstrap.TrainFeature.IsUpZinOverride = false;
        ModBootstrap.AutoPickFeature.IsUpZinOverride = false;
    }

    private void ResetRuntime()
    {
        _retryCount = 0;
        _nextCreateAttemptAtMs = 0L;
        _lastRequestedName = string.Empty;
        _nextInfoSubmitAtMs = 0L;
        _isTanSat = false;
        _isHarvestingPean = false;
        _isNhapCodeTanThu = false;
        _isPKKarinSama = false;
        _isPKT77 = false;
        _nv0MapReadyTime = 0L;
    }

    private void ResetForNextChar()
    {
        // Reset toàn bộ state để tạo char tiếp
        _retryCount = 0;
        _nextCreateAttemptAtMs = 0L;
        _lastRequestedName = string.Empty;
        _nextInfoSubmitAtMs = 0L;
        _isTanSat = false;
        _isHarvestingPean = false;
        _isNhapCodeTanThu = false;
        _isPKKarinSama = false;
        _isPKT77 = false;
        _nv0MapReadyTime = 0L;
        _myMinHP = 15; _myMinMP = 15;
        _minHPMob = 0; _maxHPMob = int.MaxValue;
        _minPeans = 0;
    }

    // ── Tạo tên char (prefix max 8 ký tự + random suffix) ──────────────
    private string BuildCharacterName()
    {
        string prefix = _prefix;
        if (prefix.Length < 3) prefix = "zin";
        if (prefix.Length > 8) prefix = prefix.Substring(0, 8);

        int minSuffix = Math.Max(1, 5 - prefix.Length);
        int maxSuffix = Math.Max(minSuffix, 15 - prefix.Length);
        int suffixLen = _random.Next(minSuffix, Math.Min(maxSuffix, 8) + 1);
        return prefix + RandomAlphaNumeric(suffixLen);
    }

    private static string NormalizePrefix(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        char[] chars = raw.Trim().Where(char.IsLetterOrDigit).Take(8).ToArray();
        return new string(chars).ToLowerInvariant();
    }

    private string RandomAlphaNumeric(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++) sb.Append(chars[_random.Next(chars.Length)]);
        return sb.ToString();
    }

    private static bool IsDuplicateNameMessage(string message)
    {
        return (message.Contains("tồn tại") && message.Contains("tên"))
            || message.Contains("name already")
            || message.Contains("already exists")
            || message.Contains("duplicate name");
    }

    // ── Auto Equip (copy từ AutoNewbieTaskFeature) ─────────────────────
    private long _lastAutoEquipMs;
    private const long AutoEquipIntervalMs = 2000L;
    private const sbyte BagBodyType = 4;

    private void TryAutoEquipBestItems(Char me)
    {
        if (me?.arrItemBag == null || me.arrItemBody == null) return;
        if (Char.ischangingMap || Controller.isStopReadMessage) return;

        for (int targetType = 0; targetType <= 4; targetType++)
        {
            int bestBagSlot = -1; sbyte bestLevel = -1;
            for (int i = 0; i < me.arrItemBag.Length; i++)
            {
                Item bagItem = me.arrItemBag[i];
                if (bagItem?.template == null || bagItem.quantity <= 0) continue;
                if (bagItem.template.type != targetType) continue;
                if (bagItem.template.level > bestLevel) { bestLevel = bagItem.template.level; bestBagSlot = i; }
            }
            if (bestBagSlot < 0) continue;
            Item bodyItem = (targetType < me.arrItemBody.Length) ? me.arrItemBody[targetType] : null;
            if (bodyItem?.template == null) { Service.gI().getItem(BagBodyType, (sbyte)bestBagSlot); return; }
            if (bestLevel > bodyItem.template.level) { Service.gI().getItem(BagBodyType, (sbyte)bestBagSlot); return; }
        }

        int[] simpleTypes = { Item.TYPE_HAIR, Item.TYPE_MOUNT_VIP };
        foreach (int equipType in simpleTypes)
        {
            Item bodySlot = (equipType < me.arrItemBody.Length) ? me.arrItemBody[equipType] : null;
            if (bodySlot?.template != null) continue;
            for (int i = 0; i < me.arrItemBag.Length; i++)
            {
                Item bagItem = me.arrItemBag[i];
                if (bagItem?.template == null || bagItem.quantity <= 0) continue;
                if (bagItem.template.type != equipType) continue;
                Service.gI().getItem(BagBodyType, (sbyte)i); return;
            }
        }
    }

    private void LogToPanel(string message)
    {
        if (string.IsNullOrEmpty(AutoLogin.idClientSocket)) return;
        SocketGame.SendMessage($"LOG|{AutoLogin.idClientSocket}|{message}");
    }
}
