using System;
using System.Text;
using System.Text.RegularExpressions;
using NRO_v247.Mods.Xmap;

namespace NRO_v247.Mods.Attendance
{
    public sealed class AutoAttendanceFeature : IAutoFeature
    {
        public sealed class AttendancePanelSettings
        {
            public bool Enabled { get; set; }
            public bool AutoStart { get; set; } = true;
            public bool ClaimMonthly { get; set; } = true;
            public bool ClaimContinuous { get; set; } = true;
            public bool ClaimOnline { get; set; } = true;
            public bool ScheduleEnabled { get; set; } = false;
            public int ScheduleHour { get; set; } = 7;
            public int ScheduleMinute { get; set; }
            public int SavedOnlineClaimedCount { get; set; }
            public string SavedOnlineClaimDate { get; set; } = string.Empty;
        }

        private enum AttendanceState
        {
            Idle,
            NavigateToVillage,
            OpenNpc,
            SelectMonthly,
            SelectContinuous,
            SelectOnline,
            HandleOnline,
            ReopenOnline,
            ReopenOnlineWaitMenu,
            ReopenOnlineWaitPopup,
            Done,
            WaitNextOnlineCheck
        }

        private const int NpcId = 74;
        private readonly AttendancePanelSettings _settings = new AttendancePanelSettings();
        private AttendanceState _state = AttendanceState.Idle;
        private long _delayUntilMs;
        private long _nextOnlineCheckMs;
        private long _lastStatusMs;
        private bool _monthlyCheckedThisRun;
        private bool _continuousCheckedThisRun;
        private bool _onlineCheckedThisRun;
        private string _monthlyClaimedKey = string.Empty;
        private string _continuousClaimDate = string.Empty;
        private string _onlineClaimDate = string.Empty;
        private int _onlineClaimedCount;
        private int _nextOnlineSeconds = -1;
        private bool _canClaimOnline;
        private long _onlinePopupStartedMs;
        private int _onlinePopupOpenAttempts;
        private int _lastScheduleAutoStartDateKey = -1;

        public bool IsActive => _settings.Enabled;
        public bool IsRequested => !ShouldYieldToUpZin()
            && ((IsActive && (_state != AttendanceState.WaitNextOnlineCheck || mSystem.currentTimeMillis() >= _nextOnlineCheckMs))
            || ShouldScheduleAutoStart());
        public bool IsUtilityTask => false;
        public int Priority => 98;
        public string CurrentState => IsActive ? $"Điểm danh: {StateText}" : string.Empty;
        public string StateText { get; private set; } = "Đang tắt";
        public string MonthlyClaimedKey => _monthlyClaimedKey;
        public string ContinuousClaimDate => _continuousClaimDate;
        public string OnlineClaimDate => _onlineClaimDate;
        public int OnlineClaimedCount => _onlineClaimedCount;
        public int NextOnlineSeconds => _nextOnlineSeconds;
        public bool CanClaimOnline => _canClaimOnline;

        public void ApplySettingsFromPanel(AttendancePanelSettings settings)
        {
            if (settings == null) return;
            bool wasEnabled = _settings.Enabled;
            _settings.Enabled = settings.Enabled;
            _settings.AutoStart = settings.AutoStart;
            _settings.ClaimMonthly = settings.ClaimMonthly;
            _settings.ClaimContinuous = settings.ClaimContinuous;
            _settings.ClaimOnline = settings.ClaimOnline;
            _settings.ScheduleEnabled = settings.ScheduleEnabled;
            _settings.ScheduleHour = Math.Max(0, Math.Min(23, settings.ScheduleHour));
            _settings.ScheduleMinute = Math.Max(0, Math.Min(59, settings.ScheduleMinute));
            RestoreOnlineProgressFromPanel(settings);

            if (wasEnabled && !_settings.Enabled)
            {
                StopFromPanel();
            }
            else if (!wasEnabled && _settings.Enabled && _settings.AutoStart)
            {
                StartFromPanel();
            }
        }

        public void StartFromPanel()
        {
            _settings.Enabled = true;
            ResetRunFlagsIfNewDay();
            _state = AttendanceState.NavigateToVillage;
            _delayUntilMs = 0;
            StateText = "Đang khởi động";
            SendStatusIfDue(true);
        }

