using System.Collections.Generic;

namespace NRO_v247.Mods.Xmap
{
    // Port từ XmapData.java
    public static class DataXmap
    {
        public static readonly int[] idMapsHome = { 21, 22, 23 };
        public static readonly int[] idMapsNamek = { 43, 22, 7, 8, 9, 11, 12, 13, 10, 31, 32, 33, 34, 43, 25 };
        public static readonly int[] idMapsXayda = { 44, 23, 14, 15, 16, 17, 18, 20, 19, 35, 36, 37, 38, 52, 44, 26, 84, 113, 127, 129 };
        public static readonly int[] idMapsTraiDat = { 42, 21, 0, 1, 2, 3, 4, 5, 6, 27, 28, 29, 30, 47, 42, 24, 53, 58, 59, 60, 61, 62, 55, 56, 54, 57 };
        public static readonly int[] idMapsTuongLai = { 102, 92, 93, 94, 96, 97, 98, 99, 100, 103 };
        public static readonly int[] idMapsCold = { 109, 108, 107, 110, 106, 105 };
        public static readonly int[] idMapsNappa = { 68, 69, 70, 71, 72, 64, 65, 63, 66, 67, 73, 74, 75, 76, 77, 81, 82, 83, 79, 80, 131, 132, 133 };
        public static readonly int[] idMapsThapleo = { 46, 45, 48, 50, 154, 155, 166 };
        public static readonly int[] idMapsManhVoBT = { 153, 156, 157, 158, 159 };
        public static readonly int[] idMapsKhiGas = { 149, 147, 152, 151, 148 };
        public static readonly int[] idMapsKhac = { 123, 124, 122 };

        public class PlanetEntry
        {
            public string Name;
            public int[] Maps;

            public PlanetEntry(string name, int[] maps)
            {
                Name = name;
                Maps = maps;
            }
        }

        public static List<PlanetEntry> planetList;
        public static Dictionary<int, List<NextMap>> linkMaps;

        // Hằng số power requirement
        public const long POWER_REQUIREMENT_40B = 40000000000L;
        public const long POWER_REQUIREMENT_60B = 60000000000L;

        public const int NRD_MAP_START = 85;
        public const int NRD_MAP_END = 91;

        static DataXmap()
        {
            // Chỉ load phần không phụ thuộc vào Char (linkMaps, planets)
            // linkMaps[999] (home theo gender) sẽ được load sau khi login via LoadHomeMap()
            LoadData();
        }

        public static void LoadData()
        {
            linkMaps = new Dictionary<int, List<NextMap>>();
            planetList = new List<PlanetEntry>();

            LoadLinkMaps();
            LoadNPCLinkMaps();
            LoadPlanetList();
        }

        /// <summary>
        /// Cập nhật linkMaps[999] (home map theo gender) sau khi đăng nhập thành công.
        /// Gọi từ AutoXmapFeature.Update() khi Char.myCharz() != null lần đầu tiên.
        /// </summary>
        public static void LoadHomeMap()
        {
            try
            {
                int gender = Char.myCharz().cgender;
                linkMaps.Remove(999);
                var list999 = new List<NextMap>();
                list999.Add(new NextMap(24 + gender, 10, 0, -1, -1, false, -1, -1));
                linkMaps[999] = list999;
            }
            catch { }
        }

