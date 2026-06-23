using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Panel.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int VipFlags { get; set; } 
        // 1: Vật Phẩm Thường, 2: Trang Bị Thường, 4: SPL, 8: Hủy Diệt, 32: Thần Linh, 64: SKH
    }

    public class InventoryData
    {
        public long Gold { get; set; }
        public long Gem { get; set; }
        public long Ruby { get; set; }
        public int BagMax { get; set; }
        public int BoxMax { get; set; }
        public List<InventoryItem> Items { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public int EmptySlots { get; set; } // Calculated: Max - Items.Count
        public int VipCount { get; set; } // Calculated: items with VipFlags > 2
    }

    public static class InventoryCacheManager
    {
        public static event Action<int, int>? DataUpdated;

        // Dictionary<AccountId, Dictionary<Type(0:Bag, 1:Box), InventoryData>>
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<int, InventoryData>> _cache = new();

        public static void UpdateCache(int accountId, int type, InventoryData data)
        {
            var accCache = _cache.GetOrAdd(accountId, _ => new ConcurrentDictionary<int, InventoryData>());
            
            data.EmptySlots = data.Items != null ? Math.Max(0, (type == 0 ? data.BagMax : data.BoxMax) - data.Items.Count) : 0;
            
            int vipCount = 0;
            if (data.Items != null)
            {
                foreach(var item in data.Items)
                {
                    if (item.VipFlags > 2) vipCount++;
                }
            }
            data.VipCount = vipCount;

            accCache[type] = data;
            
            DataUpdated?.Invoke(accountId, type);
        }

        public static InventoryData? GetCache(int accountId, int type)
        {
            if (_cache.TryGetValue(accountId, out var accCache))
            {
                if (accCache.TryGetValue(type, out var data))
                    return data;
            }
            return null;
        }
    }
}