        public void StopFromPanel()
        {
            _settings.Enabled = false;
            _state = AttendanceState.Idle;
            StateText = "Đang tắt";
            _canClaimOnline = false;
            ServiceLocator.Get<IXmapService>()?.StopFromPanel();
            SendStatusIfDue(true);
        }

        public void Update()
        {
            SendStatusIfDue(false);
            if (ShouldYieldToUpZin()) return;
            TryScheduleAutoStart();
            if (!_settings.Enabled) return;
            if (!(GameCanvas.currentScreen is GameScr)) return;

            ResetRunFlagsIfNewDay();
            long now = mSystem.currentTimeMillis();
            if (now < _delayUntilMs) return;

            if (_state == AttendanceState.WaitNextOnlineCheck && now >= _nextOnlineCheckMs)
            {
                // Hết thời gian chờ mốc online: cho phép mở lại NPC để nhận mốc kế tiếp.
                // Nếu giữ true, nhánh OpenNpc sẽ bỏ qua SelectOnline và không quay lại nhận quà.
                _onlineCheckedThisRun = false;
                _state = AttendanceState.NavigateToVillage;
            }

            switch (_state)
            {
                case AttendanceState.Idle:
                    _state = AttendanceState.NavigateToVillage;
                    break;
                case AttendanceState.NavigateToVillage:
                    HandleNavigateToVillage(now);
                    break;
                case AttendanceState.OpenNpc:
                    StateText = "Mở NPC điểm danh";
                    Service.gI().openMenu(NpcId);
                    if (ShouldClaimMonthly())
                        _state = AttendanceState.SelectMonthly;
                    else if (ShouldClaimContinuous())
                        _state = AttendanceState.SelectContinuous;
                    else if (_settings.ClaimOnline && !_onlineCheckedThisRun)
                        _state = AttendanceState.SelectOnline;
                    else
                        _state = AttendanceState.Done;
                    _delayUntilMs = now + 700;
                    break;
                case AttendanceState.SelectMonthly:
                    HandleMonthly(now);
                    break;
                case AttendanceState.SelectContinuous:
                    HandleContinuous(now);
                    break;
                case AttendanceState.SelectOnline:
                    HandleOnlineSelect(now);
                    break;
                case AttendanceState.HandleOnline:
                    HandleOnlinePopup(now);
                    break;
                case AttendanceState.ReopenOnline:
                    StateText = "Mở lại mốc online kế tiếp";
                    ClickOkIfAny();
                    Service.gI().openMenu(NpcId);
                    _state = AttendanceState.ReopenOnlineWaitMenu;
                    _delayUntilMs = now + 700;
                    break;
                case AttendanceState.ReopenOnlineWaitMenu:
                    HandleReopenOnlineWaitMenu(now);
                    break;
                case AttendanceState.ReopenOnlineWaitPopup:
                    HandleReopenOnlineWaitPopup(now);
                    break;
                case AttendanceState.Done:
                    StateText = "Đã kiểm tra điểm danh";
                    _state = AttendanceState.WaitNextOnlineCheck;
                    if (_nextOnlineSeconds > 0)
                    {
                        _nextOnlineCheckMs = now + _nextOnlineSeconds * 1000L;
                    }
                    else
                    {
                        _nextOnlineCheckMs = now + 60000;
                        _nextOnlineSeconds = 60;
                    }
                    break;
            }
        }

        private static bool ShouldYieldToUpZin()
        {
            return ModBootstrap.UpZinFeature?.IsRequested == true
                || ModBootstrap.NewbieTaskFeature?.IsRequested == true
                || ModBootstrap.UpZinTo700kFeature?.IsRequested == true;
        }

        private void HandleNavigateToVillage(long now)
        {
            int targetMap = GetVillageMapId();
            if (TileMap.mapID == targetMap)
            {
                _state = AttendanceState.OpenNpc;
                return;
            }

            StateText = "Đi tới NPC điểm danh";
            var xmap = ServiceLocator.Get<IXmapService>();
            if (xmap != null && !xmap.IsXmaping())
            {
                xmap.StartGoToMapFromPanel(targetMap);
            }
            _delayUntilMs = now + 1000;
        }