        private static void LoadLinkMaps()
        {
            AddLinkMaps(new[] { 0, 21 });
            AddLinkMaps(new[] { 1, 47 });
            AddLinkMaps(new[] { 47, 111 });
            AddLinkMaps(new[] { 2, 24 });
            AddLinkMaps(new[] { 5, 29 });
            AddLinkMaps(new[] { 7, 22 });
            AddLinkMaps(new[] { 9, 25 });
            AddLinkMaps(new[] { 13, 33 });
            AddLinkMaps(new[] { 14, 23 });
            AddLinkMaps(new[] { 16, 26 });
            AddLinkMaps(new[] { 20, 37 });
            AddLinkMaps(new[] { 39, 21 });
            AddLinkMaps(new[] { 40, 22 });
            AddLinkMaps(new[] { 41, 23 });
            AddLinkMaps(new[] { 109, 105 });
            AddLinkMaps(new[] { 109, 106 });
            AddLinkMaps(new[] { 106, 107 });
            AddLinkMaps(new[] { 108, 105 });
            AddLinkMaps(new[] { 80, 105 });
            AddLinkMaps(new[] { 3, 27, 28, 29, 30 });
            AddLinkMaps(new[] { 11, 31, 32, 33, 34 });
            AddLinkMaps(new[] { 17, 35, 36, 37, 38 });
            AddLinkMaps(new[] { 109, 108, 107, 110, 106 });
            AddLinkMaps(new[] { 47, 46, 45, 48 });
            AddLinkMaps(new[] { 131, 132, 133 });
            AddLinkMaps(new[] { 42, 0, 1, 2, 3, 4, 5, 6 });
            AddLinkMaps(new[] { 43, 7, 8, 9, 11, 12, 13, 10 });
            AddLinkMaps(new[] { 52, 44, 14, 15, 16, 17, 18, 20, 19 });
            AddLinkMaps(new[] { 53, 58, 59, 60, 61, 62, 55, 56, 54, 57 });
            AddLinkMaps(new[] { 68, 69, 70, 71, 72, 64, 65, 63, 66, 67, 73, 74, 75, 76, 77, 81, 82, 83, 79, 80 });
            AddLinkMaps(new[] { 102, 92, 93, 94, 96, 97, 98, 99, 100, 103 });
            AddLinkMaps(new[] { 153, 156, 157, 158, 159 });
            AddLinkMaps(new[] { 46, 45, 48, 50, 154, 155, 166 });
            AddLinkMaps(new[] { 149, 147, 152, 151, 148 });
            AddLinkMaps(new[] { 139, 140 });
            AddLinkMaps(new[] { 160, 161, 162, 163 });
            AddLinkMaps(new[] { 84, 104 });
            AddLinkMaps(new[] { 123, 124, 122 });
        }

