using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods.Xmap
{
    // Helper teleport/game utils - giữ lại từ CompatUtils cũ
    public static class PathUtils
    {
        public static void teleportTo(int tx, int ty)
        {
            Char me = Char.myCharz();
            me.cxSend = 0;
            me.cySend = 0;
            me.cx = tx;
            me.cy = ty;
            Service.gI().charMove();

            if (!TdltController.HasBuff())
            {
                me.cy = ty + 1;
                Service.gI().charMove();
                me.cy = ty;
                Service.gI().charMove();
            }
        }
    }

    public static class GameUtils
    {
        public static void teleportToNpc(int npcId)
        {
            Npc npc = GameScr.findNPCInMap((short)npcId);
            if (npc == null) return;

            Char.myCharz().npcFocus = npc;
            PathUtils.teleportTo(npc.cx, npc.cy - 3);
        }
    }
}