        private void HandleMonthly(long now)
        {
            StateText = "Kiểm tra điểm danh tháng";
            if (SelectMenuByName("điểm danh tháng") || SelectMenuByIndex(0))
            {
                _monthlyCheckedThisRun = true;
                _monthlyClaimedKey = DateTime.Now.ToString("yyyyMM");
                _state = AttendanceState.OpenNpc;
                _delayUntilMs = now + 1200;
                SendStatusIfDue(true);
            }
            else
            {
                RetryOpenNpc(now);
            }
        }

        private void HandleContinuous(long now)
        {
            StateText = "Kiểm tra điểm danh liên tục";
            if (SelectMenuByName("điểm danh liên tục", "liên tục") || SelectMenuByIndex(1))
            {
                _continuousCheckedThisRun = true;
                _continuousClaimDate = DateTime.Now.ToString("yyyy-MM-dd");
                _state = AttendanceState.OpenNpc;
                _delayUntilMs = now + 1200;
                SendStatusIfDue(true);
            }
            else
            {
                RetryOpenNpc(now);
            }
        }

        private bool ShouldClaimMonthly()
        {
            return _settings.ClaimMonthly
                && !_monthlyCheckedThisRun
                && _monthlyClaimedKey != DateTime.Now.ToString("yyyyMM");
        }

        private bool ShouldClaimContinuous()
        {
            return _settings.ClaimContinuous
                && !_continuousCheckedThisRun
                && _continuousClaimDate != DateTime.Now.ToString("yyyy-MM-dd");
        }

        private void HandleOnlineSelect(long now)
        {
            StateText = "Mở điểm danh online";
            if (SelectMenuByName("điểm danh online") || SelectMenuByIndex(2))
            {
                // Menu đóng -> chờ popup online load đủ text. Lần đầu server thường trả chậm hơn popup tháng/ngày.
                _onlinePopupStartedMs = now;
                _onlinePopupOpenAttempts = 0;
                _state = AttendanceState.HandleOnline;
                _delayUntilMs = now + 1200;
            }
            else if (HasPopupText())
            {
                // Popup đang hiện thì xử lý luôn
                _state = AttendanceState.HandleOnline;
                _delayUntilMs = now + 200;
            }
            else if (now >= _delayUntilMs)
            {
                // Menu chưa kịp hiện -> retry
                Service.gI().openMenu(NpcId);
                _delayUntilMs = now + 700;
            }
        }

        private void HandleOnlinePopup(long now)
        {
            string text = GetPopupText();
            if (!IsOnlinePopupText(text))
            {
                WaitOrReopenOnlinePopup(now, AttendanceState.HandleOnline);
                return;
            }

            int waitSeconds = ParseOnlineWaitSeconds(text);
            ApplyOnlineProgressFromNpcText(text);
            if (waitSeconds > 0)
            {
                StateText = "Chờ mốc online tiếp theo";
                _onlineCheckedThisRun = true;
                _canClaimOnline = false;
                SetOnlineWaitSeconds(waitSeconds, now);
                _state = AttendanceState.WaitNextOnlineCheck;
                ClickOkIfAny();
                SendStatusIfDue(true);
                return;
            }

            if (SelectMenuByName("nhận thưởng") || SelectMenuByIndex(1))
            {
                StateText = "Đã nhận mốc online";
                if (_onlineClaimedCount < 5) _onlineClaimedCount++;
                _onlineClaimDate = DateTime.Now.ToString("yyyy-MM-dd");
                _canClaimOnline = true;

                // Chưa có dữ liệu mốc kế tiếp ngay tại frame này, tránh gửi số giây giả.
                // Chờ popup refresh rồi ParseOnlineWaitSeconds() sẽ cập nhật lại chính xác.
                _nextOnlineCheckMs = now + 60000;
                _nextOnlineSeconds = -1;
                _state = AttendanceState.ReopenOnlineWaitPopup;
                _delayUntilMs = now + 300;
                SendStatusIfDue(true);
                return;
            }

            if (SelectMenuByName("ok"))
            {
                _onlineCheckedThisRun = true;
                _state = AttendanceState.Done;
                _delayUntilMs = now + 600;
                return;
            }

            if (now - _onlinePopupStartedMs > 10000)
            {
                _onlineCheckedThisRun = true;
                _state = AttendanceState.Done;
            }
            else
            {
                _delayUntilMs = now + 500;
            }
        }

