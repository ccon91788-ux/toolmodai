using System;
using System.Collections.Generic;
using NRO_v247.Mods.Utils;
using NRO_v247.Mods.Xmap;
using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods.Items
{
    public class AutoBuyFeature : NRO_v247.Mods.HotReloadFeatureBase<AutoBuyFeature.BuySettings>, IAutoFeature
    {
        public class BuySettings
        {
            public bool AutoBuyTdlt;
            public bool AutoBuyKhauTrang;
            public int BuyKhauTrangQty;
            public bool AutoBuyCoBonLa;
            public int BuyCoBonLaQty;
            public bool AutoBuyBuaDe;
            public int BuyBuaDeQty;
            public bool AutoBuyPrivateTicket;
            public bool AutoBuyThoiVang;
            public long BuyThoiVangMinGold = 1_000_000_000;
            public bool AutoBuyCustom;
            public string CustomData = string.Empty;
        }

        private bool _autoBuyTdlt;
        private bool _autoBuyKhauTrang;
        private int _buyKhauTrangQty;
        private bool _autoBuyCoBonLa;
        private int _buyCoBonLaQty;
        private bool _autoBuyBuaDe;
        private int _buyBuaDeQty;
        private bool _autoBuyPrivateTicket;
        private bool _autoBuyThoiVang;
        private long _buyThoiVangMinGold = 1_000_000_000;

        private enum BuyState
        {
            Idle,
            MovingToNpc,
            OpeningStore,
            WaitingForMenu,
            BuyingItems,
            ExecutingCustomMenu,
            ReturningToMap
        }

        private BuyState _currentState = BuyState.Idle;
        private int _originalMapId = -1;
        private int _targetMapId = -1;
        private long _stateStartTime;

        // Custom Items
        private bool _autoBuyCustom;
        private List<CustomBuyItem> _customItems = new List<CustomBuyItem>();
        private CustomBuyItem _currentCustomItem;
        private int _customMenuStep = -1;
        private int _customBuySpamCount = 0;
        
        private struct CustomBuyItem
        {
            public short ItemId;
            public int Qty;
            public int MapId;
            public short NpcId;
            public int CurrencyType;
            public int BuyMode;
            public int[] Menus; // [menu1, menu2...] 
        }
        
        // Item IDs
        private const short ID_KHAU_TRANG = 764;
        private const short ID_CO_4_LA = 1635;
        private const short ID_BUA_DE = 1628;
        private const short ID_PRIVATE_TICKET = 1825;
        private const short ID_THOI_VANG = 457;
        
        // TDLT có nhiều loại giá tuỳ vào lượng
        private const short ID_TDLT_9_NGAY = 1523;
        private const short ID_TDLT_30_NGAY = 1524;

        // Ngưỡng vàng tối thiểu để dừng mua thỏi vàng (37 triệu)
        private const long MIN_GOLD_TO_STOP_BUY_THOI_VANG = 100_000_000;

        public bool IsActive => (_autoBuyTdlt || _autoBuyKhauTrang || _autoBuyCoBonLa || _autoBuyBuaDe || _autoBuyThoiVang || _autoBuyPrivateTicket || _autoBuyCustom) && _currentState != BuyState.Idle;
        public string CurrentState => GetStateDescription();
        
        public int Priority => 100; // Mua đồ quan trọng nhất
        public bool IsRequested => _currentState != BuyState.Idle || ShouldStartBuyCycle();

        public void ApplyThoiVangSettings(bool autoBuyThoiVang, long minGold)
        {
            BuySettings next = CloneCurrentSettings();
            next.AutoBuyThoiVang = autoBuyThoiVang;
            next.BuyThoiVangMinGold = minGold > 0 ? minGold : 1_000_000_000;
            UpdateSettings(next);
            ApplyPendingSettingsImmediately();
        }

        public void ApplySettingsFromPanel(bool autoBuyTdlt, bool autoBuyKhauTrang, int buyKhauTrangQty, 
                                           bool autoBuyCoBonLa, int buyCoBonLaQty, 
                                           bool autoBuyBuaDe, int buyBuaDeQty,
                                           bool autoBuyPrivateTicket)
        {
            BuySettings next = CloneCurrentSettings();
            next.AutoBuyTdlt = autoBuyTdlt;
            next.AutoBuyKhauTrang = autoBuyKhauTrang;
            next.BuyKhauTrangQty = buyKhauTrangQty;
            next.AutoBuyCoBonLa = autoBuyCoBonLa;
            next.BuyCoBonLaQty = buyCoBonLaQty;
            next.AutoBuyBuaDe = autoBuyBuaDe;
            next.BuyBuaDeQty = buyBuaDeQty;
            next.AutoBuyPrivateTicket = autoBuyPrivateTicket;
            UpdateSettings(next);
            ApplyPendingSettingsImmediately();
        }

        public void ApplyCustomSettings(bool enable, string data)
        {
            BuySettings next = CloneCurrentSettings();
            next.AutoBuyCustom = enable;
            next.CustomData = data ?? string.Empty;
            UpdateSettings(next);
            ApplyPendingSettingsImmediately();
        }

        public void Update()
        {
            EnsureSettingsApplied();

            if (!_autoBuyTdlt && !_autoBuyKhauTrang && !_autoBuyCoBonLa && !_autoBuyBuaDe && !_autoBuyThoiVang && !_autoBuyPrivateTicket && !_autoBuyCustom)
            {
                _currentState = BuyState.Idle;
                return;
            }

            if (_currentState != BuyState.Idle)
            {
                // state is read by CurrentState property
            }

            switch (_currentState)
            {
                case BuyState.Idle:
                    if (GameCanvas.gameTick % 20 != 0) return;
                    
                    if (NeedBuyCustomItem(out var lackingItem))
                    {
                        _currentCustomItem = lackingItem;
                        _targetMapId = lackingItem.MapId;
                        ModBootstrap.TrainFeature.PauseTrainForAction();
                        _originalMapId = TileMap.mapID;
                        _currentState = BuyState.MovingToNpc;
                        ServiceLocator.Get<IXmapService>()?.StartGoToMapFromPanel(_targetMapId);
                        break;
                    }

                    if (NeedToBuyAnything())
                    {
                        _targetMapId = 5;
                        ModBootstrap.TrainFeature.PauseTrainForAction();
                        _originalMapId = TileMap.mapID;
                        _currentState = BuyState.MovingToNpc;
                        ServiceLocator.Get<IXmapService>()?.StartGoToMapFromPanel(_targetMapId); // Map 5 is Đảo Kame where Santa is
                    }
                    break;

                case BuyState.MovingToNpc:
                    if (ServiceLocator.Get<IXmapService>() != null && !ServiceLocator.Get<IXmapService>().IsXmaping())
                    {
                        if (TileMap.mapID == _targetMapId)
                        {
                            if (_targetMapId != 5 || _currentCustomItem.MapId == _targetMapId)
                            {
                                // Chạy custom
                                if (_currentCustomItem.ItemId > 0 && _currentCustomItem.MapId == _targetMapId)
                                {
                                    _currentState = BuyState.ExecutingCustomMenu;
                                    _customMenuStep = -1; // -1 = gọi open menu
                                    _customBuySpamCount = 0;
                                    _stateStartTime = mSystem.currentTimeMillis();
                                    break;
                                }
                            }
                            
                            _currentState = BuyState.OpeningStore;
                            _stateStartTime = mSystem.currentTimeMillis();
                        }
                    }
                    break;

                case BuyState.OpeningStore:
                    Xmap.NextMap.StartConfirmNpc(39, "Cửa hàng");
                    _currentState = BuyState.WaitingForMenu;
                    break;

                case BuyState.WaitingForMenu:
                    Xmap.NextMap.UpdateConfirmNpc();
                    // reflection from NextMap._confirming since it's private? No, let's just wait 1500ms and assume it's open, 
                    // or check GameCanvas.menu.showMenu. Actually, Xmap.NextMap handles it and when it's done _confirming is false.
                    // But maybe we can just wait 1.5 seconds.
                    if (mSystem.currentTimeMillis() - _stateStartTime > 1500)
                    {
                        _currentState = BuyState.BuyingItems;
                        _stateStartTime = mSystem.currentTimeMillis();
                    }
                    break;

                case BuyState.BuyingItems:
                    if (mSystem.currentTimeMillis() - _stateStartTime > 1000)
                    {
                        bool boughtSomething = false;

                        if (_autoBuyTdlt && NeedBuyTdlt())
                        {
                            short tdltId = GetTdltIdToBuy();
                            Service.gI().buyItem(1, tdltId, 0);
                            boughtSomething = true;
                        }
                        else if (_autoBuyKhauTrang && ItemHelper.GetItemQuantityInBag(ID_KHAU_TRANG) < _buyKhauTrangQty && Char.myCharz().checkLuong() >= 2)
                        {
                            Service.gI().buyItem(1, ID_KHAU_TRANG, 0); // Khẩu trang giá 2 ngọc
                            boughtSomething = true;
                        }
                        else if (_autoBuyCoBonLa && ItemHelper.GetItemQuantityInBag(ID_CO_4_LA) < _buyCoBonLaQty && Char.myCharz().checkLuong() >= 10)
                        {
                            Service.gI().buyItem(1, ID_CO_4_LA, 1);
                            Service.gI().sendClientInput(new TField[] { CreateTField((_buyCoBonLaQty - ItemHelper.GetItemQuantityInBag(ID_CO_4_LA)).ToString()) });
                            boughtSomething = true;
                        }
                        else if (_autoBuyBuaDe && ItemHelper.GetItemQuantityInBag(ID_BUA_DE) < _buyBuaDeQty && Char.myCharz().checkLuong() >= 5)
                        {
                            Service.gI().buyItem(1, ID_BUA_DE, 1);
                            Service.gI().sendClientInput(new TField[] { CreateTField((_buyBuaDeQty - ItemHelper.GetItemQuantityInBag(ID_BUA_DE)).ToString()) });
                            boughtSomething = true;
                        }
                        else if (_autoBuyPrivateTicket && ItemHelper.GetItemQuantityInBag(ID_PRIVATE_TICKET) == 0 && Char.myCharz().checkLuong() >= 1)
                        {
                            Service.gI().buyItem(1, ID_PRIVATE_TICKET, 0); // Tab Hỗ trợ (1), Mua 1 cái (0)
                            boughtSomething = true;
                        }
                        else if (_autoBuyThoiVang && CanContinueBuyThoiVang())
                        {
                            Service.gI().buyItem(0, ID_THOI_VANG, 0);
                            boughtSomething = true;
                        }

                        if (!boughtSomething)
                        {
                            _currentState = BuyState.ReturningToMap;
                            ServiceLocator.Get<IXmapService>()?.StartGoToMapFromPanel(_originalMapId);
                        }
                        else
                        {
                            _stateStartTime = mSystem.currentTimeMillis();
                        }
                    }
                    break;

                case BuyState.ExecutingCustomMenu:
                    if (mSystem.currentTimeMillis() - _stateStartTime > 500)
                    {
                        if (_customMenuStep == -1) // Mở NPC
                        {
                            Npc npc = GameScr.findNPCInMap(_currentCustomItem.NpcId);
                            if (npc != null)
                            {
                                Char.myCharz().npcFocus = npc;
                                PathUtils.teleportTo(npc.cx, npc.cy - 3);
                            }
                            Service.gI().openMenu(_currentCustomItem.NpcId);
                            _customMenuStep = 0;
                            _stateStartTime = mSystem.currentTimeMillis();
                        }
                        else if (_currentCustomItem.Menus != null && _customMenuStep < _currentCustomItem.Menus.Length)
                        {
                            Service.gI().confirmMenu(_currentCustomItem.NpcId, (sbyte)_currentCustomItem.Menus[_customMenuStep]);
                            _customMenuStep++;
                            _stateStartTime = mSystem.currentTimeMillis();
                        }
                        else
                        {
                            // if final CurrencyType >= 0 then BuyItem, else it's event exchange (finished)
                            if (_currentCustomItem.CurrencyType >= 0)
                            {
                                if (_currentCustomItem.BuyMode == 0 && _currentCustomItem.Qty > 1)
                                {
                                    if (_customBuySpamCount < _currentCustomItem.Qty)
                                    {
                                        Service.gI().buyItem((sbyte)_currentCustomItem.CurrencyType, _currentCustomItem.ItemId, 0);
                                        _customBuySpamCount++;
                                        _stateStartTime = mSystem.currentTimeMillis();
                                        break; // Spam đủ số lượng mới cho phép đi xuống reset Custom Target
                                    }
                                }
                                else
                                {
                                    int buyParam = (_currentCustomItem.BuyMode == 1 && _currentCustomItem.Qty > 1) ? 1 : 0;
                                    Service.gI().buyItem((sbyte)_currentCustomItem.CurrencyType, _currentCustomItem.ItemId, buyParam);
                                    if (_currentCustomItem.BuyMode == 1 && _currentCustomItem.Qty > 1)
                                    {
                                        Service.gI().sendClientInput(new TField[] { CreateTField(_currentCustomItem.Qty.ToString()) });
                                    }
                                }
                            }
                            
                            _currentCustomItem = default; // reset custom target
                            _currentState = BuyState.ReturningToMap;
                            ServiceLocator.Get<IXmapService>()?.StartGoToMapFromPanel(_originalMapId);
                        }
                    }
                    break;

                case BuyState.ReturningToMap:
                    if (ServiceLocator.Get<IXmapService>() != null && !ServiceLocator.Get<IXmapService>().IsXmaping())
                    {
                        if (TileMap.mapID == _originalMapId)
                        {
                            _currentState = BuyState.Idle;
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
                case BuyState.MovingToNpc: return "Mua đồ: Đang đi tìm NPC";
                case BuyState.OpeningStore: return "Mua đồ: Xin mở cửa hàng";
                case BuyState.WaitingForMenu: return "Mua đồ: Chờ cửa hàng mở";
                case BuyState.BuyingItems: return "Mua đồ: Đang mua vật phẩm";
                case BuyState.ReturningToMap: return "Mua đồ: Quay lại bãi train";
                default: return "";
            }
        }

        private bool NeedToBuyAnything()
        {
            if (_autoBuyTdlt && NeedBuyTdlt()) return true;
            if (_autoBuyKhauTrang && ItemHelper.GetItemQuantityInBag(ID_KHAU_TRANG) < _buyKhauTrangQty && Char.myCharz().checkLuong() >= 2) return true;
            if (_autoBuyCoBonLa && ItemHelper.GetItemQuantityInBag(ID_CO_4_LA) < _buyCoBonLaQty && Char.myCharz().checkLuong() >= 10) return true;
            if (_autoBuyBuaDe && ItemHelper.GetItemQuantityInBag(ID_BUA_DE) < _buyBuaDeQty && Char.myCharz().checkLuong() >= 5) return true;
            if (_autoBuyPrivateTicket && ItemHelper.GetItemQuantityInBag(ID_PRIVATE_TICKET) == 0 && Char.myCharz().checkLuong() >= 1) return true;
            if (_autoBuyThoiVang && NeedBuyThoiVang()) return true;
            return false;
        }

        private bool NeedBuyCustomItem(out CustomBuyItem lackingItem)
        {
            lackingItem = default;
            if (!_autoBuyCustom || _customItems == null) return false;
            foreach (var custom in _customItems)
            {
                if (ItemHelper.GetItemQuantityInBag(custom.ItemId) == 0)
                {
                    lackingItem = custom;
                    return true;
                }
            }
            return false;
        }

        private bool ShouldStartBuyCycle()
        {
            if (!_autoBuyTdlt && !_autoBuyKhauTrang && !_autoBuyCoBonLa && !_autoBuyBuaDe && !_autoBuyThoiVang && !_autoBuyPrivateTicket && !_autoBuyCustom)
                return false;

            if (GameCanvas.gameTick % 20 != 0)
                return false;

            if (Char.myCharz() == null)
                return false;

            if (NeedBuyCustomItem(out _)) return true;
            return NeedToBuyAnything();
        }

        private bool NeedBuyThoiVang()
        {
            long currentGold = Char.myCharz()?.xu ?? 0;
            if (currentGold < _buyThoiVangMinGold) return false;
            return HasBagSpaceForThoiVang();
        }

        private bool CanContinueBuyThoiVang()
        {
            long currentGold = Char.myCharz()?.xu ?? 0;
            if (currentGold < MIN_GOLD_TO_STOP_BUY_THOI_VANG) return false;
            return HasBagSpaceForThoiVang();
        }

        private bool HasBagSpaceForThoiVang()
        {
            if (Char.myCharz()?.arrItemBag != null)
            {
                foreach (var item in Char.myCharz().arrItemBag)
                {
                    if (item == null) return true;
                    if (item.template != null && item.template.id == ID_THOI_VANG && item.quantity < 99) return true;
                }
                return false;
            }
            return true;
        }

        private bool NeedBuyTdlt()
        {
            // TDLT id = 521, check time
            int itemTimeRemaining = TimeItemDatBiet(TdltController.ItemId);
            int buffTimeRemaining = TdltController.GetRemainingTimeMinutes();

            return itemTimeRemaining <= 10 && buffTimeRemaining <= 10 && Char.myCharz().checkLuong() >= 1;
        }

        private short GetTdltIdToBuy()
        {
            int luong = Char.myCharz().checkLuong();
            if (luong >= 22) return ID_TDLT_30_NGAY;
            if (luong >= 9) return ID_TDLT_9_NGAY;
            return TdltController.ItemId;
        }



        private int TimeItemDatBiet(short idItem)
        {
            try
            {
                Item[] arrItemBag = Char.myCharz().arrItemBag;
                foreach (Item item in arrItemBag)
                {
                    if (item?.template.id == idItem && item.itemOption != null && item.itemOption.Length > 0)
                    {
                        return item.itemOption[0].param;
                    }
                }
            }
            catch (Exception)
            {
            }
            return 0;
        }
        
        private TField CreateTField(string text)
        {
            TField tField = new TField();
            tField.setText(text);
            return tField;
        }

        protected override void OnSettingsHotReload()
        {
            ResetRuntimeForHotReload();

            _autoBuyTdlt = _settings.AutoBuyTdlt;
            _autoBuyKhauTrang = _settings.AutoBuyKhauTrang;
            _buyKhauTrangQty = _settings.BuyKhauTrangQty;
            _autoBuyCoBonLa = _settings.AutoBuyCoBonLa;
            _buyCoBonLaQty = _settings.BuyCoBonLaQty;
            _autoBuyBuaDe = _settings.AutoBuyBuaDe;
            _buyBuaDeQty = _settings.BuyBuaDeQty;
            _autoBuyPrivateTicket = _settings.AutoBuyPrivateTicket;
            _autoBuyThoiVang = _settings.AutoBuyThoiVang;
            _buyThoiVangMinGold = _settings.BuyThoiVangMinGold > 0 ? _settings.BuyThoiVangMinGold : 1_000_000_000;
            _autoBuyCustom = _settings.AutoBuyCustom;

            _customItems.Clear();
            if (!string.IsNullOrEmpty(_settings.CustomData))
            {
                var lines = _settings.CustomData.Split(new[] { '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    try
                    {
                        var parts = line.Split('|');
                        if (parts.Length >= 6)
                        {
                            var item = new CustomBuyItem
                            {
                                ItemId = short.Parse(parts[0].Trim()),
                                Qty = int.Parse(parts[1].Trim()),
                                MapId = int.Parse(parts[2].Trim()),
                                NpcId = short.Parse(parts[3].Trim()),
                                CurrencyType = int.Parse(parts[4].Trim()),
                                BuyMode = int.Parse(parts[5].Trim()),
                            };

                            if (parts.Length >= 7 && !string.IsNullOrWhiteSpace(parts[6]))
                            {
                                var menuParts = parts[6].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                item.Menus = new int[menuParts.Length];
                                for (int i = 0; i < menuParts.Length; i++)
                                    item.Menus[i] = int.Parse(menuParts[i].Trim());
                            }
                            else
                            {
                                item.Menus = new int[0];
                            }

                            _customItems.Add(item);
                        }
                    }
                    catch { }
                }
            }
        }

        private BuySettings CloneCurrentSettings()
        {
            return new BuySettings
            {
                AutoBuyTdlt = _autoBuyTdlt,
                AutoBuyKhauTrang = _autoBuyKhauTrang,
                BuyKhauTrangQty = _buyKhauTrangQty,
                AutoBuyCoBonLa = _autoBuyCoBonLa,
                BuyCoBonLaQty = _buyCoBonLaQty,
                AutoBuyBuaDe = _autoBuyBuaDe,
                BuyBuaDeQty = _buyBuaDeQty,
                AutoBuyPrivateTicket = _autoBuyPrivateTicket,
                AutoBuyThoiVang = _autoBuyThoiVang,
                BuyThoiVangMinGold = _buyThoiVangMinGold,
                AutoBuyCustom = _autoBuyCustom,
                CustomData = SerializeCustomItems(),
            };
        }

        private string SerializeCustomItems()
        {
            if (_customItems == null || _customItems.Count == 0) return string.Empty;

            List<string> lines = new List<string>();
            foreach (var item in _customItems)
            {
                string menus = item.Menus != null && item.Menus.Length > 0
                    ? "|" + string.Join(",", item.Menus)
                    : string.Empty;
                lines.Add($"{item.ItemId}|{item.Qty}|{item.MapId}|{item.NpcId}|{item.CurrencyType}|{item.BuyMode}{menus}");
            }
            return string.Join(";", lines);
        }

        private void ResetRuntimeForHotReload()
        {
            if (_currentState != BuyState.Idle)
                ModBootstrap.TrainFeature.ResumeTrainAfterAction();

            _currentState = BuyState.Idle;
            _originalMapId = -1;
            _targetMapId = -1;
            _stateStartTime = 0L;
            _currentCustomItem = default;
            _customMenuStep = -1;
            _customBuySpamCount = 0;

            ServiceLocator.Get<IXmapService>()?.StopFromPanel();
            if (GameCanvas.panel != null) GameCanvas.panel.hide();
        }
    }
}
