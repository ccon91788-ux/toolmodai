using System.Collections.Generic;

namespace NRO_v247.Mods.Boss
{
    /// <summary>
    /// Helper: Dò tìm Boss trong vCharInMap hiện tại (Khu đang đứng)
    /// Trong NRO, siêu Boss đóng vai trò như Char chứ không phải Mob thường
    /// </summary>
    internal static class BossScoutingHelper
    {
        /// <summary>
        /// Quét vCharInMap tìm mob có tên khớp với danh sách Boss cấu hình.
        /// Trả về Char đầu tiên tìm thấy, hoặc null nếu không có.
        /// </summary>
        public static Char FindBossInCurrentZone(List<string> bossNames)
        {
            if (GameScr.vCharInMap == null || bossNames == null || bossNames.Count == 0)
                return null;

            List<Char> validBosses = new List<Char>();

            for (int i = 0; i < GameScr.vCharInMap.size(); i++)
            {
                Char c = (Char)GameScr.vCharInMap.elementAt(i);
                
                // Filter bỏ những Char rác (Pet, NPC, đệ tử...)
                if (c == null || string.IsNullOrEmpty(c.cName)) continue;
                if (c.isPet || c.isMiniPet) continue;
                if (c.cName == "Trọng tài" || c.cName.StartsWith("#") || c.cName.StartsWith("$")) continue;
                
                // Lọc cái xác, boss đã chết theo cờ hệ thống
                if (c.cHP <= 0 || c.isDie) continue;
                
                // Tránh lỗi ném rác boss ngoại cỡ hoặc tọa độ ma (0,0)
                if (c.cx <= 0 || c.cy <= 0 || c.cx >= TileMap.pxw - 10 || c.cy >= TileMap.pxh - 10) continue;

                string rawName = ResolveMobName(c);

                // Guard: Boss luôn viết hoa chữ đầu — player thường không viết hoa → tránh nhầm
                if (string.IsNullOrEmpty(rawName) || !char.IsUpper(rawName[0])) continue;

                string nameLower = rawName.ToLower();
                bool isTargetBoss = false;
                foreach (var bossName in bossNames)
                {
                    if (string.IsNullOrEmpty(bossName)) continue;
                    string bn = bossName.Trim().ToLower();
                    
                    if (bn == "tdstnm")
                    {
                        if (nameLower.Contains("số 4") || nameLower.Contains("số 3") ||
                            nameLower.Contains("số 1") || nameLower.Contains("số 2") ||
                            nameLower.Contains("tiểu đội trưởng"))
                        {
                            isTargetBoss = true;
                            break;
                        }
                    }
                    
                    if (nameLower.Contains(bn))
                    {
                        isTargetBoss = true;
                        break;
                    }
                }
                
                if (!isTargetBoss) continue;

                validBosses.Add(c);
            }

            if (validBosses.Count == 0) return null;

            List<Char> pkBosses = new List<Char>();
            foreach (var b in validBosses)
            {
                if (b.cTypePk != 0) pkBosses.Add(b);
            }

            Char me = Char.myCharz();
            Char currentTarget = me?.charFocus;
            int targetId = currentTarget != null ? currentTarget.charID : -9999;

            System.Random rand = new System.Random();
            if (pkBosses.Count > 0)
            {
                Char highestGinyu = GetHighestPriorityGinyu(pkBosses);
                if (highestGinyu != null) return highestGinyu;

                foreach (var b in pkBosses)
                {
                    if (b.charID == targetId) return b;
                }
                return pkBosses[rand.Next(pkBosses.Count)];
            }
            else
            {
                Char highestGinyu = GetHighestPriorityGinyu(validBosses);
                if (highestGinyu != null) return highestGinyu;

                foreach (var b in validBosses)
                {
                    if (b.charID == targetId) return b;
                }
                return validBosses[rand.Next(validBosses.Count)];
            }
        }

        private static Char GetHighestPriorityGinyu(List<Char> bosses)
        {
            Char bestBoss = null;
            int maxPrio = 0;
            foreach (var b in bosses)
            {
                string name = ResolveMobName(b).ToLower();
                int prio = 0;
                if (name.Contains("số 4")) prio = 5;
                else if (name.Contains("số 3")) prio = 4;
                else if (name.Contains("số 1")) prio = 3;
                else if (name.Contains("số 2")) prio = 2;
                else if (name.Contains("tiểu đội trưởng")) prio = 1;

                if (prio > maxPrio)
                {
                    maxPrio = prio;
                    bestBoss = b;
                }
            }
            return bestBoss;
        }

        public static string ResolveMobName(Char mob)
        {
            if (mob == null) return "";
            if (!string.IsNullOrEmpty(mob.cName)) return mob.cName;
            return "";
        }
    }
}