        private void RetryOpenNpc(long now)
        {
            _state = AttendanceState.OpenNpc;
            _delayUntilMs = now + 1000;
        }

        private void HandleReopenOnlineWaitMenu(long now)
        {
            if (GameCanvas.menu != null && GameCanvas.menu.showMenu && GameCanvas.menu.menuItems != null)
            {
                _state = AttendanceState.SelectOnline;
                return;
            }

            if (now >= _delayUntilMs)
            {
                Service.gI().openMenu(NpcId);
                _delayUntilMs = now + 700;
            }
        }

        private void HandleReopenOnlineWaitPopup(long now)
        {
            if (now < _delayUntilMs) return;

            // Nếu đã đặt flag này -> đã chuyển sang WaitNextOnlineCheck hoặc Done, không re-enter
            if (_onlineCheckedThisRun) return;

            string text = GetPopupText();
            if (!IsOnlinePopupText(text))
            {
                WaitOrReopenOnlinePopup(now, AttendanceState.ReopenOnlineWaitPopup);
                return;
            }

            int waitSeconds = ParseOnlineWaitSeconds(text);
            ApplyOnlineProgressFromNpcText(text);
            if (waitSeconds > 0)
            {
                StateText = "Chờ mốc online tiếp theo";
                _onlineCheckedThisRun = true;
                _canClaimOnline = false;
                SetOnlineWaitSeconds(waitSeconds, now);
                _state = AttendanceState.WaitNextOnlineCheck;
                ClickOkIfAny();
                SendStatusIfDue(true);
                return;
            }

            if (SelectMenuByName("nhận thưởng") || SelectMenuByIndex(1))
            {
                // Tiếp tục có mốc kế tiếp -> chờ popup cập nhật
                _state = AttendanceState.ReopenOnlineWaitPopup;
                _delayUntilMs = now + 300;
                return;
            }

            if (SelectMenuByName("ok"))
            {
                // Không còn mốc nào -> done
                _onlineCheckedThisRun = true;
                _state = AttendanceState.Done;
                _delayUntilMs = now + 600;
                return;
            }

            if (HasPopupText())
            {
                // Popup đang hiện nhưng chưa đúng popup online -> chờ thêm, không kết luận lỗi sớm.
                WaitOrReopenOnlinePopup(now, AttendanceState.ReopenOnlineWaitPopup);
                return;
            }

            // Popup đóng rồi, mở lại menu kiểm tra
            _state = AttendanceState.ReopenOnline;
        }

        private bool IsOnlinePopupText(string text)
        {
            string normalized = Normalize(text);
            return normalized.Contains("điểm danh online") || normalized.Contains("thỏi vàng");
        }

        private void WaitOrReopenOnlinePopup(long now, AttendanceState waitState)
        {
            if (_onlinePopupStartedMs <= 0) _onlinePopupStartedMs = now;

            if (HasPopupText())
            {
                // Popup "Trung tâm điểm danh" sau khi nhận tháng/ngày có nút OK và che menu online.
                // Đóng popup này trước rồi mở lại NPC, nếu không sẽ kẹt mãi ở trạng thái chờ dữ liệu online.
                ClickOkIfAny();
                StateText = "Đóng popup điểm danh cũ";
                _state = AttendanceState.ReopenOnline;
                _delayUntilMs = now + 500;
                return;
            }

            StateText = "Chờ dữ liệu điểm danh online";
            if (now - _onlinePopupStartedMs <= 8000)
            {
                _state = waitState;
                _delayUntilMs = now + 500;
                return;
            }

            if (_onlinePopupOpenAttempts < 2)
            {
                _onlinePopupOpenAttempts++;
                _onlinePopupStartedMs = now;
                _state = AttendanceState.ReopenOnline;
                _delayUntilMs = now + 300;
                return;
            }

            _onlineCheckedThisRun = true;
            _state = AttendanceState.Done;
        }

        private static int GetVillageMapId()
        {
            try
            {
                int gender = Char.myCharz()?.cgender ?? 0;
                if (gender == 1) return 7;
                if (gender == 2) return 14;
            }
            catch { }
            return 0;
        }

