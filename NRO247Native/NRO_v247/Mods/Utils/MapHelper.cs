using System;

namespace NRO_v247.Mods.Utils
{
    public static class MapHelper
    {
        public static bool IsVillage(int mapId)
        {
            return mapId == 0 || mapId == 7 || mapId == 14;
        }

        public static bool IsHomeMap(int mapId)
        {
            return mapId == 21 || mapId == 22 || mapId == 23;
        }

        public static bool IsNrdMap(int mapId)
        {
            return mapId >= 85 && mapId <= 91;
        }

        public static bool IsFutureMap(int mapId)
        {
            return mapId >= 92 && mapId <= 104;
        }

        public static bool IsPlantMap(int mapId)
        {
            return mapId == 42 || mapId == 43 || mapId == 44;
        }

        public static bool IsBaseMap(int mapId)
        {
            // Trạm vũ trụ các hành tinh, tàu vũ trụ
            return mapId == 24 || mapId == 25 || mapId == 26 || mapId == 84 || mapId == 111; 
        }

        public static bool IsOfflineMap(int mapId)
        {
            // Các map an toàn cơ bản không có quái (Offline / Safe zone)
            return IsVillage(mapId) || IsHomeMap(mapId) || IsBaseMap(mapId) || IsPlantMap(mapId) || mapId == 47; 
        }

        public static int GetHomeMapId(int gender)
        {
            return gender + 21; // 21 (Trái Đất), 22 (Namek), 23 (Xayda)
        }

        public static int GetStationMapId(int gender)
        {
            return gender + 24; // 24 (Trạm tàu vũ trụ Trái Đất), 25, 26
        }
    }
}
