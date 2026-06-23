using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods.Boss
{
    public enum BossState { Idle, Scouting, Hunting, SyncWait, WaitingForBoss, AntiAdmin }

    public partial class AutoBossFeature : IAutoFeature, ICleanupFeature
    {
        // Khi hoạt động, Auto Boss dùng cờ ưu tiên giành giật vòng lặp chính.
        public bool IsUtilityTask => false;
        public int Priority => 80;

        private bool _isActive;
        public bool IsActive => _isActive;

        // Handler lắng nghe VipChat thông báo Boss
        private BossVipChatHandler _vipChatHandler;
        
        // Nhả luồng khi Idle và không đi dò liên tục, để AutoTrain farm quái bình thường.
        // scoutOnVipChat đứng yên chờ → cũng nhả luồng khi Idle.
        // AntiAdmin + antiBanAttackMobs: nhả luồng để Train tự chạy qua AutoMod (không giành Priority).
        public bool IsRequested
        {
            get
            {
                if (!_isActive) return false;
                if (_state == BossState.SyncWait) return false;
                if (_state == BossState.AntiAdmin && _antiBanAttackMobs)
                {
                    if (mSystem.currentTimeMillis() - _antiAdminStartTime >= _actualAntiAdminSec * 1000L) return true;
                    return false;
                }
                return _state != BossState.Idle || _autoScoutContinuous;
            }
        }

        private BossState _state = BossState.Idle;

        /// <summary>
        /// Định dạng HP rút gọn: 4,500,000 → 4tr5 | 1,200,000,000 → 1tỷ2
        /// </summary>
        private static string FormatHp(long hp)
        {
            if (hp >= 1_000_000_000L)
            {
                long tys = hp / 1_000_000_000L;
                long rem = (hp % 1_000_000_000L) / 100_000_000L;
                return rem > 0 ? $"{tys}tỷ{rem}" : $"{tys}tỷ";
            }
            if (hp >= 1_000_000L)
            {
                long trs = hp / 1_000_000L;
                long rem = (hp % 1_000_000L) / 100_000L;
                return rem > 0 ? $"{trs}tr{rem}" : $"{trs}tr";
            }
            if (hp >= 1_000L)
                return $"{hp / 1000}k";
            return hp.ToString();
        }

        /// <summary>
        /// Tìm boss (Char) đang thấy trong map để lấy HP thực tế.
        /// </summary>
        private Char FindLiveBossChar()
        {
            try
            {
                var vChar = GameScr.vCharInMap;
                if (vChar == null) return null;
                for (int i = 0; i < vChar.size(); i++)
                {
                    Char c = (Char)vChar.elementAt(i);
                    if (c == null || c.isDie || c.cHP <= 0) continue;
                    string name = BossScoutingHelper.ResolveMobName(c) ?? "";
                    if (!string.IsNullOrEmpty(_targetBossName) && name.Equals(_targetBossName, StringComparison.OrdinalIgnoreCase))
                        return c;
                }
            }
            catch { }
            return null;
        }

        public string CurrentState
        {
            get
            {
                if (!_isActive) return string.Empty;

                if (ModBootstrap.XmapFeature?.IsXmaping() == true)
                    return string.Empty; // Xmap vẽ đè

                switch (_state)
                {
                    case BossState.Idle:
                        if (_scoutOnVipChat) return "Đang chờ thông báo Boss";
                        return "Tạm nghỉ";

                    case BossState.SyncWait:
                        return "Chờ đồng bộ Hub";

                    case BossState.Scouting:
                    {
                        // Tính CD còn lại trước khi đổi khu tiếp theo
                        bool hasTdlt = TdltController.HasBuff();
                        long cdTotal = hasTdlt ? ScoutZoneCooldownWithTdltMs : ScoutZoneCooldownWithoutTdltMs;
                        long elapsed  = mSystem.currentTimeMillis() - _lastScoutZoneChangeMs;
                        long cdRemain = cdTotal - elapsed;
                        if (cdRemain < 0) cdRemain = 0;
                        long cdSec = (long)System.Math.Ceiling(cdRemain / 1000.0);

                        string cdStr = cdSec > 0 ? $" CD {cdSec}s" : "";
                        return $"Dò Boss Map {TileMap.mapID}-{TileMap.zoneID}{cdStr}";
                    }

                    case BossState.Hunting:
                    {
                        if (_targetZone >= 0 && TileMap.zoneID != _targetZone)
                            return $"Đang về khu {_targetZone} map {_targetMap}...";

                        if (_goTieBoss)
                        {
                            // Tìm CD kỹ năng trói
                            Char me = Char.myCharz();
                            Skill tieSkill = SkillHelper.GetTieSkill(me);
                            if (tieSkill != null)
                            {
                                long cdRemain = SkillHelper.GetSkillCooldownRemain(tieSkill);
                                if (cdRemain > 0)
                                    return $"Đang trói {_targetBossName} CD {cdRemain / 1000 + 1}s";
                            }
                            return $"Đang trói {_targetBossName}";
                        }

                        // Đang tấn công → tìm HP boss
                        Char bossChar = FindLiveBossChar();
                        if (bossChar != null)
                            return $"{_targetBossName} - {FormatHp(bossChar.cHP)}";
                        return $"Đang tấn công {_targetBossName}";
                    }

                    case BossState.WaitingForBoss:
                    {
                        if (_targetZone >= 0 && TileMap.zoneID != _targetZone)
                            return $"Đang về khu {_targetZone} map {_targetMap}...";

                        long waitTime = (mSystem.currentTimeMillis() - _waitStartTime) / 1000;
                        long rem = 5 - waitTime;
                        if (rem < 0) rem = 0;
                        return $"Chờ boss hồi sinh ({rem}s)";
                    }

                    case BossState.AntiAdmin:
                        return "Đang anti admin/ban";

                    default:
                        return $"Săn Boss ({_state})";
                }
            }
        }

        // Cấu hình từ Panel truyền xuống
        private bool _goAttackBoss;
        private bool _goTieBoss;
        private bool _autoScoutContinuous;
        private bool _scoutOnVipChat;
        private bool _limitMap;
        private bool _limitZone;
        private bool _enableAntiBan;
        private string _antiBanChatContents = "";

        // Cấu hình Item khi săn Boss
        private bool _itemTdlt;      // TDLT: dùng cả khi Dò và Chiến đấu
        private bool _itemCuongNo;   // Cuồng nộ: chỉ khi Chiến đấu
        private bool _itemBoHuyet;   // Bổ huyết: chỉ khi Chiến đấu
        private bool _itemGiapXen;   // Giáp xên: chỉ khi Chiến đấu
        private bool _itemAnDanh;    // Ẩn danh: chỉ khi Chiến đấu
        private bool _itemCo4La;     // Cỏ 4 lá: chỉ khi Chiến đấu
        private bool _itemThucAn;    // Thức ăn: chỉ khi Chiến đấu

        private List<int> _mapsToScout = new List<int>();
        private List<int> _zonesToScout = new List<int>();
        private List<string> _bossNames = new List<string>();

        // Biến điều hướng Target
        private int _targetMap = -1;
        private int _targetZone = -1;
        private string _targetBossName = "";
        
        private int _gobackX = -1;
        private int _gobackY = -1;
        
        private int _scoutMapIndex = 0; // Ghi nhớ đang dò tới map số mấy trong list
        private long _waitStartTime = 0; // Đếm ngược 10s chờ khởi sinh
        private long _antiAdminStartTime = 0; // Đếm ngược X giây anti-admin
        private long _lastChatTime = 0;

        private bool _limitHpAbove;
        private long _hpAboveValue;
        private bool _limitHpBelow;
        private long _hpBelowValue;

        private bool _enableFinishingMove;
        private long _finishingMoveHpValue;
        private bool _isExecutingFinishing;
        private long _finishingStartTime;

        private bool _enableTimeSchedule;
        private string _timeSchedulesStr = "";
        private List<(TimeSpan Start, TimeSpan End)> _timeSchedules = new List<(TimeSpan, TimeSpan)>();

        private bool _unequipTrainingArmor = false;
        private long _lastUnequipCheckMs = 0;

        private int _bossCtId = -1;
        private int _bossVpdlId = -1;
        private int _bossPetId = -1;

        private bool _hasSwappedBossGear = false;
        private int _ctTrain_ID = -1;
        private int _vpdlTrain_ID = -1;
        private int _petTrain_ID = -1;

        public void ApplySettingsFromPanel(bool enabled, bool goAttackBoss = false, bool goTieBoss = false, 
            bool autoScoutContinuous = false, bool scoutOnVipChat = false, 
            bool limitMap = false, string mapRanges = "", 
            bool limitZone = false, string zoneRanges = "", 
            string bossNames = "", 
            bool enableAntiBan = false, int antiAdminSec = 30, string chatContents = "",
            bool itemTdlt = false, bool itemCuongNo = false, bool itemBoHuyet = false,
            bool itemGiapXen = false, bool itemAnDanh = false, bool itemCo4La = false, bool itemThucAn = false,
            bool antiBanAttackMobs = false, string allowedSkillsStr = "NONE", bool useShieldUnderHp = false, int shieldHpPercent = 30,
            bool limitHpAbove = false, long hpAboveValue = 0, bool limitHpBelow = false, long hpBelowValue = 0,
            bool enableFinishingMove = false, long finishingMoveHpValue = 0,
            bool enableTimeSchedule = false, string timeSchedulesStr = "", bool unequipTrainingArmor = false,
            int bossCtId = -1, int bossVpdlId = -1, int bossPetId = -1)
        {
            _isActive = enabled;
            if (!enabled)
            {
                Reset();
                return;
            }

            _goAttackBoss = goAttackBoss;
            _goTieBoss = goTieBoss;
            _autoScoutContinuous = autoScoutContinuous;
            _scoutOnVipChat = scoutOnVipChat;
            _limitMap = limitMap;
            _limitZone = limitZone;
            _enableAntiBan = enableAntiBan;
            _antiAdminSecSetting = antiAdminSec;
            _antiBanAttackMobs = antiBanAttackMobs;
            _antiBanChatContents = chatContents;
            
            if (_state == BossState.Scouting && !_autoScoutContinuous)
            {
                ModBootstrap.XmapFeature?.StopFromPanel();
                ChangeState(BossState.Idle);
            }
            
            _limitHpAbove = limitHpAbove;
            _hpAboveValue = hpAboveValue;
            _limitHpBelow = limitHpBelow;
            _hpBelowValue = hpBelowValue;

            _enableFinishingMove = enableFinishingMove;
            _finishingMoveHpValue = finishingMoveHpValue;
            
            _enableTimeSchedule = enableTimeSchedule;
            _timeSchedulesStr = timeSchedulesStr;
            _timeSchedules.Clear();
            if (_enableTimeSchedule && !string.IsNullOrWhiteSpace(_timeSchedulesStr))
            {
                var parts = _timeSchedulesStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var times = part.Split('-');
                    if (times.Length == 2 && TimeSpan.TryParse(times[0].Trim(), out var start) && TimeSpan.TryParse(times[1].Trim(), out var end))
                    {
                        _timeSchedules.Add((start, end));
                    }
                }
            }

            _unequipTrainingArmor = unequipTrainingArmor;
            _bossCtId = bossCtId;
            _bossVpdlId = bossVpdlId;
            _bossPetId = bossPetId;

            _itemTdlt    = itemTdlt;
            _itemCuongNo = itemCuongNo;
            _itemBoHuyet = itemBoHuyet;
            _itemGiapXen = itemGiapXen;
            _itemAnDanh  = itemAnDanh;
            _itemCo4La   = itemCo4La;
            _itemThucAn  = itemThucAn;

            _allowedSkillIds.Clear();
            if (!string.IsNullOrEmpty(allowedSkillsStr) && allowedSkillsStr != "NONE")
            {
                foreach (var s in allowedSkillsStr.Split(','))
                {
                    if (int.TryParse(s, out int sid))
                        _allowedSkillIds.Add(sid);
                }
            }
            _useShieldUnderHp = useShieldUnderHp;
            _shieldHpPercent = shieldHpPercent;

            ParseList(mapRanges, _mapsToScout);
            ParseList(zoneRanges, _zonesToScout);
            
            _bossNames.Clear();
            if (!string.IsNullOrEmpty(bossNames))
            {
                foreach (var b in bossNames.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    _bossNames.Add(b.Trim().ToLower());
                }
            }

            if (_vipChatHandler == null)
                _vipChatHandler = new BossVipChatHandler(this, _bossNames, _mapsToScout);
            _vipChatHandler.SetScoutOnVipChat(_scoutOnVipChat, _limitMap);

            // Bắt đầu dò ngay nếu tính năng này được bật
            if (_isActive && _state == BossState.Idle && _autoScoutContinuous)
            {
                _scoutZoneIndex = 0;
                _scoutMapIndex = 0;
                ChangeState(BossState.Scouting);
            }
        }
        private static readonly System.Text.RegularExpressions.Regex _rangeRegex =
            new System.Text.RegularExpressions.Regex(@"(\d+)\s*-\s*(\d+)|(\d+)", System.Text.RegularExpressions.RegexOptions.Compiled);

        private void ParseList(string input, List<int> output)
        {
            output.Clear();
            if (string.IsNullOrWhiteSpace(input)) return;

            System.Text.RegularExpressions.MatchCollection matches = _rangeRegex.Matches(input);
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                if (m.Groups[1].Success && m.Groups[2].Success)
                {
                    if (int.TryParse(m.Groups[1].Value, out int from) &&
                        int.TryParse(m.Groups[2].Value, out int to))
                    {
                        for (int i = Math.Min(from, to); i <= Math.Max(from, to); i++)
                            output.Add(i);
                    }
                }
                else if (m.Groups[3].Success)
                {
                    if (int.TryParse(m.Groups[3].Value, out int single))
                        output.Add(single);
                }
            }
        }

        private bool IsInActiveSchedule()
        {
            if (!_enableTimeSchedule || _timeSchedules.Count == 0) return true;

            TimeSpan now = DateTime.Now.TimeOfDay;
            foreach (var schedule in _timeSchedules)
            {
                if (schedule.Start <= schedule.End)
                {
                    if (now >= schedule.Start && now <= schedule.End) return true;
                }
                else
                {
                    // Qua ngày (Ví dụ: 22:00-02:00)
                    if (now >= schedule.Start || now <= schedule.End) return true;
                }
            }
            return false;
        }

        public void Update()
        {
            if (!_isActive) return;

            if (!IsInActiveSchedule())
            {
                // Nếu ngoài khung giờ, ép về Idle để trả luồng cho các tính năng cũ (Train, ngẫu nhiên...)
                if (_state != BossState.Idle)
                {
                    ChangeState(BossState.Idle);
                    LockMapTransition(false);
                }
                return;
            }

            // Dọn dẹp cache khoá map nếu người chơi lỡ chết văng về nhà hoặc chuyển sang map khác
            if (_cachedVGoMapId != -1 && TileMap.mapID != _cachedVGoMapId)
            {
                _cachedVGo = null;
                _cachedVGoMapId = -1;
            }

            // Nếu không phải đang săn Boss, chắc chắn phải mở khoá Map
            if (_state != BossState.Hunting)
            {
                LockMapTransition(false);
            }

            switch (_state)
            {
                case BossState.Idle:
                    DoIdle();
                    break;
                case BossState.Scouting:
                    DoScouting();
                    break;
                case BossState.Hunting:
                    DoHunting();
                    break;
                case BossState.SyncWait:
                    DoSyncWait();
                    break;
                case BossState.WaitingForBoss:
                    DoWaitingForBoss();
                    break;
                case BossState.AntiAdmin:
                    DoAntiAdmin();
                    break;
            }
        }

        // private long _lastSyncWaitCheck = 0;

        private void DoSyncWait()
        {
            // Trong trạng thái này, AutoBoss nhường luồng (vì IsRequested = false / check Idle), 
            // nhưng thực ra sẽ đứng nghỉ chờ lệnh từ Hub qua socket (bắn lệnh BOSS_HUNT_START hoặc BOSS_RESET).
            // Nếu có thiết lập, có thể cho phép tự vệ hoặc buf TDLT trong lúc chờ.
            BossItemHelper.TickForScouting(_itemTdlt);
        }

        private void DoIdle()
        {
            // Nếu chỉ dò theo VIP Chat, thì đứng im - NotifyCatcher sẽ gọi StartHuntingAt() khi nhận thông báo
        }

        // ─── SCOUTING ─────────────────────────────────────────────────────────────

        private long _lastScoutZoneChangeMs = 0;
        private int _scoutZoneIndex = 0;
        private long _scoutStartMs = 0; // Fix: timeout vòng dò
        // Delay đổi khu khi dò: giống AutoTrainFeature (5500ms có TDLT, 10500ms không có)
        private const long ScoutZoneCooldownWithTdltMs    = 5500;
        private const long ScoutZoneCooldownWithoutTdltMs = 10500;
        private const long ScoutTimeoutMs = 300_000; // 5 phút tự reset nếu không tìm thấy

        private void DoScouting()
        {
            Char me = Char.myCharz();
            if (me == null || me.meDead || Char.ischangingMap || Controller.isStopReadMessage) return;

            if (ModBootstrap.XmapFeature?.IsXmaping() == true) return;

            // Fix: Timeout 5 phút chỉ áp dụng khi KHÔNG DÒ LIÊN TỤC (tức là đi theo Vipchat dẫn tới Scouting)
            if (!_autoScoutContinuous && _scoutStartMs > 0 && mSystem.currentTimeMillis() - _scoutStartMs > ScoutTimeoutMs)
            {
                GameLogger.SendLog("BOSS", "Quá 5 phút tìm không thấy Boss → Báo hết giờ!");
                SocketGame.SendMessage($"BOSS_SCOUT_DONE|{AutoLogin.idClientSocket}|{AutoLogin.server}|{_targetMap}");
                _scoutStartMs = 0;
                ChangeState(BossState.SyncWait);
                return;
            }

            // Dùng TDLT khi Scouting để bay nhanh
            BossItemHelper.TickForScouting(_itemTdlt);

            // Bước 1: Kiểm tra Boss ngay tại khu hiện tại
            Char boss = BossScoutingHelper.FindBossInCurrentZone(_bossNames);
            if (boss != null)
            {
                // TÌM THẤY BOSS! Lưu target và chuẩn bị
                _targetMap = TileMap.mapID;
                _targetZone = TileMap.zoneID;
                _targetBossName = BossScoutingHelper.ResolveMobName(boss);
                _gobackX = boss.cx;
                _gobackY = boss.cy;
                GameLogger.SendLog("BOSS", $"Đã tìm thấy boss {_targetBossName} gần ({_gobackX}, {_gobackY}) khu {_targetZone} map {_targetMap}");

                // Gửi sự kiện cho Hub (Panel) để bắn cho tất cả các ac ở cùng khu vực (Server) nhận biết
                SocketGame.SendMessage($"BOSS_FOUND|{AutoLogin.idClientSocket}|{AutoLogin.server}|{_targetMap}|{_targetZone}|{_targetBossName}");

                if (_goAttackBoss || _goTieBoss)
                {
                    ChangeState(BossState.Hunting);
                    GameLogger.SendLog("BOSS", "Chuyển chế độ tấn công/trói boss");
                }
                else
                {
                    ChangeState(BossState.SyncWait);
                    GameLogger.SendLog("BOSS", "Chuyển chế độ chờ đồng bộ (scouter)");
                }
                return;
            }

            // Bước 2: Nếu giới hạn Map, đổi sang Map tiếp theo
            if (_limitMap && _mapsToScout.Count > 0)
            {
                int targetMap = _mapsToScout[_scoutMapIndex % _mapsToScout.Count];

                if (TileMap.mapID != targetMap)
                {
                    // Đang sai map -> ra lệnh Xmap
                    ModBootstrap.XmapFeature?.StartGoToMapFromPanel(targetMap);
                    return;
                }

                // Đang đúng map -> Đổi khu
                long now = mSystem.currentTimeMillis();
                bool hasTdltBuff = TdltController.HasBuff();
                long cooldown = hasTdltBuff ? ScoutZoneCooldownWithTdltMs : ScoutZoneCooldownWithoutTdltMs;

                if (now - _lastScoutZoneChangeMs >= cooldown)
                {
                    int maxZone = 0;
                    try { maxZone = (GameScr.gI().zones != null) ? (GameScr.gI().zones.Length - 1) : 0; } catch { }

                    bool shouldNextMap = false;
                    if (_limitZone && _zonesToScout.Count > 0)
                    {
                        if (_scoutZoneIndex >= _zonesToScout.Count) shouldNextMap = true;
                    }
                    else
                    {
                        if (maxZone > 0 && _scoutZoneIndex > maxZone) shouldNextMap = true;
                        else if (maxZone == 0 && _scoutZoneIndex > 14) shouldNextMap = true;
                    }

                    if (shouldNextMap)
                    {
                        // Đã quét hết các khu cấu hình / hoặc tất cả khu hiện có, nhảy map tiếp theo
                        _scoutZoneIndex = 0;
                        _scoutMapIndex++;

                        if (_scoutMapIndex >= _mapsToScout.Count)
                        {
                            if (_autoScoutContinuous)
                            {
                                _scoutMapIndex = 0;
                                GameLogger.SendLog("BOSS", $"Đã quét hết vòng Map. Lặp lại dò liên tục...");
                            }
                            else
                            {
                                GameLogger.SendLog("BOSS", $"Đã quét xong danh sách Map. Đang đợi đồng bộ...");
                                SocketGame.SendMessage($"BOSS_SCOUT_DONE|{AutoLogin.idClientSocket}|{AutoLogin.server}|{TileMap.mapID}");
                                ChangeState(BossState.SyncWait);
                                return;
                            }
                        }
                    }
                    else
                    {
                        int targetZone = (_limitZone && _zonesToScout.Count > 0) ? _zonesToScout[_scoutZoneIndex] : _scoutZoneIndex;
                        Service.gI().requestChangeZone(targetZone, -1);
                        _scoutZoneIndex++;
                    }
                    
                    _lastScoutZoneChangeMs = now;
                }
            }
            // Bước 2b: Không giới hạn map theo list, nhưng biết _targetMap cụ thể (ví dụ: từ VipChat)
            // → Xmap đến đó và quét từng khu cho đến khi tìm thấy boss
            else if (_targetMap >= 0)
            {
                if (TileMap.mapID != _targetMap)
                {
                    ModBootstrap.XmapFeature?.StartGoToMapFromPanel(_targetMap);
                    return;
                }

                // Đã đến đúng map → quét khu theo cooldown
                long now = mSystem.currentTimeMillis();
                bool hasTdltBuff = TdltController.HasBuff();
                long cooldown = hasTdltBuff ? ScoutZoneCooldownWithTdltMs : ScoutZoneCooldownWithoutTdltMs;

                if (now - _lastScoutZoneChangeMs >= cooldown)
                {
                    int maxZone = 0;
                    try { maxZone = (GameScr.gI().zones != null) ? (GameScr.gI().zones.Length - 1) : 0; } catch { }

                    bool shouldFinishScouting = false;
                    if (_limitZone && _zonesToScout.Count > 0)
                    {
                        if (_scoutZoneIndex >= _zonesToScout.Count) shouldFinishScouting = true;
                    }
                    else
                    {
                        if (maxZone > 0 && _scoutZoneIndex > maxZone) shouldFinishScouting = true;
                        else if (maxZone == 0 && _scoutZoneIndex > 14) shouldFinishScouting = true;
                    }

                    if (shouldFinishScouting)
                    {
                        // Đã quét hết khu mà không thấy boss → báo SCOUT_DONE và về SyncWait
                        GameLogger.SendLog("BOSS", $"[VipChat] Đã quét hết khu map {_targetMap}.");
                        SocketGame.SendMessage($"BOSS_SCOUT_DONE|{AutoLogin.idClientSocket}|{AutoLogin.server}|{_targetMap}");
                        
                        _targetMap = -1;
                        _targetZone = -1;
                        _targetBossName = "";
                        _scoutZoneIndex = 0;

                        if (_autoScoutContinuous)
                        {
                            GameLogger.SendLog("BOSS", $"Chờ tín hiệu tiếp theo...");
                            ChangeState(BossState.Idle);
                        }
                        else
                        {
                            ChangeState(BossState.SyncWait);
                        }
                    }
                    else
                    {
                        int targetZone = (_limitZone && _zonesToScout.Count > 0) ? _zonesToScout[_scoutZoneIndex] : _scoutZoneIndex;
                        Service.gI().requestChangeZone(targetZone, -1);
                        _scoutZoneIndex++;
                    }
                    _lastScoutZoneChangeMs = now;
                }
            }
            // Nếu không giới hạn map và không có _targetMap, đứng chờ thông báo VipChat (ScoutOnVipChat)
        }

        // ─── HUNTING ──────────────────────────────────────────────────────────────

        private long _lastAttackMs = 0;
        private const long AttackCooldownMs = 800;

        private void DoHunting()
        {
            Char me = Char.myCharz();
            if (me == null || me.meDead) return;

            if (_unequipTrainingArmor && mSystem.currentTimeMillis() - _lastUnequipCheckMs > 5000)
            {
                _lastUnequipCheckMs = mSystem.currentTimeMillis();
                int bodyIndex = GetTrainingArmorBodyIndex(me);
                if (bodyIndex >= 0)
                {
                    Service.gI().getItem(5, (sbyte)bodyIndex); // 5 là dỡ từ body vào bag
                    GameLogger.SendLog("BOSS", "Tháo giáp luyện tập để tấn công Boss.");
                }
            }

            // Nếu chưa ở đúng map Boss -> Xmap bay tới
            if (_targetMap >= 0 && TileMap.mapID != _targetMap)
            {
                LockMapTransition(false); // Phải mở map ra mới Xmap được
                ModBootstrap.XmapFeature?.StartGoToMapFromPanel(_targetMap);
                return; // Đang bay thì block các logic dưới (đổi khu, đánh)
            }
            
            // Nếu đã đến đúng map Boss mà Xmap vẫn còn báo chạy -> dừng Xmap
            if (ModBootstrap.XmapFeature?.IsXmaping() == true)
            {
                ModBootstrap.XmapFeature.StopFromPanel();
            }

            // Nếu chưa ở đúng Khu Boss -> đổi khu
            if (_targetZone >= 0 && TileMap.zoneID != _targetZone)
            {
                LockMapTransition(false); // Để an toàn vGo
                if (!Char.ischangingMap)
                {
                    Service.gI().requestChangeZone(_targetZone, -1);
                }
                return; // KHÔNG THỂ săn khi chưa tới đúng khu! Bắt buộc return.
            }

            // Phòng hờ nếu trùng khu nhưng game đang tải màn hình chuyển map (nhân vật chưa hạ cánh)
            if (Char.ischangingMap || Controller.isStopReadMessage)
                return;

            // Đang ở đúng map và khu -> Tìm Boss
            Char boss = BossScoutingHelper.FindBossInCurrentZone(_bossNames);

            // CHỈ XỬ LÝ BOSS CHẾT KHI ĐANG Ở ĐÚNG MAP VÀ KHU MỤC TIÊU
            if (TileMap.mapID == _targetMap && TileMap.zoneID == _targetZone)
            {
                if (boss == null)
                {
                    // Boss không thấy trong tầm nhìn (hoặc đã chết)
                    // Nếu đang ở xa điểm đã ghim (Goback XY), phải chạy tới đó trước khi kết luận Boss chết
                    if (_gobackX >= 0 && _gobackY >= 0)
                    {
                        int distX = me.cx - _gobackX;
                        int distY = me.cy - _gobackY;
                        if (Math.Abs(distX) > 60 || Math.Abs(distY) > 60)
                        {
                            me.cx = _gobackX;
                            me.cy = _gobackY;
                            Service.gI().charMove();
                            return; // Chờ tick sau chạy tới nơi hoặc Boss load vào tầm nhìn
                        }
                    }

                    // Nếu đã tới được điểm GoBack mà vẫn không thấy Boss -> Boss thực sự biến mất
                    LockMapTransition(false); // Bỏ khoá map
                    GameLogger.SendLog("BOSS", $"Boss {_targetBossName} đã mất dấu tại Khu {_targetZone}, chuyển sang chế độ chờ...");
                    _waitStartTime = mSystem.currentTimeMillis();
                    ChangeState(BossState.WaitingForBoss);
                    return;
                }

                // Chứng cứ thép: Nếu Object boss tồn tại nhưng đã lăn ra chết (isDie = true hoặc cHP = 0)
                if (boss.isDie || boss.cHP <= 0 || boss.statusMe == 14 || boss.statusMe == 5)
                {
                    LockMapTransition(false);
                    GameLogger.SendLog("BOSS", $"Boss {_targetBossName} đã chết, chuyển sang chế độ chờ...");
                    _waitStartTime = mSystem.currentTimeMillis();
                    ChangeState(BossState.WaitingForBoss);
                    return;
                }
            }
            else if (boss == null)
            {
                // Nếu chưa ở đúng map/khu hoặc chưa tới nơi -> Không làm gì, để logic đổi khu xử lý
                return;
            }

            // Dùng item Chiến đấu (Cuồng nộ, Bổ huyết, Giáp xên, Ẩn danh, Cỏ 4 lá, Thức ăn, TDLT)
            BossItemHelper.TickForHunting(_itemCuongNo, _itemBoHuyet, _itemGiapXen,
                _itemAnDanh, _itemCo4La, _itemThucAn, _itemTdlt);

            // Có Boss -> ĐẨY VỀ TRẠNG THÁI KHOÁ CHUYỂN MAP ĐỂ TUYỆT ĐỐI KHÔNG BỊ VĂNG (CHỐNG DẪM HITBOX CỔNG)
            LockMapTransition(true);

            // Boss vẫn còn -> Thực hiện Đánh hoặc Trói
            _gobackX = boss.cx;
            _gobackY = boss.cy;

            // Cập nhật lại tên Boss để báo cáo đúng mục tiêu thực tế đang Focus (Tiểu Đội Sát Thủ, v.v...)
            string actualBossName = BossScoutingHelper.ResolveMobName(boss);
            if (!string.IsNullOrEmpty(actualBossName) && _targetBossName != actualBossName)
            {
                _targetBossName = actualBossName;
                GameLogger.SendLog("BOSS", $"Chuyển focus sang: {_targetBossName}");
                SocketGame.SendMessage($"BOSS_FOUND|{AutoLogin.idClientSocket}|{AutoLogin.server}|{_targetMap}|{_targetZone}|{_targetBossName}");
            }

            long now = mSystem.currentTimeMillis();

            // Luôn ghim và focus Boss (như AutoBossCL)
            me.charFocus = boss;
            me.mobFocus = null;
            me.npcFocus = null;
            me.itemFocus = null;

            // Auto tele đến boss nếu ở quá xa
            int dx = boss.cx - me.cx;
            int dy = boss.cy - me.cy;
            if (Math.Sqrt(dx * dx + dy * dy) > 30.0)
            {
                // Gọi Astar để kiểm tra đường đi. Boss chui tường => FindPath trả về null.
                int startTileX = me.cx / 24;
                int startTileY = me.cy / 24;
                int goalTileX = boss.cx / 24;
                int goalTileY = boss.cy / 24;

                var path = Astar.FindPath(new Astar.Point(startTileX, startTileY), new Astar.Point(goalTileX, goalTileY));
                
                if (path != null && path.Count > 0)
                {
                    // Tê lê đến "điểm ngoài tường nhất" - điểm cuối của con đường hợp lý nhất do Astar tính (vốn đã qua FindNearestWalkable)
                    var safeGoal = path[path.Count - 1];
                    int safePx = safeGoal.x * 24;
                    int safePy = safeGoal.y * 24 + 24; // Base pixel offset of tile

                    me.cx = safePx;
                    me.cy = safePy;
                    Service.gI().charMove();
                    if (!GameScr.canAutoPlay)
                    {
                        me.cx = safePx;
                        me.cy = safePy + 1;
                        Service.gI().charMove();
                        me.cx = safePx;
                        me.cy = safePy;
                        Service.gI().charMove();
                    }
                }
                // Ngược lại, nếu path == null thì đứng nhìn boss, KHÔNG TÊ-LÊ ĐỂ TRÁNH VÀO TƯỜNG (ban acc).
            }

            if (now - _lastAttackMs < AttackCooldownMs) return;
            
            // Không ngắt khi đang gồng KI hoặc đợi biến khỉ
            if (me.isCharge || me.isWaitMonkey) return;
            
            _lastAttackMs = now;

            // ===== KIỂM TRA LỌC HP BOSS =====
            long bossHp = boss.cHP;
            if (_limitHpAbove && bossHp <= _hpAboveValue) return;
            if (_limitHpBelow && bossHp >= _hpBelowValue) return;

            // ===== CHIÊU KẾT LIỄU =====
            if (_enableFinishingMove && bossHp < _finishingMoveHpValue)
            {
                if (ExecuteFinishingCombo(me, boss, now)) 
                    return; // Đã lọt vào chuỗi skill ks thì cấm tung chiêu khác
            }

            if (_goAttackBoss)
            {
                // Trước lần đấm đầu tiên: đảm bảo boss đã chạm đất trước khi lao vào.
                // Điều này tránh tình trạng nhân vật đuổi theo boss đang bay và không đánh được.
                if (_waitForBossOnGround && !IsBossOnGround(boss))
                {
                    return; // Boss đang bay, chờ boss hạ cánh
                }

                Skill attackSkill = ChooseAttackSkill(me);
                if (attackSkill != null)
                {
                    // Đã lao vào đấm → xóa điều kiện chờ mặt đất để không block các frame sau
                    _waitForBossOnGround = false;
                    ExecuteBossAttackSkill(me, boss, attackSkill, now);
                }
            }
            else if (_goTieBoss)
            {
                // Trói Boss: Tìm Skill Trói phù hợp theo phái và sử dụng
                DoTieBoss(me, boss);
            }
        }

        private HashSet<int> _allowedSkillIds = new HashSet<int>();
        private bool _useShieldUnderHp = false;
        private int _shieldHpPercent = 30;

        private bool ExecuteFinishingCombo(Char me, Char boss, long now)
        {
            if (me?.vSkill == null) return false;

            int gender = me.cgender;

            // Xayda
            if (gender == 2)
            {
                Skill nolo = GetSkillById(me, 14); // Tự phát nổ
                if (nolo != null)
                {
                    if (!_isExecutingFinishing)
                    {
                        if (now - nolo.lastTimeUseThisSkill >= nolo.coolDown && me.cMP >= nolo.manaUse)
                        {
                            ExecuteBossAttackSkill(me, boss, nolo, now); // Lệnh 1: Gồng tự sát
                            _isExecutingFinishing = true;
                            _finishingStartTime = now;
                            nolo.lastTimeUseThisSkill = now - nolo.coolDown; // Fake cooldown để click lại lần 2
                            return true; // Khoá chặt để bắt đầu combo
                        }
                        return false; // Chưa hồi -> nhả ra cho đánh thường
                    }
                    else
                    {
                        // Server yêu cầu > 1500ms mới cho nổ
                        if (now - _finishingStartTime >= 1600)
                        {
                            if (now - _finishingStartTime > 4000) 
                            {
                                _isExecutingFinishing = false; // Lâu quá vỡ combo
                            }
                            else 
                            {
                                ExecuteBossAttackSkill(me, boss, nolo, now); // Lệnh 2: Kick nổ
                                _isExecutingFinishing = false;
                                nolo.lastTimeUseThisSkill = now; // Tính lại cooldown chuẩn
                            }
                        }
                        return true; // Đang chạy combo thì cấm ném đá phứa
                    }
                }
            }
            // Namếc
            else if (gender == 1)
            {
                Skill masenko = GetSkillById(me, 3);
                if (masenko != null)
                {
                    if (!_isExecutingFinishing)
                    {
                        if (now - masenko.lastTimeUseThisSkill >= masenko.coolDown && me.cMP >= masenko.manaUse)
                        {
                            ExecuteBossAttackSkill(me, boss, masenko, now); // Lệnh 1: Nạp Masenko
                            _isExecutingFinishing = true;
                            _finishingStartTime = now;
                            masenko.lastTimeUseThisSkill = now - masenko.coolDown; // Fake cooldown cho lần 2
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        // Server không check time Masenko nhưng nên để nhỏ xíu để tránh nghẽn packet
                        if (now - _finishingStartTime >= 100)
                        {
                            if (now - _finishingStartTime > 3000)
                            {
                                _isExecutingFinishing = false;
                            }
                            else
                            {
                                ExecuteBossAttackSkill(me, boss, masenko, now); // Lệnh 2: Bắn Masenko
                                _isExecutingFinishing = false;
                                masenko.lastTimeUseThisSkill = now; 
                            }
                        }
                        return true;
                    }
                }
            }
            // Trái Đất
            else if (gender == 0)
            {
                Skill kame = GetSkillById(me, 1) ?? GetSkillById(me, 24);
                Skill tele = GetSkillById(me, 20);

                if (tele == null || kame == null) return false;

                if (!_isExecutingFinishing)
                {
                    // Cả Dịch Chuyển và Kame đều phải sẵn sàng và đủ Mana
                    if (now - tele.lastTimeUseThisSkill >= tele.coolDown && me.cMP >= tele.manaUse &&
                        now - kame.lastTimeUseThisSkill >= kame.coolDown && me.cMP >= (tele.manaUse + kame.manaUse))
                    {
                        ExecuteBossAttackSkill(me, boss, tele, now); // Dịch chuyển
                        _isExecutingFinishing = true;
                        _finishingStartTime = now;
                        return true;
                    }
                    return false; // Chưa hồi -> cho đấm đánh thường
                }
                else
                {
                    if (now - _finishingStartTime > 3000)
                    {
                        _isExecutingFinishing = false;
                    }
                    else
                    {
                        if (now - kame.lastTimeUseThisSkill >= kame.coolDown && me.cMP >= kame.manaUse)
                        {
                            ExecuteBossAttackSkill(me, boss, kame, now); // Chưởng ngay cho chí mạng
                            _isExecutingFinishing = false;
                        }
                    }
                    return true;
                }
            }

            return false;
        }

        private Skill ChooseAttackSkill(Char me)
        {
            if (me?.vSkill == null) return null;

            Skill prioritySkill = GetPriorityKaiokenLienHoanSkill(me);
            if (prioritySkill != null)
            {
                return prioritySkill;
            }

            // Xoá bỏ lọc theo template.isAttackSkill() cũ vì các buff như Khiên/Biến khỉ có thể bị isAttackSkill=false
            Skill skTarget = null;
            long maxScore = -1; // Để ưu tiên chọn skill có damge to nếu có nhiều skill hợp lệ

            for (int i = 0; i < me.vSkill.size(); i++)
            {
                Skill sk = (Skill)me.vSkill.elementAt(i);
                if (sk?.template == null) continue;

                int id = sk.template.id;

                // CHỈ CHẤP NHẬN SKILL ĐÃ ĐƯỢC CHECK TRÊN PANEL
                if (!_allowedSkillIds.Contains(id)) continue;

                // Nếu là khiên năng lượng và bật cơ chế kiểm soát dưới % HP
                if (id == 19 && _useShieldUnderHp)
                {
                    int currentHpPct = (int)(me.cHP * 100L / me.cHPFull);
                    if (currentHpPct > _shieldHpPercent)
                        continue; // Bỏ qua khiên nếu HP cao
                }

                long cdRemain = sk.coolDown - (mSystem.currentTimeMillis() - sk.lastTimeUseThisSkill);
                long manaRequired = (sk.template.manaUseType == 1) ? (sk.manaUse * me.cMPFull / 100) : sk.manaUse;
                
                if (cdRemain <= 0 && me.cMP >= manaRequired)
                {
                    // Lên điểm (ưu tiên skill đang focus hoặc chiêu đặc biệt)
                    long score = sk.manaUse > 0 ? sk.manaUse : 10;
                    if (me.myskill != null && me.myskill.template.id == id) score += 1000;
                    // Buffs like Khien (19), Bien khinh (13), De Trung (12), Tai Tao NL (8), Huýt sáo (21)
                    if (id == 19 || id == 13 || id == 12 || id == 8 || id == 21) score += 5000; 

                    if (score > maxScore)
                    {
                        maxScore = score;
                        skTarget = sk;
                    }
                }
            }

            return skTarget;
        }

        private Skill GetPriorityKaiokenLienHoanSkill(Char me)
        {
            if (me == null) return null;

            int[] priorityIds = me.cgender switch
            {
                0 => new[] { 9, 17 },
                1 => new[] { 17, 9 },
                _ => new[] { 9, 17 }
            };

            long now = mSystem.currentTimeMillis();

            foreach (int id in priorityIds)
            {
                if (!_allowedSkillIds.Contains(id)) continue;

                Skill sk = GetSkillById(me, id);
                if (sk?.template == null) continue;

                long cdRemain = sk.coolDown - (now - sk.lastTimeUseThisSkill);
                long manaRequired = (sk.template.manaUseType == 1) ? (sk.manaUse * me.cMPFull / 100) : sk.manaUse;
                if (cdRemain <= 0 && me.cMP >= manaRequired)
                {
                    return sk;
                }
            }

            return null;
        }

        private Skill GetSkillById(Char me, int id)
        {
            if (me?.vSkill == null) return null;
            for (int i = 0; i < me.vSkill.size(); i++)
            {
                Skill sk = (Skill)me.vSkill.elementAt(i);
                if (sk?.template != null && sk.template.id == id) return sk;
            }
            return null;
        }

        private static int GetTrainingArmorBodyIndex(Char me)
        {
            if (me?.arrItemBody == null) return -1;
            if (me.arrItemBody.Length > Item.TYPE_TRAINSUIT)
            {
                Item slotItem = me.arrItemBody[Item.TYPE_TRAINSUIT];
                if (slotItem?.template != null && slotItem.template.type == Item.TYPE_TRAINSUIT)
                    return Item.TYPE_TRAINSUIT;
            }
            for (int i = 0; i < me.arrItemBody.Length; i++)
            {
                Item item = me.arrItemBody[i];
                if (item?.template != null && item.template.type == Item.TYPE_TRAINSUIT)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Kiểm tra boss đang đứng trên mặt đất dựa theo tile map (giống logic CanWalk của A*).
        /// Tile ngay bên dưới chân boss phải là đất rắn (tileTypeAt != 0) mới được coi là đứng đất.
        /// </summary>
        private static bool IsBossOnGround(Char boss)
        {
            if (boss == null) return false;
            int tileBossX = boss.cx / TileMap.size;
            int tileBossY = boss.cy / TileMap.size;
            // Tile bên dưới chân boss: nếu != 0 thì là đất rắn → boss đang đứng trên đất
            int tileBelow = TileMap.tileTypeAt(tileBossX, tileBossY + 1);
            return tileBelow != 0;
        }

        private long _lastSpamTieMs = 0;
        private Dictionary<int, long> _bossTieStartMs = new Dictionary<int, long>();

        // Cờ chờ boss chạm đất trước lần đấm đầu tiên (reset khi bật tính năng hoặc khi bắt đầu Hunting mới)
        private bool _waitForBossOnGround = true;

        private void DoTieBoss(Char me, Char boss)
        {
            if (me?.vSkill == null || boss == null) return;

            long now = mSystem.currentTimeMillis();

            // Guard: Boss đã bị trói (holdEffID != 0) → không spam thêm
            if (boss.holdEffID != 0)
            {
                return;
            }

            // Guard: Boss phải đang đứng trên mặt đất mới được trói.
            // Nếu boss đang bay thì bỏ qua - trói boss bay gây giật cục.
            if (!IsBossOnGround(boss))
            {
                return;
            }

            Skill tieSkill = SkillHelper.GetTieSkill(me);
            if (tieSkill != null)
            {
                if (me.cMP < tieSkill.manaUse) 
                {
                    return; // Không đủ mana
                }

                // Kiểm tra cooldown thực tế của skill trói
                long cdRemain = SkillHelper.GetSkillCooldownRemain(tieSkill);
                if (cdRemain > 0)
                {
                    return; // Skill chưa hết hồi, chờ cooldown
                }

                // Rate limit tối thiểu 800ms giữa 2 lần gửi packet để tránh văng game
                if (now - _lastSpamTieMs < 800)
                {
                    return;
                }

                _lastSpamTieMs = now;

                // FIX CRASH: KHÔNG dùng doSelectSkill() vì nó gọi doFire() → sendPlayerAttack() thêm 1 lần
                // → gây DOUBLE PACKET → server đá văng.
                // Thay bằng: selectSkill (chỉ gửi packet chọn skill) + sendPlayerAttack riêng.
                if (me.myskill != tieSkill)
                {
                    me.myskill = tieSkill;
                    Service.gI().selectSkill(tieSkill.template.id);
                }

                me.charFocus = boss;
                MyVector vChar = new MyVector();
                vChar.addElement(boss);
                Service.gI().sendPlayerAttack(new MyVector(), vChar, -1);
                tieSkill.lastTimeUseThisSkill = now;
            }
        }

        private void ExecuteBossAttackSkill(Char me, Char boss, Skill skill, long now)
        {
            if (me == null || boss == null || skill?.template == null) return;

            // FIX: Không dùng doSelectSkill() vì nó trigger doFire() → gửi attack packet thừa.
            // Chỉ gửi packet selectSkill raw để server biết skill nào đang chọn.
            if (me.myskill != skill)
            {
                me.myskill = skill;
                Service.gI().selectSkill(skill.template.id);
            }

            int skillId = skill.template.id;

            // Kiểm tra các Skill buff không target và Tự Sát (TDHS, Tái tạo NL, Đẻ trứng, Biến khỉ, Tự sát, Khiên, Sao)
            if (skillId == 6 || skillId == 8 || skillId == 12 || skillId == 13 || skillId == 14 || skillId == 19 || skillId == 21)
            {
                sbyte status = 0;
                if (skillId == 8) status = 1;         // Tái tạo năng lượng
                else if (skillId == 12) status = 8;   // Đẻ trứng
                else if (skillId == 13) status = 6;   // Biến khỉ
                else if (skillId == 14) status = 7;   // Tự sát
                else if (skillId == 19) status = 9;   // Khiên năng lượng
                else if (skillId == 21) status = 10;  // Huýt sáo
                // TDHS (6) mặc định status = 0

                Service.gI().skill_not_focus(status);
            }
            else
            {
                // Skill đấm/chưởng thì gọi attack
                MyVector vChar = new MyVector();
                vChar.addElement(boss);
                Service.gI().sendPlayerAttack(new MyVector(), vChar, -1);
            }

            skill.lastTimeUseThisSkill = now;
        }

        // ─── WAITING FOR BOSS ─────────────────────────────────────────────────────

        private const long WaitForBossMs = 5_000; // 5 giây chờ Boss hồi sinh


        private void DoWaitingForBoss()
        {
            Char me = Char.myCharz();
            if (me == null || me.meDead) return;

            // Nếu chưa ở đúng map Boss -> Xmap bay tới
            if (_targetMap >= 0 && TileMap.mapID != _targetMap)
            {
                ModBootstrap.XmapFeature?.StartGoToMapFromPanel(_targetMap);
                _waitStartTime = mSystem.currentTimeMillis(); // Reset timer while travelling
                return; 
            }
            
            // Nếu đã đến đúng map Boss mà Xmap vẫn còn báo chạy -> dừng Xmap
            if (ModBootstrap.XmapFeature?.IsXmaping() == true)
            {
                ModBootstrap.XmapFeature.StopFromPanel();
            }

            // Nếu chưa ở đúng Khu Boss -> đổi khu
            if (_targetZone >= 0 && TileMap.zoneID != _targetZone && !Char.ischangingMap)
            {
                Service.gI().requestChangeZone(_targetZone, -1);
                _waitStartTime = mSystem.currentTimeMillis(); // Reset timer while getting to right zone
                return;
            }

            // Đang ở đúng map và khu thì mới tìm Boss
            long now = mSystem.currentTimeMillis();
            // Kiểm tra Boss xuất hiện lại trong khu
            Char boss = BossScoutingHelper.FindBossInCurrentZone(new List<string> { _targetBossName });
            if (boss != null)
            {
                GameLogger.SendLog("BOSS", $"Đã xuất hiện boss mới trong khu");
                _waitStartTime = 0;
                ChangeState(BossState.Hunting);
                return;
            }

            // Hết 5s chưa thấy -> Báo Panel BOSS_DEAD
            if (now - _waitStartTime >= WaitForBossMs)
            {
                GameLogger.SendLog("BOSS", "Đã 5s không thấy boss hồi sinh. Báo lên Hub...");
                SocketGame.SendMessage($"BOSS_DEAD|{AutoLogin.idClientSocket}|{AutoLogin.server}|{_targetMap}");
                
                // Trở về SyncWait chờ Panel phán quyết BOSS_RESET hoặc BOSS_ANTIADMIN_START
                ChangeState(BossState.SyncWait);
            }
        }

        // ─── ANTI-ADMIN ───────────────────────────────────────────────────────────

        private int _chatLineIndex = 0;
        private string[] _chatLines = null;
        private long _nextChatMs = 0;

        private int _antiAdminSecSetting = 30;
        private bool _antiBanAttackMobs = false;
        private int _actualAntiAdminSec = 30;

        private void EnterAntiAdmin()
        {
            _antiAdminStartTime = mSystem.currentTimeMillis();
            _lastChatTime = 0;

            if (_antiBanAttackMobs)
            {
                // Khi anti-ban cần đánh quái: bật lại Train nếu đang tắt, nhưng KHÔNG mutate trực tiếp.
                // Dùng ResumeTrainAfterAction() để Train chạy bình thường qua AutoMod luồng chính.
                // Boss (Priority=80) sẽ nhả luồng ActionTask bằng cách trả IsRequested=false tạm thời
                // → Train (Priority=0) sẽ được AutoMod lên lịch chạy tự nhiên.
                ModBootstrap.TrainFeature?.ResumeTrainAfterAction();

                _actualAntiAdminSec = _antiAdminSecSetting;
            }
            else
            {
                _actualAntiAdminSec = 30; // Hardcode 30s default if no attack mobs
            }

            ChangeState(BossState.AntiAdmin);
            GameLogger.SendLog("BOSS", $"Bắt đầu anti-ban ({_actualAntiAdminSec}s)...");
        }

        private void ExitAntiAdmin()
        {
            if (_antiBanAttackMobs)
            {
                // Tạm dừng Train lại khi hết anti-ban (nếu Boss cần tiếp tục chiếm luồng)
                ModBootstrap.TrainFeature?.PauseTrainForAction();
            }

            GameLogger.SendLog("BOSS", $"Kết thúc quá trình anti-ban, tiếp tục trạng thái ban đầu.");
            _chatLines = null; // Reset để lần sau parse lại

            SocketGame.SendMessage($"ANTI_ADMIN_DONE|{AutoLogin.idClientSocket}|{AutoLogin.server}");

            _targetMap = -1;
            _targetZone = -1;
            _targetBossName = "";
            _scoutZoneIndex = 0;

            // Nếu bật dò liên tục -> quay lại Scouting
            if (_autoScoutContinuous)
            {
                ChangeState(BossState.Scouting);
            }
            else
            {
                ChangeState(BossState.Idle);
            }
        }

        private void DoAntiAdmin()
        {
            Char me = Char.myCharz();
            if (me == null || me.meDead) return;

            long now = mSystem.currentTimeMillis();

            // Lấy danh sách chat custom
            if (_chatLines == null)
            {
                if (!string.IsNullOrEmpty(_antiBanChatContents))
                {
                    _chatLines = _antiBanChatContents.Replace("~", "\n")
                        .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                        .OrderBy(x => Res.random(100))
                        .ToArray();
                }

                if (_chatLines == null || _chatLines.Length == 0)
                {
                    _chatLines = new string[]
                    {
                        "haizz", "boss j trau the", "sdt tui m", "chet me", "nhu lol", "chan", "ae nhường", "hic", "Lag quá", "mạng cùi ghê", "bọn này phá ghê thật"
                    };
                }
                _chatLineIndex = 0;
            }

            // Giả lập chat
            if (_enableAntiBan && _chatLines.Length > 0 && now >= _nextChatMs)
            {
                string line = _chatLines[_chatLineIndex % _chatLines.Length];
                _chatLineIndex++;

                try { Service.gI().chat(line); } catch { }

                // Random 3000-5000ms
                int randomDelay = 3000 + (int)((now % 100) * 20); // 3000 - 5000ms
                _nextChatMs = now + randomDelay;
            }

            // _antiBanAttackMobs: Train đã được Resume ở EnterAntiAdmin(), sẽ tự chạy qua AutoMod.
            // Không cần (và không được) gọi TrainFeature.Update() trực tiếp ở đây.

            // Kiểm tra đã hết thời gian Anti-Admin chưa
            long antiAdminMs = (long)(_actualAntiAdminSec) * 1000L;
            if (now - _antiAdminStartTime >= antiAdminMs)
            {
                ExitAntiAdmin();
            }
        }

        // ─── HUB SYNC COMMANDS ─────────────────────────────────────────────────────

        /// <summary>Panel gọi khi có boss tìm thấy ở cùng server → Acc bắt đầu đi săn hoặc vào SyncWait.</summary>
        public void StartHuntingAt(int mapId, int zoneId, string bossName)
        {
            if (!_isActive) return;

            _targetMap = mapId;
            _targetZone = zoneId;
            _targetBossName = bossName;

            GameLogger.SendLog("BOSS", $"[HUB] Tấn công Boss {bossName} Map {mapId} Khu {(zoneId >= 0 ? zoneId.ToString() : "?")}");

            if (_goAttackBoss || _goTieBoss)
            {
                if (zoneId < 0)
                {
                    // Không biết khu (ví dụ: từ VipChat) → vào Scouting để quét từng khu tìm boss
                    // DoScouting sẽ tự chuyển sang Hunting khi tìm thấy boss trong vCharInMap
                    _scoutZoneIndex = 0;
                    _scoutMapIndex = 0;
                    ChangeState(BossState.Scouting);
                    GameLogger.SendLog("BOSS", "Không biết khu → vào Scouting quét khu");
                }
                else
                {
                    if (_state == BossState.WaitingForBoss || _state == BossState.SyncWait)
                    {
                        GameLogger.SendLog("BOSS", $"[HUB CALL] Nhận lệnh mới khi đang chờ → Lao đi đánh ngay!");
                    }
                    _waitStartTime = 0; // Reset bộ đếm chờ
                    ChangeState(BossState.Hunting);
                }
            }
            else
            {
                _waitStartTime = 0;
                ChangeState(BossState.SyncWait); // Acc chỉ dò, không đánh → đứng chờ reset
            }
        }

        /// <summary>Panel phát lệnh Anti-Admin toàn bộ sau khi boss lần cuối chết.</summary>
        public void StartAntiAdmin(int durationSeconds)
        {
            if (!_isActive) return;
            if (_state != BossState.SyncWait && _state != BossState.WaitingForBoss && _state != BossState.Hunting) return;

            GameLogger.SendLog("BOSS", $"[HUB] Bắt đầu Anti-Admin trong {durationSeconds}s");
            _actualAntiAdminSec = durationSeconds;
            EnterAntiAdmin();
        }

        /// <summary>Panel phát lệnh BOSS_RESET sau khi Anti-Admin xong → về Scouting/Idle.</summary>
        public void ResetStateForNewHunt()
        {
            if (!_isActive) return;

            GameLogger.SendLog("BOSS", "[HUB] Hết vòng săn, khôi phục trạng thái");
            ModBootstrap.TrainFeature?.ClearFocus();

            _targetMap = -1;
            _targetZone = -1;
            _targetBossName = "";

            // Reset zone index về 0 trước khi ChangeState(Scouting) để ChangeState tính lại spread
            _scoutZoneIndex = 0;
            _scoutMapIndex = 0;

            if (_autoScoutContinuous)
                ChangeState(BossState.Scouting);
            else
                ChangeState(BossState.Idle);
        }

        private void ChangeState(BossState newState)
        {
            if (_state == BossState.Hunting && newState != BossState.Hunting)
            {
                RestoreTrainGear();
            }

            _state = newState;

            if (newState == BossState.Hunting)
            {
                EquipBossGear();
                // Reset cờ chờ mặt đất: mỗi lần bắt đầu Hunting mới phải đợi boss hạ cánh trước khi đấm
                _waitForBossOnGround = true;
            }

            if (newState == BossState.Scouting)
            {
                // Fix 1: Ghi nhận thời điểm bắt đầu dò để timeout sau 5 phút
                _scoutStartMs = mSystem.currentTimeMillis();

                // Fix 2: Trải đều khu dò theo idClientSocket để các acc không cùng zone
                // Chỉ reset khi chuyển vào Scouting mới (tránh reset giữa chừng nếu ChangeState lại)
                if (_scoutZoneIndex == 0)
                {
                    if (int.TryParse(AutoLogin.idClientSocket, out int socketId))
                    {
                        int spread = socketId % 15;
                        if (spread > 0) _scoutZoneIndex = spread;
                    }
                }
            }
            else
            {
                // Ra khỏi Scouting → xóa timer
                _scoutStartMs = 0;
            }

            string stateName = newState switch
            {
                BossState.Idle => "Tạm nghỉ",
                BossState.Scouting => "Đang dò Boss",
                BossState.Hunting => "Đang chiến đấu",
                BossState.WaitingForBoss => "Đang chờ Boss",
                BossState.AntiAdmin => "Đang tránh Admin",
                _ => newState.ToString()
            };
            GameLogger.SendLog("BOSS", $"→ {stateName}");
        }



        /// <summary>
        /// Được gọi bởi AutoMod khi GLOBAL_AUTO_OFF để dọn dẹp trạng thái Boss.
        /// Đảm bảo map không bị khoá và Train không bị pause do Boss.
        /// </summary>
        public void Cleanup()
        {
            // Mở khoá map nếu đang khoá
            LockMapTransition(false);

            // Nếu đang AntiAdmin với attack mobs, phục hồi Train về trạng thái bình thường
            if (_state == BossState.AntiAdmin && _antiBanAttackMobs)
            {
                ModBootstrap.TrainFeature?.PauseTrainForAction();
            }

            // Không reset _isActive / _state ở đây — chỉ dọn dẹp side-effect.
            // Nếu cần full-reset, dùng Reset() (khi disabled từ Panel).
        }

        public void Reset()
        {
            _isActive = false;
            _state = BossState.Idle;
            _targetMap = -1;
            _targetZone = -1;
            _targetBossName = "";
            _gobackX = -1;
            _gobackY = -1;
            LockMapTransition(false);
            // Đảm bảo Train không còn bị pause do Boss sau khi reset
            ModBootstrap.TrainFeature?.ResumeTrainAfterAction();
        }

        // ─── LỆNH PANEL ────────────────────────────────

        // (Xóa các lệnh Consensus do đã revert về local)

        // ─── LOCK MAP TRANSITION ──────────────────────────────────────────────────
        private MyVector _cachedVGo = null;
        private int _cachedVGoMapId = -1;

        private void LockMapTransition(bool lockTran)
        {
            if (TileMap.vGo == null) return;

            if (lockTran)
            {
                // Khoá map: Xóa trắng danh sách điểm dịch chuyển
                if ((_cachedVGo == null || _cachedVGoMapId != TileMap.mapID) && TileMap.vGo.size() > 0)
                {
                    _cachedVGo = new MyVector();
                    for (int i = 0; i < TileMap.vGo.size(); i++)
                        _cachedVGo.addElement(TileMap.vGo.elementAt(i));
                    
                    _cachedVGoMapId = TileMap.mapID;
                    TileMap.vGo.removeAllElements();
                }
            }
            else
            {
                // Mở map: Khôi phục lại danh sách điểm dịch chuyển ban đầu
                if (_cachedVGo != null && _cachedVGoMapId == TileMap.mapID)
                {
                    if (TileMap.vGo.size() == 0) // Chỉ khôi phục nếu chưa bị LoadMap đè
                    {
                        for (int i = 0; i < _cachedVGo.size(); i++)
                            TileMap.vGo.addElement(_cachedVGo.elementAt(i));
                    }
                    _cachedVGo = null;
                    _cachedVGoMapId = -1;
                }
                else if (_cachedVGoMapId != TileMap.mapID)
                {
                    _cachedVGo = null;
                    _cachedVGoMapId = -1;
                }
            }
        }

        private void EquipBossGear()
        {
            if (_hasSwappedBossGear) return;
            _hasSwappedBossGear = true;

            _ctTrain_ID = -1;
            _vpdlTrain_ID = -1;
            _petTrain_ID = -1;

            Char me = Char.myCharz();
            if (me?.arrItemBody != null)
            {
                for (int i = 0; i < me.arrItemBody.Length; i++)
                {
                    Item it = me.arrItemBody[i];
                    if (it?.template != null)
                    {
                        if (it.template.type == 5) _ctTrain_ID = it.template.id;
                        else if (it.template.type == 11) _vpdlTrain_ID = it.template.id;
                        else if (it.template.type == 18) _petTrain_ID = it.template.id;
                    }
                }
            }

            if (_bossCtId >= 0) EquipItemFromBag(_bossCtId);
            if (_bossVpdlId >= 0) EquipItemFromBag(_bossVpdlId);
            if (_bossPetId >= 0) EquipItemFromBag(_bossPetId);
        }

        private void EquipItemFromBag(int targetId)
        {
            Char me = Char.myCharz();
            if (me?.arrItemBag == null) return;
            for (int i = 0; i < me.arrItemBag.Length; i++)
            {
                Item item = me.arrItemBag[i];
                if (item?.template != null && item.template.id == targetId)
                {
                    Service.gI().getItem(4, (sbyte)i); // 4 là mặc đồ
                    return;
                }
            }
        }

        private void RestoreTrainGear()
        {
            if (!_hasSwappedBossGear) return;
            _hasSwappedBossGear = false;
            
            RestoreItem(_bossCtId, _ctTrain_ID);
            RestoreItem(_bossVpdlId, _vpdlTrain_ID);
            RestoreItem(_bossPetId, _petTrain_ID);
        }

        private void RestoreItem(int bossId, int trainId)
        {
            if (bossId < 0) return; // Không cấu hình đồ Boss thì không tháo/mặc lại

            if (trainId >= 0)
            {
                EquipItemFromBag(trainId);
            }
            else
            {
                // Lúc trước cởi trần, nên giờ cũng tháo món Boss ra
                Char me = Char.myCharz();
                if (me?.arrItemBody != null)
                {
                    for (int i = 0; i < me.arrItemBody.Length; i++)
                    {
                        Item it = me.arrItemBody[i];
                        if (it?.template != null && it.template.id == bossId)
                        {
                            Service.gI().getItem(5, (sbyte)i); // 5 là tháo đồ
                            break;
                        }
                    }
                }
            }
        }
    }
}