        private bool SelectMenuByIndex(int index)
        {
            try
            {
                if (GameCanvas.menu == null || !GameCanvas.menu.showMenu || GameCanvas.menu.menuItems == null) return false;
                if (index < 0 || index >= GameCanvas.menu.menuItems.size()) return false;
                GameCanvas.menu.menuSelectedItem = index;
                GameCanvas.menu.performSelect();
                GameCanvas.menu.showMenu = false;
                return true;
            }
            catch { return false; }
        }

        private bool SelectMenuByName(params string[] names)
        {
            try
            {
                if (GameCanvas.menu != null && GameCanvas.menu.showMenu && GameCanvas.menu.menuItems != null)
                {
                    for (int i = 0; i < GameCanvas.menu.menuItems.size(); i++)
                    {
                        Command cmd = (Command)GameCanvas.menu.menuItems.elementAt(i);
                        if (IsCommandMatch(cmd, names))
                        {
                            GameCanvas.menu.menuSelectedItem = i;
                            GameCanvas.menu.performSelect();
                            GameCanvas.menu.showMenu = false;
                            return true;
                        }
                    }
                }

                if (TryClickChatPopupCommand(Char.chatPopup, names)) return true;
                if (TryClickChatPopupCommand(ChatPopup.currChatPopup, names)) return true;
                if (TryClickChatPopupCommand(ChatPopup.serverChatPopUp, names)) return true;
            }
            catch { }
            return false;
        }

        private bool TryClickChatPopupCommand(ChatPopup popup, string[] names)
        {
            if (popup == null) return false;
            if (IsCommandMatch(popup.cmdMsg1, names)) { popup.cmdMsg1.performAction(); return true; }
            if (IsCommandMatch(popup.cmdMsg2, names)) { popup.cmdMsg2.performAction(); return true; }
            if (IsCommandMatch(popup.cmdNextLine, names)) { popup.cmdNextLine.performAction(); return true; }
            return false;
        }

        private bool IsCommandMatch(Command cmd, string[] names)
        {
            if (cmd == null || string.IsNullOrWhiteSpace(cmd.caption)) return false;
            string caption = Normalize(cmd.caption);
            foreach (string name in names)
            {
                if (caption.Contains(Normalize(name))) return true;
            }
            return false;
        }

        private void ClickOkIfAny()
        {
            SelectMenuByName("ok", "đóng", "từ chối");
        }

        private bool HasPopupText()
        {
            return !string.IsNullOrWhiteSpace(GetPopupText());
        }

        private string GetPopupText()
        {
            StringBuilder sb = new StringBuilder();
            AppendPopupText(sb, Char.chatPopup);
            AppendPopupText(sb, ChatPopup.currChatPopup);
            AppendPopupText(sb, ChatPopup.serverChatPopUp);
            return sb.ToString();
        }

        private void AppendPopupText(StringBuilder sb, ChatPopup popup)
        {
            if (popup?.says == null) return;
            for (int i = 0; i < popup.says.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(popup.says[i])) sb.Append(' ').Append(popup.says[i]);
            }
        }

        private int ParseOnlineWaitSeconds(string text)
        {
            string normalized = Normalize(text);
            if (string.IsNullOrWhiteSpace(normalized)) return -1;

            // Text NPC dạng bảng: "7 thỏi vàng...: 1 phút 57 giây sau nhận được".
            Match rewardLineMatch = Regex.Match(normalized, @"(?:(\d+)\s*phút\s*)?(?:(\d+)\s*giây\s*)?sau\s+nhận\s+được");
            if (rewardLineMatch.Success)
            {
                int totalSeconds = 0;
                if (int.TryParse(rewardLineMatch.Groups[1].Value, out int minutes)) totalSeconds += minutes * 60;
                if (int.TryParse(rewardLineMatch.Groups[2].Value, out int seconds)) totalSeconds += seconds;
                return Math.Max(1, totalSeconds);
            }

            // Popup cũ dạng: "nhận thưởng điểm danh online sau 10'" hoặc "sau 30s".
            if (!normalized.Contains("nhận thưởng điểm danh online sau")) return -1;

            Match minuteMatch = Regex.Match(normalized, @"sau\s+(\d+)\s*'");
            if (minuteMatch.Success && int.TryParse(minuteMatch.Groups[1].Value, out int oldMinutes)) return Math.Max(1, oldMinutes * 60);

            Match secondMatch = Regex.Match(normalized, @"sau\s+(\d+)\s*s");
            if (secondMatch.Success && int.TryParse(secondMatch.Groups[1].Value, out int oldSeconds)) return Math.Max(1, oldSeconds);

            return 30;
        }

