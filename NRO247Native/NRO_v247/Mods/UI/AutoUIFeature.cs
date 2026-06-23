using System;
using System.Collections.Generic;

namespace NRO_v247.Mods.UI
{
    public class AutoUIFeature : IAutoFeature
    {
        public bool IsActive => true;
        public string CurrentState => "";
        public bool IsUtilityTask => true;
        
        // This feature just renders, no intense update cycle needed.
        public void Update() { }

        // Tracker cho thời gian bắt đầu chịu hiệu ứng của Boss
        private static Dictionary<int, long> _tieStartMs = new Dictionary<int, long>();
        private static Dictionary<int, long> _stunStartMs = new Dictionary<int, long>();
        private static int _lastMapId = -1;
        private static int _lastZoneId = -1;

        public void OnPaintOverlay(mGraphics g)
        {
            try
            {
                // 1. Nếu đang ở màn hình GameScr -> Vẽ UI In-game hiện tại
                if (GameCanvas.currentScreen is GameScr)
                {
                    PaintGameScrOverlay(g);
                }
                // 2. Nếu đang ở màn hình chờ/login -> Vẽ bảng tiến trình Login
                else if (GameCanvas.currentScreen is SplashScr || 
                         GameCanvas.currentScreen is LoginScr || 
                         GameCanvas.currentScreen is ServerListScreen || 
                         GameCanvas.currentScreen is ServerScr ||
                         GameCanvas.currentScreen is SelectCharScr ||
                         GameCanvas.currentScreen is CreateCharScr)
                {
                    PaintLoginOverlay(g);
                }
            }
            catch (Exception ex)
            {
                Cout.LogError("Loi ham paint UI Tool: " + ex.ToString());
            }
        }

