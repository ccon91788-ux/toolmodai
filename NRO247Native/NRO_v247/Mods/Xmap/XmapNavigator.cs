using System.Collections.Generic;
using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods.Xmap
{
    // Port từ XmapNavigator.java
    public static class XmapNavigator
    {
        private static int[] wayPointMapLeft;
        private static int[] wayPointMapCenter;
        private static int[] wayPointMapRight;

        public static void gotoMap(int mapID)
        {
            if (!DataXmap.linkMaps.ContainsKey(TileMap.mapID))
                return;

            NextMap nextMap = null;
            NextMap nextMap2 = null;

            List<NextMap> list = DataXmap.linkMaps[TileMap.mapID];
            foreach (NextMap item in list)
            {
                if (item.MapID == mapID)
                {
                    if (item.Npc != -1 || item.walk)
                    {
                        nextMap = item;
                        break;
                    }
                    if (item.Npc == -1 && item.Index == -1 && !item.walk)
                    {
                        // Ưu tiên waypoint đúng position
                        if (nextMap2 == null || item.WaypointPosition != 0)
                            nextMap2 = item;
                    }
                }
            }

            if (nextMap != null)
                nextMap.GotoMap();
            else if (nextMap2 != null)
                nextMap2.GotoMap();
        }

        public static void loadMapLeft()
        {
            loadMap(0);
        }

        public static void loadMapCenter()
        {
            loadMap(2);
        }

        public static void loadMapRight()
        {
            loadMap(1);
        }

        // Trạng thái chờ giữa charMove và requestChangeMap
        private static bool _pendingChangeMap = false;
        private static bool _pendingGetMapOffline = false;
        private static long _charMoveSentTime = 0L;
        private const long CHARMAP_TO_CHANGEMAP_DELAY = 200L;
        private static bool _hasInFlightMapChange = false;
        private static bool _lastSentOffline = false;
        private static long _lastMapChangeSentAt = 0L;
        private static int _mapChangeRetryCount = 0;
        private const long MAP_CHANGE_RETRY_AFTER = 1500L; // ms
        private const int MAP_CHANGE_MAX_RETRY = 1;
        private const long MAP_CHANGE_STUCK_RESET_AFTER = 4500L; // ms

        // Gọi trong Update() của AutoXmapFeature hoặc bất kỳ update loop nào
        public static void FlushPendingMapChange()
        {
            if (!_pendingChangeMap && !_pendingGetMapOffline) return;
            if (mSystem.currentTimeMillis() - _charMoveSentTime < CHARMAP_TO_CHANGEMAP_DELAY) return;

            if (_pendingGetMapOffline)
            {
                _pendingGetMapOffline = false;
                Service.gI().getMapOffline();
                _lastSentOffline = true;
            }
            else
            {
                _pendingChangeMap = false;
                Service.gI().requestChangeMap();
                _lastSentOffline = false;
            }

            _hasInFlightMapChange = true;
            _lastMapChangeSentAt = mSystem.currentTimeMillis();
            _mapChangeRetryCount = 0;
            Char.ischangingMap = true;
        }

        // Gọi mỗi frame để chống kẹt đổi map.
        public static void UpdateMapChangeWatchdog()
        {
            if (!Char.ischangingMap)
            {
                _hasInFlightMapChange = false;
                _mapChangeRetryCount = 0;
                return;
            }

            long now = mSystem.currentTimeMillis();
            if (_hasInFlightMapChange && !_pendingChangeMap && !_pendingGetMapOffline
                && _mapChangeRetryCount < MAP_CHANGE_MAX_RETRY
                && now - _lastMapChangeSentAt >= MAP_CHANGE_RETRY_AFTER)
            {
                if (_lastSentOffline) Service.gI().getMapOffline();
                else Service.gI().requestChangeMap();
                _mapChangeRetryCount++;
                _lastMapChangeSentAt = now;
                return;
            }

            if (_hasInFlightMapChange && now - _lastMapChangeSentAt > MAP_CHANGE_STUCK_RESET_AFTER)
            {
                // Global self-heal: không phụ thuộc AutoXmap, tránh kẹt "Xin chờ" vô hạn.
                if (Controller.isStopReadMessage)
                    Controller.isStopReadMessage = false;
                Char.ischangingMap = false;
                _hasInFlightMapChange = false;
                _mapChangeRetryCount = 0;
                return;
            }
        }

        // Queue request đổi map cho flow waypoint/NPC trong NextMap.
        // useImmediateFlush=true khi caller đã tự chờ đủ delay sau charMove.
        public static void QueueMapChangeRequest(bool isOffline, bool useImmediateFlush = false)
        {
            // Đang chờ server phản hồi request trước đó: bỏ qua request trùng để tránh reset timer vô hạn.
            if (_hasInFlightMapChange)
                return;

            bool hasPending = _pendingChangeMap || _pendingGetMapOffline;
            if (hasPending)
            {
                bool pendingOffline = _pendingGetMapOffline;
                if (pendingOffline == isOffline)
                    return;
            }

            // Luôn giữ 1 loại pending duy nhất để tránh flush sai loại request từ state cũ.
            _pendingGetMapOffline = isOffline;
            _pendingChangeMap = !isOffline;

            _charMoveSentTime = useImmediateFlush
                ? mSystem.currentTimeMillis() - CHARMAP_TO_CHANGEMAP_DELAY
                : mSystem.currentTimeMillis();

            // Guard sớm để ngăn spam gotoMap cùng waypoint trước khi Flush thực thi.
            Char.ischangingMap = true;
        }

        private static Waypoint wpLeft;
        private static Waypoint wpCenter;
        private static Waypoint wpRight;

        private static void loadMap(int position)
        {
            if (DataXmap.IsNRDMap(TileMap.mapID))
            {
                teleportInNRDMap(position);
                return;
            }

            loadWaypointsInMap();

            int targetX = 0;
            int targetY = 0;
            bool isOffline = false;

            switch (position)
            {
                case 0:
                    if (wayPointMapLeft[0] != 0 && wayPointMapLeft[1] != 0)
                    {
                        targetX = wayPointMapLeft[0];
                        targetY = wayPointMapLeft[1];
                        if (wpLeft != null) isOffline = wpLeft.isOffline;
                    }
                    else
                    {
                        targetX = 60;
                        targetY = getYGround(60);
                    }
                    break;

                case 1:
                    if (wayPointMapRight[0] != 0 && wayPointMapRight[1] != 0)
                    {
                        targetX = wayPointMapRight[0];
                        targetY = wayPointMapRight[1];
                        if (wpRight != null) isOffline = wpRight.isOffline;
                    }
                    else
                    {
                        targetX = TileMap.pxw - 60;
                        targetY = getYGround(TileMap.pxw - 60);
                    }
                    break;

                case 2:
                    if (wayPointMapCenter[0] != 0 && wayPointMapCenter[1] != 0)
                    {
                        targetX = wayPointMapCenter[0];
                        targetY = wayPointMapCenter[1];
                        if (wpCenter != null) isOffline = wpCenter.isOffline;
                    }
                    else
                    {
                        targetX = TileMap.pxw / 2;
                        targetY = getYGround(TileMap.pxw / 2);
                    }
                    break;
            }

            PathUtils.teleportTo(targetX, targetY);
            // teleportTo() đã gọi charMove() bên trong — không gọi thêm lần 2

            // Ghi nhận pending để FlushPendingMapChange() gửi sau 200ms.
            QueueMapChangeRequest(isOffline);
        }

        private static void loadWaypointsInMap()
        {
            resetSavedWaypoints();
            int waypointCount = TileMap.vGo.size();

            if (waypointCount == 2)
            {
                Waypoint wp0 = (Waypoint)TileMap.vGo.elementAt(0);
                Waypoint wp1 = (Waypoint)TileMap.vGo.elementAt(1);

                if ((wp0.maxX < 60 && wp1.maxX < 60) ||
                    (wp0.minX > TileMap.pxw - 60 && wp1.minX > TileMap.pxw - 60))
                {
                    wayPointMapLeft[0] = wp0.minX + 15;
                    wayPointMapLeft[1] = wp0.maxY;
                    wpLeft = wp0;
                    wayPointMapRight[0] = wp1.maxX - 15;
                    wayPointMapRight[1] = wp1.maxY;
                    wpRight = wp1;
                }
                else if (wp0.maxX < wp1.maxX)
                {
                    wayPointMapLeft[0] = wp0.minX + 15;
                    wayPointMapLeft[1] = wp0.maxY;
                    wpLeft = wp0;
                    wayPointMapRight[0] = wp1.maxX - 15;
                    wayPointMapRight[1] = wp1.maxY;
                    wpRight = wp1;
                }
                else
                {
                    wayPointMapLeft[0] = wp1.minX + 15;
                    wayPointMapLeft[1] = wp1.maxY;
                    wpLeft = wp1;
                    wayPointMapRight[0] = wp0.maxX - 15;
                    wayPointMapRight[1] = wp0.maxY;
                    wpRight = wp0;
                }
                return;
            }

            for (int i = 0; i < waypointCount; i++)
            {
                Waypoint waypoint = (Waypoint)TileMap.vGo.elementAt(i);

                if (waypoint.maxX < 60)
                {
                    wayPointMapLeft[0] = waypoint.minX + 15;
                    wayPointMapLeft[1] = waypoint.maxY;
                    wpLeft = waypoint;
                }
                else if (waypoint.minX > TileMap.pxw - 60)
                {
                    wayPointMapRight[0] = waypoint.maxX - 15;
                    wayPointMapRight[1] = waypoint.maxY;
                    wpRight = waypoint;
                }
                else
                {
                    wayPointMapCenter[0] = waypoint.minX + 15;
                    wayPointMapCenter[1] = waypoint.maxY;
                    wpCenter = waypoint;
                }
            }
        }

        private static void resetSavedWaypoints()
        {
            wayPointMapLeft = new int[2];
            wayPointMapCenter = new int[2];
            wayPointMapRight = new int[2];
            wpLeft = null;
            wpCenter = null;
            wpRight = null;
        }

        public static int getYGround(int x)
        {
            int y = 50;
            int attempts = 0;

            while (attempts < 30)
            {
                attempts++;
                y += 24;

                if (TileMap.tileTypeAt(x, y, 2))
                {
                    if (y % 24 != 0)
                        y -= y % 24;
                    break;
                }
            }
            return y;
        }

        private static void teleportInNRDMap(int position)
        {
            switch (position)
            {
                case 0:
                    PathUtils.teleportTo(60, getYGround(60));
                    break;

                case 1:
                    PathUtils.teleportTo(TileMap.pxw - 60, getYGround(TileMap.pxw - 60));
                    break;

                case 2:
                    for (int i = 0; i < GameScr.vNpc.size(); i++)
                    {
                        Npc npc = (Npc)GameScr.vNpc.elementAt(i);
                        if (npc.template.npcTemplateId >= 30 && npc.template.npcTemplateId <= 36)
                        {
                            Char.myCharz().npcFocus = npc;
                            PathUtils.teleportTo(npc.cx, npc.cy - 3);
                            break;
                        }
                    }
                    break;
            }
        }
    }
}
