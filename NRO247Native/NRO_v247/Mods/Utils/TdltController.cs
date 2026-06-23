using System;

namespace NRO_v247.Mods.Utils
{
    public static class TdltController
    {
        public const int ItemId = 521;
        public const int BuffIconId = 4387;

        private static long _lastUseTdltAtMs = 0L;
        private const long UseTdltCooldownMs = 1500;
        private static bool _isRequestedThisFrame = false;
        private static long _holdActiveUntilMs = 0L;

        public static bool HasBuff()
        {
            return ItemTime.isExistItem(BuffIconId);
        }

        public static int GetRemainingTimeMinutes()
        {
            var buff = ItemTime.getItemById(BuffIconId);
            return buff != null ? buff.minute : 0;
        }


        /// <summary>
        /// Gọi để báo hiệu frame này cần giữ kết nối TDLT
        /// </summary>
        public static void Update(bool keepActive)
        {
            if (keepActive)
            {
                _isRequestedThisFrame = true;
            }
        }

        /// <summary>
        /// Giữ TDLT ở trạng thái active trong một khoảng thời gian ngắn
        /// để tránh rớt 1-2 frame khi chuyển trạng thái map/screen.
        /// </summary>
        public static void HoldActiveFor(long durationMs)
        {
            if (durationMs <= 0) return;
            long now = mSystem.currentTimeMillis();
            long holdUntil = now + durationMs;
            if (holdUntil > _holdActiveUntilMs)
            {
                _holdActiveUntilMs = holdUntil;
            }
        }

        /// <summary>
        /// Được ModManager gọi ở cuối mỗi frame
        /// Nhằm tự động bật/tắt tuỳ theo số lượng request trong frame đó
        /// </summary>
        public static void EndFrameUpdate()
        {
            long now = mSystem.currentTimeMillis();
            bool shouldKeepActive = _isRequestedThisFrame || now < _holdActiveUntilMs;

            if (shouldKeepActive)
            {
                TryUse();
            }
            else
            {
                TryDisable();
            }
            _isRequestedThisFrame = false;
        }

        private static void TryUse()
        {
            long now = mSystem.currentTimeMillis();
            if (now - _lastUseTdltAtMs < UseTdltCooldownMs) return;
            if (HasBuff()) { _lastUseTdltAtMs = now; return; }

            Char me = Char.myCharz();
            if (me?.arrItemBag == null) return;

            for (int i = 0; i < me.arrItemBag.Length; i++)
            {
                Item item = me.arrItemBag[i];
                if (item?.template == null || item.quantity <= 0) continue;
                if (item.template.id == ItemId)
                {
                    Service.gI().useItem(0, 1, (sbyte)i, -1);
                    _lastUseTdltAtMs = now;
                    return;
                }
            }
        }

        private static void TryDisable()
        {
            if (!HasBuff()) return;
            Char me = Char.myCharz();
            if (me?.arrItemBag == null) return;

            long now = mSystem.currentTimeMillis();
            if (now - _lastUseTdltAtMs < UseTdltCooldownMs) return;

            for (int i = 0; i < me.arrItemBag.Length; i++)
            {
                Item item = me.arrItemBag[i];
                if (item?.template == null || item.quantity <= 0) continue;
                if (item.template.id == ItemId)
                {
                    Service.gI().useItem(0, 1, (sbyte)i, -1);
                    _lastUseTdltAtMs = now;
                    return;
                }
            }
        }
    }
}