        private static void LoadNPCLinkMaps()
        {
            AddNPCLinkMap(19, 68, 12, 1, -1, -1, false, -1, -1);
            AddNPCLinkMap(19, 109, 12, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(19, 109, 12, 1, -1, -1, false, -1, -1);
            AddNPCLinkMap(24, 25, 10, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(24, 26, 10, 1, -1, -1, false, -1, -1);
            AddNPCLinkMap(24, 84, 10, 2, -1, -1, false, -1, -1);
            AddNPCLinkMap(25, 24, 11, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(25, 26, 11, 1, -1, -1, false, -1, -1);
            AddNPCLinkMap(25, 84, 11, 2, -1, -1, false, -1, -1);
            AddNPCLinkMap(26, 24, 12, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(26, 25, 12, 1, -1, -1, false, -1, -1);
            AddNPCLinkMap(26, 84, 12, 2, -1, -1, false, -1, -1);
            AddNPCLinkMap(27, 102, 38, 1, -1, -1, false, -1, -1);
            AddNPCLinkMap(27, 53, 25, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(28, 102, 38, 1, -1, -1, false, -1, -1);
            AddNPCLinkMap(29, 102, 38, 1, -1, -1, false, -1, -1);
            AddNPCLinkMap(45, 48, 19, 3, -1, -1, false, -1, -1);
            AddNPCLinkMap(52, 127, 44, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(52, 129, 23, 2, -1, -1, false, -1, -1);
            AddNPCLinkMap(52, 113, 23, 1, -1, -1, false, -1, -1);
            AddNPCLinkMap(68, 19, 12, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(80, 131, 60, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(102, 27, 38, 1, -1, -1, false, -1, -1);
            AddNPCLinkMap(113, 52, 22, 4, -1, -1, false, -1, -1);
            AddNPCLinkMap(127, 52, 44, 2, -1, -1, false, -1, -1);
            AddNPCLinkMap(129, 52, 23, 3, -1, -1, false, -1, -1);
            AddNPCLinkMap(131, 80, 60, 1, -1, -1, false, -1, -1);
            AddNPCLinkMapByName(5, 153, 13, "Nói chuyện", "Về khu vực bang");
            AddNPCLinkMapByName(153, 156, 47, "OK", null);
            AddNPCLinkMap(48, 50, 20, 3, 1, -1, false, -1, -1);
            AddNPCLinkMap(50, 154, 44, 1, -1, -1, false, -1, -1);
            AddNPCLinkMap(154, 155, 44, 1, -1, -1, false, -1, -1);
            AddNPCLinkMap(50, 48, 44, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(48, 45, 20, 3, 0, -1, false, -1, -1);
            AddNPCLinkMap(154, 50, 55, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(155, 154, 44, 0, -1, -1, false, -1, -1);
            AddNPCLinkMapByName(153, 5, 10, "Đảo Kame", null);
            AddNPCLinkMap(155, 166, -1, -1, -1, -1, true, 1400, 600);
            AddNPCLinkMap(46, 47, -1, -1, -1, -1, true, 80, 700);
            AddNPCLinkMap(45, 46, -1, -1, -1, -1, true, 80, 700);
            AddNPCLinkMap(46, 45, -1, -1, -1, -1, true, 380, 90);
            AddNPCLinkMap(0, 149, 67, 3, 0, -1, false, -1, -1);
            AddNPCLinkMap(24, 139, 63, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(139, 24, 63, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(84, 26, 10, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(84, 25, 10, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(84, 24, 10, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(126, 19, 53, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(19, 126, 53, 0, -1, -1, false, -1, -1);
            AddNPCLinkMap(52, 181, 44, 1, 0, -1, false, -1, -1);
            AddNPCLinkMap(181, 52, 44, 0, -1, -1, false, -1, -1);
            AddNPCLinkMapByName(0, 123, 49, "Đồng ý", null);
            AddNPCLinkMapByName(123, 0, 49, "Về Làng Aru", null);

            // Map 999: home theo gender — được load sau khi login via LoadHomeMap()
            // Không cần placeholder vì BFS dùng ContainsKey trước khi truy cập
        }

        private static void LoadPlanetList()
        {
            planetList.Add(new PlanetEntry("Về nhà", idMapsHome));
            planetList.Add(new PlanetEntry("Trái đất", idMapsTraiDat));
            planetList.Add(new PlanetEntry("Namek", idMapsNamek));
            planetList.Add(new PlanetEntry("Xayda", idMapsXayda));
            planetList.Add(new PlanetEntry("Fide", idMapsNappa));
            planetList.Add(new PlanetEntry("Tương lai", idMapsTuongLai));
            planetList.Add(new PlanetEntry("Cold", idMapsCold));
            planetList.Add(new PlanetEntry("Tháp leo", idMapsThapleo));
            planetList.Add(new PlanetEntry("Khu vực bang", idMapsManhVoBT));
            planetList.Add(new PlanetEntry("Khí Gas", idMapsKhiGas));
            planetList.Add(new PlanetEntry("Khác", idMapsKhac));
        }

        private static void AddLinkMaps(int[] link)
        {
            for (int i = 0; i < link.Length; i++)
            {
                if (!linkMaps.ContainsKey(link[i]))
                    linkMaps[link[i]] = new List<NextMap>();

                List<NextMap> list = linkMaps[link[i]];

                if (i != 0)
                {
                    NextMap nm = new NextMap(link[i - 1], -1, -1, -1, -1, false, -1, -1);
                    nm.WaypointPosition = -1;
                    list.Add(nm);
                }
                if (i != link.Length - 1)
                {
                    NextMap nm = new NextMap(link[i + 1], -1, -1, -1, -1, false, -1, -1);
                    nm.WaypointPosition = 1;
                    list.Add(nm);
                }
            }
        }

        private static void AddNPCLinkMapByName(int currentMapID, int nextMapID, int npcID, string selectName, string selectName2)
        {
            if (!linkMaps.ContainsKey(currentMapID))
                linkMaps[currentMapID] = new List<NextMap>();

            linkMaps[currentMapID].Add(new NextMap(nextMapID, npcID, selectName, selectName2, null, false, -1, -1));
        }

        private static void AddNPCLinkMap(int currentMapID, int nextMapID, int npcID, int select, int select2, int select3, bool walk, int x, int y)
        {
            if (!linkMaps.ContainsKey(currentMapID))
                linkMaps[currentMapID] = new List<NextMap>();

            linkMaps[currentMapID].Add(new NextMap(nextMapID, npcID, select, select2, select3, walk, x, y));
        }

        public static bool IsHomeMap(int mapId)
        {
            foreach (int id in idMapsHome)
                if (mapId == id) return true;
            return false;
        }

        public static bool IsHomeArray(int[] array)
        {
            if (array == null || array.Length != idMapsHome.Length) return false;
            for (int i = 0; i < idMapsHome.Length; i++)
                if (array[i] != idMapsHome[i]) return false;
            return true;
        }

        public static bool IsFutureMap(int mapID)
        {
            foreach (int id in idMapsTuongLai)
                if (id == mapID) return true;
            return false;
        }

        public static bool IsNRDMap(int mapID) => mapID >= NRD_MAP_START && mapID <= NRD_MAP_END;

        public static bool RequiresClan(int mapID)
        {
            foreach (int id in idMapsKhiGas)
                if (id == mapID) return true;
            foreach (int id in idMapsManhVoBT)
                if (id == mapID) return true;
            if (mapID >= 53 && mapID <= 62) return true;
            return false;
        }
    }
}
