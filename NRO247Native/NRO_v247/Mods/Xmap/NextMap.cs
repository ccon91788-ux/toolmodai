using System;
using System.Globalization;
using System.Text;
using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods.Xmap
{
    // Port từ NextMap.java
    public class NextMap
    {
        public int MapID;
        public int Npc;
        public int Index;        // menu index (int version)
        public string IndexName; // menu name (string version, có thể null)
        public int Index2;
        public int Index3;
        public string Index2Name;
        public string Index3Name;
        public bool walk;
        public int x;
        public int y;
        public int WaypointPosition = 0;

        private bool _isEntering;
        private long _enterDelayStart;
        private bool _isTeleNpc;
        private long _teleNpcStartTime;
        private bool _hasTeleported;
        private long _teleportTime;
        private int _currentConfirmIndex = 0;
        private long _lastConfirmTime = 0L;
        private const long CONFIRM_DELAY = 200L;
        private bool _isConfirming = false;
        public bool IsConfirming => _isConfirming || _isTeleNpc;

        // Constructor dùng int index (giống Java addNPCLinkMap)
        public NextMap(int mapID, int npcID, int index, int index2, int index3, bool walk, int x, int y)
        {
            this.MapID = mapID;
            this.Npc = npcID;
            this.Index = index;
            this.Index2 = index2;
            this.Index3 = index3;
            this.walk = walk;
            this.x = x;
            this.y = y;
            this._hasTeleported = false;
            this._teleportTime = 0L;
            this.IndexName = null;
            this.Index2Name = null;
            this.Index3Name = null;
            this._currentConfirmIndex = 0;
            this._lastConfirmTime = 0L;
            this._isConfirming = false;
        }

        // Constructor dùng string name (giống Java addNPCLinkMapByName)
        public NextMap(int mapID, int npcID, string indexName, string index2Name, string index3Name, bool walk, int x, int y)
        {
            this.MapID = mapID;
            this.Npc = npcID;
            this.Index = -1;
            this.Index2 = -1;
            this.Index3 = -1;
            this.IndexName = indexName;
            this.Index2Name = index2Name;
            this.Index3Name = index3Name;
            this.walk = walk;
            this.x = x;
            this.y = y;
            this._hasTeleported = false;
            this._teleportTime = 0L;
            this._currentConfirmIndex = 0;
            this._lastConfirmTime = 0L;
            this._isConfirming = false;
        }

        public void GotoMap()
        {
            if (!walk)
            {
                // Trường hợp waypoint thuần túy (không NPC, không walk)
                if (Index == -1 && Npc == -1)
                {
                    Waypoint wayPoint = GetWayPoint();
                    if (wayPoint != null)
                    {
                        Enter(wayPoint);
                    }
                }
                else
                {
                    // Trường hợp NPC
                    if (Npc == -1 || (Index == -1 && (IndexName == null || IndexName.Length == 0)))
                        return;

                    if (!_isTeleNpc)
                    {
                        Npc npcCheck = GameScr.findNPCInMap((short)Npc);
                        if (npcCheck == null) return; // NPC chưa load, đừng set state
                        Char.myCharz().npcFocus = npcCheck;
                        _isTeleNpc = true;
                        _teleNpcStartTime = mSystem.currentTimeMillis();
                        _isConfirming = false;
                        _currentConfirmIndex = 0;
                    }
                    else
                    {
                        // Timeout 8 giây: reset toàn bộ nếu NPC không phản hồi
                        long elapsed = mSystem.currentTimeMillis() - _teleNpcStartTime;
                        if (elapsed > 8000L)
                        {
                            ResetConfirmState();
                            return;
                        }
                        if (elapsed < 150L)
                            return;

                        if (!_isConfirming)
                        {
                            Service.gI().openMenu(Npc);
                            _isConfirming = true;
                            _lastConfirmTime = mSystem.currentTimeMillis();
                            return;
                        }

                        if (mSystem.currentTimeMillis() - _lastConfirmTime < CONFIRM_DELAY)
                            return;

                        if (IndexName != null && IndexName.Length > 0)
                        {
                            ConfirmWithDelay();
                        }
                        else
                        {
                            bool flag = Char.myCharz().taskMaint.taskId > 30;
                            int num = Index;
                            if (TileMap.mapID == 19 && MapID == 68 && Char.myCharz().taskMaint.taskId == 23)
                                num = 0;
                            if (TileMap.mapID == 19 && MapID == 109 && !flag)
                                num = 1;

                            Service.gI().confirmMenu((short)Npc, (sbyte)num);
                            if (Index2 != -1)
                            {
                                Service.gI().confirmMenu((short)Npc, (sbyte)Index2);
                                if (Index3 != -1)
                                {
                                    Service.gI().confirmMenu((short)Npc, (sbyte)Index3);
                                }
                            }
                            _isTeleNpc = false;
                            _isConfirming = false;
                        }
                    }
                }
            }
            else if (x != -1 && y != -1)
            {
                Char.myCharz().currentMovePoint = new MovePoint(x, y);
            }
        }

        private void ConfirmWithDelay()
        {
            long currentTime = mSystem.currentTimeMillis();

            // Timeout sau 10 giây
            if (currentTime - _lastConfirmTime >= 10000L)
            {
                Service.gI().confirmMenu((short)Npc, (sbyte)0);
                GameCanvas.menu.doCloseMenu();
                Char.chatPopup = null;
                ResetConfirmState();
                return;
            }

            switch (_currentConfirmIndex)
            {
                case 0:
                    if (ConfirmByNameAndClose(IndexName))
                    {
                        _currentConfirmIndex++;
                        _lastConfirmTime = currentTime;
                        if (Index2Name == null || Index2Name.Length == 0)
                            ResetConfirmState();
                    }
                    break;

                case 1:
                    if (Index2Name != null && Index2Name.Length > 0)
                    {
                        if (ConfirmByNameAndClose(Index2Name))
                        {
                            _currentConfirmIndex++;
                            _lastConfirmTime = currentTime;
                            if (Index3Name == null || Index3Name.Length == 0)
                                ResetConfirmState();
                        }
                    }
                    else
                    {
                        ResetConfirmState();
                    }
                    break;

                case 2:
                    if (Index3Name != null && Index3Name.Length > 0)
                        ConfirmByNameAndClose(Index3Name);
                    ResetConfirmState();
                    break;
            }
        }

        private bool ConfirmByNameAndClose(string menuName)
        {
            if (menuName == null || menuName.Length == 0) return false;
            if (GameCanvas.menu == null || GameCanvas.menu.menuItems == null) return false;

            string searchText = NormalizeText(menuName);

            for (int i = 0; i < GameCanvas.menu.menuItems.size(); i++)
            {
                try
                {
                    Command cmd = (Command)GameCanvas.menu.menuItems.elementAt(i);
                    if (cmd == null || cmd.caption == null) continue;

                    string menuText = NormalizeText(cmd.caption);

                    if (menuText == searchText || menuText.IndexOf(searchText, StringComparison.Ordinal) >= 0)
                    {
                        Service.gI().confirmMenu((short)Npc, (sbyte)i);
                        GameCanvas.menu.doCloseMenu();
                        Char.chatPopup = null;
                        return true;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return false;
        }

        private void ResetConfirmState()
        {
            _currentConfirmIndex = 0;
            _lastConfirmTime = 0L;
            _isTeleNpc = false;
            _isConfirming = false;
        }

        public Waypoint GetWayPoint()
        {
            string targetName = GetMapName();
            var matched = new System.Collections.Generic.List<Waypoint>();

            for (int i = 0; i < TileMap.vGo.size(); i++)
            {
                Waypoint wp = (Waypoint)TileMap.vGo.elementAt(i);
                if (GetMapNameFromPopUp(wp.popup) == targetName)
                    matched.Add(wp);
            }

            if (matched.Count == 0) return null;
            if (matched.Count == 1 || WaypointPosition == 0) return matched[0];

            if (WaypointPosition == -1)
            {
                Waypoint leftmost = matched[0];
                for (int i = 1; i < matched.Count; i++)
                    if (matched[i].minX < leftmost.minX)
                        leftmost = matched[i];
                return leftmost;
            }
            else if (WaypointPosition == 1)
            {
                Waypoint rightmost = matched[0];
                for (int i = 1; i < matched.Count; i++)
                    if (matched[i].minX > rightmost.minX)
                        rightmost = matched[i];
                return rightmost;
            }

            return matched[0];
        }

        public string GetMapName()
        {
            if (TileMap.mapNames != null && MapID >= 0 && MapID < TileMap.mapNames.Length && TileMap.mapNames[MapID] != null)
                return TileMap.mapNames[MapID];
            return "Map " + MapID;
        }

        public string GetMapNameFromPopUp(PopUp popup)
        {
            if (popup == null || popup.says == null) return string.Empty;
            var sb = new StringBuilder();
            for (int i = 0; i < popup.says.Length; i++)
            {
                sb.Append(popup.says[i]);
                sb.Append(' ');
            }
            return sb.ToString().Trim();
        }

        public void Enter(Waypoint waypoint)
        {
            if (!_isEntering)
            {
                _isEntering = true;
                _enterDelayStart = mSystem.currentTimeMillis();
                _hasTeleported = false;
                _teleportTime = 0L;
            }
            else
            {
                long now = mSystem.currentTimeMillis();
                // Tăng delay sau teleport lên 500ms để server kịp xử lý position trước khi gửi tiếp
                if (now - _enterDelayStart < 100L)
                    return;

                // Map 166 → 155 đặc biệt: dùng loadMapLeft
                if (TileMap.mapID == 166 && MapID == 155)
                {
                    XmapNavigator.loadMapLeft();
                    ResetEnterState();
                    return;
                }

                int num = (waypoint.maxX < 60) ? 15
                    : ((waypoint.minX > TileMap.pxw - 60)
                        ? (TileMap.pxw - 15)
                        : (waypoint.minX + (waypoint.maxX - waypoint.minX) / 2));
                int maxY = waypoint.maxY;

                if (num == -1 || maxY == -1)
                {
                    ResetEnterState();
                    return;
                }

                if (!_hasTeleported)
                {
                    PathUtils.teleportTo(num, maxY);
                    _hasTeleported = true;
                    _teleportTime = now;

                    bool useOfflineMap = waypoint.isOffline;
                    // Queue request sau charMove; map nhà/làng/trạm/khu an toàn phải dùng getMapOffline(),
                    // nếu gửi requestChangeMap() thường sẽ bị server đẩy lại và nhìn như nháy cùng map.
                    XmapNavigator.QueueMapChangeRequest(useOfflineMap, useImmediateFlush: false);
                    ResetEnterState();
                    return;
                }
            }
        }

        private void ResetEnterState()
        {
            _isEntering = false;
            _hasTeleported = false;
            _teleportTime = 0L;
            _currentConfirmIndex = 0;
            _lastConfirmTime = 0L;
            _isConfirming = false;
        }

        /// <summary>
        /// Reset toàn bộ state của NextMap — gọi từ AutoXmapFeature khi StartXmap/FinishXmap
        /// để tránh state rác nếu xmap bị hủy giữa chừng.
        /// </summary>
        public void ResetState()
        {
            _isEntering = false;
            _hasTeleported = false;
            _teleportTime = 0L;
            _isTeleNpc = false;
            _teleNpcStartTime = 0L;
            _isConfirming = false;
            _currentConfirmIndex = 0;
            _lastConfirmTime = 0L;
        }

        // Chuẩn hóa text để so sánh menu (bỏ dấu, viết thường)
        public static string NormalizeText(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            string noControl = input.Replace("\n", " ").Replace("\r", " ").Trim().ToLowerInvariant();
            string normalized = noControl.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (char c in normalized)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark && !char.IsWhiteSpace(c) && !char.IsControl(c))
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        // ─── Static NPC confirm helpers (dùng bởi AutoBuyFeature) ────────────────

        private static short _confirmNpcId;
        private static string _confirmNpcName;
        private static bool _isStaticConfirming;
        private static bool _openedStaticMenu;
        private static long _staticConfirmStartMs;
        private const long STATIC_CONFIRM_TIMEOUT = 5000L;

        public static void StartConfirmNpc(int npcId, string menuName)
        {
            _confirmNpcId = (short)npcId;
            _confirmNpcName = menuName;
            _isStaticConfirming = true;
            _openedStaticMenu = false;
            _staticConfirmStartMs = mSystem.currentTimeMillis();

            Npc npc = GameScr.findNPCInMap(_confirmNpcId);
            if (npc != null)
            {
                Char.myCharz().npcFocus = npc;
            }
        }

        public static void UpdateConfirmNpc()
        {
            if (!_isStaticConfirming) return;
            long now = mSystem.currentTimeMillis();

            if (now - _staticConfirmStartMs > STATIC_CONFIRM_TIMEOUT)
            {
                _isStaticConfirming = false;
                return;
            }

            if (!_openedStaticMenu)
            {
                Service.gI().openMenu(_confirmNpcId);
                _openedStaticMenu = true;
                return;
            }

            if (GameCanvas.menu == null || !GameCanvas.menu.showMenu) return;
            if (GameCanvas.menu.menuItems == null) return;

            if (!string.IsNullOrEmpty(_confirmNpcName))
            {
                string search = NormalizeText(_confirmNpcName);
                for (int i = 0; i < GameCanvas.menu.menuItems.size(); i++)
                {
                    Command cmd = (Command)GameCanvas.menu.menuItems.elementAt(i);
                    if (cmd == null || cmd.caption == null) continue;
                    string text = NormalizeText(cmd.caption);
                    if (text == search || text.IndexOf(search, System.StringComparison.Ordinal) >= 0)
                    {
                        GameCanvas.menu.menuSelectedItem = i;
                        GameCanvas.menu.performSelect();
                        GameCanvas.menu.doCloseMenu();
                        _isStaticConfirming = false;
                        return;
                    }
                }
            }
        }
    }
}

