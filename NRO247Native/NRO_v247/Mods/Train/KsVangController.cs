using System;
using System.Collections.Generic;

namespace NRO_v247.Mods
{
    public static class KsVangController
    {
        private static long _zoneJoinedAtMs = 0L;
        private static int _lastMapId = -1;
        private static int _lastZoneId = -1;

        /// <summary>
        /// Gọi mỗi frame (hoặc đều đặn) từ AutoTrainFeature để kiểm tra điều kiện đổi khu theo thời gian.
        /// </summary>
        public static void OnUpdate(AutoTrainFeature trainFeature)
        {
            if (!trainFeature.IsPureTrainActive() || !trainFeature.OptimizeKsVang)
                return;

            long now = mSystem.currentTimeMillis();

            // Nhận diện vừa chuyển map/khu để reset Timer
            if (_lastMapId != TileMap.mapID || _lastZoneId != TileMap.zoneID)
            {
                _lastMapId = TileMap.mapID;
                _lastZoneId = TileMap.zoneID;
                _zoneJoinedAtMs = now;
            }

            // KS Vàng có chế độ nhảy khu theo thời gian
            if (trainFeature.KsVangAutoZoneTrigger == 1 && trainFeature.KsVangAutoZoneTimeMin > 0)
            {
                long elapsedMinutes = (now - _zoneJoinedAtMs) / 60000L;
                if (elapsedMinutes >= trainFeature.KsVangAutoZoneTimeMin && !Char.ischangingMap && !Controller.isStopReadMessage)
                {
                    _zoneJoinedAtMs = now; // Chống spam khi chưa nhận được zone
                    trainFeature.TriggerZoneReset();
                    return;
                }
            }

            // KS Vàng Né Char (Blacklist) ngay tại Map hiện tại
            if (trainFeature.KsVangAvoidChars && !string.IsNullOrWhiteSpace(trainFeature.KsVangAvoidCharsList) && GameScr.vCharInMap != null)
            {
                // Chỉ check thả cửa nếu đã đứng ở Map được 1.5s để chống việc nhận diện ẩu lúc mới đổi khu
                if (now - _zoneJoinedAtMs > 1500 && !Char.ischangingMap && !Controller.isStopReadMessage)
                {
                    string[] blacklist = trainFeature.KsVangAvoidCharsList.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
                    bool foundBlacklist = false;

                    for (int i = 0; i < GameScr.vCharInMap.size(); i++)
                    {
                        Char c = (Char)GameScr.vCharInMap.elementAt(i);
                        if (c != null && c != Char.myCharz() && !string.IsNullOrEmpty(c.cNameClear))
                        {
                            string lowerName = c.cNameClear.ToLower();
                            for (int j = 0; j < blacklist.Length; j++)
                            {
                                if (lowerName.Equals(blacklist[j].Trim().ToLower()))
                                {
                                    foundBlacklist = true;
                                    break;
                                }
                            }
                        }
                        if (foundBlacklist) break;
                    }

                    if (foundBlacklist)
                    {
                        _zoneJoinedAtMs = now; // Update time để cho nhịp nghỉ cooldown nhảy khu
                        trainFeature.TriggerZoneReset();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Xử lý thuật toán chọn Khu cho KS Vàng. Trả về True nếu đã nhận lệnh đổi khu, 
        /// False nếu bỏ qua để rớt xuống logic cơ bản (chẳng hạn khi KS Vàng tắt).
        /// </summary>
        public static bool TryHandleAdvanceZoneChange(AutoTrainFeature trainFeature, Message msg, int[] zones, int[] numPlayer)
        {
            if (!trainFeature.IsPureTrainActive() || !trainFeature.OptimizeKsVang)
                return false; // Rớt xuống logic cũ của game/train

            int bestZone = -1;
            int conditionValue = trainFeature.KsVangAutoZoneMode == 1 ? -1 : int.MaxValue; // Mode 1 = Đông nhất, Mode 0 = Ít nhất
            
            var avoidNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (trainFeature.KsVangAvoidChars && !string.IsNullOrWhiteSpace(trainFeature.KsVangAvoidCharsList))
            {
                string[] parts = trainFeature.KsVangAvoidCharsList.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts) avoidNames.Add(p.Trim().ToLower());
            }

            var validZones = new List<int>();

            for (int i = 0; i < zones.Length; i++)
            {
                int zoneId = zones[i];
                int pCount = numPlayer[i];

                // Bỏ qua khu 0 (thường có trụ cột/tân thủ/lag) và khu đang đứng
                if (zoneId == 0 || zoneId == TileMap.zoneID) 
                    continue;

                // Nếu Server thông báo khu đầy (thường là 15, nhưng đôi khi set 16 báo màu cam) -> Không vào
                if (pCount >= 15)
                    continue;

                // Giới hạn Min-Max
                if (trainFeature.KsVangFilterPlayer)
                {
                    if (pCount < trainFeature.KsVangPlayerMin || pCount > trainFeature.KsVangPlayerMax)
                        continue;
                }

                // Chặn Blacklist: Kiểm tra xem có người chơi nào thuộc blacklist không
                // Đáng tiếc là packet UI_ZONE không có name người trong zone.
                // Do đó, Blacklist name tại thời điểm này không check được từ xa, 
                // chỉ dựa vào List Char đã quét lúc trước. Tuy nhiên, nếu vừa có char trong map:
                // Ta chỉ né những Zone nếu ta đang có thông tin. Logic Tạm: Không làm gắt bước này ở ZoneList.

                validZones.Add(zoneId);

                if (trainFeature.KsVangAutoZoneMode == 1)
                {
                    // Ưu tiên Đông nhất trong số hợp lệ
                    if (pCount > conditionValue)
                    {
                        conditionValue = pCount;
                        bestZone = zoneId;
                    }
                }
                else
                {
                    // Ưu tiên Ít nhất trong số hợp lệ
                    if (pCount < conditionValue)
                    {
                        conditionValue = pCount;
                        bestZone = zoneId;
                    }
                }
            }

            // Fallback (Sinh Tồn): Nếu list Valid trống, bắt buộc phải nhảy random để giữ Auto chạy (Tránh kẹt mãi ở khu hiện tại)
            if (bestZone == -1)
            {
                if (validZones.Count > 0)
                {
                    bestZone = validZones[Res.random(validZones.Count)];
                }
                else
                {
                    // Ngẫu nhiên hoàn toàn (trừ khu 0 và khu đang đứng)
                    var emergencyZones = new List<int>();
                    for (int i = 0; i < zones.Length; i++)
                    {
                        if (zones[i] != 0 && zones[i] != TileMap.zoneID && numPlayer[i] < 15) 
                            emergencyZones.Add(zones[i]);
                    }
                    
                    if (emergencyZones.Count > 0)
                        bestZone = emergencyZones[Res.random(emergencyZones.Count)];
                }
            }

            if (bestZone != -1)
            {
                Service.gI().requestChangeZone(bestZone, -1);
                return true; // Đã xử lý xong
            }

            return true; // Dù không tìm được thì vẫn 'ăn' luồng này, ép đứng chờ chứ KHÔNG rớt xuống code Tàn sát cũ.
        }

        /// <summary>
        /// Xử lý logic Turbo KS (Đánh 0ms) cho quái mới focus.
        /// </summary>
        public static void ApplyFirstHitOptimization(AutoTrainFeature trainFeature, Char me, Mob target, ref long lastAttackAtMs)
        {
            if (!trainFeature.OptimizeKsVang || !trainFeature.IsPureTrainActive())
                return;

            if (me.mobFocus == null || me.mobFocus.mobId != target.mobId)
            {
                lastAttackAtMs = 0;
            }
        }
    }
}
