using System.Collections.Generic;
using NRO_v247.Mods.Xmap;
using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods.Items
{
    /// <summary>
    /// Port 1:1 từ AutoSellTrashItems.java.
    /// Flow: Idle → MovingToStation → SellTrashItems (teleport + openMenu + sell inline) → ReturningToMap
    /// </summary>
    public class AutoSellFeature : NRO_v247.Mods.HotReloadFeatureBase<AutoSellFeature.SellSettings>, IAutoFeature
    {
        public class SellSettings
        {
            public bool Enabled;
            public int EmptySlotThreshold;
            public bool KeepStar;
            public bool KeepGod;
            public bool KeepSkh;
            public int SellMaxLevel;
            public string KeepIds = string.Empty;
            public string ForceSellIds = string.Empty;
            public bool DropInsteadOfSell;
        }

        private enum SellState
        {
            Idle,
            MovingToStation,
            SellTrashItems,    // Port từ Java sellTrashItems(): teleport, openMenu, confirm dialog, scan items — tất cả trong 1 step
            ReturningToMap
        }

        private SellState _state = SellState.Idle;

        // ── Settings từ Panel ────────────────────────────────────────────────

        private bool _enabled;
        private int _emptySlotThreshold;
        private bool _keepStarItems;
        private bool _keepGodItems;
        private bool _keepSkhItems;
        private int _sellMaxLevel;
        private bool _dropInsteadOfSell;
        private readonly HashSet<int> _keepIds = new();
        private readonly HashSet<int> _forceSellIds = new();

        // ── Dữ liệu chuyến đi ───────────────────────────────────────────────

        private int _originMapId = -1;
        private int _originZoneId = -1;

        // ── Port từ Java: lastRemoveItemIndex + removeAttempts ──────────────
        private int _lastRemoveItemIndex = -1;
        private int _removeAttempts = 0;
        private long _lastTimeUpdate = 0L;

        public bool IsActive => _enabled && _state != SellState.Idle;
        public string CurrentState => GetStateDescription();
        public bool IsUtilityTask => true;

        // ── Public API ───────────────────────────────────────────────────────

        public bool IsRunning() => _state != SellState.Idle;

        public void ApplySettingsFromPanel(
            bool enabled,
            int emptySlotThreshold,
            bool keepStar,
            bool keepGod,
            bool keepSkh,
            int sellMaxLevel,
            string keepIds,
            string forceSellIds,
            bool dropInsteadOfSell = false)
        {
            UpdateSettings(new SellSettings
            {
                Enabled = enabled,
                EmptySlotThreshold = emptySlotThreshold,
                KeepStar = keepStar,
                KeepGod = keepGod,
                KeepSkh = keepSkh,
                SellMaxLevel = sellMaxLevel,
                KeepIds = keepIds ?? string.Empty,
                ForceSellIds = forceSellIds ?? string.Empty,
                DropInsteadOfSell = dropInsteadOfSell,
            });
            ApplyPendingSettingsImmediately();
        }

        // ── Update mỗi frame ─────────────────────────────────────────────────

        public void Update()
        {
            EnsureSettingsApplied();

            if (!_enabled)
            {
                _state = SellState.Idle;
                return;
            }

            // Throttle 750ms giống Java: mSystem.currentTimeMillis() - lastTimeUpdate <= 750
            long now = mSystem.currentTimeMillis();
            if (now - _lastTimeUpdate <= 750L)
                return;
            _lastTimeUpdate = now;

            switch (_state)
            {
                case SellState.Idle:
                    TryTriggerSell();
                    break;

                case SellState.MovingToStation:
                    GotoSpaceshipStation();
                    break;

                case SellState.SellTrashItems:
                    SellTrashItems();
                    break;

                case SellState.ReturningToMap:
                    GotoLastMapAndZone();
                    break;
            }
        }

        private string GetStateDescription()
        {
            switch (_state)
            {
                case SellState.MovingToStation: return "Bán đồ: Đi trạm tàu...";
                case SellState.SellTrashItems:  return "Bán đồ: Đang bán đồ...";
                case SellState.ReturningToMap:  return "Bán đồ: Quay lại vị trí cũ...";
                default: return "";
            }
        }

        // ── Step 0 (Idle): kiểm tra có nên bán không ────────────────────────

        private void TryTriggerSell()
        {
            if (ItemHelper.GetEmptyBagSlotsCount() > _emptySlotThreshold) return;
            if (!HasAnyTrashItem()) return;

            _originMapId  = TileMap.mapID;
            _originZoneId = TileMap.zoneID;
            _lastRemoveItemIndex = -1;
            _removeAttempts = 0;

            ModBootstrap.TrainFeature.PauseTrainForAction();

            if (_dropInsteadOfSell)
            {
                _state = SellState.SellTrashItems;
                return;
            }

            int stationMap = MapHelper.GetStationMapId(Char.myCharz().cgender);
            if (TileMap.mapID != stationMap)
            {
                ServiceLocator.Get<IXmapService>()?.StartGoToMapFromPanel(stationMap);
                _state = SellState.MovingToStation;
            }
            else
            {
                _state = SellState.SellTrashItems;
            }
        }

        // ── Step 3 (Java): gotoSpaceshipStation ─────────────────────────────

        private void GotoSpaceshipStation()
        {
            int stationMap = MapHelper.GetStationMapId(Char.myCharz().cgender);

            if (TileMap.mapID != stationMap)
            {
                var xmap = ServiceLocator.Get<IXmapService>();
                if (xmap != null && !xmap.IsXmaping())
                    xmap.StartGoToMapFromPanel(stationMap);
                return;
            }

            // Đã đến trạm → step bán đồ
            _state = SellState.SellTrashItems;
        }

        // ── Step 4 (Java): sellTrashItems — port 1:1 ────────────────────────

        private void SellTrashItems()
        {
            var me = Char.myCharz();

            if (!_dropInsteadOfSell)
            {
                // 1) Xác định toạ độ NPC shop theo map trạm
                int targetX = 389;
                int targetY = 336;

                if (TileMap.mapID == 24)
                {
                    targetX = 389; targetY = 336;
                }
                else if (TileMap.mapID == 25)
                {
                    targetX = 508; targetY = 336;
                }
                else if (TileMap.mapID == 26)
                {
                    targetX = 511; targetY = 336;
                }

                // 2) Teleport đến gần NPC (giống Java: teleportMyChar rồi return)
                if (Res.distance(me.cx, me.cy, targetX, targetY) > 15)
                {
                    PathUtils.teleportTo(targetX, targetY);
                    return;
                }

                // 3) Mở menu NPC nếu panel chưa show (giống Java: openMenu(16) rồi return)
                if (GameCanvas.panel != null && !GameCanvas.panel.isShow)
                {
                    Service.gI().openMenu(16);
                    return;
                }

                // 4) Nếu có dialog xác nhận đang hiện → confirm bán (giống Java)
                if (GameCanvas.currentDialog != null && _lastRemoveItemIndex > -1)
                {
                    Service.gI().saleItem(1, 1, (short)_lastRemoveItemIndex);
                    GameCanvas.endDlg();
                    return;
                }
            }

            // 5) Quét túi tìm đồ rác để bán (giống Java: duyệt từ cuối về đầu)
            int startIdx = me.arrItemBag.Length - 1;
            if (_lastRemoveItemIndex != -1)
                startIdx = _lastRemoveItemIndex;

            for (int i = startIdx; i >= 0; i--)
            {
                Item item = me.arrItemBag[i];
                if (item != null && ShouldSellItem(item))
                {
                    if (_dropInsteadOfSell)
                    {
                        Service.gI().useItem(2, 1, (sbyte)i, -1);
                    }
                    else
                    {
                        Service.gI().saleItem(0, 1, (short)i);
                    }

                    if (i == _lastRemoveItemIndex)
                        _removeAttempts++;
                    else
                        _removeAttempts = 0;

                    if (_removeAttempts >= 5)
                        _lastRemoveItemIndex = i - 1;
                    else
                        _lastRemoveItemIndex = i;
                    return;
                }
            }

            // 6) Hết đồ rác → đóng panel, chuyển sang quay về
            if (GameCanvas.panel != null)
                GameCanvas.panel.hide();

            _removeAttempts = 0;
            _lastRemoveItemIndex = -1;

            FinishAndReturn();
        }

        // ── Step 6 (Java): gotoLastMapAndZone ───────────────────────────────

        private void GotoLastMapAndZone()
        {
            if (TileMap.mapID != _originMapId)
            {
                var xmap = ServiceLocator.Get<IXmapService>();
                if (xmap != null && !xmap.IsXmaping())
                    xmap.StartGoToMapFromPanel(_originMapId);
                return;
            }

            if (TileMap.zoneID != _originZoneId && _originZoneId >= 0)
            {
                Service.gI().requestChangeZone(_originZoneId, -1);
                return;
            }

            _state = SellState.Idle;
            ModBootstrap.TrainFeature.ResumeTrainAfterAction();
        }

        private void FinishAndReturn()
        {
            if (_originMapId > 0 && TileMap.mapID != _originMapId)
            {
                ServiceLocator.Get<IXmapService>()?.StartGoToMapFromPanel(_originMapId);
                _state = SellState.ReturningToMap;
            }
            else
            {
                _state = SellState.ReturningToMap;
                GotoLastMapAndZone();
            }
        }

        // ── Logic lọc đồ ────────────────────────────────────────────────────

        private bool ShouldSellItem(Item item)
        {
            if (item == null || item.template == null) return false;

            if (_forceSellIds.Contains(item.template.id)) return true;

            int t = item.template.type;
            if (t == 5 || t == 6 || t == 9 || t == 11 || t == 12 || t == 14 ||
                t == 23 || t == 24 || t == 27 || t == 29 || t == 32)
                return false;

            if (_keepIds.Contains(item.template.id)) return false;

            if (t < 0 || t > 4) return false;

            if (_keepStarItems && ItemHelper.IsStarItem(item)) return false;
            if (_keepSkhItems  && ItemHelper.IsSkhItem(item)) return false;
            if (_keepGodItems  && ItemHelper.IsGodItem(item)) return false;

            return item.template.level <= _sellMaxLevel;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private bool HasAnyTrashItem()
        {
            var bag = Char.myCharz()?.arrItemBag;
            if (bag == null) return false;
            foreach (var item in bag)
                if (ShouldSellItem(item)) return true;
            return false;
        }



        private static void ParseIds(string raw, HashSet<int> target)
        {
            target.Clear();
            if (string.IsNullOrWhiteSpace(raw)) return;
            foreach (var part in raw.Split(new[] { ';', ',', '\n', '\r' },
                         System.StringSplitOptions.RemoveEmptyEntries))
                if (int.TryParse(part.Trim(), out int id))
                    target.Add(id);
        }

        protected override void OnSettingsHotReload()
        {
            ResetRuntimeForHotReload();

            _enabled = _settings.Enabled;
            _emptySlotThreshold = _settings.EmptySlotThreshold;
            _keepStarItems = _settings.KeepStar;
            _keepGodItems = _settings.KeepGod;
            _keepSkhItems = _settings.KeepSkh;
            _sellMaxLevel = _settings.SellMaxLevel;
            _dropInsteadOfSell = _settings.DropInsteadOfSell;

            ParseIds(_settings.KeepIds, _keepIds);
            ParseIds(_settings.ForceSellIds, _forceSellIds);
        }

        private void ResetRuntimeForHotReload()
        {
            if (_state != SellState.Idle)
                ModBootstrap.TrainFeature.ResumeTrainAfterAction();

            _state = SellState.Idle;
            _originMapId = -1;
            _originZoneId = -1;
            _lastRemoveItemIndex = -1;
            _removeAttempts = 0;
            _lastTimeUpdate = 0L;

            ServiceLocator.Get<IXmapService>()?.StopFromPanel();
            if (GameCanvas.panel != null) GameCanvas.panel.hide();
        }
    }
}
