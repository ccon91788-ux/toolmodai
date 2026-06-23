using System;
using NRO_v247.Mods;

namespace NRO_v247.Mods.Utils
{
    public static class NavigationController
    {
        private static long _lastZoneRequestTime = 0;
        private static long _lastTeleportTime = 0;
        private static int _lastObservedMapId = -1;
        private static long _lastMapChangedAtMs = 0;
        private const long MapSettleBeforeZoneChangeMs = 1500;

        /// <summary>
        /// Thử đi tới Map. Trả về true nếu đã ở đúng MapId.
        /// Quá trình đi tới map được giao cho XmapService.
        /// </summary>
        public static bool TryGoToMap(int targetMapId)
        {
            if (TileMap.mapID == targetMapId)
                return true; // Đã tới

            var xmapSvc = ServiceLocator.Get<IXmapService>();
            bool isXmapping = xmapSvc?.IsXmaping() ?? false;

            if (!isXmapping)
            {
                xmapSvc?.StartGoToMapFromPanel(targetMapId);
            }

            return false; // Đang trên đường tới
        }

        /// <summary>
        /// Thử đổi sang khu vực (Zone) chỉ định. Trả về true nếu đã ở đúng ZoneId.
        /// Tự động áp dụng Delay tùy theo việc có buff TDLT hay không (5.5s vs 10s).
        /// </summary>
        public static bool TryChangeZone(int targetZoneId)
        {
            if (targetZoneId < 0)
            {
                return true; // Không cần hoặc không thể đổi khu
            }

            if (TileMap.zoneID == targetZoneId)
            {
                return true; // Đã ở đúng khu
            }

            long now = mSystem.currentTimeMillis();
            
            // Nếu có TDLT thì thời gian cooldown an toàn để đổi khu là 5.5 giây, nếu không là 10.5 giây
            long zoneCooldown = TdltController.HasBuff() ? 5500 : 10500;

            if (now - _lastZoneRequestTime >= zoneCooldown)
            {
                Service.gI().requestChangeZone(targetZoneId, -1);
                _lastZoneRequestTime = now;
            }

            return false; // Đang chờ lệnh đổi khu có hiệu lực
        }

        /// <summary>
        /// Thử di chuyển hoặc dịch chuyển tới tọa độ chỉ định. Trả về true nếu khoảng cách đủ gần (trong sai số tolerance).
        /// </summary>
        public static bool TryMoveTo(Char me, int targetX, int targetY, int tolerance = 20, bool directTeleport = false, long cooldownMs = 500)
        {
            if (me == null || targetX < 0 || targetY < 0) return true;

            int dx = Math.Abs(me.cx - targetX);
            int dy = Math.Abs(me.cy - targetY);

            if (dx <= tolerance && dy <= tolerance)
            {
                return true; // Đủ gần
            }

            long now = mSystem.currentTimeMillis();
            if (now - _lastTeleportTime >= cooldownMs)
            {
                // Áp dụng thủ thuật 3 Packets của bản Java:
                // Tránh việc server check chạm hố cản địa hình hoặc chặn khoảng cách.
                
                me.cxSend = 0; me.cySend = 0;
                me.cx = targetX; me.cy = targetY;
                Service.gI().charMove();

                // Gửi thêm packet cắm đất (y + 1) để lừa check ground của server
                me.cxSend = 0; me.cySend = 0;
                me.cx = targetX; me.cy = targetY + 1;
                Service.gI().charMove();

                // Gửi lại toạ độ đúng
                me.cxSend = 0; me.cySend = 0;
                me.cx = targetX; me.cy = targetY;
                Service.gI().charMove();

                _lastTeleportTime = now;
            }

            return false; // Đang trên đường đi / chờ cd di chuyển
        }

        /// <summary>
        /// Xử lý chuỗi trở về (Goback) liên hoàn: Map -> Zone -> Tọa độ.
        /// Nên được gọi liên tục mỗi frame. Nó sẽ trả về TRUE khi nhân vật đã tới nơi một cách hoàn toàn.
        /// </summary>
        public static bool ProcessDumbGoback(Char me, 
            int targetMapId, 
            bool requireZone, int targetZoneId, 
            bool requirePosition, int targetX, int targetY,
            int posTolerance = 20, bool directTeleport = false,
            bool useTdltDuringGoback = true)
        {
            if (me == null) return false;
            long now = mSystem.currentTimeMillis();

            // Theo dõi thời điểm map vừa đổi để tránh request đổi khu quá sớm.
            if (_lastObservedMapId != TileMap.mapID)
            {
                _lastObservedMapId = TileMap.mapID;
                _lastMapChangedAtMs = now;
            }

            // Mặc định bật ké TDLT trong chuỗi goback để tele an toàn hơn.
            if (useTdltDuringGoback)
            {
                TdltController.Update(true);
                // Giữ thêm một nhịp ngắn để chống rớt TDLT do hụt frame khi vừa đổi map/khu.
                TdltController.HoldActiveFor(2500);
            }

            // 1. Kiểm tra Map
            if (targetMapId > 0 && TileMap.mapID != targetMapId)
            {
                // Thêm delay 2 giây khi ở map nhà (sau khi chết hoặc vừa login) rồi mới ra map làng (0, 7, 14)
                if (MapHelper.IsHomeMap(TileMap.mapID))
                {
                    if (now - _lastMapChangedAtMs < 2500)
                    {
                        return false; // Chờ map ổn định / delay theo yêu cầu
                    }
                }

                if (!TryGoToMap(targetMapId))
                {
                    return false; // Blocking
                }
            }

            // 2. Kiểm tra Zone 
            // Cần ở đúng Map thì mới bắt đầu xử lý Zone, tránh request sai Map dẫn đến kẹt
            if (requireZone && targetMapId > 0 && TileMap.mapID == targetMapId)
            {
                if (now - _lastMapChangedAtMs < MapSettleBeforeZoneChangeMs)
                {
                    return false; // Chờ map ổn định 1.5s rồi mới đổi khu
                }

                if (!TryChangeZone(targetZoneId))
                {
                    return false; // Blocking
                }
            }

            // 3. Kiểm tra Tọa độ
            // Cần ở đúng Map (và đúng Zone nếu có cấu hình) thì mới bắt đầu Move
            bool isInTargetMap = targetMapId <= 0 || TileMap.mapID == targetMapId;
            bool isInTargetZone = !requireZone || TileMap.zoneID == targetZoneId;

            if (requirePosition && isInTargetMap && isInTargetZone)
            {
                if (!TryMoveTo(me, targetX, targetY, posTolerance, directTeleport))
                {
                    return false; // Blocking
                }
            }

            // Đã vượt qua mọi trạm kiềm duyệt rào cản -> đã tới đích!
            return true;
        }
    }
}
