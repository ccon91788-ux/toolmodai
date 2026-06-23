using System;
using System.Collections.Generic;
using NRO_v247.Mods.Utils;
using NRO_v247.Mods.Notifications;
using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods.Items
{
    /// <summary>
    /// Auto nhặt item – port từ Java AutoPick.java.
    /// Không xử lý vứt đồ (đã có AutoItemFeature riêng).
    /// Tương tác AutoTrain qua IsPickingNow:
    ///   - Train check IsPickingNow, nếu true thì nhường cho pick.
    /// </summary>
    public class AutoPickFeature : IAutoFeature
    {
        public bool Enabled { get; private set; }
        public string FeatureName => "AutoPickFeature";

        public bool IsActive => (Enabled || IsUpZinOverride) && !string.IsNullOrEmpty(CurrentState);
        public string CurrentState { get; private set; } = "";
        public bool IsUtilityTask => true;

        /// <summary>Đang thực sự dịch chuyển để nhặt item ngoài tầm. Train nhường khi này.</summary>
        public bool IsPickingNow { get; private set; }

        // ── Settings từ Panel ──────────────────────────────────────────
        private bool _autoPick;
        private int _pickMode;           // 0 = nhặt tất cả, 1 = nhặt theo whitelist
        private bool _onlyMyItems;       // chỉ nhặt đồ mình + đồ không chủ
        private int _pickDistance = 50;  // tầm quét (pixels)

        private readonly HashSet<int> _whiteList = new HashSet<int>();
        private readonly HashSet<int> _blackList = new HashSet<int>();

        // ── Cooldown ───────────────────────────────────────────────────
        private const long PickIntervalMs = 550L;
        private long _lastTimePickedItem;
        private int _lastPickedItemMapId = -1;

        // ── Pick count ring buffer (anti-spam, tối đa 10 lần/item) ───
        private const int CountCap = 64;
        private const int MaxPickPerItem = 10;
        private readonly int[] _pickIds = new int[CountCap];
        private readonly byte[] _pickCounts = new byte[CountCap];
        private int _pickHead;

        public AutoPickFeature()
        {
            NotifyCatcher.OnNotifyReceived += HandleNotification;
        }

        private void HandleNotification(NotifyCatcher.NotifyEvent ev)
        {
            if (!Enabled) return;
            string msg = ev.Message;
            if (string.IsNullOrEmpty(msg)) return;

            // Bỏ qua item nếu server báo không thể nhặt
            if (msg.Contains("Không thể nhặt vật phẩm của người khác") || msg.Contains("vật phẩm của người khác"))
            {
                if (_lastPickedItemMapId != -1)
                {
                    BanItem(_lastPickedItemMapId);
                    _lastPickedItemMapId = -1;
                }
            }
        }


        // ══════════════════════════════════════════════════════════════
        // Public API – nhận settings từ Panel (command PICK_SETTING)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Nhận settings từ Panel.
        /// Format: PICK_SETTING|autoPick|pickMode|onlyMyItems|pickDistance|teleportToItem|whiteList|blackList
        /// </summary>
        public void ApplySettingsFromPanel(
            bool autoPick,
            int pickMode,
            bool onlyMyItems,
            int pickDistance,
            string whiteListRaw,
            string blackListRaw)
        {
            _autoPick = autoPick;
            _pickMode = pickMode;
            _onlyMyItems = onlyMyItems;
            _pickDistance = Math.Max(50, Math.Min(500, pickDistance));

            ParseIdList(whiteListRaw, _whiteList);
            ParseIdList(blackListRaw, _blackList);

            Enabled = _autoPick;
        }

        // ══════════════════════════════════════════════════════════════
        // Update – gọi mỗi frame từ AutoMod
        // ══════════════════════════════════════════════════════════════

        public void Update()
        {
            if (!Enabled && !IsUpZinOverride)
            {
                IsPickingNow = false;
                CurrentState = "";
                return;
            }

            Char myChar = Char.myCharz();
            if (myChar == null)
            {
                IsPickingNow = false;
                CurrentState = "";
                return;
            }

            var xmap = ServiceLocator.Get<IXmapService>();
            if (xmap != null && xmap.IsXmaping())
            {
                IsPickingNow = false;
                CurrentState = "";
                return;
            }

            // Ở map nhà → không nhặt (giống Java: homeMapId = 21 + gender)
            int homeMapId = 21 + myChar.cgender;
            if (TileMap.mapID == homeMapId)
            {
                IsPickingNow = false;
                CurrentState = "";
                return;
            }

            // Có bùa Tự Động Luyện Tập (TDLT) hút đồ thì không tự nhặt
            if (TdltController.HasBuff())
            {
                IsPickingNow = false;
                CurrentState = "";
                return;
            }

            // Túi đầy → không nhặt
            if (IsBagFull(myChar))
            {
                IsPickingNow = false;
                CurrentState = "";
                return;
            }

            // Cooldown giữa mỗi lần nhặt
            // Giữ IsPickingNow = true trong toàn bộ thời gian cooldown
            // để AutoTrain không nhảy vào giữa chừng gây giật lùi
            long now = mSystem.currentTimeMillis();
            if (now - _lastTimePickedItem < PickIntervalMs)
            {
                IsPickingNow = true; // vẫn đang "bận nhặt", train nhường
                return;
            }

            // Không có item trên map
            if (GameScr.vItemMap == null || GameScr.vItemMap.size() == 0)
            {
                IsPickingNow = false;
                CurrentState = "";
                return;
            }

            int myId = myChar.charID;
            int radius2 = _pickDistance * _pickDistance;
            bool foundItem = false;

            for (int i = GameScr.vItemMap.size() - 1; i >= 0; i--)
            {
                ItemMap itemMap = (ItemMap)GameScr.vItemMap.elementAt(i);
                if (itemMap == null || itemMap.template == null) continue;
                int templateId = itemMap.template.id;

                // Không nhặt item trong blacklist
                if (_blackList.Contains(templateId)) continue;

                if (IsNrdMap(TileMap.mapID))
                {
                    int dxNrd = Math.Abs(myChar.cx - itemMap.x);
                    if (dxNrd <= 60 && IsNrdItem(itemMap))
                    {
                        IsPickingNow = true;
                        CurrentState = $"Nhặt NRD: {itemMap.template.name}";
                        Service.gI().pickItem(itemMap.itemMapID);
                        _lastTimePickedItem = now;
                        return;
                    }
                    continue; // NRD map chỉ nhặt NRD item
                }

                // ── Kiểm tra filter ──
                if (!ShouldPickItem(templateId)) continue;

                // ── Chỉ nhặt đồ của mình ──
                if (_onlyMyItems)
                {
                    // Fix: Nhận diện cả đồ đệ tử (ID đệ tử theo source Server Java luôn bằng âm ID Sư Phụ: -myId)
                    bool isMyItem = itemMap.playerId == myId || itemMap.playerId == -myId || itemMap.playerId == -1;
                    if (!isMyItem) continue;
                }

                // ── Anti-spam: tối đa 10 lần nhặt/item ──
                if (GetPickCount(itemMap.itemMapID) >= MaxPickPerItem) continue;

                // ── Xử lý tầm quét: lấy mốc tọa độ Sư Phụ hoặc Đệ Tử ──
                int originX = myChar.cx;
                int originY = myChar.cy;
                int currentRadius2 = radius2;

                if (itemMap.playerId == -myId && ModBootstrap.AutoPetFeature != null && ModBootstrap.AutoPetFeature.IsActive)
                {
                    Char myPet = GameScr.findCharInMap(-myId);
                    if (myPet != null)
                    {
                        originX = myPet.cx;
                        originY = myPet.cy;
                        currentRadius2 = 250 * 250; // Bán kính đệ tử nới lên 250px
                    }
                }

                int dxOrigin = originX - itemMap.x;
                int dyOrigin = originY - itemMap.y;
                int distanceToOrigin2 = dxOrigin * dxOrigin + dyOrigin * dyOrigin;

                if (distanceToOrigin2 <= currentRadius2)
                {
                    foundItem = true;
                    
                    // Đo khoảng cách thực tế từ Sư Phụ => Item để xem có cần bay qua không
                    int masterDist = Res.distance(myChar.cx, myChar.cy, itemMap.x, itemMap.y);

                    // Di chuyển tới item (port Java logic)
                    if (masterDist > 50)
                    {
                        TeleportTo(itemMap.x, itemMap.y);
                    }
                    IsPickingNow = true;
                    CurrentState = $"Nhặt đồ: {itemMap.template.name}";
                    PickItem(itemMap, now);
                    return;
                }
            }

            if (!foundItem)
            {
                IsPickingNow = false;
                CurrentState = "";
            }
        }

        // ══════════════════════════════════════════════════════════════
        // OnPaint – Vẽ vòng tròn tầm nhặt
        // ══════════════════════════════════════════════════════════════
        public void OnPaint(mGraphics g)
        {
            if (!Enabled) return;

            Char myChar = Char.myCharz();
            if (myChar == null) return;

            int cx = myChar.cx;
            int cy = myChar.cy;

            // Draw a circle of radius _pickDistance around character
            int r = _pickDistance;
            g.setColor(16776960); // Màu vàng
            g.drawCircle(cx, cy, r);
        }

        // ══════════════════════════════════════════════════════════════
        // Private helpers
        // ══════════════════════════════════════════════════════════════

        // ── Override Mode dành cho UpZin ─────────────────────────────────
        public bool IsUpZinOverride = false;

        /// <summary>Quyết định có nên nhặt item này không (blackList → pickAll → pickByList).</summary>
        private bool ShouldPickItem(int templateId)
        {
            if (IsUpZinOverride)
            {
                if (templateId >= 828 && templateId <= 842) return false;
                if (templateId == 859 || templateId == 362) return false;
                if (templateId >= 353 && templateId <= 360) return false;
                return true; // UpZin nhặt mọi thứ còn lại (Radar, đùi gà, etc.)
            }

            if (_blackList.Contains(templateId))
                return false;

            if (_pickMode == 0) // nhặt tất cả
                return true;

            if (_pickMode == 1) // nhặt theo whitelist
                return _whiteList.Contains(templateId);

            return false;
        }

        /// <summary>Túi đầy → không nhặt.</summary>
        private static bool IsBagFull(Char myChar)
        {
            return ItemHelper.IsBagFull();
        }

        /// <summary>Gửi packet nhặt item + ghi pick count.</summary>
        private void PickItem(ItemMap itemMap, long now)
        {
            Service service = Service.gI();
            if (service != null)
            {
                service.pickItem(itemMap.itemMapID);
                IncrementPickCount(itemMap.itemMapID);
                _lastTimePickedItem = now;
                _lastPickedItemMapId = itemMap.itemMapID;
            }
        }

        /// <summary>Teleport nhân vật tới vị trí (x,y).</summary>
        private static void TeleportTo(int x, int y)
        {
            Char me = Char.myCharz();
            if (me == null) return;

            me.cx = x;
            me.cy = y;
            Service.gI().charMove();

            // Trick: move 1 pixel rồi quay lại để server chấp nhận
            me.cy = y + 1;
            Service.gI().charMove();
            me.cy = y;
            Service.gI().charMove();
        }

        // ── Pick count ring buffer ─────────────────────────────────────

        private int GetPickCount(int itemMapId)
        {
            for (int i = 0; i < CountCap; i++)
            {
                if (_pickIds[i] == itemMapId)
                    return _pickCounts[i] & 0xFF;
            }
            return 0;
        }

        private void IncrementPickCount(int itemMapId)
        {
            for (int i = 0; i < CountCap; i++)
            {
                if (_pickIds[i] == itemMapId)
                {
                    if (_pickCounts[i] < 127) _pickCounts[i]++;
                    return;
                }
            }

            _pickIds[_pickHead] = itemMapId;
            _pickCounts[_pickHead] = 1;
            _pickHead++;
            if (_pickHead >= CountCap) _pickHead = 0;
        }

        private void BanItem(int itemMapId)
        {
            for (int i = 0; i < CountCap; i++)
            {
                if (_pickIds[i] == itemMapId)
                {
                    _pickCounts[i] = (byte)MaxPickPerItem;
                    return;
                }
            }

            _pickIds[_pickHead] = itemMapId;
            _pickCounts[_pickHead] = (byte)MaxPickPerItem;
            _pickHead++;
            if (_pickHead >= CountCap) _pickHead = 0;
        }

        // ── NRD (Ngọc Rồng Đen) map check ─────────────────────────────

        private static bool IsNrdMap(int mapId) => mapId >= 85 && mapId <= 91;

        private static bool IsNrdItem(ItemMap item) =>
            item.template.id >= 372 && item.template.id <= 378;

        // ── Parse ID list ──────────────────────────────────────────────

        private static void ParseIdList(string raw, HashSet<int> target)
        {
            target.Clear();
            if (string.IsNullOrEmpty(raw)) return;

            string[] parts = raw.Split(';');
            foreach (string part in parts)
            {
                string trimmed = part.Trim();
                if (trimmed.Length > 0 && int.TryParse(trimmed, out int id))
                    target.Add(id);
            }
        }
    }
}
