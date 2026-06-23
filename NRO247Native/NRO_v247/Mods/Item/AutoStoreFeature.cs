using System.Collections.Generic;
using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods.Items
{
    public class AutoStoreFeature : NRO_v247.Mods.HotReloadFeatureBase<AutoStoreFeature.StoreSettings>, IAutoFeature
    {
        public class StoreSettings
        {
            public bool Enabled;
            public bool StoreKh;
            public bool StoreTl;
            public bool StorePl;
            public int Star;
            public bool StoreCustom;
            public string CustomList = string.Empty;
        }

        private enum StoreState
        {
            Idle,
            MovingToNpc,
            OpeningStore,
            StoringItems,
            ReturningToMap
        }

        private StoreState _currentState = StoreState.Idle;
        private bool _enabled;
        private bool _storeKichHoat;
        private bool _storeThanLinh;
        private bool _storePhaLe;
        private int _starCount;
        
        private bool _storeCustom;
        private Dictionary<short, int> _customStoreDict = new Dictionary<short, int>();
        private HashSet<short> _idsToStoreThisTrip = new HashSet<short>();

        private int _originalMapId = -1;
        private int _originalZoneId = -1;
        private long _stateStartTime;
        private int _currentItemIndex;

        public bool IsActive => _enabled && _currentState != StoreState.Idle;
        public string CurrentState => GetStateDescription();
        public bool IsUtilityTask => true;

        public void ApplySettingsFromPanel(bool enabled, bool storeKh, bool storeTl, bool storePl, int star, bool storeCustom = false, string customList = "")
        {
            UpdateSettings(new StoreSettings
            {
                Enabled = enabled,
                StoreKh = storeKh,
                StoreTl = storeTl,
                StorePl = storePl,
                Star = star,
                StoreCustom = storeCustom,
                CustomList = customList ?? string.Empty,
            });
            ApplyPendingSettingsImmediately();
        }

        public void Update()
        {
            EnsureSettingsApplied();

            if (!_enabled)
            {
                _currentState = StoreState.Idle;
                return;
            }

            switch (_currentState)
            {
                case StoreState.Idle:
                    if (!IsBoxFull())
                    {
                        bool hasItemToStore = false;
                        _idsToStoreThisTrip.Clear();

                        for (int i = 0; i < Char.myCharz().arrItemBag.Length; i++)
                        {
                            Item item = Char.myCharz().arrItemBag[i];
                            if (item == null || item.template == null) continue;

                            if (IsVipItem(item))
                            {
                                hasItemToStore = true;
                                _idsToStoreThisTrip.Add(item.template.id);
                            }
                            else if (_storeCustom && _customStoreDict.TryGetValue(item.template.id, out int requiredQty))
                            {
                                if (ItemHelper.GetItemQuantityInBag(item.template.id) >= requiredQty)
                                {
                                    hasItemToStore = true;
                                    _idsToStoreThisTrip.Add(item.template.id);
                                }
                            }
                        }

                        if (hasItemToStore)
                        {
                            ModBootstrap.TrainFeature.PauseTrainForAction();
                            _originalMapId = TileMap.mapID;
                            _originalZoneId = TileMap.zoneID;
                            _currentState = StoreState.MovingToNpc;
                            int targetMap = MapHelper.GetHomeMapId(Char.myCharz().cgender); // 21, 22, 23 (Làng)
                            ServiceLocator.Get<IXmapService>()?.StartGoToMapFromPanel(targetMap);
                        }
                    }
                    break;

                case StoreState.MovingToNpc:
                    if (ServiceLocator.Get<IXmapService>() != null && !ServiceLocator.Get<IXmapService>().IsXmaping())
                    {
                        if (TileMap.mapID == MapHelper.GetHomeMapId(Char.myCharz().cgender))
                        {
                            _currentState = StoreState.OpeningStore;
                            _stateStartTime = mSystem.currentTimeMillis();
                        }
                    }
                    break;

                case StoreState.OpeningStore:
                    if (mSystem.currentTimeMillis() - _stateStartTime > 2000) // Đợi 2s sau xmap
                    {
                        // Gọi NPC 3 (Trưởng Lão)
                        Service.gI().openMenu(3);
                        _currentState = StoreState.StoringItems;
                        _currentItemIndex = Char.myCharz().arrItemBag.Length - 1;
                        _stateStartTime = mSystem.currentTimeMillis();
                    }
                    break;

                case StoreState.StoringItems:
                    if (mSystem.currentTimeMillis() - _stateStartTime > 500) // Cất mỗi món cách 0.5s
                    {
                        if (_currentItemIndex < 0 || IsBoxFull()) // Xong hoặc Rương đồ đã đầy
                        {
                            _currentState = StoreState.ReturningToMap;
                            ServiceLocator.Get<IXmapService>()?.StartGoToMapFromPanel(_originalMapId);
                            break;
                        }

                        Item item = Char.myCharz().arrItemBag[_currentItemIndex];
                        if (item != null && item.template != null && _idsToStoreThisTrip.Contains(item.template.id))
                        {
                            Service.gI().getItem(1, (sbyte)_currentItemIndex);
                        }
                        
                        _currentItemIndex--;
                        _stateStartTime = mSystem.currentTimeMillis();
                    }
                    break;

                case StoreState.ReturningToMap:
                    if (ServiceLocator.Get<IXmapService>() != null && !ServiceLocator.Get<IXmapService>().IsXmaping())
                    {
                        if (TileMap.mapID == _originalMapId)
                        {
                            _currentState = StoreState.Idle;
                            ModBootstrap.TrainFeature.ResumeTrainAfterAction();
                        }
                    }
                    break;
            }
        }

        private string GetStateDescription()
        {
            switch (_currentState)
            {
                case StoreState.MovingToNpc: return "Cất đồ: Đang bay về làng";
                case StoreState.OpeningStore: return "Cất đồ: Xin mở rương";
                case StoreState.StoringItems: return "Cất đồ: Đang cất vật phẩm";
                case StoreState.ReturningToMap: return "Cất đồ: Quay lại map";
                default: return "";
            }
        }

        private bool IsBoxFull()
        {
            if (Char.myCharz().arrItemBox == null) return false;

            int count = 0;
            for (int i = 0; i < Char.myCharz().arrItemBox.Length; i++)
            {
                if (Char.myCharz().arrItemBox[i] != null) count++;
            }
            return count >= Char.myCharz().arrItemBox.Length;
        }



        private bool IsVipItem(Item item)
        {
            if (item == null || item.template == null) return false;

            // Kích hoạt
            if (_storeKichHoat && ItemHelper.IsSkhItem(item))
            {
                return true;
            }

            // Thần linh
            if (_storeThanLinh && ItemHelper.IsGodItem(item))
            {
                return true;
            }

            // Pha lê
            if (_storePhaLe && item.itemOption != null)
            {
                int starCount = 0;
                foreach (var opt in item.itemOption)
                {
                    // Option ID 107 (Pha lê)
                    if (opt != null && opt.optionTemplate != null && opt.optionTemplate.id == 107)
                    {
                        starCount = opt.param;
                        break;
                    }
                }
                if (starCount >= _starCount)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void OnSettingsHotReload()
        {
            ResetRuntimeForHotReload();

            _enabled = _settings.Enabled;
            _storeKichHoat = _settings.StoreKh;
            _storeThanLinh = _settings.StoreTl;
            _storePhaLe = _settings.StorePl;
            _starCount = _settings.Star;
            _storeCustom = _settings.StoreCustom;

            _customStoreDict.Clear();
            if (_storeCustom && !string.IsNullOrEmpty(_settings.CustomList))
            {
                string[] items = _settings.CustomList.Split(new[] { ';', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var itemStr in items)
                {
                    string[] pt = itemStr.Split('-');
                    if (pt.Length == 2 && short.TryParse(pt[0].Trim(), out short id) && int.TryParse(pt[1].Trim(), out int qty))
                    {
                        _customStoreDict[id] = qty;
                    }
                }
            }
        }

        private void ResetRuntimeForHotReload()
        {
            if (_currentState != StoreState.Idle)
                ModBootstrap.TrainFeature.ResumeTrainAfterAction();

            _currentState = StoreState.Idle;
            _idsToStoreThisTrip.Clear();
            _originalMapId = -1;
            _originalZoneId = -1;
            _stateStartTime = 0L;
            _currentItemIndex = 0;

            ServiceLocator.Get<IXmapService>()?.StopFromPanel();
            if (GameCanvas.panel != null) GameCanvas.panel.hide();
        }
    }
}
