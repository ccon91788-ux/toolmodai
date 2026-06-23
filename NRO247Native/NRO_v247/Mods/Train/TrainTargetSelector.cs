using System;
using System.Collections.Generic;
using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods
{
    internal static class TrainTargetSelector
    {
        private const int TileSize = 24;

        public static Mob SelectTarget(TrainRuntimeSettings settings, Char me)
        {
            if (settings == null || me == null || GameScr.vMob == null)
                return null;

            bool usingTdlt = TdltController.HasBuff();

            return usingTdlt
                ? SelectByManhattan(settings, me)
                : SelectByAstar(settings, me);
        }

        private static Mob SelectByManhattan(TrainRuntimeSettings settings, Char me)
        {
            Mob result = null;
            int bestDist = int.MaxValue;
            Mob fallback = null;
            int bestTime = int.MaxValue;

            for (int i = 0; i < GameScr.vMob.size(); i++)
            {
                Mob mob = (Mob)GameScr.vMob.elementAt(i);
                if (!settings.IsMobAllowed(mob)) continue;

                int dist = Math.Abs(me.cx - mob.xFirst) + Math.Abs(me.cy - mob.yFirst);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    result = mob;
                }

                if (mob.timeStatus < bestTime)
                {
                    bestTime = mob.timeStatus;
                    fallback = mob;
                }
            }

            return result ?? fallback;
        }

        private static Mob SelectByAstar(TrainRuntimeSettings settings, Char me)
        {
            var startTile = new Astar.Point(me.cx / TileSize, me.cy / TileSize);

            Mob bestMob = null;
            int bestPathLen = int.MaxValue;
            Mob fallback = null;
            int bestTime = int.MaxValue;

            for (int i = 0; i < GameScr.vMob.size(); i++)
            {
                Mob mob = (Mob)GameScr.vMob.elementAt(i);
                if (!settings.IsMobAllowed(mob)) continue;

                var goalTile = new Astar.Point(mob.xFirst / TileSize, mob.yFirst / TileSize);
                List<Astar.Point> path = Astar.FindPath(startTile, goalTile);

                if (path != null && path.Count < bestPathLen)
                {
                    bestPathLen = path.Count;
                    bestMob = mob;
                }

                if (mob.timeStatus < bestTime)
                {
                    bestTime = mob.timeStatus;
                    fallback = mob;
                }
            }

            return bestMob ?? fallback;
        }
    }
}