using System;

namespace NRO_v247.Mods.Utils
{
    public static class MobHelper
    {
        /// <summary>
        /// Kiểm tra xem quái có còn sống hay không (HP > 0 và trạng thái không phải là đã chết)
        /// </summary>
        public static bool IsMobAlive(Mob mob)
        {
            if (mob == null) return false;
            return mob.status != 0 && mob.status != 1 && mob.hp > 0;
        }

        /// <summary>
        /// Kiểm tra xem quái có hợp lệ để đánh hay không (còn sống, hiển thị trên màn hình, không bị đóng băng nếu cần thiết)
        /// </summary>
        public static bool IsMobValidToAttack(Mob mob, bool allowFrozen = true)
        {
            if (!IsMobAlive(mob))
                return false;

            if (mob.isHide) 
                return false;

            if (!allowFrozen && mob.isFreez)
                return false;

            return true;
        }

        /// <summary>
        /// Lấy quái sống gần nhất so với tọa độ của đối tượng đang xét
        /// </summary>
        public static Mob GetNearestAliveMob(int x, int y, int maxDistance = 600)
        {
            if (GameScr.vMob == null) return null;

            Mob nearest = null;
            int minDistance = maxDistance;

            for (int i = 0; i < GameScr.vMob.size(); i++)
            {
                Mob mob = (Mob)GameScr.vMob.elementAt(i);
                if (IsMobAlive(mob))
                {
                    int dist = Res.distance(x, y, mob.x, mob.y);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        nearest = mob;
                    }
                }
            }

            return nearest;
        }
    }
}
