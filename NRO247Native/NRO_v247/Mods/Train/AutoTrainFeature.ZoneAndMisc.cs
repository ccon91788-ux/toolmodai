using System;
using System.Collections.Generic;

namespace NRO_v247.Mods
{
    public partial class AutoTrainFeature
    {
        // ──────────────────────────────────────────────────────────────────
        // Đổi khu (Auto Zone)
        // ──────────────────────────────────────────────────────────────────
        public static void TryAutoSwitchZone()
        {
            try
            {
                var zones = GameScr.gI().zones;
                var numPlayer = GameScr.gI().numPlayer;
                if (zones == null || numPlayer == null) return;

                if (ModBootstrap.TrainFeature != null)
                {
                    if (KsVangController.TryHandleAdvanceZoneChange(ModBootstrap.TrainFeature, null, zones, numPlayer))
                        return;
                }

                int currentZone = TileMap.zoneID;

                // Tìm số lượng người ít nhất ở CÁC KHU KHÁC (loại trừ khu hiện tại)
                int minCountOther = int.MaxValue;
                for (int i = 0; i < zones.Length; i++)
                {
                    if (zones[i] == -1 || zones[i] == currentZone) continue;
                    int cnt = numPlayer[i];
                    if (cnt < minCountOther) minCountOther = cnt;
                }

                var candidates = new List<int>();
                for (int i = 0; i < zones.Length; i++)
                {
                    if (zones[i] == -1 || zones[i] == currentZone) continue;
                    // Lấy các khu có số người ngang bằng mức thấp nhất (hoặc chênh lệch 1 người để đảo đa dạng)
                    if (numPlayer[i] <= minCountOther + 1) candidates.Add(zones[i]);
                }

                if (candidates.Count == 0) 
                {
                    // Trường hợp game chỉ có 1 khu duy nhất (Ví dụ: vGo/Doanh trại) => F5 luôn khu hiện tại
                    Service.gI().requestChangeZone(-1, -1);
                    return;
                }

                int targetZone = candidates[_zoneRandom.Next(candidates.Count)];
                Service.gI().requestChangeZone(targetZone, -1);
            }
            catch { }
        }

        public void TriggerZoneReset()
        {
            try
            {
                if (RequireZone && ZoneId >= 0)
                {
                    // Nếu đang khóa khu: chọn đại một khu khác (random) để "thoát ra"
                    // Chờ nhịp sau EnsureMapAndZone sẽ tự đưa về khu cũ
                    int currentZone = TileMap.zoneID;
                    int maxZone = GameScr.gI().zones.Length;
                    if (maxZone > 1)
                    {
                        int randomZone;
                        do
                        {
                            randomZone = _zoneRandom.Next(maxZone);
                        } while (randomZone == currentZone || randomZone == ZoneId);

                        Service.gI().requestChangeZone(randomZone, -1);
                        GameScr.info1?.addInfo($"Reset khu: {currentZone} -> {randomZone} (đợi quay lại {ZoneId})", 0);
                    }
                    else
                    {
                        // Chỉ có 1 khu -> F5 khu đó
                        Service.gI().requestChangeZone(-1, -1);
                        GameScr.info1?.addInfo("Reset khu: F5 khu hiện tại", 0);
                    }
                }
                else
                {
                    // Không khóa khu: đổi sang khu random bất kỳ (khác khu hiện tại)
                    var zones = GameScr.gI()?.zones;
                    if (zones == null || zones.Length == 0) return;
                    int currentZone = TileMap.zoneID;
                    var candidates = new System.Collections.Generic.List<int>();
                    for (int i = 0; i < zones.Length; i++)
                    {
                        if (zones[i] != -1 && zones[i] != currentZone)
                            candidates.Add(zones[i]);
                    }
                    if (candidates.Count == 0)
                    {
                        // Chỉ có 1 khu -> F5 reload khu đó
                        Service.gI().requestChangeZone(-1, -1);
                    }
                    else
                    {
                        int targetZone = candidates[_zoneRandom.Next(candidates.Count)];
                        Service.gI().requestChangeZone(targetZone, -1);
                    }
                }
            }
            catch { }
        }