        private void PaintGameScrOverlay(mGraphics g)
        {
            if (GameCanvas.panel != null && GameCanvas.panel.isShow) return;

            // 0. Proxy Info (Below MP bar, approx y=20)
            string proxyText;
            if (Session_ME.HasProxyConfigured)
            {
                if (string.IsNullOrEmpty(Session_ME.ProxyHost) || Session_ME.ProxyPort <= 0)
                {
                    proxyText = "Proxy: SAI ĐỊNH DẠNG/LỖI !";
                }
                else
                {
                    proxyText = $"Proxy: {Session_ME.ProxyHost}:{Session_ME.ProxyPort}";
                }
            }
            else
            {
                proxyText = "Proxy: Không (Direct)";
            }
            mFont.tahoma_7b_red.drawString(g, proxyText, 83, 34, 0, mFont.tahoma_7b_dark);

            // 0b. Danh sách player trong map ở góc trên-phải
            if (GameScr.vCharInMap != null)
            {
                Char meChar = Char.myCharz();
                int listY = 110;
                int rank = 1;
                for (int i = 0; i < GameScr.vCharInMap.size(); i++)
                {
                    Char c = (Char)GameScr.vCharInMap.elementAt(i);
                    if (c == null || string.IsNullOrEmpty(c.cName)) continue;
                    if (c.isPet || c.isMiniPet) continue;
                    if (c.cName.StartsWith("#") || c.cName.StartsWith("$")) continue;
                    if (meChar != null && c.charID == meChar.charID) continue;
                    
                    string entry = $"{rank}. {c.cName}";
                    mFont.tahoma_7b_red.drawString(g, entry, GameCanvas.w - 2, listY, 1, mFont.tahoma_7b_dark);
                    listY += 12;
                    rank++;
                }
            }

            int textX = 5; // Left alignment
            int textY = 110;
            if (TileMap.mapID == 172) textY += 15; // Dodge the timer bar

            // 1. Basic Info
            string mapStr = "Map: " + TileMap.mapID;
            string zoneStr = "Zone: " + TileMap.zoneID;
            string posStr = "Pos: " + (Char.myCharz() != null ? $"{Char.myCharz().cx}, {Char.myCharz().cy}" : "...");

            mFont.tahoma_7b_white.drawString(g, mapStr, textX, textY, 0, mFont.tahoma_7b_dark);
            textY += 12;
            mFont.tahoma_7b_white.drawString(g, zoneStr, textX, textY, 0, mFont.tahoma_7b_dark);
            textY += 12;
            mFont.tahoma_7b_white.drawString(g, posStr, textX, textY, 0, mFont.tahoma_7b_dark);
            textY += 12;

            // 2. Fusion State
            bool isFusion = Char.myCharz() != null && (Char.myCharz().isFusion || Char.myCharz().isNhapThe);
            string fusionStr = "Fusion: " + (isFusion ? "True" : "False");
            mFont fontFusion = isFusion ? mFont.tahoma_7b_yellow : mFont.tahoma_7b_white;
            fontFusion.drawString(g, fusionStr, textX, textY, 0, mFont.tahoma_7b_dark);
            textY += 12;

            // 3. Mob Stats (Only when auto training)
            if (ModBootstrap.IsAnyAutoTrainActive())
            {
                int totalMobs = 0;
                int liveMobs = 0;
                int deadMobs = 0;

                if (GameScr.vMob != null)
                {
                    totalMobs = GameScr.vMob.size();
                    for (int i = 0; i < totalMobs; i++)
                    {
                        Mob mob = (Mob)GameScr.vMob.elementAt(i);
                        if (mob.status == 0 || mob.status == 1) // 0: INHELL, 1: DEADFLY
                        {
                            deadMobs++;
                        }
                        else
                        {
                            liveMobs++;
                        }
                    }
                }

                string mobStatsStr = $"Mobs: {liveMobs} / {deadMobs} / {totalMobs}";
                mFont.tahoma_7b_white.drawString(g, mobStatsStr, textX, textY, 0, mFont.tahoma_7b_dark);
            }

            // 4. Boss HPs & Status
            if (TileMap.mapID != _lastMapId || TileMap.zoneID != _lastZoneId)
            {
                _tieStartMs.Clear();
                _stunStartMs.Clear();
                _lastMapId = TileMap.mapID;
                _lastZoneId = TileMap.zoneID;
            }

            long now = mSystem.currentTimeMillis();
            int cx = GameCanvas.w / 2;
            int cy = 100;

            if (GameScr.vCharInMap != null)
            {
                for (int i = 0; i < GameScr.vCharInMap.size(); i++)
                {
                    Char c = (Char)GameScr.vCharInMap.elementAt(i);
                    if (c == null || string.IsNullOrEmpty(c.cName)) continue;
                    if (c.isPet || c.isMiniPet || c.cName.StartsWith("#") || c.cName.StartsWith("$")) continue;
                    
                    Char me = Char.myCharz();
                    if (me == null || me.charFocus == null || me.charFocus.charID != c.charID) continue;
                    if (c.cTypePk != 5) continue;
                    
                    if (c.cHP <= 0 || c.isDie || c.statusMe == 14 || c.statusMe == 5)
                    {
                        _tieStartMs.Remove(c.charID);
                        _stunStartMs.Remove(c.charID);
                        continue;
                    }

                    if (c.holdEffID != 0)
                    {
                        if (!_tieStartMs.ContainsKey(c.charID))
                            _tieStartMs[c.charID] = now;
                    }
                    else
                    {
                        _tieStartMs.Remove(c.charID);
                    }

                    bool isStunned = c.blindEff || c.sleepEff;
                    if (isStunned)
                    {
                        if (!_stunStartMs.ContainsKey(c.charID))
                            _stunStartMs[c.charID] = now;
                    }
                    else
                    {
                        _stunStartMs.Remove(c.charID);
                    }

                    string hpText = $"{FormatLargeNumber(c.cHP)}/{FormatLargeNumber(c.cHPFull)}";
                    string title = $"{c.cName} [{hpText}]";
                    mFont.tahoma_7b_red.drawString(g, title, cx, cy, 2, mFont.tahoma_7b_dark);
                    cy += 12;

                    if (c.holdEffID != 0)
                    {
                        long elapsed = now - _tieStartMs[c.charID];
                        long remainDuration = 35000 - elapsed;
                        if (remainDuration < 0) remainDuration = 0;
                        string tieStr = $"Bị trói ({remainDuration / 1000}s)";
                        mFont.tahoma_7b_white.drawString(g, tieStr, cx, cy, 2, mFont.tahoma_7b_dark);
                        cy += 12;
                    }

                    if (isStunned)
                    {
                        long elapsed = now - _stunStartMs[c.charID];
                        long remainDuration = 5000 - elapsed;
                        if (remainDuration < 0) remainDuration = 0;
                        string stunStr = $"Bị choáng ({remainDuration / 1000}s)";
                        mFont.tahoma_7b_white.drawString(g, stunStr, cx, cy, 2, mFont.tahoma_7b_dark);
                        cy += 12;
                    }
                    cy += 5;
                }
            }
        }

