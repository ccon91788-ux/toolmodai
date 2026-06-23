using System.Collections.Generic;
using NRO_v247.Mods.Xmap;

namespace NRO_v247.Mods
{
    // Port từ XmapController.java
    // Giữ nguyên public API để tương thích với ModBootstrap, SocketGame, AutoTrainFeature, v.v.
    public class AutoXmapFeature : IAutoFeature, IActionListener, IXmapService
    {
        private static AutoXmapFeature _instance;

        public static bool IsEatChicken = true;
        public static bool IsUseTdlt = false;
        public static bool IsUseCapsule = true;

        // Delay sau mỗi lần gọi nextMap thực sự (ms)
        private const int DelayAfterNextMap = 1200;

        public bool IsUtilityTask => true;
        public bool IsActive => _isXmaping;
        public string CurrentState => GetState();

        private bool _isXmaping = false;
        private int _idMapEnd = -1;
        private long _delayUpdateXmap = 0;

        // Delay chờ server thật gửi dữ liệu mob sau khi load map xong
        public static int PostMapLoadDelayMs = 500;
        private long _postMapLoadUntil = 0;   // mSystem timestamp, xmap block cho đến khi qua mốc này
        private bool _wasChangingMap = false;  // detect khoảnh khắc ischangingMap: true → false
        private long _changingMapStartTime = 0L; // thời điểm ischangingMap bắt đầu = true

        private long _lastWaitTime = 0;
        private long _lastErrorTime = 0;
        private long _lastItemUseTime = 0;
        private bool _isUsingItem = false;
        private bool _capsuleUsed = false; // Đã dùng capsule trong phiên xmap hiện tại chưa — chỉ dùng 1 lần

        // ─── Capsule state ───────────────────────────────────────────────────────
        private enum XmapState { Default, WaitingCapsule, MovingWithCapsule }
        private XmapState _state = XmapState.Default;
        private List<int> _capsuleTargets = null;
        private Dictionary<int, int> _capsuleMapToIndex = null;
        private long _stateStartTime = 0;

        // ─── Zone change state ───────────────────────────────────────────────────
        // Đổi khu nếu sau ZONE_CHANGE_TIMEOUT ms không next map được
        private const long ZONE_CHANGE_TIMEOUT = 6000L; // 6s không tiến được → đổi khu
        private const int MAP_CHANGE_FAIL_THRESHOLD = 3; // Tránh đổi khu quá sớm do lag server nhất thời
        private long _stuckSinceTime = 0L;              // thời điểm bắt đầu bị kẹt
        private bool _requireRandomZoneChange = false;
        private long _lastRandomZoneTime = 0;
        private int _mapBeforeChange = -1;
        private int _mapChangeFailCount = 0;
        private bool _isChangingZoneByXmap = false;

        // ─── Home map lazy init ───────────────────────────────────────────────────
        private bool _homeMapLoaded = false;

        // Cho menu UI
        private int[] _currentMapSelection = null;
        private int[] _currentPlanetMaps = null;

        private const int ACTION_PLANET_SELECT = 1000;
        private const int ACTION_MAP_SELECT = 2000;
        private const int ACTION_BACK_TO_PLANETS = 3000;
        private const int ACTION_GO_HOME = 5000;
        private const int ACTION_TOGGLE_CAPSULE = 6000;
        private const int ACTION_STOP_XMAP = 7000;

        private readonly Xmap.XmapPathfinder _pathfinder = Xmap.XmapPathfinder.GetInstance();

        private int _startMapId = -1;

