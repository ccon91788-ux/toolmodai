using System;
using System.Collections.Generic;

namespace NRO_v247.Mods.Items
{
    public class AutoItemFeature : IAutoFeature
    {
        private bool _enabled = false;

        public bool IsActive => false;
        public string CurrentState => "";
        public bool IsUtilityTask => true;

        private static readonly short[] DefaultTrashIds = new short[] { 225, 226, 19, 20, 212 };

        // ─── Drop Item settings ─────────────────────────────────────────────
        private struct DropEntry { public short ItemId; public int HsdThreshold; } // -1 = không lọc HSD

        private bool _dropTrash;
        private bool _dropCustom;
        private readonly System.Collections.Generic.List<DropEntry> _dropEntries = new System.Collections.Generic.List<DropEntry>();
        private long _lastUpdateDrop;
        private const int DropIntervalMs = 3000;
        private const int HsdOptionId = 93;

        // ─── Use Item settings ─────────────────────────────────────────────
        private bool _useCuongNo;   // Id=381, idIcon buff=2754
        private bool _useBoHuyet;  // Id=382, idIcon buff=2755
        private bool _useBoKhi;    // Id=383, idIcon buff=2756
        private bool _useGiapXen;  // Id=384, idIcon buff=2757
        private bool _useMask;     // Id=764, idIcon buff=7149
        private bool _useClover;   // Id=1635, idIcon buff=13618
        private bool _useFood;     // Do an: Kem dau/Mi ly/Xuc xich/Sushi/Pudding
        private bool _useDetector; // Id=379, idIcon buff=2758

        private long _lastUpdateUse;

        private static readonly short[] FoodItemIds = new short[] { 665, 666, 664, 667, 663 };
        private static readonly int[]   FoodIdCons  = new int[]   { 6326, 6327, 6325, 6328, 6324 };

        // ─── Custom item timer (itemId-intervalMs) ─────────────────────────
        // Cau truc: itemId -> intervalMs
        private Dictionary<short, long> _customItemIntervals = new Dictionary<short, long>();
        // itemId -> lastUsedTime (ms)
        private Dictionary<short, long> _customItemLastUsed = new Dictionary<short, long>();

        // ─── Apply Drop settings ───────────────────────────────────────────

