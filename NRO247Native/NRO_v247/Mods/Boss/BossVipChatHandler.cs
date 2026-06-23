using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NRO_v247.Mods.Notifications;

namespace NRO_v247.Mods.Boss
{
    /// <summary>
    /// Lắng nghe thông báo VipChat (Cmd 93) để phát hiện Boss xuất hiện.
    /// Khi nhận được thông báo "BOSS X vừa xuất hiện tại Y", parse ra mapId
    /// rồi gọi AutoBossFeature.StartHuntingAt() để bắt đầu di chuyển.
    /// Chỉ hoạt động khi _scoutOnVipChat == true trong AutoBossFeature.
    /// </summary>
    internal class BossVipChatHandler
    {
        // Pattern khớp cả server VN lẫn server lậu EN
        // VD: "BOSS Đại Ma Vương vừa xuất hiện tại Làng Pháo Đài"
        // VD: "BOSS BigBoss appear at Some Map"
        private static readonly Regex _bossPattern = new Regex(
            @"BOSS\s+(.+?)\s+(?:vừa xuất hiện tại|appear at)\s+(.+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly AutoBossFeature _feature;
        private readonly List<string> _bossNames;
        private readonly List<int> _mapsToScout; // null = không giới hạn map
        private bool _scoutOnVipChat;
        private bool _limitMap;

        public BossVipChatHandler(AutoBossFeature feature, List<string> bossNames, List<int> mapsToScout)
        {
            _feature = feature;
            _bossNames = bossNames;
            _mapsToScout = mapsToScout;
            NotifyCatcher.OnNotifyReceived += OnNotify;
        }

        /// <summary>
        /// Gọi từ AutoBossFeature.ApplySettingsFromPanel() để cập nhật trạng thái bật/tắt.
        /// </summary>
        public void SetScoutOnVipChat(bool enabled, bool limitMap)
        {
            _scoutOnVipChat = enabled;
            _limitMap = limitMap;
        }

        private void OnNotify(NotifyCatcher.NotifyEvent ev)
        {
            if (!_scoutOnVipChat) return;
            if (ev.Type != NotifyCatcher.NotifyType.ChatVip) return;

            string msg = ev.Message;
            if (string.IsNullOrEmpty(msg)) return;
            if (!msg.StartsWith("BOSS ", StringComparison.OrdinalIgnoreCase)) return;

            var match = _bossPattern.Match(msg);
            if (!match.Success) return;

            string bossName = match.Groups[1].Value.Trim();
            string mapName  = match.Groups[2].Value.Trim();

            // Lọc: chỉ xử lý boss có trong danh sách cấu hình (nếu đã cấu hình)
            bool isTdstnmTarget = false;
            if (_bossNames != null && _bossNames.Count > 0)
            {
                bool matched = false;
                string bossLower = bossName.ToLower();

                // Lọc alias đăc biệt "tdstnm" (Tiểu Đội Sát Thủ Namek)
                foreach (var b in _bossNames)
                {
                    if (!string.IsNullOrEmpty(b) && b.Trim().ToLower() == "tdstnm")
                    {
                        if (bossLower.Contains("số 4") || bossLower.Contains("số 3") || 
                            bossLower.Contains("số 1") || bossLower.Contains("số 2") || 
                            bossLower.Contains("tiểu đội trưởng"))
                        {
                            matched = true;
                            isTdstnmTarget = true;
                            break;
                        }
                    }
                }

                if (!matched)
                {
                    foreach (var b in _bossNames)
                    {
                        if (!string.IsNullOrEmpty(b) && bossLower.Contains(b.Trim().ToLower()))
                        {
                            matched = true;
                            break;
                        }
                    }
                }
                if (!matched) return;
            }

            int mapId = GetMapID(bossName, mapName);
            if (mapId < 0)
            {
                GameLogger.SendLog("BOSS", $"[VipChat] Không tìm được mapId cho '{mapName}' — bỏ qua");
                return;
            }

            // Ràng buộc riêng cho tdstnm: Chỉ tìm các map thuộc hành tinh Namek
            if (isTdstnmTarget)
            {
                // Danh sách map Namek theo yêu cầu
                int[] idMapsNamek = { 43, 22, 7, 8, 9, 11, 12, 13, 10, 31, 32, 33, 34, 25 }; 
                if (System.Array.IndexOf(idMapsNamek, mapId) < 0)
                {
                    GameLogger.SendLog("BOSS", $"[VipChat] Boss '{bossName}' ở map {mapId} ngoài Namek — bỏ qua tdstnm");
                    return;
                }
            }

            // Lọc theo map: nếu bật limitMap và đăng ký danh sách map,
            // chỉ đi khi boss xuất hiện đúng map trong list
            if (_limitMap && _mapsToScout != null && _mapsToScout.Count > 0)
            {
                if (!_mapsToScout.Contains(mapId))
                {
                    GameLogger.SendLog("BOSS", $"[VipChat] Boss '{bossName}' ở map {mapId} ('{mapName}') ngoài danh sách — bỏ qua");
                    return;
                }
            }

            GameLogger.SendLog("BOSS", $"[VipChat] Boss '{bossName}' tại '{mapName}' (mapId={mapId})");

            // zoneId = -1: VipChat không biết khu cụ thể.
            // StartHuntingAt với zoneId=-1 sẽ tự vào Scouting để quét từng khu trên map đó.
            // Luồng đầy đủ: Scouting → Hunting → WaitingForBoss → BOSS_DEAD → AntiAdmin → Reset → Idle
            _feature.StartHuntingAt(mapId, -1, bossName);
        }

        /// <summary>
        /// Tra cứu mapId từ tên map, giống logic Unity Boss.GetMapID().
        /// Trường hợp đặc biệt: Tiểu đội trưởng / Số + "Trạm tàu vũ trụ" → trả về 25.
        /// </summary>
        private static int GetMapID(string bossName, string mapName)
        {
            if (TileMap.mapNames == null) return -1;

            string mapNorm = mapName.ToLower().Trim().Replace("  ", " ");
            string bossLower = bossName.ToLower().Trim().Replace("  ", " ");

            for (int i = 0; i < TileMap.mapNames.Length; i++)
            {
                if (TileMap.mapNames[i] == null) continue;
                // Bỏ qua map đặc biệt giống Unity
                if (i == 40 || i == 39 || i == 155) continue;

                string namNorm = TileMap.mapNames[i].ToLower().Trim().Replace("  ", " ");
                if (!namNorm.Equals(mapNorm)) continue;

                // Trường hợp đặc biệt: Tiểu đội trưởng / Số tại Trạm tàu vũ trụ → map 25
                if (mapName == "Trạm tàu vũ trụ" &&
                    (bossLower.StartsWith("tiểu đội trưởng") || bossLower.StartsWith("số")))
                {
                    return 25;
                }

                return i;
            }
            return -1;
        }
    }
}