        public void Update()
        {
            HandleHotkey();

            // Flush pending map change request (charMove đã gửi, chờ 200ms rồi gửi requestChangeMap)
            XmapNavigator.FlushPendingMapChange();

            Char me = Char.myCharz();
            if (me == null) return;

            // Lazy init home map sau khi login (linkMaps[999] cần Char.myCharz().cgender)
            if (!_homeMapLoaded)
            {
                _homeMapLoaded = true;
                DataXmap.LoadHomeMap();
            }

            if (me.meDead)
            {
                _lastWaitTime = mSystem.currentTimeMillis() + 1000L;
                _stuckSinceTime = 0L; // reset stuck khi chết
                return;
            }

            // Đang bay tàu vũ trụ (TransportScr) — tạm dừng xmap hoàn toàn.
            // Tàu mất khoảng 60s, nếu không block thì stuck timer sẽ kích hoạt đổi khu,
            // sau đó lại lên tàu → vòng lặp vô tận.
            if (GameCanvas.currentScreen is TransportScr)
            {
                _stuckSinceTime = 0L;
                _requireRandomZoneChange = false;
                _wasChangingMap = false;
                _changingMapStartTime = 0L;
                return;
            }

            // Pick gà nếu đang ở home map
            bool isPickingChicken = false;
            if (IsEatChicken && DataXmap.IsHomeMap(TileMap.mapID))
                isPickingChicken = ChickenPickup();

            if (_isXmaping && TileMap.mapID == _idMapEnd)
            {
                FinishXmap(null);
                return;
            }

            if (!_isXmaping) return;

            if (IsUseTdlt)
            {
                NRO_v247.Mods.Utils.TdltController.Update(true);
            }

            if (isPickingChicken) return;

            long now = mSystem.currentTimeMillis();
            if (now - _lastWaitTime <= 100L) return;
            if (now < _delayUpdateXmap) return; // Throttle sau mỗi lần nextMap thực sự

            if (Char.ischangingMap)
            {
                if (!_wasChangingMap)
                {
                    _wasChangingMap = true;
                    _mapBeforeChange = TileMap.mapID;
                    _changingMapStartTime = now;
                }
                else if (now - _changingMapStartTime > 5000L)
                {
                    // Timeout 5s: server không trả data → force reset
                    Char.ischangingMap = false;
                    _changingMapStartTime = 0L;
                    // Giữ _wasChangingMap = true để nhánh else bên dưới chạy ngay tick tiếp theo
                }
                else
                {
                    return;
                }
            }
            else
            {
                _changingMapStartTime = 0L;
                // Phát hiện khoảnh khắc map vừa load xong (ischangingMap: true → false)
                // Đây là nơi DUY NHẤT reset stuck khi chuyển map thành công/thất bại
                if (_wasChangingMap)
                {
                    _wasChangingMap = false;
                    _postMapLoadUntil = now + PostMapLoadDelayMs;

                    if (!_isChangingZoneByXmap)
                    {
                        if (TileMap.mapID == _mapBeforeChange)
                        {
                            // Chuyển map thất bại (map không đổi). Chỉ tăng fail-count ở đây để tránh double-count.
                            _mapChangeFailCount++;
                            if (_stuckSinceTime == 0L)
                                _stuckSinceTime = now;

                            if (_mapChangeFailCount >= MAP_CHANGE_FAIL_THRESHOLD && !_requireRandomZoneChange)
                            {
                                if (GameScr.info1 != null) GameScr.info1.addInfo("Xmap hụt nhiều lần, đang tự đổi khu...", 0);
                                _requireRandomZoneChange = true;
                                _lastRandomZoneTime = now - 5000L; // cho phép thực hiện ngay
                                _mapChangeFailCount = 0;
                            }

                            // Đặt lại trạng thái capsule để cho phép thử bấm lại thay vì chạy bộ
                            _capsuleUsed = false;
                        }
                        else
                        {
                            // Chuyển map thành công → reset stuck/fail-count
                            _stuckSinceTime = 0L;
                            _mapChangeFailCount = 0;
                        }
                    }
                    else
                    {
                        _isChangingZoneByXmap = false;
                        _stuckSinceTime = 0L;
                        _mapChangeFailCount = 0;
                    }
                }
            }

            // Chờ hết post-map-load delay trước khi tiếp tục xmap
            if (now < _postMapLoadUntil) return;

            if (Controller.isStopReadMessage) return;

            _delayUpdateXmap = now;

            bool shouldProceed = true;
            if (DataXmap.IsFutureMap(_idMapEnd))
            {
                try
                {
                    if (me.taskMaint.taskId <= 24)
                    {
                        FinishXmap(null);
                        return;
                    }
                }
                catch { }
                shouldProceed = FutureMapNpcFinding();
            }

            // Đổi khu nếu stuck quá ZONE_CHANGE_TIMEOUT ms
            // KHÔNG đổi khu khi capsule đang active
            bool capsuleActive = _state == XmapState.WaitingCapsule;
            if (!capsuleActive && _stuckSinceTime > 0L && now - _stuckSinceTime >= ZONE_CHANGE_TIMEOUT)
            {
                _stuckSinceTime = 0L;
                if (!_requireRandomZoneChange)
                {
                    _requireRandomZoneChange = true;
                    _lastRandomZoneTime = now - 5000L; // cho phép thực hiện ngay
                }
            }
            // Reset stuck timer khi capsule đang chờ (không tính thời gian chờ capsule là bị kẹt)
            if (capsuleActive && _stuckSinceTime > 0L)
                _stuckSinceTime = now;

            if (_requireRandomZoneChange)
            {
                if (now - _lastRandomZoneTime >= 5000L)
                {
                    _requireRandomZoneChange = false;
                    _lastRandomZoneTime = now;
                    DoRandomZoneChange();
                    _mapChangeFailCount = 0;
                }
                return;
            }

            if (shouldProceed)
                UpdateXmap(_idMapEnd);
        }

