using System.Collections.Generic;

namespace NRO_v247.Mods.Xmap
{
    // Port từ XmapPathfinder.java (BFS tìm đường)
    public class XmapPathfinder
    {
        private static XmapPathfinder _instance;

        public static XmapPathfinder GetInstance()
        {
            if (_instance == null) _instance = new XmapPathfinder();
            return _instance;
        }

        private XmapPathfinder() { }

        private class PathNode
        {
            public int[] path;
            public PathNode(int[] path) { this.path = path; }
        }

        // Trả về mảng mapID theo thứ tự từ currentMap đến targetMap
        public int[] FindPath(int mapID, int currentMapID, long cPower, bool canGoCold, List<int> capsuleTargets = null, bool avoidNpc38 = false)
        {
            var validPaths = new List<int[]>();
            var queue = new Queue<PathNode>();
            var visited = new HashSet<int>();

            int[] startPath = { currentMapID };
            queue.Enqueue(new PathNode(startPath));
            visited.Add(currentMapID);

            while (queue.Count > 0)
            {
                PathNode node = queue.Dequeue();
                int[] currentPath = node.path;
                int currentMap = currentPath[currentPath.Length - 1];

                if (currentMap == mapID)
                {
                    if (IsValidPath(currentPath, cPower, canGoCold))
                        validPaths.Add(currentPath);
                }
                else
                {
                    if (!DataXmap.linkMaps.ContainsKey(currentMap)) continue;

                    // 1. Map liền kề mặc định
                    List<NextMap> nextMaps = DataXmap.linkMaps[currentMap];
                    foreach (NextMap nextMap in nextMaps)
                    {
                        int nextMapID = nextMap.MapID;

                        // Bỏ qua cold maps hoàn toàn khi !canGoCold
                        if (!canGoCold && nextMapID >= 105 && nextMapID <= 110)
                            continue;

                        // Bỏ qua NPC tàu thời gian (NPC 38) khi ưu tiên di chuyển nội bộ future maps
                        if (avoidNpc38 && nextMap.Npc == 38)
                            continue;

                        if (!visited.Contains(nextMapID))
                        {
                            visited.Add(nextMapID);
                            queue.Enqueue(new PathNode(AppendToPath(currentPath, nextMapID)));
                        }
                    }

                    // 2. Map từ Capsule (chỉ tính từ điểm khởi đầu)
                    if (currentMap == currentMapID && capsuleTargets != null)
                    {
                        foreach (int capMapId in capsuleTargets)
                        {
                            // Bỏ qua cold maps hoàn toàn khi !canGoCold
                            if (!canGoCold && capMapId >= 105 && capMapId <= 110)
                                continue;

                            if (!visited.Contains(capMapId))
                            {
                                visited.Add(capMapId);
                                queue.Enqueue(new PathNode(AppendToPath(currentPath, capMapId)));
                            }
                        }
                    }
                }
            }

            // Chọn đường ngắn nhất
            int shortestLength = int.MaxValue;
            int[] result = null;
            foreach (int[] path in validPaths)
            {
                if (path.Length < shortestLength)
                {
                    shortestLength = path.Length;
                    result = path;
                }
            }
            return result;
        }

        private bool IsValidPath(int[] path, long cPower, bool canGoCold)
        {
            if (HasWayGoFutureAndBack(path)) return false;
            if (!canGoCold && HasWayGoToColdMap(path)) return false;

            foreach (int mapId in path)
            {
                if (mapId != 155 && mapId >= 153 && mapId <= 159 && cPower < DataXmap.POWER_REQUIREMENT_40B)
                    return false;

                if ((mapId == 155 || mapId == 166) && cPower < DataXmap.POWER_REQUIREMENT_60B)
                    return false;

                foreach (int futureMapId in DataXmap.idMapsTuongLai)
                {
                    try
                    {
                        if (Char.myCharz().taskMaint.taskId <= 24 && mapId == futureMapId)
                            return false;
                    }
                    catch { }
                }
            }

            return true;
        }

        private bool HasWayGoFutureAndBack(int[] ways)
        {
            for (int i = 1; i < ways.Length - 1; i++)
            {
                if (ways[i] == 102 && ways[i + 1] == 24 &&
                    (ways[i - 1] == 27 || ways[i - 1] == 28 || ways[i - 1] == 29))
                    return true;
            }
            return false;
        }

        private bool HasWayGoToColdMap(int[] ways)
        {
            foreach (int way in ways)
                if (way >= 105 && way <= 110) return true;
            return false;
        }

        private int[] AppendToPath(int[] path, int newMapID)
        {
            int[] newPath = new int[path.Length + 1];
            path.CopyTo(newPath, 0);
            newPath[path.Length] = newMapID;
            return newPath;
        }

        // Tìm NextMap object cụ thể để GotoMap
        public NextMap FindNextMapToGo(int currentMapID, int nextMapID)
        {
            if (!DataXmap.linkMaps.ContainsKey(currentMapID)) return null;

            NextMap preferred = null;
            NextMap fallback = null;

            foreach (NextMap map in DataXmap.linkMaps[currentMapID])
            {
                if (map.MapID != nextMapID) continue;
                if (map.Npc != -1 || map.walk)
                {
                    preferred = map;
                    break;
                }
                if (map.Npc == -1 && map.Index == -1 && !map.walk)
                {
                    if (fallback == null || map.WaypointPosition != 0)
                        fallback = map;
                }
            }

            return preferred ?? fallback;
        }

        public string GetPathErrorMessage(int mapID, int currentMapID, long currentPower, bool canGoCold)
        {
            if (mapID == 154 && currentPower < DataXmap.POWER_REQUIREMENT_40B)
                return "Yêu cầu sức mạnh tối thiểu cho map 154: 40,000,000,000.";
            if (mapID == 155 && currentPower < DataXmap.POWER_REQUIREMENT_60B)
                return "Yêu cầu sức mạnh tối thiểu cho map 155: 60,000,000,000.";
            if (mapID == 166 && currentPower < DataXmap.POWER_REQUIREMENT_60B)
                return "Yêu cầu sức mạnh tối thiểu cho map 166: 60,000,000,000.";
            if (mapID >= 153 && mapID <= 159 && mapID != 155 && currentPower < DataXmap.POWER_REQUIREMENT_40B)
                return "Yêu cầu sức mạnh tối thiểu cho map " + mapID + ": 40,000,000,000.";

            try
            {
                if (DataXmap.IsFutureMap(mapID) && Char.myCharz().taskMaint.taskId <= 24)
                    return "Hãy hoàn thành nhiệm vụ để vào map " + mapID + ".";

                if (Char.myCharz().clan == null && DataXmap.RequiresClan(mapID))
                    return "Cần có bang hội để vào map " + mapID + ".";
            }
            catch { }

            if (mapID == 160)
                return "Không có Nhẫn thời không!";

            return "Không thể tìm thấy đường đi từ map " + currentMapID + " đến map " + mapID + ".";
        }
    }
}