        public void ApplyDropSettingsFromPanel(bool dropTrash, bool dropCustom, string idsRaw)
        {
            _dropTrash = dropTrash;
            _dropCustom = dropCustom;
            _dropEntries.Clear();

            if (_dropCustom && !string.IsNullOrEmpty(idsRaw))
            {
                var lines = idsRaw.Split(new[] { ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;

                    var segments = trimmed.Split('|');
                    if (segments.Length >= 1 && short.TryParse(segments[0].Trim(), out short itemId))
                    {
                        int threshold = -1;
                        if (segments.Length >= 2 && int.TryParse(segments[1].Trim(), out int t))
                            threshold = t;
                        _dropEntries.Add(new DropEntry { ItemId = itemId, HsdThreshold = threshold });
                    }
                }
            }
        }

        // ─── Apply Use Item settings ───────────────────────────────────────

        public void ApplyUseSettingsFromPanel(bool cuongNo, bool boHuyet, bool boKhi, bool giapXen,
                                               bool mask, bool clover, bool food, bool detector,
                                               string itemByIds = "")
        {
            _useCuongNo  = cuongNo;
            _useBoHuyet  = boHuyet;
            _useBoKhi    = boKhi;
            _useGiapXen  = giapXen;
            _useMask     = mask;
            _useClover   = clover;
            _useFood     = food;
            _useDetector = detector;

            // Parse custom item timer list: "itemId-intervalMs;itemId2-intervalMs2"
            _customItemIntervals.Clear();
            if (!string.IsNullOrEmpty(itemByIds))
            {
                var entries = itemByIds.Split(new[] { ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var entry in entries)
                {
                    var parts = entry.Trim().Split('-');
                    if (parts.Length == 2
                        && short.TryParse(parts[0].Trim(), out short id)
                        && long.TryParse(parts[1].Trim(), out long interval)
                        && interval > 0)
                    {
                        _customItemIntervals[id] = interval;
                    }
                }
            }

            _enabled = true;
        }

        public void DisableFromPanel()
        {
            _enabled = false;
        }

        // ─── Update ───────────────────────────────────────────────────────

        public void Update()
        {
            if (Char.myCharz() == null || Char.myCharz().statusMe == 14 ||
                Char.myCharz().cHP <= 0 || Char.myCharz().isDie) return;

            long now = mSystem.currentTimeMillis();

            // Tự vứt đồ (mỗi 3 giây)
            if ((_dropTrash || _dropCustom) && now - _lastUpdateDrop > DropIntervalMs)
            {
                _lastUpdateDrop = now;
                HandleAutoDropItems();
            }

            // Tự dùng item (mỗi 1 giây)
            if (now - _lastUpdateUse > 1000)
            {
                _lastUpdateUse = now;
                HandleAutoUseItems(now);
            }

        }

        // ─── Auto Drop Item logic ─────────────────────────────────────────

        private void HandleAutoDropItems()
        {
            var bag = Char.myCharz()?.arrItemBag;
            if (bag == null) return;

            // Vứt đồ rác cố định
            if (_dropTrash)
            {
                var trashSet = new HashSet<short>(DefaultTrashIds);
                for (int i = 0; i < bag.Length; i++)
                {
                    var bagItem = bag[i];
                    if (bagItem?.template != null && trashSet.Contains(bagItem.template.id))
                    {
                        Service.gI().useItem(2, 1, (sbyte)i, -1);
                        return; // Vứt 1 item/lần, lần sau tiếp tục
                    }
                }
            }

            // Vứt theo ID tùy chỉnh (có hỗ trợ HSD)
            if (_dropCustom)
            {
                foreach (var entry in _dropEntries)
                {
                    for (int i = 0; i < bag.Length; i++)
                    {
                        var bagItem = bag[i];
                        if (bagItem?.template == null || bagItem.template.id != entry.ItemId) continue;

                        if (entry.HsdThreshold == -1)
                        {
                            // Không lọc HSD → vứt luôn
                            Service.gI().useItem(2, 1, (sbyte)i, -1);
                            return;
                        }
                        else
                        {
                            // Lọc theo HSD: vứt khi option 93 < threshold
                            int hsd = GetItemOptionParam(bagItem, HsdOptionId);
                            if (hsd >= 0 && hsd < entry.HsdThreshold)
                            {
                                Service.gI().useItem(2, 1, (sbyte)i, -1);
                                return;
                            }
                        }
                    }
                }
            }
        }

        // ─── Auto Use Item logic ──────────────────────────────────────────

        private void HandleAutoUseItems(long now)
        {
            // Buff items: dung khi buff het han va con item trong tui
            TryUseBuffItem(_useCuongNo,  381,  2754);
            TryUseBuffItem(_useBoHuyet,  382,  2755);
            TryUseBuffItem(_useBoKhi,    383,  2756);
            TryUseBuffItem(_useGiapXen,  384,  2757);
            TryUseBuffItem(_useMask,     764,  7149);
            TryUseBuffItem(_useClover,   1635, 13618);
            TryUseBuffItem(_useDetector, 379,  2758);

            // Do an: quet tui theo slot, gap cai nao truoc thi dung
            if (_useFood)
            {
                bool hasActiveFoodBuff = false;
                for (int j = 0; j < FoodIdCons.Length; j++)
                {
                    if (ItemTime.getItemById(FoodIdCons[j]) != null)
                    {
                        hasActiveFoodBuff = true;
                        break;
                    }
                }

                if (!hasActiveFoodBuff)
                {
                    var bag = Char.myCharz()?.arrItemBag;
                    if (bag != null)
                    {
                        var foodSet = new HashSet<short>(FoodItemIds);
                        for (int i = 0; i < bag.Length; i++)
                        {
                            var bagItem = bag[i];
                            if (bagItem?.template != null && foodSet.Contains(bagItem.template.id))
                            {
                                Service.gI().useItem(0, 1, (sbyte)i, -1);
                                break;
                            }
                        }
                    }
                }
            }

            // Custom item timer: tuyen dung theo interval
            if (_customItemIntervals.Count > 0)
            {
                var bag = Char.myCharz()?.arrItemBag;
                if (bag != null)
                {
                    foreach (var kvp in _customItemIntervals)
                    {
                        short itemId = kvp.Key;
                        long interval = kvp.Value;

                        // Kiem tra interval
                        if (_customItemLastUsed.TryGetValue(itemId, out long lastUsed)
                            && now - lastUsed < interval)
                            continue;

                        // Tim slot co item
                        for (int i = 0; i < bag.Length; i++)
                        {
                            var bagItem = bag[i];
                            if (bagItem?.template != null && bagItem.template.id == itemId)
                            {
                                Service.gI().useItem(0, 1, (sbyte)i, -1);
                                _customItemLastUsed[itemId] = now;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void TryUseBuffItem(bool enabled, int itemId, int idIconBuff)
        {
            if (!enabled) return;
            if (ItemTime.getItemById(idIconBuff) != null) return;
            var bag = Char.myCharz()?.arrItemBag;
            if (bag == null) return;
            for (int i = 0; i < bag.Length; i++)
            {
                var item = bag[i];
                if (item?.template != null && item.template.id == itemId)
                {
                    Service.gI().useItem(0, 1, (sbyte)i, -1);
                    return;
                }
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────

        private int GetItemOptionParam(Item item, int optionId)
        {
            if (item.itemOption != null)
            {
                for (int i = 0; i < item.itemOption.Length; i++)
                {
                    ItemOption opt = item.itemOption[i];
                    if (opt != null && opt.optionTemplate != null && opt.optionTemplate.id == optionId)
                        return opt.param;
                }
            }
            return -1;
        }
    }
}