        // ──────────────────────────────────────────────────────────────────
        // Giáp luyện tập
        // ──────────────────────────────────────────────────────────────────
        private void TryHandleTrainingArmor(Char me)
        {
            if (TrainingArmorMode == 0 || me == null) return;
            if (me.meDead || me.cHP <= 0 || me.statusMe == 14) return;
            if (Char.ischangingMap || Controller.isStopReadMessage) return;

            long now = mSystem.currentTimeMillis();
            if (now - _lastTrainingArmorActionAtMs < TrainingArmorCooldownMs) return;

            if (TrainingArmorMode == 1)
            {
                if (GetTrainingArmorBodyIndex(me) >= 0) return;

                int bagIndex = GetTrainingArmorBagIndex(me);
                if (bagIndex < 0) return;

                Service.gI().getItem(BagBodyType, (sbyte)bagIndex);
                _lastTrainingArmorActionAtMs = now;
                return;
            }

            if (TrainingArmorMode == 2)
            {
                int bodyIndex = GetTrainingArmorBodyIndex(me);
                if (bodyIndex < 0) return;

                Service.gI().getItem(BodyBagType, (sbyte)bodyIndex);
                _lastTrainingArmorActionAtMs = now;
            }
        }

        private static int GetTrainingArmorBagIndex(Char me)
        {
            if (me?.arrItemBag == null) return -1;

            for (int i = 0; i < me.arrItemBag.Length; i++)
            {
                Item item = me.arrItemBag[i];
                if (item?.template == null || item.quantity <= 0) continue;
                if (item.template.type == Item.TYPE_TRAINSUIT) return i;
            }

            return -1;
        }

        private static int GetTrainingArmorBodyIndex(Char me)
        {
            if (me?.arrItemBody == null) return -1;

            if (me.arrItemBody.Length > Item.TYPE_TRAINSUIT)
            {
                Item slotItem = me.arrItemBody[Item.TYPE_TRAINSUIT];
                if (slotItem?.template != null && slotItem.template.type == Item.TYPE_TRAINSUIT)
                    return Item.TYPE_TRAINSUIT;
            }

            for (int i = 0; i < me.arrItemBody.Length; i++)
            {
                Item item = me.arrItemBody[i];
                if (item?.template != null && item.template.type == Item.TYPE_TRAINSUIT)
                    return i;
            }

            return -1;
        }

        // ──────────────────────────────────────────────────────────────────
        // Đậu / TDLT / An toàn
        // ──────────────────────────────────────────────────────────────────
        private void UseGrape(Char me)
        {
            if (me?.arrItemBag == null) return;
            for (int i = 0; i < me.arrItemBag.Length; i++)
            {
                Item item = me.arrItemBag[i];
                if (item?.template != null && (item.template.id == 211 || item.template.id == 212))
                {
                    Service.gI().useItem(0, 1, (sbyte)i, -1);
                    return;
                }
            }
        }


        private static bool IsSafeToMove()
        {
            Char me = Char.myCharz();
            if (me == null) return false;

            int status = me.statusMe;
            if (status == 2 || status == 3 || status == 4 || status == 16) return false;
            if (me.isLockMove || me.isWaitMonkey || me.meDead) return false;

            return true;
        }

        private static bool IsCombatGuardBlocked(Char me)
        {
            if (me == null) return true;
            if (me.meDead || me.cHP <= 0 || me.statusMe == 14 || me.statusMe == 5) return true;
            if (me.isWaitMonkey || me.isCharge || me.isFlyAndCharge || me.isUseChargeSkill()) return true;
            return GameScr.gI().isCharging();
        }


        internal static void HandleOnDeath(Char me)
        {
            switch (ModBootstrap.ActionOnDeath)
            {
                case 1:
                    // Hồi sinh Ngọc (nếu hết ngọc thì về nhà)
                    if (me != null && (me.luong + me.luongKhoa) > 0)
                        Service.gI().wakeUpFromDead();
                    else
                        Service.gI().returnTownFromDead();
                    break;
                case 2:
                    // Chờ – không làm gì
                    break;
                default:
                    // 0 hoặc không xác định: Về nhà
                    Service.gI().returnTownFromDead();
                    break;
            }
        }
    }
}