        /// <summary>
        /// Vẽ bảng thông tin trạng thái login ở góc trên trái màn hình.
        /// Hiển thị khi AutoLogin đang bật và client chưa vào game.
        /// </summary>
        private static void PaintLoginOverlay(mGraphics g)
        {
            try
            {
                // Nội dung các dòng
                string proxyStatus;
                if (Session_ME.HasProxyConfigured)
                {
                    if (string.IsNullOrEmpty(Session_ME.ProxyHost) || Session_ME.ProxyPort <= 0)
                        proxyStatus = "SAI ĐỊNH DẠNG/LỖI !";
                    else
                        proxyStatus = $"{Session_ME.ProxyHost}:{Session_ME.ProxyPort}";
                }
                else
                {
                    proxyStatus = "Không (Direct)";
                }
                string ipPortStr   = (string.IsNullOrEmpty(AutoLogin.IP) || AutoLogin.Port == 0)
                                   ? "Chưa có"
                                   : $"{AutoLogin.IP}:{AutoLogin.Port}";

                string statusStr   = AutoLogin.gI().GetStatus();
                // Nếu có thông điệp server (bảo trì, sai pass...) thì ưu tiên hiện
                string overrideMsg = AutoMod.GlobalOverrideState;
                if (!string.IsNullOrEmpty(overrideMsg))
                    statusStr = overrideMsg;

                string[] lines = new[]
                {
                    "Đang tự động đăng nhập.",
                    $"Proxy: {proxyStatus}",
                    $"Status: {statusStr}",
                    $"IP:PORT: {ipPortStr}",
                };

                // Tính kích thước bảng — dùng getWidth đúng API mFont
                int padding = 4;
                int lineH   = 11;
                int boxW    = 0;
                foreach (var line in lines)
                {
                    int lw = mFont.tahoma_7b_white.getWidth(line);
                    if (lw > boxW) boxW = lw;
                }
                boxW += padding * 2;
                int boxH = lines.Length * lineH + padding * 2;

                // Vẽ nền đen mờ ở góc trên trái
                g.setColor(0x000000);
                g.fillRect(5, 5, boxW, boxH);
                g.setColor(0xFF4400); // Viền cam đỏ
                g.drawRect(5, 5, boxW, boxH);

                // Vẽ từng dòng chữ
                int tx = 5 + padding;
                int ty = 5 + padding;
                for (int i = 0; i < lines.Length; i++)
                {
                    mFont font = (i == 0) ? mFont.tahoma_7b_yellow  // Dòng tiêu đề: vàng
                                          : mFont.tahoma_7b_white;   // Các dòng còn lại: trắng
                    font.drawString(g, lines[i], tx, ty + i * lineH, 0, mFont.tahoma_7b_dark);
                }
            }
            catch { }
        }

        /// <summary>
        /// Rút gọn số lớn (HP, Sức mạnh, Vàng...) sang định dạng k, tr, tỷ.
        /// </summary>
        private static string FormatLargeNumber(long number)
        {
            if (number >= 1000000000)
            {
                return $"{(double)number / 1000000000:0.##} tỷ".Replace(',', '.');
            }
            if (number >= 1000000)
            {
                return $"{(double)number / 1000000:0.##} tr".Replace(',', '.');
            }
            if (number >= 1000)
            {
                return $"{(double)number / 1000:0.##} k".Replace(',', '.');
            }
            return number.ToString();
        }
    }
}