        private void DoRandomZoneChange()
        {
            try
            {
                var zones = GameScr.gI().zones;
                if (zones == null) return;
                
                var candidates = new List<int>();
                for (int i = 0; i < zones.Length; i++)
                {
                    if (zones[i] == -1 || zones[i] == TileMap.zoneID) continue;
                    candidates.Add(zones[i]);
                }
                
                if (candidates.Count == 0) return;
                
                int targetZone = candidates[new System.Random().Next(candidates.Count)];
                Service.gI().requestChangeZone(targetZone, -1);
                Char.ischangingMap = true; // NGĂN CHẶN CÁC LỆNH KHÁC KHI ĐANG ĐỢI ĐỔI KHU
                _isChangingZoneByXmap = true;
                if (GameScr.info1 != null) GameScr.info1.addInfo("Xmap lỗi map, đang đổi khu...", 0);
            }
            catch { }
        }

        private bool ChickenPickup()
        {
            long now = mSystem.currentTimeMillis();
            for (int i = 0; i < GameScr.vItemMap.size(); i++)
            {
                ItemMap itemMap = (ItemMap)GameScr.vItemMap.elementAt(i);
                if ((itemMap.playerId == Char.myCharz().charID || itemMap.playerId == -1) &&
                    itemMap.template.id == 74)
                {
                    Char.myCharz().itemFocus = itemMap;
                    if (now - _lastWaitTime > 600L)
                    {
                        _lastWaitTime = now;
                        Service.gI().pickItem(itemMap.itemMapID);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool FutureMapNpcFinding()
        {
            if (GameScr.findNPCInMap(38) != null)
                return true; // NPC có mặt → xmap bình thường

            // NPC không có mặt ở map hiện tại → di chuyển tìm
            switch (TileMap.mapID)
            {
                case 27: UpdateXmap(28); return false;
                case 28: UpdateXmap(29); return false;
                case 29: UpdateXmap(27); return false;
            }
            return true;
        }

        private void UpdateXmap(int mapID)
        {
            try
            {
                long now = mSystem.currentTimeMillis();
                Char me = Char.myCharz();
                bool canGoCold = me != null && me.taskMaint != null && me.taskMaint.taskId > 30;
                long cPower = me != null ? me.cPower : 0;

                // 1. Ưu tiên tìm đường nội bộ nếu cả 2 map đều trong future maps
                // — tránh NPC tàu thời gian (NPC 38) cực chậm khi đi lại trong vùng tương lai
                bool bothInFuture = DataXmap.IsFutureMap(TileMap.mapID) && DataXmap.IsFutureMap(mapID);
                int[] normalPath = _pathfinder.FindPath(mapID, TileMap.mapID, cPower, canGoCold, null, avoidNpc38: bothInFuture);

                // Nếu không tìm được path nội bộ → fallback về path thông thường (có NPC)
                if (normalPath == null && bothInFuture)
                    normalPath = _pathfinder.FindPath(mapID, TileMap.mapID, cPower, canGoCold, null, avoidNpc38: false);

                // 2. Quyết định xem có nên dùng Capsule không
                // Chỉ dùng capsule 1 lần/phiên (chắn spam capsule ở các map trung gian)
                // Khi cả 2 map đều trong future, không dùng capsule nếu đã có đường nội bộ
                // (tránh capsule nhảy ra map ngoài rồi vào lại qua NPC 38)
                bool needCapsule = IsUseCapsule && !_capsuleUsed && !bothInFuture && (normalPath == null || normalPath.Length > 2);

                if (needCapsule && _state == XmapState.Default)
                {
                    short capsuleId = FindCapsuleInBag(me, true); 
                    if (capsuleId > 0)
                    {
                        if (GameCanvas.panel != null) GameCanvas.panel.mapNames = null;
                        Service.gI().useItem(0, 1, -1, capsuleId);
                        _capsuleUsed = true; // Đánh dấu đã dùng capsule — không dùng lại trong phiên này
                        _state = XmapState.WaitingCapsule;
                        _stateStartTime = now;
                        return;
                    }
                }

                if (_state == XmapState.WaitingCapsule)
                {
                    if (GameCanvas.panel != null && GameCanvas.panel.mapNames != null)
                    {
                        _capsuleTargets = new List<int>();
                        _capsuleMapToIndex = new Dictionary<int, int>();
                        for (int i = 0; i < GameCanvas.panel.mapNames.Length; i++)
                        {
                            string panelName = GameCanvas.panel.mapNames[i];
                            int toId = GetMapIdFromName(panelName);
                            if (toId != -1)
                            {
                                _capsuleTargets.Add(toId);
                                _capsuleMapToIndex[toId] = i;
                            }
                        }
                        if (GameCanvas.panel != null) GameCanvas.panel.hide();
                        _state = XmapState.MovingWithCapsule;
                    }
                    else if (now - _stateStartTime > 5500L) // Timeout 5.5s (Chờ hết 5s cooldown của capsule để bấm lại lần 2 chắc chắn ăn)
                    {
                        _state = XmapState.Default; // Fallback, sẽ thử lại capsule ở lần lặp update kế tiếp
                        _capsuleTargets = null;
                        _capsuleMapToIndex = null;
                    }
                    else
                    {
                        return; // Đang chờ panel
                    }
                }

                // 3. Lấy path cuối cùng
                int[] path = normalPath;
                if (_state == XmapState.MovingWithCapsule && _capsuleTargets != null)
                {
                    // Truyền avoidNpc38 để capsule cũng không chọn path qua NPC 38 khi cả 2 map là future
                    int[] capPath = _pathfinder.FindPath(mapID, TileMap.mapID, cPower, canGoCold, _capsuleTargets, avoidNpc38: bothInFuture);
                    if (capPath != null && (normalPath == null || capPath.Length < normalPath.Length))
                    {
                        path = capPath;
                    }
                    else
                    {
                        path = normalPath;
                        _state = XmapState.Default; 
                        _capsuleTargets = null;
                        _capsuleMapToIndex = null;
                    }
                }

                if (path == null)
                {
                    // Thoát private map
                    bool inPrivateMap = TileMap.vGo != null && TileMap.vGo.size() == 0;
                    var trainFeature = ModBootstrap.TrainFeature;
                    if (inPrivateMap && trainFeature != null && trainFeature.UsePrivateTicket)
                    {
                        if (TryEscapePrivateMapWithTicket(me, now)) return;
                    }
                    PathError(mapID);
                    return;
                }

                if (TileMap.mapID != path[0] || Char.ischangingMap || Controller.isStopReadMessage)
                    return;

                if (me != null && me.clan == null && DataXmap.RequiresClan(_idMapEnd))
                {
                    FinishXmap(null);
                    return;
                }

                if (!_isUsingItem || now - _lastItemUseTime >= 500)
                {
                    if (_isUsingItem && TileMap.mapID == 160)
                        _isUsingItem = false;

                    if (path.Length < 2) return;

                    int nextMapId = path[1];
                    Xmap.NextMap nextWalk = _pathfinder.FindNextMapToGo(TileMap.mapID, nextMapId);

                    if (nextWalk == null && _state == XmapState.MovingWithCapsule && _capsuleTargets != null && _capsuleMapToIndex != null && _capsuleMapToIndex.ContainsKey(nextMapId))
                    {
                        int pId = _capsuleMapToIndex[nextMapId];
                        Service.gI().requestMapSelect(pId);
                        Char.ischangingMap = true; // NGĂN CHẶN XMAP CHẠY TIẾP KHI ĐANG ĐỢI SERVER ĐỔI MAP QUA CAPSULE
                        _delayUpdateXmap = now + 1000L; 
                        _state = XmapState.Default;
                        _capsuleTargets = null;
                        _capsuleMapToIndex = null;
                        return;
                    }

                    if (nextWalk != null)
                    {
                        XmapNavigator.gotoMap(nextMapId);
                        _delayUpdateXmap = now + DelayAfterNextMap;
                        _state = XmapState.Default;
                        _capsuleTargets = null;
                        _capsuleMapToIndex = null;
                    }
                }
            }
            catch { }
        }

        private bool TryEscapePrivateMapWithTicket(Char me, long now)
        {
            if (me?.arrItemBag == null) return false;
            if (now - _lastItemUseTime < 2000L) return false;

            for (int i = 0; i < me.arrItemBag.Length; i++)
            {
                Item item = me.arrItemBag[i];
                if (item == null || item.template == null || item.quantity <= 0) continue;

                if (item.template.id == 1825)
                {
                    Service.gI().useItem(0, 1, (sbyte)i, -1);
                    _lastItemUseTime = now;
                    GameScr.info1?.addInfo("Map riêng tư: Dùng Vé để thoát ra", 0);
                    return true;
                }
            }
            return false;
        }

        // ─── Shared capsule helpers ───────────────────────────────────────────────

        /// <summary>
        /// Tìm Capsule trong túi nhân vật.
        /// Ưu tiên id 194 (đặc biệt), sau đó 193 (thường) nếu allowNormal = true.
        /// Trả về template id nếu tìm thấy, -1 nếu không.
        /// </summary>
        private static short FindCapsuleInBag(Char me, bool allowNormal)
        {
            if (me?.arrItemBag == null) return -1;

            // Ưu tiên capsule đặc biệt (194)
            for (int i = 0; i < me.arrItemBag.Length; i++)
            {
                Item item = me.arrItemBag[i];
                if (item == null || item.template == null) continue;
                if (item.template.id == 194 && item.quantity > 0) return 194;
            }

            // Sau đó mới đến capsule thường (193)
            if (allowNormal)
            {
                for (int i = 0; i < me.arrItemBag.Length; i++)
                {
                    Item item = me.arrItemBag[i];
                    if (item == null || item.template == null) continue;
                    if (item.template.id == 193 && item.quantity > 0) return 193;
                }
            }
            return -1;
        }

        private static int GetMapIdFromName(string panelName)
        {
            if (string.IsNullOrEmpty(panelName)) return -1;
            string normalized = Xmap.NextMap.NormalizeText(panelName);

            Char me = Char.myCharz();
            int gender = me != null ? me.cgender : 0;

            if (normalized.Contains("ve nha")) return 21 + gender;
            if (normalized.Contains("tram vu tru")) return 24 + gender;

            if (TileMap.mapNames != null)
            {
                for (int i = 0; i < TileMap.mapNames.Length; i++)
                {
                    if (string.IsNullOrEmpty(TileMap.mapNames[i])) continue;
                    string mapName = Xmap.NextMap.NormalizeText(TileMap.mapNames[i]);
                    // Kiểm tra chuỗi name có chứa tên map gốc hay không (VD: panel hiện 'Đảo Kame (phí...)' thì phải lấy 'đảo kame')
                    if (normalized == mapName || normalized.IndexOf(mapName, System.StringComparison.Ordinal) >= 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private void PathError(int mapID)
        {
            long now = mSystem.currentTimeMillis();
            if (now - _lastErrorTime >= 1000L)
            {
                bool canGoCold = false;
                long cPower = 0;
                try
                {
                    canGoCold = Char.myCharz().taskMaint.taskId > 30;
                    cPower = Char.myCharz().cPower;
                }
                catch { }
                string msg = _pathfinder.GetPathErrorMessage(mapID, TileMap.mapID, cPower, canGoCold);
                GameScr.info1.addInfo(msg, 0);
                _lastErrorTime = now;
            }
        }

        // ─── Public API (giữ nguyên để tương thích) ────────────────────────────

        public bool IsXmaping() => _isXmaping;

        public void StartGoToMapFromPanel(int mapId)
        {
            if (mapId < 0) return;
            if (_isXmaping && _idMapEnd == mapId) return;
            StartXmap(mapId);
        }

        public void StopFromPanel() => FinishXmap(null);

        public void ApplySettingsFromPanel(bool eatChicken, int postMapLoadDelayMs = -1, bool useTdlt = false)
        {
            IsEatChicken = eatChicken;
            IsUseTdlt = useTdlt;
            if (postMapLoadDelayMs >= 0)
            {
                if (postMapLoadDelayMs > 10000) postMapLoadDelayMs = 10000;
                PostMapLoadDelayMs = postMapLoadDelayMs;
            }
        }

        public string GetState()
        {
            if (!_isXmaping) return null;
            if (!(GameCanvas.currentScreen is GameScr))
            {
                // Chỉ báo "tàu vũ trụ" khi nhân vật đã load (tránh false positive lúc loading sau login)
                if (Char.myCharz() == null) return null;
                if (GameCanvas.currentScreen == null || GameCanvas.currentScreen is ServerListScreen || GameCanvas.currentScreen is LoginScr || !Session_ME.gI().isConnected())
                    return null;
                return "Đang bay bằng tàu vũ trụ";
            }
            return "Xmap " + _startMapId + " -> " + _idMapEnd + " : " + TileMap.mapID;
        }

        // ─── Xmap control ────────────────────────────────────────────────────────

        private void StartXmap(int mapID)
        {
            _isXmaping = true;
            _idMapEnd = mapID;
            _startMapId = TileMap.mapID;
            _lastWaitTime = 0;
            _lastErrorTime = 0;
            _capsuleUsed = false;
            _requireRandomZoneChange = false;
            _isChangingZoneByXmap = false;
            _stuckSinceTime = 0L;
            _mapChangeFailCount = 0;
            _changingMapStartTime = 0L;

            _state = XmapState.Default;
            _capsuleTargets = null;
            _capsuleMapToIndex = null;

            // Reset state của tất cả NextMap instances trong path hiện tại
            ResetAllNextMapStates();
        }

        private void FinishXmap(string message)
        {
            _isXmaping = false;
            _capsuleUsed = false;
            _state = XmapState.Default;
            _capsuleTargets = null;
            _capsuleMapToIndex = null;

            _idMapEnd = -1;
            _stuckSinceTime = 0L;
            _mapChangeFailCount = 0;

            // Reset state của tất cả NextMap instances để tránh state rác
            ResetAllNextMapStates();

            try
            {
                if (Char.myCharz() != null)
                    Char.myCharz().currentMovePoint = null;
            }
            catch { }

            if (!string.IsNullOrEmpty(message))
                GameScr.info1.addInfo(message, 0);
        }

        /// <summary>
        /// Reset state của tất cả NextMap instances trong DataXmap.linkMaps
        /// để tránh _isEntering hoặc _isTeleNpc bị kẹt khi xmap hủy giữa chừng.
        /// </summary>
        private static void ResetAllNextMapStates()
        {
            try
            {
                foreach (var list in DataXmap.linkMaps.Values)
                    foreach (var nm in list)
                        nm.ResetState();
            }
            catch { }
        }

        // ─── Hotkey & Menu UI ────────────────────────────────────────────────────

        private void HandleHotkey()
        {
            if (!(GameCanvas.currentScreen is GameScr)) return;
            if (GameCanvas.menu != null && GameCanvas.menu.showMenu) return;
            if (GameCanvas.panel != null && GameCanvas.panel.isShow) return;

            int key = GameCanvas.keyAsciiPress;
            if (key != 120 && key != 88) return; // 'x' hoặc 'X'

            GameCanvas.keyAsciiPress = 0;
            OpenRootMenu();
        }

        private void OpenRootMenu()
        {
            var menu = new MyVector();

            if (_isXmaping)
                menu.addElement(new Command("Dừng xmap", this, ACTION_STOP_XMAP, null));

            menu.addElement(new Command("Xmap", this, ACTION_PLANET_SELECT + 999, null));
            menu.addElement(new Command("Capsule: " + (IsUseCapsule ? "ON" : "OFF"), this, ACTION_TOGGLE_CAPSULE, null));
            GameCanvas.menu.startAt(menu, 0);
        }

        public void perform(int idAction, object p)
        {
            switch (idAction)
            {
                case ACTION_BACK_TO_PLANETS:
                    OpenXmapPlanets();
                    break;

                case ACTION_GO_HOME:
                    GoToHome();
                    break;

                case ACTION_STOP_XMAP:
                    FinishXmap("Xmap: da dung");
                    break;

                case ACTION_TOGGLE_CAPSULE:
                    IsUseCapsule = !IsUseCapsule;
                    GameScr.info1.addInfo("Capsule: " + (IsUseCapsule ? "ON" : "OFF"), 0);
                    OpenRootMenu();
                    break;

                default:
                    if (idAction >= ACTION_PLANET_SELECT && idAction < ACTION_PLANET_SELECT + 1000)
                    {
                        int idx = idAction - ACTION_PLANET_SELECT;
                        if (idx == 999)
                        {
                            // mở planet list
                            OpenXmapPlanets();
                        }
                        else
                        {
                            PlanetSelection(idx);
                        }
                    }
                    else if (idAction >= ACTION_MAP_SELECT && idAction < ACTION_MAP_SELECT + 1000)
                    {
                        MapSelection(idAction - ACTION_MAP_SELECT);
                    }
                    break;
            }
        }

        private void OpenXmapPlanets()
        {
            try
            {
                var menu = new MyVector();
                var planets = DataXmap.planetList;
                for (int i = 0; i < planets.Count; i++)
                {
                    var entry = planets[i];
                    menu.addElement(new Command(entry.Name, this, ACTION_PLANET_SELECT + i, entry.Maps));
                }
                menu.addElement(new Command("Quay lại", this, -1, null));
                GameCanvas.menu.startAt(menu, 0);
            }
            catch { }
        }

        private void OpenXmapMaps(int[] mapIDs)
        {
            if (mapIDs == null) return;

            _currentPlanetMaps = mapIDs;
            var menu = new MyVector();
            var validMaps = new List<int>();
            int mapIndex = 0;
            int gender = Char.myCharz().cgender;

            for (int i = 0; i < mapIDs.Length; i++)
            {
                int id = mapIDs[i];
                if (!IsMapValidForGender(id, gender)) continue;

                string name = (TileMap.mapNames != null && id >= 0 && id < TileMap.mapNames.Length && TileMap.mapNames[id] != null)
                    ? TileMap.mapNames[id] : "Map " + id;

                menu.addElement(new Command(name + " [" + id + "]", this, ACTION_MAP_SELECT + mapIndex, id));
                validMaps.Add(id);
                mapIndex++;
            }

            menu.addElement(new Command("Quay lại", this, ACTION_BACK_TO_PLANETS, null));

            _currentMapSelection = new int[validMaps.Count];
            for (int i = 0; i < validMaps.Count; i++)
                _currentMapSelection[i] = validMaps[i];

            GameCanvas.menu.startAt(menu, 0);
        }

        private void PlanetSelection(int selectedIndex)
        {
            if (DataXmap.planetList == null) return;
            if (selectedIndex < 0 || selectedIndex >= DataXmap.planetList.Count) return;

            var entry = DataXmap.planetList[selectedIndex];
            if (DataXmap.IsHomeArray(entry.Maps))
            {
                GoToHome();
                if (GameCanvas.panel != null) GameCanvas.panel.hide();
            }
            else
            {
                OpenXmapMaps(entry.Maps);
            }
        }

        private void MapSelection(int selectedIndex)
        {
            if (_currentMapSelection == null) return;
            if (selectedIndex < 0 || selectedIndex >= _currentMapSelection.Length) return;

            StartXmap(_currentMapSelection[selectedIndex]);
            if (GameCanvas.menu != null) GameCanvas.menu.doCloseMenu();
        }

        private void GoToHome()
        {
            try
            {
                int gender = Char.myCharz().cgender;
                int homeMapId = 21 + gender;
                if (TileMap.mapID != homeMapId)
                    StartXmap(homeMapId);
            }
            catch { }
        }

        private static bool IsMapValidForGender(int mapId, int gender)
        {
            if (gender == 0 && (mapId == 22 || mapId == 23)) return false;
            if (gender == 1 && (mapId == 21 || mapId == 23)) return false;
            if (gender == 2 && (mapId == 21 || mapId == 22)) return false;
            return true;
        }
    }
}
