using System;
using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods.Boss
{
    /// <summary>
    /// Quản lý việc dùng Item trong phiên Săn Boss.
    /// - Scouting: chỉ cắn TDLT
    /// - Hunting/Tying: cắn tất cả item người dùng đã tick
    /// </summary>
    internal static class BossItemHelper
    {
        // ─── Item ID + Icon Buff ID ───────────────────────────────────────────
        // (Đồng bộ với AutoItemFeature.cs và AutoTrainFeature.cs)
        private const int ID_CUONG_NO  = 381;  private const int ICON_CUONG_NO  = 2754;
        private const int ID_BO_HUYET  = 382;  private const int ICON_BO_HUYET  = 2755;
        private const int ID_GIAP_XEN  = 384;  private const int ICON_GIAP_XEN  = 2757;
        private const int ID_AN_DANH   = 764;  private const int ICON_AN_DANH   = 7149;
        private const int ID_CO_4_LA   = 1635; private const int ICON_CO_4_LA   = 13618;

        private static readonly short[] FoodItemIds = new short[] { 665, 666, 664, 667, 663 };
        private static readonly int[]   FoodIconIds  = new int[]   { 6326, 6327, 6325, 6328, 6324 };

        private static long _lastUseMs = 0;
        private const long UseIntervalMs = 1000; // Kiểm tra mỗi 1 giây

        /// <summary>
        /// Gọi từ DoScouting(): Chỉ dùng TDLT để dò boss nhanh hơn.
        /// </summary>
        public static void TickForScouting(bool useTdlt)
        {
            long now = mSystem.currentTimeMillis();
            if (now - _lastUseMs < UseIntervalMs) return;
            _lastUseMs = now;

            if (useTdlt) TdltController.Update(true);
        }

        /// <summary>
        /// Gọi từ DoHunting() và DoTieBoss(): Dùng tất cả item đã tick.
        /// </summary>
        public static void TickForHunting(
            bool useCuongNo, bool useBoHuyet, bool useGiapXen,
            bool useAnDanh, bool useCo4La, bool useFood, bool useTdlt)
        {
            long now = mSystem.currentTimeMillis();
            if (now - _lastUseMs < UseIntervalMs) return;
            _lastUseMs = now;

            if (useCuongNo)  TryUseBuffItem(ID_CUONG_NO, ICON_CUONG_NO);
            if (useBoHuyet)  TryUseBuffItem(ID_BO_HUYET, ICON_BO_HUYET);
            if (useGiapXen)  TryUseBuffItem(ID_GIAP_XEN, ICON_GIAP_XEN);
            if (useAnDanh)   TryUseBuffItem(ID_AN_DANH,  ICON_AN_DANH);
            if (useCo4La)    TryUseBuffItem(ID_CO_4_LA,  ICON_CO_4_LA);
            if (useTdlt)     TdltController.Update(true);
            if (useFood)     TryUseFood();
        }

        // ─── Internal helpers ─────────────────────────────────────────────────

        private static void TryUseBuffItem(int itemId, int idIconBuff)
        {
            // Buff còn hiệu lực -> không cắn
            if (ItemTime.getItemById(idIconBuff) != null) return;

            var bag = Char.myCharz()?.arrItemBag;
            if (bag == null) return;

            for (int i = 0; i < bag.Length; i++)
            {
                Item item = bag[i];
                if (item?.template != null && item.template.id == itemId)
                {
                    Service.gI().useItem(0, 1, (sbyte)i, -1);
                    return;
                }
            }
        }

        private static void TryUseFood()
        {
            // Kiểm tra có buff Đồ ăn nào đang hoạt động chưa
            for (int j = 0; j < FoodIconIds.Length; j++)
            {
                if (ItemTime.getItemById(FoodIconIds[j]) != null) return; // Đang có buff rồi
            }

            // Chưa có buff -> tìm slot đồ ăn đầu tiên trong túi
            var bag = Char.myCharz()?.arrItemBag;
            if (bag == null) return;

            var foodSet = new System.Collections.Generic.HashSet<short>(FoodItemIds);
            for (int i = 0; i < bag.Length; i++)
            {
                Item item = bag[i];
                if (item?.template != null && foodSet.Contains(item.template.id))
                {
                    Service.gI().useItem(0, 1, (sbyte)i, -1);
                    return;
                }
            }
        }
    }
}