        private void ApplyOnlineProgressFromNpcText(string text)
        {
            string normalized = Normalize(text);
            if (!IsOnlinePopupText(text)) return;

            int claimedCount = Regex.Matches(normalized, @"đã\s+nhận").Count;
            if (claimedCount <= 0) return;

            if (claimedCount > _onlineClaimedCount)
            {
                _onlineClaimedCount = claimedCount;
            }
            _onlineClaimDate = DateTime.Now.ToString("yyyy-MM-dd");

            if (_onlineClaimedCount >= 5)
            {
                _onlineCheckedThisRun = true;
                _canClaimOnline = false;
                _nextOnlineSeconds = -1;
                _state = AttendanceState.Idle;
                _settings.Enabled = false;
                StateText = "Đã nhận đủ 5 mốc online hôm nay";
            }
        }

        private void RestoreOnlineProgressFromPanel(AttendancePanelSettings settings)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (settings.SavedOnlineClaimDate == today)
            {
                _onlineClaimDate = today;
                _onlineClaimedCount = Math.Max(_onlineClaimedCount, settings.SavedOnlineClaimedCount);
                if (_onlineClaimedCount >= 5)
                {
                    _onlineCheckedThisRun = true;
                    _settings.Enabled = false;
                    _state = AttendanceState.Idle;
                    _nextOnlineSeconds = -1;
                    _canClaimOnline = false;
                    StateText = "Đã nhận đủ 5 mốc online hôm nay";
                }
            }
        }

        private bool ShouldScheduleAutoStart()
        {
            if (!_settings.ScheduleEnabled || IsActive) return false;
            DateTime now = DateTime.Now;
            int todayKey = now.Year * 10000 + now.Month * 100 + now.Day;
            if (_lastScheduleAutoStartDateKey == todayKey) return false;
            if (_onlineClaimDate == now.ToString("yyyy-MM-dd") && _onlineClaimedCount >= 5) return false;
            DateTime scheduledTime = now.Date.AddHours(_settings.ScheduleHour).AddMinutes(_settings.ScheduleMinute);
            return now >= scheduledTime;
        }

        private void TryScheduleAutoStart()
        {
            if (!ShouldScheduleAutoStart()) return;
            DateTime now = DateTime.Now;
            _lastScheduleAutoStartDateKey = now.Year * 10000 + now.Month * 100 + now.Day;
            StartFromPanel();
            StateText = "Tự bật điểm danh theo giờ";
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            string text = value.ToLower().Replace("\n", " ").Replace("\r", " ");
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        private void ResetRunFlagsIfNewDay()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (_onlineClaimDate != today)
            {
                _onlineClaimedCount = 0;
                _onlineCheckedThisRun = false;
                _nextOnlineSeconds = -1;
                _canClaimOnline = false;
            }
            if (_continuousClaimDate != today) _continuousCheckedThisRun = false;
            if (_monthlyClaimedKey != DateTime.Now.ToString("yyyyMM")) _monthlyCheckedThisRun = false;
        }

        private void SetOnlineWaitSeconds(int waitSeconds, long now)
        {
            long candidateCheckMs = now + waitSeconds * 1000L;
            if (_state == AttendanceState.WaitNextOnlineCheck && _nextOnlineCheckMs > now)
            {
                long currentRemaining = Math.Max(1, (_nextOnlineCheckMs - now) / 1000);
                if (waitSeconds >= currentRemaining) return;
            }

            _nextOnlineSeconds = waitSeconds;
            _nextOnlineCheckMs = candidateCheckMs;
        }

        private void SendStatusIfDue(bool force)
        {
            long now = mSystem.currentTimeMillis();
            if (!force && now - _lastStatusMs < 3000) return;
            _lastStatusMs = now;
            int nextSeconds = _state == AttendanceState.WaitNextOnlineCheck && _nextOnlineCheckMs > now
                ? (int)Math.Max(1, (_nextOnlineCheckMs - now) / 1000)
                : _nextOnlineSeconds;
            SocketGame.SendAttendanceStatus(this, nextSeconds);
        }
    }
}
